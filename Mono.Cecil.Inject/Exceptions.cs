using System;

namespace Mono.Cecil.Inject
{
    public class HookDefinitionException : Exception
    {
        public HookDefinitionException(string message) : base(message) {}
    }
}