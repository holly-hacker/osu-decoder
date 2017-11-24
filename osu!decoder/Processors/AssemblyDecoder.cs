using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using dnlib.DotNet;
using EazDecodeLib;

namespace osu_decoder_dnlib.Processors
{
	internal class AssemblyDecoder
	{
		public static SourceMap SrcMap;

		private static readonly Regex RegexObfuscated = new Regex("^#=[a-zA-Z0-9_$]+={0,2}$", RegexOptions.Compiled);
		private static CryptoHelper _crypto;

		public static void Process(ModuleDefMD ass)
		{
		    _crypto = new CryptoHelper(Program.Options.Password);

		    SrcMap = new SourceMap();

		    DecodeRecursive(ass.Types);
		}

		private static void DecodeRecursive(IEnumerable<IFullName> members)
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

		private static void DecodeSingle(IFullName param)
		{
		    if (!RegexObfuscated.IsMatch(param.Name)) return;

		    string text = _crypto.Decrypt(param.Name);
		    SrcMap.Add(param.Name, text);
			if (param is TypeDef typeDef && text.Contains('.')) {
				typeDef.Namespace = text.Substring(0, text.LastIndexOf('.'));
				typeDef.Name = text.Substring(text.LastIndexOf('.') + 1);
			}
            else if (param is TypeSpec spec)
		        DecodeSingle(spec.ScopeType);
		    else
		        param.Name = text;
		}

		private static void DecodeSingle(IVariable param)
		{
		    if (!RegexObfuscated.IsMatch(param.Name)) return;

		    string text = _crypto.Decrypt(param.Name);
		    SrcMap.Add(param.Name, text);
		    param.Name = text;
		}
	}
}
