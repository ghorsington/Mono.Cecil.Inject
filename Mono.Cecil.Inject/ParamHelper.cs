using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Mono.Cecil;

namespace Mono.Cecil.Inject
{
    public static class ParamHelper
    {
        private static readonly ModuleDefinition resolverModule;

        static ParamHelper()
        {
            resolverModule = ModuleDefinition.CreateModule("ResolverModule", new ModuleParameters());
        }

        public static Type CreateDummyType(string name)
        {
            AssemblyName an = new AssemblyName("TmpAssembly");
            AssemblyBuilder ab = Thread.GetDomain().DefineDynamicAssembly(an, AssemblyBuilderAccess.ReflectionOnly);
            ModuleBuilder mb = ab.DefineDynamicModule("TmpModule");
            TypeBuilder tb = mb.DefineType(name);

            return tb.CreateType();
        }

        public static TypeReference CreateGeneric(string name)
        {
            return new GenericParameter(new TypeReference(name, null, resolverModule, null)) {Name = name};
        }

        public static TypeReference FromType<T>()
        {
            return FromType(typeof (T));
        }

        public static TypeReference FromType(Type type)
        {
            TypeReference tr = resolverModule.Import(type);
            return tr;
        }
    }
}