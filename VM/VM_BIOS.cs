using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VM
{
    class VM_BIOS: IHardware
    {
        private VM_CPU cpu;
        private VM_Memory memory;
        private VM_Screen screen;

        public VM_BIOS(VM_CPU cpu, VM_Memory memory, VM_Screen screen)
        {
            this.cpu = cpu;
            this.memory = memory;
            this.screen = screen;
        }

        public void Reset()
        {
            cpu.Reset();
            memory.Reset();
            screen.Reset();
        }
    }
}
