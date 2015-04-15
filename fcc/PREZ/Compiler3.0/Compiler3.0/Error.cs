using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Compiler1._0
{
    static class Error
    {
        static public void Stop(string c)
        {
            MessageBox.Show(c, "Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
            Environment.Exit(1);
        }
    }
    static class Processor
    {
        static public int NoOfRegisters = 7;
        static public int MemoryAvailable = 0x1FFF;
        static public string[] RegisterNames;
        static Processor()
        {
            RegisterNames=new string[NoOfRegisters];
            for (int i = 0; i < NoOfRegisters; i++)
                RegisterNames[i] = "R" + i;
        }
        static public string FunctResult = "%RESULT%";
        static public bool IsRegName(this String s)
        {
            return RegisterNames.Contains(s);
        }
    }
    static class Cwords
    {
        static public string[] unwanted = {
                                        //"sizetype",
                                        "FIQ",
                                        "IRQ",
                                        //"MEM[(",
                                        "bittype",
                                        "temp.",
                                        "const.",
                                        "R0",
                                        "R1",
                                        "R2",
                                        "R3",
                                        "R4",
                                        "R5",
                                        "R6",
                                    };
        static public string Float = "float";
        static public string Pointer = "unsigned int";
        static public string Bool = "_Bool";
        static public string Int = "int";
        static public string IntU = "unsigned int";
        static public string Short = "short int";
        static public string ShortU = "short unsigned int";
        static public string Char = "char";
        static public string CharU = "unsigned char";
        static public string CharS = "signed char";
        static public string Long = "long int";
        static public string LongU = "long unsigned int";
        static public string Sizetype = "sizetype";
        static public string[] Types = {
                                         Float,
                                         Pointer,
                                         Bool,
                                         Int,
                                         IntU,
                                         Short,
                                         ShortU,
                                         Char,
                                         CharU,
                                         CharS,
                                         Long,
                                         LongU,
                                         Sizetype,
                                     };
        static private string[] Unsigned = {
                                              Pointer,
                                              IntU,
                                              ShortU,
                                              CharU,
                                              LongU,
                                              Sizetype,
                                          };
        static private string[] Signed = {
                                            Int,
                                            Short,
                                            Char,
                                            Long,
                                            CharS,
                                            Bool,
                                        };
        static private string[] Real ={
                                         Float,
                                     };
        static private string[] Byte1 ={
                                         Char,
                                         CharU,
                                         CharS,
                                     };
        static private string[] Byte2 ={
                                         Short,
                                         ShortU,
                                     };
        static private string[] Byte4 ={
                                         Int,
                                         Long,
                                         IntU,
                                         LongU,
                                         Float,
                                     };

        static private string Addition = "Addition";
        static private string Subtraction = "Subtraction";
        static private string Multiply = "Multiply";
        static private string Divide = "Divide";
        static private string Modulo = "Modulo";

        static public string Zero = "000000000";
         
        static public bool IsUnsigned(this String s)
        {
            if(Unsigned.Contains(s))
                return true;
            return false;
        }
        static public bool IsSigned(this String s)
        {
            if (Signed.Contains(s))
                return true;
            return false;
        }
        static public bool StartsWithType(this String s)
        {
            s = s.TrimStart();
            foreach (string t in Types)
            {
                if (s.StartsWith(t))
                    return true;
            }
            return false;
        }
        static public bool IsReal(this String s)
        {
            if(Real.Contains(s))
                return true;
            return false;
        }
        static public bool IsFloat(this String s)
        {
            if (Float.Equals(s))
                return true;
            return false;
        }
        static public string OperandName(this String s)
        {
            switch (s)
            {
                case "+":
                    return Cwords.Addition;
                case "-":
                    return Cwords.Subtraction;
                case "*":
                    return Cwords.Multiply;
                case "/":
                    return Cwords.Divide;
                case "%":
                    return Cwords.Modulo;
                default:
                    return s;
            }
        }
        static public int GetLength(this String s)
        {
            if (Byte4.Contains(s))
            {
                return 4;
            }
            else if (Byte1.Contains(s))
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }
    }
}
