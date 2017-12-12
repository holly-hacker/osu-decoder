using System;
using System.Collections.Generic;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using EazDecodeLib;
using static osu_decoder_dnlib.Constants;

namespace osu_decoder_dnlib.Processors
{
    internal static class AssemblyStringDecoder
    {
        private static CryptoHelper _crypto;

        public static void Process(ModuleDefMD ass, CryptoHelper crypto)
        {
            _crypto = crypto;

            UpdateRecursive(ass.Types);
        }

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
