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
        ///     A 32-bit signed integer is passed to the injection method. The value is specified when injection the method.
        /// </summary>
        PassTag = 0x01,

        /// <summary>
        ///     Pass the instance of the type which calls (invokes) the injection method (hook). The type of this parameter is the
        ///     one that contains the injected method.
        /// </summary>
        PassInvokingInstance = 0x02,

        /// <summary>
        ///     Specifies Cecil.Inject that the injection method should be allowed to prematurely stop the execution of the
        ///     injected method.
        ///     In addition, of the injected method has a return value, the injection method (hook) should be allowed to modify it.
        ///     In this case the return type of the injection method (hook) MUST be a boolean. If the injected method has a return
        ///     type (not void), the hook must
        ///     have a reference parameter (marked as "out") of the same type as that of what the injected method returns.
        /// </summary>
        ModifyReturn = 0x04,

        /// <summary>
        ///     Pass local variables found in the injected method. The variables will be passed as references (marked as "ref").
        /// </summary>
        PassLocals = 0x08,

        /// <summary>
        ///     Pass class fields of the type that contains the injected method. The method must not be static.
        ///     The variables will be passed as references (marked as "ref").
        /// </summary>
        PassFields = 0x10,

        /// <summary>
        ///     Pass all parameters of the injected method. The parameters are passed by value.
        ///     <b>Note:</b> Some methods (like constructor of <see cref="InjectionDefinition" />) implement partial parameter
        ///     passing.
        ///     It means that the injection method doesn't need to have all of the parameters for injected method (as long as they
        ///     are in the same order).
        /// </summary>
        PassParametersVal = 0x20,

        /// <summary>
        ///     Pass all parameters of the injected method. The parameters are passed by reference (marked as "ref").
        ///     <b>Note:</b> Some methods (like constructor of <see cref="InjectionDefinition" />) implement partial parameter
        ///     passing.
        ///     It means that the injection method doesn't need to have all of the parameters for injected method (as long as they
        ///     are in the same order).
        /// </summary>
        PassParametersRef = 0x40,

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
            ByReference = 2
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
        ///     Pass a 32-bit signed integer to the injection method. The value is specified when injection the method.
        /// </summary>
        public bool PassTag;

        /// <summary>
        ///     Convert the injection flags into an instance of InjectValues.
        /// </summary>
        /// <param name="flags">Flags to convert.</param>
        public InjectValues(InjectFlags flags)
        {
            ModifyReturn = flags.IsSet(InjectFlags.ModifyReturn);
            PassTag = flags.IsSet(InjectFlags.PassTag);
            PassInvokingInstance = flags.IsSet(InjectFlags.PassInvokingInstance);
            PassFields = flags.IsSet(InjectFlags.PassFields);
            PassLocals = flags.IsSet(InjectFlags.PassLocals);
            ParameterType = flags.IsSet(InjectFlags.PassParametersVal)
                            ? PassParametersType.ByValue
                            : (flags.IsSet(InjectFlags.PassParametersRef)
                               ? PassParametersType.ByReference : PassParametersType.None);
        }

        /// <summary>
        ///     If true, parameters will be passed (either by value or by reference).
        /// </summary>
        public bool PassParameters
            => ParameterType == PassParametersType.ByReference || ParameterType == PassParametersType.ByValue;

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
            return (ModifyReturn ? InjectFlags.ModifyReturn : 0) | (PassTag ? InjectFlags.PassTag : 0)
                   | (PassInvokingInstance ? InjectFlags.PassInvokingInstance : 0)
                   | (PassFields ? InjectFlags.PassFields : 0) | (PassLocals ? InjectFlags.PassLocals : 0)
                   | (ParameterType == PassParametersType.ByValue
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