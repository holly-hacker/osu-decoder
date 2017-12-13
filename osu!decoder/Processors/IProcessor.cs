using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dnlib.DotNet;

namespace osu_decoder_dnlib.Processors
{
    internal interface IProcessor
    {
        void Process(ModuleDef def);
    }
}
