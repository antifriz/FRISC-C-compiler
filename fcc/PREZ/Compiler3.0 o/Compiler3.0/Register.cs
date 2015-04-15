using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compiler1._0
{
    class Register
    {
        public string VarName=null;
        public int RegName;
        public bool Save = false;
        public bool Start = false;
        public bool End = false;
        public bool SaveNow = false;
        public bool LoadNow = false;
        public bool Edited = false;
        public bool isNewLine = false;
        public void AddSave(bool notSave)
        {
            this.Save = this.Save || !notSave;
        }
        public void AddSave(Register reg)
        {
            this.Save = this.Save || reg.Save;
        }
        public Register(Command line,int arg)
        {
            VarName = line.Arguments[arg];
            AddSave(line.LoadStore[arg]);
            if (arg == 0)
                isNewLine = true;
        }
        public Register()
        {
            RegName = -1;
        }
        public Register(bool a)
        {
            VarName = "";
        }
    }
}
