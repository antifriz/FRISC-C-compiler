using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace fcc
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
            var allparameters = prototype.Remove(0,prototype.IndexOf('(')+1);
            var parameters = allparameters.Split(',');
            if (parameters.Count() == 1)
            {
                if (parameters[0].Length == 1)
                    return;
                var s = parameters[0].TrimEnd(')');
                var v = new Variable();
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
            parameters[parameters.Count() - 1] = parameters[parameters.Count() - 1].TrimEnd(')');
            foreach(var c in parameters)
            {
                var v = new Variable();
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
