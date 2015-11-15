using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace VM
{
    public class VM_Memory: IHardware
    {
        public class MemoryAddressIllegalException : Exception
        {
            public MemoryAddressIllegalException()
            {
            }

            public MemoryAddressIllegalException(string message) : base(message)
            {
            }

            public MemoryAddressIllegalException(string message, Exception inner) : base(message, inner)
            {
            }
        }

        public delegate void ScreenUpdateEventHandler();
        public event ScreenUpdateEventHandler ScreenUpdateEvent;

        static public UInt16 MemorySize
        { get; } = 65535;

        static public UInt16 VideoMemorySize
        { get; } = (UInt16)(VM_Screen.CharsXAxis * VM_Screen.CharsYAxis * 2);

        static public UInt16 VideoMemoryStartAddr
        { get; } = 0xa000;

        private byte[] memory; 

        public VM_Memory()
        {
            memory = new byte[MemorySize];
        }

        public void Reset()
        {
            for (UInt16 i = 0; i < MemorySize; ++i)
                memory[i] = 0;
        }

        public byte this[UInt16 addr]
        {
            get
            {
                if (addr >= MemorySize)
                    throw new MemoryAddressIllegalException("Memory address cannot exceed " + MemorySize + "!");

                return memory[addr];
            }

            set
            {
                if (addr >= MemorySize)
                    throw new MemoryAddressIllegalException("Memory address cannot exceed " + MemorySize + "!");

                memory[addr] = value;
                if (addr >= VideoMemoryStartAddr && addr < VideoMemoryStartAddr + VideoMemorySize)
                    if (ScreenUpdateEvent != null)
                        ScreenUpdateEvent();
            }
        }

        public byte[] Segment(UInt16 firstAddr, UInt16 lastAddr)
        {
            if (firstAddr >= MemorySize || lastAddr >= MemorySize)
                throw new MemoryAddressIllegalException("Memory address cannot exceed " + MemorySize + "!");

            Debug.Assert(firstAddr <= lastAddr);
            var ret = new byte[lastAddr - firstAddr];

            for (var i = firstAddr; i < lastAddr; ++i)
                ret[i - firstAddr] = memory[i];

            return ret;
        }
    }
}
