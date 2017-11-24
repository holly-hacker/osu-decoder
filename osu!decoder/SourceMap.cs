using System.Collections.Generic;
using System.IO;
using System.Text;

namespace osu_decoder_dnlib
{
	internal class SourceMap
	{
        public Dictionary<string, string> Entries = new Dictionary<string, string>();

		public void Add(string orig, string dec)
		{
            Entries[orig] = dec;
        }

		public void Write(string path)
		{
			var sb = new StringBuilder();

		    foreach (var idk in Entries)
		        sb.AppendLine($"{idk.Key}:{idk.Value}");

		    File.WriteAllText(path, sb.ToString().TrimEnd('\r', '\n'));
		}
	}
}
