using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mono.Cecil.Inject
{
    /// <summary>
    ///     Extensions for <see cref="TypeDefinition" />
    /// </summary>
    public static class TypeDefinitionExtensions
    {
        /// <summary>
        ///     Changes accessibility of a type member (target or a field).
        /// </summary>
        /// <param name="type">Type (class, struct) that contains the member to alter.</param>
        /// <param name="member">Name of the member as a regular expression.</param>
        /// <param name="makePublic">If true, will make the member public.</param>
        /// <param name="makeVirtual">If true and the member is a target, will make it public.</param>
        /// <param name="makeAssignable">
        ///     If true, turns read-only members into assignable ones. NOTE: Will not for for constant
        ///     (marked with "const" prefix) -- only for "readonly".
        /// </param>
        /// <param name="recursive">If true, will recusively call this method on nested types as well.</param>
        public static void ChangeAccess(this TypeDefinition type,
                                        string member,
                                        bool makePublic = true,
                                        bool makeVirtual = true,
                                        bool makeAssignable = true,
                                        bool recursive = false)
        {
            Logger.LogLine(LogMask.ChangeAccess, $"Changing access for {member} in {type.FullName}");
            Logger.LogLine(LogMask.ChangeAccess, "Flags:");
            Logger.LogLine(LogMask.ChangeAccess, $"Make public: {makePublic}");
            Logger.LogLine(LogMask.ChangeAccess, $"Make virtual: {makeVirtual}");
            Logger.LogLine(LogMask.ChangeAccess, $"Make assignable: {makeAssignable}");
            Logger.LogLine(LogMask.ChangeAccess, $"Recurse on nested types: {recursive}");

            Regex pattern = new Regex($"^{member}$");
            type.Methods.Where(m => pattern.IsMatch(m.Name)).ForEach(
            m =>
            {
                Logger.LogLine(LogMask.ChangeAccess, $"Changing access for method {m.Name}");
                Logger.LogLine(
                LogMask.ChangeAccess,
                $"Flags before: {(m.IsVirtual ? "VIRTUAL" : string.Empty)} {(m.IsPublic ? "PUBLIC" : string.Empty)} {(m.IsPrivate ? "PRIVATE" : string.Empty)}");

                if (m.Name != ".ctor")
                    m.IsVirtual = makeVirtual;
                m.IsPublic = makePublic;
                m.IsPrivate = !makePublic;

                Logger.LogLine(
                LogMask.ChangeAccess,
                $"Flags now: {(m.IsVirtual ? "VIRTUAL" : string.Empty)} {(m.IsPublic ? "PUBLIC" : string.Empty)} {(m.IsPrivate ? "PRIVATE" : string.Empty)}");
            });

            type.Fields.Where(f => pattern.IsMatch(f.Name)).ForEach(
            f =>
            {
                Logger.LogLine(LogMask.ChangeAccess, $"Changing access for field {f.Name}");
                Logger.LogLine(
                LogMask.ChangeAccess,
                $"Flags before: {(f.IsInitOnly ? "READONLY" : string.Empty)} {(f.IsPublic ? "PUBLIC" : string.Empty)} {(f.IsPrivate ? "PRIVATE" : string.Empty)}");

                f.IsPublic = makePublic;
                f.IsPrivate = !makePublic;
                f.IsInitOnly = !makeAssignable;

                Logger.LogLine(
                LogMask.ChangeAccess,
                $"Flags now: {(f.IsInitOnly ? "READONLY" : string.Empty)} {(f.IsPublic ? "PUBLIC" : string.Empty)} {(f.IsPrivate ? "PRIVATE" : string.Empty)}");
            });

            type.NestedTypes.Where(f => pattern.IsMatch(f.Name)).ForEach(
            f =>
            {
                Logger.LogLine(LogMask.ChangeAccess, $"Changing access for nested type {f.Name}");
                Logger.LogLine(
                LogMask.ChangeAccess,
                $"Flags before: {(f.IsNestedPublic ? "NEST_PUBLIC" : string.Empty)} {(f.IsNestedPrivate ? "NEST_PRIVATE" : string.Empty)} {(f.IsPublic ? "PUBLIC" : string.Empty)}");

                f.IsPublic = makePublic;
                f.IsNestedPublic = makePublic;
                f.IsNestedPrivate = !makePublic;
                if (recursive)
                    f.ChangeAccess(member, makePublic, makeVirtual, makeAssignable, true);
            });
        }

        /// <summary>
        ///     Gets a field by its name.
        /// </summary>
        /// <param name="self">Reference to type definition that owns the method/member.</param>
        /// <param name="memberName">Name of the field.</param>
        /// <returns>Field definiton of the said field. Null, if none found.</returns>
        public static FieldDefinition GetField(this TypeDefinition self, string memberName)
        {
            return self.Fields.FirstOrDefault(f => f.Name == memberName);
        }

        /// <summary>
        ///     Searches for a method that can be used to inject into the specified target.
        /// </summary>
        /// <param name="type">This type in which the possible injection method lies.</param>
        /// <param name="name">Name of the injection method.</param>
        /// <param name="target">The target method which to inject.</param>
        /// <param name="flags">
        ///     Injection flags that specify what values to pass to the injection method and how to inject it. This
        ///     method attempts to find the hook method that satisfies all the specified flags.
        /// </param>
        /// <param name="localVarIDs">
        ///     An array of indicies of local variables to pass to the injection method. Used only if
        ///     <see cref="InjectFlags.PassLocals" /> is specified, otherwise ignored.
        /// </param>
        /// <param name="memberReferences">
        ///     An array of class fields from the type the target lies in to pass to the injection
        ///     method. Used only if <see cref="InjectFlags.PassFields" /> is specified, otherwise ignored.
        /// </param>
        /// <returns>
        ///     An instance of <see cref="InjectionDefinition" />, if a suitable injection method with the given name has been
        ///     found. Otherwise, null.
        /// </returns>
        public static InjectionDefinition GetInjectionMethod(this TypeDefinition type,
                                                             string name,
                                                             MethodDefinition target,
                                                             InjectFlags flags,
                                                             int[] localVarIDs = null,
                                                             params FieldDefinition[] memberReferences)
        {
            Logger.LogLine(LogMask.GetInjectionMethod, "##### GET INJECTION METHOD BEGIN #####");
            Logger.LogLine(
            LogMask.GetInjectionMethod,
            $"Attempting to get a suitable injection method for {type.Name}.{target?.Name}");

            if (string.IsNullOrEmpty(name))
            {
                Logger.LogLine(LogMask.GetInjectionMethod, "No injection method name specified");
                return null;
            }
            if (target == null || !target.HasBody)
            {
                Logger.LogLine(LogMask.GetInjectionMethod, "No target specified or the target has no definition");
                return null;
            }

            InjectValues hFlags = flags.ToValues();

#if DEBUG
            Logger.LogLine(LogMask.GetInjectionMethod, "Patch parameters:");
            Logger.LogLine(LogMask.GetInjectionMethod, $"Pass tag: {hFlags.PassTag}");
            Logger.LogLine(LogMask.GetInjectionMethod, $"Modify return value: {hFlags.ModifyReturn}");
            Logger.LogLine(LogMask.GetInjectionMethod, $"Pass THIS: {hFlags.PassInvokingInstance}");
            Logger.LogLine(LogMask.GetInjectionMethod, $"Pass method locals: {hFlags.PassLocals}");
            Logger.LogLine(LogMask.GetInjectionMethod, $"Pass member fields: {hFlags.PassFields}");
            Logger.LogLine(LogMask.GetInjectionMethod, $"Pass member parameters: {hFlags.PassParameters}");
            if (hFlags.PassParameters)
            {
                Logger.LogLine(
                LogMask.GetInjectionMethod,
                $"Member parameters are passed by {(hFlags.PassParametersByRef ? "reference" : "value")}");
            }
#endif

            if (hFlags.PassInvokingInstance && target.IsStatic)
            {
                Logger.LogLine(
                LogMask.GetInjectionMethod,
                $"{nameof(hFlags.PassInvokingInstance)} is true, but target is static!");
                return null;
            }
            if (hFlags.PassFields && (target.IsStatic || memberReferences == null || memberReferences.Length == 0))
            {
                Logger.LogLine(
                LogMask.GetInjectionMethod,
                $"{nameof(hFlags.PassFields)} is true, but target is either static or no member references were specified");
                return null;
            }
            if (hFlags.PassLocals && (!target.Body.HasVariables || localVarIDs == null || localVarIDs.Length == 0))
            {
                Logger.LogLine(
                LogMask.GetInjectionMethod,
                $"{nameof(hFlags.PassLocals)} is true, but target either doesn't have any locals or no local IDs were specified");
                return null;
            }

            bool isVoid = target.ReturnType.FullName == "System.Void";

            int offsetSelf = Convert.ToInt32(hFlags.PassTag);
            int offsetReturn = Convert.ToInt32(hFlags.PassTag) + Convert.ToInt32(hFlags.PassInvokingInstance);
            int customParamCount = Convert.ToInt32(hFlags.ModifyReturn && !isVoid) + offsetReturn;
            int memberRefCount = hFlags.PassFields ? memberReferences.Length : 0;
            int localRefCount = hFlags.PassLocals ? localVarIDs.Length : 0;
            int paramCount = (hFlags.PassParameters ? target.Parameters.Count : 0) + customParamCount + memberRefCount
                             + localRefCount;

            Logger.LogLine(LogMask.GetInjectionMethod, "Needed parameter count:");
            Logger.LogLine(LogMask.GetInjectionMethod, $"Custom parameters (tag + return): {customParamCount}");
            Logger.LogLine(LogMask.GetInjectionMethod, $"Member references: {memberRefCount}");
            Logger.LogLine(LogMask.GetInjectionMethod, $"Local references: {localRefCount}");
            Logger.LogLine(LogMask.GetInjectionMethod, $"Member parameters: {paramCount}");

            IEnumerable<TypeReference> v;
            TypeComparer comparer = new TypeComparer();

            MethodDefinition injection =
            type.Methods.FirstOrDefault(
            m =>
            m.Name == name && m.Parameters.Count == paramCount && m.IsStatic && m.IsPublic && m.HasBody
            && (!hFlags.PassTag || m.Parameters[0].ParameterType.FullName == "System.Int32")
            && (!hFlags.PassInvokingInstance
                || comparer.Equals(m.Parameters[offsetSelf].ParameterType, target.DeclaringType))
            && (!hFlags.ModifyReturn
                || (m.ReturnType.FullName == "System.Boolean"
                    && (isVoid
                        || comparer.Equals(
                        m.Parameters[offsetReturn].ParameterType,
                        new ByReferenceType(target.ReturnType)))))
            && (!hFlags.PassLocals
                || localVarIDs.All(i => 0 <= i && i < target.Body.Variables.Count)
                && m.Parameters.Slice(customParamCount, localRefCount)
                    .Select((p, i) => new {param = p, index = i})
                    .All(
                    t =>
                    t.param.ParameterType.IsByReference
                    && comparer.Equals(
                    t.param.ParameterType,
                    new ByReferenceType(target.Body.Variables[t.index].VariableType))))
            && (!hFlags.PassFields
                || (v =
                    m.Parameters.Slice(customParamCount + localRefCount, memberRefCount).Select(p => p.ParameterType))
                   .All(p => p.IsByReference)
                && v.SequenceEqual(
                memberReferences.Select(p => (TypeReference) new ByReferenceType(p.FieldType)),
                comparer))
            && (!hFlags.PassParameters
                || (m.HasGenericParameters == target.HasGenericParameters
                    && (!m.HasGenericParameters || m.GenericParameters.Count <= target.GenericParameters.Count)
                    && (!hFlags.PassParametersByRef
                        || m.Parameters.Skip(customParamCount + localRefCount + memberRefCount)
                            .All(p => p.ParameterType.IsByReference))
                    && m.Parameters.Skip(customParamCount + localRefCount + memberRefCount)
                        .Select(p => p.ParameterType)
                        .SequenceEqual(
                        target.Parameters.Select(
                        p => (hFlags.PassParametersByRef ? new ByReferenceType(p.ParameterType) : p.ParameterType)),
                        comparer))));

            if (injection == null)
            {
                Logger.LogLine(LogMask.GetInjectionMethod, "Did not find any matching methods!");
                return null;
            }
            Logger.LogLine(LogMask.GetInjectionMethod, "Found injection method.");
            Logger.LogLine(LogMask.GetInjectionMethod, "##### GET INJECTION METHOD END #####");
            return new InjectionDefinition
            {
                InjectMethod = injection,
                InjectTarget = target,
                Flags = flags,
                MemberReferences = memberReferences,
                LocalVarIDs = localVarIDs,
                _PrefixCount = customParamCount,
                _MemeberRefCount = hFlags.PassFields ? memberReferences.Length : 0,
                _ParameterCount = hFlags.PassParameters ? target.Parameters.Count : 0
            };
        }

        /// <summary>
        ///     Gets the method by its name. If more overloads exist, only the first one defined is chosen.
        /// </summary>
        /// <param name="self">Reference to type definition that owns the method/member.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <returns>Method definition for the given method name. If no methods with such names are found, returns null.</returns>
        public static MethodDefinition GetMethod(this TypeDefinition self, string methodName)
        {
            return self.Methods.FirstOrDefault(m => m.Name == methodName);
        }

        /// <summary>
        ///     Gets the method by its name. If more overloads exist, only the one that has the same specified parameters is
        ///     chosen.
        ///     To easily obtain parameter types, refer to <see cref="ParamHelper" /> class.
        /// </summary>
        /// <param name="self">Reference to type definition that owns the method/member.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="paramTypes">Parameter types in the order they are declared in the method.</param>
        /// <returns>
        ///     Method definition for the given method name and overload. If no methods with such names and parameters are
        ///     found, returns null.
        /// </returns>
        public static MethodDefinition GetMethod(this TypeDefinition self,
                                                 string methodName,
                                                 params TypeReference[] paramTypes)
        {
            return GetMethod(self, methodName, paramTypes.Select(p => p.FullName).ToArray());
            /*
            self.Methods.FirstOrDefault(
            m =>
            m.Name == methodName
            && paramTypes.SequenceEqual(m.Parameters.Select(p => p.ParameterType), new TypeComparer()));
            */
        }

        /// <summary>
        ///     Gets the method by its name. If more overloads exist, only the one that has the same specified parameters is
        ///     chosen.
        /// </summary>
        /// <param name="self">Reference to type definition that owns the method/member.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="types">Parameter types in the order they are declared in the method.</param>
        /// <returns>
        ///     Method definition for the given method name and overload. If no methods with such names and parameters are
        ///     found, returns null.
        /// </returns>
        public static MethodDefinition GetMethod(this TypeDefinition self, string methodName, params Type[] types)
        {
            return GetMethod(self, methodName, types.Select(t => ParamHelper.FromType(t).FullName).ToArray());
        }

        /// <summary>
        ///     Gets the method by its name. If more overloads exist, only the one that has the same specified parameters is
        ///     chosen.
        /// </summary>
        /// <param name="self">Reference to type definition that owns the method/member.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="types">
        ///     Full name of the parameter types in the order they are declared in the method. The full name is
        ///     specified by <see cref="Type" /> specification.
        /// </param>
        /// <returns>
        ///     Method definition for the given method name and overload. If no methods with such names and parameters are
        ///     found, returns null.
        /// </returns>
        public static MethodDefinition GetMethod(this TypeDefinition self, string methodName, params string[] types)
        {
            return
            self.Methods.FirstOrDefault(
            m =>
            m.Name == methodName
            && types.SequenceEqual(m.Parameters.Select(p => p.ParameterType.FullName), StringComparer.InvariantCulture));
        }

        /// <summary>
        ///     Gets the all the method overloads with the given name.
        /// </summary>
        /// <param name="self">Reference to type definition that owns the method/member.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <returns>
        ///     An array of all the method overloads with the specified name.
        /// </returns>
        public static MethodDefinition[] GetMethods(this TypeDefinition self, string methodName)
        {
            return self.Methods.Where(m => m.Name == methodName).ToArray();
        }

        /// <summary>
        ///     Finds the methods with the given name that have at least the provided parameters. The number of parameters need not
        ///     match, which is why
        ///     the methods returned may have more parameters than passed to this method.
        /// </summary>
        /// <param name="self">Reference to type definition that owns the method/member.</param>
        /// <param name="methodName">Name of the method to match.</param>
        /// <param name="types">Parameter types in the order they should be declared in the method.</param>
        /// <returns>An array of methods that have the specified name and *at least* the given parameters.</returns>
        public static MethodDefinition[] MatchMethod(this TypeDefinition self, string methodName, params Type[] types)
        {
            return MatchMethod(self, methodName, types.Select(t => ParamHelper.FromType(t).FullName).ToArray());
        }

        /// <summary>
        ///     Finds the methods with the given name that have at least the provided parameters. The number of parameters need not
        ///     match, which is why
        ///     the methods returned may have more parameters than passed to this method.
        /// </summary>
        /// <param name="self">Reference to type definition that owns the method/member.</param>
        /// <param name="methodName">Name of the method to match.</param>
        /// <param name="paramTypes">Parameter types in the order they should be declared in the method.</param>
        /// <returns>An array of methods that have the specified name and *at least* the given parameters.</returns>
        public static MethodDefinition[] MatchMethod(this TypeDefinition self,
                                                     string methodName,
                                                     params TypeReference[] paramTypes)
        {
            return MatchMethod(self, methodName, paramTypes.Select(p => p.FullName).ToArray());
        }

        /// <summary>
        ///     Finds the methods with the given name that have at least the provided parameters. The number of parameters need not
        ///     match, which is why
        ///     the methods returned may have more parameters than passed to this method.
        /// </summary>
        /// <param name="self">Reference to type definition that owns the method/member.</param>
        /// <param name="methodName">Name of the method to match.</param>
        /// <param name="paramTypes">Parameter types in the order they should be declared in the method.</param>
        /// <returns>An array of methods that have the specified name and *at least* the given parameters.</returns>
        public static MethodDefinition[] MatchMethod(this TypeDefinition self,
                                                     string methodName,
                                                     params string[] paramTypes)
        {
            return
            self.Methods.Where(
            m =>
            m.Name == methodName && paramTypes.Length <= m.Parameters.Count
            && paramTypes.SequenceEqual(
            m.Parameters.Take(paramTypes.Length).Select(p => p.ParameterType.FullName),
            StringComparer.InvariantCulture)).ToArray();
        }
    }

    internal class TypeComparer : IEqualityComparer<TypeReference>
    {
        public bool Equals(TypeReference x, TypeReference y)
        {
            Logger.LogLine(LogMask.TypeCompare, "##### TYPE COMPARE INFO #####");
            Logger.LogLine(LogMask.TypeCompare, $"Comparing types {x.FullName} and {y.FullName}");
            Logger.LogLine(LogMask.TypeCompare, $"             Name| x: {x.Name}, y: {y.Name}");
            Logger.LogLine(LogMask.TypeCompare, $"        Namespace| x: {x.Namespace}, y: {y.Namespace}");
            Logger.LogLine(LogMask.TypeCompare, $"         Is array| x: {x.IsArray}, y: {y.IsArray}");
            Logger.LogLine(
            LogMask.TypeCompare,
            $"       Is generic| x: {x.IsGenericInstance}, y: {y.IsGenericInstance}");
            Logger.LogLine(LogMask.TypeCompare, $"     Is reference| x: {x.IsByReference}, y: {y.IsByReference}");


            if (
            !(x.Name == y.Name && x.Namespace == y.Namespace && x.IsArray == y.IsArray
              && x.IsGenericInstance == y.IsGenericInstance && x.IsByReference == y.IsByReference))
                return false;
            if (!x.IsGenericInstance)
                return true;
            GenericInstanceType gx = (GenericInstanceType) x;
            GenericInstanceType gy = (GenericInstanceType) y;

            Logger.LogLine(
            LogMask.TypeCompare,
            $"Generic arg count| x: {gx.GenericArguments.Count}, y: {gy.GenericArguments.Count}");
            Logger.LogLine(LogMask.TypeCompare, "Comparing generics");

            return gx.GenericArguments.Count == gy.GenericArguments.Count
                   && !gx.GenericArguments.Where((t, i) => !Equals(t, gy.GenericArguments[i])).Any();
        }

        public int GetHashCode(TypeReference obj)
        {
            return obj.GetHashCode();
        }
    }
}