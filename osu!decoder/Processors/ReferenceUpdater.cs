using System;
using System.Collections.Generic;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using static osu_decoder_dnlib.Constants;

namespace osu_decoder_dnlib.Processors
{
    internal class ReferenceUpdater : IProcessor
    {
        private readonly Dictionary<string, string> _srcMap;

        public ReferenceUpdater(Dictionary<string, string> srcMap)
        {
            _srcMap = srcMap;
        }

        public void Process(ModuleDef def) => UpdateRecursive(def.Types);

        private void UpdateRecursive(IEnumerable<ITypeOrMethodDef> members)
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

        private void UpdateSingle(MethodDef method)
        {
            //Update the Overrides for interface implementations
            foreach (MethodOverride methodOverride in method.Overrides) {
                var baseMethod = methodOverride.MethodDeclaration;

                if (RegexObfuscated.IsMatch(baseMethod.Name)) {
                    baseMethod.Name = _srcMap[baseMethod.Name];
                }
            }
            
            //Update calls to methods in method bodies
            if (!method.HasBody) return;
            foreach (Instruction i in method.Body.Instructions) {
                if (i.Operand == null || !(i.Operand is IFullName f)) continue;

                if (RegexObfuscated.IsMatch(f.Name)) {
                    f.Name = _srcMap[f.Name];
                }
            }
        }
    }
}
