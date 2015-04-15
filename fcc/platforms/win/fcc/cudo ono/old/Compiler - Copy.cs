using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Compiler_4_0_lite;
using ExtensionMethods;

namespace fcc
{
    class Compiler
    {
        private const bool IspraviFRISC3 = true;

        #region Variables

            public static string InputFile = null;

            public static string Starter = "\t\tMOVE 1000,SP\n\t\tJR _start\n;\t\tDW IRQ_start\n;\t\tJR FIQ_start\n_start\n\t\tSUB SP,4,SP\n\t\tCALL main\n\t\tADD SP,4,SP\n\t\tHALT\n";

            private static readonly List<string> AdditionalFunctionList = new List<string>();

            private static List<Function> _functionList = new List<Function>();

            public static List<string> Headers = new List<string>();

            private static bool _started;

            private static int _regUsed = -1;

            public static string AdditionalFunctionPath = @"data\predefined\functions\additional";

            public static string EmbeddedFunctionsPath = @"data\predefined\functions\built-in";

            private static List<string> _embeddedFunctionList = new List<string>();

            private static readonly List<string> EmbeddedFunctionFoundList = new List<string>();

            private static readonly List<Variable> GlobalVariableDeclaredList = new List<Variable>();

        #endregion

        #region Print

            static private void Print(IEnumerable<Command> list)
            {
                Console.WriteLine();
                Console.WriteLine("Sporedni ispis:");
                Console.WriteLine();
                foreach (var c in list)
                {
                    Print(c);

                 //   if (i++ % 100 == 0)
                 //       Console.ReadKey(true);
                }
                Console.WriteLine();
            }

            static public void PrintBlank(List<Command> list)
            {
     

                Console.WriteLine();
                Console.WriteLine("Glavni ispis:");
                Console.WriteLine();
                foreach (var c in list)
                {
                    PrintBlank(c);
                }
            }

           static public void PrintToStream(List<Command> list)
            {
                var stream = new StreamWriter("mate.a");
                foreach (var c in list)
                {
                    PrintToStream(c,stream);
                }
                stream.Close();
            }

