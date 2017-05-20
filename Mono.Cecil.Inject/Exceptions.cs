using System;

namespace Mono.Cecil.Inject
{
    /// <summary>
    ///     An exception thrown when there is an issue when attempting to link the target method with the injection method.
    /// </summary>
    public class InjectionDefinitionException : Exception
    {
        /// <summary>
        ///     Initialises the exception with a message.
        /// </summary>
        /// <param name="message">Message to display.</param>
        public InjectionDefinitionException(string message) : base(message)
        {
        }
    }
}