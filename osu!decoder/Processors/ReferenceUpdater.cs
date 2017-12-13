using System;
using System.Collections.Generic;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using static osu_decoder_dnlib.Constants;

namespace osu_decoder_dnlib.Processors
{
    internal class ReferenceUpdater
    {
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
            //Update the Overrides for interface implementations
            foreach (MethodOverride methodOverride in method.Overrides) {
                var baseMethod = methodOverride.MethodDeclaration;

                if (RegexObfuscated.IsMatch(baseMethod.Name)) {
                    baseMethod.Name = AssemblyDecoder.SrcMap[baseMethod.Name];
                }
            }
            
            //Update calls to methods in method bodies
            if (!method.HasBody) return;
            foreach (Instruction i in method.Body.Instructions) {
                if (i.Operand == null || !(i.Operand is IFullName f)) continue;

                if (RegexObfuscated.IsMatch(f.Name)) {
                    f.Name = AssemblyDecoder.SrcMap[f.Name];
                }
            }
        }
    }
}
