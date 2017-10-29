using System;
using CommandLine;
using CommandLine.Text;

namespace osu_decoder_dnlib
{
	// Token: 0x02000004 RID: 4
	internal class CliOptions
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x0600000C RID: 12 RVA: 0x00002388 File Offset: 0x00000588
		// (set) Token: 0x0600000D RID: 13 RVA: 0x00002390 File Offset: 0x00000590
		[ValueOption(0)]
		public string Input { get; set; }

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x0600000E RID: 14 RVA: 0x0000239C File Offset: 0x0000059C
		// (set) Token: 0x0600000F RID: 15 RVA: 0x000023A4 File Offset: 0x000005A4
		[ValueOption(1)]
		public string Input2 { get; set; }

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000010 RID: 16 RVA: 0x000023B0 File Offset: 0x000005B0
		// (set) Token: 0x06000011 RID: 17 RVA: 0x000023B8 File Offset: 0x000005B8
		[Option('o', "output", HelpText = "Path of the output file")]
		public string Output { get; set; }

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000012 RID: 18 RVA: 0x000023C4 File Offset: 0x000005C4
		// (set) Token: 0x06000013 RID: 19 RVA: 0x000023CC File Offset: 0x000005CC
		[Option('v', "verbose", HelpText = "Prints more output.")]
		public bool Verbose { get; set; }

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000014 RID: 20 RVA: 0x000023D8 File Offset: 0x000005D8
		// (set) Token: 0x06000015 RID: 21 RVA: 0x000023E0 File Offset: 0x000005E0
		[Option('d', "debug", HelpText = "Prints a lot more output, you should pipe this to a file.")]
		public bool Debug { get; set; }

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000016 RID: 22 RVA: 0x000023EC File Offset: 0x000005EC
		// (set) Token: 0x06000017 RID: 23 RVA: 0x000023F4 File Offset: 0x000005F4
		[Option('s', "sourcemap", HelpText = "Writes a file containing an original:decoded source map.")]
		public bool Sourcemap { get; set; }

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000018 RID: 24 RVA: 0x00002400 File Offset: 0x00000600
		// (set) Token: 0x06000019 RID: 25 RVA: 0x00002408 File Offset: 0x00000608
		[Option('r', "dry-run", HelpText = "Do not write decoded executable to disk.")]
		public bool DryRun { get; set; }

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x0600001A RID: 26 RVA: 0x00002414 File Offset: 0x00000614
		public string Password { get; } = "recorderinthesandybridge";

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x0600001B RID: 27 RVA: 0x0000241C File Offset: 0x0000061C
		// (set) Token: 0x0600001C RID: 28 RVA: 0x00002424 File Offset: 0x00000624
		[Option("exp-eagerdecode", HelpText = "EXPERIMENTAL: decode from reference in method bodies", DefaultValue = false)]
		public bool ExperimentEagerDecoding { get; set; }

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x0600001F RID: 31 RVA: 0x00002444 File Offset: 0x00000644
		// (set) Token: 0x06000020 RID: 32 RVA: 0x0000244C File Offset: 0x0000064C
		[Option("exp-patch", HelpText = "Apply patch to remove signature and filename check", DefaultValue = false)]
		public bool ExperimentPatch { get; set; }

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000021 RID: 33 RVA: 0x00002458 File Offset: 0x00000658
		// (set) Token: 0x06000022 RID: 34 RVA: 0x00002460 File Offset: 0x00000660
		[Option("exp-full", HelpText = "Decode everything, resulting in unrunnable executable", DefaultValue = false)]
		public bool ExperimentFullDecrypt { get; set; }

		// Token: 0x06000023 RID: 35 RVA: 0x0000246C File Offset: 0x0000066C
		[HelpOption]
		public string GetHelp()
		{
			HelpText helpText = new HelpText();
			helpText.Heading = new HeadingInfo("osu!decoder", "v1.2");
			helpText.Copyright = new CopyrightInfo("HoLLy/JustM3", 2017);
			helpText.AdditionalNewLineAfterOption = false;
			helpText.AddDashesToOption = true;
			helpText.MaximumDisplayWidth = Console.BufferWidth;
			helpText.AddPreOptionsLine("\nExample usage:");
			helpText.AddPreOptionsLine("\t- osu!decoder osu!.exe");
			helpText.AddPreOptionsLine("\t- osu!decoder osu!.exe -v -p updated_password -o c:/newosu!.exe");
			helpText.AddOptions(this);
			helpText.AddPostOptionsLine("Please do not distribute. If you have this, it means I trust you :)");
			return helpText;
		}
	}
}
