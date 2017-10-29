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
		    if (Program.Options.ExperimentFullDecrypt)
		        Program.Verbose("Unsafe decoding enabled! Output executable will likely not be runnable.");

		    _crypto = new CryptoHelper(Program.Options.Password);

		    if (Program.Options.Sourcemap)
		        SrcMap = new SourceMap();

		    DecodeRecursive(ass.Types, Program.Options.ExperimentFullDecrypt);
		}

		public static void DecodeSingleLayer(ModuleDefMD ass, string password)
		{
			_crypto = new CryptoHelper(password);
			DecodeAll(ass.Types);
		}

		private static void DecodeAll(IEnumerable<IFullName> members)
		{
			foreach (IFullName param in members)
			{
				DecodeSingle(param);
			}
		}

		private static void DecodeRecursive(IEnumerable<IFullName> members, bool unSafe)
		{
			foreach (IFullName fullName in members) {
			    switch (fullName) {
			        case TypeDef typeDef:
			            DecodeSingle(typeDef);
			            foreach (GenericParam param in typeDef.GenericParameters)
			                DecodeSingle(param);
			            DecodeRecursive(typeDef.Events, unSafe);
			            DecodeRecursive(typeDef.Fields, unSafe);
			            DecodeRecursive(typeDef.Methods, unSafe);
			            DecodeRecursive(typeDef.NestedTypes, unSafe);
			            DecodeRecursive(typeDef.Properties, unSafe);
			            foreach (var impl in typeDef.Interfaces)
			                DecodeSingle(impl.Interface);
			            break;
			        case MethodDef methodDef:
			            if (unSafe)
			                DecodeSingle(fullName);
			            foreach (GenericParam param2 in methodDef.GenericParameters)
			                DecodeSingle(param2);
			            foreach (Parameter param3 in methodDef.Parameters)
			                DecodeSingle(param3);
			            if (methodDef.Body == null || !Program.Options.ExperimentEagerDecoding)
			                continue;

			            foreach (var instruction in methodDef.Body.Instructions)
			                if (instruction.Operand is IFullName fn && RegexObfuscated.IsMatch(fn.Name))
			                    DecodeSingle(fn);
			            break;
			        case FieldDef _:
			            if (unSafe)
			                DecodeSingle(fullName);
			            break;
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
		    SrcMap?.Add(param.Name, text);
		    TypeDef typeDef;
			if ((typeDef = param as TypeDef) != null && text.Contains('.'))
			{
				typeDef.Namespace = text.Substring(0, text.LastIndexOf('.'));
				typeDef.Name = text.Substring(text.LastIndexOf('.') + 1);
				return;
			}
			param.Name = text;
		}

		private static void DecodeSingle(IVariable param)
		{
		    if (!RegexObfuscated.IsMatch(param.Name)) return;

		    string text = _crypto.Decrypt(param.Name);
		    SrcMap?.Add(param.Name, text);
		    param.Name = text;
		}
	}
}
