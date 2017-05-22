using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Inject;

namespace Tests
{
    public class TypeDefinitionTests
    {
        public bool testMember;
        private int hiddenStuff;

        public static bool HookTest1(int tag, TypeDefinitionTests test, out byte ret, ref bool testM, ref int val)
        {
            Console.WriteLine("I AM HOOK");
            ret = 1;
            return true;
        }

        public static bool HookTest2(int tag,
                                     TypeDefinitionTests test,
                                     out int ret,
                                     int val1,
                                     byte val2,
                                     List<int> val3)
        {
            ret = 1;
            return false;
        }

        public static bool HookTest3(string tag,
                                     TypeDefinitionTests test,
                                     out int ret,
                                     int val1,
                                     byte val2,
                                     List<int> val3)
        {
            ret = 1;
            return false;
        }

        public static void Main(string[] args)
        {
            AssemblyDefinition ad = AssemblyLoader.LoadAssembly("Tests.exe");
            TypeDefinition testType = ad.MainModule.GetType("Tests.TypeDefinitionTests");
            Console.WriteLine($"Loaded type: {testType}");

            TypeReference tr =
                    ParamHelper.FromType(
                        typeof(Dictionary<,>).MakeGenericType(ParamHelper.CreateDummyType("T"),
                                                              ParamHelper.CreateDummyType("T")));

            MethodDefinition md = testType.GetMethod(
                "Test",
                typeof(List<>).MakeGenericType(ParamHelper.CreateDummyType("T")),
                typeof(Dictionary<,>).MakeGenericType(
                    typeof(List<>).MakeGenericType(ParamHelper.CreateDummyType("T")),
                    typeof(List<>).MakeGenericType(
                        typeof(Dictionary<,>).MakeGenericType(
                            ParamHelper.CreateDummyType("U"),
                            ParamHelper.CreateDummyType("T")))));

            Console.WriteLine($"Type: {tr}");
            Console.WriteLine($"Method: {md}");

            InjectionDefinition hd = new InjectionDefinition(
                testType.GetMethod("Test2"),
                testType.GetMethod("HookTest1"),
                InjectFlags.ModifyReturn |
                InjectFlags.PassInvokingInstance |
                InjectFlags.PassTag |
                InjectFlags.PassFields |
                InjectFlags.PassParametersRef,
                new[] {0},
                testType.GetField("testMember"));

            Console.WriteLine($"Hook def: {hd}");

            InjectionDefinition hd2 = testType.GetInjectionMethod(
                "HookTest1",
                testType.GetMethod("Test2"),
                InjectFlags.ModifyReturn |
                InjectFlags.PassInvokingInstance |
                InjectFlags.PassTag |
                InjectFlags.PassFields |
                InjectFlags.PassParametersRef,
                new[] {0},
                testType.GetField("testMember"));

            InjectionDefinition hd3 = testType.GetInjectionMethod("", null, InjectFlags.None, null, null);
            /*
            testType.GetMethod("Test2")
                    .InjectWith(
                    testType.GetMethod("HookTest1"),
                    0,
                    0,
                    InjectFlags.All_Ref | InjectFlags.ModifyReturn,
                    InjectDirection.Before,
                    new[] {0},
                    new[] {testType.GetField("testMember")});
            */
            InjectionDefinition hd4 = new InjectionDefinition(
                testType.GetMethod(nameof(TestPartialParams)),
                testType.GetMethod(nameof(HookTest2)),
                new InjectValues
                {
                    TagType = InjectValues.PassTagType.Int32,
                    PassInvokingInstance = true,
                    ModifyReturn = true,
                    ParameterType = InjectValues.PassParametersType.ByValue
                }.GetCombinedFlags());

            hd4.Inject(token: 5);

            InjectionDefinition hd5 = new InjectionDefinition(
                testType.GetMethod(nameof(TestPartialParams)),
                testType.GetMethod(nameof(HookTest3)),
                new InjectValues
                {
                    TagType = InjectValues.PassTagType.String,
                    PassInvokingInstance = true,
                    ModifyReturn = true,
                    ParameterType = InjectValues.PassParametersType.ByValue
                }.GetCombinedFlags());

            hd5.Inject(token: "test");

            MethodDefinition[] matches = testType.MatchMethod("TestMatch", typeof(int), typeof(string));

            Console.WriteLine($"Another hookdef: {hd2}");

            Console.WriteLine($"Another hookdef3: {hd3}");

            testType.ChangeAccess("hidden.*", recursive: true);

            hd2.Inject(2, 2);

            ad.Write("Test_patched.exe");

            Console.ReadLine();
        }

        public static void Test<T, U>(List<T> test, Dictionary<List<T>, List<Dictionary<U, T>>> woah)
        {
            Console.WriteLine("I AM TEST!");
        }

        public static void Test3(params object[] asd)
        {
        }

        public byte Test2(int val)
        {
            Console.WriteLine("I AM ANOTHER TEST");
            return 0;
        }

        protected void TestMatch(int val1, bool val2)
        {
            Console.WriteLine("I am Groot!");
        }

        protected void TestMatch(int val1, bool val2, byte val3)
        {
            Console.WriteLine("I am Groot!");
        }

        protected void TestMatch(int val1, string val2)
        {
            Console.WriteLine("I am Groot!");
        }

        private int TestPartialParams(int val1, byte val2, List<int> val3)
        {
            return -1;
        }

        private class Nested1
        {
            private class Nested2
            {
                private class Nested3
                {
                    private void Wow()
                    {
                        Console.WriteLine("Woah");
                    }
                }
            }
        }
    }
}