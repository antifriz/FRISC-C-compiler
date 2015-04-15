using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ExtensionMethods;
using System.Diagnostics;

namespace Compiler1._0
{
    class Parser
    {
        #region Variables

            public string InputFile = null;

            private List<string> AdditionalFunctions = new List<string>();

            private List<Function> Functions = new List<Function>();

            private bool Started = false;

            private int regUsed = -1;

            public string HeaderDirectory = null;

            public string AdditionalFunctionsPath = @"data\Additional Functions";

            private List<string> EmbeddedFunctions = new List<string>();

            private List<string> EmbeddedFunctionsFound = new List<string>();

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
                if (c.Instruction.Equals("DB")||c.Instruction.Equals("DW")||c.Instruction.Equals("DS")||c.Instruction.Equals("DH"))
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
                if (c.Instruction.Equals("DB") || c.Instruction.Equals("DW") || c.Instruction.Equals("DS") || c.Instruction.Equals("DH"))
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
                Console.WriteLine("{0,14}|{1,5}|{2,5}|{3,5}|{4,5}|{5,5}", "Name:", "Load!", "Save!", "Load", "Save", "Edit");
                for (int i = 0; i < 46; i++)
                    Console.Write("-");
                Console.WriteLine();
                foreach (Register c in list)
                {
                    if (c.VarName != null)
                        Console.WriteLine("{0,14}|{1,5}|{2,5}|{3,5}|{4,5}|{5,5}|{6,1}", c.VarName, c.LoadNow, c.SaveNow, c.Load, c.Save, c.Edited, c.RegName);
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

        public void Parse(string sourcePath,string destinationPath)
        {
            InputFile = sourcePath;

            AnalyzeSource(sourcePath);
            
            InitializeParser();          

            List<Command> parserOutputRaw = new List<Command>(StartParsing());

            PrintBlank(parserOutputRaw);

            string parserOutput = ConvertToString(parserOutputRaw);

            PrintAdditionalFuncts(AdditionalFunctions);

            parserOutput = AddAdditionalFunctions(parserOutput);

            File.WriteAllText(destinationPath, parserOutput);

        }

        private void AnalyzeSource(string sourcePath)
        {
            string s = File.ReadAllText(sourcePath);
            foreach (string c in Cwords.unwanted)
            {
                if(System.Text.RegularExpressions.Regex.IsMatch(s,string.Format(@"\b{0}\b",System.Text.RegularExpressions.Regex.Escape(c))))
                    Error.Stop("Using: "+c);
            }
            int idx1=-1,idx2;
            while ((idx1 = s.IndexOf('{', idx1 + 1)) >= 0)
            {
                idx2 = s.IndexOf('}', idx1);
                if (!s.Substring(idx1, idx2 - idx1 + 1).Contains(Environment.NewLine + Environment.NewLine))
                    s=s.Insert(idx1 + 1, Environment.NewLine);
            }
            if(s.Contains("InitIRQ"))
                EmbeddedFunctionsFound.Add("InitIRQ");
            if(s.Contains("InitFIQ"))
                EmbeddedFunctionsFound.Add("InitFIQ");
            File.WriteAllText(sourcePath, s);
        }

        private void InitializeParser()
        {
            EmbeddedFunctions = new List<string>(File.ReadAllLines(Path.Combine(AdditionalFunctionsPath, "functlist.p")));
        }

        private List<Command> StartParsing()
        {
            ParseFunctionPrototypes();

            List<Command> output = new List<Command>();
            //output = ParseMain();

            List<Command> functSum = new List<Command>();
            if (Functions != null)
            {
                List<Command> funct = new List<Command>();
                Command c = new Command();
                foreach (Function f in Functions)
                {
                    funct = ParseFunction(f);
                    //c = new Command();
                    //c.Instruction = "";
                    //if (!EmbeddedFunctions.Contains(f.Name))
                    //    c.Label = GetNewFunctName(f.Name);
                    //else
                    //    c.Label = f.Name;
                    //funct.Insert(0, c);
                    functSum.AddRange(funct);
                }
            }
            return functSum;
        }

        #region Start Parsing Functions

            private void ParseFunctionPrototypes()
            {
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
                       // if (prototype.Contains("main ()"))
                        //    continue;
                        Function f = new Function();
                        f.ReadData(read, prototype);
                        Functions.Add(f);
                    }
                }

            }

            #region Parsing Main

                private List<Command> ParseMain()
                {
                    List<Variable> VariablesDeclared = new List<Variable>();
                    List<Command> Commands = new List<Command>();
                    PreParsingMain(VariablesDeclared, "main");
                    using (StreamReader MainStream = FindMain())
                    {

                        while (ParseDeclaration(MainStream, VariablesDeclared)) ;

                        Started = false;

                        while (ParseCommandMain(MainStream, VariablesDeclared, Commands)) ;
                    }
                    Print(Commands);
                    //Print(VariablesDeclared);
                    ConvertToRegisters(Commands, VariablesDeclared);
                    //PrintBlank(Commands);
                    //ReduceJumps(Commands);

                    AddMainVarsAndConsts(VariablesDeclared, Commands);

                    FinishParsingMain(VariablesDeclared, Commands);
                    PrintBlank(Commands);
                    //PrintToStream(Commands);
                    Print(VariablesDeclared);
                    return Commands;
                }

                #region Parsing Main Functions

                    private StreamReader FindMain()
                    {
                        StreamReader Finder = new StreamReader(InputFile);
                        while (!Finder.ReadLine().Contains("main ()")) ;
                        Finder.ReadLine();
                        return Finder;
                    }

                    private void PreParsingMain(List<Variable> list,string name)
                    {
                        int idxS;
                        int idxE;
                        Variable v = new Variable();
                        string str,buffer;
                        string[] buffers=File.ReadAllLines(InputFile);
                        int i=0;
                        while (!buffers[i].StartsWith(name))
                            i++;
                        i+=2;
                        for(;;i++)
                        {
                            if (buffers[i].Equals("}"))
                                break;
                            buffer = buffers[i];
                            idxE = -1;
                            while ((idxS = buffer.IndexOf('\"', idxE + 1)) >= 0)
                            {
                                idxE = buffer.IndexOf('"', idxS + 1);
                                while(buffer[idxE-1].Equals('\\')&&!buffer[idxE-1].Equals('\\'))
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
                                buffers[i]=buffers[i].Replace("\""+str+"\"", v.Name);
                                list.Add(v);
                            }
                        }
                        File.WriteAllLines(InputFile,buffers);
                    }

