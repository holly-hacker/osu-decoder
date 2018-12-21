using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;
using dnlib.DotNet;
using EazDecodeLib;
using osu_decoder_dnlib.Processors;

namespace osu_decoder_dnlib
{
	internal class Program
	{
		internal static CliOptions Options = new CliOptions();

		private static void Main(string[] args)
		{
			if (!Parser.Default.ParseArguments(args, Options) || string.IsNullOrWhiteSpace(Options.Input))
			{
				Console.WriteLine(Options.GetHelp());
				return;
			}
			Verbose("Verbose output enabled.");

		    string input = Options.Input;

		    Console.WriteLine("Loading crypto...");
		    var crypto = new CryptoHelper(Program.Options.Password);

            Console.WriteLine("Loading assembly...");
			ModuleDefMD moduleDefMD = ModuleDefMD.Load(input);

			Verbose("Total amount of types in root: " + moduleDefMD.Types.Count);

		    Dictionary<string, string> sourceMap = null;
			if (Options.ExperimentPatch)
			{
				Console.WriteLine("Patching Authenticode/WinVerifyTrust...");
				BinaryPatch.PatchSignatureCheck(moduleDefMD);

				Console.WriteLine("Patching executable name check...");
				BinaryPatch.PatchExecutableName(moduleDefMD);
			}

		    if (!Options.NoTypes)
		    {
		        Console.WriteLine("Decrypting...");
		        var ass = new AssemblyDecoder(crypto);
                ass.Process(moduleDefMD);
		        
                Console.WriteLine("Updating references...");
		        new ReferenceUpdater(sourceMap = ass.SrcMap).Process(moduleDefMD);
		    }
		    else
		    {
		        Console.WriteLine("Decrypting strings...");
		        new AssemblyStringDecoder(crypto).Process(moduleDefMD);

            }


		    int periodIdx = input.LastIndexOf('.');
		    if (periodIdx == -1)
			    periodIdx = input.Length;
            string fileOut = string.IsNullOrEmpty(Options.Output)
                ? input.Substring(0, periodIdx) + "-decrypted" + input.Substring(periodIdx)
                : Options.Output;

			if (Options.Sourcemap && sourceMap != null)
			{
				string srcmap = $"{fileOut}.srcmap";
				Console.WriteLine("Writing sourcemap to " + srcmap);

			    using (StreamWriter sw = new StreamWriter(srcmap))
			        foreach (var entry in sourceMap)
			            sw.WriteLine($"{entry.Key}:{entry.Value}");
			}

			if (!Options.DryRun)
			{
				Console.WriteLine("Writing new assembly...");
				moduleDefMD.Write(fileOut);
				Console.WriteLine("Written to " + fileOut);
			}
            else
            {
                Console.WriteLine("Dry run, not writing new assembly to disk");
            }
        }

		public static void Verbose(string input)
		{
		    if (Options.Verbose || Options.Debug)
		        Console.WriteLine("[v] " + input);
		}

		public static void Debug(string input)
		{
		    if (Options.Debug)
		        Console.WriteLine("[D] " + input);
		}
	}
}
