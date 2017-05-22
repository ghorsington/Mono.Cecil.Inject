using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace Mono.Cecil.Inject
{
    /// <summary>
    ///     Specifies methods for easy parameter type creation and conversion between <see cref="Type" /> and
    ///     <see cref="TypeReference" />.
    ///     Most likely you won't need to use these, as Cecil.Inject does the conversion for you.
    /// </summary>
    public static class ParamHelper
    {
        private static readonly ModuleDefinition resolverModule;

        static ParamHelper()
        {
            resolverModule = ModuleDefinition.CreateModule("ResolverModule", new ModuleParameters());
        }

        /// <summary>
        ///     Creates a fake type with the specified name to use in place of generic types.
        /// </summary>
        /// <param name="name">Name of the type to create.</param>
        /// <returns>An instance of <see cref="Type" /> for the specified fake type.</returns>
        public static Type CreateDummyType(string name)
        {
            AssemblyName an = new AssemblyName("TmpAssembly");
            AssemblyBuilder ab = Thread.GetDomain().DefineDynamicAssembly(an, AssemblyBuilderAccess.ReflectionOnly);
            ModuleBuilder mb = ab.DefineDynamicModule("TmpModule");
            TypeBuilder tb = mb.DefineType(name);

            return tb.CreateType();
        }

        /// <summary>
        ///     Creates a generic type with a specified name.
        /// </summary>
        /// <param name="name">Name of the generic to create.</param>
        /// <returns>
        ///     Type reference for the generic type. The generic type does not have a namespace nor any properties besides its
        ///     name and the fact that it is a generic type.
        /// </returns>
        public static TypeReference CreateGeneric(string name)
        {
            return new GenericParameter(new TypeReference(name, null, resolverModule, null))
            {
                Name = name
            };
        }

        /// <summary>
        ///     Obtains the type reference from <see cref="System.Type" /> that has generic parameters.
        /// </summary>
        /// <typeparam name="T">Type to turn into Mono.Cecil representation of type refrence.</typeparam>
        /// <returns>Type reference of the type that contains generic parameters.</returns>
        public static TypeReference FromType<T>()
        {
            return FromType(typeof(T));
        }

        /// <summary>
        ///     Obtains the type reference from <see cref="Type" />.
        /// </summary>
        /// <param name="type">Type to turn into Mono.Cecil representation of type refrence.</param>
        /// <returns>An instance of <see cref="TypeReference" /> for the provided type.</returns>
        public static TypeReference FromType(Type type)
        {
            TypeReference tr = resolverModule.Import(type);
            return tr;
        }
    }
}