                    private bool ParseDeclaration(StreamReader Read, List<Variable> List)
                    {
                        string s = Read.ReadLine();
                        if (s.Length <= 1)
                            return false;
                        if (s.FirstWord().Equals("extern"))
                        {
                            Function f = new Function();
                            f.Name = s.Word(s.WordCount() - 1).TrimEnd(';');
                            f.NoOfParameters = -1;
                            return true;
                        }
                        if (s.FirstWord().TrimEnd(';').Equals("return"))
                            return false;
                        if (!s.StartsWithType())
                        {
                            return false;
                        }
                        Variable v = new Variable();
                        int idx = s.LastIndexOf(' ');
                        string name = s.Remove(0, idx + 1);
                        name = name.TrimEnd(';');
                        string type = s.Remove(idx);
                        type = type.TrimStart(' ');
                        /*
                        if (name.Contains('['))
                        {
                            string rest = name.Remove(0, name.IndexOf('[') + 1).TrimEnd(';', ']');
                            name = name.Remove(name.IndexOf('['));
                            v.SetVariable(name, type);
                            v = v.CloneUp();

                            string[] arrayVals = rest.Trim(']').Split('[');
                            foreach (string ggg in arrayVals)
                            {
                                try
                                {
                                    v.Array.Add(int.Parse(ggg));
                                }
                                catch
                                {
                                    Error.Stop(ggg + " nije broj u deklaraciji");
                                }
                            }
                        }
                        else
                        {*/
                        v.SetVariable(name, type);
                        //  v.NonNullArray();
                        /*  }*/
                        List.Add(v);
                        return true;
                    }

                    #region Parsing Command Main

                        private bool ParseCommandMain(StreamReader read, List<Variable> list, List<Command> line)
                        {
                            string s = read.ReadLine();
                            if (s.Length <= 1)
                                return false;/*
                            Command c = new Command();
                             c.Label = s;
                             c.Touch = false;
                             line.Add(c);*/
                            s = s.TrimStart();
                             /*   if (line.Count > 0)
                                {
                                    Console.Clear();
                                    Print(line);
                                }*/
                            string determinator = s.FirstWord();
                            switch (determinator)
                            {
                                case "goto":
                                    Started = true;
                                    ParseCommandGoto(s, line);
                                    break;
                                case "if":
                                    Started = true;
                                    ParseCommandIf(s, list, line);
                                    break;
                                case "return":
                                    Started = true;
                                    ParseCommandReturnMain(line);
                                    break;
                                case "switch":
                                    Started = true;
                                    ParseCommandSwitch(s, list, line);
                                    break;
                                default: //varijabla,funkcija,labela
                                    if (determinator[0] == '*')
                                    {
                                        Started = true;
                                        determinator = determinator.Remove(0, 1);
                                        ParseCommandDereferenced(FindVariable(list, determinator), s, list, line);
                                    }
                                    Variable v = new Variable();
                                    if ((v = FindVariable(list, determinator)) != null || determinator.Contains('['))
                                        ParseCommandVariable(v, s, list, line);
                                    else if (FindFunciton(Functions, determinator) != null)
                                        ParseCommandFunction(s, list, line, null);
                                    else
                                        ParseCommandLabel(s, list, line);
                                    break;
                            }
                            return true;
                        }

                        #region Parsing Command Main Functions

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
                                if (Op1.Type.IsSigned())
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
                                        Error.Stop(buffer);
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

                            private void ParseCommandReturnMain(List<Command> line)
                            {
                                Command c = new Command();
                                //c = SetLabel(c);
                                c.Touch = false;
                                c.Instruction = "HALT";
                                line.Add(c);
                            }

                            private void ParseCommandSwitch(string buffer, List<Variable> list, List<Command> line)
                            {
                                Command c = new Command();
                                //c = SetLabel(c);
                                Error.Stop("switch");
                            }

