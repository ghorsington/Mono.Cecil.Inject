using Mono.Cecil;

namespace Mono.Cecil.Inject
{
    public static class MethodDefinitionExtensions
    {
        public static void InjectWith(this MethodDefinition method,
                                      MethodDefinition injectionMethod,
                                      int codeOffset,
                                      int tag = 0,
                                      InjectFlags flags = InjectFlags.None,
                                      InjectDirection dir = InjectDirection.Before,
                                      int[] localsID = null,
                                      FieldDefinition[] typeFields = null)
        {
            InjectionDefinition id = new InjectionDefinition(method, injectionMethod, flags, localsID, typeFields);
            id.Inject(codeOffset, tag, dir);
        }

        public static InjectionDefinition GetInjector(this MethodDefinition target,
                                                      TypeDefinition type,
                                                      string name,
                                                      InjectFlags flags = InjectFlags.None,
                                                      int[] localsID = null,
                                                      params FieldDefinition[] typeFields)
        {
            return type.GetInjectionMethod(name, target, flags, localsID, typeFields);
        }
    }
}