using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection.Emit;
using System.Security.Permissions;
using System.Text;
using Mono.Cecil.Inject;
using Mono.Cecil;

namespace Tests
{
    public class TypeDefinitionTests
    {
        public bool testMember;

        private int hiddenStuff;

        private class Nested1
        {
             private class Nested2
             {
                  private class Nested3
                  {
                      void Wow()
                      {
                          Console.WriteLine("Woah");
                      }
                  }
             }
        }

        public byte Test2(int val)
        {
            int v = 1 + val;
            Console.WriteLine("I AM ANOTHER TEST" + v);
            return 0;
        }

        public static bool HookTest1(int tag, TypeDefinitionTests test, out byte ret, ref int v, ref bool testM, ref int val)
        {
            Console.WriteLine("I AM HOOK");
            ret = 1;
            return true;
        }

        public static void Test3(params object[] asd)
        {
            
        }

        public static void Test<T, U>(List<T> test, Dictionary<List<T>, List<Dictionary<U, T>>> woah)
        {
            Console.WriteLine("I AM TEST!");
        }

        public static void Main(string[] args)
        {
            AssemblyDefinition ad = AssemblyLoader.LoadAssembly("Tests.exe");
            TypeDefinition testType = ad.MainModule.GetType("Tests.TypeDefinitionTests");
            Console.WriteLine($"Loaded type: {testType}");

            TypeReference tr = ParamHelper.FromType(typeof (Dictionary<,>).MakeGenericType(ParamHelper.CreateDummyType("T"), ParamHelper.CreateDummyType("T")));

            MethodDefinition md = testType.GetMethod("Test", typeof(List<>).MakeGenericType(ParamHelper.CreateDummyType("T")), typeof(Dictionary<,>).MakeGenericType(typeof(List<>).MakeGenericType(ParamHelper.CreateDummyType("T")), typeof(List<>).MakeGenericType(typeof(Dictionary<,>).MakeGenericType(ParamHelper.CreateDummyType("U"), ParamHelper.CreateDummyType("T")))));

            Console.WriteLine($"Type: {tr}");
            Console.WriteLine($"Method: {md}");

            InjectionDefinition hd = new InjectionDefinition(testType.GetMethod("Test2"), testType.GetMethod("HookTest1"), InjectFlags.ModifyReturn | InjectFlags.PassInvokingInstance | InjectFlags.PassLocals | InjectFlags.PassTag | InjectFlags.PassFields | InjectFlags.PassParametersRef, new []{0}, testType.GetField("testMember"));

            Console.WriteLine($"Hook def: {hd}");

            InjectionDefinition hd2 = testType.GetInjectionMethod(
            "HookTest1",
            testType.GetMethod("Test2"),
            InjectFlags.ModifyReturn | InjectFlags.PassLocals | InjectFlags.PassInvokingInstance | InjectFlags.PassTag | InjectFlags.PassFields
            | InjectFlags.PassParametersRef,
            new[] {0},
            testType.GetField("testMember"));

            InjectionDefinition hd3 = testType.GetInjectionMethod(
            "",
            null,
            InjectFlags.None,
            null,
            null);

            testType.GetMethod("Test2").InjectWith(testType.GetMethod("HookTest1"), 0, 0, InjectFlags.All_Ref | InjectFlags.ModifyReturn, InjectDirection.Before, new []{0}, new []{testType.GetField("testMember")});

            Console.WriteLine($"Another hookdef: {hd2}");

            Console.WriteLine($"Another hookdef3: {hd3}");

            testType.ChangeAccess("*", recursive: true);

            hd2.Inject();

            Console.ReadLine();
        }
    }
}
