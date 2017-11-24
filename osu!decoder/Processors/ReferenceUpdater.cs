using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace osu_decoder_dnlib.Processors
{
    internal class ReferenceUpdater
    {
        private static readonly Regex RegexObfuscated = new Regex("^#=[a-zA-Z0-9_$]+={0,2}$", RegexOptions.Compiled);

        public static void Process(ModuleDefMD ass) => UpdateRecursive(ass.Types);

        private static void UpdateRecursive(IEnumerable<ITypeOrMethodDef> members)
        {
            foreach (ITypeOrMethodDef def in members)
            {
                switch (def)
                {
                    case TypeDef t:
                        UpdateRecursive(t.Methods);
                        UpdateRecursive(t.NestedTypes);
                        break;
                    case MethodDef m:
                        UpdateSingle(m);
                        break;
                }
            }
        }

        private static void UpdateSingle(MethodDef method)
        {
            foreach (MethodOverride methodOverride in method.Overrides) {
                var baseMethod = methodOverride.MethodDeclaration;

                if (RegexObfuscated.IsMatch(baseMethod.Name)) {
                    baseMethod.Name = AssemblyDecoder.SrcMap.Entries[baseMethod.Name];
                }
            }
            
            if (!method.HasBody) return;
            foreach (Instruction i in method.Body.Instructions) {
                if (i.Operand != null && i.Operand is IFullName f) {
                    if (RegexObfuscated.IsMatch(f.Name)) {
                        f.Name = AssemblyDecoder.SrcMap.Entries[f.Name];
                    }
                }
            }
        }
    }
}
