using System;
using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace osu_decoder_dnlib
{
	internal class BinaryPatch
	{
		public static void PatchSignatureCheck(ModuleDefMD module)
		{
			bool flag = false;
            foreach(TypeDef typeDef in module.Types.Where(a => a.Methods.Any(b => b.IsPinvokeImpl && b.ImplMap.Name == "WinVerifyTrust")))
			{
				foreach (MethodDef methodDef in typeDef.Methods.Where(a => a.ReturnType.TypeName == "Boolean"))
				{
					Program.Verbose("Writing ret true to " + methodDef.FullName);
					methodDef.Body.Instructions.Insert(0, new Instruction(OpCodes.Ldc_I4_1));
					methodDef.Body.Instructions.Insert(1, new Instruction(OpCodes.Ret));
					flag = true;
				}
			}
		    if (!flag)
		        Console.WriteLine("WARNING: did not write any changes.");
		}

		public static void PatchExecutableName(ModuleDefMD module)
		{
			try
			{
				var methodDef = (MethodDef)module.EntryPoint.Body.Instructions[0].Operand;

				Program.Verbose("Patching name check in " + methodDef.FullName);
				IList<Instruction> instructions = methodDef.Body.Instructions;

				int index = instructions.IndexOf(instructions.Last(a => a.OpCode == OpCodes.Brfalse_S));
				for (int i = -4; i < 10; i++)
				{
					instructions[i + index].OpCode = OpCodes.Nop;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Could not patch: " + ex.Message);
			}
		}

		public static MethodDef FindEazStringMethod(ModuleDefMD module)
		{
			var methodDef = (MethodDef)module.EntryPoint.Body.Instructions[0].Operand;
			IList<Instruction> instructions = methodDef.Body.Instructions;

			for (int i = 0; i < methodDef.Body.Instructions.Count; i++)
			{
				Instruction instruction = instructions[i];
				if (instruction.OpCode == OpCodes.Newobj 
                    && instruction.Operand is MemberRef memberRef 
                    && memberRef.Class.Name == "Exception") {
					return (MethodDef)instructions[i - 1].Operand;
				}
			}
			return null;
		}
	}
}
