using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compiler1._0
{
    class Command
    {
        public string MemShift="";
        public string Instruction;
        public string Label = null;
        public string[] Arguments=new string[3];
        public bool[] Change = { true, true, true};
        public bool[] LoadStore = { true, true, false };
        public bool Touch = true;
        public bool MemReg = false;

    }
}
