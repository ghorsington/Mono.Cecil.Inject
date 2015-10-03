using Mono.Cecil.Cil;

namespace Mono.Cecil.Inject
{
    public static class ILUtils
    {
        public static Instruction CopyInstruction(Instruction ins)
        {
            Instruction result;
            if (ins.Operand == null)
                result = Instruction.Create(ins.OpCode);
            else
            {
                result =
                (Instruction)
                typeof (Instruction).GetMethod("Create", new[] {typeof (OpCode), ins.Operand.GetType()})
                                    .Invoke(null, new[] {ins.OpCode, ins.Operand});
            }
            return result;
        }
    }
}