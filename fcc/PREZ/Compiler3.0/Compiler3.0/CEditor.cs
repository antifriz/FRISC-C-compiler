using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Compiler3_0
{
    public partial class CEditor : Form
    {
        internal bool textChanged = false;

        public CEditor()
        {
            InitializeComponent();
        }

        public CEditor(string f)
        {
            if (filePath != null && !File.Exists(filePath))
            {
                MessageBox.Show("File is missing!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            InitializeComponent();
            filePath = f;
            Text = Path.GetFileName(filePath);
            if (f == null)
            {
                textBox.Text = "int main ()\n{\n\t\n\treturn 0;\n}\n";
                textBox.Select(15, 0);
            }
            else
                textBox.Text = File.ReadAllText(f);
            this.Show();
        }

        public bool Save(string s)
        {
            if (s != null)
            {
                return false;
            }
            if (!s.EndsWith(".c"))
            {
                if (s.Contains("."))
                {
                    s = s.Remove(s.LastIndexOf("."));
                }
                s += ".c";
            }
            File.WriteAllText(s, textBox.Text);
            return true;
        }

        internal string filePath = null;

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            textChanged = true;
        }


    }
}
