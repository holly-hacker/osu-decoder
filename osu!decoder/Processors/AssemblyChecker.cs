using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace osu_decoder_dnlib.Processors
{
	// Token: 0x0200000B RID: 11
	internal class AssemblyChecker
	{
		// Token: 0x0400001C RID: 28
		private static readonly Regex RegexObfuscated = new Regex("^#=[a-zA-Z0-9_$]+={0,2}$");

		// Token: 0x0400001D RID: 29
		private static CryptoHelper _crypto;

		// Token: 0x06000037 RID: 55 RVA: 0x00002AC0 File Offset: 0x00000CC0
		public static void Process(ModuleDefMD ass)
		{
			AssemblyChecker._crypto = new CryptoHelper(Program.Options.Password);
			AssemblyChecker.CheckRecursive(ass.Types);
		}

		// Token: 0x06000038 RID: 56 RVA: 0x00002AE4 File Offset: 0x00000CE4
		private static void CheckRecursive(IEnumerable<IFullName> members)
		{
			foreach (IFullName fullName in members)
			{
				AssemblyChecker.RegexObfuscated.IsMatch(fullName.Name);
				if (fullName is TypeDef)
				{
					TypeDef typeDef = (TypeDef)fullName;
					AssemblyChecker.CheckRecursive(typeDef.Events);
					AssemblyChecker.CheckRecursive(typeDef.Fields);
					AssemblyChecker.CheckRecursive(typeDef.Methods);
					AssemblyChecker.CheckRecursive(typeDef.NestedTypes);
					AssemblyChecker.CheckRecursive(typeDef.Properties);
				}
				else if (fullName is MethodDef)
				{
					MethodDef methodDef = (MethodDef)fullName;
					if (methodDef.HasBody)
					{
						foreach (Instruction instruction in methodDef.Body.Instructions)
						{
							if (instruction.Operand is IFullName)
							{
								IFullName fullName2 = (IFullName)instruction.Operand;
								if (AssemblyChecker.RegexObfuscated.IsMatch(fullName2.Name))
								{
									MemberRef memberRef = fullName2 as MemberRef;
									string newValue = AssemblyChecker._crypto.Decrypt(fullName2.Name);
									newValue = memberRef.FullName.Replace(fullName2.Name, newValue);
									if (memberRef.Resolve() == null)
									{
										int num = 0;
										foreach (TypeDef typeDef2 in memberRef.Module.Types)
										{
											memberRef.DeclaringType.Name == typeDef2.Name;
										}
										if (num == 0)
										{
											Console.WriteLine("fuck: " + memberRef.DeclaringType.Name + " | " + memberRef.FullName);
										}
									}
								}
							}
						}
					}
				}
			}
		}
	}
}
