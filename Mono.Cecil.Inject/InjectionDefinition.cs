using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil.Cil;

namespace Mono.Cecil.Inject
{
    /// <summary>
    ///     The core class containing all the injector methods.
    /// </summary>
    public class InjectionDefinition
    {
        internal InjectionDefinition()
        {
        }

        /// <summary>
        ///     Attempts to construct an instance of <see cref="InjectionDefinition" /> by linking the injection method with the
        ///     injection target (the method to be injected).
        ///     The way how the method is injected is specified by the injection flags. If the injection method does not match the
        ///     criteria set by the injection flags, an exception will be thrown.
        /// </summary>
        /// <param name="injectTarget">The method that will be injected.</param>
        /// <param name="injectMethod">The method which to inject.</param>
        /// <param name="flags">Injection flags that specify what values to pass to the injection method and how to inject it.</param>
        /// <param name="localVarIDs">
        ///     An array of indicies of local variables to pass to the injection method. Used only if
        ///     <see cref="InjectFlags.PassLocals" /> is specified, otherwise ignored.
        /// </param>
        /// <param name="memberReferences">
        ///     An array of class fields from the type the target lies in to pass to the injection
        ///     method. Used only if <see cref="InjectFlags.PassFields" /> is specified, otherwise ignored.
        /// </param>
        public InjectionDefinition(MethodDefinition injectTarget,
                                   MethodDefinition injectMethod,
                                   InjectFlags flags,
                                   int[] localVarIDs = null,
                                   params FieldDefinition[] memberReferences)
        {
            ParameterCount = VerifyInjectionDefinition(injectMethod, injectTarget, flags, localVarIDs,
                                                       memberReferences);
            InjectMethod = injectMethod;
            InjectTarget = injectTarget;
            Flags = flags;
            MemberReferences = memberReferences;
            LocalVarIDs = localVarIDs;
        }

        /// <summary>
        ///     Flags that specify how the injection will be performed and which parameters are to be passed to the injection
        ///     method.
        /// </summary>
        public InjectFlags Flags { get; internal set; }

        /// <summary>
        ///     The injection method; the method the call of which will be injected into the target.
        /// </summary>
        public MethodDefinition InjectMethod { get; internal set; }

        /// <summary>
        ///     The injection target-
        /// </summary>
        public MethodDefinition InjectTarget { get; internal set; }

        /// <summary>
        ///     An array of indicies of local variables to pass to the injection method. Used only if
        ///     <see cref="InjectFlags.PassLocals" /> is specified, otherwise null.
        /// </summary>
        public int[] LocalVarIDs { get; internal set; }

        /// <summary>
        ///     An array of class fields from the type the target lies in to pass to the injection method. Used only if
        ///     <see cref="InjectFlags.PassFields" /> is specified, otherwise null.
        /// </summary>
        public FieldDefinition[] MemberReferences { get; internal set; }

        private int ParameterCount { get; set; }

        internal static InjectionDefinition FindInjectionDefinition(TypeDefinition type,
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
                Logger.LogLine(
                    LogMask.GetInjectionMethod,
                    $"Member parameters are passed by {(hFlags.PassParametersByRef ? "reference" : "value")}");
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

            int parameterCount = 0;

            MethodDefinition injection =
                type.Methods.FirstOrDefault(m =>
                {
                    try
                    {
                        parameterCount = VerifyInjectionDefinition(m, target, flags, localVarIDs, memberReferences);
                    }
                    catch (InjectionDefinitionException e)
                    {
                        Logger.LogLine(LogMask.GetInjectionMethod, e.Message);
                        return false;
                    }
                    return true;
                });

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
                ParameterCount = parameterCount
            };
        }

        private static void Assert(bool val, string message)
        {
            if (!val)
                throw new InjectionDefinitionException(message);
        }

