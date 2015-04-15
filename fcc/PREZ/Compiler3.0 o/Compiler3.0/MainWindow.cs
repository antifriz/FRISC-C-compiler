using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using Compiler1._0;

namespace Compiler3_0
{
    public partial class MainWindow : Form
    {
        #region Variables

            private int childFormNumber = 0;

            private string userPath=null;

            private string cygwinPath=null;

            private string gimplingDirectory = @"data";

            private string fileToStdErrPath = @"stdErr.txt";

            private string fileToGimplePath = @"togimple.c";

            private string globalToGimplePath = @"global.c";

            private string gimpleFilePath = @"togimple.c.004t.gimple";

            private string gimpleGlobalPath = @"global.c.004t.gimple";

            private string headerDirectory = @"header";

            private string gimpleFileText = null;

            private string defaultValuesPath = @".default";

            private string defaultCygwinPath = @"C:\cygwin";

            private string atlasTemplateLocation = @"data\AtlasTemplate.bat";

            private string atlasTempFolder = @"data\";

            private string atlasTempName = @"temp";

            private string atlasBatLocation = @"data\Atlas.bat";

            private string assemblerOutputSubfolder = "assembler";

            private string atlasPath = null;

            private string defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FRISC Compiler");

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            MdiChildActivate += new EventHandler(MainWindow_MdiChildActivate);

            FetchDefaultValues();

            CreateFolder(defaultPath);

            CreateFolder(Path.Combine(defaultPath,assemblerOutputSubfolder));

            CheckCygwin();

            CheckAtlas();

            SaveDefaultValues();
        }

        void MainWindow_MdiChildActivate(object sender, EventArgs e)
        {
            if (ActiveMdiChild == null)
            {
                this.Text = "FRISC C Compiler";
                return;
            }
            CEditor ce = (CEditor)ActiveMdiChild;
            this.Text = "FRISC C Compiler"+ce.Text;
        }

        #region Cygwin

