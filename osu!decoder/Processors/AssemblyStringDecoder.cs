using System;
using System.Collections.Generic;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using EazDecodeLib;
using static osu_decoder_dnlib.Constants;

namespace osu_decoder_dnlib.Processors
{
    internal class AssemblyStringDecoder : IProcessor
    {
        private readonly CryptoHelper _crypto;
        
        public AssemblyStringDecoder(CryptoHelper crypto)
        {
            _crypto = crypto;
        }

        public void Process(ModuleDef def)
        {
            UpdateRecursive(def.Types);
        }

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
            //Update calls to methods in method bodies
            if (!method.HasBody) return;
            foreach (Instruction i in method.Body.Instructions)
            {
                if (i.Operand == null || !(i.Operand is string s)) continue;

                i.Operand = RegexObfuscated.Replace(s, match => _crypto.Decrypt(match.Value));
            }
        }
    }
}
