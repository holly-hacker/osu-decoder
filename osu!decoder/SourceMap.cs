using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace osu_decoder_dnlib
{
	// Token: 0x02000008 RID: 8
	internal class SourceMap
	{
		// Token: 0x04000017 RID: 23
		public const string NameFormat = "{0}.srcmap";

		// Token: 0x04000018 RID: 24
		public List<SourceMap.Entry> Entries = new List<SourceMap.Entry>();

		// Token: 0x06000031 RID: 49 RVA: 0x0000298C File Offset: 0x00000B8C
		public void Add(string orig, string dec)
		{
			if (this.ContainsKey(orig))
			{
				return;
			}
			SourceMap.Entry item = new SourceMap.Entry
			{
				OriginalName = orig,
				DecodedName = dec
			};
			this.Entries.Add(item);
		}

		// Token: 0x06000032 RID: 50 RVA: 0x000029CC File Offset: 0x00000BCC
		public void Write(string path)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (SourceMap.Entry entry in this.Entries)
			{
				stringBuilder.AppendLine(string.Format("{0}:{1}", entry.OriginalName, entry.DecodedName));
			}
			File.WriteAllText(path, stringBuilder.ToString().TrimEnd(new char[]
			{
				'\r',
				'\n'
			}));
		}

		// Token: 0x06000033 RID: 51 RVA: 0x00002A5C File Offset: 0x00000C5C
		public bool ContainsKey(string orig)
		{
			return this.Entries.Any((SourceMap.Entry a) => a.OriginalName == orig);
		}

		// Token: 0x02000009 RID: 9
		public struct Entry
		{
			// Token: 0x04000019 RID: 25
			public string OriginalName;

			// Token: 0x0400001A RID: 26
			public string DecodedName;
		}
	}
}