                            private void ParseCommandDereferenced(Variable Dest, string buffer, List<Variable> list, List<Command> line)
                            {
                                string op2 = EditOp2(Dest, buffer.Word(3).Trim(';'), list, line);
                                Command c = new Command();
                                //c = SetLabel(c);
                                c.Instruction = "STORE";
                                c.Arguments[0] = op2;
                                c.Arguments[1] = Dest.Name;
                                c.Change[1] = false;
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
                                Variable Op1 = null;
                                Function F = null;
                                #endregion

                                #region a = (cast) b;
                                if (op1[0] == '(')
                                {
                                    memExt=GetMemExtension(Dest.Type.Length);
                                    Started = true;

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
                                        if (!AdditionalFunctions.Contains(c.Arguments[0]))
                                            AdditionalFunctions.Add(c.Arguments[0]);
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
                                        c.Instruction = "STORE"+memExt;
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
                                    if (!Started)
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
                                        if (!AdditionalFunctions.Contains(c.Arguments[0]))
                                            AdditionalFunctions.Add(c.Arguments[0]);
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

                                switch (n)
                                {
                                    case (3):
                                        op1 = op1.TrimEnd(';');
                                        switch (op1[0])
                                        {
                                            case ('*'):
                                                #region a = *b;

                                                    Started = true;
                                                    op1 = op1.Remove(0, 1);
                                                    v = FindVariable(list, op1);
                                                    temp1 = CreateTemp(list, v);
                                                    memExt = GetMemExtension(Dest.Type.Length);
                                                    c.Instruction = "LOAD" + memExt;
                                                    c.Arguments[0] = temp1;
                                                    c.Arguments[1] = op1;
                                                    //c = SetLabel(c);
                                                    line.Add(c);
                                                    c = new Command();
                                                    c.Instruction = "STORE" + memExt;
                                                    c.Arguments[0] = temp1;
                                                    c.Arguments[1] = Dest.Name;
                                                    c.Change[1] = false;
                                                    line.Add(c);
                                                    break;

                                                #endregion
                                            case ('&'):
                                                #region a = &b;

                                                    Started = true;
                                                    op1 = op1.Remove(0, 1);
                                                    v = FindVariable(list, op1);
                                                    memExt = GetMemExtension(v.Type.Length);
                                                    c.Instruction = "STORE" + memExt;
                                                    c.Arguments[0] = op1;
                                                    c.Change[0] = false;
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

                                                    Started = true;
                                                    op1 = op1.Remove(0, 1);
                                                    c.Instruction = "MOVE";
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
                                                    c.Instruction = "DB 04, 00, 80, D7; JR_EQ (PC+4)";
                                                    c.Touch = false;
                                                    line.Add(c);
                                                    c = new Command();
                                                    c.Instruction = "MOVE";
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
                                                        if (!Started && op1[0].IsNumberPart())
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
                                                                c.Instruction = "LOAD"+memExtOp2;
                                                                //c = SetLabel(c);
                                                                c.Arguments[0] = temp1;
                                                                c.Arguments[1] = op2;
                                                                line.Add(c);

                                                                c = new Command();
                                                                c.Instruction = "STORE"+memExt;
                                                                c.Arguments[0] = temp1;
                                                                c.Arguments[1] = Dest.Name;
                                                                c.Change[1] = false;
                                                                line.Add(c);
                                                                return;
                                                            }
                                                            if (!char.IsLetter(offset[0]))
                                                            {
                                                                c.Instruction = "LOAD"+memExtOp2;
                                                                //c = SetLabel(c);
                                                                c.Arguments[0] = temp1;
                                                                c.Arguments[1] = op2;
                                                                c.MemShift = " + " + offset;
                                                                line.Add(c);

                                                                c = new Command();
                                                                c.Instruction = "STORE"+memExt;
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
                                                                c.Instruction = "LOAD"+memExtOp2;
                                                                c.Arguments[0] = temp1;
                                                                c.Arguments[1] = op2;
                                                                c.MemShift = " + " + offset;
                                                                line.Add(c);

                                                                c = new Command();
                                                                c.Instruction = "STORE"+memExt;
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
                                                        c.Instruction = "STORE"+memExt;
                                                        c.Arguments[0] = Dest.Name;
                                                        c.Arguments[1] = temp2;
                                                        c.Change[1] = true;
                                                        line.Add(c);
                                                        return;
                                                    }
                                                    break;

                                                #endregion
                                        }
                                        break;
                                    case (5):
                                        Started = true;
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
                                                            c.Instruction = "PUSH";
                                                            op1 = EditOp2(Dest, op1, list, line);
                                                            c.Arguments[0] = op1;
                                                            //c = SetLabel(c);
                                                            line.Add(c);

                                                            c = new Command();
                                                            c.Instruction = "PUSH";
                                                            op2 = EditOp2(Dest, op2, list, line);
                                                            c.Arguments[0] = op2;
                                                            line.Add(c);

                                                            c = new Command();
                                                            c.Instruction = "CALL";
                                                            c.Touch = false;
                                                            c.Arguments[0] = GetEmbeddedFunctionName(operand, Dest.Type.Type);
                                                            if (!AdditionalFunctions.Contains(c.Arguments[0]))
                                                                AdditionalFunctions.Add(c.Arguments[0]);
                                                            line.Add(c);

                                                            c = new Command();
                                                            c.Instruction = "POP";
                                                            c.Arguments[0] = Dest.Name;
                                                            c.LoadStore[0] = false;
                                                            line.Add(c);

                                                            c = new Command();
                                                            c.Instruction = "ADD";
                                                            c.Arguments[0] = c.Arguments[2] = "SP";
                                                            n = 4;
                                                            c.Arguments[1] = n.ToHex9();
                                                            c.Touch = false;
                                                            line.Add(c);
                                                        }
                                                    }

                                                #endregion

                                                #region a = &b + c;

                                                    else
                                                    {
                                                        c.Instruction = "MOVE";
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
                                                        c.Instruction = "ADD";
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
                                                        c.Instruction = "PUSH";
                                                        op1 = EditOp2(Dest, op1, list, line);
                                                        c.Arguments[0] = op1;
                                                        //c = SetLabel(c);
                                                        line.Add(c);

                                                        c = new Command();
                                                        c.Instruction = "PUSH";
                                                        op2 = EditOp2(Dest, op2, list, line);
                                                        c.Arguments[0] = op2;
                                                        line.Add(c);

                                                        c = new Command();
                                                        c.Instruction = "CALL";
                                                        c.Touch = false;
                                                        c.Arguments[0] = GetEmbeddedFunctionName(operand, Dest.Type.Type);
                                                        if (!AdditionalFunctions.Contains(c.Arguments[0]))
                                                            AdditionalFunctions.Add(c.Arguments[0]);
                                                        line.Add(c);

                                                        c = new Command();
                                                        c.Instruction = "POP";
                                                        c.Arguments[0] = Dest.Name;
                                                        c.LoadStore[0] = false;
                                                        line.Add(c);

                                                        c = new Command();
                                                        c.Instruction = "ADD";
                                                        c.Arguments[0] = c.Arguments[2] = "SP";
                                                        n = 4;
                                                        c.Arguments[1] = n.ToHex9();
                                                        c.Touch = false;
                                                        line.Add(c);
                                                    }
                                                    break;

                                                #endregion
                                            case ("*"):
                                                #region a = b * c;

                                                    c.Instruction = "PUSH";
                                                    c.Arguments[0] = op1;
                                                    //c = SetLabel(c);
                                                    line.Add(c);

                                                    c = new Command();
                                                    c.Instruction = "PUSH";
                                                    op2 = EditOp2(Dest, op2, list, line);
                                                    c.Arguments[0] = op2;
                                                    line.Add(c);

                                                    c = new Command();
                                                    c.Instruction = "CALL";
                                                    c.Touch = false;
                                                    c.Arguments[0] = GetEmbeddedFunctionName(operand, Dest.Type.Type);
                                                    if (!AdditionalFunctions.Contains(c.Arguments[0]))
                                                        AdditionalFunctions.Add(c.Arguments[0]);
                                                    line.Add(c);

