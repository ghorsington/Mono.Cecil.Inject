using Mono.Cecil.Cil;

namespace Mono.Cecil.Inject
{
    /// <summary>
    ///     Miscellaneous utilities used by the injector.
    /// </summary>
    public static class ILUtils
    {
        /// <summary>
        ///     Creates a new IL instruction that is a copy of the provided one. Does not link the new instruction to a method.
        /// </summary>
        /// <param name="ins">Instruction to copy.</param>
        /// <returns>A copy of the provided IL instruction.</returns>
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