            static private void Print(Command c)
            {
                if (c.Instruction == null)
                {
                    Console.BackgroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine("-------------------------------------------------------------------------------");
                    Console.BackgroundColor = ConsoleColor.Black;
                }
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("{0,-16}", c.Label ?? "");
                Console.Write("{0,-16}", c.Instruction);
                for (var i = 0; i < c.Arguments.Count(); i++)
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
                    if (c.Instruction != null && ((c.Instruction.Contains("STORE") || c.Instruction.Contains("LOAD")) && i == 1))
                        s = "( " + c.Arguments[i] + c.MemShift + " )";
                    else if (c.Instruction != null && (c.Instruction.Contains("DB") && i == 2))
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

           static private void PrintBlank(Command c)
            {
                if (c.Instruction == null)//|| c.Instruction.Contains(':') || c.Instruction.Contains(';'))
                    return;
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("{0,-16}", (c.Label == null) ? "" : c.Label.Replace(".",""));
                Console.Write("{0,-16}", c.Instruction);
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
                for (var i = 0; i < c.Arguments.Count(); i++)
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

            static private void PrintToStream(Command c,StreamWriter stream)
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
                for (var i = 0; i < c.Arguments.Count(); i++)
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

            static private void Print(IEnumerable<Function> list)
            {
                Console.WriteLine();
                Console.WriteLine("Ispis funkcija i broja parametara:");
                Console.WriteLine();
                foreach (var c in list)
                {
                    if (c.Name != null)
                        Console.WriteLine(c.Name + " " + c.NoOfParameters);
                }
                Console.WriteLine();
            }

           static private void Print(IEnumerable<Variable> list)
            {
                Console.WriteLine();
                Console.WriteLine("Ispis varijabli:");
                Console.BackgroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("{0,-12}|{1,-16}|{2,-10}|{3,1}|{4,5}|{5,5}|{6,5}|{7}", "Name", "Type", "Deref Type", "B", "Array", "Temp", "Const", "Value    ");
                Console.BackgroundColor = ConsoleColor.Black;
                foreach (var c in list)
                {
                    Print(c);
                }
                Console.WriteLine();
            }

            static private void Print(IEnumerable<Register> list)
            {
                if (list == null)
                    return;
                Console.WriteLine("{0,14}|{1,5}|{2,5}|{3,5}|{4,5}", "Name:", "Load!", "Save!", "Save", "Edit");
                for (var i = 0; i < 46; i++)
                    Console.Write("-");
                Console.WriteLine();
                foreach (var c in list)
                {
                    if (c.VarName != null)
                        Console.WriteLine("{0,14}|{1,5}|{2,5}|{3,5}|{4,5}|{5,1}", c.VarName, c.LoadNow, c.SaveNow, c.Save, c.Edited, c.RegName);
                }
                for (var i = 0; i < 46; i++)
                    Console.Write("-");
                Console.WriteLine();
                for (var i = 0; i < 46; i++)
                    Console.Write("-");
                Console.WriteLine();
            }

           static private void Print(Variable v)
            {
                Console.Write("{0,-12}|{1,-16}|{2,-10}|{3,1}|{4,5}|{5,5}|{6,5}|{7}", v.Name, v.Type.Type, v.Type.RootType, v.Type.Length, v.IsArray, v.IsTemp, v.IsConst, v.Value);
                Console.WriteLine();
            }

            private static void PrintAdditionalFuncts(ICollection<string> list)
            {
                if (list.Count <= 0)
                    return;
                foreach (var s in list)
                {
                    Console.WriteLine(s);
                }
            }

            static private string ConvertToString(IEnumerable<Command> list)
            {
                return list.Aggregate("", (current, c) => current + ConvertToString(c));
            }

        static string ConvertToString(Command c)
            {
                var output = "";
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
                for (var i = 0; i < c.Arguments.Count(); i++)
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

            var parserOutputRaw = new List<Command>(Parse());

            AddGlobalVarsAndConsts(GlobalVariableDeclaredList, parserOutputRaw);

            //PrintBlank(parserOutputRaw);

            var parserOutput = ConvertToString(parserOutputRaw);

            //PrintAdditionalFuncts(AdditionalFunctions);

            //parserOutput = AddAdditionalFunctions(parserOutput);

            File.WriteAllText(destinationPath, parserOutput);
        }

        private static void GlobalParse()
        {
            ParseFunctionPrototypes();

            var f = _functionList[0];
            
            PreParsing(GlobalVariableDeclaredList, f.Name);

            var commands = new List<Command>();

            var i=GlobalVariableDeclaredList.Count;

            using (var functionStream = FindFunction(f))
            {
                while (true)
                {
                    if(!ParseDeclaration(functionStream, GlobalVariableDeclaredList)) 
                        break;
                }
                

                _started = false;

                while (true)
                {
                    if (!ParseCommands(functionStream, GlobalVariableDeclaredList, commands))
                        break;
                }
            }

            if (i < GlobalVariableDeclaredList.Count)
                GlobalVariableDeclaredList.RemoveAt(i);
            
            i = 0;
            while (i < GlobalVariableDeclaredList.Count)
            {
                for (; i < GlobalVariableDeclaredList.Count; i++)
                {
                    Variable v;
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

            foreach (var t in GlobalVariableDeclaredList)
                t.IsGlobal = true;
        }

        private static void AnalyzeSource(string sourcePath)
        {
            var s = File.ReadAllText(sourcePath);
            var s1 = s;
            foreach (var c in Cwords.unwanted.Where(c => System.Text.RegularExpressions.Regex.IsMatch(s1,string.Format(@"\b{0}\b",System.Text.RegularExpressions.Regex.Escape(c)))))
            {
                Error.printError("Using: "+c,1,false);
                return;
            }
            var idx1=-1;
            while ((idx1 = s.IndexOf('{', idx1 + 3)) >= 0)
            {
                var idx2 = s.IndexOf('}', idx1);
                if (!s.Substring(idx1, idx2 - idx1 + 1).Contains(Environment.NewLine + Environment.NewLine))
                    s=s.Insert(idx1 + 1, Environment.NewLine);
            }
            s=s.Replace(Cwords.Sizetype, Cwords.Pointer);
            File.WriteAllText(sourcePath, s);
        }

        private static void InitializeParser()
        {
            _embeddedFunctionList= new List<string>();
            _embeddedFunctionList.AddRange(Directory.GetFiles(EmbeddedFunctionsPath, "*.a"));
        }

        private static IEnumerable<Command> Parse()
        {
            ParseFunctionPrototypes();

            var functSum = new List<Command>();
            foreach (var f in _functionList)
            {
                functSum.AddRange(ParseFunction(f));
            }

            return functSum;
        }

        #region Parse Functions

            private static void ParseFunctionPrototypes()
            {
                _functionList = new List<Function>();
                using (var read = new StreamReader(InputFile))
                {
                    while (!read.EndOfStream)
                    {
                        var prototype = read.ReadLine();
                        if (prototype == null)
                            break;
                        if (prototype.Length == 0 || !char.IsLetter(prototype[0]))
                            continue;
                        var f = new Function();
                        f.ReadData(read, prototype);
                        _functionList.Add(f);
                    }
                }

            }

            #region Parsing Functions

                private static IEnumerable<Command> ParseFunction(Function f)
                {
                    var variablesDeclared = new List<Variable>();
                    variablesDeclared.AddRange(GlobalVariableDeclaredList);
                    foreach(var v in f.Parameters)
                    {
                        Variable p;
                        if ((p = VariableNameExists(v.Name, variablesDeclared)) != null)
                            variablesDeclared.Remove(p);
                        variablesDeclared.Add(v);
                    }
                    var commands = new List<Command>();

                    PreParsing(variablesDeclared, f.Name);
                    using (var functionStream = FindFunction(f))
                    {
                        while (true)
                        {
                            if(!ParseDeclaration(functionStream, variablesDeclared))
                                break;
                        }

                    _started = false;
                        
                        while (true)
                        {
                            if (!ParseCommands(functionStream, variablesDeclared, commands)) 
                                break;
                        }
                    }
                    //Print(Commands);
                    //Print(VariablesDeclared);
                   //ReduceJumps(Commands);
                    //Console.ReadKey();
                    _regUsed = -1; 
                    ConvertToRegisters(commands, variablesDeclared);
                    //PrintBlank(Commands);
                    ConvertParameters(commands,f,variablesDeclared);
                    //PrintBlank(Commands);
                    //Console.WriteLine(regUsed);
                    FillWithPushPulls(commands);

                    FinishParsing(commands, f.Name);
                    //PrintBlank(Commands);
                    //Print(VariablesDeclared);

                    return commands;
                }

                #region Parsing Commands

                    private static StreamReader FindFunction(Function f)
                    {
                        var finder = new StreamReader(InputFile);
                        while (true)
                        {
                            var readLine = finder.ReadLine();
                            if (readLine == null)
                                return null;
                            if (!readLine.StartsWith(f.Name)) 
                                break;
                        }
                        finder.ReadLine();
                        return finder;
                    }

                    private static void PreParsing(ICollection<Variable> list, string name)
                    {
                        //get file
                        string[] buffers;
                        try
                        {
                            buffers = File.ReadAllLines(InputFile);
                        }
                        catch (Exception)
                        {
                            Error.printError("IO problem occured. Try again.");
                            return;
                        }

                        var i = 0;
                        while (!buffers[i].StartsWith(name))
                            i++;
                        i += 2;
                        for (; ; i++)
                        {
                            if (buffers[i].Equals("}"))
                                break;
                            var buffer = buffers[i];
                            var idxE = -1;
                            int idxS;
                            while ((idxS = buffer.IndexOf('\"', idxE + 1)) >= 0)
                            {
                                idxE = buffer.IndexOf('"', idxS + 1);
                                while (buffer[idxE - 1].Equals('\\') && !buffer[idxE - 1].Equals('\\'))
                                    idxE = buffer.IndexOf('"', idxE + 1);
                                var str = buffer.Substring(idxS + 1, idxE - idxS - 1);
                                var v = new Variable {Name = GetConstName(), IsArray = true};
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

                    private static bool ParseDeclaration(TextReader read, ICollection<Variable> list)
                    {
                        var s = read.ReadLine();
                        if (s==null||s.Length <= 1)
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
                        var idx = s.LastIndexOf(' ');
                        var name = s.Remove(0, idx + 1);
                        name = name.TrimEnd(';');
                        Variable v;
                        if ((v = VariableNameExists(name, list)) != null)
                            list.Remove(v);
                        v = new Variable();
                        var type = s.Remove(idx);
                        type = type.TrimStart(' ');
                        v.SetVariable(name, type);
                        list.Add(v);
                        return true;
                    }

                    private static bool ParseCommands(TextReader read, List<Variable> list, List<Command> line)
                    {
                        if (read == null)
                            return false;
                        var s = read.ReadLine();
                        if (s==null||s.Length <= 1)
                            return false;
                        
                        s = s.TrimStart();
                        var determinator = s.FirstWord().TrimEnd(';');
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
                                Variable v;
                                if ((v = FindVariable(list, determinator)) != null || determinator.Contains('['))
                                    ParseCommandVariable(v, s, list, line);
                                else if (FindFunciton(_functionList, determinator) != null)
                                    ParseCommandFunction(s, list, line, null);
                                else
                                    ParseCommandLabel(s, line);
                                break;
                        }
                        return true;
                    }
        
                    #region Parsing Commands Functions

                        private static void ParseCommandGoto(string buffer, ICollection<Command> line)
                        {
                            var c = new Command {Instruction = "JR", Touch = false};
                            //c = SetLabel(c);
                            c.Arguments[0] = buffer.Substring(buffer.LastIndexOf(' ') + 1).Trim('<', '>', ';');
                            line.Add(c);
                        }

                        private static void ParseCommandIf(string buffer, ICollection<Variable> list, ICollection<Command> line)
                        {
                            var c = new Command();
                            //c = SetLabel(c);
                            var op1 = buffer.Word(2).TrimStart('(');
                            var op2 = buffer.Word(4).TrimEnd(')');
                            var jpt = buffer.Word(6).Trim(';', '<', '>');
                            var jpf = buffer.Word(9).Trim(';', '<', '>');
                            string adt;
                            string adf;
                            string ads;

                            var oOp1 = FindVariable(list, op1);
                            
                            op2 = CreateConst(op2, list, oOp1);
                            if (!char.IsLetter(op2[0]))
                                c.Change[1] = false;
                            if (IspraviFRISC3 || oOp1.Type.IsSigned())
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
                                    Error.printError(buffer, 1, false);
                                    return;
                            }

                            c.Instruction = "CMP";
                            c.Arguments[0] = op1;
                            c.Arguments[1] = op2;
                            line.Add(c);

                            c = new Command {Instruction = "JR_" + ads + adt};
                            c.Arguments[0] = jpt;
                            c.Touch = false;
                            line.Add(c);

                            c = new Command {Instruction = "JR_" + ads + adf};
                            c.Arguments[0] = jpf;
                            c.Touch = false;
                            line.Add(c);
                        }

                        private static void ParseCommandReturn(Variable var, ICollection<Command> line)
                        {
                            var c = new Command();
                            //c = SetLabel(c);
                            if (var != null)
                            {
                                c.Instruction = "STORE";
                                c.Arguments[0] = var.Name;
                                c.Arguments[1] = Processor.FunctResult;
                                c.Change[1] = false;
                                line.Add(c);
                            }

                            c = new Command {Touch = false, Instruction = "RET"};
                            line.Add(c);
                        }

                        private static void ParseCommandSwitch()
                        {
                            Error.printError("Switch is not supported yet.", 1, false);
                        }

        private static void ParseCommandDereferenced(Variable dest, string buffer, List<Variable> list, List<Command> line)
                        {
                            var op2 = EditOp2(dest, buffer.Word(3).Trim(';'), list, line);
                            var memExt = GetMemExtension(dest.Type.GetRootLength());
                            var c = new Command {Instruction = "STORE" + memExt};
                            //c = SetLabel(c);
                            c.Arguments[0] = op2;
                            c.Arguments[1] = dest.Name;
                           // c.Change[1] = false;
                            line.Add(c);
                        }

                        private static void ParseCommandVariable(Variable dest, string buffer, List<Variable> list, List<Command> line)
                        {
                            #region Variables
                            var n = buffer.WordCount();
                            var op1 = buffer.Word(3);
                            var operand = buffer.Word(4);
                            var op2 = buffer.Word(5);
                            string temp1;
                            string memExt;
                            #endregion

                            #region a[i] = b;
                            if (dest == null)
                            {
                                ParseCommandArray(buffer, list, line);
                                return;
                            }
                            #endregion

                            #region Classes
                            var c = new Command();
                            Variable oOp1;

                            #endregion

                            #region a = (cast) b;
                            if (op1[0] == '(')
                            {
                                memExt = GetMemExtension(dest.Type.Length);
                                _started = true;

                                op1 = buffer.LastWord().TrimEnd(';');
                                oOp1 = FindVariable(list, op1);

                                #region UsingFloat
                                if (oOp1.Type.Type == "float" || dest.Type.Type == "float")
                                {
                                    RefreshValue(oOp1, line);

                                    if (oOp1.Type.Type == "float")
                                    {
                                        operand = "FloatTo";
                                    }
                                    else
                                    {
                                        operand = oOp1.Type.Type.IsSigned() ? "SignedTo" : "UnsignedTo";
                                    }
                                    c.Instruction = "PUSH";
                                    c.Arguments[0] = oOp1.Name;
                                    //c = SetLabel(c);
                                    line.Add(c);

                                    c = new Command {Instruction = "CALL", Touch = false};
                                    c.Arguments[0] = GetEmbeddedFunctionName(operand, dest.Type.Type);
                                    if (!AdditionalFunctionList.Contains(c.Arguments[0]))
                                        AdditionalFunctionList.Add(c.Arguments[0]);
                                    line.Add(c);

                                    c = new Command {Instruction = "POP"};
                                    c.Arguments[0] = dest.Name;
                                    c.LoadStore[0] = false;
                                    line.Add(c);

                                    RefreshValue(dest, line);
                                }
                                #endregion
                                else
                                {

                                    RefreshValue(FindVariable(list, op1), line);
                                    c.Instruction = "STORE" + memExt;
                                    EditOp2(dest, buffer, list, line);
                                    //c = SetLabel(c);
                                    c.Arguments[0] = oOp1.Name;
                                    c.Arguments[1] = dest.Name;
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
                                    dest.FillString(op1);
                                }
                                else
                                {
                                    c.Instruction = "PUSH";
                                    c.Arguments[0] = dest.Name;
                                    //c = SetLabel(c);
                                    line.Add(c);

                                    c = new Command {Instruction = "PUSH"};
                                    oOp1 = FindString(list, op1);
                                    c.Arguments[0] = oOp1.Name;
                                    line.Add(c);

                                    c = new Command {Instruction = "CALL", Touch = false};
                                    c.Arguments[0] = GetEmbeddedFunctionName("String", dest.Type.Type);
                                    if (!AdditionalFunctionList.Contains(c.Arguments[0]))
                                        AdditionalFunctionList.Add(c.Arguments[0]);
                                    line.Add(c);

                                    c = new Command {Instruction = "ADD"};
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
                                var idx1 = buffer.IndexOf(')');
                                idx1++;
                                var idx2 = buffer.IndexOf(' ', idx1);
                                op1 = buffer.Substring(idx1, idx2 - idx1);
                                op1=op1.Trim('&');
                                idx1 = buffer.IndexOf("+ ", StringComparison.Ordinal);
                                idx1 += 2;
                                idx2 = buffer.IndexOf('B');
                                temp1 = buffer.Substring(idx1, idx2 - idx1);
                                temp1 = CreateConst(temp1, list, dest.CloneUp());
                                c = new Command {Instruction = "LOAD"};
                                c.Arguments[0] = dest.Name;
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
                                    if (op1[0] == '-' && (oOp1 = FindVariable(list, op1.TrimStart('-'))) != null)
                                    {
                                        n = -1;
                                        temp1=CreateTemp(list, dest);
                                        c.Instruction = "XOR";
                                        c.Arguments[0] = oOp1.Name;
                                        c.Arguments[1] = n.ToHex9();
                                        c.Arguments[2] = temp1;
                                        c.Change[1] = false;
                                        line.Add(c);
                                        c = new Command();
                                        n = 1;
                                        c.Instruction = "ADD";
                                        c.Arguments[0] = temp1;
                                        c.Arguments[1] = n.ToHex9();
                                        c.Arguments[2] = dest.Name;
                                        c.Change[1] = false;
                                        line.Add(c);
                                        break;
                                    }
                                    #endregion

                                    Variable v;
                                    switch (op1[0])
                                    {
                                        case ('*'):
                                            #region a = *b;

                                            _started = true;
                                            op1 = op1.Remove(0, 1);
                                            v = FindVariable(list, op1);
                                            CreateTemp(list, v);
                                            memExt = GetMemExtension(dest.Type.Length);
                                            c.Instruction = "LOAD" + memExt;
                                            c.Arguments[0] = dest.Name;
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
                                                    Error.printWarning("Smatrati cu " + op1 + " kao da pise " + op1.Remove(op1.IndexOf('[')) + " jer su 2 sata i idem spavati");
                                                }
                                                else
                                                {
                                                    op1 = op1.Remove(op1.IndexOf('['));
                                                }
                                            }
                                            v = FindVariable(list, op1);
                                            if (v.Type.Level.Equals(dest.Type.Level) && v.Type.RootType.Equals(dest.Type.RootType))
                                                c.Change[1] = false;
                                            else
                                                c.Change[1] = true;
                                            memExt = GetMemExtension(v.Type.Length);
                                            c.Instruction = "STORE" + memExt;
                                            c.Arguments[0] = op1;
                                            c.Arguments[1] = dest.Name;
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
                                            c.Arguments[1] = dest.Name;
                                            //c = SetLabel(c);
                                            line.Add(c);
                                            c = new Command {Instruction = "CMP"};
                                            c.Arguments[0] = op1;
                                            n = 0;
                                            c.Arguments[1] = n.ToHex9();
                                            c.Change[1] = false;
                                            line.Add(c);
                                            c = new Command
                                            {
                                                Instruction = "DB 04, 00, 80, 0D7; JR_EQ (PC+4)",
                                                Touch = false
                                            };
                                            line.Add(c);
                                            c = new Command {Instruction = "MOVE"};
                                            c.LoadStore[1] = false;
                                            c.Arguments[0] = n.ToHex9();
                                            c.Change[0] = false;
                                            c.Arguments[1] = dest.Name;
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
                                                        dest.Value = float.Parse(op1).ToHex9();
                                                        dest.ValueSet = true;
                                                    }
                                                    else
                                                    {
                                                        dest.Value = int.Parse(op1).ToHex9();
                                                        dest.ValueSet = true;
                                                    }
                                                }
                                                else if (!_started&&InputFile.Contains("global"))
                                                {
                                                    dest.Value = op1;
                                                }
                                                else
                                                {
                                                    c.Instruction = "STORE";
                                                    op1 = EditOp2(dest, op1, list, line);
                                                    //c = SetLabel(c);
                                                    c.Arguments[0] = op1;
                                                    c.Arguments[1] = dest.Name;
                                                    c.Change[1] = false;
                                                    line.Add(c);
                                                }
                                            }

                                            #endregion

                                            #region a = b[c];
                                            else
                                            {
                                                var splitter = op1.Split('[');
                                                op2 = splitter[0];
                                                var memExtOp2 = GetMemExtension(FindVariable(list, op2).Type.GetRootLength());
                                                var offset = splitter[1].TrimEnd(']');
                                                temp1 = CreateTemp(list, dest);
                                                memExt = GetMemExtension(dest.Type.Length);
                                                if (!char.IsLetter(offset[0]))
                                                {
                                                    offset = CreateConstOffset(offset, list, dest);
                                                    if (offset.Equals("000000000"))
                                                    {
                                                        //NULA
                                                        c.Instruction = "LOAD" + memExtOp2;
                                                        //c = SetLabel(c);
                                                        c.Arguments[0] = temp1;
                                                        c.Arguments[1] = op2;
                                                        line.Add(c);

                                                        c = new Command {Instruction = "STORE" + memExt};
                                                        c.Arguments[0] = temp1;
                                                        c.Arguments[1] = dest.Name;
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

                                                        c = new Command {Instruction = "STORE" + memExt};
                                                        c.Arguments[0] = temp1;
                                                        c.Arguments[1] = dest.Name;
                                                        c.Change[1] = false;
                                                        line.Add(c);
                                                        return;
                                                    }
                                                    if ((n = dest.Type.GetRootLength()) > 1)
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

                                                    c = new Command {Instruction = "LOAD" + memExtOp2};
                                                    c.Arguments[0] = temp1;
                                                    c.Arguments[1] = op2;
                                                    c.MemShift = " + " + offset;
                                                    line.Add(c);

                                                    c = new Command {Instruction = "STORE" + memExt};
                                                    c.Arguments[0] = temp1;
                                                    c.Arguments[1] = dest.Name;
                                                    c.Change[1] = false;
                                                    line.Add(c);
                                                    return;
                                                }
                                                var temp2 = CreateTemp(list, dest.CloneUp());
                                                if ((n = dest.Type.GetRootLength()) > 1)
                                                {
                                                    c.Instruction = "SHL";
                                                    c.Arguments[0] = offset;
                                                    c.Arguments[2] = temp2;
                                                    n = (int)Math.Log(n, 2);
                                                    c.Arguments[1] = n.ToHex9();
                                                    c.Change[1] = false;
                                                    line.Add(c);
                                                    //c = SetLabel(c);

                                                    c = new Command {Instruction = "ADD"};
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
                                                }
                                                c = new Command {Instruction = "LOAD" + memExt};
                                                c.Arguments[0] = dest.Name;
                                                c.Arguments[1] = temp2;
                                                c.LoadStore[0] = false;
                                                c.Change[1] = true;
                                                line.Add(c);
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
                                                if (dest.Type.Type != "float")
                                                {
                                                    op2 = CreateConst(op2, list, dest);
                                                    if (!char.IsLetter(op2[0]))
                                                        c.Change[1] = false;
                                                    c.Instruction = "ADD";
                                                    c.Arguments[0] = op1;
                                                    c.Arguments[1] = op2;
                                                    c.Arguments[2] = dest.Name;
                                                    //c = SetLabel(c);
                                                    line.Add(c);
                                                }
                                                else
                                                {
                                                    c.Instruction = "STORE";
                                                    op1 = EditOp2(dest, op1, list, line);
                                                    c.Arguments[0] = op1;
                                                    c.Arguments[1] = "SP-4";
                                                    c.Change[1] = false;
                                                    line.Add(c);

                                                    c = new Command {Instruction = "STORE"};
                                                    op2 = EditOp2(dest, op2, list, line);
                                                    c.Arguments[0] = op2;
                                                    c.Arguments[1] = "SP-8";
                                                    c.Change[1] = false;
                                                    line.Add(c);

                                                    c = new Command {Instruction = "SUB"};
                                                    c.Arguments[0] = c.Arguments[2] = "SP";
                                                    c.Arguments[1] = "8";
                                                    c.Touch = false;
                                                    line.Add(c);

                                                    c = new Command {Instruction = "CALL", Touch = false};
                                                    c.Arguments[0] = GetEmbeddedFunctionName(operand, dest.Type.Type);
                                                    if (!AdditionalFunctionList.Contains(c.Arguments[0]))
                                                        AdditionalFunctionList.Add(c.Arguments[0]);
                                                    line.Add(c);

                                                    c = new Command {Instruction = "ADD"};
                                                    c.Arguments[0] = c.Arguments[2] = "SP";
                                                    c.Arguments[1] = "4";
                                                    c.Touch = false;
                                                    line.Add(c);

                                                    c = new Command {Instruction = "POP"};
                                                    c.Arguments[0] = dest.Name;
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
                                                op1 = CreateTemp(list, dest);
                                                if (!char.IsLetter(op2[0]))
                                                    c.Change[0] = false;
                                                c.Arguments[1] = op1;
                                                //c = SetLabel(c);
                                                line.Add(c);

                                                op2 = CreateConst(op2, list, dest);
                                                if (!char.IsLetter(op2[0]))
                                                    c.Change[1] = false;
                                                c.Instruction = "ADD";
                                                c.Arguments[0] = op1;
                                                c.Arguments[1] = op2;
                                                c.Arguments[2] = dest.Name;
                                                line.Add(c);
                                            }
                                            break;

                                            #endregion
                                        case ("-"):
                                            #region a = b - c;

                                            if (dest.Type.Type != "float")
                                            {
                                                var inverse = false;
                                                if (op1.IsNumber())
                                                {
                                                    temp1 = op1;
                                                    op1 = op2;
                                                    op2 = temp1;
                                                    inverse = true;
                                                }
                                                op2 = CreateConst(op2, list, dest);
                                                if (op2.IsNumber())
                                                    c.Change[1] = false;
                                                c.Instruction = "SUB";
                                                c.Arguments[0] = op1;
                                                c.Arguments[1] = op2;
                                                c.Arguments[2] = dest.Name;
                                                //c = SetLabel(c);
                                                line.Add(c);
                                                if (!inverse) return;
                                                c = new Command {Instruction = "XOR"};
                                                c.Arguments[0] = c.Arguments[2] = dest.Name;
                                                n = 1;
                                                c.Arguments[1] = n.ToHex9();
                                                c.Change[1] = false;
                                                line.Add(c);
                                            }
                                            else
                                            {
                                                c.Instruction = "STORE";
                                                op1 = EditOp2(dest, op1, list, line);
                                                c.Arguments[0] = op1;
                                                c.Arguments[1] = "SP-4";
                                                c.Change[1] = false;
                                                line.Add(c);

                                                c = new Command {Instruction = "STORE"};
                                                op2 = EditOp2(dest, op2, list, line);
                                                c.Arguments[0] = op2;
                                                c.Arguments[1] = "SP-8";
                                                c.Change[1] = false;
                                                line.Add(c);

                                                c = new Command {Instruction = "SUB"};
                                                c.Arguments[0] = c.Arguments[2] = "SP";
                                                c.Arguments[1] = "8";
                                                c.Touch = false;
                                                line.Add(c);

                                                c = new Command {Instruction = "CALL", Touch = false};
                                                c.Arguments[0] = GetEmbeddedFunctionName(operand, dest.Type.Type);
                                                if (!AdditionalFunctionList.Contains(c.Arguments[0]))
                                                    AdditionalFunctionList.Add(c.Arguments[0]);
                                                line.Add(c);

                                                c = new Command {Instruction = "ADD"};
                                                c.Arguments[0] = c.Arguments[2] = "SP";
                                                c.Arguments[1] = "4";
                                                c.Touch = false;
                                                line.Add(c);

                                                c = new Command {Instruction = "POP"};
                                                c.Arguments[0] = dest.Name;
                                                c.LoadStore[0] = false;
                                                line.Add(c);
                                            }
                                            break;

                                            #endregion
                                        case ("*"):
                                            #region a = b * c;

                                            c.Instruction = "STORE";
                                            op1 = EditOp2(dest, op1, list, line);
                                            c.Arguments[0] = op1;
                                            c.Arguments[1] = "SP-4";
                                            c.Change[1] = false;
                                            line.Add(c);

                                            c = new Command {Instruction = "STORE"};
                                            op2 = EditOp2(dest, op2, list, line);
                                            c.Arguments[0] = op2;
                                            c.Arguments[1] = "SP-8";
                                            c.Change[1] = false;
                                            line.Add(c);

                                            c = new Command {Instruction = "SUB"};
                                            c.Arguments[0] = c.Arguments[2] = "SP";
                                            c.Arguments[1] = "8";
                                            c.Touch = false;
                                            line.Add(c);

                                            c = new Command {Instruction = "CALL", Touch = false};
                                            c.Arguments[0] = GetEmbeddedFunctionName(operand, dest.Type.Type);
                                            if (!AdditionalFunctionList.Contains(c.Arguments[0]))
                                                AdditionalFunctionList.Add(c.Arguments[0]);
                                            line.Add(c);

                                            c = new Command {Instruction = "ADD"};
                                            c.Arguments[0] = c.Arguments[2] = "SP";
                                            c.Arguments[1] = "4";
                                            c.Touch = false;
                                            line.Add(c);

                                            c = new Command {Instruction = "POP"};
                                            c.Arguments[0] = dest.Name;
                                            c.LoadStore[0] = false;
                                            line.Add(c);
                                            break;

                                            #endregion
                                        case ("/"):
                                            #region a = b / c;

                                            c.Instruction = "STORE";
                                            op1 = EditOp2(dest, op1, list, line);
                                            c.Arguments[0] = op1;
                                            c.Arguments[1] = "SP-4";
                                            c.Change[1] = false;
                                            line.Add(c);

                                            c = new Command {Instruction = "STORE"};
                                            op2 = EditOp2(dest, op2, list, line);
                                            c.Arguments[0] = op2;
                                            c.Arguments[1] = "SP-8";
                                            c.Change[1] = false;
                                            line.Add(c);

                                            c = new Command {Instruction = "SUB"};
                                            c.Arguments[0] = c.Arguments[2] = "SP";
                                            c.Arguments[1] = "8";
                                            c.Touch = false;
                                            line.Add(c);

                                            c = new Command {Instruction = "CALL", Touch = false};
                                            c.Arguments[0] = GetEmbeddedFunctionName(operand, dest.Type.Type);
                                            if (!AdditionalFunctionList.Contains(c.Arguments[0]))
                                                AdditionalFunctionList.Add(c.Arguments[0]);
                                            line.Add(c);

                                            c = new Command {Instruction = "ADD"};
                                            c.Arguments[0] = c.Arguments[2] = "SP";
                                            c.Arguments[1] = "4";
                                            c.Touch = false;
                                            line.Add(c);

                                            c = new Command {Instruction = "POP"};
                                            c.Arguments[0] = dest.Name;
                                            c.LoadStore[0] = false;
                                            line.Add(c);
                                            break;

                                            #endregion
                                        case ("%"):
                                            #region a = b % c;

                                            c.Instruction = "STORE";
                                            op1 = EditOp2(dest, op1, list, line);
                                            c.Arguments[0] = op1;
                                            c.Arguments[1] = "SP-4";
                                            c.Change[1] = false;
                                            line.Add(c);

                                            c = new Command {Instruction = "STORE"};
                                            op2 = EditOp2(dest, op2, list, line);
                                            c.Arguments[0] = op2;
                                            c.Arguments[1] = "SP-8";
                                            c.Change[1] = false;
                                            line.Add(c);

                                            c = new Command {Instruction = "SUB"};
                                            c.Arguments[0] = c.Arguments[2] = "SP";
                                            c.Arguments[1] = "8";
                                            c.Touch = false;
                                            line.Add(c);

                                            c = new Command {Instruction = "CALL", Touch = false};
                                            c.Arguments[0] = GetEmbeddedFunctionName(operand, dest.Type.Type);
                                            if (!AdditionalFunctionList.Contains(c.Arguments[0]))
                                                AdditionalFunctionList.Add(c.Arguments[0]);
                                            line.Add(c);

                                            c = new Command {Instruction = "ADD"};
                                            c.Arguments[0] = c.Arguments[2] = "SP";
                                            c.Arguments[1] = "4";
                                            c.Touch = false;
                                            line.Add(c);

                                            c = new Command {Instruction = "POP"};
                                            c.Arguments[0] = dest.Name;
                                            c.LoadStore[0] = false;
                                            line.Add(c);
                                            break;

                                            #endregion
                                        case ("<<"):
                                            #region a = b << c;

                                            op2 = CreateConst(op2, list, dest);
                                            if (!char.IsLetter(op2[0]))
                                                c.Change[1] = false;
                                            op1 = EditOp2(dest, op1, list, line);
                                            c.Instruction = "SHL";
                                            c.Arguments[0] = op1;
                                            c.Arguments[1] = op2;
                                            c.Arguments[2] = dest.Name;
                                            //c = SetLabel(c);
                                            line.Add(c);
                                            break;

                                            #endregion
                                        case (">>"):
                                            #region a = b >> c;

                                            op2 = CreateConst(op2, list, dest);
                                            if (!char.IsLetter(op2[0]))
                                                c.Change[1] = false;
                                            op1 = EditOp2(dest, op1, list, line);
                                            c.Instruction = dest.Type.IsSigned() ? "ASHR" : "SHR";
                                            c.Arguments[0] = op1;
                                            c.Arguments[1] = op2;
                                            c.Arguments[2] = dest.Name;
                                            //c = SetLabel(c);
                                            line.Add(c);
                                            break;

                                            #endregion
                                        case ("|"):
                                            #region a = b | c;

                                            op2 = CreateConst(op2, list, dest);
                                            if (!char.IsLetter(op2[0]))
                                                c.Change[1] = false;
                                            c.Instruction = "OR";
                                            c.Arguments[0] = op1;
                                            c.Arguments[1] = op2;
                                            c.Arguments[2] = dest.Name;
                                            //c = SetLabel(c);
                                            line.Add(c);
                                            break;

                                            #endregion
                                        case ("&"):
                                            #region a = b & c;

                                            op2 = CreateConst(op2, list, dest);
                                            if (!char.IsLetter(op2[0]))
                                                c.Change[1] = false;
                                            c.Instruction = "AND";
                                            c.Arguments[0] = op1;
                                            c.Arguments[1] = op2;
                                            c.Arguments[2] = dest.Name;
                                            //c = SetLabel(c);
                                            line.Add(c);
                                            break;

                                            #endregion
                                        case ("^"):
                                            #region a = b ^ c;

                                            op2 = CreateConst(op2, list, dest);
                                            if (!char.IsLetter(op2[0]))
                                                c.Change[1] = false;
                                            c.Instruction = "XOR";
                                            c.Arguments[0] = op1;
                                            c.Arguments[1] = op2;
                                            c.Arguments[2] = dest.Name;
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
                                            c.Arguments[1] = dest.Name;
                                            //c = SetLabel(c);
                                            line.Add(c);
                                            c = new Command {Instruction = "CMP"};
                                            c.Arguments[0] = op1;
                                            op2 = CreateConst(op2, list, dest);
                                            if (!char.IsLetter(op2[0]))
                                                c.Change[1] = false;
                                            c.Arguments[1] = op2;
                                            line.Add(c);
                                            c = new Command
                                            {
                                                Instruction = "DB 04, 00, 80, 0D7; JR_EQ (PC+4)",
                                                Touch = false
                                            };
                                            line.Add(c);
                                            c = new Command {Instruction = "MOVE"};
                                            c.LoadStore[1] = false;
                                            c.Arguments[0] = n.ToHex9();
                                            c.Change[0] = false;
                                            c.Arguments[1] = dest.Name;
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
                                            c.Arguments[1] = dest.Name;
                                            //c = SetLabel(c);
                                            line.Add(c);
                                            c = new Command {Instruction = "CMP"};
                                            c.Arguments[0] = op1;
                                            op2 = CreateConst(op2, list, dest);
                                            if (!char.IsLetter(op2[0]))
                                                c.Change[1] = false;
                                            c.Arguments[1] = op2;
                                            line.Add(c);
                                            c = new Command
                                            {
                                                Instruction =
                                                    dest.Type.IsSigned()
                                                        ? "DB 04, 00, 00, 0D7; JR_SLE (PC+4)"
                                                        : "DB 04, 00, 40, 0D6; JR_ULE (PC+4)",
                                                Touch = false
                                            };
                                            line.Add(c);
                                            c = new Command {Instruction = "MOVE"};
                                            c.LoadStore[1] = false;
                                            c.Arguments[0] = n.ToHex9();
                                            c.Change[0] = false;
                                            c.Arguments[1] = dest.Name;
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
                                            c.Arguments[1] = dest.Name;
                                            //c = SetLabel(c);
                                            line.Add(c);
                                            c = new Command {Instruction = "CMP"};
                                            c.Arguments[0] = op1;
                                            op2 = CreateConst(op2, list, dest);
                                            if (!char.IsLetter(op2[0]))
                                                c.Change[1] = false;
                                            c.Arguments[1] = op2;
                                            line.Add(c);
                                            c = new Command
                                            {
                                                Instruction =
                                                    dest.Type.IsSigned()
                                                        ? "DB 04, 00, 40, 0D7; JR_SGE (PC+4)"
                                                        : "DB 04, 00, C0, 0D5; JR_UGE (PC+4)",
                                                Touch = false
                                            };
                                            line.Add(c);
                                            c = new Command {Instruction = "MOVE"};
                                            c.LoadStore[1] = false;
                                            c.Arguments[0] = n.ToHex9();
                                            c.Change[0] = false;
                                            c.Arguments[1] = dest.Name;
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
                                            c.Arguments[1] = dest.Name;
                                            //c = SetLabel(c);
                                            line.Add(c);
                                            c = new Command {Instruction = "CMP"};
                                            c.Arguments[0] = op1;
                                            op2 = CreateConst(op2, list, dest);
                                            if (!char.IsLetter(op2[0]))
                                                c.Change[1] = false;
                                            c.Arguments[1] = op2;
                                            line.Add(c);
                                            c = new Command
                                            {
                                                Instruction =
                                                    dest.Type.IsSigned()
                                                        ? "DB 04, 00, C0, 0D6; JR_SLT (PC+4)"
                                                        : "DB 04, 00, C0, 0D4; JR_ULT (PC+4)",
                                                Touch = false
                                            };
                                            line.Add(c);
                                            c = new Command {Instruction = "MOVE"};
                                            c.LoadStore[1] = false;
                                            c.Arguments[0] = n.ToHex9();
                                            c.Change[0] = false;
                                            c.Arguments[1] = dest.Name;
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
                                            c.Arguments[1] = dest.Name;
                                            //c = SetLabel(c);
                                            line.Add(c);
                                            c = new Command {Instruction = "CMP"};
                                            c.Arguments[0] = op1;
                                            op2 = CreateConst(op2, list, dest);
                                            if (!char.IsLetter(op2[0]))
                                                c.Change[1] = false;
                                            c.Arguments[1] = op2;
                                            line.Add(c);
                                            c = new Command
                                            {
                                                Instruction =
                                                    dest.Type.IsSigned()
                                                        ? "DB 04, 00, 80, 0D7; JR_SGT (PC+4)"
                                                        : "DB 04, 00, 80, 0D6; JR_UGT (PC+4)",
                                                Touch = false
                                            };
                                            line.Add(c);
                                            c = new Command {Instruction = "MOVE"};
                                            c.LoadStore[1] = false;
                                            c.Arguments[0] = n.ToHex9();
                                            c.Change[0] = false;
                                            c.Arguments[1] = dest.Name;
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
                                            c.Arguments[1] = dest.Name;
                                            //c = SetLabel(c);
                                            line.Add(c);
                                            c = new Command {Instruction = "CMP"};
                                            c.Arguments[0] = op1;
                                            op2 = CreateConst(op2, list, dest);
                                            if (!char.IsLetter(op2[0]))
                                                c.Change[1] = false;
                                            c.Arguments[1] = op2;
                                            line.Add(c);
                                            c = new Command
                                            {
                                                Instruction = "DB 04, 00, 00, 0D6 ; JR_NE (PC+4)",
                                                Touch = false
                                            };
                                            line.Add(c);
                                            c = new Command {Instruction = "MOVE"};
                                            c.Arguments[0] = n.ToHex9();
                                            c.Change[0] = false;
                                            c.LoadStore[1] = false;
                                            c.Arguments[1] = dest.Name;
                                            line.Add(c);
                                            break;

                                            #endregion
                                        default:
                                            #region a = f (a, ... ,n);
                                            _started = true;
                                            ParseCommandFunction(buffer.Remove(0, buffer.IndexOf("=", StringComparison.Ordinal) + 2), list, line, dest.Name);
                                            break;
                                    }
                                    break;
                                default:
                                    _started = true;
                                    ParseCommandFunction(buffer.Remove(0, buffer.IndexOf("=", StringComparison.Ordinal) + 2), list, line, dest.Name);
                                    break;
                                            #endregion
                            }
                        }

                        private static void ParseCommandArray(string buffer, List<Variable> list, List<Command> line)
                        {
                            var splitter = buffer.FirstWord().Split('[');
                            var dest = FindVariable(list, splitter[0]);
                            var offset = splitter[1].TrimEnd(']', ';');
                            var op2 = (buffer.LastWord()).TrimEnd(';');
                            var c = new Command();
                            var memExt = GetMemExtension(dest.Type.GetRootLength());
                            int n;

                            _started = true;

                            op2 = EditOp2(dest, op2, list, line);

                            if (!char.IsLetter(offset[0]))
                            {
                                offset = CreateConstOffset(offset, list, dest);
                                if (offset.Equals("000000000"))
                                {
                                    //NULA
                                    c.Instruction = "STORE" + memExt;
                                    //c = SetLabel(c);
                                    c.Arguments[0] = op2;
                                    c.Arguments[1] = dest.Name;
                                    c.Change[1] = true;
                                    line.Add(c);
                                    return;
                                }
                                if (!char.IsLetter(offset[0]))
                                {
                                    c.Instruction = "STORE" + memExt;
                                    //c = SetLabel(c);
                                    c.Arguments[0] = op2;
                                    c.Arguments[1] = dest.Name;
                                    c.MemShift = " + " + offset;
                                    line.Add(c);
                                    return;
                                }
                                if ((n = dest.Type.GetRootLength()) > 1)
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
                                c.Arguments[0] = dest.Name;
                                c.Arguments[1] = offset;
                                c.Arguments[2] = offset;
                                line.Add(c);

                                c = new Command {Instruction = "STORE" + memExt};
                                c.Arguments[0] = op2;
                                c.Arguments[1] = dest.Name;
                                c.MemShift = " + " + offset;
                                line.Add(c);
                                return;
                            }
                            var temp = CreateTemp(list, dest.CloneUp());
                            if ((n = dest.Type.GetRootLength()) > 1)
                            {
                                c.Instruction = "SHL";
                                c.Arguments[0] = offset;
                                c.Arguments[2] = temp;
                                n = (int)Math.Log(n, 2);
                                c.Arguments[1] = n.ToHex9();
                                c.Change[1] = false;
                                line.Add(c);
                                //c = SetLabel(c);

                                c = new Command {Instruction = "ADD"};
                                c.Arguments[0] = dest.Name;
                                c.Arguments[1] = temp;
                                c.Arguments[2] = temp;
                                line.Add(c);
                            }
                            else
                            {
                                c.Instruction = "ADD";
                                //c = SetLabel(c);
                                c.Arguments[0] = dest.Name;
                                c.Arguments[1] = offset;
                                c.Arguments[2] = temp;
                                line.Add(c);
                            }
                            c = new Command {Instruction = "STORE" + memExt};
                            c.Arguments[0] = op2;
                            c.Arguments[1] = temp;
                            line.Add(c);
                        }

                        private static void ParseCommandFunction(string buffer, List<Variable> list, List<Command> line, string result)
                        {
                            Command c;
                            _started = true;

                            var f = FindFunciton(_functionList, buffer.FirstWord());

                            var allparameters = buffer.Remove(0, buffer.IndexOf('(') + 1).TrimEnd(';', ')');
                            var parameters = allparameters.Split(',');
                            var cnt = 4;
                            if (parameters[0].Length >= 1)
                            {
                                parameters[parameters.Count()]= parameters[parameters.Count() - 1].TrimEnd(')');
                                cnt = 0;
                                for (var i = 0; i < parameters.Count(); i++)
                                {
                                    c = new Command();
                                    var m = parameters[i].TrimStart(' ');
                                    var vp = f.Parameters.ElementAt(i);
                                    var vf = FindVariable(list, m);
                                    switch (m[0])
                                    {
                                        case '&':
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
                                            break;
                                        case '\"':
                                        {
                                            var constStr = FindString(list, m.Substring(1, m.Length - 2));
                                            m = constStr.Name;
                                        }
                                            break;
                                    }

                                    while ((cnt + vp.Type.Length) % 4 != 0)
                                        cnt++;
                                    cnt += vp.Type.Length;
                                    if (vf != null && !vp.Type.Type.Equals(vf.Type.Type))
                                        RefreshValue(vf, line);
                                    var memExt = GetMemExtension(vp.Type.Length);
                                    c.Instruction = "STORE" + memExt;
                                    m = CreateConst(m, list, vp);
                                    if (char.IsDigit(m[0]))
                                    {
                                        var com = new Command {Label = c.Label};
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
                                    c.Arguments[1] = "SP-" + cnt.ToHex9();
                                    c.Change[1] = false;
                                    line.Add(c);
                                }
                                c = new Command {Instruction = "SUB", Touch = false};
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
                                c = new Command {Instruction = "CALL", Touch = false};
                                c.Arguments[0] = f.Name;
                                line.Add(c);

                                if (result == null)
                                {
                                    c = new Command {Instruction = "ADD"};
                                    c.Arguments[0] = c.Arguments[2] = "SP";
                                    c.Touch = false;
                                    c.Arguments[1] = (cnt).ToHex9();
                                    if (cnt != 0)
                                        line.Add(c);
                                }
                                else
                                {
                                    c = new Command {Instruction = "ADD"};
                                    c.Arguments[0] = c.Arguments[2] = "SP";
                                    c.Touch = false;
                                    c.Arguments[1] = (cnt - 4).ToHex9();
                                    if (cnt - 4 != 0)
                                        line.Add(c);
                                    c = new Command {Instruction = "POP"};
                                    c.Arguments[0] = result;
                                    c.LoadStore[0] = false;
                                    line.Add(c);
                                }
                            }
                            else
                            {
                                c = new Command {Instruction = "SUB", Touch = false};
                                c.Arguments[0] = c.Arguments[2] = "SP";
                                c.Arguments[1] = cnt.ToHex9();
                                if (cnt != 0)
                                    line.Add(c);

                                c = new Command {Instruction = "CALL", Touch = false};
                                c.Arguments[0] = f.Name;
                                line.Add(c);

                                if (result == null)
                                {
                                    c = new Command {Instruction = "ADD"};
                                    c.Arguments[0] = c.Arguments[2] = "SP";
                                    c.Touch = false;
                                    c.Arguments[1] = cnt.ToHex9();
                                    if (cnt != 0)
                                        line.Add(c);
                                }
                                else
                                {
                                    c = new Command {Instruction = "POP"};
                                    c.LoadStore[0] = false;
                                    c.Arguments[0] = result;
                                    line.Add(c);
                                }
                            }
                        }

                        private static void ParseCommandLabel(string buffer, ICollection<Command> line)
                        {
                            _started = true;
                            var c = new Command {Label = buffer.Trim(':', '<', '>'), Instruction = ""};
                            line.Add(c);
                        }

                    #endregion

                #endregion

                #region Convert To Registers

                    #region Variables

                        private static int _currLine;

                        private static int _currArg = -1;

                    #endregion

                    private static void ConvertToRegisters(List<Command> line, List<Variable> list)
                    {
                        var regList = FillRegList(line);
                        var i = 0;
                        //Print(regList);
                        while (true)
                        {
                            if (!EditCloseVar(regList, i++))
                                break;
                        } 
                        //Print(regList);
                        regList = AddRegData(regList);
                        //Print(regList);
                        //PrintBlank(line);
                        FillWithRegList(line, regList, list);
                        //PrintBlank(line);
                    }

                    #region Convert To Registers Functions

                        private static Command FindNextVar(IList<Command> lines)
                        {
                            if (lines.Count <= _currLine)
                                return null;
                            Command line;
                            do
                            {
                                _currArg++;
                                if (_currArg < 3)
                                    line = lines[_currLine];
                                else
                                {
                                    _currArg = 0;
                                    do
                                    {
                                        ++_currLine;
                                        if (lines.Count <= _currLine)
                                        {
                                            return null;
                                        }
                                        line = lines[_currLine];
                                        if ((line.Instruction != null && (line.Instruction.Contains("JR") || line.Instruction.Contains("RET"))) || line.Label != null)
                                            break;
                                    } while (!line.Touch || line.MemReg);
                                    if ((line.Instruction != null && (line.Instruction.Contains("JR") || line.Instruction.Contains("RET"))) || line.Label != null)
                                        break;
                                }
                            } while (line.Arguments[_currArg] == null || !char.IsLetter(line.Arguments[_currArg][0]) || !line.Change[_currArg] || !line.Touch);
                            return line;
                        }

                        private static List<Register> FillRegList(IList<Command> lines)
                        {
                            _currLine = 0;
                            _currArg = -1;
                            var regList = new List<Register>();
                            Command c;
                            while ((c = FindNextVar(lines)) != null)
                            {
                                if (c.Instruction.Contains("JR") || c.Instruction.Contains("RET") || c.Label != null)
                                {
                                    _currLine++;
                                    _currArg = -1;
                                    regList.Add(new Register(true));
                                }
                                else
                                    regList.Add(new Register(c, _currArg));
                            }
                            return regList;
                        }


                        private static bool Exists(IEnumerable<Register> list, string s)
                        {
                            return list.Any(r => r.VarName.Equals(s));
                        }

        private static bool EditCloseVar(IList<Register> list, int idx)
                        {
                            if (list.Count <= idx)
                                return false;
                            if (list[idx].Edited)
                                return true;
                            if (list[idx].VarName.Equals(""))
                                return true;
                            var foundRegisters = new List<Register>();
                            var thisRegister = list[idx];
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
            var lastRegister = thisRegister;
                            int i;
                            for (i = idx + 1; i < list.Count; i++)
                            {
                                Register foundRegister;
                                if ((foundRegister = list[i]).VarName.Equals(""))
                                {
                                    break;
                                }
                                if (foundRegister.VarName.Equals(thisRegister.VarName))
                                {
                                   // thisRegister.AddSave(foundRegister);
                                    foundRegister.Edited = true;
                                    var sv = lastRegister.SaveNow;
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


                        private static List<Register> AddRegData(IEnumerable<Register> regList)
                        {
                            var freeRegs = new FreeRegs();
                            var ret = new List<Register>();
                            var temp = new List<int>();
                            var usedList = new string[Processor.NoOfRegisters];
                            
                            foreach (Register register in regList)
                            {
                                int reg;
                                if (register.VarName.Equals(""))
                                {
                                    ret.Add(register);
                                    freeRegs = new FreeRegs();
                                    temp = new List<int>();
                                    continue;
                                }
                                if (register.isNewLine)
                                {
                                    freeRegs.Add(temp);
                                    temp = new List<int>();
                                }
                                if (register.Start)
                                {
                                    reg = freeRegs.Get();
                                    usedList[reg] = register.VarName;
                                    register.RegName = reg;
                                    //if (!regList[i].Load)
                                    //    regList[i].LoadNow = false;
                                    if (register.End)
                                    {
                                        temp.Add(reg);
                                        usedList[reg] = null;
                                    }
                                }
                                else
                                {
                                    if ((reg = Array.IndexOf(usedList, register.VarName)) >= 0)
                                        register.RegName = reg;
                                    else
                                    {
                                        reg = freeRegs.Get();
                                        usedList[reg] = register.VarName;
                                        register.RegName = reg;
                                    }
                                    if (register.End)
                                    {
                                        temp.Add(reg);
                                        usedList[reg] = null;
                                    }
                                }
                                ret.Add(register);
                            }
                            return ret;
                        }

                        private static List<Command> AddLoad(List<Command> lines, Register reg, IEnumerable<Variable> list)
                        {
                            if (reg.VarName.Contains("temp_"))
                                return lines;
                            var c = new Command();
                            var v = FindVariable(list, reg.VarName);
                            c.Instruction = "LOAD" + GetMemExtension(v.Type.Length);
                            c.MemReg = true;
                            c.Change[1] = false;
                            c.LoadStore[0] = false;
                            c.Arguments[0] = "R" + reg.RegName;
                            c.Arguments[1] = reg.VarName;
                            lines.Insert(_currLine++, c);
                            if (v.Type.Length == 4)
                                return lines;

                            c = new Command {Instruction = "SHL"};
                            c.Arguments[0] = c.Arguments[2] = "R" + reg.RegName;
                            int n = v.Type.Length == 2 ? 16 : 24;
                            c.Arguments[1] = n.ToHex9();
                            //c.LoadStore[2] = true;
                            c.MemReg = true;
                            lines.Insert(_currLine++, c);

                            c = new Command {Instruction = v.Type.IsSigned() ? "ASHR" : "SHR"};
                            c.Arguments[0] = c.Arguments[2] = "R" + reg.RegName;
                            c.Arguments[1] = n.ToHex9();
                            c.Touch = false;
                            c.MemReg = true;
                            lines.Insert(_currLine++, c);
                            if (!MatchingLinesRefresh(lines[_currLine - 2], lines[_currLine]) ||
                                !MatchingLinesRefresh(lines[_currLine - 1], lines[_currLine + 1])) return lines;
                            lines.RemoveAt(_currLine - 2);
                            lines.RemoveAt(_currLine - 2);
                            _currLine -= 2;

                            return lines;
                        }

                        private static List<Command> AddSave(List<Command> lines, Register reg, IEnumerable<Variable> list)
                        {
                            if (reg.VarName.Contains("temp_") || reg.VarName.Contains("const_"))
                                return lines;
                            var c = new Command();
                            var v = FindVariable(list, reg.VarName);
                            c.Instruction = "STORE" + GetMemExtension(v.Type.Length);
                            c.MemReg = true;
                            c.Change[1] = false;
                            c.Arguments[0] = "R" + reg.RegName;
                            c.Arguments[1] = reg.VarName;
                            lines.Insert(_currLine + 1, c);
                            return lines;
                        }

                        private static void FillWithRegList(List<Command> lines, IEnumerable<Register> regList, List<Variable> list)
                        {
                            _currLine = 0;
                            _currArg = -1;
                            foreach (var register in regList)
                            {
                                var c = FindNextVar(lines);
                                if (register.VarName.Equals(""))
                                {
                                    _currArg = -1;
                                    _currLine++;
                                    continue;
                                }
                                //string name = c.Arguments[_currArg];
                                if (register.LoadNow)
                                    lines = AddLoad(lines, register, list);
                                if (register.SaveNow)
                                    lines = AddSave(lines, register, list);
                                c.Arguments[_currArg] = "R" + register.RegName;
                                if (register.RegName > _regUsed)
                                    _regUsed = register.RegName;
                            }

                        }

                    #endregion

                #endregion

                #region Convert Parameters

                    private static void ConvertParameters(List<Command> line, Function f, List<Variable> list)
                    {
                        var positions = new List<string>();
                        var i = list.Count - 1;
                        var offset = 0;
                        var set = 0;
                        var spShift = 0;
                        var noParam = false;
                        var loc = "";
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
                                spShift = offset;
                                if (_regUsed > 1)
                                    offset += 4 + (_regUsed) * 4+4;
                                else
                                    offset += 4 + 4+4;
                                set = i + 1;
                            }

                            while (offset % list[i].Type.Length != 0)
                            {
                                offset++;
                            }

                            positions.Insert(0, "SP + " + offset.ToHex9());
                            if (list[i].IsArray)
                            {
                                list[i].Type.GetRootLength();
                                list[i].ArrayLength();
                                offset += list[i].ArrayLength() * list[i].Type.GetRootLength();
                                while (offset % 4 != 0)
                                    offset++;
                            }
                            offset += list[i].Type.Length;
                            i--;
                        }
                        if (noParam)
                        {
                            spShift = offset;
                            if (_regUsed > 1)
                                offset +=(_regUsed) * 4;
                            else
                                offset +=4;
                        }
                        else
                            offset -= 4;


                        var cs = new Command();
                        if (line.Count<=0||!line.Last().Instruction.Contains("RET"))
                        {
                            cs.Instruction = "RET";
                            line.Add(cs);
                        }

                        if (set <= list.Count - 1)
                            AddFunctVarsAndConsts(list, line, positions);
                        if (spShift != 0&&line.Count>1)
                        {
                            cs = new Command {Instruction = "SUB"};
                            cs.Arguments[0] = cs.Arguments[2] = "SP";
                            cs.Arguments[1] = spShift.ToHex9();
                            line.Insert(0, cs);
                        }
                        for (i = 0; i < line.Count; i++)
                        {

                            if (line[i].Instruction.Equals("") || line[i].Instruction == null)
                                continue;
                            if (line[i].Instruction.Contains("RET") && spShift != 0)
                            {
                                if (line.Count <= 1)
                                    continue;
                                cs = new Command {Instruction = "ADD"};
                                cs.Arguments[0] = cs.Arguments[2] = "SP";
                                cs.Arguments[1] = spShift.ToHex9();
                                line.Insert(i, cs);
                                i++;
                            }
                            else
                            {
                                if (!line[i].Instruction.Contains("LOAD") && !line[i].Instruction.Contains("STORE"))
                                    continue;
                                if (line[i].Arguments[1] == null || line[i].Arguments[1].IsRegName()) continue;
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

                    #region Convert Parameters Functions

                        private static string GetStackPosition(string s, List<Variable> list, List<string> pos)
                        {
                            int i;
                            for (i = 0; i < list.Count; i++)
                            {
                                if (!list[i].Name.Equals(s)) continue;
                                if (list[i].IsGlobal)
                                    return s;
                                break;
                            }
                            return i >= list.Count ? s : pos[i];
                        }

                        private static void AddFunctVarsAndConsts(List<Variable> list, List<Command> line,IList<string> positions)
                        {
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
                                Command c;
                                for (var j = i; j < list.Count; j++)
                                {
                                    if (list[j].ValueSet&&list[j].Value.Equals(list[i].Value))
                                    {
                                        c = new Command {Instruction = "STORE" + GetMemExtension(list[j].Type.Length)};
                                        c.Arguments[0] = "R0";
                                        c.Arguments[1] = positions[j];
                                        line.Insert(0, c);
                                        list[j].ValueSet = false;
                                    }
                                }

                                if (!list[i].Value.IsLegalHex())
                                {
                                    c = new Command {Instruction = "ADD"};
                                    c.Arguments[0] = c.Arguments[2] = "R0";
                                    c.Arguments[1] = "R1";
                                    line.Insert(0, c);

                                    c = new Command {Instruction = "MOVE"};
                                    c.Arguments[0] = list[i].Value.Remove(0,5);
                                    c.Arguments[1] = "R0";
                                    line.Insert(0, c);

                                    c = new Command {Instruction = "MOVE"};
                                    c.Arguments[0] = list[i].Value.Remove(5);
                                    c.Arguments[1] = "R1";
                                    line.Insert(0, c);
                                }
                                else
                                {
                                    c = new Command {Instruction = "MOVE"};
                                    c.Arguments[0] = list[i].Value;
                                    c.Arguments[1] = "R0";
                                    line.Insert(0, c);
                                }
                            }
                        }

                        private static void AddFunctArray(Variable var, List<Command> line,string position)
                        {
                            var n=Convert.ToInt32(position.LastWord(),16)+4;
                            var c = new Command {Instruction = "STORE"};

                            c.Arguments[0] = "R0";
                            c.Arguments[1] = position;
                            line.Insert(0, c);

                            c = new Command {Instruction = "ADD"};
                            c.Arguments[0] = "SP";
                            c.Arguments[1] = n.ToHex9();
                            c.Arguments[2] = "R0";
                            line.Insert(0, c);

                            if (var.IsArrayEmpty)
                            {
                                return;
                            }
                            var memExt=GetMemExtension(var.Type.GetRootLength());
                            var memLength = var.Type.RootType.GetLength();
                            var cnt = var.ArrayLength();
                            for (var i = 0; i < cnt; i++)
                            {
                                if (!var.ArraySet[i]) continue;
                                for (var j = i; j < cnt; j++)
                                {
                                    if (!var.ArraySet[i] || !var.ArrayValues[i].Equals(var.ArrayValues[j]))
                                        continue;
                                    c = new Command {Instruction = "STORE" + memExt};
                                    c.Arguments[0] = "R0";
                                    c.Arguments[1] = "SP + " + (n+i*memLength).ToHex9();
                                    line.Insert(0, c);
                                    var.ArraySet[i] = false;
                                }

                                if (!var.ArrayValues[i].IsLegalHex())
                                {
                                    c = new Command {Instruction = "ADD"};
                                    c.Arguments[0] = c.Arguments[2] = "R0";
                                    c.Arguments[1] = "R1";
                                    line.Insert(0, c);

                                    c = new Command {Instruction = "MOVE"};
                                    c.Arguments[0] = var.ArrayValues[i].Remove(0, 5);
                                    c.Arguments[1] = "R0";
                                    line.Insert(0, c);

                                    c = new Command {Instruction = "MOVE"};
                                    c.Arguments[0] = var.ArrayValues[i].Remove(5);
                                    c.Arguments[1] = "R1";
                                    line.Insert(0, c);
                                }
                                else
                                {
                                    c = new Command {Instruction = "MOVE"};
                                    c.Arguments[0] = var.ArrayValues[i];
                                    c.Arguments[1] = "R0";
                                    line.Insert(0, c);
                                }
                            }
                        }

                    #endregion

                #endregion

                private static void FillWithPushPulls(IList<Command> line)
                {
                    if (_regUsed == -1)
                    {
                        for (var i = 0; i < line.Count && i < 3; i++)
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
                    var c=new Command();
                    for (var i = _regUsed; i >= 0; i--)
                    {
                        c.Instruction="PUSH";
                        c.Arguments[0]="R"+i;
                        line.Insert(0, c);
                        c=new Command();
                    }
                    for (var j = 0; j < line.Count; j++)
                    {
                        if (line[j].Instruction.Contains("RET"))
                        {
                            for (var i = _regUsed; i >= 0; i--)
                            {
                                c.Instruction = "POP";
                                c.Arguments[0] = "R" + i;
                                line.Insert(j, c);
                                c = new Command();
                                j++;
                            }
                        }
                    }
                }

                private static void FinishParsing(List<Command> line,string name)
                {
                    foreach (var c in line)
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
                                if (!EmbeddedFunctionFoundList.Contains(c.Arguments[0]))
                                    EmbeddedFunctionFoundList.Add(c.Arguments[0]);
                            }
                        }
                    }

                    AddFunctName(line, name); 
                }

                private static void AddFunctName(IList<Command> line, string name)
                {
                    var c = new Command {Label = name, Instruction = ""};
                    line.Insert(0,c);
                }

                #region Const and Temp

                    #region Variables

                        private static int _noTemp;

                        private static bool _lockTemp;

                        private static int _noConst;

                    #endregion

                    private static string EditOp2(Variable dest, string op2, List<Variable> list, List<Command> line)
                    {
                        var name = CreateConst(op2, list, dest);
                        if (name[0].IsNumberPart())
                        {
                            var c = new Command {Instruction = "MOVE"};
                            //c = SetLabel(c);
                            c.LoadStore[1] = false;
                            c.Arguments[0] = name;
                            c.Change[0] = false;
                            name = CreateTemp(list, dest);
                            c.Arguments[1] = name;
                            line.Add(c);
                        }
                        return name;
                    }

                    #region Const

                        private static string CreateConst(string name, ICollection<Variable> list, Variable var)
                        {
                            if (!(name[0].IsNumberPart()))
                                return name;
                            string s;
                            if (name.Contains('.'))
                            {
                                var f = Single.Parse(name, CultureInfo.InvariantCulture);
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
                            var v = var.Clone();
                            v.ConvertValue(s);
                            if ((v.Name = ConstExists(list, v.Value)) == null)
                            {
                                v.Name = GetConstName(v.Value);
                                v.IsConst = true;
                                list.Add(v);
                            }
                            return v.Name;
                        }

                        private static string CreateConstOffset(string name, List<Variable> list, Variable var)
                        {
                            if (!(name[0].IsNumberPart()))
                                return name;
                            var s = (int.Parse(name) * var.Type.GetRootLength()).ToHex9();
                            if (s.IsLegalHex())
                            {
                                return s;
                            }
                            var v = var.Clone();
                            v.ConvertValue(int.Parse(name) * var.Type.Length);
                            if ((v.Name = ConstExists(list, v.Value)) != null) return v.Name;
                            GetConstName(v.Value);
                            v.IsConst = true;
                            list.Add(v);
                            return v.Name;
                        }

                        private static string ConstExists(IEnumerable<Variable> list, string value)
                        {
                            return (from c in list where c.Value.Equals(value) select c.Name).FirstOrDefault();
                        }

        private static string GetConstName(string val)
                        {
                            return "const_" + val.Remove(0, 5);
                        }

                        private static string GetConstName()
                        {
                            return "const." + _noConst++;
                        }

                    #endregion

                    #region Temp

                        private static string CreateTemp(ICollection<Variable> list, Variable var)
                        {
                            var v = var.Clone();
                            v.IsTemp = true;
                            v.Name = GetTempName();
                            if (!_lockTemp)
                                list.Add(v);
                            if (_noTemp >= Processor.NoOfRegisters)
                                _lockTemp = true;
                            return v.Name;
                        }

                        private static string GetTempName()
                        {
                            return "temp_" + _noTemp++;
                        }

                    #endregion

                #endregion

            #endregion

        #endregion

        #region Add Global Variable Values

            private static void AddGlobalVarsAndConsts(IEnumerable<Variable> list, List<Command> line)
            {
                foreach (var var in list.Where(var => !var.IsTemp))
                {
                    if (var.IsArray)
                    {
                        AddGlobalArray(var, line);
                        continue;
                    }
                    var c = new Command {Label = var.Name};
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
                            Error.printError("unknown length", 1, false);
                            return;
                    }
                    c.Arguments[0] = var.Value;
                    line.Add(c);
                }
            }

        private static void AddGlobalArray(Variable var, ICollection<Command> line)
            {
                var ptr = GetTempName();
                var c = new Command {Label = var.Name, Instruction = "DW"};
                c.Arguments[0] = ptr;
                line.Add(c);
                var cnt = 0;
                c = new Command();
                if (var.IsArrayEmpty)
                {
                    c.Instruction = "`DS";
                    c.Label = ptr;
                    c.Arguments[0] = (var.ArrayLength() * var.Type.GetRootLength()).ToString(CultureInfo.InvariantCulture);
                    line.Add(c);
                    return;
                }
                switch (var.Type.GetRootLength())
                {
                    case 4:
                        for (var i = 0; i < var.ArrayLength(); i++)
                        {
                            if (i == 0)
                                c.Label = ptr;
                            c.Instruction = "DW";
                            c.Arguments[0] = var.ArrayValues[i];
                            line.Add(c);
                            c = new Command();
                        }
                        break;
                    case 2:
                        for (var i = 0; i < var.ArrayLength(); i++)
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
                        break;
                    default:
                        for (var i = 0; i < var.ArrayValues.Count; i++)
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
                        break;
                }

            }

        #endregion

        private static string AddAdditionalFunctions(string input)
        {
            var output = "";

            //add additionals
            if (!Directory.Exists(AdditionalFunctionPath))
                Error.printError("Directory missing: " + AdditionalFunctionPath);
            try
            {
                foreach (var additionalFunction in AdditionalFunctionList)
                {
                    var files = Directory.GetFiles(AdditionalFunctionPath, additionalFunction + '.');
                    if (files.Length <= 0)
                    {
                        File.Create(Path.Combine(AdditionalFunctionPath, additionalFunction) + ".a").Close();
                        Error.printError(Path.Combine(AdditionalFunctionPath, additionalFunction) +
                                         " not written yet.\n Blank file created. Add assembler code and try again.");
                    }

                    string fileName = files[0];
                    try
                    {
                        using (var s = new StreamReader(Path.Combine(AdditionalFunctionPath, fileName)))
                            output += s.ReadToEnd();
                    }
                    catch (Exception)
                    {
                        Error.printError("File unreachable: " + Path.Combine(AdditionalFunctionPath, fileName));
                        return null;
                    }
                }
            }
            catch (Exception)
            {
                Error.printError("Directory unreachable: " + AdditionalFunctionPath);
                return null;
            }

            //add embedded
            if (!Directory.Exists(EmbeddedFunctionsPath))
            {
                Error.printError("Directory missing: " + EmbeddedFunctionsPath);
                return null;
            }
            try
            {
                foreach (var embeddedFunction in EmbeddedFunctionFoundList)
                {
                    try
                    {
                        string buffer;
                        using (var s = new StreamReader(Path.Combine(AdditionalFunctionPath, embeddedFunction) + ".a"))
                        {
                            buffer = s.ReadToEnd();
                        }
                        input = input.Replace(embeddedFunction + "  \n\t\tRET", buffer);
                    }
                    catch (Exception)
                    {
                        Error.printError("File unreachable: " + Path.Combine(AdditionalFunctionPath, embeddedFunction + ".a")); 
                        return null;
                    }
                }
            }
            catch (Exception)
            {
                Error.printError("Additional Functions directory unreachable.");
                return null;
            }

            input=input.Insert(0,output);

            input = input.Insert(0, Starter);

            return input;
        }

        #region Additional Functions

        private static Variable VariableNameExists(string name, IEnumerable<Variable> list)
        {
            return list.FirstOrDefault(v => v.Name.Equals(name));
        }

        private static string GetMemExtension(int n)
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
                        Error.printError("Var Length", 1, false);
                        return null;
                }
            }

            private static string GetEmbeddedFunctionName(string operand, string type)
            {
                if (type.IsSigned())
                    return operand.OperandName() + "Signed";
                if (type.IsUnsigned())
                    return operand.OperandName() + "Unsigned";
                if (type.IsReal())
                    return operand.OperandName() + "Float";
                return operand.OperandName() + "Unknown";
            }

        private static void RefreshValue(Variable var,ICollection<Command> line)
            {
                if (var.Type.Length == 4)
                    return;
                var shift = var.Type.Length == 2 ? 16 : 24;
                var c = new Command {Instruction = "SHL"};
                c.Arguments[0] = c.Arguments[2] = var.Name;
                c.Arguments[1] = shift.ToHex9();
                //c.LoadStore[2] = true;
                c.Change[1] = false;
                //c = SetLabel(c);
                line.Add(c);
                c = new Command {Instruction = var.Type.Type.IsSigned() ? "ASHR" : "SHR"};
                c.Arguments[0] = c.Arguments[2] = var.Name;
                //c.LoadStore[2] = true;
                c.Arguments[1] = shift.ToHex9();
                c.Change[1] = false;
                //c = SetLabel(c);
                line.Add(c);
            }
    
/*
            private string GetNewVarName(IList<Variable> list, string name, string prefix)
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
*/

/*
            private string GetNewFunctName(string name)
            {
                if (_functionList == null)
                    return null;
                int i;
                for (i = 0; i < _functionList.Count; i++)
                {
                    if (_functionList[i].Name.Equals(name))
                        return String.Format("{0}{1:X4}","F", i);
                }
                return name;
            }
*/

            private static bool MatchingLinesRefresh(Command c1, Command c2)
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

            private static Variable FindVariable(IEnumerable<Variable> list, string name)
            {
                var found = false;
                var val = new Variable();
                foreach (var v in list.Where(v => v.Name.Equals(name)))
                {
                    found = true;
                    val = v;
                    break;
                }
                return found ? val : null;
            }

            private static Function FindFunciton(IEnumerable<Function> list, string name)
            {
                var found = false;
                Function val = null;
                foreach (var v in list.Where(v => v.Name.Equals(name)))
                {
                    found = true;
                    val = v;
                    break;
                }
                return found ? val : null;
            }

            private static Variable FindString(IEnumerable<Variable> list, string name)
            {
                var found = false;
                Variable val = null;
                name = System.Text.RegularExpressions.Regex.Unescape(name);
                foreach (var v in list.Where(v => v.ArrayString.Equals(name)))
                {
                    found = true;
                    val = v;
                    break;
                }
                return found ? val : null;
            }

        #endregion
    }
}
