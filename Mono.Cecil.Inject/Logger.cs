using System;
using System.Diagnostics;
using System.IO;

namespace Mono.Cecil.Inject
{
    public static class LogMask
    {
        public const uint Inject = 1;
        public const uint TypeCompare = 2;
        public const uint GetInjectionMethod = 4;
        public const uint ChangeAccess = 8;
    }

    public static class Logger
    {
        private static uint logMask;

        static Logger()
        {
            LogOutput = Console.Out;
        }

        public static TextWriter LogOutput { get; set; }

        public static bool IsSet(uint mask)
        {
            return mask == (logMask & mask);
        }

        [Conditional("DEBUG")]
        public static void Log(uint mask, string message)
        {
            if (IsSet(mask))
                LogOutput.Write(message);
        }

        [Conditional("DEBUG")]
        public static void LogLine(uint mask, string message)
        {
            if (IsSet(mask))
                LogOutput.WriteLine(message);
        }

        [Conditional("DEBUG")]
        public static void LogLine(uint mask)
        {
            if (IsSet(mask))
                LogOutput.WriteLine();
        }

        public static void SetLogMask(uint mask)
        {
            logMask |= mask;
        }

        public static void UnsetMask(uint mask)
        {
            logMask &= ~mask;
        }
    }
}