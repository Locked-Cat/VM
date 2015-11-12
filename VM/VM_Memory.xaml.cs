using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace VM
{
    /// <summary>
    /// VM_Memory.xaml 的交互逻辑
    /// </summary>
    public partial class VM_Memory : UserControl
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

        public delegate void ScreenUpdateEventHandler(object sender, EventArgs args);
        public event ScreenUpdateEventHandler ScreenUpdateEvent;

        static public UInt16 MemorySize
        { get; } = 65535;

        static public UInt16 VideoMemorySize
        { get; } = (UInt16)(VM_Screen.CharsXAxis * VM_Screen.CharsYAxis * 2);

        static public UInt16 VideoMemoryStartAddr
        { get; } = 0xa000;

        private byte[] memory = new byte[MemorySize];

        public VM_Memory()
        {
            InitializeComponent();
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
                    ScreenUpdateEvent(this, EventArgs.Empty);
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
