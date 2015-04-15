using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace fcc
{
    static class Error
    {

        static public void PrintError(string e,int exitCode,bool printHelp)
        {
            if (printHelp)
            {
                Console.WriteLine("- ERROR: " + e);
                PrintInfo();
            }
            else
                Console.WriteLine("- ERROR: " + e+ "\nUse --help for more info.\n");
            if(exitCode>=0)
                Environment.Exit(exitCode);
        }
        static public void PrintError(string e)
        {
                Console.WriteLine("- ERROR: " + e + "\nUse --help for more info.\n");
                Environment.Exit(1);
        }
        static public void PrintWarning(string w)
        {
            Console.WriteLine("- WARNING: " + w);
        }
        static public void PrintHelp()
        {
            PrintInfo();
            try
            {
                Console.WriteLine(File.ReadAllText(AppData.HelpPath)+"\n");
            }
            catch
            {
                PrintError("Cannot locate " + AppData.HelpPath + ".",1,false); 
            }
            Console.WriteLine("For bug reporting contact " + AppData.Author + " at " + AppData.AuthorContact+".\n");
        }
        static public void PrintToDo()
        {
            PrintInfo();
            try
            {
                Console.WriteLine(File.ReadAllText(AppData.ToDoPath));
            }
            catch
            {
                PrintError("Cannot locate " + AppData.ToDoPath + ".", 1, false);
            }
            Console.WriteLine("For bug reporting contact " + AppData.Author + " at " + AppData.AuthorContact + ".\n");
        }
        static public void PrintInfo()
        {
            Console.WriteLine(AppData.Name + " " + AppData.Version + " " + AppData.Date + " " + AppData.Author);
            Console.WriteLine("Use --help for more info.\nUsage: " + AppData.Filename + " [options] file\n");
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
            for (var i = 0; i < NoOfRegisters; i++)
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
        static public readonly string[] Unwanted = {
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
        static public readonly string Float = "float";
        static public readonly string Pointer = "unsigned int";
        static public readonly string Bool = "_Bool";
        static public readonly string Int = "int";
        static public readonly string IntU = "unsigned int";
        static public readonly string Short = "short int";
        static public readonly string ShortU = "short unsigned int";
        static public readonly string Char = "char";
        static public readonly string CharU = "unsigned char";
        static public readonly string CharS = "signed char";
        static public readonly string Long = "long int";
        static public readonly string LongU = "long unsigned int";
        static public readonly string Sizetype = "sizetype";
        static public readonly string[] Types = {
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
                                         Sizetype
                                     };
        static private readonly string[] Unsigned = {
                                              Pointer,
                                              IntU,
                                              ShortU,
                                              CharU,
                                              LongU,
                                              Sizetype
                                          };
        static private readonly string[] Signed = {
                                            Int,
                                            Short,
                                            Char,
                                            Long,
                                            CharS,
                                            Bool
                                        };
        static private readonly string[] Real ={
                                         Float
                                     };
        static private readonly string[] Byte1 ={
                                         Char,
                                         CharU,
                                         CharS,
                                     };
        static private readonly string[] Byte4 ={
                                         Int,
                                         Long,
                                         IntU,
                                         LongU,
                                         Float
                                     };

        private const string Addition = "Addition";
        private const string Subtraction = "Subtraction";
        private const string Multiply = "Multiply";
        private const string Divide = "Divide";
        private const string Modulo = "Modulo";

        static public string Zero = "000000000";
         
        static public bool IsUnsigned(this String s)
        {
            return Unsigned.Contains(s);
        }

        static public bool IsSigned(this String s)
        {
            return Signed.Contains(s);
        }

        static public bool StartsWithType(this String s)
        {
            s = s.TrimStart();
            foreach (var t in Types)
            {
                if (s.StartsWith(t))
                    return true;
            }
            return false;
        }
        static public bool IsReal(this String s)
        {
            return Real.Contains(s);
        }

        static public bool IsFloat(this String s)
        {
            return Float.Equals(s);
        }

        static public string OperandName(this String s)
        {
            switch (s)
            {
                case "+":
                    return Addition;
                case "-":
                    return Subtraction;
                case "*":
                    return Multiply;
                case "/":
                    return Divide;
                case "%":
                    return Modulo;
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
            if (Byte1.Contains(s))
            {
                return 1;
            }
            return 2;
        }
    }
}
