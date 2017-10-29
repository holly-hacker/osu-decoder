using System;
using CommandLine;
using dnlib.DotNet;
using osu_decoder_dnlib.Processors;

namespace osu_decoder_dnlib
{
	// Token: 0x02000006 RID: 6
	internal class Program
	{
		// Token: 0x04000013 RID: 19
		internal static CliOptions Options = new CliOptions();

		// Token: 0x04000014 RID: 20
		public const bool DebugBuild = false;

		// Token: 0x06000028 RID: 40 RVA: 0x000026C8 File Offset: 0x000008C8
		private static void Main(string[] args)
		{
			if (!Parser.Default.ParseArguments(args, Program.Options) || string.IsNullOrWhiteSpace(Program.Options.Input))
			{
				Console.WriteLine(Program.Options.GetHelp());
				if (args.Length == 0)
				{
					Console.Write("Press any key to exit...");
					Console.ReadKey(true);
				}
				return;
			}
			Program.Verbose("Verbose output enabled.");
			string.IsNullOrWhiteSpace(Program.Options.Input);
			string input = Program.Options.Input;
			Console.WriteLine("Loading assembly...");
			ModuleDefMD moduleDefMD = ModuleDefMD.Load(input);
			Console.WriteLine("Loaded assembly.");
			Program.Verbose("Total amount of types in root: " + moduleDefMD.Types.Count);
			if (Program.Options.ExperimentPatch)
			{
				Console.WriteLine("Patching Authenticode/WinVerifyTrust");
				BinaryPatch.PatchSignatureCheck(moduleDefMD);
				Console.WriteLine("Patching executable name check");
				BinaryPatch.PatchExecutableName(moduleDefMD);
			}
			Console.WriteLine("Decrypting...");
			AssemblyDecoder.Process(moduleDefMD);
			Console.WriteLine("Finished decrypting.");
			string text = (!string.IsNullOrEmpty(Program.Options.Output)) ? Program.Options.Output : (input.Substring(0, input.LastIndexOf('.')) + "-decrypted" + input.Substring(input.LastIndexOf('.')));
			if (Program.Options.Sourcemap)
			{
				AssemblyDecoder.SrcMap.Entries.Sort((SourceMap.Entry entry, SourceMap.Entry entry1) => string.CompareOrdinal(entry.DecodedName, entry1.DecodedName));
				string text2 = string.Format("{0}.srcmap", text);
				Console.WriteLine("Writing sourcemap to " + text2);
				AssemblyDecoder.SrcMap.Write(text2);
			}
			if (!Program.Options.DryRun)
			{
				Console.WriteLine("Writing new assembly...");
				moduleDefMD.Write(text);
				Console.WriteLine("Written to " + text);
				return;
			}
			Console.WriteLine("Dry run, not writing new assembly to disk");
		}

		// Token: 0x06000029 RID: 41 RVA: 0x000028C0 File Offset: 0x00000AC0
		public static void Verbose(string input)
		{
			if (Program.Options.Verbose || Program.Options.Debug)
			{
				Console.WriteLine("[v] " + input);
			}
		}

		// Token: 0x0600002A RID: 42 RVA: 0x000028EC File Offset: 0x00000AEC
		public static void Debug(string input)
		{
			if (Program.Options.Debug)
			{
				Console.WriteLine("[D] " + input);
			}
		}

		// Token: 0x0600002B RID: 43 RVA: 0x0000290C File Offset: 0x00000B0C
		private static void UpdateProgress(string action, int cur, int max)
		{
			int length = max.ToString().Length;
			Console.Write(string.Format("\r{0} [{1}/{2}]", action, cur.ToString().PadLeft(length), max.ToString().PadLeft(length)));
		}
	}
}
