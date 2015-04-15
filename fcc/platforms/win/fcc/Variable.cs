using System;
using System.Collections.Generic;
using System.Linq;

namespace fcc
{
    class Variable
    {
        public string Name = null;
        public bool IsTemp = false;
        public bool IsConst = false;
        public string Value = "000000000";
        public bool ValueSet = false;
        public VariableType Type=new VariableType();
        public List<int> Array=new List<int>();
        public List<bool> ArraySet = new List<bool>();
        public List<string> ArrayValues = new List<string>();
        public string ArrayString = null; //shift in functions
        public bool IsArray = false;
        public bool IsArrayEmpty = true;
        public bool IsGlobal = false;

        public void SetVariable(string name,string type)
        {
            IsArray = false;
            Name = name;
            Type.Set(type);
            if(!Name.Contains('['))
                return;
            var s = Name.Split('[');
            Name = s[0];
            foreach (var c in s)
            {
                if (c.Equals(s[0]))
                    continue;
                Array.Add(int.Parse(c.TrimEnd(']')));
            }
            Type.Level++;
            Type.Type = Cwords.Pointer;
            Type.Length = 4;
            Type.RootType = type;
            IsArray = true;
            NullArray();
        }
        public int ArrayLength()
        {
            if (Array.Count <= 0)
                return 0;
            var mul=1;
            foreach (var i in Array)
                mul *= i;
            return mul;
        }
        public void NonNullArray()
        {
            var n = ArrayLength();
            for (var i = 0; i < n; i++)
                if (ArrayValues.Count <= i)
                {
                    ArrayValues.Add(Cwords.Zero);
                    ArraySet.Add(false);
                }
            IsArrayEmpty = false;
        }
        public void NullArray()
        {
            var n = ArrayLength();
            for (var i = 0; i < n; i++)
                if (ArrayValues.Count <= i)
                {
                    ArrayValues.Add(Cwords.Zero);
                    ArraySet.Add(false);
                }
            IsArrayEmpty = true;
        }

        public Variable Clone()
        {
            var n = new Variable
            {
                Name = Name,
                Type = {Type = Type.Type, Length = Type.Length, RootType = Type.RootType, Level = Type.Level},
                IsArray = IsArray,
                Value = "000000000",
                ValueSet = false,
                IsTemp = IsTemp,
                IsConst = IsConst
            };
            if (Array != null)
            {
                foreach (var i in Array)
                {
                    n.Array.Add(i);
                }
            }
            if (ArraySet != null)
            {
                foreach (var i in ArraySet)
                {
                    n.ArraySet.Add(i);
                }
            }
            if (ArrayValues != null)
            {
                foreach (var i in ArrayValues)
                {
                    n.ArrayValues.Add(i);
                }
            }
            return n;
        }
        public Variable CloneUp()
        {
            var c = Clone();
            c.Type.UpType();
            return c;
        }
        public Variable CloneDown()
        {
            var c=Clone();
            c.Type.DownType();
            return c;
        }
        public void ConvertValue(string s)
        {
            ValueSet = true;
            Value = s;
            //if(s.IsReal())
            //    Value = String.Format("{0:X9}", BitConverter.GetBytes(float.Parse(s)).ToString());
            //else
            //    Value = String.Format("{0:X9}", s);
        }
        public void ConvertValue(int s)
        {
            ValueSet = true;
            switch (Type.Type)
            {
                case ("int"):
                case ("long int"):
                case ("_BOOL"):
                case ("short"):
                case ("signed char"):
                case ("unsigned int"):
                case ("unsigned short"):
                case ("char"):
                case ("unsigned char"):
                    Value = String.Format("{0:X9}", s);
                    break;
                case ("float"):
                case ("double"):
                case ("long double"):
                case ("long long int"):
                case ("unsigned long long int"):
                default:
                    Error.PrintError("Using: " + Type.Type,1,false);
                    break;
            }
        }
        public void FillString(string s)
        {
            IsArray = true;
            int n;
            var i = 0;
            s = System.Text.RegularExpressions.Regex.Unescape(s);
            foreach (var c in s)
            {
                n = c;
                ArrayValues[i]=n.ToHex9();
                ArraySet[i] = true;
                i++;
            }
            n = 0;
            ArrayValues[i]=n.ToHex9();
            ArraySet[i] = true;
            ArrayString = s;
        }
    }
}