                                                    c = new Command();
                                                    c.Instruction = "POP";
                                                    c.Arguments[0] = Dest.Name;
                                                    c.LoadStore[0] = false;
                                                    line.Add(c);

                                                    c = new Command();
                                                    c.Instruction = "ADD";
                                                    c.Arguments[0] = c.Arguments[2] = "SP";
                                                    n = 4;
                                                    c.Arguments[1] = n.ToHex9();
                                                    c.Touch = false;
                                                    line.Add(c);
                                                    break;

                                                #endregion
                                            case ("/"):
                                                #region a = b / c;

                                                    c.Instruction = "PUSH";
                                                    op1 = EditOp2(Dest, op1, list, line);
                                                    c.Arguments[0] = op1;
                                                    //c = SetLabel(c);
                                                    line.Add(c);

                                                    c = new Command();
                                                    c.Instruction = "PUSH";
                                                    op2 = EditOp2(Dest, op2, list, line);
                                                    c.Arguments[0] = op2;
                                                    line.Add(c);

                                                    c = new Command();
                                                    c.Instruction = "CALL";
                                                    c.Touch = false;
                                                    c.Arguments[0] = GetEmbeddedFunctionName(operand, Dest.Type.Type);
                                                    if (!AdditionalFunctions.Contains(c.Arguments[0]))
                                                        AdditionalFunctions.Add(c.Arguments[0]);
                                                    line.Add(c);

                                                    c = new Command();
                                                    c.Instruction = "POP";
                                                    c.Arguments[0] = Dest.Name;
                                                    c.LoadStore[0] = false;
                                                    line.Add(c);

                                                    c = new Command();
                                                    c.Instruction = "ADD";
                                                    c.Arguments[0] = c.Arguments[2] = "SP";
                                                    n = 4;
                                                    c.Arguments[1] = n.ToHex9();
                                                    c.Touch = false;
                                                    line.Add(c);
                                                    break;

                                                #endregion
                                            case ("%"):
                                                #region a = b % c;

                                                    c.Instruction = "PUSH";
                                                    op1 = EditOp2(Dest, op1, list, line);
                                                    c.Arguments[0] = op1;
                                                    //c = SetLabel(c);
                                                    line.Add(c);

                                                    c = new Command();
                                                    c.Instruction = "PUSH";
                                                    op2 = EditOp2(Dest, op2, list, line);
                                                    c.Arguments[0] = op2;
                                                    line.Add(c);

                                                    c = new Command();
                                                    c.Instruction = "CALL";
                                                    c.Touch = false;
                                                    c.Arguments[0] = GetEmbeddedFunctionName(operand, Dest.Type.Type);
                                                    if(!AdditionalFunctions.Contains(c.Arguments[0]))
                                                        AdditionalFunctions.Add(c.Arguments[0]);
                                                    line.Add(c);

                                                    c = new Command();
                                                    c.Instruction = "POP";
                                                    c.Arguments[0] = Dest.Name;
                                                    c.LoadStore[0] = false;
                                                    line.Add(c);
                                                    c = new Command();
                                                    c.Instruction = "ADD";
                                                    c.Arguments[0] = c.Arguments[2] = "SP";
                                                    n = 4;
                                                    c.Arguments[1] = n.ToHex9();
                                                    c.Touch = false;
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

                                                    op1 = op1.Remove(0, 1);
                                                    c.Instruction = "MOVE";
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
                                                    c.Instruction = "DB 04, 00, 80, D7; JR_EQ (PC+4)";
                                                    c.Touch = false;
                                                    line.Add(c);
                                                    c = new Command();
                                                    c.Instruction = "MOVE";
                                                    c.Arguments[0] = n.ToHex9();
                                                    c.Change[0] = false;
                                                    c.Arguments[1] = Dest.Name;
                                                    line.Add(c);
                                                    break;

                                                #endregion
                                            case ("<="):
                                                #region a = b <= c;

                                                    op1 = op1.Remove(0, 1);
                                                    c.Instruction = "MOVE";
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
                                                        c.Instruction = "DB 04, 00, 00, D7; JR_SLE (PC+4)";
                                                    else
                                                        c.Instruction = "DB 04, 00, 40, D6; JR_ULE (PC+4)";
                                                    c.Touch = false;
                                                    line.Add(c);
                                                    c = new Command();
                                                    c.Instruction = "MOVE";
                                                    c.Arguments[0] = n.ToHex9();
                                                    c.Change[0] = false;
                                                    c.Arguments[1] = Dest.Name;
                                                    line.Add(c);
                                                    break;

                                                #endregion
                                            case (">="):
                                                #region a = b >= c;

                                                    op1 = op1.Remove(0, 1);
                                                    c.Instruction = "MOVE";
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
                                                        c.Instruction = "DB 04, 00, 40, D7; JR_SGE (PC+4)";
                                                    else
                                                        c.Instruction = "DB 04, 00, C0, D5; JR_UGE (PC+4)";
                                                    c.Touch = false;
                                                    line.Add(c);
                                                    c = new Command();
                                                    c.Instruction = "MOVE";
                                                    c.Arguments[0] = n.ToHex9();
                                                    c.Change[0] = false;
                                                    c.Arguments[1] = Dest.Name;
                                                    line.Add(c);
                                                    break;

                                                #endregion
                                            case ("<"):
                                                #region a = b < c;

                                                    op1 = op1.Remove(0, 1);
                                                    c.Instruction = "MOVE";
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
                                                        c.Instruction = "DB 04, 00, C0, D6; JR_SLT (PC+4)";
                                                    else
                                                        c.Instruction = "DB 04, 00, C0, D4; JR_ULT (PC+4)";
                                                    c.Touch = false;
                                                    line.Add(c);
                                                    c = new Command();
                                                    c.Instruction = "MOVE";
                                                    c.Arguments[0] = n.ToHex9();
                                                    c.Change[0] = false;
                                                    c.Arguments[1] = Dest.Name;
                                                    line.Add(c);
                                                    break;

                                                #endregion
                                            case (">"):
                                                #region a = b > c;

