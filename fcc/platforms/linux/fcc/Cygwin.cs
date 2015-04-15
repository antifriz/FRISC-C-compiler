using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace fcc
{
    public static class Cygwin
    {
        #region Variables

            public static string ErrorLog = null;

            public static PreParser PreParser = null;

            public static string OutputPath = null;

            public static string OutputGlobalPath = null;

            public static string CygwinDownloadSite = "http://box-soft.com";


            private static string _userPath;

            private static string _cygwinPath;

            private static string _bashPath;


            private static int _usrCnt = 1;

            public static string DataDirectory = AppData.NameUnderscore + @"\data\";

            public const string HeaderDirectory = @"headers";

            private const string UserHeaderDirectory = @"user";
        
            private const string FileToStdErrPath = @"stdErr.txt";

            private const string FileToGimplePath = @"togimple.c";

            private const string GlobalToGimplePath = @"global.c";


            private const string GimpleFilePath = @"togimple.c.004t.gimple";

            private const string GimpleGlobalPath = @"global.c.004t.gimple";

        #endregion

        //PUBLIC METHODS

        public static bool Initialize()
        {
            GetDefaultValues();
            if (IsImported())
                return true;
            return false;
        }

        public static bool SetUsrCnt(string inUsrCnt)
        {
            int tp;
            if (!int.TryParse(inUsrCnt, out tp))
                return false;
            if (tp > 0)
                _usrCnt = tp;
            return true;
        }

        public static bool SetDownloadSite(string cygwinDownloadSite)
        {
            if(!Uri.IsWellFormedUriString(cygwinDownloadSite,UriKind.Absolute))
                return false;
            return true;
        }

        public static bool Import(string inCygwinPath)
        {
            //check bash
            var bashPathTemp = Path.Combine(inCygwinPath, @"bin\bash.exe");
            if (!File.Exists(bashPathTemp))
                return false; //TODO: cygwin/bin/bash missing, abort!

            //check home
            var homePath = Path.Combine(inCygwinPath, "home");
            if (!Directory.Exists(homePath))
            {
                RunExecutable(bashPathTemp, "--login -i", 1500);
                if (!Directory.Exists(homePath))
                    return false; //TODO: directory doesn't exist, abort!
            }

            //check usr
            var usrPaths = Directory.GetDirectories(homePath);
            if (_usrCnt >= usrPaths.Length)
            {
                RunExecutable(bashPathTemp, "--login -i", 1500);
                usrPaths = Directory.GetDirectories(homePath);
                if (_usrCnt > usrPaths.Length)
                    return false; //TODO: usr doesn't exist, abort!
            }
            var usrPath = usrPaths[_usrCnt - 1];
            _userPath = usrPath;

            //copy sys headers to /FRISC/data/header
            if (!CopyHeaders(null, false, false))
                Error.PrintWarning("Couldn't import system headers.");


            //save old .bash_profile
            var profilePath = Path.Combine(usrPath, ".bash_profile");
            if (File.Exists(profilePath))
                try
                {
                    File.Copy(profilePath, profilePath + "_old");
                }
                catch (Exception)
                {
                }

            //create new .bash_profile
            var bashCommands = "cd " + DataDirectory.Replace(@"\", "/") + "\ngcc -S -fdump-tree-gimple -nostdinc -iquote/home/" + usrPath.Remove(0, usrPath.LastIndexOf('\\') + 1) + "/" + DataDirectory.TrimEnd('\\').Replace(@"\", "/") + "/" + HeaderDirectory + " " + FileToGimplePath + " 2>" + FileToStdErrPath + "\ngcc -fdump-tree-gimple -S -nostdinc -iquote/home/" + usrPath.Remove(0, usrPath.LastIndexOf('\\') + 1) + "/" + DataDirectory.TrimEnd('\\').Replace(@"\", "/") + "/" + HeaderDirectory + " " + GlobalToGimplePath + "\nexit\n";
            try
            {
                File.WriteAllText(profilePath, bashCommands);
            }
            catch (Exception)
            {
                return false; //TODO: couldn't write to .bash_profile
            }

            //store imported values
            var dataFolder = Path.Combine(usrPath, DataDirectory);
            var outputPath = Path.Combine(dataFolder, GimpleFilePath);
            var outputGlobalPath = Path.Combine(dataFolder, GimpleGlobalPath);
            OutputPath = outputPath;
            OutputGlobalPath = outputGlobalPath;
            _cygwinPath = inCygwinPath;
            _bashPath = bashPathTemp;
            if (!SetDefaultValues())
                Error.PrintWarning("Default values couldn't be saved. Set them again next time.");
            return true;
        }

        public static bool Install(string installPath)
        {
            Console.WriteLine("Cygwin instalation has ~250MB.");
            Console.WriteLine("Internet connection is neccessary.");
            Console.WriteLine("Installation will last few minutes.");
            Console.WriteLine("\nPress enter to start installation.");
            Console.ReadLine();
            if (!RunExecutable(@"installCygwin\setup-x86.exe", @"-q -n -N -d -R " + installPath + @" -s "+CygwinDownloadSite+" -l " + Path.Combine(installPath, "downloaded_packages") + "--no-admin -P gcc-g++", 0))
                return false;
            Import(installPath);
            return true;
        }

        public static bool Compile(string inInputPath)
        {
            PreParser = null;
            ErrorLog = null;

            //check sys headers
            if (!CopyHeaders(null, false, false))
                Error.PrintWarning("System headers possibly corrupt."); //couldn't check headers

            //copy to working dir (/home/usr/FRISC/data/)
            var dataFolder = Path.Combine(_userPath, DataDirectory);
            var inputPath = Path.Combine(dataFolder, FileToGimplePath);
            try
            {
                File.Copy(inInputPath, inputPath, true);
            }
            catch (Exception)
            {
                return false; //TODO: couldn't copy input file to working dir, abort!
            }

            //extract global variables
            string inputBuffer;
            var inputGlobalPath = Path.Combine(dataFolder, GlobalToGimplePath);
            try
            {
                inputBuffer = File.ReadAllText(inInputPath);
            }
            catch (Exception)
            {
                return false; //TODO: couldn't read input, abort!
            }
            var preParserTemp = new PreParser(inputBuffer);
            var globals = preParserTemp.GetGlobals();
            try
            {
                File.WriteAllText(inputGlobalPath, globals);
            }
            catch (Exception)
            {
                return false; //TODO: couldn't save globals, abort!
            }

            //delete old outputs
            var outputErrPath = Path.Combine(dataFolder, FileToStdErrPath);
            try
            {
                File.Delete(OutputPath);
                File.Delete(OutputGlobalPath);
                File.Delete(outputErrPath);
            }
            catch (Exception)
            {
                return false; //TODO: couldn't delete old output files, abort!
            }

            //get gimple output  
            RunExecutable(_bashPath, "--login -i", 0);

            //read error log
            if (!File.Exists(outputErrPath))
                return false; //TODO: error log not created, abort!
            string errorLog;
            try
            {
                errorLog = File.ReadAllText(outputErrPath).Replace("\n", Environment.NewLine);
            }
            catch (Exception)
            {
                return false; //TODO: can't read error log, abort!
            }
            if (errorLog.Length != 0)
            {
                Console.WriteLine(errorLog);
                ErrorLog = errorLog;
                return false; //TODO: compiling error, abort! 
            }

            //check whether output was created or not
            if (!File.Exists(OutputGlobalPath) || !File.Exists(OutputPath))
                return false; //TODO: gcc didn't create output, abort!

            //clear gcc output
            try
            {
                File.WriteAllText(OutputPath, ConvertGimple(File.ReadAllText(OutputPath).Replace("\n", Environment.NewLine)));
                File.WriteAllText(OutputGlobalPath, ConvertGimple(File.ReadAllText(OutputGlobalPath).Replace("\n", Environment.NewLine)));
            }
            catch
            {
                return false; //TODO: couldn't access output files, abort!
            }

            RemoveUserHeaders();
            PreParser = preParserTemp;
            return true;
        }

        public static bool CopyHeaders(string fromPath, bool areUserHeaders, bool overwrite)
        {
            string toPath;
            try
            {
                toPath = Path.Combine(_userPath, DataDirectory + HeaderDirectory);
            }
            catch (Exception)
            {
                return false;
            }
            if (fromPath == null)
                fromPath = AppData.ToCopyFolder;
            else if (areUserHeaders)
            {
                toPath = Path.Combine(toPath, UserHeaderDirectory);
            }

            if (!CreateFolder(toPath))
                return false; //TODO: couldn't create subdirectory /data/header
            try
            {
                var files = Directory.GetFiles(fromPath).OrderBy(f => f);
                foreach (var fil in files)
                    if (fil.ToLower().EndsWith(".h"))
                    {
                        if (!overwrite &&
                            File.Exists(Path.Combine(Path.Combine(_userPath, DataDirectory + HeaderDirectory),
                                Path.GetFileName(fil)))) continue;
                        File.Copy(fil, Path.Combine(Path.Combine(_userPath, DataDirectory + HeaderDirectory), Path.GetFileName(fil)), true);
                    }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static void GetDefaultValues()
        {
            if (!File.Exists(AppData.DefaultValuesFile))
                return;
            var reader = new System.Xml.Serialization.XmlSerializer(typeof(DefaultValuess));
            DefaultValuess dv;
            try
            {
                using (var file = new StreamReader(AppData.DefaultValuesFile))
                {

                        dv = (DefaultValuess)reader.Deserialize(file);

                }
            }
            catch (Exception)
            {
                return;
            }
            _cygwinPath = dv.CygwinPath;
            _userPath = dv.UserPath;
            _bashPath = dv.BashPath;
            OutputPath = dv.OutputPath;
            OutputGlobalPath = dv.OutputGlobalPath;
            CygwinDownloadSite = dv.CygwinDownloadSite;
            Compiler.IspraviFRISC3 = dv.IspraviFRISC3;
        }
 
        //PRIVATE METHODS

        private static void RemoveUserHeaders()
        {
            try
            {
                Directory.Delete(Path.Combine(Path.Combine(_userPath, DataDirectory + HeaderDirectory), UserHeaderDirectory), true);
            }
            catch (DirectoryNotFoundException)
            {
            }
            catch (Exception)
            {
                Error.PrintWarning("Couldn't clear temporary user headers.");
            }
        }

        private static bool SetDefaultValues()
        {
            if(!CreateFile(AppData.DefaultValuesFile))
                return false;
            File.Exists(AppData.DefaultValuesFile);
            var dv = new DefaultValuess(_cygwinPath, _userPath,_bashPath,OutputPath,OutputGlobalPath,CygwinDownloadSite,Compiler.IspraviFRISC3);
            var writer = new System.Xml.Serialization.XmlSerializer(typeof(DefaultValuess));
            try
            {
                using (var file = new StreamWriter(AppData.DefaultValuesFile))
                {
                    writer.Serialize(file, dv);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private static bool CreateFile(string s)
        {
            if (File.Exists(s)) return true;
            FileStream v = null;
            try
            {
                v =File.Create(s);
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (v != null) v.Close();
            }
            return true;
        }

        private static bool CreateFolder(string s)
        {
            if (Directory.Exists(s)) return true;
            try
            {
                Directory.CreateDirectory(s);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private static bool IsImported()
        {
            if (Directory.Exists(_cygwinPath))
            {
                if (Directory.Exists(_userPath))
                    return true;
                Error.PrintError("Cygwin path not set, use [-cimport path].",78,false);
                return false;
            }
            Error.PrintError(
                Directory.Exists(_userPath)
                    ? "Cygwin path not set, use [-cimport path]."
                    : "Cygwin path not set. Use [-cimport path]|[-cinstall path].", 78, false);
            return false;
        }

        private static bool RunExecutable(string exePath,string arguments,int wait)
        {
            var pProcess = new Process();
            try
            {
                pProcess.StartInfo.FileName = exePath;
                pProcess.StartInfo.Arguments = arguments;
                pProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                pProcess.Start();
                if (wait == 0)
                    pProcess.WaitForExit();
                else
                    pProcess.WaitForExit(wait);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private static string ConvertGimple(string s)
        {
            int idx;
            while ((idx = s.IndexOf("try" + Environment.NewLine)) >= 0)
            {
                idx = s.LastIndexOf(Environment.NewLine, idx);
                int idx2 = s.IndexOf("{" + Environment.NewLine, idx + 1);
                if (idx2 < 0) continue;
                s = s.Remove(idx, idx2 - idx + 1);
                idx = s.IndexOf("}" + Environment.NewLine, idx);
                idx2 = s.IndexOf("}" + Environment.NewLine, idx + 1);
                idx = s.LastIndexOf(Environment.NewLine, idx);

                s = s.Remove(idx, idx2 - idx + 1);
            }
            s = s.Replace(Environment.NewLine + "      ", Environment.NewLine + "  ");
            return s;
        }
    }
}