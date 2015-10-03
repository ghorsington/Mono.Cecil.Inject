using System;

namespace Mono.Cecil.Inject
{
    public enum InjectDirection
    {
        Before,
        After
    }

    [Flags]
    public enum InjectFlags
    {
        None = 0x00,
        ModifyReturn = 0x01,
        PassTag = 0x02,
        PassInvokingInstance = 0x04,
        PassFields = 0x08,
        PassLocals = 0x10,
        PassParametersVal = 0x20,
        PassParametersRef = 0x40,

        All_Val = 0x3E,
        All_Ref = 0x5E
    }

    public struct InjectValues
    {
        public enum PassParametersType
        {
            None = 0,
            ByValue = 1,
            ByReference = 2
        }

        public bool ModifyReturn;
        public PassParametersType ParameterType;
        public bool PassFields;
        public bool PassInvokingInstance;
        public bool PassLocals;
        public bool PassTag;

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

        public bool PassParameters
            => ParameterType == PassParametersType.ByReference || ParameterType == PassParametersType.ByValue;

        public bool PassParametersByRef => ParameterType == PassParametersType.ByReference;

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

    public static class InjectFlagMethods
    {
        public static bool IsSet(this InjectFlags flags, InjectFlags flag)
        {
            return flag == (flags & flag);
        }

        public static InjectValues ToValues(this InjectFlags flags)
        {
            return new InjectValues(flags);
        }
    }
}