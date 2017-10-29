using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace osu_decoder_dnlib
{
	internal class SourceMap
	{
		public List<Entry> Entries = new List<Entry>();

		public void Add(string orig, string dec)
		{
		    if (!ContainsKey(orig))
            {
                Entries.Add(new Entry {
		            OriginalName = orig,
		            DecodedName = dec
		        });
            }
        }

		public void Write(string path)
		{
			var sb = new StringBuilder();

		    foreach (Entry entry in Entries)
		        sb.AppendLine($"{entry.OriginalName}:{entry.DecodedName}");

		    File.WriteAllText(path, sb.ToString().TrimEnd('\r', '\n'));
		}

		public bool ContainsKey(string orig)
		{
			return Entries.Any(a => a.OriginalName == orig);
		}

		public struct Entry
		{
			public string OriginalName;
			public string DecodedName;
		}
	}
}
