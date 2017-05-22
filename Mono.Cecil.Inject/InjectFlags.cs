using System;

namespace Mono.Cecil.Inject
{
    /// <summary>
    ///     Specifies the direction in which to insert the injection code.
    /// </summary>
    public enum InjectDirection
    {
        /// <summary>
        ///     Insert the injection code before the specified instruction.
        /// </summary>
        Before,

        /// <summary>
        ///     Insert the injection code after the specified instruction.
        /// </summary>
        After
    }

    /// <summary>
    ///     Enumeration of possible parameters to pass to the injection method and different ways to inject the code.
    ///     The flags are used by Cecil.Inject to search for the right injection method and to inject it the right way.
    ///     The enumeration also dictates the order in which certain method parameters must be specified in the injection
    ///     (hook) method
    ///     in order to be recognized by Cecil.Inject correctly.
    /// </summary>
    [Flags]
    public enum InjectFlags
    {
        /// <summary>
        ///     No paramaters are passed to the injection method (hook). Therefore the hook method must not have any parameters in
        ///     its signature.
        /// </summary>
        None = 0x00,

        /// <summary>
        ///     A 32-bit constant signed integer is passed to the injection method. The constant is specified when injecting.
        ///     <b>Note:</b> Only one type of a constant tag can be used at a time. If both are set, <see cref="PassTag" />
        ///     overrides <see cref="PassStringTag" />.
        /// </summary>
        PassTag = 0x01,

        /// <summary>
        ///     Pass the instance of the type which calls (invokes) the injection method (hook). The type of this parameter is the
        ///     one that contains the injected method.
        /// </summary>
        PassInvokingInstance = 0x01 << 1,

        /// <summary>
        ///     Specifies Cecil.Inject that the injection method should be allowed to prematurely stop the execution of the
        ///     injected method.
        ///     In addition, of the injected method has a return value, the injection method (hook) should be allowed to modify it.
        ///     In this case the return type of the injection method (hook) MUST be a boolean. If the injected method has a return
        ///     type (not void), the hook must
        ///     have a reference parameter (marked as "out") of the same type as that of what the injected method returns.
        /// </summary>
        ModifyReturn = 0x01 << 2,

        /// <summary>
        ///     Pass local variables found in the injected method. The variables will be passed as references (marked as "ref").
        /// </summary>
        PassLocals = 0x01 << 3,

        /// <summary>
        ///     Pass class fields of the type that contains the injected method. The method must not be static.
        ///     The variables will be passed as references (marked as "ref").
        /// </summary>
        PassFields = 0x01 << 4,

        /// <summary>
        ///     Pass all parameters of the injected method. The parameters are passed by value.
        ///     <b>Note:</b> Some methods (like constructor of <see cref="InjectionDefinition" />) implement partial parameter
        ///     passing.
        ///     It means that the injection method doesn't need to have all of the parameters for injected method (as long as they
        ///     are in the same order).
        /// </summary>
        PassParametersVal = 0x01 << 5,

        /// <summary>
        ///     Pass all parameters of the injected method. The parameters are passed by reference (marked as "ref").
        ///     <b>Note:</b> Some methods (like constructor of <see cref="InjectionDefinition" />) implement partial parameter
        ///     passing.
        ///     It means that the injection method doesn't need to have all of the parameters for injected method (as long as they
        ///     are in the same order).
        /// </summary>
        PassParametersRef = 0x01 << 6,

        /// <summary>
        ///     A constant string is passed to the injection method. The constant is specified when performing the injection.
        ///     <b>Note:</b> Only one type of a constant tag can be used at a time. If both are set, <see cref="PassTag" />
        ///     overrides <see cref="PassStringTag" />.
        /// </summary>
        PassStringTag = 0x01 << 7,

        /// <summary>
        ///     Same as PassTag | PassInvokingInstance | PassLocals | PassFields | PassParametersVal
        /// </summary>
        All_Val = 0x3E,

        /// <summary>
        ///     Same as PassTag | PassInvokingInstance | PassLocals | PassFields | PassParametersRef
        /// </summary>
        All_Ref = 0x5E
    }

    /// <summary>
    ///     A convenience sturcture that can be used to process <see cref="InjectFlags" /> as properties instead of flags.
    /// </summary>
    public struct InjectValues
    {
        /// <summary>
        ///     Enumeration of how the parameters are passed
        /// </summary>
        public enum PassParametersType
        {
            /// <summary>
            ///     No parameters are passed at all
            /// </summary>
            None = 0,

            /// <summary>
            ///     Pass parameters by value
            /// </summary>
            ByValue = 1,

            /// <summary>
            ///     Pass parameters by reference
            /// </summary>
            ByReference = 2,

            Max
        }

        /// <summary>
        ///     Enumeration of how the tag is passed
        /// </summary>
        public enum PassTagType
        {
            /// <summary>
            ///     No tags are passed.
            /// </summary>
            None = 0,

            /// <summary>
            ///     An integer tag is passed
            /// </summary>
            Int32 = 1,

