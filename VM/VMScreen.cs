using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VM
{
    public partial class VMScreen : UserControl
    {
        private ushort screenMemoryLocation;
        private byte[] screenMemory;

        public ushort ScreenMemoryLocation
        {
            get
            {
                return screenMemoryLocation;
            }
            set
            {
                ScreenMemoryLocation = value;
            }
        }

        public VMScreen()
        {
            InitializeComponent();
            screenMemoryLocation = 0xa000;
            screenMemory = new byte[4000];

            for (var i = 0; i < 4000; i += 2)
            {
                screenMemory[i] = 32;
                screenMemory[i + 1] = 7;
            }
        }

        public void Poke(ushort address, byte value)
        {
            ushort memoryLocation;

            try
            {
                memoryLocation = (ushort)(address - screenMemoryLocation);
            }
            catch (Exception)
            {
                return;
            }

            if (memoryLocation < 0 || memoryLocation > 3999)
                return;

            screenMemory[memoryLocation] = value;
            Refresh();
        }

        public byte Peek(ushort address)
        {
            ushort memoryLoacation;

            try
            {
                memoryLoacation = (ushort)(address - screenMemoryLocation);
            }
            catch (Exception)
            {
                return (byte)0;
            }

            if (memoryLoacation < 0 || memoryLoacation > 3999)
                return (byte)0;

            return screenMemory[memoryLoacation];
        }
    }
}
