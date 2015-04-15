using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExtensionMethods;

namespace Compiler3_0
{
    class PreParser
    {
        private string text;

        public List<string> headers = new List<string>();

        private List<string> strBackup = new List<string>();

        private List<string> structs = new List<string>();

        private string[] blockWords = {
                                          "else",
                                          "do",
                                      };

        public PreParser(string textToPreParse)
        {
            text = textToPreParse;
        }

        private bool RealQuotes(int idx)
        {
            bool real = true;
            while (idx-- > 0)
            {
                if (text[idx] == '\\')
                    real = !real;
                else
                    break;
            }
            return real;
        }

        private void RemoveStrings()
        {
            if (text == null)
                return;
            int idx1 = -1, idx2 = -1, i = 0;
            while ((idx1 = text.IndexOf('"', idx2 + 1)) > 0)
            {
                do
                {
                    idx2 = idx1;
                } while (!RealQuotes(idx2 = text.IndexOf('"', idx2 + 1)));
                if (idx2 < 0)
                {
                    text = null;
                    return;
                }
                strBackup.Add(text.Substring(idx1, idx2 - idx1 + 1));
                text = text.Remove(idx1, idx2 - idx1 + 1);
                text = text.Insert(idx1, string.Format("//\\\\string{0}", i));
                idx2 = idx1;
                i++;
            }
        }

        private void RestoreStrings()
        {
            int idx1 = -1, idx2 = -1;
            int i = 0;
            if (text == null)
                return;

            while ((idx1 = text.IndexOf("//\\\\string", idx2 + 1)) > 0)
            {
                idx1 = idx1+"//\\\\string".Length;
                idx2 = idx1;
                while (idx2<text.Length&&char.IsDigit(text[idx2]))
                    idx2++;
                i=int.Parse(text.Substring(idx1,idx2-idx1));
                idx2 = idx1 - "//\\\\string".Length + strBackup[i].Length;
                text = text.Replace("//\\\\string"+i.ToString(),strBackup[i]);

            }
        }

        private void GetPreprocessor()
        {
            int idx1 = 0, idx2 = 0, id;
            string line;
            if (text == null)
                return;
            while ((idx2 = text.IndexOf("\n", idx1)) >= 0)
            {
                line = text.Substring(idx1, idx2 - idx1 + 1);
                if (line.FirstWord().StartsWith("#") || line.Length <= 1)
                {
                    if ((id = line.ToLower().IndexOf("include")) >= 0)
                    {
                        id += "include".Length;
                        headers.Add(line.Substring(id).Trim().TrimStart('<', '"').TrimEnd('>', '"'));
                    }
                    text = text.Remove(idx1, line.Length);
                }
                else
                {
                    idx1 = idx2 + 1;
                    if (idx1 >= text.Length)
                        break;
                }
            }
        }

        private bool TestBlock(string s)
        {
            foreach (string b in blockWords)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(s, string.Format(@"\b{0}\b", System.Text.RegularExpressions.Regex.Escape(b))))
                    return true;
            }
            return false;
        }

        private void RemoveBraced()
        {
            int idx1 = 0, idx2 = 0;
            string buffer;
            if (text == null)
                return;
            while ((idx2 = text.IndexOf('}')) >= 0)
            {
                if ((idx1 = text.LastIndexOf('{', idx2)) < 0)
                {
                    text = null;
                    return;
                }

                buffer = text.Substring(idx1, idx2 - idx1 + 1);
                text = text.Remove(idx1, idx2 - idx1 + 1);

                idx2 = idx1;
                while (idx2>=text.Length||(idx2 >= 0 && text[idx2] != ')' && text[idx2] != ';' && text[idx2] != '}' && text[idx2] != '=' && text[idx2] != '{'))
                    --idx2;
                if (idx2 >= 0 && text[idx2] != '=' && (text[idx2] == ')' || TestBlock(text.Substring(idx2, idx1 - idx2 + 1))))
                {
                    while (idx2 >= 0 && text[idx2] != ';' && text[idx2] != '}' && text[idx2] != '{')
                        idx2--;
                    // Console.WriteLine(":::"+text.Substring(idx2+1,idx1-idx2) +":::");

                    text = text.Remove(idx2 + 1, idx1 - idx2 - 1);
                }
                else
                    text = text.Insert(idx1, buffer.Replace("{", @"<strukt>").Replace("}", @"<\strukt>").Replace('\n', ' ').Replace('\t', ' ').Replace("  ", " "));
            }

        }

        private void TrimText()
        {
            if (text == null)
                return;
            text = "int main(){" + text.Replace('\n', ' ').Replace('\t', ' ').Replace("  ", " ").Replace("<strukt>", "{").Replace("<\\strukt>", "}") + "return 0; }";
        }

        public string GetGlobals()
        {
            RemoveStrings();
            GetPreprocessor();
            RemoveBraced();
            TrimText();
            RestoreStrings();
            //Console.WriteLine(text);
            return text;
        }
    }
}