        private static int VerifyInjectionDefinition(MethodDefinition injectMethod, MethodDefinition injectTarget,
                                                     InjectFlags flags, int[] localVarIDs = null,
                                                     params FieldDefinition[] memberReferences)
        {
            Assert(
                injectMethod.IsStatic,
                $"{nameof(injectMethod)} must be static in order to be used as the injection.");
            Assert(injectMethod.IsPublic, $"{nameof(injectMethod)} must be public.");
            Assert(
                injectMethod.HasBody,
                $"{nameof(injectMethod)} must have a definition in order to be used as the injecton.");

            InjectValues hFlags = flags.ToValues();

            bool isVoid = injectTarget.ReturnType.FullName == "System.Void";

            Assert(
                !hFlags.PassLocals || localVarIDs != null,
                $"Supposed to pass local references, but {nameof(localVarIDs)} is empty");
            Assert(
                !hFlags.PassFields || memberReferences != null,
                $"Supposed to pass member fields, but {nameof(memberReferences)} is empty!");

            int prefixCount = Convert.ToInt32(hFlags.PassTag) + Convert.ToInt32(hFlags.PassInvokingInstance)
                              + Convert.ToInt32(hFlags.ModifyReturn && !isVoid);
            int localsCount = hFlags.PassLocals ? localVarIDs.Length : 0;
            int memberRefCount = hFlags.PassFields ? memberReferences.Length : 0;
            int paramCount = hFlags.PassParameters ? injectTarget.Parameters.Count : 0;
            int parameters = injectMethod.Parameters.Count - prefixCount - localsCount - memberRefCount;

            Assert(
                hFlags.PassParameters && 0 < parameters && parameters <= injectTarget.Parameters.Count
                || !hFlags.PassParameters && parameters == 0,
                $@"The injection method has a wrong number of parameters! Check that the provided target method, local variables, member references and injection flags add up to the right number of parameters.
Needed parameters: Prefix: {prefixCount}, Locals: {localsCount}, Members: {memberRefCount}, Parameters: {
                        (hFlags.PassParameters ? "between 1 and " + paramCount : "0")
                    }, TOTAL: {
                        (hFlags.PassParameters ? $"between {prefixCount + localsCount + memberRefCount + 1} and " : "")
                    }{prefixCount + localsCount + memberRefCount + paramCount}.
Injection has {injectMethod.Parameters.Count} parameters.");

            TypeComparer comparer = TypeComparer.Instance;

            if (hFlags.PassTag)
            {
                string typeName = Enum.GetName(typeof(InjectValues.PassTagType), hFlags.TagType);
                Assert(
                    injectMethod.Parameters[0].ParameterType.FullName == $"System.{typeName}",
                    $"Supposed to pass a tag, but the provided tag must be of type System.{typeName}.");
            }

            if (hFlags.PassInvokingInstance)
            {
                Assert(
                    !injectTarget.IsStatic,
                    "Supposed to pass invoking instance, but the target method is static and thus is not bound to any instances.");
                Assert(
                    comparer.Equals(
                        injectMethod.Parameters[Convert.ToInt32(hFlags.PassTag)].ParameterType,
                        injectTarget.DeclaringType),
                    "Supposed to pass invoking instance, but the type of the instance does not match with the type declared in the injection method.");
            }

            if (hFlags.ModifyReturn)
            {
                Assert(
                    injectMethod.ReturnType.FullName == "System.Boolean",
                    "The injection method must return a boolean in order to alter the return value.");
                Assert(
                    isVoid
                    || comparer.Equals(
                        injectMethod
                            .Parameters[Convert.ToInt32(hFlags.PassTag) + Convert.ToInt32(hFlags.PassInvokingInstance)]
                            .ParameterType,
                        new ByReferenceType(injectTarget.ReturnType)),
                    "Supposed to modify the return value, but the provided return type does not match with the return type of the target method! Also make sure the type is passed by reference (out/ref).");
            }

            if (hFlags.PassLocals)
            {
                Assert(injectTarget.Body.HasVariables, "The target method does not have any locals.");
                Assert(
                    localVarIDs.Length != 0,
                    "Supposed to pass method locals, but the IDs of the locals were not specified.");
                Assert(
                    localVarIDs.All(i => 0 <= i && i < injectTarget.Body.Variables.Count),
                    "Supposed to receive local references, but the provided local variable index/indices do not exist in the target method!");
                Assert(
                    injectMethod.Parameters.Slice(prefixCount, localsCount)
                                .Select((p, i) => new {param = p, index = localVarIDs[i]})
                                .All(
                                    t =>
                                        t.param.ParameterType.IsByReference
                                        && comparer.Equals(
                                            t.param.ParameterType,
                                            new ByReferenceType(injectTarget.Body.Variables[t.index].VariableType))),
                    "Supposed to receive local references, but the types between injection method and target method mismatch. Also make sure they are passed by reference (ref/out).");
            }

            if (hFlags.PassFields)
            {
                Assert(!injectTarget.IsStatic, "Cannot pass member references if the injection method is static!");
                Assert(
                    memberReferences.Length != 0,
                    "Supposed to pass member references, but no members were specified.");
                Assert(
                    memberReferences.All(
                        m =>
                            m.DeclaringType.FullName == injectTarget.DeclaringType.FullName
                            && m.DeclaringType.BaseType.FullName == injectTarget.DeclaringType.BaseType.FullName),
                    $"The provided member fields do not belong to {injectTarget.DeclaringType}");

                IEnumerable<TypeReference> paramRefs =
                    injectMethod.Parameters.Slice(prefixCount + localsCount, memberRefCount)
                                .Select(p => p.ParameterType);
                IEnumerable<TypeReference> typeReferences = paramRefs as TypeReference[] ?? paramRefs.ToArray();

                Assert(
                    typeReferences.All(p => p.IsByReference),
                    "Supposed to pass class members, but the provided parameters in the injection method are not of a reference type (ref)!");
                Assert(
                    typeReferences.SequenceEqual(
                        memberReferences.Select(f => (TypeReference) new ByReferenceType(f.FieldType)),
                        comparer),
                    "Supposed to pass class members, but the existing members are of a different type than the ones specified in the injection method.");
            }

            if (hFlags.PassParameters)
            {
                Assert(
                    injectMethod.HasGenericParameters ==
                    injectTarget.Parameters.Any(p => p.ParameterType.IsGenericParameter),
                    "The injection and target methods have mismatching specification of generic parameters!");

                Assert(
                    !injectMethod.HasGenericParameters
                    || injectMethod.GenericParameters.Count <= injectTarget.GenericParameters.Count +
                    injectTarget.DeclaringType.GenericParameters.Count,
                    "The injection and target methods have a mismatching number of generic parameters! The injection method must have less or the same number of generic parameters as the target!");

                Assert(
                    !hFlags.PassParametersByRef
                    || injectMethod.Parameters.Skip(prefixCount + localsCount + memberRefCount)
                                   .All(p => p.ParameterType.IsByReference),
                    "Supposed to pass target method parameters by reference, but the provided parameters in the injection method are not of a reference type (ref).");

                Assert(
                    injectMethod.Parameters.Skip(prefixCount + localsCount + memberRefCount)
                                .Select(p => p.ParameterType)
                                .SequenceEqual(
                                    injectTarget.Parameters.Take(parameters)
                                                .Select(
                                                    p =>
                                                        hFlags.PassParametersByRef
                                                            ? new ByReferenceType(p.ParameterType)
                                                            : p.ParameterType),
                                    comparer),
                    "Supposed to pass target method parameters by reference, but the types specified in injection and target methods do not match.");
            }

            return parameters;
        }

        /// <summary>
        ///     Inject the call of the injection method into the target.
        /// </summary>
        /// <param name="startCode">
        ///     The index of the instruction from which to start injecting. If positive, will count from the
        ///     beginning of the method. If negative, will count from the end. For instance, -1 is the method's last instruction
        ///     and 0 is the first.
        /// </param>
        /// <param name="token">
        ///     If <see cref="InjectFlags.PassTag" /> is specified, the value of this parameter will be passed as a
        ///     parameter to the injection method.
        /// </param>
        /// <param name="direction">The direction in which to insert the call: either above the start code or below it.</param>
        public void Inject(int startCode = 0, object token = null, InjectDirection direction = InjectDirection.Before)
        {
            startCode = startCode < 0 ? InjectTarget.Body.Instructions.Count + startCode : startCode;
            Inject(InjectTarget.Body.Instructions[startCode], token, direction);
        }

        /// <summary>
        ///     Inject the call of the injection method into the target.
        /// </summary>
        /// <param name="startCode">The instruction from which to start injecting.</param>
        /// <param name="token">
        ///     If <see cref="InjectFlags.PassTag" /> is specified, the value of this parameter will be passed as a
        ///     parameter to the injection method.
        /// </param>
        /// <param name="direction">The direction in which to insert the call: either above the start code or below it.</param>
        public void Inject(Instruction startCode, object token = null,
                           InjectDirection direction = InjectDirection.Before)
        {
            InjectValues flags = Flags.ToValues();

#if DEBUG
            Logger.LogLine(LogMask.Inject, "##### INJECTION START #####");
            Logger.LogLine(
                LogMask.Inject,
                $"Injecting a call to {InjectMethod.Module.Name}.{InjectMethod.Name} into {InjectTarget.Module.Name}.{InjectTarget.Name}.");
            Logger.LogLine(LogMask.Inject, "Patch parameters:");
            Logger.LogLine(LogMask.Inject, $"Pass tag: {flags.PassTag}");
            Logger.LogLine(LogMask.Inject, $"Modify return value: {flags.ModifyReturn}");
            Logger.LogLine(LogMask.Inject, $"Pass THIS: {flags.PassInvokingInstance}");
            Logger.LogLine(LogMask.Inject, $"Pass method locals: {flags.PassLocals}");
            Logger.LogLine(LogMask.Inject, $"Pass member fields: {flags.PassFields}");
            Logger.LogLine(LogMask.Inject, $"Pass member parameters: {flags.PassParameters}");
            if (flags.PassParameters)
                Logger.LogLine(
                    LogMask.Inject,
                    $"Member parameters are passed by {(flags.PassParametersByRef ? "reference" : "value")}");
#endif

            bool isVoid = InjectTarget.ReturnType.FullName == "System.Void";

            MethodReference hookRef = InjectTarget.Module.Import(InjectMethod);

            // If the hook is generic but not instantiated fully, attempt to fill in the generic arguments with the ones specified in the target method/class
            if (hookRef.HasGenericParameters && (!hookRef.IsGenericInstance ||
                                                 hookRef.IsGenericInstance &&
                                                 ((GenericInstanceMethod) hookRef).GenericArguments.Count <
                                                 hookRef.GenericParameters.Count))
            {
                GenericInstanceMethod genericInjectMethod = new GenericInstanceMethod(hookRef);
                foreach (GenericParameter genericParameter in InjectMethod.GenericParameters)
                {
                    List<GenericParameter> @params = new List<GenericParameter>();
                    @params.AddRange(InjectTarget.GenericParameters);
                    @params.AddRange(InjectTarget.DeclaringType.GenericParameters);
                    GenericParameter param = @params.FirstOrDefault(p => p.Name == genericParameter.Name);
                    if (param == null)
                        throw new Exception(
                            "Could not find a suitable type to bind to the generic injection method. Try to manually instantiate the generic injection method before injecting.");
                    genericInjectMethod.GenericArguments.Add(param);
                }
                hookRef = genericInjectMethod;
            }

            MethodBody targetBody = InjectTarget.Body;
            ILProcessor il = targetBody.GetILProcessor();
            int startIndex = targetBody.Instructions.IndexOf(startCode);
            if (startIndex == -1)
                throw new ArgumentOutOfRangeException(nameof(startCode));
            Instruction startInstruction = startCode;

            if (direction == InjectDirection.Before && startIndex != 0)
            {
                Instruction oldIns = ILUtils.CopyInstruction(startCode);
                ILUtils.ReplaceInstruction(startCode, il.Create(OpCodes.Nop));
                Instruction ins = targetBody.Instructions[startIndex];
                il.InsertAfter(ins, oldIns);
                startInstruction = targetBody.Instructions[startIndex + 1];
            }
            else if (direction == InjectDirection.After)
            {
                il.InsertAfter(startCode, il.Create(OpCodes.Nop));
                startInstruction = targetBody.Instructions[startIndex + 1];
            }

            VariableDefinition returnDef = null;
            if (flags.ModifyReturn && !isVoid)
            {
                targetBody.InitLocals = true;
                returnDef = new VariableDefinition(
                    InjectMethod.Name + "_return",
                    InjectTarget.ReturnType);
                targetBody.Variables.Add(returnDef);
            }


            if (flags.PassTag)
            {
                Logger.LogLine(LogMask.Inject, $"Passing custom token value: {token}");

                switch (flags.TagType)
                {
                    case InjectValues.PassTagType.Int32:
                    {
                        int tag = token as int? ?? 0;
                        il.InsertBefore(startInstruction, il.Create(OpCodes.Ldc_I4, tag));
                    }
                        break;
                    case InjectValues.PassTagType.String:
                    {
                        string tag = token as string;
                        il.InsertBefore(startInstruction, il.Create(OpCodes.Ldstr, tag));
                    }
                        break;
                    case InjectValues.PassTagType.None:
                        break;
                    case InjectValues.PassTagType.Max:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            if (flags.PassInvokingInstance)
            {
                Logger.LogLine(LogMask.Inject, "Passing THIS argument");
                il.InsertBefore(startInstruction, il.Create(OpCodes.Ldarg_0));
            }
            if (flags.ModifyReturn && !isVoid)
            {
                Logger.LogLine(LogMask.Inject, "Passing return reference");
                il.InsertBefore(startInstruction, il.Create(OpCodes.Ldloca_S, returnDef));
            }
            if (flags.PassLocals)
            {
                Logger.LogLine(LogMask.Inject, "Passing local variable references");
                foreach (int i in LocalVarIDs)
                {
                    Logger.LogLine(LogMask.Inject, $"Passing local variable index: {i}");
                    il.InsertBefore(startInstruction, il.Create(OpCodes.Ldloca_S, (byte) i));
                }
            }
            if (flags.PassFields)
            {
                Logger.LogLine(LogMask.Inject, "Passing member field references");
                IEnumerable<FieldReference> memberRefs = MemberReferences.Select(t => t.Module.Import(t));
                foreach (FieldReference t in memberRefs)
                {
                    Logger.LogLine(LogMask.Inject, $"Passing member field {t.FullName}");
                    il.InsertBefore(startInstruction, il.Create(OpCodes.Ldarg_0));
                    il.InsertBefore(startInstruction, il.Create(OpCodes.Ldflda, t));
                }
            }
            if (flags.PassParameters)
            {
                Logger.LogLine(
                    LogMask.Inject,
                    $"Passing member parameters by {(flags.PassParametersByRef ? "reference" : "value")}");
                int icr = Convert.ToInt32(!InjectTarget.IsStatic);
                for (int i = 0; i < ParameterCount; i++)
                {
                    Logger.LogLine(LogMask.Inject, $"Passing parameter of index {i + icr}");
                    il.InsertBefore(
                        startInstruction,
                        flags.PassParametersByRef
                            ? il.Create(OpCodes.Ldarga_S, (byte) (i + icr))
                            : il.Create(OpCodes.Ldarg_S, (byte) (i + icr)));
                }
            }
            Logger.LogLine(LogMask.Inject, "Injecting the call to the method");
            il.InsertBefore(startInstruction, il.Create(OpCodes.Call, hookRef));
            if (flags.ModifyReturn)
            {
                Logger.LogLine(LogMask.Inject, "Inserting return check");
                il.InsertBefore(startInstruction, il.Create(OpCodes.Brfalse_S, startInstruction));
                if (!isVoid)
                {
                    Logger.LogLine(LogMask.Inject, "Inserting return value");
                    il.InsertBefore(startInstruction, il.Create(OpCodes.Ldloc_S, returnDef));
                }
                Logger.LogLine(LogMask.Inject, "Inserting return command");
                il.InsertBefore(startInstruction, il.Create(OpCodes.Ret));
            }
            // If we don't use the return value of InjectMethod, pop it from the ES
            else if (InjectMethod.ReturnType.FullName != "System.Void")
                il.InsertBefore(startInstruction, il.Create(OpCodes.Pop));
            if (direction == InjectDirection.After)
                il.Remove(startInstruction);
            Logger.LogLine(LogMask.Inject, "Injection complete");
            Logger.LogLine(LogMask.Inject, "##### INJECTION END #####");
        }
    }
}