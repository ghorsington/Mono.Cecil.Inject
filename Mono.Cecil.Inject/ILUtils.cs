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

        /// <summary>
        ///     Replaces the insturction with another by replacing the opcode and the operand.
        ///     Unlike <see cref="ILProcessor.Replace" />, preserves the references to the original instruction (which branches
        ///     might use, for instance).
        /// </summary>
        /// <param name="original">The instruction to replace.</param>
        /// <param name="newIns">The instruction to replace with.</param>
        public static void ReplaceInstruction(Instruction original, Instruction newIns)
        {
            original.OpCode = newIns.OpCode;
            original.Operand = newIns.Operand;
        }
    }
}