                                                    op1 = op1.Remove(0, 1);
                                                    c.Instruction = "MOVE";
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
                                                        c.Instruction = "DB 04, 00, 80, D7; JR_SGT (PC+4)";
                                                    else
                                                        c.Instruction = "DB 04, 00, 80, D6; JR_UGT (PC+4)";
                                                    c.Touch = false;
                                                    line.Add(c);
                                                    c = new Command();
                                                    c.Instruction = "MOVE";
                                                    c.Arguments[0] = n.ToHex9();
                                                    c.Change[0] = false;
                                                    c.Arguments[1] = Dest.Name;
                                                    line.Add(c);
                                                    break;

                                                #endregion
                                            case ("!="):
                                                #region a = b != c;

                                                    op1 = op1.Remove(0, 1);
                                                    c.Instruction = "MOVE";
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
                                                    c.Instruction = "DB 04, 00, 00, D6 ; JR_NE (PC+4)";
                                                    c.Touch = false;
                                                    line.Add(c);
                                                    c = new Command();
                                                    c.Instruction = "MOVE";
                                                    c.Arguments[0] = n.ToHex9();
                                                    c.Change[0] = false;
                                                    c.Arguments[1] = Dest.Name;
                                                    line.Add(c);
                                                    break;

                                                #endregion
                                            default:
                                                #region a = f (a, ... ,n);
                                                Started = true;
                                                F = FindFunciton(Functions, op1);
                                                ParseCommandFunction(buffer.Remove(0, buffer.IndexOf("=") + 2), list, line, Dest.Name);
                                                break;
                                        }
                                        break;
                                    default:
                                        Started = true;
                                        F = FindFunciton(Functions, op1);
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

                                if (!Started)
                                {
                                    if (offset.IsNumber() && op2.IsNumber() && Dest.ArrayLength() > int.Parse(offset))
                                    {
                                        //Dest.NonNullArray();
                                        Dest.ArrayValues[int.Parse(offset)] = int.Parse(op2).ToHex9();
                                        Dest.ArraySet[int.Parse(offset)] = true;
                                        return;
                                    }
                                    else
                                    {
                                        Started = false;
                                    }
                                }

                                op2 = EditOp2(Dest, op2, list, line);

                                if (!char.IsLetter(offset[0]))
                                {
                                    offset = CreateConstOffset(offset, list, Dest);
                                    if (offset.Equals("000000000"))
                                    {
                                        //NULA
                                        c.Instruction = "STORE"+memExt;
                                        //c = SetLabel(c);
                                        c.Arguments[0] = op2;
                                        c.Arguments[1] = Dest.Name;
                                        line.Add(c);
                                        return;
                                    }
                                    if (!char.IsLetter(offset[0]))
                                    {
                                        c.Instruction = "STORE"+memExt;
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
                                        c.Instruction = "STORE"+memExt;
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
                                c.Instruction = "STORE"+memExt;
                                c.Arguments[0] = op2;
                                c.Arguments[1] = temp;
                                line.Add(c);
                                return;
                            }

                            private void ParseCommandFunction(string buffer, List<Variable> list, List<Command> line, string result)
                            {
                                Command c = new Command();
                                Started = true;

                                Function f = FindFunciton(Functions, buffer.FirstWord());
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
                                        //c = SetLabel(c);
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
                                        c.Instruction = "STORE"+memExt;
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
                                    if(cnt!=0)
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
                                        if(cnt!=0)
                                            line.Add(c);
                                    }
                                    else
                                    {
                                        c = new Command();
                                        c.Instruction = "POP";
                                        c.Arguments[0] = result;
                                        c.LoadStore[0] = false;
                                        line.Add(c);
                                        c = new Command();
                                        c.Instruction = "ADD";
                                        c.Arguments[0] = c.Arguments[2] = "SP";
                                        c.Touch = false;
                                        c.Arguments[1] = (cnt - 4).ToHex9();
                                        if(cnt-4!=0)
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
                                Started = true;
                                Command c = new Command();
                                c.Label=buffer.Trim(':', '<', '>');
                                c.Instruction = "";
                                line.Add(c);
                            }

                        #endregion

                    #endregion

                    #region Convert To Registers

                        #region Variables

                            private int currLine = 0;

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
                            FillWithRegList(line, regList,list);
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
                                            if ((line.Instruction != null && (line.Instruction.Contains("JR")||line.Instruction.Contains("RET"))) || line.Label != null)
                                                break;
                                        } while (!line.Touch || line.MemReg);
                                        if ((line.Instruction != null && (line.Instruction.Contains("JR")||line.Instruction.Contains("RET")))||line.Label!=null)
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
                                    if (c.Instruction.Contains("JR")||c.Instruction.Contains("RET")||c.Label!=null)
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
                                thisRegister.LoadNow = true;
                                thisRegister.SaveNow = true;
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
                                        thisRegister.AddLoadSave(foundRegister);
                                        foundRegister.Edited = true;
                                        lastRegister.SaveNow = false;
                                        lastRegister = foundRegister;
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

                                for (int j = idx + 1; j < i; j++)
                                {
                                    foundRegister = list[j];
                                    if (foundRegister.VarName.Equals(thisRegister.VarName))
                                    {
                                        foundRegister.Load = thisRegister.Load;
                                        foundRegister.Save = thisRegister.Save;
                                    }
                                }

                                return true;
                            }


                            private List<Register> AddRegData(List<Register> regList)
                            {
                                Register r = new Register();
                                FreeRegs freeRegs = new FreeRegs();
                                List<Register> ret = new List<Register>();
                                string[] usedList = new string[Processor.NoOfRegisters];
                                int reg;
                                for (int i = 0; i < regList.Count; i++)
                                {
                                    if (regList[i].VarName.Equals(""))
                                    {
                                        ret.Add(regList[i]);
                                        freeRegs = new FreeRegs();
                                        continue;
                                    }
                                    if (regList[i].LoadNow)
                                    {
                                        reg = freeRegs.Get();
                                        usedList[reg] = regList[i].VarName;
                                        regList[i].RegName = reg;
                                        if (!regList[i].Load)
                                            regList[i].LoadNow = false;
                                        if (regList[i].SaveNow)
                                        {
                                            freeRegs.Add(reg);
                                            if (!regList[i].Save)
                                                regList[i].SaveNow = false;
                                        }
                                        else
                                        {

                                        }
                                    }
                                    else
                                    {
                                        if ((reg = Array.IndexOf<string>(usedList, regList[i].VarName)) >= 0)
                                            regList[i].RegName = reg;
                                        else
                                            Error.Stop("Error while turning into reg's");
                                        if (regList[i].SaveNow)
                                        {
                                            freeRegs.Add(reg);
                                            regList[reg] = null;
                                            if (!regList[i].Save)
                                                regList[i].SaveNow = false;
                                        }
                                    }
                                    ret.Add(regList[i]);
                                }
                                return ret;
                            }

                            private List<Command> AddLoad(List<Command> lines, Register reg, List<Variable> list)
                            {
                                if (reg.VarName.Contains("temp."))
                                    return lines;
                                Command c = new Command();
                                Variable v = FindVariable(list, reg.VarName);
                                c.Instruction = "LOAD"+GetMemExtension(v.Type.Length);
                                c.MemReg = true;
                                c.Change[1] = false;
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
                                c.LoadStore[2] = true;
                                c.MemReg = true;
                                lines.Insert(currLine++, c);

                                c = new Command();
                                if (v.Type.IsSigned())
                                    c.Instruction = "ASHR";
                                else
                                    c.Instruction = "SHR";
                                c.Arguments[0] = c.Arguments[2] = "R" + reg.RegName.ToString();
                                c.Arguments[1] = n.ToHex9();
                                c.LoadStore[2] = true;
                                c.MemReg = true;
                                lines.Insert(currLine++, c);
                                if (MatchingLinesRefresh(lines[currLine - 2], lines[currLine]) && MatchingLinesRefresh(lines[currLine - 1], lines[currLine+1]))
                                {
                                    lines.RemoveAt(currLine-2);
                                    lines.RemoveAt(currLine-2);
                                    currLine -= 2;
                                }
                                
                                return lines;
                            }

                            private List<Command> AddSave(List<Command> lines, Register reg, List<Variable> list)
                            {
                                if (reg.VarName.Contains("temp.") || reg.VarName.Contains("const."))
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
                                string name;
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
                                    name = c.Arguments[currArg];
                                    if (regList[i].LoadNow == true)
                                        lines = AddLoad(lines, regList[i],list);
                                    if (regList[i].SaveNow == true)
                                        lines = AddSave(lines, regList[i],list);
                                    c.Arguments[currArg] = "R" + regList[i].RegName.ToString();
                                    if (regList[i].RegName > regUsed)
                                        regUsed = regList[i].RegName;
                                }

                            }

                        #endregion

                    #endregion

                    #region Add Initial Variable Values

                        private void AddMainVarsAndConsts(List<Variable> list, List<Command> line)
                        {
                            Command c=null;
                            foreach (Variable var in list)
                            {
                                if (var.IsTemp)
                                    continue;
                                if (var.IsArray)
                                {
                                    AddMainArray(var, line);
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
                                        Error.Stop("unknown length");
                                        break;
                                }
                                c.Arguments[0] = var.Value;
                                line.Add(c);
                            }
                        }
                    
                        private void AddMainArray(Variable var, List<Command> line)
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
                                c.Instruction = "DS";
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
                                    if (c.MemShift == null||c.MemShift.Length<=0)
                                        c.MemShift = Cwords.Zero;
                                    line.Add(c);
                                }
                            }

                    }

                    #endregion

                    private void FinishParsingMain(List<Variable> list, List<Command> line)
                        {
                            foreach (Command c in line)
                            {
                                if (c.Instruction.Equals("DW") || c.Instruction.Equals("DB") || c.Instruction.Equals("DH"))
                                {
                                    if (c.Label != null)
                                        c.Label = GetNewVarName(list, c.Label, "MAINV");
                                    if (c.Arguments[0] != null)
                                        c.Arguments[0] = c.Arguments[0].Replace(".", "");
                                }
                                else if (c.Label != null)
                                    c.Label = c.Label.Replace(".", "");
                                if (c.Instruction.Contains("JR"))
                                {
                                    c.Arguments[0] = c.Arguments[0].Replace(".", "");
                                }
                                else if (c.Instruction.Equals("CALL"))
                                { 
                                    if(!EmbeddedFunctions.Contains(c.Arguments[0]))
                                    {
                                        c.Arguments[0]=GetNewFunctName(c.Arguments[0]);
                                    }
                                    else
                                    {
                                        if (!EmbeddedFunctionsFound.Contains(c.Arguments[0]))
                                            EmbeddedFunctionsFound.Add(c.Arguments[0]);
                                    }
                                }
                                else
                                {
                                    if (c.Arguments[0] != null && !c.Arguments[0].IsNumber())
                                        c.Arguments[0] = GetNewVarName(list, c.Arguments[0], "MAINV");
                                    if (c.Arguments[1] != null && !c.Arguments[1].IsNumber())
                                        c.Arguments[1] = GetNewVarName(list, c.Arguments[1], "MAINV");
                                    if (c.Arguments[2] != null && !c.Arguments[2].IsNumber())
                                        c.Arguments[2] = GetNewVarName(list, c.Arguments[2], "MAINV");
                                }
                            }
                        }

                    #region Const and Temp

                        #region Variables

                            private int NoTemp = 0;

                            private bool LockTemp = false;
                                
                            private int NoConst = 0;

                        #endregion

                        private string EditOp2(Variable Dest, string op2, List<Variable> list, List<Command> line)
                        {
                            string name = CreateConst(op2, list, Dest);
                            if (name[0].IsNumberPart())
                            {
                                Command c = new Command();
                                //c = SetLabel(c);
                                c.Instruction = "MOVE";
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
                                    v.Name=GetConstName(v.Value);
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
                                return "const." + val.Remove(0, 5);
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
                                return "temp." + NoTemp++.ToString();
                            }

                        #endregion

                    #endregion

                #endregion

            #endregion

            #region Parsing Functions

                private List<Command> ParseFunction(Function f)
                {
                    List<Variable> VariablesDeclared = new List<Variable>(f.Parameters);
                    List<Command> Commands = new List<Command>();

                    AddFunctName(Commands,f.Name); 

                    PreParsingMain(VariablesDeclared, f.Name);
                    using (StreamReader FunctionStream = FindFunction(f))
                    {

                        while (ParseDeclaration(FunctionStream, VariablesDeclared)) ;

                        Started = false;

                        while (ParseCommandFunction(FunctionStream, VariablesDeclared, Commands))
                        {

                        }
                    }
                    Print(Commands);
                    //Print(VariablesDeclared);
                   //ReduceJumps(Commands);

                    regUsed = -1; 
                    ConvertToRegisters(Commands, VariablesDeclared);
                    //PrintBlank(Commands);
                    ConvertParameters(Commands,f,VariablesDeclared);
                    //PrintBlank(Commands);
                    //Console.WriteLine(regUsed);
                    FillWithPushPulls(Commands,VariablesDeclared);

                    FinishParsingFunct(VariablesDeclared, Commands);
                    PrintBlank(Commands);
                    Print(VariablesDeclared);
                    return Commands;
                }

                #region Parsing Command Functions

                    private void AddFunctName(List<Command> line,string name)
                    {
                        Command c = new Command();
                        c.Label = name;
                        c.Instruction = "";
                        line.Add(c);
                    }

                    private StreamReader FindFunction(Function f)
                    {
                        StreamReader Finder = new StreamReader(InputFile);
                        while (!Finder.ReadLine().StartsWith(f.Name)) ;
                        Finder.ReadLine();
                        return Finder;
                    }

                    private bool ParseCommandFunction(StreamReader read, List<Variable> list, List<Command> line)
                    {
                        string s = read.ReadLine();
                        if (s.Length <= 1)
                            return false;
                       /* Command c = new Command();
                         c.Label = s;
                         c.Touch = false;
                         line.Add(c);
                        Console.Clear();
                        PrintBlank(line);
                        Print(list);
                        Console.ReadLine();*/
                        
                        s = s.TrimStart();
                        /*   if (line.Count > 0)
                           {
                               Console.Clear();
                               Print(line);
                           }*/
                        string determinator = s.FirstWord().TrimEnd(';');
                        switch (determinator)
                        {
                            case "goto":
                                Started = true;
                                ParseCommandGoto(s, line);
                                break;
                            case "if":
                                Started = true;
                                ParseCommandIf(s, list, line);
                                break;
                            case "return":
                                Started = true;
                                ParseCommandReturnFunction(FindVariable(list,s.LastWord().TrimEnd(';')),line);
                                break;
                            case "switch":
                                Started = true;
                                ParseCommandSwitch(s, list, line);
                                break;
                            default: //varijabla,funkcija,labela
                                if (determinator[0] == '*')
                                {
                                    Started = true;
                                    determinator = determinator.Remove(0, 1);
                                    ParseCommandDereferenced(FindVariable(list, determinator), s, list, line);
                                }
                                Variable v = new Variable();
                                if ((v = FindVariable(list, determinator)) != null || determinator.Contains('['))
                                    ParseCommandVariable(v, s, list, line);
                                else if (FindFunciton(Functions, determinator) != null)
                                    ParseCommandFunction(s, list, line, null);
                                else
                                    ParseCommandLabel(s, list, line);
                                break;
                        }
                        return true;
                    }

                    #region Parsing Command Function Functions

                        private void ParseCommandReturnFunction(Variable var, List<Command> line)
                        {
                            Command c = new Command();
                            //c = SetLabel(c);
                            c.Instruction = "STORE";
                            c.Arguments[0] = var.Name;
                            c.Arguments[1] = Processor.FunctResult;
                            c.Change[1] = false;
                            line.Add(c);

                            c = new Command();            
                            c.Touch = false;
                            c.Instruction = "RET";
                            line.Add(c);
                        }

                    #endregion

                #endregion


                private string GetStackPosition(string s, List<Variable> list, List<string> pos)
                {
                    int i;
                    for (i = 0; i < list.Count; i++)
                    {
                        if (list[i].Name.Equals(s))
                            break;
                    }
                    if (i >= list.Count)
                        return s;
                    return pos[i];
                }

                #region Add Initial Variable Values

                    private void AddFunctVarsAndConsts(List<Variable> list, List<Command> line,List<string> positions,int set)
                    {
                        Command c;
                        int i;
                        for(i=0;i<list.Count;i++)
                        {
                            //if (set > i)
                            //    continue;
                            if (list[i].IsTemp)
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
                                    c.Arguments[1] = "SP + " + n.ToHex9();
                                    n += memLength;
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

                private void ConvertParameters(List<Command> line, Function f, List<Variable> list)
                {
                    List<string> positions=new List<string>();
                    int i = list.Count - 1;
                    int offset = 0;
                    int set = 0;
                    int SPShift = 0;
                    int retStoreCalc = 0;
                    bool noParam = false;
                    string loc="";
                    if (f.Parameters.Count <= 0)
                        noParam = true;
                    else
                        loc =f.Parameters[f.Parameters.Count-1].Name;
                    while (i>=0)
                    {
                        if (list[i].IsTemp)
                        {
                            positions.Insert(0, " ");
                            i--;
                            continue;
                        }
                        if (!noParam&&loc.Equals(list[i].Name))
                        {
                            while (offset % 4 != 0)
                                offset++;
                            SPShift = offset;
                            if (regUsed > 1)
                                offset += 4 + (regUsed+1)*4;
                            else
                                offset += 4 + 2 * 4;
                            retStoreCalc = offset;
                            set = i+1;
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
                            int iii = list[i].Type.GetRootLength();
                            int jjj = list[i].ArrayLength();
                            offset += list[i].ArrayLength()*list[i].Type.GetRootLength();
                        }
                        offset += list[i].Type.Length;
                        i--;
                    }
                    if (retStoreCalc == 0)
                    {
                        retStoreCalc = offset + 4;
                    }
                    if (noParam)
                        SPShift = offset;

                    Command cs = new Command();
                    if (line.Count<=0||!line.Last().Instruction.Contains("RET"))
                    {
                        cs.Instruction = "RET";
                        line.Add(cs);
                    }

                    if (set <= list.Count-1)                     
                        AddFunctVarsAndConsts(list, line,positions,set);
                    if (SPShift != 0)
                    {
                        cs = new Command();
                        cs.Instruction = "SUB";
                        cs.Arguments[0] = cs.Arguments[2] = "SP";
                        cs.Arguments[1] = SPShift.ToHex9();
                        line.Insert(0, cs);
                    }
                    for(i=0;i<line.Count;i++)
                    {

                        if (line[i].Instruction.Equals("") || line[i].Instruction == null)
                            continue;
                        if (line[i].Instruction.Contains("RET")&&SPShift!=0)
                        {
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
                                        line[i].Arguments[1] = "SP + " + retStoreCalc.ToHex9();
                                    else
                                        line[i].Arguments[1] = GetStackPosition(line[i].Arguments[1], list, positions);
                                }
                            }
                        }
                    }
                }

                private void FillWithPushPulls(List<Command> line,List<Variable> list)
                {
                    if (regUsed == -1)
                    {
                        for (int i = 0; i < line.Count && i < 3; i++)
                        {
                            if(line[i].Arguments[0]!=null&&line[i].Arguments[0].Equals(Processor.RegisterNames[0]))
                                regUsed=0;
                            else if (line[i].Arguments[0] != null && line[i].Arguments[0].Equals(Processor.RegisterNames[1]))
                            {    
                                regUsed=1;
                                break;
                            }
                        }
                        if(regUsed==-1)
                            return;
                    }
                    Command c=new Command();
                    for (int i = regUsed; i >= 0; i--)
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
                            for (int i = regUsed; i >= 0; i--)
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

                private void FinishParsingFunct(List<Variable> list, List<Command> line)
                {
                    foreach (Command c in line)
                    {
                        if (c.Label != null)
                            c.Label = c.Label.Replace(".", "");
                        if (c.Instruction.Contains("JR"))
                        {
                            c.Arguments[0] = c.Arguments[0].Replace(".", "");
                        }
                        else if (c.Instruction.Equals("CALL"))
                        {
                            if (!EmbeddedFunctions.Contains(c.Arguments[0]))
                            {
                                c.Arguments[0] = GetNewFunctName(c.Arguments[0]);
                            }
                            else
                            {
                                if (!EmbeddedFunctionsFound.Contains(c.Arguments[0]))
                                    EmbeddedFunctionsFound.Add(c.Arguments[0]);
                            }
                        }
                    }
                }

            #endregion

        #endregion

        private string AddAdditionalFunctions(string input)
        {
            string output="";
            string standardStart = "\t\tMOVE " + (Processor.MemoryAvailable+1).ToHex9() + ", SP\n\t\tJP MAIN\n;DW InitIRQ\n;JP InitFIQ\n";
            
            if(!Directory.Exists(AdditionalFunctionsPath))
                Error.Stop("AdditionalFunctions Directory missing");
            foreach (string AdditionalFunction in AdditionalFunctions)
            {
                if (File.Exists(Path.Combine(AdditionalFunctionsPath, AdditionalFunction) + ".a"))
                {
                    using (StreamReader s = new StreamReader(Path.Combine(AdditionalFunctionsPath, AdditionalFunction)+".a"))
                    {
                        output += s.ReadToEnd();
                    }
                }
                else
                {
                    File.Create(Path.Combine(AdditionalFunctionsPath, AdditionalFunction) + ".a");
                    Error.Stop(AdditionalFunction + " not written yet.");
                }
            }

            string buffer;
            foreach (string EmbeddedFunction in EmbeddedFunctionsFound)
            {
                buffer = "";
                if (File.Exists(Path.Combine(AdditionalFunctionsPath, EmbeddedFunction + ".a")))
                {
                    using (StreamReader s = new StreamReader(Path.Combine(AdditionalFunctionsPath, EmbeddedFunction) + ".a"))
                    {
                        buffer = s.ReadToEnd();
                    }
                    input = input.Replace(EmbeddedFunction + "  \n\t\tRET", buffer);
                }
                else
                {
                    if (EmbeddedFunction == "InitIRQ")
                    {
                        standardStart = standardStart.Replace(";DW InitIRQ", "\t\tDW InitIRQ");
                        input = input.Replace(EmbeddedFunction + "  \n\t\tRET", buffer);
                        output += GenerateIRQCode();
                        input=input.Insert(input.LastIndexOf("\t\tHALT"), "MAINLOOP JP MAINLOOP\n");                       
                    }
                    else if (EmbeddedFunction == "InitFIQ")
                    {
                        standardStart = standardStart.Replace(";JP", "\t\tJP");
                        input = input.Replace(EmbeddedFunction + "  \n\t\tRET", buffer);
                        output += GenerateFIQCode();
                        input=input.Insert(input.LastIndexOf("\t\tHALT"), "MAINLOOP JP MAINLOOP\n");   
                    }
                    else
                    {
                    }
                }
                
            }

            input=input.Insert(0,output);
            input = input.Insert(0, standardStart);

            return input;
        }

        private string GenerateIRQCode()
        {
            return File.ReadAllText(Path.Combine(AdditionalFunctionsPath, "IRQ.a"));
        }

        private string GenerateFIQCode()
        {
            return File.ReadAllText(Path.Combine(AdditionalFunctionsPath, "FIQ.a"));
        }

        #region Additional Functions

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
                        Error.Stop("Var Length");
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
                c.LoadStore[2] = true;
                c.Change[1] = false;
                //c = SetLabel(c);
                line.Add(c);
                c = new Command();
                if (var.Type.Type.IsSigned())
                    c.Instruction = "ASHR";
                else
                    c.Instruction = "SHR";
                c.Arguments[0] = c.Arguments[2] = var.Name;
                c.LoadStore[2] = true;
                c.Arguments[1] = shift.ToHex9();
                c.Change[1] = false;
                //c = SetLabel(c);
                line.Add(c);
            }
    
            private string GetNewVarName(List<Variable> list, string name, string prefix)
            {
                if (list == null)
                    return null;
                int i;
                for (i = 0; i < list.Count; i++)
                {
                    if (list[i].Name.Equals(name))
                        return prefix + String.Format("{0:X5}", i);
                }
                return name;
            }

            private string GetNewFunctName(string name)
            {
                if (Functions == null)
                    return null;
                int i;
                for (i = 0; i < Functions.Count; i++)
                {
                    if (Functions[i].Name.Equals(name))
                        return String.Format("{0}{1:X4}","F", i);
                }
                return name;
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
