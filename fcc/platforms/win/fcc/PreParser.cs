using System.Collections.Generic;

namespace fcc
{
    public class PreParser
    {
        private static string _text;

        public List<string> Headers = new List<string>();

        private readonly static List<string> StrBackup = new List<string>();

        private readonly static string[] BlockWords = {
                                          "else",
                                          "do"
                                      };

        public PreParser(string textToPreParse)
        {
            _text = textToPreParse;
        }

        private static bool RealQuotes(int idx)
        {
            var real = true;
            while (idx-- > 0)
            {
                if (_text[idx] == '\\')
                    real = !real;
                else
                    break;
            }
            return real;
        }

        private static void RemoveStrings()
        {
            if (_text == null)
                return;
            int idx1, idx2 = -1, i = 0;
            while ((idx1 = _text.IndexOf('"', idx2 + 1)) > 0)
            {
                do
                {
                    idx2 = idx1;
                } while (!RealQuotes(idx2 = _text.IndexOf('"', idx2 + 1)));
                if (idx2 < 0)
                {
                    _text = null;
                    return;
                }
                StrBackup.Add(_text.Substring(idx1, idx2 - idx1 + 1));
                _text = _text.Remove(idx1, idx2 - idx1 + 1);
                _text = _text.Insert(idx1, string.Format("//\\\\string{0}", i));
                idx2 = idx1;
                i++;
            }
        }

        private static void RestoreStrings()
        {
            int idx1, idx2 = -1;
            if (_text == null)
                return;

            while ((idx1 = _text.IndexOf("//\\\\string", idx2 + 1)) > 0)
            {
                idx1 = idx1+"//\\\\string".Length;
                idx2 = idx1;
                while (idx2<_text.Length&&char.IsDigit(_text[idx2]))
                    idx2++;
                int i = int.Parse(_text.Substring(idx1,idx2-idx1));
                idx2 = idx1 - "//\\\\string".Length + StrBackup[i].Length;
                _text = _text.Replace("//\\\\string"+i,StrBackup[i]);

            }
        }

        private void GetPreprocessor()
        {
            int idx1 = 0, idx2;
            if (_text == null)
                return;
            while ((idx2 = _text.IndexOf("\n", idx1)) >= 0)
            {
                string line = _text.Substring(idx1, idx2 - idx1 + 1);
                if (line.FirstWord().StartsWith("#") || line.Length <= 1)
                {
                    int id;
                    if ((id = line.ToLower().IndexOf("include")) >= 0)
                    {
                        id += "include".Length;
                        Headers.Add(line.Substring(id).Trim().TrimStart('<', '"').TrimEnd('>', '"'));
                    }
                    _text = _text.Remove(idx1, line.Length);
                }
                else
                {
                    idx1 = idx2 + 1;
                    if (idx1 >= _text.Length)
                        break;
                }
            }
        }

        private static bool TestBlock(string s)
        {
            foreach (var b in BlockWords)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(s, string.Format(@"\b{0}\b", System.Text.RegularExpressions.Regex.Escape(b))))
                    return true;
            }
            return false;
        }

        private static void RemoveBraced()
        {
            int idx2;
            if (_text == null)
                return;
            while ((idx2 = _text.IndexOf('}')) >= 0)
            {
                int idx1;
                if ((idx1 = _text.LastIndexOf('{', idx2)) < 0)
                {
                    _text = null;
                    return;
                }

                string buffer = _text.Substring(idx1, idx2 - idx1 + 1);
                _text = _text.Remove(idx1, idx2 - idx1 + 1);

                idx2 = idx1;
                while (idx2>=_text.Length||(idx2 >= 0 && _text[idx2] != ')' && _text[idx2] != ';' && _text[idx2] != '}' && _text[idx2] != '=' && _text[idx2] != '{'))
                    --idx2;
                if (idx2 >= 0 && _text[idx2] != '=' && (_text[idx2] == ')' || TestBlock(_text.Substring(idx2, idx1 - idx2 + 1))))
                {
                    while (idx2 >= 0 && _text[idx2] != ';' && _text[idx2] != '}' && _text[idx2] != '{')
                        idx2--;
                    // Console.WriteLine(":::"+text.Substring(idx2+1,idx1-idx2) +":::");

                    _text = _text.Remove(idx2 + 1, idx1 - idx2 - 1);
                }
                else
                    _text = _text.Insert(idx1, buffer.Replace("{", @"<strukt>").Replace("}", @"<\strukt>").Replace('\n', ' ').Replace('\t', ' ').Replace("  ", " "));
            }

        }

        private static void TrimText()
        {
            if (_text == null)
                return;
            _text = "int main(){" + _text.Replace('\n', ' ').Replace('\t', ' ').Replace("  ", " ").Replace("<strukt>", "{").Replace("<\\strukt>", "}") + "return 0; }";
        }

        public string GetGlobals()
        {
            RemoveStrings();
            GetPreprocessor();
            RemoveBraced();
            TrimText();
            RestoreStrings();
            //Console.WriteLine(text);
            return _text;
        }
    }
}
