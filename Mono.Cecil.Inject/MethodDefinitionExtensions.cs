namespace Mono.Cecil.Inject
{
    /// <summary>
    ///     Extensions to <see cref="MethodDefinition" />.
    /// </summary>
    public static class MethodDefinitionExtensions
    {
        /// <summary>
        ///     Finds a method that could be used as an injection method (hook) for this method and constructs an instance of
        ///     <see cref="InjectionDefinition" /> from it.
        /// </summary>
        /// <param name="target">This method that is used as a target.</param>
        /// <param name="injectionType">Type that contains the injection method (hook).</param>
        /// <param name="name">Name of the injection method (hook).</param>
        /// <param name="flags">
        ///     Injection flags that specify what values to pass to the injection method and how to inject it. This
        ///     method attempts to find the hook method that satisfies all the specified flags.
        /// </param>
        /// <param name="localsID">
        ///     An array of indicies of local variables to pass to the injection method. Used only if
        ///     <see cref="InjectFlags.PassLocals" /> is specified, otherwise ignored.
        /// </param>
        /// <param name="typeFields">
        ///     An array of class fields from the type the target lies in to pass to the injection method.
        ///     Used only if <see cref="InjectFlags.PassFields" /> is specified, otherwise ignored.
        /// </param>
        /// <returns>
        ///     An instance of <see cref="InjectionDefinition" />, if a suitable injection method is found from the given
        ///     type. Otherwise, null.
        /// </returns>
        public static InjectionDefinition GetInjector(this MethodDefinition target,
                                                      TypeDefinition injectionType,
                                                      string name,
                                                      InjectFlags flags = InjectFlags.None,
                                                      int[] localsID = null,
                                                      params FieldDefinition[] typeFields)
        {
            return injectionType.GetInjectionMethod(name, target, flags, localsID, typeFields);
        }

        /// <summary>
        ///     Inject a hook call into this method.
        /// </summary>
        /// <param name="method">This method that is used as a target.</param>
        /// <param name="injectionMethod">The method the call of which to inject.</param>
        /// <param name="codeOffset">
        ///     The index of the instruction from which to start injecting. If positive, will count from the
        ///     beginning of the method. If negative, will count from the end. For instance, -1 is the method's last instruction
        ///     and 0 is the first.
        /// </param>
        /// <param name="tag">
        ///     If <see cref="InjectFlags.PassTag" /> is specified, the value of this parameter will be passed as a
        ///     parameter to the injection method.
        /// </param>
        /// <param name="flags">Injection flags that specify what values to pass to the injection method and how to inject it.</param>
        /// <param name="dir">The direction in which to insert the call: either above the start code or below it.</param>
        /// <param name="localsID">
        ///     An array of indicies of local variables to pass to the injection method. Used only if
        ///     <see cref="InjectFlags.PassLocals" /> is specified, otherwise ignored.
        /// </param>
        /// <param name="typeFields">
        ///     An array of class fields from the type the target lies in to pass to the injection method.
        ///     Used only if <see cref="InjectFlags.PassFields" /> is specified, otherwise ignored.
        /// </param>
        public static void InjectWith(this MethodDefinition method,
                                      MethodDefinition injectionMethod,
                                      int codeOffset = 0,
                                      int tag = 0,
                                      InjectFlags flags = InjectFlags.None,
                                      InjectDirection dir = InjectDirection.Before,
                                      int[] localsID = null,
                                      FieldDefinition[] typeFields = null)
        {
            InjectionDefinition id = new InjectionDefinition(method, injectionMethod, flags, localsID, typeFields);
            id.Inject(codeOffset, tag, dir);
        }
    }
}