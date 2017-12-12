using System;
using System.Text.RegularExpressions;

namespace osu_decoder_dnlib
{
    internal static class Constants
    {
        public static readonly Regex RegexObfuscated = new Regex("^#=[a-zA-Z0-9_$]+={0,2}$", RegexOptions.Compiled);
    }
}
