using System;
using System.IO;
using System.Reflection;
using Mono.Cecil;

namespace Mono.Cecil.Inject
{
    public static class AssemblyLoader
    {
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