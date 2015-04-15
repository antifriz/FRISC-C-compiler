using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace fcc
{
    public static class Gcc
    {
        #region Variables

            public static PreParser PreParser;

        #endregion

        //PUBLIC METHODS

        public static void Import()
        {
			if (Directory.Exists(AppData.AppDataPath))
				return;
			Error.PrintWarning("Application directory not found.\n\tDefault one will be created.\n\tIgnore this warning if this is first time use.");
			if(!CopyDirectory (AppData.ToCopyPath,AppData.AppDataPath))
				Error.PrintError ("Cannot create application directory.");
        }

        public static bool Compile(string inInputPath)
        {				
			//extract global variables to working dir
            string inputBuffer;
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
                File.WriteAllText(AppData.PreGccGPath, globals);
            }
            catch (Exception)
            {
                return false; //TODO: couldn't save globals, abort!
            }
			
			//copy input to working dir
			try
			{
				File.Copy(inInputPath,AppData.PreGccPath,true);
			}
			catch
			{
				return false; //TODO: couldn't copy input file to working dir, abort!
			}
			
			//delete old outputs
            try
            {
                File.Delete(AppData.ErrDumpPath);
                File.Delete(AppData.GccGPath);
                File.Delete(AppData.GccPath);
            }
            catch (Exception)
            {
                return false; //TODO: couldn't delete old output files, abort!
            }
			
			//precompiling		
			bool val;
			val = RunGcc ("-S -fdump-tree-gimple -ffreestanding"
			               			+" -I"+AppData.SystemHeadersPath
			               			+" -I"+AppData.UserHeadersPath
			               			+" "+ AppData.PreGccPath);
			val &=RunGcc ("-S -fdump-tree-gimple -ffreestanding"
			                     	+" -I"+AppData.SystemHeadersPath
			               			+" -I"+AppData.UserHeadersPath
			               			+" "+ AppData.PreGccGPath);
			if(!val)
				return false;
			
			//read main error log
            if (!File.Exists(AppData.ErrDumpPath))
                return false; //TODO: error log not created, abort!
            string errorLog;
            try
            {
                errorLog = File.ReadAllText(AppData.ErrDumpPath);
            }
            catch (Exception)
            {
                return false; //TODO: can't read error log, abort!
            }
            if (errorLog.Length != 0)
            {
                Console.WriteLine(errorLog);
				Environment.Exit(1); //TODO: compiling error, abort! 
            }
			
            //check whether output was created or not
            if (!File.Exists(AppData.GccPath) || !File.Exists(AppData.GccGPath))
                return false; //TODO: gcc didn't create output, abort!
            
			//clear gcc output
            try
            {
                File.WriteAllText(AppData.GccPath, ConvertGimple(File.ReadAllText(AppData.GccPath)));
                File.WriteAllText(AppData.PreGccGPath, ConvertGimple(File.ReadAllText(AppData.GccGPath)));
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
			if(areUserHeaders)
				toPath=AppData.UserHeadersPath;
			else
				toPath=AppData.SystemHeadersPath;

            try
            {
                var files = Directory.GetFiles(fromPath).OrderBy(f => f);
                foreach (var fil in files)
                    if (fil.ToLower().EndsWith(".h"))
                    {
                        if (!overwrite && File.Exists(Path.Combine(toPath,Path.GetFileName(fil)))) 
					    		continue;
                        File.Copy(fil, Path.Combine(toPath, Path.GetFileName(fil)), true);
                    }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
 
        //PRIVATE METHODS

        private static void RemoveUserHeaders()
        {
			if(Directory.Exists (AppData.UserHeadersPath))
	            try
	            {
					DirectoryInfo d = new DirectoryInfo(AppData.UserHeadersPath);
					foreach (FileInfo file in d.GetFiles())
						file.Delete(); 
					foreach (DirectoryInfo dir in d.GetDirectories())
					    dir.Delete(true); 
	            }
	            catch (Exception)
	            {
	                Error.PrintWarning("Couldn't clear temporary user headers.");
	            }
			Directory.CreateDirectory (AppData.UserHeadersPath);	
        }

		private static bool CopyDirectory(string sourcePath, string destinationPath)
		{
			try
			{
				//create all subdirectories
				foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", 
				    SearchOption.AllDirectories))
				    Directory.CreateDirectory(dirPath.Replace(sourcePath, destinationPath));
				
				//Copy all the files
				foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", 
				    SearchOption.AllDirectories))
				    File.Copy(newPath, newPath.Replace(sourcePath, destinationPath));
			}
			catch
			{
				return false;
			}
			return true;
		}
		
        private static bool RunGcc(string arguments)
        {
            var pProcess = new Process();
            try
            {
                pProcess.StartInfo.FileName = "gcc";
                pProcess.StartInfo.Arguments = arguments;
                pProcess.StartInfo.UseShellExecute=false;
				pProcess.StartInfo.RedirectStandardError=true;
				pProcess.StartInfo.WorkingDirectory=AppData.AppDataPath;
                pProcess.Start();
                pProcess.WaitForExit();
				File.AppendAllText (AppData.ErrDumpPath, pProcess.StandardError.ReadToEnd());
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