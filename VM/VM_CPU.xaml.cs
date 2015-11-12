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
    /// VM_CPU.xaml 的交互逻辑
    /// </summary>
    public partial class VM_CPU : UserControl
    {
        public delegate void RegisterStatusUpdateEventHandler(object sender, EventArgs args);
        public event RegisterStatusUpdateEventHandler RegisterStatusUpdateEvent;

        private byte al;
        private byte ah;
        private UInt16 a;

        public byte AL
        {
            get
            {
                return al;
            }

            set
            {
                al = value;
                var bytes = System.BitConverter.GetBytes(a);
                bytes[0] = value;
                a = System.BitConverter.ToUInt16(bytes, 0);
            }
        }

        public byte AH
        {
            get
            {
                return ah;
            }

            set
            {
                ah = value;
                var bytes = System.BitConverter.GetBytes(a);
                bytes[1] = value;
                a = System.BitConverter.ToUInt16(bytes, 0);
            }
        }

        public UInt16 A
        {
            get
            {
                return a;
            }

            set
            {
                a = value;
                var bytes = System.BitConverter.GetBytes(a);
                al = bytes[0];
                ah = bytes[1];
            }
        }

        public UInt16 B
        {
            get;
            set;
        }

        public UInt16 C
        {
            get;
            set;
        }

        public UInt16 D
        {
            get;
            set;
        }

        private VM_Memory memory;

        public VM_CPU()
        {
            InitializeComponent();
        }

        public void BindingMemory(VM_Memory memory)
        {
            this.memory = memory;
        }

        public void InitializeRegister()
        {
            AH = AL = 0;
            A = B = C = D = 0;
            RegisterStatusUpdateEvent(this, EventArgs.Empty);
        }

        public void ExecuteProgram(UInt16 programCounter, UInt16 programLength)
        {
            Debug.Assert(memory != null);
            while (programLength > 0)
            {
                var instruction = memory[programCounter];
                --programLength;
                ++programCounter;

                switch (instruction)
                {
                    case 0x01:      //LDT R VALUE
                        {
                            var registerID = (Register)memory[programCounter];
                            var bytes = new byte[] { memory[(UInt16)(programCounter + 1)], memory[(UInt16)(programCounter + 2)] };
                            var fromAddress = System.BitConverter.ToUInt16(bytes, 0);
                            programCounter += 3;
                            programLength -= 3;
                            MemoryToRegister(registerID, fromAddress);
                            RegisterStatusUpdateEvent(this, EventArgs.Empty);
                            break;
                        }
                    case 0x02:      //STT VALUE R
                        {
                            var bytes = new byte[] { memory[(UInt16)(programCounter)], memory[(UInt16)(programCounter + 1)] };
                            var toAddress = System.BitConverter.ToUInt16(bytes, 0);
                            var registerID = (Register)memory[(UInt16)(programCounter + 2)];
                            programCounter += 3;
                            programLength -= 3;
                            RegisterToMemory(registerID, toAddress);
                            break;
                        }
                    case 0x03:      //SET R VALUE
                        {
                            var registerID = (Register)memory[programCounter];
                            if (registerID == Register.AH || registerID == Register.AL)
                            {
                                if (registerID == Register.AH)
                                    AH = (memory[(UInt16)(programCounter + 1)]);
                                else
                                    AL = (memory[(UInt16)(programCounter + 1)]);
                            }
                            else
                            {
                                var bytes = new byte[] { memory[(UInt16)(programCounter + 1)], memory[(UInt16)(programCounter + 2)] };
                                var value = System.BitConverter.ToUInt16(bytes, 0);
                                switch (registerID)
                                {
                                    case Register.A:
                                        A = value;
                                        break;
                                    case Register.B:
                                        B = value;
                                        break;
                                    case Register.C:
                                        C = value;
                                        break;
                                    case Register.D:
                                        D = value;
                                        break;
                                }
                            }
                            programCounter += 3;
                            programLength -= 3;
                            RegisterStatusUpdateEvent(this, EventArgs.Empty);
                            break;
                        }
                    case 0x04:
                        programCounter += 2;
                        programLength -= 2;
                        break;
                }
            }
        }

        private void RegisterToMemory(Register registerID, UInt16 toAddress)
        {
            if (registerID == Register.AL || registerID == Register.AH)
            {
                if (registerID == Register.AL)
                    memory[toAddress] = AL;
                else
                    memory[toAddress] = AH;
            }
            else
            {
                var bytes = new byte[2];
                switch (registerID)
                {
                    // little endin
                    case Register.A:
                        bytes = System.BitConverter.GetBytes(A);
                        break;
                    case Register.B:
                        bytes = System.BitConverter.GetBytes(B);
                        break;
                    case Register.C:
                        bytes = System.BitConverter.GetBytes(C);
                        break;
                    case Register.D:
                        bytes = System.BitConverter.GetBytes(D);
                        break;
                }

                memory[toAddress] = bytes[0];
                memory[(UInt16)(toAddress + 1)] = bytes[1];
            }
        }

        private void MemoryToRegister(Register registerID, UInt16 fromAddress)
        {
            if (registerID == Register.AL || registerID == Register.AH)
            {
                if (registerID == Register.AL)
                    AL = (memory[fromAddress]);
                else
                    AH = (memory[fromAddress]);
            }
            else
            {
                var bytes = new byte[] { memory[fromAddress], memory[(UInt16)(fromAddress + 1)] };
                var value = System.BitConverter.ToUInt16(bytes, fromAddress);
                switch (registerID)
                {
                    case Register.A:
                        A = value;
                        break;
                    case Register.B:
                        B = value;
                        break;
                    case Register.C:
                        C = value;
                        break;
                    case Register.D:
                        D = value;
                        break;
                }
            }
        }
    }
}
