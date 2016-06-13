using System;
using System.Diagnostics;
using System.IO;

namespace Mono.Cecil.Inject
{
    /// <summary>
    ///     General logging masks to turn on/off certain logging messages (if logging is enabled). These are reserved for
    ///     methods found in Cecil.Inject.
    /// </summary>
    public static class LogMask
    {
        /// <summary>
        ///     Log messages from injection methods.
        /// </summary>
        public const uint Inject = 1;

        /// <summary>
        ///     Log messages from type comparison methods.
        /// </summary>
        public const uint TypeCompare = 2;

        /// <summary>
        ///     Log messages from <see cref="TypeDefinitionExtensions.GetInjectionMethod" />
        /// </summary>
        public const uint GetInjectionMethod = 4;

        /// <summary>
        ///     Log messages from <see cref="TypeDefinitionExtensions.ChangeAccess" />
        /// </summary>
        public const uint ChangeAccess = 8;
    }

    /// <summary>
    ///     Logger. Works only in the DEBUG build.
    /// </summary>
    public static class Logger
    {
        private static uint logMask;

        static Logger()
        {
            LogOutput = Console.Out;
        }

        /// <summary>
        ///     The destination of the log messages. If not set, defaults to the standard output.
        /// </summary>
        public static TextWriter LogOutput { get; set; }

        /// <summary>
        ///     Checks whether a certaing logging flag is set.
        /// </summary>
        /// <param name="mask">The flag to check.</param>
        /// <returns>True, if logging should be done when the given mask is encountered.</returns>
        public static bool IsSet(uint mask)
        {
            return mask == (logMask & mask);
        }

        /// <summary>
        ///     Writes a message to the log if the given logging mask is set.
        /// </summary>
        /// <param name="mask">The logging mask to which to send the message. Think of it like a separate message channel.</param>
        /// <param name="message">Message to log, if the mask is set.</param>
        [Conditional("DEBUG")]
        public static void Log(uint mask, string message)
        {
            if (IsSet(mask))
                LogOutput.Write(message);
        }

        /// <summary>
        ///     Writes a message to the log if the given logging mask is set. Appends a line break at the end of the message.
        /// </summary>
        /// <param name="mask">The logging mask to which to send the message. Think of it like a separate message channel.</param>
        /// <param name="message">Message to log, if the mask is set.</param>
        [Conditional("DEBUG")]
        public static void LogLine(uint mask, string message)
        {
            if (IsSet(mask))
                LogOutput.WriteLine(message);
        }

        /// <summary>
        ///     Writes a line break to the log if the given logging mask is set.
        /// </summary>
        /// <param name="mask">The logging mask to which to send the message. Think of it like a separate message channel.</param>
        [Conditional("DEBUG")]
        public static void LogLine(uint mask)
        {
            if (IsSet(mask))
                LogOutput.WriteLine();
        }

        /// <summary>
        ///     Sets the log mask on so that the log could write messages when it encounters this mask.
        /// </summary>
        /// <param name="mask">The mask to set.</param>
        public static void SetLogMask(uint mask)
        {
            logMask |= mask;
        }

        /// <summary>
        ///     Unsets the log mask on so that the log wouldn't output messages with the given mask.
        /// </summary>
        /// <param name="mask">The mask to unset.</param>
        public static void UnsetMask(uint mask)
        {
            logMask &= ~mask;
        }
    }
}