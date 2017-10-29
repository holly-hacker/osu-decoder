using System;
using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace osu_decoder_dnlib
{
	// Token: 0x02000002 RID: 2
	internal class BinaryPatch
	{
		// Token: 0x06000002 RID: 2 RVA: 0x00002058 File Offset: 0x00000258
		public static void PatchSignatureCheck(ModuleDefMD module)
		{
			bool flag = false;
			foreach (TypeDef typeDef in from a in module.Types
			where a.Methods.Any((MethodDef b) => b.IsPinvokeImpl && b.ImplMap.Name == "WinVerifyTrust")
			select a)
			{
				foreach (MethodDef methodDef in typeDef.Methods.Where((MethodDef a) => a.ReturnType.TypeName == "Boolean"))
				{
					Program.Verbose("Writing ret true to " + methodDef.FullName);
					methodDef.Body.Instructions.Insert(0, new Instruction(OpCodes.Ldc_I4_1));
					methodDef.Body.Instructions.Insert(1, new Instruction(OpCodes.Ret));
					flag = true;
				}
			}
			if (!flag)
			{
				Console.WriteLine("WARNING: did not write any changes.");
			}
		}

		// Token: 0x06000003 RID: 3 RVA: 0x0000217C File Offset: 0x0000037C
		public static void PatchExecutableName(ModuleDefMD module)
		{
			try
			{
				MethodDef methodDef = (MethodDef)module.EntryPoint.Body.Instructions[0].Operand;
				Program.Verbose("Patching name check in " + methodDef.FullName);
				IList<Instruction> instructions = methodDef.Body.Instructions;
				Instruction item = instructions.Last((Instruction a) => a.OpCode == OpCodes.Brfalse_S);
				int num = instructions.IndexOf(item);
				for (int i = -4; i < 10; i++)
				{
					instructions[i + num].OpCode = OpCodes.Nop;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Could not patch: " + ex.Message);
			}
		}

		// Token: 0x06000004 RID: 4 RVA: 0x0000224C File Offset: 0x0000044C
		public static MethodDef FindEazStringMethod(ModuleDefMD module)
		{
			MethodDef methodDef = (MethodDef)module.EntryPoint.Body.Instructions[0].Operand;
			IList<Instruction> instructions = methodDef.Body.Instructions;
			for (int i = 0; i < methodDef.Body.Instructions.Count; i++)
			{
				Instruction instruction = instructions[i];
				MemberRef memberRef;
				if (instruction.OpCode == OpCodes.Newobj && (memberRef = (instruction.Operand as MemberRef)) != null && memberRef.Class.Name == "Exception")
				{
					return (MethodDef)instructions[i - 1].Operand;
				}
			}
			return null;
		}
	}
}
