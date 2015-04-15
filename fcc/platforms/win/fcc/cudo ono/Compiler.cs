using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace fcc
{
    class Compiler
    {
        private const bool IspraviFRISC3 = true;

        private const string Boot = "\t\tMOVE 1000,SP\n\t\tJR _start\n;\t\tDW IRQ_start\n;\t\tJR FIQ_start\n_start\n\t\tSUB SP,4,SP\n\t\tCALL main\n\t\tADD SP,4,SP\n\t\tHALT\n";

        #region Variables

            public string InputFile = null;

            private static readonly List<string> EmbeddedFunctionList = new List<string>();

            private static List<Function> _functionList = new List<Function>();

            public static List<string> Headers = new List<string>();

            private static bool _started;

            private static int _regUsed = -1;



            private static List<string> _embeddedFunctionList = new List<string>();

            private static readonly List<string> AdditionalFunctionList = new List<string>();

            private static readonly List<Variable> GlobalVariableDeclaredList = new List<Variable>();

        #endregion

        #region Print

            private void Print(List<Command> list)
            {
                Console.WriteLine();
                Console.WriteLine("Sporedni ispis:");
                Console.WriteLine();
                foreach (Command c in list)
                {
                    Print(c);

                 //   if (i++ % 100 == 0)
                 //       Console.ReadKey(true);
                }
                Console.WriteLine();
            }

            public void PrintBlank(List<Command> list)
            {
     

                Console.WriteLine();
                Console.WriteLine("Glavni ispis:");
                Console.WriteLine();
                foreach (Command c in list)
                {
                    PrintBlank(c);
                }
            }

            public void PrintToStream(List<Command> list)
            {
                StreamWriter stream = new StreamWriter("mate.a");
                foreach (Command c in list)
                {
                    PrintToStream(c,stream);
                }
                stream.Close();
            }

            private void Print(Command c)
            {
                if (c.Instruction == null)
                {
                    Console.BackgroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine("-------------------------------------------------------------------------------");
                    Console.BackgroundColor = ConsoleColor.Black;
                }
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(String.Format("{0,-16}", (c.Label == null) ? "" : c.Label));
                Console.Write(String.Format("{0,-16}", c.Instruction));
                for (int i = 0; i < c.Arguments.Count<string>(); i++)
                {
                    if (c.Arguments[i] == null)
                        break;
                    if (c.Change[i] && c.Touch)//&& !char.IsDigit(c.Arguments[i][0]))
                    {
                        if (c.LoadStore[i])
                        {
                            Console.BackgroundColor = ConsoleColor.DarkCyan;
                            Console.ForegroundColor = ConsoleColor.Green;
                        }
                        else
                        {
                            Console.BackgroundColor = ConsoleColor.DarkMagenta;
                            Console.ForegroundColor = ConsoleColor.Red;
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.BackgroundColor = ConsoleColor.Black;
                    }
                    string s;
                    if ((c.Instruction.Contains("STORE") || c.Instruction.Contains("LOAD")) && i == 1)
                        s = "( " + c.Arguments[i] + c.MemShift + " )";
                    else if (c.Instruction.Contains("DB") && i == 2)
                        s = c.Arguments[i] + ", " + c.MemShift;
                    else
                        s = c.Arguments[i];
                    Console.Write(s);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.BackgroundColor = ConsoleColor.Black;
                    if (i < 2 && c.Arguments[i + 1] != null)
                        Console.Write(", ");
                }
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.BackgroundColor = ConsoleColor.Black;
            }

            private void PrintBlank(Command c)
            {
                if (c.Instruction == null)//|| c.Instruction.Contains(':') || c.Instruction.Contains(';'))
                    return;
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(String.Format("{0,-16}", (c.Label == null) ? "" : c.Label.Replace(".","")));
                Console.Write(String.Format("{0,-16}", c.Instruction));
                if (c.Instruction.Equals("DB")||c.Instruction.Equals("DW")||c.Instruction.Equals("`DS")||c.Instruction.Equals("DH"))
                {
                    if (c.Arguments[0] == null)
                    {
                        Console.WriteLine();
                        return;
                    }
                    Console.Write(c.Arguments[0]);
                    if (c.Arguments[1] == null)
                    {
                        Console.WriteLine();
                        return;
                    }
                    Console.Write(", " + c.Arguments[1]);
                    if (c.Arguments[2] == null)
                    {
                        Console.WriteLine();
                        return;
                    }
                    Console.Write(", " + c.Arguments[2]);
                    if (c.MemShift == null)
                    {
                        Console.WriteLine();
                        return;
                    }
                    Console.WriteLine(", " + c.MemShift);
                    return;
                }
                for (int i = 0; i < c.Arguments.Count<string>(); i++)
                {
                    if (c.Arguments[i] == null)
                        break;
                    if (c.Change[i] && c.Touch)//&& !char.IsDigit(c.Arguments[i][0]))
                    {
                        if (c.LoadStore[i])
                        {
                            Console.BackgroundColor = ConsoleColor.DarkCyan;
                            Console.ForegroundColor = ConsoleColor.Green;
                        }
                        else
                        {
                            Console.BackgroundColor = ConsoleColor.DarkMagenta;
                            Console.ForegroundColor = ConsoleColor.Red;
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.BackgroundColor = ConsoleColor.Black;
                    }
                    string s;
                    if ((c.Instruction.Contains("STORE") || c.Instruction.Contains("LOAD")) && i == 1)
                        s = "( " + c.Arguments[i] + c.MemShift + " )";
                    else
                        s = c.Arguments[i];
                    Console.Write(s);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.BackgroundColor = ConsoleColor.Black;
                    if (i < 2 && c.Arguments[i + 1] != null)
                        Console.Write(", ");
                }
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.BackgroundColor = ConsoleColor.Black;
            }

            private void PrintToStream(Command c,StreamWriter stream)
            {
                if (c.Instruction == null)//|| c.Instruction.Contains(':') || c.Instruction.Contains(';'))
                    return;
                stream.Write((c.Label == null) ? "\t\t" : c.Label.Replace(".","")+" ");
                stream.Write(c.Instruction+" ");
                if (c.Instruction.Equals("DB") || c.Instruction.Equals("DW") || c.Instruction.Equals("`DS") || c.Instruction.Equals("DH"))
                {
                    if (c.Arguments[0] == null)
                    {
                        stream.Write("\n");
                        return;
                    }
                    stream.Write(c.Arguments[0]);
                    if (c.Arguments[1] == null)
                    {
                        stream.Write("\n");
                        return;
                    }
                    stream.Write(", " + c.Arguments[1]);
                    if (c.Arguments[2] == null)
                    {
                        stream.Write("\n");
                        return;
                    }
                    stream.Write(", " + c.Arguments[2]);
                    if (c.MemShift == null)
                    {
                        stream.Write("\n");
                        return;
                    }
                    stream.WriteLine(", " + c.MemShift);
                    return;
                }
                for (int i = 0; i < c.Arguments.Count<string>(); i++)
                {
                    if (c.Arguments[i] == null)
                        break;

                    string s;
                    if ((c.Instruction.Contains("STORE") || c.Instruction.Contains("LOAD")) && i == 1)
                        s = "( " + c.Arguments[i].Replace(".","") + c.MemShift + " )";
                    else
                        s = c.Arguments[i].Replace(".","");
                    stream.Write(s);

                    if (i < 2 && c.Arguments[i + 1] != null)
                        stream.Write(", ");
                }
                stream.Write("\n");

            }

            private void Print(List<Function> list)
            {
                Console.WriteLine();
                Console.WriteLine("Ispis funkcija i broja parametara:");
                Console.WriteLine();
                foreach (Function c in list)
                {
                    if (c.Name != null)
                        Console.WriteLine(c.Name + " " + c.NoOfParameters.ToString());
                }
                Console.WriteLine();
            }

            private void Print(List<Variable> list)
            {
                Console.WriteLine();
                Console.WriteLine("Ispis varijabli:");
                Console.BackgroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("{0,-12}|{1,-16}|{2,-10}|{3,1}|{4,5}|{5,5}|{6,5}|{7}", "Name", "Type", "Deref Type", "B", "Array", "Temp", "Const", "Value    ");
                Console.BackgroundColor = ConsoleColor.Black;
                foreach (Variable c in list)
                {
                    Print(c);
                }
                Console.WriteLine();
            }

            private void Print(List<Register> list)
            {
                if (list == null)
                    return;
                Console.WriteLine("{0,14}|{1,5}|{2,5}|{3,5}|{4,5}", "Name:", "Load!", "Save!", "Save", "Edit");
                for (int i = 0; i < 46; i++)
                    Console.Write("-");
                Console.WriteLine();
                foreach (Register c in list)
                {
                    if (c.VarName != null)
                        Console.WriteLine("{0,14}|{1,5}|{2,5}|{3,5}|{4,5}|{5,1}", c.VarName, c.LoadNow, c.SaveNow, c.Save, c.Edited, c.RegName);
                }
                for (int i = 0; i < 46; i++)
                    Console.Write("-");
                Console.WriteLine();
                for (int i = 0; i < 46; i++)
                    Console.Write("-");
                Console.WriteLine();
            }

            private void Print(Variable v)
            {
                Console.Write("{0,-12}|{1,-16}|{2,-10}|{3,1}|{4,5}|{5,5}|{6,5}|{7}", v.Name, v.Type.Type, v.Type.RootType, v.Type.Length, v.IsArray, v.IsTemp, v.IsConst, v.Value);
                Console.WriteLine();
            }

            private void PrintAdditionalFuncts(List<string> list)
            {
                if (list.Count <= 0)
                    return;
                foreach (string s in list)
                {
                    Console.WriteLine(s);
                }
            }

            private string ConvertToString(List<Command> list)
            {
                string output = "";
                foreach (Command c in list)
                {
                    output += ConvertToString(c);
                }
                return output;
            }

            private string ConvertToString(Command c)
            {
                string output = "";
                if (c.Instruction == null)//|| c.Instruction.Contains(':') || c.Instruction.Contains(';'))
                    return output;
                output+=(c.Label == null) ? "\t\t" : c.Label.Replace(".", "") + " ";
                output+=c.Instruction + " ";
                if (c.Instruction.Equals("DB") || c.Instruction.Equals("DW") || c.Instruction.Equals("DS") || c.Instruction.Equals("DH"))
                {
                    if (c.Arguments[0] == null)
                    {
                        output+="\n";
                        return output;
                    }
                    output+=c.Arguments[0];
                    if (c.Arguments[1] == null)
                    {
                        output+="\n";
                        return output;
                    }
                    output+=", " + c.Arguments[1];
                    if (c.Arguments[2] == null)
                    {
                        output+="\n";
                        return output;
                    }
                    output+=", " + c.Arguments[2];
                    if (c.MemShift == null)
                    {
                        output+="\n";
                        return output;
                    }
                    output+=", " + c.MemShift+"\n";
                    return output;
                }
                for (int i = 0; i < c.Arguments.Count<string>(); i++)
                {
                    if (c.Arguments[i] == null)
                        break;

                    string s;
                    if ((c.Instruction.Contains("STORE") || c.Instruction.Contains("LOAD")) && i == 1)
                        s = "( " + c.Arguments[i].Replace(".", "") + c.MemShift + " )";
                    else
                        s = c.Arguments[i].Replace(".", "");
                    output+=s;

                    if (i < 2 && c.Arguments[i + 1] != null)
                        output+=", ";
                }
                output+="\n";
                return output;
            }

        #endregion 

        public Compiler(string sourcePath,string destinationPath,string globalsPath,List<string> head)
        {
            InputFile = globalsPath;

            Headers = head;

            GlobalParse();

            InputFile = sourcePath;

            AnalyzeSource(sourcePath);
            
            InitializeParser();          

            List<Command> parserOutputRaw = new List<Command>(Parse());

            AddGlobalVarsAndConsts(GlobalVariableDeclaredList, parserOutputRaw);

            //PrintBlank(parserOutputRaw);

            string parserOutput = ConvertToString(parserOutputRaw);

            //PrintAdditionalFuncts(AdditionalFunctions);

            parserOutput = AddAdditionalFunctions(parserOutput);

            File.WriteAllText(destinationPath, parserOutput);
        }

        private void GlobalParse()
        {
            ParseFunctionPrototypes();

            Function f = _functionList[0];
            
            PreParsing(GlobalVariableDeclaredList, f.Name);

            List<Command> Commands = new List<Command>();

            int i=GlobalVariableDeclaredList.Count;

            using (StreamReader FunctionStream = FindFunction(f))
            {
                while (ParseDeclaration(FunctionStream, GlobalVariableDeclaredList)) ;

                _started = false;

                while (ParseCommands(FunctionStream, GlobalVariableDeclaredList, Commands)) ;
            }

            if (i < GlobalVariableDeclaredList.Count)
                GlobalVariableDeclaredList.RemoveAt(i);
            Variable v;
            i = 0;
            while (i < GlobalVariableDeclaredList.Count)
            {
                for (; i < GlobalVariableDeclaredList.Count; i++)
                {
                    if (GlobalVariableDeclaredList[i].Value.IsNumber())
                        continue;
                    if ((v = FindVariable(GlobalVariableDeclaredList, GlobalVariableDeclaredList[i].Value)) == null)
                        continue;
                    v.Name = GlobalVariableDeclaredList[i].Name;
                    GlobalVariableDeclaredList.RemoveAt(i);
                    i--;
                    break;
                }
            }

           // GlobalVariablesDeclared.RemoveAt(0);

            for (int j = 0; j < GlobalVariableDeclaredList.Count; j++)
                GlobalVariableDeclaredList[j].IsGlobal = true;
        }

        private void AnalyzeSource(string sourcePath)
        {
            string s = File.ReadAllText(sourcePath);
            foreach (string c in Cwords.Unwanted)
            {
                if(System.Text.RegularExpressions.Regex.IsMatch(s,string.Format(@"\b{0}\b",System.Text.RegularExpressions.Regex.Escape(c))))
                    Error.PrintError("Using: "+c,1,false);
            }
            int idx1=-1,idx2;
            while ((idx1 = s.IndexOf('{', idx1 + 3)) >= 0)
            {
                idx2 = s.IndexOf('}', idx1);
                if (!s.Substring(idx1, idx2 - idx1 + 1).Contains(Environment.NewLine + Environment.NewLine))
                    s=s.Insert(idx1 + 1, Environment.NewLine);
            }
            s=s.Replace(Cwords.Sizetype, Cwords.Pointer);
            File.WriteAllText(sourcePath, s);
        }

        private void InitializeParser()
        {
            //_embeddedFunctionList= new List<string>();
           // _embeddedFunctionList.AddRange(Directory.GetFiles(ProgramStats.DefaultBuiltInFunctionsFolder, "*.a"));
        }

        private List<Command> Parse()
        {
            ParseFunctionPrototypes();

            List<Command> functSum = new List<Command>();
            foreach (Function f in _functionList)
            {
                functSum.AddRange(ParseFunction(f));
            }

            return functSum;
        }

        #region Parse Functions

            private void ParseFunctionPrototypes()
            {
                _functionList = new List<Function>();
                using (StreamReader read = new StreamReader(InputFile))
                {
                    string prototype;
                    while (!read.EndOfStream)
                    {
                        prototype = read.ReadLine();
                        if (prototype == null)
                            break;
                        if (prototype.Length == 0 || !char.IsLetter(prototype[0]))
                            continue;
                        Function f = new Function();
                        f.ReadData(read, prototype);
                        _functionList.Add(f);
                    }
                }

            }

            #region Parsing Functions

                private List<Command> ParseFunction(Function f)
                {
                    List<Variable> VariablesDeclared = new List<Variable>();
                    VariablesDeclared.AddRange(GlobalVariableDeclaredList);
                    Variable p;
                    foreach(Variable v in f.Parameters)
                    {
                        if ((p = VariableNameExists(v.Name, VariablesDeclared)) != null)
                            VariablesDeclared.Remove(p);
                        VariablesDeclared.Add(v);
                    }
                    List<Command> Commands = new List<Command>();

                    PreParsing(VariablesDeclared, f.Name);
                    using (StreamReader FunctionStream = FindFunction(f))
                    {
                        while (ParseDeclaration(FunctionStream, VariablesDeclared)) ;

                        _started = false;

                        while (ParseCommands(FunctionStream, VariablesDeclared, Commands)) ;
                    }
                    //Print(Commands);
                    //Print(VariablesDeclared);
                   //ReduceJumps(Commands);
                    //Console.ReadKey();
                    _regUsed = -1; 
                    ConvertToRegisters(Commands, VariablesDeclared);
                    //PrintBlank(Commands);
                    ConvertParameters(Commands,f,VariablesDeclared);
                    //PrintBlank(Commands);
                    //Console.WriteLine(regUsed);
                    FillWithPushPulls(Commands);

                    FinishParsing(Commands, f.Name);
                    //PrintBlank(Commands);
                    //Print(VariablesDeclared);

                    return Commands;
                }

                #region Parsing Commands

                    private StreamReader FindFunction(Function f)
                    {
                        StreamReader Finder = new StreamReader(InputFile);
                        while (!Finder.ReadLine().StartsWith(f.Name)) ;
                        Finder.ReadLine();
                        return Finder;
                    }

                    private void PreParsing(List<Variable> list, string name)
                    {
                        int idxS;
                        int idxE;
                        Variable v = new Variable();
                        string str, buffer;

                        //get file
                        string[] buffers = null;
                        try
                        {
                            buffers = File.ReadAllLines(InputFile);
                        }
                        catch (Exception)
                        {
                            Error.PrintError("IO problem occured. Try again.");
                        }

                        int i = 0;
                        while (!buffers[i].StartsWith(name))
                            i++;
                        i += 2;
                        for (; ; i++)
                        {
                            if (buffers[i].Equals("}"))
                                break;
                            buffer = buffers[i];
                            idxE = -1;
                            while ((idxS = buffer.IndexOf('\"', idxE + 1)) >= 0)
                            {
                                idxE = buffer.IndexOf('"', idxS + 1);
                                while (buffer[idxE - 1].Equals('\\') && !buffer[idxE - 1].Equals('\\'))
                                    idxE = buffer.IndexOf('"', idxE + 1);
                                str = buffer.Substring(idxS + 1, idxE - idxS - 1);
                                v = new Variable();
                                v.Name = GetConstName();
                                v.IsArray = true;
                                v.Type.Set("char *");
                                v.Array.Add(str.Length + 1);
                                v.NonNullArray();
                                v.FillString(str);
                                v.IsConst = true;
                                buffers[i] = buffers[i].Replace("\"" + str + "\"", v.Name);
                                list.Add(v);
                            }
                        }
                        File.WriteAllLines(InputFile, buffers);


                    }

                    private bool ParseDeclaration(StreamReader Read, List<Variable> List)
                    {
                        string s = Read.ReadLine();
                        if (s.Length <= 1)
                            return false;
                        if (s.FirstWord().Equals("extern"))
                        {
                            return true;
                        }
                        if (s.FirstWord().TrimEnd(';').Equals("return"))
                            return false;
                        if (!s.StartsWithType())
                        {
                            return false;
                        }
                        int idx = s.LastIndexOf(' ');
                        string name = s.Remove(0, idx + 1);
                        name = name.TrimEnd(';');
                        Variable v = new Variable();
                        if ((v = VariableNameExists(name, List)) != null)
                            List.Remove(v);
                        v = new Variable();
                        string type = s.Remove(idx);
                        type = type.TrimStart(' ');
                        v.SetVariable(name, type);
                        List.Add(v);
                        return true;
                    }

                    private bool ParseCommands(StreamReader read, List<Variable> list, List<Command> line)
                    {
                        string s = read.ReadLine();
                        if (s.Length <= 1)
                            return false;
                        
                        s = s.TrimStart();
                        string determinator = s.FirstWord().TrimEnd(';');
                        switch (determinator)
                        {
                            case "goto":
                                _started = true;
                                ParseCommandGoto(s, line);
                                break;
                            case "if":
                                _started = true;
                                ParseCommandIf(s, list, line);
                                break;
                            case "return":
                                _started = true;
                                ParseCommandReturn(FindVariable(list,s.LastWord().TrimEnd(';')),line);
                                break;
                            case "switch":
                                _started = true;
                                ParseCommandSwitch();
                                break;
                            default: //varijabla,funkcija,labela
                                if (determinator[0] == '*')
                                {
                                    _started = true;
                                    determinator = determinator.Remove(0, 1);
                                    ParseCommandDereferenced(FindVariable(list, determinator), s, list, line);
                                    break;
                                }
                                Variable v = new Variable();
                                if ((v = FindVariable(list, determinator)) != null || determinator.Contains('['))
                                    ParseCommandVariable(v, s, list, line);
                                else if (FindFunciton(_functionList, determinator) != null)
                                    ParseCommandFunction(s, list, line, null);
                                else
                                    ParseCommandLabel(s, list, line);
                                break;
                        }
                        return true;
                    }
        
                    #region Parsing Commands Functions

                        private void ParseCommandGoto(string buffer, List<Command> line)
                        {
                            Command c = new Command();
                            //c = SetLabel(c);
                            c.Instruction = "JR";
                            c.Touch = false;
                            c.Arguments[0] = buffer.Substring(buffer.LastIndexOf(' ') + 1).Trim('<', '>', ';');
                            line.Add(c);
                        }

                        private void ParseCommandIf(string buffer, List<Variable> list, List<Command> line)
                        {
                            Command c = new Command();
                            //c = SetLabel(c);
                            string op1 = buffer.Word(2).TrimStart('(');
                            string op2 = buffer.Word(4).TrimEnd(')');
                            string jpt = buffer.Word(6).Trim(';', '<', '>');
                            string jpf = buffer.Word(9).Trim(';', '<', '>');
                            string adt = null;
                            string adf = null;
                            string ads = null;

                            Variable Op1 = FindVariable(list, op1);
                            op2 = CreateConst(op2, list, Op1);
                            if (!char.IsLetter(op2[0]))
                                c.Change[1] = false;
                            if (IspraviFRISC3 || Op1.Type.IsSigned())
                                ads = "S";
                            else
                            {
                                ads = "U";
                            }

                            switch (buffer.Word(3))
                            {
                                case ("=="):
                                    adt = "EQ";
                                    adf = "NE";
                                    ads = "";
                                    break;
                                case ("!="):
                                    adt = "NE";
                                    adf = "EQ";
                                    ads = "";
                                    break;
                                case ("<"):
                                    adt = "LT";
                                    adf = "GE";
                                    break;
                                case (">"):
                                    adt = "GT";
                                    adf = "LE";
                                    break;
                                case ("<="):
                                    adt = "LE";
                                    adf = "GT";
                                    break;
                                case (">="):
                                    adt = "GE";
                                    adf = "LT";
                                    break;
                                default:
                                    Error.PrintError(buffer, 1, false);
                                    break;
                            }

                            c.Instruction = "CMP";
                            c.Arguments[0] = op1;
                            c.Arguments[1] = op2;
                            line.Add(c);

                            c = new Command();
                            c.Instruction = "JR_" + ads + adt;
                            c.Arguments[0] = jpt;
                            c.Touch = false;
                            line.Add(c);

                            c = new Command();
                            c.Instruction = "JR_" + ads + adf;
                            c.Arguments[0] = jpf;
                            c.Touch = false;
                            line.Add(c);
                        }

                        private void ParseCommandReturn(Variable var, List<Command> line)
                        {
                            Command c = new Command();
                            //c = SetLabel(c);
                            if (var != null)
                            {
                                c.Instruction = "STORE";
                                c.Arguments[0] = var.Name;
                                c.Arguments[1] = Processor.FunctResult;
                                c.Change[1] = false;
                                line.Add(c);
                            }

                            c = new Command();
                            c.Touch = false;
                            c.Instruction = "RET";
                            line.Add(c);
                        }

                        private void ParseCommandSwitch()
                        {
                            Error.PrintError("switch", 1, false);
                        }

                        private void ParseCommandDereferenced(Variable dest, string buffer, List<Variable> list, List<Command> line)
                        {
                            string op2 = EditOp2(dest, buffer.Word(3).Trim(';'), list, line);
                            string memExt = GetMemExtension(dest.Type.GetRootLength());
                            Command c = new Command();
                            //c = SetLabel(c);
                            c.Instruction = "STORE"+memExt;
                            c.Arguments[0] = op2;
                            c.Arguments[1] = dest.Name;
                           // c.Change[1] = false;
                            line.Add(c);
                        }

                        private void ParseCommandVariable(Variable Dest, string buffer, List<Variable> list, List<Command> line)
                        {
                            #region Variables
                            int n = buffer.WordCount();
                            string op1 = buffer.Word(3);
                            string operand = buffer.Word(4);
                            string op2 = buffer.Word(5);
                            string temp1 = null;
                            string temp2 = null;
                            string memExt = null;
                            #endregion

                            #region a[i] = b;
                            if (Dest == null)
                            {
                                ParseCommandArray(buffer, list, line);
                                return;
                            }
                            #endregion

                            #region Classes
                            Command c = new Command();
                            Variable v = new Variable();
                            Variable Op1;

                            #endregion

                            #region a = (cast) b;
                            if (op1[0] == '(')
                            {
                                memExt = GetMemExtension(Dest.Type.Length);
                                _started = true;

                                op1 = buffer.LastWord().TrimEnd(';');
                                Op1 = FindVariable(list, op1);

                                #region UsingFloat
                                if (Op1.Type.Type == "float" || Dest.Type.Type == "float")
                                {
                                    RefreshValue(Op1, line);

                                    if (Op1.Type.Type == "float")
                                    {
                                        operand = "FloatTo";
                                    }
                                    else
                                    {
                                        if (Op1.Type.Type.IsSigned())
                                        {
                                            operand = "SignedTo";
                                        }
                                        else
                                        {
                                            operand = "UnsignedTo";
                                        }
                                    }
                                    c.Instruction = "PUSH";
                                    c.Arguments[0] = Op1.Name;
                                    //c = SetLabel(c);
                                    line.Add(c);

                                    c = new Command();
                                    c.Instruction = "CALL";
                                    c.Touch = false;
                                    c.Arguments[0] = GetEmbeddedFunctionName(operand, Dest.Type.Type);
                                    if (!EmbeddedFunctionList.Contains(c.Arguments[0]))
                                        EmbeddedFunctionList.Add(c.Arguments[0]);
                                    line.Add(c);

                                    c = new Command();
                                    c.Instruction = "POP";
                                    c.Arguments[0] = Dest.Name;
                                    c.LoadStore[0] = false;
                                    line.Add(c);

                                    RefreshValue(Dest, line);
                                }
                                #endregion
                                else
                                {

                                    RefreshValue(FindVariable(list, op1), line);
                                    c.Instruction = "STORE" + memExt;
                                    op1 = EditOp2(Dest, buffer, list, line);
                                    //c = SetLabel(c);
                                    c.Arguments[0] = Op1.Name;
                                    c.Arguments[1] = Dest.Name;
                                    c.Change[1] = false;
                                    line.Add(c);
                                }
                                return;
                            }
                            #endregion

                            #region s = "string";

                            if (buffer.Contains('"'))
                            {
                                op1 = buffer.Substring(buffer.IndexOf('"'), buffer.LastIndexOf('"') + 1 - buffer.IndexOf('"'));
                                op1 = op1.Substring(1, op1.Length - 2);
                                if (!_started)
                                {
                                    list.Remove(FindString(list, op1));
                                    Dest.FillString(op1);
                                }
                                else
                                {
                                    c.Instruction = "PUSH";
                                    c.Arguments[0] = Dest.Name;
                                    //c = SetLabel(c);
                                    line.Add(c);

                                    c = new Command();
                                    c.Instruction = "PUSH";
                                    Op1 = FindString(list, op1);
                                    c.Arguments[0] = Op1.Name;
                                    line.Add(c);

                                    c = new Command();
                                    c.Instruction = "CALL";
                                    c.Touch = false;
                                    c.Arguments[0] = GetEmbeddedFunctionName("String", Dest.Type.Type);
                                    if (!EmbeddedFunctionList.Contains(c.Arguments[0]))
                                        EmbeddedFunctionList.Add(c.Arguments[0]);
                                    line.Add(c);

                                    c = new Command();
                                    c.Instruction = "ADD";
                                    c.Arguments[0] = c.Arguments[2] = "SP";
                                    n = 8;
                                    c.Arguments[1] = n.ToHex9();
                                    c.Touch = false;
                                    line.Add(c);
                                }
                                return;
                            }
                            #endregion

                            #region a = MEM[(cast)b + cB];
                            if (buffer.Contains("MEM[("))
                            {
                                int idx1, idx2;
                                idx1 = buffer.IndexOf(')');
                                idx1++;
                                idx2 = buffer.IndexOf(' ', idx1);
                                op1 = buffer.Substring(idx1, idx2 - idx1);
                                op1=op1.Trim('&');
                                idx1 = buffer.IndexOf("+ ");
                                idx1 += 2;
                                idx2 = buffer.IndexOf('B');
                                temp1 = buffer.Substring(idx1, idx2 - idx1);
                                temp1 = CreateConst(temp1, list, Dest.CloneUp());
                                c = new Command();
                                c.Instruction = "LOAD";
                                c.Arguments[0] = Dest.Name;
                                c.LoadStore[0] = false;
                                c.Arguments[1] = op1;
                                c.MemShift = " + " + temp1;
                                line.Add(c);
                                return;
                            }
                            #endregion

                            switch (n)
                            {
                                case (3):
                                    op1 = op1.TrimEnd(';');
                                    #region a = -b;
                                    if (op1[0] == '-' && (Op1 = FindVariable(list, op1.TrimStart('-'))) != null)
                                    {
                                        n = -1;
                                        temp1=CreateTemp(list, Dest);
                                        c.Instruction = "XOR";
                                        c.Arguments[0] = Op1.Name;
                                        c.Arguments[1] = n.ToHex9();
                                        c.Arguments[2] = temp1;
                                        c.Change[1] = false;
                                        line.Add(c);
                                        c = new Command();
                                        n = 1;
                                        c.Instruction = "ADD";
                                        c.Arguments[0] = temp1;
                                        c.Arguments[1] = n.ToHex9();
                                        c.Arguments[2] = Dest.Name;
                                        c.Change[1] = false;
                                        line.Add(c);
                                        break;
                                    }
                                    #endregion
                                    switch (op1[0])
                                    {
                                        case ('*'):
                                            #region a = *b;

                                            _started = true;
                                            op1 = op1.Remove(0, 1);
                                            v = FindVariable(list, op1);
                                            temp1 = CreateTemp(list, v);
                                            memExt = GetMemExtension(Dest.Type.Length);
                                            c.Instruction = "LOAD" + memExt;
                                            c.Arguments[0] = Dest.Name;
                                            c.LoadStore[0] = false;
                                            c.Arguments[1] = op1;
                                            //c = SetLabel(c);
                                            line.Add(c);
                                            break;

                                            #endregion
                                        case ('&'):
                                            #region a = &b;

                                            _started = true;
                                            op1 = op1.Remove(0, 1);
                                            if (op1.Contains('['))
                                            {
                                                if (op1.Contains("][") || !op1.Contains("[0]"))
                                                {
                                                    Error.PrintWarning("Smatrati cu " + op1 + " kao da pise " + op1.Remove(op1.IndexOf('[')) + " jer su 2 sata i idem spavati");
                                                }
                                                else
                                                {
                                                    op1 = op1.Remove(op1.IndexOf('['));
                                                }
                                            }
                                            v = FindVariable(list, op1);
                                            if (v.Type.Level.Equals(Dest.Type.Level) && v.Type.RootType.Equals(Dest.Type.RootType))
                                                c.Change[1] = false;
                                            else
                                                c.Change[1] = true;
                                            memExt = GetMemExtension(v.Type.Length);
                                            c.Instruction = "STORE" + memExt;
                                            c.Arguments[0] = op1;
                                            c.Arguments[1] = Dest.Name;
                                            //c = SetLabel(c);
                                            line.Add(c);
                                            break;

                                            #endregion
                                        case ('{'):
                                            #region a = null;

                                            //donothing

                                            break;
                                            #endregion
                                        case ('!'):
                                            #region a = !b;

                                            _started = true;
                                            op1 = op1.Remove(0, 1);
                                            c.Instruction = "MOVE";
                                            c.LoadStore[1] = false;
                                            n = 1;
                                            c.Arguments[0] = n.ToHex9();
                                            c.Change[0] = false;
                                            c.Arguments[1] = Dest.Name;
                                            //c = SetLabel(c);
                                            line.Add(c);
                                            c = new Command();
                                            c.Instruction = "CMP";
                                            c.Arguments[0] = op1;
                                            n = 0;
                                            c.Arguments[1] = n.ToHex9();
                                            c.Change[1] = false;
                                            line.Add(c);
                                            c = new Command();
                                            c.Instruction = "DB 04, 00, 80, 0D7; JR_EQ (PC+4)";
                                            c.Touch = false;
                                            line.Add(c);
                                            c = new Command();
                                            c.Instruction = "MOVE";
                                            c.LoadStore[1] = false;
                                            c.Arguments[0] = n.ToHex9();
                                            c.Change[0] = false;
                                            c.Arguments[1] = Dest.Name;
                                            line.Add(c);
                                            break;

                                            #endregion
                                        default:
                                            #region a = b;

                                            if (!op1.Contains('['))
                                            {
                                                if (!_started && op1[0].IsNumberPart())
                                                {
                                                    if (op1.Contains('.'))
                                                    {
                                                        Dest.Value = float.Parse(op1).ToHex9();
                                                        Dest.ValueSet = true;
                                                    }
                                                    else
                                                    {
                                                        Dest.Value = int.Parse(op1).ToHex9();
                                                        Dest.ValueSet = true;
                                                    }
                                                }
                                                else if (!_started&&InputFile.Contains("global"))
                                                {
                                                    Dest.Value = op1;
                                                }
                                                else
                                                {
                                                    c.Instruction = "STORE";
                                                    op1 = EditOp2(Dest, op1, list, line);
                                                    //c = SetLabel(c);
                                                    c.Arguments[0] = op1;
                                                    c.Arguments[1] = Dest.Name;
                                                    c.Change[1] = false;
                                                    line.Add(c);
                                                }
                                            }

                                            #endregion

                                            #region a = b[c];
                                            else
                                            {
                                                string[] splitter = op1.Split('[');
                                                op2 = splitter[0];
                                                string memExtOp2 = GetMemExtension(FindVariable(list, op2).Type.GetRootLength());
                                                string offset = splitter[1].TrimEnd(']');
                                                temp1 = CreateTemp(list, Dest);
                                                memExt = GetMemExtension(Dest.Type.Length);
                                                if (!char.IsLetter(offset[0]))
                                                {
                                                    offset = CreateConstOffset(offset, list, Dest);
                                                    if (offset.Equals("000000000"))
                                                    {
                                                        //NULA
                                                        c.Instruction = "LOAD" + memExtOp2;
                                                        //c = SetLabel(c);
                                                        c.Arguments[0] = temp1;
                                                        c.Arguments[1] = op2;
                                                        line.Add(c);

                                                        c = new Command();
                                                        c.Instruction = "STORE" + memExt;
                                                        c.Arguments[0] = temp1;
                                                        c.Arguments[1] = Dest.Name;
                                                        c.Change[1] = false;
                                                        line.Add(c);
                                                        return;
                                                    }
                                                    if (!char.IsLetter(offset[0]))
                                                    {
                                                        c.Instruction = "LOAD" + memExtOp2;
                                                        //c = SetLabel(c);
                                                        c.Arguments[0] = temp1;
                                                        c.Arguments[1] = op2;
                                                        c.MemShift = " + " + offset;
                                                        line.Add(c);

                                                        c = new Command();
                                                        c.Instruction = "STORE" + memExt;
                                                        c.Arguments[0] = temp1;
                                                        c.Arguments[1] = Dest.Name;
                                                        c.Change[1] = false;
                                                        line.Add(c);
                                                        return;
                                                    }
                                                    else
                                                    {
                                                        if ((n = Dest.Type.GetRootLength()) > 1)
                                                        {
                                                            c.Instruction = "SHL";
                                                            c.Arguments[0] = c.Arguments[2] = offset;
                                                            n = (int)Math.Log(n, 2);
                                                            c.Arguments[1] = n.ToHex9();
                                                            c.Change[1] = false;
                                                            line.Add(c);
                                                            //c = SetLabel(c);
                                                            c = new Command();
                                                        }
                                                        c.Instruction = "ADD";
                                                        //c = SetLabel(c);
                                                        c.Arguments[0] = op2;
                                                        c.Arguments[1] = offset;
                                                        c.Arguments[2] = offset;
                                                        line.Add(c);

                                                        c = new Command();
                                                        c.Instruction = "LOAD" + memExtOp2;
                                                        c.Arguments[0] = temp1;
                                                        c.Arguments[1] = op2;
                                                        c.MemShift = " + " + offset;
                                                        line.Add(c);

                                                        c = new Command();
                                                        c.Instruction = "STORE" + memExt;
                                                        c.Arguments[0] = temp1;
                                                        c.Arguments[1] = Dest.Name;
                                                        c.Change[1] = false;
                                                        line.Add(c);
                                                        return;
                                                    }
                                                }
                                                temp2 = CreateTemp(list, Dest.CloneUp());
                                                if ((n = Dest.Type.GetRootLength()) > 1)
                                                {
                                                    c.Instruction = "SHL";
                                                    c.Arguments[0] = offset;
                                                    c.Arguments[2] = temp2;
                                                    n = (int)Math.Log(n, 2);
                                                    c.Arguments[1] = n.ToHex9();
                                                    c.Change[1] = false;
                                                    line.Add(c);
                                                    //c = SetLabel(c);

                                                    c = new Command();
                                                    c.Instruction = "ADD";
                                                    c.Arguments[0] = op2;
                                                    c.Arguments[1] = temp2;
                                                    c.Arguments[2] = temp2;
                                                    line.Add(c);
                                                }
                                                else
                                                {
                                                    c.Instruction = "ADD";
                                                    //c = SetLabel(c);
                                                    c.Arguments[0] = op2;
                                                    c.Arguments[1] = offset;
                                                    c.Arguments[2] = temp2;
                                                    line.Add(c);
                                                    c = new Command();
                                                }
                                                c = new Command();
                                                c.Instruction = "LOAD" + memExt;
                                                c.Arguments[0] = Dest.Name;
                                                c.Arguments[1] = temp2;
                                                c.LoadStore[0] = false;
                                                c.Change[1] = true;
                                                line.Add(c);

                                                return;
                                            }
                                            break;

                                            #endregion
                                    }
                                    break;
                                case (5):
                                    _started = true;
                                    op2 = op2.TrimEnd(';');
                                    switch (operand)
                                    {
                                        case ("+"):
                                            #region a = b + c;

                                            if (op1[0] != '&')
                                            {
                                                if (Dest.Type.Type != "float")
                                                {
                                                    op2 = CreateConst(op2, list, Dest);
                                                    if (!char.IsLetter(op2[0]))
                                                        c.Change[1] = false;
                                                    c.Instruction = "ADD";
                                                    c.Arguments[0] = op1;
                                                    c.Arguments[1] = op2;
                                                    c.Arguments[2] = Dest.Name;
                                                    //c = SetLabel(c);
                                                    line.Add(c);
                                                }
                                                else
                                                {
                                                    c.Instruction = "STORE";
                                                    op1 = EditOp2(Dest, op1, list, line);
                                                    c.Arguments[0] = op1;
                                                    c.Arguments[1] = "SP-4";
                                                    c.Change[1] = false;
                                                    line.Add(c);

                                                    c = new Command();
                                                    c.Instruction = "STORE";
                                                    op2 = EditOp2(Dest, op2, list, line);
                                                    c.Arguments[0] = op2;
                                                    c.Arguments[1] = "SP-8";
                                                    c.Change[1] = false;
                                                    line.Add(c);

                                                    c = new Command();
                                                    c.Instruction = "SUB";
                                                    c.Arguments[0] = c.Arguments[2] = "SP";
                                                    c.Arguments[1] = "8";
                                                    c.Touch = false;
                                                    line.Add(c);

                                                    c = new Command();
                                                    c.Instruction = "CALL";
                                                    c.Touch = false;
                                                    c.Arguments[0] = GetEmbeddedFunctionName(operand, Dest.Type.Type);
                                                    if (!EmbeddedFunctionList.Contains(c.Arguments[0]))
                                                        EmbeddedFunctionList.Add(c.Arguments[0]);
                                                    line.Add(c);

                                                    c = new Command();
                                                    c.Instruction = "ADD";
                                                    c.Arguments[0] = c.Arguments[2] = "SP";
                                                    c.Arguments[1] = "4";
                                                    c.Touch = false;
                                                    line.Add(c);

                                                    c = new Command();
                                                    c.Instruction = "POP";
                                                    c.Arguments[0] = Dest.Name;
                                                    c.LoadStore[0] = false;
                                                    line.Add(c);
                                                }
                                            }

                                            #endregion

                                            #region a = &b + c;

                                            else
                                            {
                                                c.Instruction = "MOVE";
                                                c.LoadStore[1] = false;
                                                c.Arguments[0] = op1.TrimStart('&');
                                                c.Change[0] = false;
                                                op1 = CreateTemp(list, Dest);
                                                if (!char.IsLetter(op2[0]))
                                                    c.Change[0] = false;
                                                c.Arguments[1] = op1;
                                                //c = SetLabel(c);
                                                line.Add(c);

                                                op2 = CreateConst(op2, list, Dest);
                                                if (!char.IsLetter(op2[0]))
                                                    c.Change[1] = false;
                                                c.Instruction = "ADD";
                                                c.Arguments[0] = op1;
                                                c.Arguments[1] = op2;
                                                c.Arguments[2] = Dest.Name;
                                                line.Add(c);
                                            }
                                            break;

                                            #endregion
                                        case ("-"):
                                            #region a = b - c;

                                            if (Dest.Type.Type != "float")
                                            {
                                                bool inverse = false;
                                                if (op1.IsNumber())
                                                {
                                                    temp1 = op1;
                                                    op1 = op2;
                                                    op2 = temp1;
                                                    inverse = true;
                                                }
                                                op2 = CreateConst(op2, list, Dest);
                                                if (op2.IsNumber())
                                                    c.Change[1] = false;
                                                c.Instruction = "SUB";
                                                c.Arguments[0] = op1;
                                                c.Arguments[1] = op2;
                                                c.Arguments[2] = Dest.Name;
                                                //c = SetLabel(c);
                                                line.Add(c);
                                                if (inverse)
                                                {
                                                    c = new Command();
                                                    c.Instruction = "XOR";
                                                    c.Arguments[0] = c.Arguments[2] = Dest.Name;
                                                    n = 1;
                                                    c.Arguments[1] = n.ToHex9();
                                                    c.Change[1] = false;
                                                    line.Add(c);
                                                }
                                            }
                                            else
                                            {
                                                c.Instruction = "STORE";
                                                op1 = EditOp2(Dest, op1, list, line);
                                                c.Arguments[0] = op1;
                                                c.Arguments[1] = "SP-4";
                                                c.Change[1] = false;
                                                line.Add(c);

                                                c = new Command();
                                                c.Instruction = "STORE";
                                                op2 = EditOp2(Dest, op2, list, line);
                                                c.Arguments[0] = op2;
                                                c.Arguments[1] = "SP-8";
                                                c.Change[1] = false;
                                                line.Add(c);

                                                c = new Command();
                                                c.Instruction = "SUB";
                                                c.Arguments[0] = c.Arguments[2] = "SP";
                                                c.Arguments[1] = "8";
                                                c.Touch = false;
                                                line.Add(c);

                                                c = new Command();
                                                c.Instruction = "CALL";
                                                c.Touch = false;
                                                c.Arguments[0] = GetEmbeddedFunctionName(operand, Dest.Type.Type);
                                                if (!EmbeddedFunctionList.Contains(c.Arguments[0]))
                                                    EmbeddedFunctionList.Add(c.Arguments[0]);
                                                line.Add(c);

                                                c = new Command();
                                                c.Instruction = "ADD";
                                                c.Arguments[0] = c.Arguments[2] = "SP";
                                                c.Arguments[1] = "4";
                                                c.Touch = false;
                                                line.Add(c);

                                                c = new Command();
                                                c.Instruction = "POP";
                                                c.Arguments[0] = Dest.Name;
                                                c.LoadStore[0] = false;
                                                line.Add(c);
                                            }
                                            break;

                                            #endregion
                                        case ("*"):
                                            #region a = b * c;

                                            c.Instruction = "STORE";
                                            op1 = EditOp2(Dest, op1, list, line);
                                            c.Arguments[0] = op1;
                                            c.Arguments[1] = "SP-4";
                                            c.Change[1] = false;
                                            line.Add(c);

                                            c = new Command();
                                            c.Instruction = "STORE";
                                            op2 = EditOp2(Dest, op2, list, line);
                                            c.Arguments[0] = op2;
                                            c.Arguments[1] = "SP-8";
                                            c.Change[1] = false;
                                            line.Add(c);

                                            c = new Command();
                                            c.Instruction = "SUB";
                                            c.Arguments[0] = c.Arguments[2] = "SP";
                                            c.Arguments[1] = "8";
                                            c.Touch = false;
                                            line.Add(c);

                                            c = new Command();
                                            c.Instruction = "CALL";
                                            c.Touch = false;
                                            c.Arguments[0] = GetEmbeddedFunctionName(operand, Dest.Type.Type);
                                            if (!EmbeddedFunctionList.Contains(c.Arguments[0]))
                                                EmbeddedFunctionList.Add(c.Arguments[0]);
                                            line.Add(c);

                                            c = new Command();
                                            c.Instruction = "ADD";
                                            c.Arguments[0] = c.Arguments[2] = "SP";
                                            c.Arguments[1] = "4";
                                            c.Touch = false;
                                            line.Add(c);

                                            c = new Command();
                                            c.Instruction = "POP";
                                            c.Arguments[0] = Dest.Name;
                                            c.LoadStore[0] = false;
                                            line.Add(c);
                                            break;

                                            #endregion
                                        case ("/"):
                                            #region a = b / c;

                                            c.Instruction = "STORE";
                                            op1 = EditOp2(Dest, op1, list, line);
                                            c.Arguments[0] = op1;
                                            c.Arguments[1] = "SP-4";
                                            c.Change[1] = false;
                                            line.Add(c);

                                            c = new Command();
                                            c.Instruction = "STORE";
                                            op2 = EditOp2(Dest, op2, list, line);
                                            c.Arguments[0] = op2;
                                            c.Arguments[1] = "SP-8";
                                            c.Change[1] = false;
                                            line.Add(c);

                                            c = new Command();
                                            c.Instruction = "SUB";
                                            c.Arguments[0] = c.Arguments[2] = "SP";
                                            c.Arguments[1] = "8";
                                            c.Touch = false;
                                            line.Add(c);

                                            c = new Command();
                                            c.Instruction = "CALL";
                                            c.Touch = false;
                                            c.Arguments[0] = GetEmbeddedFunctionName(operand, Dest.Type.Type);
                                            if (!EmbeddedFunctionList.Contains(c.Arguments[0]))
                                                EmbeddedFunctionList.Add(c.Arguments[0]);
                                            line.Add(c);

                                            c = new Command();
                                            c.Instruction = "ADD";
                                            c.Arguments[0] = c.Arguments[2] = "SP";
                                            c.Arguments[1] = "4";
                                            c.Touch = false;
                                            line.Add(c);

                                            c = new Command();
                                            c.Instruction = "POP";
                                            c.Arguments[0] = Dest.Name;
                                            c.LoadStore[0] = false;
                                            line.Add(c);
                                            break;

                                            #endregion
                                        case ("%"):
                                            #region a = b % c;

                                            c.Instruction = "STORE";
                                            op1 = EditOp2(Dest, op1, list, line);
                                            c.Arguments[0] = op1;
                                            c.Arguments[1] = "SP-4";
                                            c.Change[1] = false;
                                            line.Add(c);

                                            c = new Command();
                                            c.Instruction = "STORE";
                                            op2 = EditOp2(Dest, op2, list, line);
                                            c.Arguments[0] = op2;
                                            c.Arguments[1] = "SP-8";
                                            c.Change[1] = false;
                                            line.Add(c);

                                            c = new Command();
                                            c.Instruction = "SUB";
                                            c.Arguments[0] = c.Arguments[2] = "SP";
                                            c.Arguments[1] = "8";
                                            c.Touch = false;
                                            line.Add(c);

                                            c = new Command();
                                            c.Instruction = "CALL";
                                            c.Touch = false;
                                            c.Arguments[0] = GetEmbeddedFunctionName(operand, Dest.Type.Type);
                                            if (!EmbeddedFunctionList.Contains(c.Arguments[0]))
                                                EmbeddedFunctionList.Add(c.Arguments[0]);
                                            line.Add(c);

                                            c = new Command();
                                            c.Instruction = "ADD";
                                            c.Arguments[0] = c.Arguments[2] = "SP";
                                            c.Arguments[1] = "4";
                                            c.Touch = false;
                                            line.Add(c);

                                            c = new Command();
                                            c.Instruction = "POP";
                                            c.Arguments[0] = Dest.Name;
                                            c.LoadStore[0] = false;
                                            line.Add(c);
                                            break;

                                            #endregion
                                        case ("<<"):
                                            #region a = b << c;

                                            op2 = CreateConst(op2, list, Dest);
                                            if (!char.IsLetter(op2[0]))
                                                c.Change[1] = false;
                                            op1 = EditOp2(Dest, op1, list, line);
                                            c.Instruction = "SHL";
                                            c.Arguments[0] = op1;
                                            c.Arguments[1] = op2;
                                            c.Arguments[2] = Dest.Name;
                                            //c = SetLabel(c);
                                            line.Add(c);
                                            break;

                                            #endregion
                                        case (">>"):
                                            #region a = b >> c;

                                            op2 = CreateConst(op2, list, Dest);
                                            if (!char.IsLetter(op2[0]))
                                                c.Change[1] = false;
                                            op1 = EditOp2(Dest, op1, list, line);
                                            if (Dest.Type.IsSigned())
                                                c.Instruction = "ASHR";
                                            else
                                                c.Instruction = "SHR";
                                            c.Arguments[0] = op1;
                                            c.Arguments[1] = op2;
                                            c.Arguments[2] = Dest.Name;
                                            //c = SetLabel(c);
                                            line.Add(c);
                                            break;

                                            #endregion
                                        case ("|"):
                                            #region a = b | c;

                                            op2 = CreateConst(op2, list, Dest);
                                            if (!char.IsLetter(op2[0]))
                                                c.Change[1] = false;
                                            c.Instruction = "OR";
                                            c.Arguments[0] = op1;
                                            c.Arguments[1] = op2;
                                            c.Arguments[2] = Dest.Name;
                                            //c = SetLabel(c);
                                            line.Add(c);
                                            break;

                                            #endregion
                                        case ("&"):
                                            #region a = b & c;

                                            op2 = CreateConst(op2, list, Dest);
                                            if (!char.IsLetter(op2[0]))
                                                c.Change[1] = false;
                                            c.Instruction = "AND";
                                            c.Arguments[0] = op1;
                                            c.Arguments[1] = op2;
                                            c.Arguments[2] = Dest.Name;
                                            //c = SetLabel(c);
                                            line.Add(c);
                                            break;

                                            #endregion
                                        case ("^"):
                                            #region a = b ^ c;

                                            op2 = CreateConst(op2, list, Dest);
                                            if (!char.IsLetter(op2[0]))
                                                c.Change[1] = false;
                                            c.Instruction = "XOR";
                                            c.Arguments[0] = op1;
                                            c.Arguments[1] = op2;
                                            c.Arguments[2] = Dest.Name;
                                            //c = SetLabel(c);
                                            line.Add(c);
                                            break;

                                            #endregion
                                        case ("=="):
                                            #region a = b == c;

                                            c.Instruction = "MOVE";
                                            c.LoadStore[1] = false;
                                            n = 1;
                                            c.Arguments[0] = n.ToHex9();
                                            c.Change[0] = false;
                                            c.Arguments[1] = Dest.Name;
                                            //c = SetLabel(c);
                                            line.Add(c);
                                            c = new Command();
                                            c.Instruction = "CMP";
                                            c.Arguments[0] = op1;
                                            op2 = CreateConst(op2, list, Dest);
                                            if (!char.IsLetter(op2[0]))
                                                c.Change[1] = false;
                                            c.Arguments[1] = op2;
                                            line.Add(c);
                                            c = new Command();
                                            c.Instruction = "DB 04, 00, 80, 0D7; JR_EQ (PC+4)";
                                            c.Touch = false;
                                            line.Add(c);
                                            c = new Command();
                                            c.Instruction = "MOVE";
                                            c.LoadStore[1] = false;
                                            c.Arguments[0] = n.ToHex9();
                                            c.Change[0] = false;
                                            c.Arguments[1] = Dest.Name;
                                            line.Add(c);
                                            break;

                                            #endregion
                                        case ("<="):
                                            #region a = b <= c;

                                            c.Instruction = "MOVE";
                                            c.LoadStore[1] = false;
                                            n = 1;
                                            c.Arguments[0] = n.ToHex9();
                                            c.Change[0] = false;
                                            c.Arguments[1] = Dest.Name;
                                            //c = SetLabel(c);
                                            line.Add(c);
                                            c = new Command();
                                            c.Instruction = "CMP";
                                            c.Arguments[0] = op1;
                                            op2 = CreateConst(op2, list, Dest);
                                            if (!char.IsLetter(op2[0]))
                                                c.Change[1] = false;
                                            c.Arguments[1] = op2;
                                            line.Add(c);
                                            c = new Command();
                                            if (Dest.Type.IsSigned())
                                                c.Instruction = "DB 04, 00, 00, 0D7; JR_SLE (PC+4)";
                                            else
                                                c.Instruction = "DB 04, 00, 40, 0D6; JR_ULE (PC+4)";
                                            c.Touch = false;
                                            line.Add(c);
                                            c = new Command();
                                            c.Instruction = "MOVE";
                                            c.LoadStore[1] = false;
                                            c.Arguments[0] = n.ToHex9();
                                            c.Change[0] = false;
                                            c.Arguments[1] = Dest.Name;
                                            line.Add(c);
                                            break;

                                            #endregion
                                        case (">="):
                                            #region a = b >= c;

                                            c.Instruction = "MOVE";
                                            c.LoadStore[1] = false;
                                            n = 1;
                                            c.Arguments[0] = n.ToHex9();
                                            c.Change[0] = false;
                                            c.Arguments[1] = Dest.Name;
                                            //c = SetLabel(c);
                                            line.Add(c);
                                            c = new Command();
                                            c.Instruction = "CMP";
                                            c.Arguments[0] = op1;
                                            op2 = CreateConst(op2, list, Dest);
                                            if (!char.IsLetter(op2[0]))
                                                c.Change[1] = false;
                                            c.Arguments[1] = op2;
                                            line.Add(c);
                                            c = new Command();
                                            if (Dest.Type.IsSigned())
                                                c.Instruction = "DB 04, 00, 40, 0D7; JR_SGE (PC+4)";
                                            else
                                                c.Instruction = "DB 04, 00, C0, 0D5; JR_UGE (PC+4)";
                                            c.Touch = false;
                                            line.Add(c);
                                            c = new Command();
                                            c.Instruction = "MOVE";
                                            c.LoadStore[1] = false;
                                            c.Arguments[0] = n.ToHex9();
                                            c.Change[0] = false;
                                            c.Arguments[1] = Dest.Name;
                                            line.Add(c);
                                            break;

                                            #endregion
                                        case ("<"):
                                            #region a = b < c;

                                            c.Instruction = "MOVE";
                                            c.LoadStore[1] = false;
                                            n = 1;
                                            c.Arguments[0] = n.ToHex9();
                                            c.Change[0] = false;
                                            c.Arguments[1] = Dest.Name;
                                            //c = SetLabel(c);
                                            line.Add(c);
                                            c = new Command();
                                            c.Instruction = "CMP";
                                            c.Arguments[0] = op1;
                                            op2 = CreateConst(op2, list, Dest);
                                            if (!char.IsLetter(op2[0]))
                                                c.Change[1] = false;
                                            c.Arguments[1] = op2;
                                            line.Add(c);
                                            c = new Command();
                                            if (Dest.Type.IsSigned())
                                                c.Instruction = "DB 04, 00, C0, 0D6; JR_SLT (PC+4)";
                                            else
                                                c.Instruction = "DB 04, 00, C0, 0D4; JR_ULT (PC+4)";
                                            c.Touch = false;
                                            line.Add(c);
                                            c = new Command();
                                            c.Instruction = "MOVE";
                                            c.LoadStore[1] = false;
                                            c.Arguments[0] = n.ToHex9();
                                            c.Change[0] = false;
                                            c.Arguments[1] = Dest.Name;
                                            line.Add(c);
                                            break;

                                            #endregion
                                        case (">"):
                                            #region a = b > c;

                                            c.Instruction = "MOVE";
                                            c.LoadStore[1] = false;
                                            n = 1;
                                            c.Arguments[0] = n.ToHex9();
                                            c.Change[0] = false;
                                            c.Arguments[1] = Dest.Name;
                                            //c = SetLabel(c);
                                            line.Add(c);
                                            c = new Command();
                                            c.Instruction = "CMP";
                                            c.Arguments[0] = op1;
                                            op2 = CreateConst(op2, list, Dest);
                                            if (!char.IsLetter(op2[0]))
                                                c.Change[1] = false;
                                            c.Arguments[1] = op2;
                                            line.Add(c);
                                            c = new Command();
                                            if (Dest.Type.IsSigned())
                                                c.Instruction = "DB 04, 00, 80, 0D7; JR_SGT (PC+4)";
                                            else
                                                c.Instruction = "DB 04, 00, 80, 0D6; JR_UGT (PC+4)";
                                            c.Touch = false;
                                            line.Add(c);
                                            c = new Command();
                                            c.Instruction = "MOVE";
                                            c.LoadStore[1] = false;
                                            c.Arguments[0] = n.ToHex9();
                                            c.Change[0] = false;
                                            c.Arguments[1] = Dest.Name;
                                            line.Add(c);
                                            break;

                                            #endregion
                                        case ("!="):
                                            #region a = b != c;

                                            c.Instruction = "MOVE";
                                            n = 1;
                                            c.LoadStore[1] = false;
                                            c.Arguments[0] = n.ToHex9();
                                            c.Change[0] = false;
                                            c.Arguments[1] = Dest.Name;
                                            //c = SetLabel(c);
                                            line.Add(c);
                                            c = new Command();
                                            c.Instruction = "CMP";
                                            c.Arguments[0] = op1;
                                            op2 = CreateConst(op2, list, Dest);
                                            if (!char.IsLetter(op2[0]))
                                                c.Change[1] = false;
                                            c.Arguments[1] = op2;
                                            line.Add(c);
                                            c = new Command();
                                            c.Instruction = "DB 04, 00, 00, 0D6 ; JR_NE (PC+4)";
                                            c.Touch = false;
                                            line.Add(c);
                                            c = new Command();
                                            c.Instruction = "MOVE";
                                            c.Arguments[0] = n.ToHex9();
                                            c.Change[0] = false;
                                            c.LoadStore[1] = false;
                                            c.Arguments[1] = Dest.Name;
                                            line.Add(c);
                                            break;

                                            #endregion
                                        default:
                                            #region a = f (a, ... ,n);
                                            _started = true;
                                            FindFunciton(_functionList, op1);
                                            ParseCommandFunction(buffer.Remove(0, buffer.IndexOf("=") + 2), list, line, Dest.Name);
                                            break;
                                    }
                                    break;
                                default:
                                    _started = true;
                                    FindFunciton(_functionList, op1);
                                    ParseCommandFunction(buffer.Remove(0, buffer.IndexOf("=") + 2), list, line, Dest.Name);
                                    break;
                                            #endregion
                            }
                        }

                        private void ParseCommandArray(string buffer, List<Variable> list, List<Command> line)
                        {
                            string[] splitter = buffer.FirstWord().Split('[');
                            Variable Dest = FindVariable(list, splitter[0]);
                            string offset = splitter[1].TrimEnd(']', ';');
                            string op2 = (buffer.LastWord()).TrimEnd(';');
                            Command c = new Command();
                            string memExt = GetMemExtension(Dest.Type.GetRootLength());
                            int n;

                            _started = true;

                            op2 = EditOp2(Dest, op2, list, line);

                            if (!char.IsLetter(offset[0]))
                            {
                                offset = CreateConstOffset(offset, list, Dest);
                                if (offset.Equals("000000000"))
                                {
                                    //NULA
                                    c.Instruction = "STORE" + memExt;
                                    //c = SetLabel(c);
                                    c.Arguments[0] = op2;
                                    c.Arguments[1] = Dest.Name;
                                    c.Change[1] = true;
                                    line.Add(c);
                                    return;
                                }
                                if (!char.IsLetter(offset[0]))
                                {
                                    c.Instruction = "STORE" + memExt;
                                    //c = SetLabel(c);
                                    c.Arguments[0] = op2;
                                    c.Arguments[1] = Dest.Name;
                                    c.MemShift = " + " + offset;
                                    line.Add(c);
                                    return;
                                }
                                else
                                {
                                    if ((n = Dest.Type.GetRootLength()) > 1)
                                    {
                                        c.Instruction = "SHL";
                                        c.Arguments[0] = c.Arguments[2] = offset;
                                        n = (int)Math.Log(n, 2);
                                        c.Arguments[1] = n.ToHex9();
                                        c.Change[1] = false;
                                        line.Add(c);
                                        //c = SetLabel(c);
                                        c = new Command();
                                    }
                                    c.Instruction = "ADD";
                                    //c = SetLabel(c);
                                    c.Arguments[0] = Dest.Name;
                                    c.Arguments[1] = offset;
                                    c.Arguments[2] = offset;
                                    line.Add(c);

                                    c = new Command();
                                    c.Instruction = "STORE" + memExt;
                                    c.Arguments[0] = op2;
                                    c.Arguments[1] = Dest.Name;
                                    c.MemShift = " + " + offset;
                                    line.Add(c);
                                    return;
                                }
                            }
                            string temp = CreateTemp(list, Dest.CloneUp());
                            if ((n = Dest.Type.GetRootLength()) > 1)
                            {
                                c.Instruction = "SHL";
                                c.Arguments[0] = offset;
                                c.Arguments[2] = temp;
                                n = (int)Math.Log(n, 2);
                                c.Arguments[1] = n.ToHex9();
                                c.Change[1] = false;
                                line.Add(c);
                                //c = SetLabel(c);

                                c = new Command();
                                c.Instruction = "ADD";
                                c.Arguments[0] = Dest.Name;
                                c.Arguments[1] = temp;
                                c.Arguments[2] = temp;
                                line.Add(c);
                            }
                            else
                            {
                                c.Instruction = "ADD";
                                //c = SetLabel(c);
                                c.Arguments[0] = Dest.Name;
                                c.Arguments[1] = offset;
                                c.Arguments[2] = temp;
                                line.Add(c);
                                c = new Command();
                            }
                            c = new Command();
                            c.Instruction = "STORE" + memExt;
                            c.Arguments[0] = op2;
                            c.Arguments[1] = temp;
                            line.Add(c);
                            return;
                        }

                        private void ParseCommandFunction(string buffer, List<Variable> list, List<Command> line, string result)
                        {
                            Command c = new Command();
                            _started = true;

                            Function f = FindFunciton(_functionList, buffer.FirstWord());
                            Variable vp = new Variable();
                            Variable vf = new Variable();

                            string allparameters = buffer.Remove(0, buffer.IndexOf('(') + 1).TrimEnd(';', ')');
                            string[] parameters = allparameters.Split(',');
                            int cnt = 4;
                            if (parameters[0].Length >= 1)
                            {
                                parameters[parameters.Count<string>() - 1].TrimEnd(')');
                                string memExt = "";
                                cnt = 0;
                                for (int i = 0; i < parameters.Count<string>(); i++)
                                {
                                    c = new Command();
                                    string m = parameters[i].TrimStart(' ');
                                    vp = f.Parameters.ElementAt<Variable>(i);
                                    vf = FindVariable(list, m);
                                    if (m[0] == '&')
                                    {
                                        m = m.TrimStart('&');
                                        vf = FindVariable(list, m);
                                        if (vf != null)
                                        {
                                            vf = vf.Clone();
                                            if (!vf.Type.Type.Equals(vp.Type.Type))
                                            {
                                                c.Change[0] = false;
                                            }
                                        }
                                    }
                                    else if (m[0] == '\"')
                                    {
                                        Variable constStr = FindString(list, m.Substring(1, m.Length - 2));
                                        m = constStr.Name;
                                    }

                                    while ((cnt + vp.Type.Length) % 4 != 0)
                                        cnt++;
                                    cnt += vp.Type.Length;
                                    if (vf != null && !vp.Type.Type.Equals(vf.Type.Type))
                                        RefreshValue(vf, line);
                                    memExt = GetMemExtension(vp.Type.Length);
                                    c.Instruction = "STORE" + memExt;
                                    m = CreateConst(m, list, vp);
                                    if (char.IsDigit(m[0]))
                                    {
                                        Command com = new Command();
                                        com.Label = c.Label;
                                        c.Label = null;
                                        com.Instruction = "MOVE";
                                        com.LoadStore[1] = false;
                                        com.Arguments[0] = m;
                                        com.Change[0] = false;
                                        m = CreateTemp(list, vp);
                                        com.Arguments[1] = m;
                                        line.Add(com);
                                    }
                                    c.Arguments[0] = m;
                                    c.Arguments[1] = "SP-" + String.Format("{0:X9}", cnt.ToHex9());
                                    c.Change[1] = false;
                                    line.Add(c);
                                }
                                c = new Command();
                                c.Instruction = "SUB";
                                c.Touch = false;
                                c.Arguments[0] = c.Arguments[2] = "SP";

                                if (cnt % 4 == 0)
                                {
                                    c.Arguments[1] = (cnt).ToHex9();
                                }
                                else
                                {
                                    cnt = cnt / 4 * 4 + 4;
                                    c.Arguments[1] = cnt.ToHex9();
                                }
                                if (cnt != 0)
                                    line.Add(c);
                                c = new Command();
                                c.Instruction = "CALL";
                                c.Touch = false;
                                c.Arguments[0] = f.Name;
                                line.Add(c);

                                if (result == null)
                                {
                                    c = new Command();
                                    c.Instruction = "ADD";
                                    c.Arguments[0] = c.Arguments[2] = "SP";
                                    c.Touch = false;
                                    c.Arguments[1] = (cnt).ToHex9();
                                    if (cnt != 0)
                                        line.Add(c);
                                }
                                else
                                {
                                    c = new Command();
                                    c.Instruction = "ADD";
                                    c.Arguments[0] = c.Arguments[2] = "SP";
                                    c.Touch = false;
                                    c.Arguments[1] = (cnt - 4).ToHex9();
                                    if (cnt - 4 != 0)
                                        line.Add(c);
                                    c = new Command();
                                    c.Instruction = "POP";
                                    c.Arguments[0] = result;
                                    c.LoadStore[0] = false;
                                    line.Add(c);
                                }
                            }
                            else
                            {
                                c = new Command();
                                c.Instruction = "SUB";
                                c.Touch = false;
                                c.Arguments[0] = c.Arguments[2] = "SP";
                                c.Arguments[1] = cnt.ToHex9();
                                if (cnt != 0)
                                    line.Add(c);

                                c = new Command();
                                c.Instruction = "CALL";
                                c.Touch = false;
                                c.Arguments[0] = f.Name;
                                line.Add(c);

                                if (result == null)
                                {
                                    c = new Command();
                                    c.Instruction = "ADD";
                                    c.Arguments[0] = c.Arguments[2] = "SP";
                                    c.Touch = false;
                                    c.Arguments[1] = cnt.ToHex9();
                                    if (cnt != 0)
                                        line.Add(c);
                                }
                                else
                                {
                                    c = new Command();
                                    c.Instruction = "POP";
                                    c.LoadStore[0] = false;
                                    c.Arguments[0] = result;
                                    line.Add(c);
                                }
                            }
                        }

                        private void ParseCommandLabel(string buffer, List<Variable> list, List<Command> line)
                        {
                            _started = true;
                            Command c = new Command();
                            c.Label = buffer.Trim(':', '<', '>');
                            c.Instruction = "";
                            line.Add(c);
                        }

                    #endregion

                #endregion

                #region Convert To Registers

                    #region Variables

                        private int currLine;

                        private int currArg = -1;

                    #endregion

                    private void ConvertToRegisters(List<Command> line, List<Variable> list)
                    {
                        List<Register> regList = FillRegList(line);
                        int i = 0;
                        //Print(regList);
                        while (EditCloseVar(regList, i++)) ;
                        //Print(regList);
                        regList = AddRegData(regList);
                        //Print(regList);
                        //PrintBlank(line);
                        FillWithRegList(line, regList, list);
                        //PrintBlank(line);
                    }

                    #region Convert To Registers Functions

                        private Command FindNextVar(List<Command> lines)
                        {
                            if (lines.Count <= currLine)
                                return null;
                            Command line = new Command();
                            do
                            {
                                currArg++;
                                if (currArg < 3)
                                    line = lines[currLine];
                                else
                                {
                                    currArg = 0;
                                    do
                                    {
                                        ++currLine;
                                        if (lines.Count <= currLine)
                                        {
                                            return null;
                                        }
                                        line = lines[currLine];
                                        if ((line.Instruction != null && (line.Instruction.Contains("JR") || line.Instruction.Contains("RET"))) || line.Label != null)
                                            break;
                                    } while (!line.Touch || line.MemReg);
                                    if ((line.Instruction != null && (line.Instruction.Contains("JR") || line.Instruction.Contains("RET"))) || line.Label != null)
                                        break;
                                }
                            } while (line.Arguments[currArg] == null || !char.IsLetter(line.Arguments[currArg][0]) || !line.Change[currArg] || !line.Touch);
                            return line;
                        }

                        private List<Register> FillRegList(List<Command> lines)
                        {
                            currLine = 0;
                            currArg = -1;
                            List<Register> regList = new List<Register>();
                            Command c = new Command();
                            while ((c = FindNextVar(lines)) != null)
                            {
                                if (c.Instruction.Contains("JR") || c.Instruction.Contains("RET") || c.Label != null)
                                {
                                    currLine++;
                                    currArg = -1;
                                    regList.Add(new Register(true));
                                }
                                else
                                    regList.Add(new Register(c, currArg));
                            }
                            return regList;
                        }


                        private bool Exists(List<Register> list, string s)
                        {
                            foreach (Register r in list)
                            {
                                if (r.VarName.Equals(s))
                                    return true;
                            }
                            return false;
                        }

                        private bool EditCloseVar(List<Register> list, int idx)
                        {
                            if (list.Count <= idx)
                                return false;
                            if (list[idx].Edited)
                                return true;
                            if (list[idx].VarName.Equals(""))
                                return true;
                            List<Register> foundRegisters = new List<Register>();
                            Register thisRegister = list[idx];
                            bool sv;
                            thisRegister.Start = true;
                            thisRegister.End = true;
                            if (thisRegister.Save)
                            {
                                thisRegister.LoadNow = false;
                                thisRegister.SaveNow = true; 
                            }
                            else
                            {
                                thisRegister.LoadNow = true;
                                thisRegister.SaveNow = false; 
                            }
                            thisRegister.Edited = true;
                            Register foundRegister = new Register();
                            Register lastRegister = thisRegister;
                            int i;
                            for (i = idx + 1; i < list.Count; i++)
                            {
                                if ((foundRegister = list[i]).VarName.Equals(""))
                                {
                                    break;
                                }
                                if (foundRegister.VarName.Equals(thisRegister.VarName))
                                {
                                   // thisRegister.AddSave(foundRegister);
                                    foundRegister.Edited = true;
                                    sv=lastRegister.SaveNow;
                                    lastRegister.SaveNow = false;
                                    lastRegister.End = false;
                                    lastRegister = foundRegister;
                                    lastRegister.End = true;
                                    if(sv||lastRegister.Save)
                                        lastRegister.SaveNow = true;
                                    

                                }
                                else if (!Exists(foundRegisters, foundRegister.VarName))
                                {
                                    if (foundRegisters.Count < Processor.NoOfRegisters - 1)
                                    {
                                        foundRegisters.Add(foundRegister);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }

                            //for (int j = idx + 1; j < i; j++)
                            //{
                            //    foundRegister = list[j];
                            //    if (foundRegister.VarName.Equals(thisRegister.VarName))
                            //    {
                            //        foundRegister.Save = thisRegister.Save;
                            //    }
                            //}

                            return true;
                        }


                        private List<Register> AddRegData(List<Register> regList)
                        {
                            FreeRegs freeRegs = new FreeRegs();
                            List<Register> ret = new List<Register>();
                            List<int> temp = new List<int>();
                            string[] usedList = new string[Processor.NoOfRegisters];
                            int reg;
                            for (int i = 0; i < regList.Count; i++)
                            {
                                if (regList[i].VarName.Equals(""))
                                {
                                    ret.Add(regList[i]);
                                    freeRegs = new FreeRegs();
                                    temp = new List<int>();
                                    continue;
                                }
                                if (regList[i].IsNewLine)
                                {
                                    freeRegs.Add(temp);
                                    temp = new List<int>();
                                }
                                if (regList[i].Start)
                                {
                                    reg = freeRegs.Get();
                                    usedList[reg] = regList[i].VarName;
                                    regList[i].RegName = reg;
                                    //if (!regList[i].Load)
                                    //    regList[i].LoadNow = false;
                                    if (regList[i].End)
                                    {
                                        temp.Add(reg);
                                        usedList[reg] = null;
                                    }
                                }
                                else
                                {
                                    if ((reg = Array.IndexOf(usedList, regList[i].VarName)) >= 0)
                                        regList[i].RegName = reg;
                                    else
                                    {
                                        reg = freeRegs.Get();
                                        usedList[reg] = regList[i].VarName;
                                        regList[i].RegName = reg;
                                    }
                                    if (regList[i].End)
                                    {
                                        temp.Add(reg);
                                        usedList[reg] = null;
                                    }
                                }
                                ret.Add(regList[i]);
                            }
                            return ret;
                        }

                        private List<Command> AddLoad(List<Command> lines, Register reg, List<Variable> list)
                        {
                            if (reg.VarName.Contains("temp_"))
                                return lines;
                            Command c = new Command();
                            Variable v = FindVariable(list, reg.VarName);
                            c.Instruction = "LOAD" + GetMemExtension(v.Type.Length);
                            c.MemReg = true;
                            c.Change[1] = false;
                            c.LoadStore[0] = false;
                            c.Arguments[0] = "R" + reg.RegName.ToString();
                            c.Arguments[1] = reg.VarName;
                            lines.Insert(currLine++, c);
                            if (v.Type.Length == 4)
                                return lines;

                            int n;
                            c = new Command();
                            c.Instruction = "SHL";
                            c.Arguments[0] = c.Arguments[2] = "R" + reg.RegName.ToString();
                            if (v.Type.Length == 2)
                                n = 16;
                            else
                                n = 24;
                            c.Arguments[1] = n.ToHex9();
                            //c.LoadStore[2] = true;
                            c.MemReg = true;
                            lines.Insert(currLine++, c);

                            c = new Command();
                            if (v.Type.IsSigned())
                                c.Instruction = "ASHR";
                            else
                                c.Instruction = "SHR";
                            c.Arguments[0] = c.Arguments[2] = "R" + reg.RegName.ToString();
                            c.Arguments[1] = n.ToHex9();
                            c.Touch = false;
                            c.MemReg = true;
                            lines.Insert(currLine++, c);
                            if (MatchingLinesRefresh(lines[currLine - 2], lines[currLine]) && MatchingLinesRefresh(lines[currLine - 1], lines[currLine + 1]))
                            {
                                lines.RemoveAt(currLine - 2);
                                lines.RemoveAt(currLine - 2);
                                currLine -= 2;
                            }

                            return lines;
                        }

                        private List<Command> AddSave(List<Command> lines, Register reg, List<Variable> list)
                        {
                            if (reg.VarName.Contains("temp_") || reg.VarName.Contains("const_"))
                                return lines;
                            Command c = new Command();
                            Variable v = FindVariable(list, reg.VarName);
                            c.Instruction = "STORE" + GetMemExtension(v.Type.Length);
                            c.MemReg = true;
                            c.Change[1] = false;
                            c.Arguments[0] = "R" + reg.RegName.ToString();
                            c.Arguments[1] = reg.VarName;
                            lines.Insert(currLine + 1, c);
                            return lines;
                        }

                        private void FillWithRegList(List<Command> lines, List<Register> regList, List<Variable> list)
                        {
                            currLine = 0;
                            currArg = -1;
                            Command c = new Command();
                            for (int i = 0; i < regList.Count; i++)
                            {
                                c = FindNextVar(lines);
                                if (regList[i].VarName.Equals(""))
                                {
                                    currArg = -1;
                                    currLine++;
                                    continue;
                                }
                                if (regList[i].LoadNow == true)
                                    lines = AddLoad(lines, regList[i], list);
                                if (regList[i].SaveNow == true)
                                    lines = AddSave(lines, regList[i], list);
                                c.Arguments[currArg] = "R" + regList[i].RegName.ToString();
                                if (regList[i].RegName > _regUsed)
                                    _regUsed = regList[i].RegName;
                            }

                        }

                    #endregion

                #endregion

                #region Convert Parameters

                    private void ConvertParameters(List<Command> line, Function f, List<Variable> list)
                    {
                        List<string> positions = new List<string>();
                        int i = list.Count - 1;
                        int offset = 0;
                        int set = 0;
                        int SPShift = 0;
                        int retStoreCalc = 0;
                        bool noParam = false;
                        string loc = "";
                        if (f.Parameters.Count <= 0)
                            noParam = true;
                        else
                            loc = f.Parameters[f.Parameters.Count - 1].Name;
                        while (i >= 0)
                        {
                            if (list[i].IsTemp||list[i].IsGlobal)
                            {
                                positions.Insert(0, " ");
                                i--;
                                continue;
                            }
                            if (!noParam && loc.Equals(list[i].Name))
                            {
                                while (offset % 4 != 0)
                                    offset++;
                                SPShift = offset;
                                if (_regUsed > 1)
                                    offset += 4 + (_regUsed) * 4+4;
                                else
                                    offset += 4 + 4+4;
                                set = i + 1;
                            }
                            if (retStoreCalc != 0)
                            {
                                while (offset % 4 != 0)
                                {
                                    offset++;
                                }
                            }
                            else
                            {
                                while (offset % list[i].Type.Length != 0)
                                {
                                    offset++;
                                }
                            }
                            positions.Insert(0, "SP + " + offset.ToHex9());
                            if (list[i].IsArray)
                            {
                                offset += list[i].ArrayLength() * list[i].Type.GetRootLength();
                                while (offset % 4 != 0)
                                    offset++;
                            }
                            offset += list[i].Type.Length;
                            i--;
                        }
                        if (noParam)
                        {
                            SPShift = offset;
                            if (_regUsed > 1)
                                offset +=(_regUsed) * 4;
                            else
                                offset +=4;
                        }
                        else
                            offset -= 4;


                        Command cs = new Command();
                        if (line.Count<=0||!line.Last().Instruction.Contains("RET"))
                        {
                            cs.Instruction = "RET";
                            line.Add(cs);
                        }

                        if (set <= list.Count - 1)
                            AddFunctVarsAndConsts(list, line, positions);
                        if (SPShift != 0&&line.Count>1)
                        {
                            cs = new Command();
                            cs.Instruction = "SUB";
                            cs.Arguments[0] = cs.Arguments[2] = "SP";
                            cs.Arguments[1] = SPShift.ToHex9();
                            line.Insert(0, cs);
                        }
                        for (i = 0; i < line.Count; i++)
                        {

                            if (line[i].Instruction.Equals("") || line[i].Instruction == null)
                                continue;
                            if (line[i].Instruction.Contains("RET") && SPShift != 0)
                            {
                                if (line.Count <= 1)
                                    continue;
                                cs = new Command();
                                cs.Instruction = "ADD";
                                cs.Arguments[0] = cs.Arguments[2] = "SP";
                                cs.Arguments[1] = SPShift.ToHex9();
                                line.Insert(i, cs);
                                i++;
                                continue;
                            }
                            else
                            {
                                if (line[i].Instruction.Contains("LOAD") || line[i].Instruction.Contains("STORE"))
                                {
                                    if (line[i].Arguments[1] != null && !line[i].Arguments[1].IsRegName())
                                    {
                                        if (line[i].Arguments[1].Equals(Processor.FunctResult))
                                        {
                                          //  int m;
                                          //  if (f.NoOfParameters <= 1)
                                            //    m = retStoreCalc;
                                           // else
                                          //      m = retStoreCalc + (f.NoOfParameters) * 4;
                                            line[i].Arguments[1] = "SP + " + offset.ToHex9();
                                        }
                                        else
                                            line[i].Arguments[1] = GetStackPosition(line[i].Arguments[1], list, positions);
                                    }
                                }
                            }
                        }
                    }

                    #region Convert Parameters Functions

                        private string GetStackPosition(string s, List<Variable> list, List<string> pos)
                        {
                            int i;
                            for (i = 0; i < list.Count; i++)
                            {
                                if (list[i].Name.Equals(s))
                                {
                                    if (list[i].IsGlobal)
                                        return s;
                                    else
                                        break;
                                }
                            }
                            if (i >= list.Count)
                                return s;
                            return pos[i];
                        }

                        private void AddFunctVarsAndConsts(List<Variable> list, List<Command> line,List<string> positions)
                        {
                            Command c;
                            int i;
                            for(i=0;i<list.Count;i++)
                            {
                                //if (set > i)
                                //    continue;
                                if (list[i].IsTemp||list[i].IsGlobal)
                                    continue;
                                if (list[i].IsArray)
                                {
                                    AddFunctArray(list[i], line,positions[i]);
                                    continue;
                                }
                                if (!list[i].ValueSet)
                                    continue;
                                for (int j = i; j < list.Count; j++)
                                {
                                    if (list[j].ValueSet==true&&list[j].Value.Equals(list[i].Value))
                                    {
                                        c = new Command();
                                        c.Instruction = "STORE" + GetMemExtension(list[j].Type.Length);
                                        c.Arguments[0] = "R0";
                                        c.Arguments[1] = positions[j];
                                        line.Insert(0, c);
                                        list[j].ValueSet = false;
                                    }
                                }

                                if (!list[i].Value.IsLegalHex())
                                {
                                    c = new Command();
                                    c.Instruction = "ADD";
                                    c.Arguments[0] = c.Arguments[2] = "R0";
                                    c.Arguments[1] = "R1";
                                    line.Insert(0, c);

                                    c = new Command();
                                    c.Instruction = "MOVE";
                                    c.Arguments[0] = list[i].Value.Remove(0,5);
                                    c.Arguments[1] = "R0";
                                    line.Insert(0, c);

                                    c = new Command();
                                    c.Instruction = "MOVE";
                                    c.Arguments[0] = list[i].Value.Remove(5);
                                    c.Arguments[1] = "R1";
                                    line.Insert(0, c);
                                }
                                else
                                {
                                    c = new Command();
                                    c.Instruction = "MOVE";
                                    c.Arguments[0] = list[i].Value;
                                    c.Arguments[1] = "R0";
                                    line.Insert(0, c);
                                }
                            }
                        }

                        private void AddFunctArray(Variable var, List<Command> line,string position)
                        {
                            int n=Convert.ToInt32(position.LastWord(),16)+4;
                            Command c = new Command();

                            c.Instruction = "STORE";
                            c.Arguments[0] = "R0";
                            c.Arguments[1] = position;
                            line.Insert(0, c);

                            c = new Command();
                            c.Instruction = "ADD";
                            c.Arguments[0] = "SP";
                            c.Arguments[1] = n.ToHex9();
                            c.Arguments[2] = "R0";
                            line.Insert(0, c);

                            if (var.IsArrayEmpty)
                            {
                                return;
                            }
                            string memExt=GetMemExtension(var.Type.GetRootLength());
                            int memLength = var.Type.RootType.GetLength();
                            int cnt = var.ArrayLength();
                            for (int i = 0; i < cnt; i++)
                            {
                                if (var.ArraySet[i])
                                {
                                    for (int j = i; j < cnt; j++)
                                    {
                                        if (var.ArraySet[i] == true && var.ArrayValues[i].Equals(var.ArrayValues[j]))
                                        {
                                            c = new Command();
                                            c.Instruction = "STORE" + memExt;
                                            c.Arguments[0] = "R0";
                                            c.Arguments[1] = "SP + " + (n+i*memLength).ToHex9();
                                            line.Insert(0, c);
                                            var.ArraySet[i] = false;
                                        }
                                    }

                                    if (!var.ArrayValues[i].IsLegalHex())
                                    {
                                        c = new Command();
                                        c.Instruction = "ADD";
                                        c.Arguments[0] = c.Arguments[2] = "R0";
                                        c.Arguments[1] = "R1";
                                        line.Insert(0, c);

                                        c = new Command();
                                        c.Instruction = "MOVE";
                                        c.Arguments[0] = var.ArrayValues[i].Remove(0, 5);
                                        c.Arguments[1] = "R0";
                                        line.Insert(0, c);

                                        c = new Command();
                                        c.Instruction = "MOVE";
                                        c.Arguments[0] = var.ArrayValues[i].Remove(5);
                                        c.Arguments[1] = "R1";
                                        line.Insert(0, c);
                                    }
                                    else
                                    {
                                        c = new Command();
                                        c.Instruction = "MOVE";
                                        c.Arguments[0] = var.ArrayValues[i];
                                        c.Arguments[1] = "R0";
                                        line.Insert(0, c);
                                    }
                                }
                            }
                        }

                    #endregion

                #endregion

                private void FillWithPushPulls(List<Command> line)
                {
                    if (_regUsed == -1)
                    {
                        for (int i = 0; i < line.Count && i < 3; i++)
                        {
                            if(line[i].Arguments[0]!=null&&line[i].Arguments[0].Equals(Processor.RegisterNames[0]))
                                _regUsed=0;
                            else if (line[i].Arguments[0] != null && line[i].Arguments[0].Equals(Processor.RegisterNames[1]))
                            {    
                                _regUsed=1;
                                break;
                            }
                        }
                        if(_regUsed==-1)
                            return;
                    }
                    Command c=new Command();
                    for (int i = _regUsed; i >= 0; i--)
                    {
                        c.Instruction="PUSH";
                        c.Arguments[0]="R"+i.ToString();
                        line.Insert(0, c);
                        c=new Command();
                    }
                    for (int j = 0; j < line.Count; j++)
                    {
                        if (line[j].Instruction.Contains("RET"))
                        {
                            for (int i = _regUsed; i >= 0; i--)
                            {
                                c.Instruction = "POP";
                                c.Arguments[0] = "R" + i.ToString();
                                line.Insert(j, c);
                                c = new Command();
                                j++;
                            }
                        }
                    }
                    return;
                }

                private void FinishParsing(List<Command> line,string name)
                {
                    foreach (Command c in line)
                    {
                        if (c.Label != null)
                            c.Label = c.Label.Replace(".", "");
                        if (c.Instruction.StartsWith("JR"))
                        {
                            c.Arguments[0] = c.Arguments[0].Replace(".", "");
                        }
                        else if (c.Instruction.Equals("CALL"))
                        {
                            if (_embeddedFunctionList.Contains(c.Arguments[0]))
                            {
                                if (!AdditionalFunctionList.Contains(c.Arguments[0]))
                                    AdditionalFunctionList.Add(c.Arguments[0]);
                            }
                        }
                    }

                    AddFunctName(line, name); 
                }

                private void AddFunctName(List<Command> line, string name)
                {
                    Command c = new Command();
                    c.Label = name;
                    c.Instruction = "";
                    line.Insert(0,c);
                }

                #region Const and Temp

                    #region Variables

                        private int NoTemp;

                        private bool LockTemp;

                        private int NoConst;

        #endregion

                    private string EditOp2(Variable Dest, string op2, List<Variable> list, List<Command> line)
                    {
                        string name = CreateConst(op2, list, Dest);
                        if (name[0].IsNumberPart())
                        {
                            Command c = new Command();
                            //c = SetLabel(c);
                            c.Instruction = "MOVE";
                            c.LoadStore[1] = false;
                            c.Arguments[0] = name;
                            c.Change[0] = false;
                            name = CreateTemp(list, Dest);
                            c.Arguments[1] = name;
                            line.Add(c);
                        }
                        return name;
                    }

                    #region Const

                        private string CreateConst(string name, List<Variable> list, Variable Var)
                        {
                            if (!(name[0].IsNumberPart()))
                                return name;
                            string s;
                            if (name.Contains('.'))
                            {
                                float f = Single.Parse(name, System.Globalization.CultureInfo.InvariantCulture);
                                s = f.ToHex9();
                            }
                            else
                            {
                                s = int.Parse(name).ToHex9();
                            }
                            if (s.IsLegalHex())
                            {
                                return s;
                            }
                            Variable v = Var.Clone();
                            v.ConvertValue(s);
                            if ((v.Name = ConstExists(list, v.Value)) == null)
                            {
                                v.Name = GetConstName(v.Value);
                                v.IsConst = true;
                                list.Add(v);
                            }
                            return v.Name;
                        }

                        private string CreateConstOffset(string name, List<Variable> list, Variable Var)
                        {
                            if (!(name[0].IsNumberPart()))
                                return name;
                            string s;
                            s = (int.Parse(name) * Var.Type.GetRootLength()).ToHex9();
                            if (s.IsLegalHex())
                            {
                                return s;
                            }
                            Variable v = Var.Clone();
                            v.ConvertValue(int.Parse(name) * Var.Type.Length);
                            if ((v.Name = ConstExists(list, v.Value)) == null)
                            {
                                GetConstName(v.Value);
                                v.IsConst = true;
                                list.Add(v);
                            }
                            return v.Name;
                        }

                        private string ConstExists(List<Variable> list, string value)
                        {
                            foreach (Variable c in list)
                            {
                                if (c.Value.Equals(value))
                                    return c.Name;
                            }
                            return null;
                        }

                        private string GetConstName(string val)
                        {
                            return "const_" + val.Remove(0, 5);
                        }

                        private string GetConstName()
                        {
                            return "const." + NoConst++.ToString();
                        }

                    #endregion

                    #region Temp

                        private string CreateTemp(List<Variable> list, Variable Var)
                        {
                            Variable v = Var.Clone();
                            v.IsTemp = true;
                            v.Name = GetTempName();
                            if (!LockTemp)
                                list.Add(v);
                            if (NoTemp >= Processor.NoOfRegisters)
                                LockTemp = true;
                            return v.Name;
                        }

                        private string GetTempName()
                        {
                            return "temp_" + NoTemp++.ToString();
                        }

                    #endregion

                #endregion

            #endregion

        #endregion

        #region Add Global Variable Values

            private void AddGlobalVarsAndConsts(List<Variable> list, List<Command> line)
            {
                Command c = null;
                foreach (Variable var in list)
                {
                    if (var.IsTemp)
                        continue;
                    if (var.IsArray)
                    {
                        AddGlobalArray(var, line);
                        continue;
                    }
                    c = new Command();
                    c.Label = var.Name;
                    switch (var.Type.Length)
                    {
                        case 4:
                            c.Instruction = "DW";
                            break;
                        case 2:
                            c.Instruction = "DH";
                            break;
                        case 1:
                            c.Instruction = "DB";
                            break;
                        default:
                            Error.PrintError("unknown length", 1, false);
                            break;
                    }
                    c.Arguments[0] = var.Value;
                    line.Add(c);
                }
            }

            private void AddGlobalArray(Variable var, List<Command> line)
            {
                string ptr = GetTempName();
                Command c = new Command();
                c.Label = var.Name;
                c.Instruction = "DW";
                c.Arguments[0] = ptr;
                line.Add(c);
                int cnt = 0;
                c = new Command();
                if (var.IsArrayEmpty)
                {
                    c.Instruction = "`DS";
                    c.Label = ptr;
                    c.Arguments[0] = (var.ArrayLength() * var.Type.GetRootLength()).ToString();
                    line.Add(c);
                    return;
                }
                if (var.Type.GetRootLength() == 4)
                {
                    for (int i = 0; i < var.ArrayLength(); i++)
                    {
                        if (i == 0)
                            c.Label = ptr;
                        c.Instruction = "DW";
                        c.Arguments[0] = var.ArrayValues[i];
                        line.Add(c);
                        c = new Command();
                    }
                }
                else if (var.Type.GetRootLength() == 2)
                {
                    for (int i = 0; i < var.ArrayLength(); i++)
                    {
                        if (i == 0)
                            c.Label = ptr;
                        c.Arguments[cnt] = var.ArrayValues[i];
                        if (cnt == 1)
                        {
                            c.Instruction = "DH";
                            line.Add(c);
                            c = new Command();
                        }
                        cnt++;
                        cnt %= 2;
                    }
                    if (cnt != 0)
                    {
                        c.Instruction = "DB";
                        if (c.Arguments[1].Length <= 0)
                            c.Arguments[1] = Cwords.Zero;
                        line.Add(c);
                    }
                }
                else
                {
                    for (int i = 0; i < var.ArrayValues.Count; i++)
                    {
                        if (i == 0)
                            c.Label = ptr;
                        if (cnt == 3)
                        {
                            c.MemShift = var.ArrayValues[i];
                            c.Instruction = "DB";
                            line.Add(c);
                            c = new Command();
                        }
                        else
                        {
                            c.Arguments[cnt] = var.ArrayValues[i];
                        }
                        cnt++;
                        cnt %= 4;
                    }
                    if (cnt != 0)
                    {
                        c.Instruction = "DB";
                        if (c.Arguments[1] == null || c.Arguments[1].Length <= 0)
                            c.Arguments[1] = Cwords.Zero;
                        if (c.Arguments[2] == null || c.Arguments[2].Length <= 0)
                            c.Arguments[2] = Cwords.Zero;
                        if (c.MemShift == null || c.MemShift.Length <= 0)
                            c.MemShift = Cwords.Zero;
                        line.Add(c);
                    }
                }

            }

        #endregion

            private static string AddAdditionalFunctions(string input)
            {
                var output = "";

                //add built-in/embedded
                if (!Directory.Exists(ProgramStats.DefaultBuiltInFunctionsFolder))
                    Error.PrintError("Directory missing: " + ProgramStats.DefaultBuiltInFunctionsFolder);
                try
                {
                    foreach (var builtInFunction in EmbeddedFunctionList)
                    {
                        var files = Directory.GetFiles(ProgramStats.DefaultBuiltInFunctionsFolder, builtInFunction+".*");
                        if (files.Length <= 0)
                        {
                            File.Create(Path.Combine(ProgramStats.DefaultBuiltInFunctionsFolder, builtInFunction)+".a").Close();
                            Error.PrintError(Path.Combine(ProgramStats.DefaultBuiltInFunctionsFolder, builtInFunction) +
                                             ".a not written yet.\n Blank file created. Add assembler code and try again.");
                        }

                        var fileName = files[0];
                        try
                        {
                            using (var s = new StreamReader(Path.Combine(ProgramStats.DefaultBuiltInFunctionsFolder, fileName)))
                                output += s.ReadToEnd();
                        }
                        catch (Exception)
                        {
                            Error.PrintError("File unreachable: " + Path.Combine(ProgramStats.DefaultBuiltInFunctionsFolder, fileName));
                            return null;
                        }
                    }
                }
                catch (Exception)
                {
                    Error.PrintError("Directory unreachable: " + ProgramStats.DefaultBuiltInFunctionsFolder);
                    return null;
                }

                //add additional
                if (!Directory.Exists(ProgramStats.DefaultAdditionalFunctionsFolder))
                {
                    Error.PrintError("Directory missing: " + ProgramStats.DefaultAdditionalFunctionsFolder);
                    return null;
                }
                try
                {
                    foreach (var additionalFunction in AdditionalFunctionList)
                    {
                        try
                        {
                            string buffer;
                            using (var s = new StreamReader(Path.Combine(ProgramStats.DefaultAdditionalFunctionsFolder, ".a")))
                            {
                                buffer = s.ReadToEnd();
                            }
                            input = input.Replace(additionalFunction + "  \n\t\tRET", buffer);
                        }
                        catch (Exception)
                        {
                            Error.PrintError("File unreachable: " + Path.Combine(ProgramStats.DefaultAdditionalFunctionsFolder,".a"));
                            return null;
                        }
                    }
                }
                catch (Exception)
                {
                    Error.PrintError("Directory unreachable: " + ProgramStats.DefaultAdditionalFunctionsFolder);
                    return null;
                }

                input = input.Insert(0, output);

                input = input.Insert(0, Boot);

                return input;
            }

        #region Additional Functions

            private Variable VariableNameExists(string name, List<Variable> list)
            {
                foreach (Variable v in list)
                {
                    if (v.Name.Equals(name))
                        return v;
                }
                return null;
            }

            private string GetMemExtension(int n)
            {
                switch (n)
                {
                    case (1):
                        return "B";
                    case (2):
                        return "H";
                    case (4):
                        return "";
                    default:
                        Error.PrintError("Var Length", 1, false);
                        return null;
                }
            }

            private string GetEmbeddedFunctionName(string operand, string type)
            {
                if (type.IsSigned())
                    return operand.OperandName() + "Signed";
                else if (type.IsUnsigned())
                    return operand.OperandName() + "Unsigned";
                else if (type.IsReal())
                    return operand.OperandName() + "Float";
                else
                    return operand.OperandName() + "Unknown";
            }

            private void RefreshValue(Variable var,List<Command> line)
            {
                if (var.Type.Length == 4)
                    return;
                int shift;
                if (var.Type.Length == 2)
                    shift = 16;
                else
                    shift = 24;
                Command c = new Command();
                c.Instruction = "SHL";
                c.Arguments[0] = c.Arguments[2] = var.Name;
                c.Arguments[1] = shift.ToHex9();
                //c.LoadStore[2] = true;
                c.Change[1] = false;
                //c = SetLabel(c);
                line.Add(c);
                c = new Command();
                if (var.Type.Type.IsSigned())
                    c.Instruction = "ASHR";
                else
                    c.Instruction = "SHR";
                c.Arguments[0] = c.Arguments[2] = var.Name;
                //c.LoadStore[2] = true;
                c.Arguments[1] = shift.ToHex9();
                c.Change[1] = false;
                //c = SetLabel(c);
                line.Add(c);
            }
    
            private bool MatchingLinesRefresh(Command c1, Command c2)
            {
                if (!c1.Instruction.Equals(c2.Instruction))
                    return false;
                if (!c1.Arguments[1].Equals(c2.Arguments[1]))
                    return false;
                if (!c1.Arguments[0].Equals(c1.Arguments[2]))
                    return false;
                if (!c2.Arguments[0].Equals(c2.Arguments[2]))
                    return false;
                return true;
            }

            private Variable FindVariable(List<Variable> list, string name)
            {
                bool found = false;
                Variable val = new Variable();
                foreach (Variable v in list)
                {
                    if (v.Name.Equals(name))
                    {
                        found = true;
                        val = v;
                        break;
                    }
                }
                if (found)
                    return val;
                else
                    return null;
            }

            private Function FindFunciton(List<Function> list, string name)
            {
                bool found = false;
                Function val = null;
                foreach (Function v in list)
                {
                    if (v.Name.Equals(name))
                    {
                        found = true;
                        val = v;
                        break;
                    }
                }
                if (found)
                    return val;
                else
                    return null;
            }

            private Variable FindString(List<Variable> list, string name)
            {
                bool found = false;
                Variable val = null;
                name = System.Text.RegularExpressions.Regex.Unescape(name);
                foreach (Variable v in list)
                {
                    if (v.ArrayString.Equals(name))
                    {
                        found = true;
                        val = v;
                        break;
                    }
                }
                if (found)
                    return val;
                else
                    return null;
            }

        #endregion
    }
}
