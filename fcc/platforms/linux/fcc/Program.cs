using System;
using System.IO;

namespace fcc
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            string fileToCompile = null;
            var fileToCompileSet = false;
            string outputFile = null;

            Gcc.Import ();

            if (args.Length == 0)
            {
                Error.PrintHelp();
                return;
            }
            for (var i = 0; i < args.Length; i++)
                if (args[i].StartsWith("-"))
                {
                    #region Process options

                        switch (args[i])
                        {
                            case "-o":
                            case "-out":
                            case "-output":
                            case "-output-file":
                                try
                                {
                                    outputFile=Path.GetFullPath(args[++i]);
                                }
                                catch
                                {
                                    Error.PrintError("Invalid output path.", 1, false);
                                }
                                break;
                            case "--help":
                            case "-help":
                            case "-?":
                            case "-h":
                            case "-m":
                            case "-man":
                            case "-manual":
                            case "-usage":
                            case "-howto":
                            case "-tutorial":
                                Error.PrintHelp();
                                Environment.Exit(0);
                                break;
                            case "--todo":
                            case "-todo":
                            case "-unimplemented":
                            case "-td":
                            case "-bug-report":
                                Error.PrintToDo();
                                Environment.Exit(0);
                                break;
                            case "-reset":
								Console.WriteLine ("Do you really want to reset application data to defaults? [y/n]");
								var ans = Console.ReadLine();
								if(!ans.ToLower().Equals("y"))
								{
									Console.WriteLine ("Reset aborted.");
									return;
								}
								Gcc.Import ();
                                break;
                            case "-import-sys-headers":
                            case "-import-system-headers":
                            case "-importsysheaders":
                                try
                                {
                                    if(!args[++i].ToLower().Equals("null"))
                                        Path.GetFullPath(args[i]);
                                }
                                catch
                                {
                                    Error.PrintError("Invalid import path.", 1, false);
                                }
                                if (args[i].ToLower().Equals("null"))
                                {
                                    if (!Gcc.CopyHeaders(null, false, true))
                                        Error.PrintError("Couldn't import system headers.", 1, false);
                                }
                                else
                                {
                                    if (!Gcc.CopyHeaders(args[i], false, true))
                                        Error.PrintError("Couldn't import system headers.", 1, false);
                                }
                                break;
                            case "-import-user-headers":
                            case "-importuserheaders":
                            case "-user-header-folder":
                            case "-using-headers":
                                try
                                {
                                    if (!args[++i].ToLower().Equals("null"))
                                        Path.GetFullPath(args[i]);
                                }
                                catch
                                {
                                    Error.PrintError("Invalid import path.", 1, false);
                                }
                                if (!Gcc.CopyHeaders(args[i],true, true))
                                    Error.PrintError("Couldn't import user headers.", 1, false);
                                break;
                            case "-print-application-data":
                            case "-print-app-data":
                            case "-printappdata":
                                Console.WriteLine (AppData.AppDataPath);
								return;
                            default:
                                Error.PrintError("Option " + args[i] + " is not a valid option.", 64, true);
                                break;
                        }

                    #endregion
                }
                else if (!fileToCompileSet)
                {
                    var fileToCompileTemp = args[i];

                    if (!fileToCompileTemp.ToLower().EndsWith(".c"))
                        Error.PrintError("Compiling only files with following extensions: *.c *.C", 65,true);
                    if (!File.Exists(fileToCompileTemp))
                        Error.PrintError("File not found: "+fileToCompileTemp, 66, true);
                    fileToCompile = fileToCompileTemp;
                    fileToCompileSet = true;

                    if (outputFile == null)
                        outputFile = fileToCompileTemp.Remove(fileToCompileTemp.LastIndexOf("."))+AppData.OutputExtension;
                }
                else
                    Error.PrintError("Can't parse your request",127,true);

            //check if there is something to compile
            if (!fileToCompileSet)
                return; //nothing to do, end.

            //get gimple
            if (!Gcc.Compile(fileToCompile))
                Error.PrintError("Error occured during preprocessing.",1,false);

            //run compiler
            new Compiler(AppData.GccPath, outputFile, AppData.GccGPath, Gcc.PreParser.Headers);
        }
    }
}