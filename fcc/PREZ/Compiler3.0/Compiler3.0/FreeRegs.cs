using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compiler1._0
{
    class FreeRegs
    {
        private List<int> queue = new List<int>();
        public FreeRegs()
        {
            for(int i=0;i<Processor.NoOfRegisters;i++)
                this.queue.Add(i);
        }
        public void Add(List<int> reg)
        {
            queue.AddRange(reg);
            queue.Sort();
        }
        public int Get()
        {
            int r=0;
            try
            {
                r=queue.First();
                queue.RemoveAt(0);
            }
            catch
            {
                Error.Stop("No free regs?!");
            }
            return r;
        }
    }
}
