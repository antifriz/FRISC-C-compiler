using System.Linq;

namespace fcc
{
    class VariableType
    {
        public string Type;
        public int Length;
        public int Level;
        public string RootType;

        public void Set(string type)
        {
            Level=0;
            while(type.Contains('*'))
            {
                type=type.Replace("*","");
                Level++;
            }
            string typeRoot = type.TrimEnd();

            RootType=typeRoot;

            Type = Level==0 ? typeRoot : Cwords.Pointer;

            Length = GetLength(Type);
        }
        public int GetLength(string c)
        {
            return c.GetLength();
        }
        public int GetRootLength()
        {
            return RootType.GetLength();
        }
        public bool IsSigned()
        {
            return Type.IsSigned();
        }
        public void DownType()
        {
            if (Level == 0)
                return;
            if (Level == 1)
            {
                Level--;
                Type = RootType;
                Length = GetLength(Type);
            }
            else
            {
                Level--;
            }
        }
        public void UpType()
        {
            if (Level == 0)
            {
                Level++;
                Type = Cwords.Pointer;
                Length = 4;
            }
            else
            {
                Level++;
            }
        }
    }
}
