using System.IO;
using System;
namespace fcc
{
	static class AppData
    {
		//info
        public const string Name = "FRISC C compiler";
        public const string NameUnderscore = "FRISC_C_compiler";
        public const string Version = "v0.9.4.5 lite";
        public const string Author = "Ivan Jurin";
        public const string AuthorContact = "ivan.jurin@fer.hr";
        public const string Date = "14/12/2013";
		
		//data
		private const string AppDataFolderName = "frisc-c-compiler";

		public static readonly string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppDataFolderName);
		
        private const string AdditionalFunctionsFolder = @"additional_f";
        private const string BuiltInFunctionsFolder = @"built-in_f";
		private const string SystemHeadersFolder = @"headers/system";
        private const string UserHeadersFolder = @"headers/user";	
        private const string HelpFile = "help.txt";
        private const string ToDoFile = "todo.txt";
        private const string DefaultValuesFile = @".default";
		private const string PreGccFile = @"pregcc.c";
		private const string PreGccGFile = @"pregcch.c";
		private const string ErrDumpFile = @"errdmp.txt";
		
		private static readonly string GccFile = PreGccFile+@".004t.gimple";
		private static readonly string GccGFile = PreGccGFile+@".004t.gimple";
		
        private const string ToCopyFolder = @"to-copy";
		
		//private const string InstallPath = @"/usr/local/lib/frisc-c-compiler";

        private static readonly string InstallPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
		
		//appfolder
        public static readonly string AdditionalFunctionsPath = Path.Combine(AppDataPath,AdditionalFunctionsFolder);
        public static readonly string BuiltInFunctionsPath = Path.Combine(AppDataPath,BuiltInFunctionsFolder);
        public static readonly string SystemHeadersPath = Path.Combine(AppDataPath,SystemHeadersFolder);
        public static readonly string UserHeadersPath = Path.Combine(AppDataPath,UserHeadersFolder);
        public static readonly string HelpPath = Path.Combine(AppDataPath, HelpFile);
        public static readonly string ToDoPath = Path.Combine(AppDataPath, ToDoFile);
        public static readonly string PreGccPath = Path.Combine(AppDataPath, PreGccFile);
        public static readonly string PreGccGPath = Path.Combine(AppDataPath, PreGccGFile);
        public static readonly string ErrDumpPath = Path.Combine(AppDataPath, ErrDumpFile);
        public static readonly string GccPath = Path.Combine(AppDataPath, GccFile);
        public static readonly string GccGPath = Path.Combine(AppDataPath, GccGFile);
		
		//installfolder
		public static readonly string ToCopyPath = Path.Combine(InstallPath, ToCopyFolder);


        public static string Filename = "fcc";

        public const string OutputExtension = ".S";

        static AppData()
        {

        }
    }
}