            /// <summary>
            ///     A string tag is passed
            /// </summary>
            String = 2,

            Max
        }

        /// <summary>
        ///     Specifies Cecil.Inject that the injection method should be allowed to prematurely stop the execution of the
        ///     injected method.
        ///     In addition, of the injected method has a return value, the injection method (hook) should be allowed to modify it.
        ///     In this case the return type of the injection method (hook) MUST be a boolean. If the injected method has a return
        ///     type (not void), the hook must
        ///     have a reference parameter (marked as "out") of the same type as that of what the injected method returns.
        /// </summary>
        public bool ModifyReturn;

        /// <summary>
        ///     Specifies how parameters should be passed
        /// </summary>
        public PassParametersType ParameterType;

        /// <summary>
        ///     Pass class fields of the type that contains the injected method. The method must not be static.
        ///     The variables will be passed as references (marked as "ref").
        /// </summary>
        public bool PassFields;

        /// <summary>
        ///     Pass the instance of the type which calls (invokes) the injection method (hook). The type of this parameter is the
        ///     one that contains the injected method.
        /// </summary>
        public bool PassInvokingInstance;

        /// <summary>
        ///     Pass local variables found in the injected method. The variables will be passed as references (marked as "ref").
        /// </summary>
        public bool PassLocals;


        /// <summary>
        ///     Type of the tag that will get passed.
        /// </summary>
        public PassTagType TagType;

        /// <summary>
        ///     Convert the injection flags into an instance of InjectValues.
        /// </summary>
        /// <param name="flags">Flags to convert.</param>
        public InjectValues(InjectFlags flags)
        {
            ModifyReturn = flags.IsSet(InjectFlags.ModifyReturn);
            TagType = flags.IsSet(InjectFlags.PassTag)
                ? PassTagType.Int32
                : flags.IsSet(InjectFlags.PassStringTag)
                    ? PassTagType.String
                    : PassTagType.None;
            PassInvokingInstance = flags.IsSet(InjectFlags.PassInvokingInstance);
            PassFields = flags.IsSet(InjectFlags.PassFields);
            PassLocals = flags.IsSet(InjectFlags.PassLocals);
            ParameterType = flags.IsSet(InjectFlags.PassParametersVal)
                ? PassParametersType.ByValue
                : (flags.IsSet(InjectFlags.PassParametersRef)
                    ? PassParametersType.ByReference
                    : PassParametersType.None);
        }

        /// <summary>
        ///     Pass a constant value to the injection method. The value is specified when calling
        ///     <see cref="InjectionDefinition.Inject(int,object,Mono.Cecil.Inject.InjectDirection)" />.
        /// </summary>
        public bool PassTag => PassTagType.None < TagType && TagType < PassTagType.Max;

        /// <summary>
        ///     If true, parameters will be passed (either by value or by reference).
        /// </summary>
        public bool PassParameters => PassParametersType.None < ParameterType && ParameterType < PassParametersType.Max;

        /// <summary>
        ///     If true, parameters will be passed by reference.
        /// </summary>
        public bool PassParametersByRef => ParameterType == PassParametersType.ByReference;

        /// <summary>
        ///     Combines the specified properties into an equivalent value of <see cref="InjectFlags" />.
        /// </summary>
        /// <returns>The combination of the properties in a single <see cref="InjectFlags" /> value to use in injection.</returns>
        public InjectFlags GetCombinedFlags()
        {
            return (ModifyReturn ? InjectFlags.ModifyReturn : 0) |
                   (TagType == PassTagType.Int32
                       ? InjectFlags.PassTag
                       : TagType == PassTagType.String
                           ? InjectFlags.PassStringTag
                           : 0) |
                   (PassInvokingInstance ? InjectFlags.PassInvokingInstance : 0) |
                   (PassFields ? InjectFlags.PassFields : 0) |
                   (PassLocals ? InjectFlags.PassLocals : 0) |
                   (ParameterType == PassParametersType.ByValue
                       ? InjectFlags.PassParametersVal
                       : (ParameterType == PassParametersType.ByReference ? InjectFlags.PassParametersRef : 0));
        }
    }

    /// <summary>
    ///     Extension methods for <see cref="InjectFlags" /> enumeration.
    /// </summary>
    public static class InjectFlagMethods
    {
        /// <summary>
        ///     Checks whether a certain flag has been set in the given flag.
        /// </summary>
        /// <param name="flags">Flag combination to check.</param>
        /// <param name="flag">Flag to check with.</param>
        /// <returns>True, if the specified flag is specified in the flag combination.</returns>
        public static bool IsSet(this InjectFlags flags, InjectFlags flag)
        {
            return flag == (flags & flag);
        }

        /// <summary>
        ///     Converts the flag (combination) into an instance of <see cref="InjectValues" />.
        /// </summary>
        /// <param name="flags">Flags to convert.</param>
        /// <returns>The injection flags represented as an instance of <see cref="InjectValues" />.</returns>
        public static InjectValues ToValues(this InjectFlags flags)
        {
            return new InjectValues(flags);
        }
    }
}