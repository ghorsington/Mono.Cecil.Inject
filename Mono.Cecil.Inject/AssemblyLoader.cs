using System;
using System.IO;

namespace Mono.Cecil.Inject
{
    /// <summary>
    ///     Class for assembly loading methods.
    /// </summary>
    public static class AssemblyLoader
    {
        /// <summary>
        ///     Loads an assembly from the specified path.
        /// </summary>
        /// <param name="path">Path to the assembly. Can be either relative (to the executing assembly directory) or absolute.</param>
        /// <returns>An instance of <see cref="AssemblyDefinition" /> of the loaded assembly.</returns>
        public static AssemblyDefinition LoadAssembly(string path)
        {
            AssemblyDefinition result;

            if (!File.Exists(path))
                throw new FileNotFoundException($"Missing DLL: {path}");
            using (Stream s = File.OpenRead(path))
            {
                result = AssemblyDefinition.ReadAssembly(s);
            }
            if (result == null)
                throw new NullReferenceException($"Failed to read assembly {path}");
            return result;
        }
    }
}