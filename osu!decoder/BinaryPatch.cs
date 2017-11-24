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
			bool found = false;
            foreach(TypeDef typeDef in module.Types.Where(a => a.Methods.Any(b => b.IsPinvokeImpl && b.ImplMap.Name == "WinVerifyTrust")))
			{
				foreach (MethodDef methodDef in typeDef.Methods.Where(a => a.ReturnType.TypeName == "Boolean"))
				{
					Program.Verbose("Writing ret true to " + methodDef.FullName);
					methodDef.Body.Instructions.Insert(0, new Instruction(OpCodes.Ldc_I4_1));
					methodDef.Body.Instructions.Insert(1, new Instruction(OpCodes.Ret));
					found = true;
				}
			}
		    if (!found)
		        Console.WriteLine("WARNING: did not write any changes.");
		}

		public static void PatchExecutableName(ModuleDefMD module)
		{
			try
			{
				var realMain = (MethodDef)module.EntryPoint.Body.Instructions[0].Operand;

				Program.Verbose("Patching name check in " + realMain.FullName);
				IList<Instruction> instructions = realMain.Body.Instructions;

                //Patch out all method name check instructions
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
	}
}
