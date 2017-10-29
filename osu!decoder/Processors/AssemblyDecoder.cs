using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace osu_decoder_dnlib.Processors
{
	// Token: 0x0200000C RID: 12
	internal class AssemblyDecoder
	{
		// Token: 0x0400001E RID: 30
		private static readonly Regex RegexObfuscated = new Regex("^#=[a-zA-Z0-9_$]+={0,2}$");

		// Token: 0x0400001F RID: 31
		private static CryptoHelper _crypto;

		// Token: 0x04000020 RID: 32
		public static SourceMap SrcMap;

		// Token: 0x0600003B RID: 59 RVA: 0x00002D38 File Offset: 0x00000F38
		public static void Process(ModuleDefMD ass)
		{
			if (Program.Options.ExperimentFullDecrypt)
			{
				Program.Verbose("Unsafe decoding enabled! Output executable will likely not be runnable.");
			}
			AssemblyDecoder._crypto = new CryptoHelper(Program.Options.Password);
			if (Program.Options.Sourcemap)
			{
				AssemblyDecoder.SrcMap = new SourceMap();
			}
			AssemblyDecoder.DecodeRecursive(ass.Types, Program.Options.ExperimentFullDecrypt);
		}

		// Token: 0x0600003C RID: 60 RVA: 0x00002D9C File Offset: 0x00000F9C
		public static void DecodeSingleLayer(ModuleDefMD ass, string password)
		{
			AssemblyDecoder._crypto = new CryptoHelper(password);
			AssemblyDecoder.DecodeAll(ass.Types);
		}

		// Token: 0x0600003D RID: 61 RVA: 0x00002DB4 File Offset: 0x00000FB4
		private static void DecodeAll(IEnumerable<IFullName> members)
		{
			foreach (IFullName param in members)
			{
				AssemblyDecoder.DecodeSingle(param);
			}
		}

		// Token: 0x0600003E RID: 62 RVA: 0x00002DFC File Offset: 0x00000FFC
		private static void DecodeRecursive(IEnumerable<IFullName> members, bool unSafe)
		{
			foreach (IFullName fullName in members)
			{
				TypeDef typeDef;
				if ((typeDef = (fullName as TypeDef)) != null)
				{
					AssemblyDecoder.DecodeSingle(typeDef);
					foreach (GenericParam param in typeDef.GenericParameters)
					{
						AssemblyDecoder.DecodeSingle(param);
					}
					AssemblyDecoder.DecodeRecursive(typeDef.Events, unSafe);
					AssemblyDecoder.DecodeRecursive(typeDef.Fields, unSafe);
					AssemblyDecoder.DecodeRecursive(typeDef.Methods, unSafe);
					AssemblyDecoder.DecodeRecursive(typeDef.NestedTypes, unSafe);
					AssemblyDecoder.DecodeRecursive(typeDef.Properties, unSafe);
					using (IEnumerator<InterfaceImpl> enumerator3 = typeDef.Interfaces.GetEnumerator())
					{
						while (enumerator3.MoveNext())
						{
							InterfaceImpl interfaceImpl = enumerator3.Current;
							AssemblyDecoder.DecodeSingle(interfaceImpl.Interface);
						}
						continue;
					}
				}
				MethodDef methodDef;
				if ((methodDef = (fullName as MethodDef)) != null)
				{
					if (unSafe)
					{
						AssemblyDecoder.DecodeSingle(fullName);
					}
					foreach (GenericParam param2 in methodDef.GenericParameters)
					{
						AssemblyDecoder.DecodeSingle(param2);
					}
					foreach (Parameter param3 in methodDef.Parameters)
					{
						AssemblyDecoder.DecodeSingle(param3);
					}
					if (methodDef.Body == null || !Program.Options.ExperimentEagerDecoding)
					{
						continue;
					}
					using (IEnumerator<Instruction> enumerator5 = methodDef.Body.Instructions.GetEnumerator())
					{
						while (enumerator5.MoveNext())
						{
							IFullName fullName2;
							if ((fullName2 = (enumerator5.Current.Operand as IFullName)) != null && AssemblyDecoder.RegexObfuscated.IsMatch(fullName2.Name))
							{
								AssemblyDecoder.DecodeSingle(fullName2);
							}
						}
						continue;
					}
				}
				if (fullName is FieldDef)
				{
					if (unSafe)
					{
						AssemblyDecoder.DecodeSingle(fullName);
					}
				}
				else if (fullName is PropertyDef || fullName is EventDef)
				{
					AssemblyDecoder.DecodeSingle(fullName);
				}
				else
				{
					Program.Verbose("Unhandled type: " + fullName.GetType());
					AssemblyDecoder.DecodeSingle(fullName);
				}
			}
		}

		// Token: 0x0600003F RID: 63 RVA: 0x000030B8 File Offset: 0x000012B8
		private static void DecodeSingle(IFullName param)
		{
			if (!AssemblyDecoder.RegexObfuscated.IsMatch(param.Name))
			{
				return;
			}
			string text = AssemblyDecoder._crypto.Decrypt(param.Name);
			SourceMap srcMap = AssemblyDecoder.SrcMap;
			if (srcMap != null)
			{
				srcMap.Add(param.Name, text);
			}
			TypeDef typeDef;
			if ((typeDef = (param as TypeDef)) != null && text.Contains('.'))
			{
				typeDef.Namespace = text.Substring(0, text.LastIndexOf('.'));
				typeDef.Name = text.Substring(text.LastIndexOf('.') + 1);
				return;
			}
			param.Name = text;
		}

		// Token: 0x06000040 RID: 64 RVA: 0x00003168 File Offset: 0x00001368
		private static void DecodeSingle(IVariable param)
		{
			if (!AssemblyDecoder.RegexObfuscated.IsMatch(param.Name))
			{
				return;
			}
			string text = AssemblyDecoder._crypto.Decrypt(param.Name);
			SourceMap srcMap = AssemblyDecoder.SrcMap;
			if (srcMap != null)
			{
				srcMap.Add(param.Name, text);
			}
			param.Name = text;
		}
	}
}
