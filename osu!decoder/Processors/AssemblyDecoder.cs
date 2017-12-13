using System;
using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet;
using EazDecodeLib;
using static osu_decoder_dnlib.Constants;

namespace osu_decoder_dnlib.Processors
{
	internal class AssemblyDecoder : IProcessor
	{
	    public Dictionary<string, string> SrcMap;

        private readonly CryptoHelper _crypto;

	    public AssemblyDecoder(CryptoHelper crypto)
	    {
		    _crypto = crypto;
	    }

		public void Process(ModuleDef def)
		{
            SrcMap = new Dictionary<string, string>();

		    DecodeRecursive(def.Types);
		}

		private void DecodeRecursive(IEnumerable<IFullName> members)
		{
			foreach (IFullName fullName in members) {
			    switch (fullName) {
			        case TypeDef t:
			            DecodeSingle(t);

			            foreach (GenericParam generic in t.GenericParameters)
			                DecodeSingle(generic);

			            DecodeRecursive(t.Events);
			            DecodeRecursive(t.Fields);
			            DecodeRecursive(t.Methods);
			            DecodeRecursive(t.NestedTypes);
			            DecodeRecursive(t.Properties);

			            foreach (InterfaceImpl impl in t.Interfaces)
			                DecodeSingle(impl.Interface);
			            break;
			        case MethodDef m:
			             DecodeSingle(m);

			            foreach (GenericParam generic in m.GenericParameters)
			                DecodeSingle(generic);
			            foreach (Parameter param in m.Parameters)
			                DecodeSingle(param);
			            break;
			        case FieldDef _:
			        case PropertyDef _:
			        case EventDef _:
			            DecodeSingle(fullName);
			            break;
			        default:
			            Program.Verbose("Unhandled type: " + fullName.GetType());
			            DecodeSingle(fullName);
			            break;
			    }
			}
		}

		private void DecodeSingle(IFullName param)
		{
		    if (!RegexObfuscated.IsMatch(param.Name)) return;

		    string text = _crypto.Decrypt(param.Name);
		    SrcMap[param.Name] = text;
			if (param is TypeDef typeDef && text.Contains('.')) {
				typeDef.Namespace = text.Substring(0, text.LastIndexOf('.'));
				typeDef.Name = text.Substring(text.LastIndexOf('.') + 1);
			}
            else if (param is TypeSpec spec)
		        DecodeSingle(spec.ScopeType);
		    else
		        param.Name = text;
		}

		private void DecodeSingle(IVariable param)
		{
		    if (!RegexObfuscated.IsMatch(param.Name)) return;

		    string text = _crypto.Decrypt(param.Name);
		    SrcMap[param.Name] = text;
            param.Name = text;
		}
	}
}
