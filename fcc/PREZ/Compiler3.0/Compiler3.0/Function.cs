using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExtensionMethods;
using System.IO;

namespace Compiler1._0
{
    class Function
    {
        public string Name = null;
        public List<Variable> Parameters =new List<Variable>();
        public int NoOfParameters;
        public void ReadData(StreamReader read, string prototype)
        {
            NoOfParameters = 0;
            read.ReadLine();
            Name = prototype.Remove(prototype.IndexOf('(')-1);
            string name;
            string type;
            int idx;
            string s;
            string allparameters = prototype.Remove(0,prototype.IndexOf('(')+1);
            string[] parameters = allparameters.Split(',');
            if (parameters.Count<string>() == 1)
            {
                if (parameters[0].Length == 1)
                    return;
                else
                {
                    s = parameters[0].TrimEnd(')');
                    Variable v = new Variable();
                    idx = s.LastIndexOf(' ');
                    name = s.Remove(0, idx + 1);
                    name = name.TrimEnd(';');
                    type = s.Remove(idx);
                    type = type.TrimStart(' ');
                    v.SetVariable(name, type);
                    Parameters.Add(v);
                    NoOfParameters = 1;
                    return;
                }
            }
            parameters[parameters.Count<string>() - 1] = parameters[parameters.Count<string>() - 1].TrimEnd(')');
            foreach(string c in parameters)
            {
                Variable v = new Variable();
                idx = c.LastIndexOf(' ');
                name = c.Remove(0, idx + 1);
                name = name.TrimEnd(';');
                type = c.Remove(idx);
                type = type.TrimStart(' ');
                v.SetVariable(name, type);
                Parameters.Add(v);
                NoOfParameters++;
            }
        }
        
    }
}