            private void CheckCygwin()
            {
                if (userPath != null && cygwinPath != null)
                {
                    return;
                }
                if (MessageBox.Show("Cygwin not imported! Is Cygwin installed?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                {
                    if (MessageBox.Show("Cygwin is necessary to run this program. Do you want to install Cygwin?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                    {
                        this.Close();
                        Environment.Exit(0);
                    }
                    InstallCygwin();
                }
                ImportCygwin();
            }

            private void ImportCygwin()
            {
                MessageBox.Show("Please select Cygwin root folder", "Importing", MessageBoxButtons.OK, MessageBoxIcon.Information);
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                if (cygwinPath != null)
                {
                    CreateFolder(cygwinPath);
                    fbd.SelectedPath = cygwinPath;
                }
                else
                {
                    CreateFolder(defaultCygwinPath);
                    fbd.SelectedPath = defaultCygwinPath;
                }
                if (fbd.ShowDialog() != DialogResult.OK)
                {
                    if (cygwinPath != null)
                        return;
                    else
                    {
                        this.Close();
                        Environment.Exit(0);
                    }
                }
                cygwinPath = fbd.SelectedPath;
                EditCygwin();
            }

            private void EditCygwin()
            {
                if (!Directory.Exists(Path.Combine(cygwinPath, "bin")))
                {
                    MessageBox.Show("Can't find " + Path.Combine(cygwinPath, "bin") + ".", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    Environment.Exit(0);
                }
                string homePath = Path.Combine(cygwinPath, "home");
                if(!Directory.Exists(homePath))
                {
                    MessageBox.Show("Can't find " + Path.Combine(cygwinPath, "home") + ".", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    Environment.Exit(0);
                }
                string[] userPaths = Directory.GetDirectories(homePath);
                if (userPaths.Length ==0)
                {
                    MessageBox.Show(Path.Combine(cygwinPath, "home") + " has no directories.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    Environment.Exit(0);
                }
                if (!File.Exists(Path.Combine(cygwinPath, "Cygwin.bat")))
                {
                    File.Create(Path.Combine(cygwinPath, "Cygwin.bat"));
                    File.WriteAllText(Path.Combine(cygwinPath, "Cygwin.bat"),@"bin\bash --login -i");
                }
                Process n = new Process();
                n.StartInfo.FileName = Path.Combine(cygwinPath, "Cygwin.bat");
                n.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                n.Start();
                n.WaitForExit(1500);
                if (n.HasExited == false)
                    n.Kill();
                userPath = Path.Combine(homePath, userPaths[0]);
                string bashProfilePath = Path.Combine(userPath, ".bash_profile");
                CreateFolder(Path.Combine(userPath, gimplingDirectory));
                string headerDirFullPath = Path.Combine(userPath, Path.Combine(Path.Combine(userPath, gimplingDirectory), headerDirectory));
                CreateFolder(headerDirFullPath);
                string[] files = Directory.GetFiles(@"ToCopy");
                foreach (string fil in files)
                {
                    File.Copy(fil, Path.Combine(headerDirFullPath, Path.GetFileName(fil)), true);
                }
                string command = "cd " + gimplingDirectory + "\ngcc -fdump-tree-gimple -nostdinc -iquote/home/"+userPath.Remove(0,userPath.LastIndexOf('\\')+1)+"/"+gimplingDirectory+"/"+ headerDirectory + " " + fileToGimplePath + " 2>" + fileToStdErrPath+"\ngcc -fdump-tree-gimple -nostdinc -iquote/home/"+userPath.Remove(0,userPath.LastIndexOf('\\')+1)+"/"+gimplingDirectory+"/"+ headerDirectory + " " + globalToGimplePath +"\nexit\n";
                if (File.Exists(bashProfilePath))
                {
                    try
                    {
                        File.Copy(bashProfilePath, bashProfilePath + "_old");
                    }
                    catch
                    {

                    }
                }
                File.WriteAllText(bashProfilePath, command);
                MessageBox.Show("Cygwin successfully imported", "Done!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }

            private void InstallCygwin()
            {
                MessageBox.Show("Cygwin will now be installed. Please choose install location.", "Proceeding to instalation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                CreateFolder(defaultCygwinPath);
                fbd.SelectedPath = defaultCygwinPath;
                if (fbd.ShowDialog() != DialogResult.OK)
                {
                    if (cygwinPath != null)
                        return;
                    else
                    {
                        this.Close();
                        Environment.Exit(0);
                    }
                }
                Process install = new Process();
                install.StartInfo.FileName = @"installCygwin\setup-x86.exe";
                install.StartInfo.Arguments = @"-q -n -N -d -R "+fbd.SelectedPath+@" -s http://box-soft.com -l C:\cygwinDownloadedPackages -P gcc-g++";
                install.Start();
                install.WaitForExit();
            }

        #endregion

        #region Atlas

            private void CheckAtlas()
            {
                if (atlasPath != null)
                {
                    return;
                }
                if (MessageBox.Show("Atlas not imported! Is Atlas installed?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                {
                    if (MessageBox.Show("Atlas is necessary to run this program. Do you want to install Atlas?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        InstallAtlas();
                    }
                    this.Close();
                    Environment.Exit(0);
                }
                ImportAtlas();
            }

            private void ImportAtlas()
            {
                MessageBox.Show("Please select Atlas/cygwin folder", "Importing", MessageBoxButtons.OK, MessageBoxIcon.Information);
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                if (atlasPath != null)
                {
                    CreateFolder(cygwinPath);
                    fbd.SelectedPath = atlasPath;
                }
                else
                {
                    fbd.SelectedPath=Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                }
                if (fbd.ShowDialog() != DialogResult.OK)
                {
                    if (atlasPath != null)
                        return;
                    else
                    {
                        this.Close();
                        Environment.Exit(0);
                    }
                }
                atlasPath = fbd.SelectedPath;
                EditAtlas();
            }

            private void EditAtlas()
            {
                if (!Directory.Exists(Path.Combine(cygwinPath, "bin")))
                {
                    MessageBox.Show("Can't find " + Path.Combine(cygwinPath, "bin") + ".", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    Environment.Exit(0);
                }
                string xwinPath = Path.Combine(atlasPath, @"bin\startxwin.exe");
                if (!File.Exists(xwinPath))
                {
                    MessageBox.Show("Can't find startxwin.exe in bin folder.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    Environment.Exit(0);
                }
                string buffer = File.ReadAllText(atlasTemplateLocation).Replace("%ATLASLOCATION%",atlasPath);
                File.WriteAllText(atlasBatLocation,buffer);
                MessageBox.Show("Atlas successfully imported", "Done!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }

            private void InstallAtlas()
            {
                Process.Start(@"http://staticweb.rasip.fer.hr/research/atlas/download/");
            }

        #endregion

        #region Additional Functions

            private void CreateFolder(string s)
            {
                if (!Directory.Exists(s))
                {
                    Directory.CreateDirectory(s);
                }
            }

            private void CreateFile(string s)
            {
                if (!File.Exists(s))
                {
                    File.Create(s);
                }
            }

            private string ConvertGimple(string s)
            {
                int idx2;
                int idx;
                while ((idx = s.IndexOf("try" + Environment.NewLine)) >= 0)
                {
                    idx = s.LastIndexOf(Environment.NewLine, idx);
                    idx2 = s.IndexOf("{" + Environment.NewLine, idx + 1);
                    if (idx2 >= 0)
                    {
                        s = s.Remove(idx, idx2 - idx + 1);
                        idx = s.IndexOf("}" + Environment.NewLine, idx);
                        idx2 = s.IndexOf("}" + Environment.NewLine, idx + 1);
                        idx = s.LastIndexOf(Environment.NewLine, idx);
                        int i = s.Length;

                        s = s.Remove(idx, idx2 - idx + 1);
                    }
                }
                s = s.Replace(Environment.NewLine + "      ", Environment.NewLine + "  ");
                return s;
            }

            private void ShowNewFormFunct(string filePath)
            {
                if (filePath != null && !File.Exists(filePath))
                {
                    MessageBox.Show("File is missing!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                CEditor childForm = new CEditor(filePath);

                childForm.MdiParent = this;
                childForm.WindowState = FormWindowState.Maximized;
                ++childFormNumber;

                if (childForm.Text == "")
                    childForm.Text = "Untitled document (" + childFormNumber + ")";
                childForm.FormClosing += new FormClosingEventHandler(childForm_FormClosing);
            }

        #endregion

        #region Default Values

            private void FetchDefaultValues()
            {
                if (!File.Exists(defaultValuesPath))
                    return;
                System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(DefaultValues));
                DefaultValues dv = new DefaultValues();
                using (StreamReader file = new StreamReader(defaultValuesPath))
                {
                    try
                    {
                        dv = (DefaultValues)reader.Deserialize(file);
                    }
                    catch(Exception)
                    {
                        return;
                    }
                }
                cygwinPath = dv.cygwinPath;
                userPath = dv.userPath;
                atlasPath = dv.atlasPath;
            }

            private void SaveDefaultValues()
            {
                CreateFile(defaultValuesPath);
                DefaultValues dv = new DefaultValues(cygwinPath, userPath,atlasPath);
                System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(DefaultValues));
                using (StreamWriter file = new StreamWriter(defaultValuesPath))
                {
                    writer.Serialize(file, dv);
                }
            }

        #endregion


        private void Compile()
        {
            if (ActiveMdiChild == null)
            {
                MessageBox.Show("No document selected!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            CEditor ce = (CEditor)ActiveMdiChild;
            string dataFolder = Path.Combine(userPath, gimplingDirectory);
            File.WriteAllText(Path.Combine(dataFolder,fileToGimplePath), ce.textBox.Text);
            PreParser p = new PreParser(ce.textBox.Text);
            string glob = p.GetGlobals();
            File.WriteAllText(Path.Combine(dataFolder, globalToGimplePath), glob);
            string binPath = Path.Combine(cygwinPath, "bin");
            string bashPath = Path.Combine(binPath, "bash.exe");
            if (File.Exists(Path.Combine(dataFolder,gimpleFilePath)))
                File.Delete(Path.Combine(dataFolder, gimpleFilePath));
            if (File.Exists(Path.Combine(dataFolder, gimpleGlobalPath)))
                File.Delete(Path.Combine(dataFolder, gimpleGlobalPath));
            if (File.Exists(Path.Combine(dataFolder, fileToStdErrPath)))
                File.Delete(Path.Combine(dataFolder, fileToStdErrPath));
            Process n = new Process();
            n.StartInfo.FileName = bashPath;
            n.StartInfo.Arguments = "--login -i";
            n.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            n.Start();
            n.WaitForExit();

            tabControl1.Height = 150;

            if (!File.Exists(Path.Combine(dataFolder, fileToStdErrPath)))
            {
                this.CompileLogText.Text = "error: missing gimple log";
                return;
            }
            using (StreamReader s = new StreamReader(Path.Combine(dataFolder, fileToStdErrPath)))
                CompileLogText.Text = s.ReadToEnd().Replace("\n", Environment.NewLine);
            if (CompileLogText.Text.Length != 0)
                return;
            if (!File.Exists(Path.Combine(dataFolder, gimpleFilePath)) || !File.Exists(Path.Combine(dataFolder,gimpleGlobalPath)))
            {
                gimpleFileText = null;
                return;
            }

            using (StreamReader s = new StreamReader(Path.Combine(dataFolder,gimpleFilePath)))
            {
                gimpleFileText = ConvertGimple(s.ReadToEnd().Replace("\n", Environment.NewLine));
            }

            using (StreamReader s = new StreamReader(Path.Combine(dataFolder, gimpleGlobalPath)))
            {
                glob = ConvertGimple(s.ReadToEnd().Replace("\n", Environment.NewLine));
            }

            File.WriteAllText(Path.Combine(dataFolder,gimpleFilePath), gimpleFileText);
            File.WriteAllText(Path.Combine(dataFolder, gimpleGlobalPath), glob);
            string parserOutput;
            if(ce.Text.Contains('.'))
                parserOutput = ce.Text.Remove(ce.Text.LastIndexOf('.')).Trim() + ".a";
            else
                parserOutput = ce.Text.Trim() + ".a";
            List<Command> line = new List<Command>();
            Compiler parser = new Compiler();
            parser.Compile(Path.Combine(dataFolder, gimpleFilePath), Path.Combine(Path.Combine(defaultPath, assemblerOutputSubfolder), parserOutput), Path.Combine(dataFolder, gimpleGlobalPath),p.headers);
            
            string buffer = File.ReadAllText(Path.Combine(Path.Combine(defaultPath, assemblerOutputSubfolder), parserOutput));
            File.WriteAllText(Path.Combine(atlasTempFolder, atlasTempName)+".a", buffer);
            File.Delete(Path.Combine(atlasTempFolder, atlasTempName)+".p");


            n = new Process();
            n.StartInfo.FileName = atlasBatLocation;
            n.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            n.Start();
            n.WaitForExit();

            int i;
            for (i = 0; i < 10; i++)
            {
                System.Threading.Thread.Sleep(400);
                if (File.Exists(Path.Combine(atlasTempFolder, atlasTempName) + ".p"))
                    break;
            }
            if (i == 10)
            {
                Process.Start(Path.Combine(Path.Combine(defaultPath, assemblerOutputSubfolder), parserOutput));
                return;
            }
       
            for (i = 0; i < 10; i++)
            {
                System.Threading.Thread.Sleep(200);
                Process[] proc = Process.GetProcessesByName("XWin");
                if (proc.Count() > 0)
                {
                    proc[0].Kill();
                    break;
                }
            }
            File.Copy(Path.Combine(atlasTempFolder, atlasTempName)+".p",Path.Combine(Path.Combine(defaultPath, assemblerOutputSubfolder),parserOutput.Replace(".a",".p")),true);
            //Process.Start(defaultPath);
            File.Copy(Path.Combine(defaultPath, Path.Combine(Path.Combine(defaultPath, assemblerOutputSubfolder), parserOutput.Replace(".a", ".p"))), @"C:\Users\Ivan\Documents\atlas\FRISCIRANJE\vj0.p", true);
            //Process.Start(Path.Combine(defaultPath, Path.Combine(Path.Combine(defaultPath, assemblerOutputSubfolder),parserOutput.Replace(".a",".p"))));

            n = new Process();
            n.StartInfo.FileName = @"C:\Users\Ivan\Documents\atlas\FRISCIRANJE\Kruh 64-bit.lnk";
            n.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            n.StartInfo.WorkingDirectory = @"C:\Users\Ivan\Documents\atlas\FRISCIRANJE\";
            n.StartInfo.Arguments = @"PtoBit.bat vj0 vj00";
            n.Start();
            n.WaitForExit();
            
            CompileLogText.Text += "Compiling completed!";
        }

        #region Clicks

        private void ShowNewForm(object sender, EventArgs e)
        {
            ShowNewFormFunct(null);
        }

        void childForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveAs(false);
        }

        private void OpenFile(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = defaultPath;
            openFileDialog.Filter = "C files(*.c)|*.c|All Files (*.*)|*.*";
            openFileDialog.Title = "Open file";
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string FileName = openFileDialog.FileName;
                //defaultPath = openFileDialog.InitialDirectory;
                ShowNewFormFunct(FileName);
            }

        }

        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveAs(true);
        }

        private void SaveAs(bool saveBlank)
        {
            if (ActiveMdiChild == null)
            {
                return;
            }
                CEditor ce = (CEditor)ActiveMdiChild;
            if (!ce.textChanged)
                return;
            if (ce.textBox.Text.Length == 0)
            {
                if(saveBlank)
                    if (MessageBox.Show("File is blank, are you sure you want to save it?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                        return;
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = defaultPath;
            saveFileDialog.Filter = "C files(*.c)|*.c|All Files (*.*)|*.*";
            saveFileDialog.Title = "Save file";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string FileName = saveFileDialog.FileName;
                //defaultPath = saveFileDialog.InitialDirectory;
                File.WriteAllText(FileName, ce.textBox.Text);
            }
        }

        private void ExitToolsStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form f in MdiChildren)
            {
                ActivateMdiChild(f);
                childForm_FormClosing(null, null);
            }
            this.Close();
        }

        private void CutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CEditor ActiveChild = (CEditor)this.ActiveMdiChild;
            Clipboard.SetText(ActiveChild.textBox.SelectedText);
            ActiveChild.textBox.Text= ActiveChild.textBox.Text.Remove(ActiveChild.textBox.SelectionStart, ActiveChild.textBox.SelectionLength);
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CEditor ActiveChild = (CEditor)this.ActiveMdiChild;
            Clipboard.SetText(ActiveChild.textBox.SelectedText);
        }

        private void PasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CEditor ActiveChild = (CEditor) this.ActiveMdiChild;
            string text;
            text=Clipboard.GetText();
            ActiveChild.textBox.Text = ActiveChild.textBox.Text.Remove(ActiveChild.textBox.SelectionStart, ActiveChild.textBox.SelectionLength);
            ActiveChild.textBox.Text = ActiveChild.textBox.Text.Insert(ActiveChild.textBox.SelectionStart, text);

        }

        private void ToolBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStrip.Visible = toolBarToolStripMenuItem.Checked;
        }

        private void StatusBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            statusStrip.Visible = statusBarToolStripMenuItem.Checked;
        }

        private void CascadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
        }

        private void TileVerticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileVertical);
        }

        private void TileHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void ArrangeIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.ArrangeIcons);
        }

        private void CloseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form childForm in MdiChildren)
            {
                childForm.Close();
            }
        }

        private void tabControl1_MouseClick(object sender, MouseEventArgs e)
        {
            if (tabControl1.Height == 25)
                tabControl1.Height = 150;
            else
                tabControl1.Height = 25;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //CEditor ceditor = (CEditor) this.ActiveMdiChild;
            SaveAsToolStripMenuItem_Click(null, null);
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            SaveAsToolStripMenuItem_Click(null, null);
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportCygwin();
        }

        private void installToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstallCygwin();
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveDefaultValues();
        }

        private void compileToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Compile();
        }

        private void installToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            InstallAtlas();
        }

        private void importToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ImportAtlas();
        }

#endregion 
    }
}
