namespace fcc
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
        public bool IsNewLine = false;
        public void AddSave(bool notSave)
        {
            Save = Save || !notSave;
        }
        public void AddSave(Register reg)
        {
            Save = Save || reg.Save;
        }
        public Register(Command line,int arg)
        {
            VarName = line.Arguments[arg];
            AddSave(line.LoadStore[arg]);
            if (arg == 0)
                IsNewLine = true;
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
