using System.Collections.Generic;
using System.Linq;

namespace fcc
{
    class FreeRegs
    {
        private readonly List<int> _queue = new List<int>();
        public FreeRegs()
        {
            for(var i=0;i<Processor.NoOfRegisters;i++)
                _queue.Add(i);
        }
        public void Add(List<int> reg)
        {
            _queue.AddRange(reg);
            _queue.Sort();
        }
        public int Get()
        {
            var r=0;
            try
            {
                r=_queue.First();
                _queue.RemoveAt(0);
            }
            catch
            {
                Error.PrintError("No free regs?!",1,false);
            }
            return r;
        }
    }
}
