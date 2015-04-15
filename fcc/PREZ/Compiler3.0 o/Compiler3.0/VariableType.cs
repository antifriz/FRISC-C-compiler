using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compiler1._0
{
    class VariableType
    {
        public string Type;
        public int Length;
        public int Level;
        public string RootType;

        public void Set(string type)
        {
            string TypeRoot;
            Level=0;
            while(type.Contains('*'))
            {
                type=type.Replace("*","");
                Level++;
            }
            TypeRoot=type.TrimEnd();

            RootType=TypeRoot;

            if(Level==0)
                Type=TypeRoot;
            else
                Type=Cwords.Pointer;

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
