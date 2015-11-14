using System;
using System.Windows.Controls;
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

        public byte Flags
        {
            get;
            set;
        }

        public UInt16 InstructionPointer
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
            if (this.memory == null)
            {
                Debug.Assert(memory != null);
                this.memory = memory;
            }
        }

        public void InitializeRegister()
        {
            AH = AL = 0;
            A = B = C = D = 0;
            Flags = 0;
            InstructionPointer = 0;
            RegisterStatusUpdateEvent(this, EventArgs.Empty);
        }

        public void ExecuteProgram(UInt16 programPosition)
        {
            Debug.Assert(memory != null);

            InstructionPointer = programPosition;
            bool isProgramEnd = false;

            while (!isProgramEnd)
            {
                var instruction = memory[InstructionPointer];
                ++InstructionPointer;

                switch (instruction)
                {
                    case 0x01:      //LDT R VALUE
                        {
                            var registerID = (Register)memory[InstructionPointer];
                            var bytes = new byte[] { memory[(UInt16)(InstructionPointer + 1)], memory[(UInt16)(InstructionPointer + 2)] };
                            var fromAddress = System.BitConverter.ToUInt16(bytes, 0);
                            InstructionPointer += 3;
                            MemoryToRegister(registerID, fromAddress);
                            RegisterStatusUpdateEvent(this, EventArgs.Empty);
                            break;
                        }
                    case 0x02:      //STT VALUE R
                        {
                            var bytes = new byte[] { memory[(UInt16)(InstructionPointer)], memory[(UInt16)(InstructionPointer + 1)] };
                            var toAddress = System.BitConverter.ToUInt16(bytes, 0);
                            var registerID = (Register)memory[(UInt16)(InstructionPointer + 2)];
                            InstructionPointer += 3;
                            RegisterToMemory(registerID, toAddress);
                            break;
                        }
                    case 0x03:      //SET R VALUE
                        {
                            var registerID = (Register)memory[InstructionPointer];
                            if (registerID == Register.AH || registerID == Register.AL)
                            {
                                if (registerID == Register.AH)
                                    AH = (memory[(UInt16)(InstructionPointer + 1)]);
                                else
                                    AL = (memory[(UInt16)(InstructionPointer + 1)]);
                            }
                            else
                            {
                                var bytes = new byte[] { memory[(UInt16)(InstructionPointer + 1)], memory[(UInt16)(InstructionPointer + 2)] };
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
                            InstructionPointer += 3;
                            RegisterStatusUpdateEvent(this, EventArgs.Empty);
                            break;
                        }
                    case 0x04:      //END ADDR
                        InstructionPointer += 2;
                        isProgramEnd = true;
                        break;
                    case 0x0c:      //CMP V V
                        {
                            var leftValue = System.BitConverter.ToUInt16(memory.Segment(InstructionPointer, (UInt16)(InstructionPointer + 2)), 0);
                            var rightValue = System.BitConverter.ToUInt16(memory.Segment((UInt16)(InstructionPointer + 2), (UInt16)(InstructionPointer + 4)), 0);

                            Flags = 0;
                            if (leftValue == rightValue)
                                Flags = (byte)(Flags | 1);
                            if (leftValue != rightValue)
                                Flags = (byte)(Flags | 2);
                            if (leftValue > rightValue)
                                Flags = (byte)(Flags | 4);
                            if (leftValue < rightValue)
                                Flags = (byte)(Flags | 8);

                            InstructionPointer += 4;
                            RegisterStatusUpdateEvent(this, EventArgs.Empty);
                            break;
                        }
                    case 0x0d:      //CMP V R
                        {
                            var leftValue = System.BitConverter.ToUInt16(memory.Segment(InstructionPointer, (UInt16)(InstructionPointer + 2)), 0);
                            var registerID = (Register)memory[(UInt16)(InstructionPointer + 2)];

                            UInt16 rightValue = 0;
                            switch (registerID)
                            {
                                case Register.AH:
                                    rightValue = AH;
                                    break;
                                case Register.AL:
                                    rightValue = AL;
                                    break;
                                case Register.A:
                                    rightValue = A;
                                    break;
                                case Register.B:
                                    rightValue = B;
                                    break;
                                case Register.C:
                                    rightValue = C;
                                    break;
                                case Register.D:
                                    rightValue = D;
                                    break;
                            }

                            Flags = 0;
                            if (leftValue == rightValue)
                                Flags = (byte)(Flags | 1);
                            if (leftValue != rightValue)
                                Flags = (byte)(Flags | 2);
                            if (leftValue > rightValue)
                                Flags = (byte)(Flags | 4);
                            if (leftValue < rightValue)
                                Flags = (byte)(Flags | 8);

                            InstructionPointer += 3;
                            RegisterStatusUpdateEvent(this, EventArgs.Empty);
                            break;
                        }
                    case 0x0e:      //CMP R V
                        {
                            var registerID = (Register)memory[(UInt16)(InstructionPointer + 2)];
                            var rightValue = System.BitConverter.ToUInt16(memory.Segment(InstructionPointer, (UInt16)(InstructionPointer + 2)), 0); ;

                            UInt16 leftValue = 0;
                            switch (registerID)
                            {
                                case Register.AH:
                                    leftValue = AH;
                                    break;
                                case Register.AL:
                                    leftValue = AL;
                                    break;
                                case Register.A:
                                    leftValue = A;
                                    break;
                                case Register.B:
                                    leftValue = B;
                                    break;
                                case Register.C:
                                    leftValue = C;
                                    break;
                                case Register.D:
                                    leftValue = D;
                                    break;
                            }

                            Flags = 0;
                            if (leftValue == rightValue)
                                Flags = (byte)(Flags | 1);
                            if (leftValue != rightValue)
                                Flags = (byte)(Flags | 2);
                            if (leftValue > rightValue)
                                Flags = (byte)(Flags | 4);
                            if (leftValue < rightValue)
                                Flags = (byte)(Flags | 8);

                            InstructionPointer += 3;
                            RegisterStatusUpdateEvent(this, EventArgs.Empty);
                            break;
                        }
                    case 0x0f:      //CMP R R
                        {
                            var leftRegisterID = (Register)memory[InstructionPointer];
                            var rightRegisterID = (Register)memory[(UInt16)(InstructionPointer + 1)];

                            UInt16 leftValue = 0;
                            UInt16 rightValue = 0;

                            switch (leftRegisterID)
                            {
                                case Register.AH:
                                    leftValue = AH;
                                    break;
                                case Register.AL:
                                    leftValue = AL;
                                    break;
                                case Register.A:
                                    leftValue = A;
                                    break;
                                case Register.B:
                                    leftValue = B;
                                    break;
                                case Register.C:
                                    leftValue = C;
                                    break;
                                case Register.D:
                                    leftValue = D;
                                    break;
                            }

                            switch (rightRegisterID)
                            {
                                case Register.AH:
                                    rightValue = AH;
                                    break;
                                case Register.AL:
                                    rightValue = AL;
                                    break;
                                case Register.A:
                                    rightValue = A;
                                    break;
                                case Register.B:
                                    rightValue = B;
                                    break;
                                case Register.C:
                                    rightValue = C;
                                    break;
                                case Register.D:
                                    rightValue = D;
                                    break;
                            }

                            Flags = 0;
                            if (leftValue == rightValue)
                                Flags = (byte)(Flags | 1);
                            if (leftValue != rightValue)
                                Flags = (byte)(Flags | 2);
                            if (leftValue > rightValue)
                                Flags = (byte)(Flags | 4);
                            if (leftValue < rightValue)
                                Flags = (byte)(Flags | 8);

                            InstructionPointer += 4;
                            RegisterStatusUpdateEvent(this, EventArgs.Empty);
                            break;
                        }
                    case 0x05:      //JMP ADDR
                        {
                            var addr = System.BitConverter.ToInt16(memory.Segment(InstructionPointer, (UInt16)(InstructionPointer + 2)), 0);

                            UInt16 absAddr = (UInt16)System.Math.Abs(addr);
                            if (addr >= 0)
                                InstructionPointer += absAddr;
                            else
                                InstructionPointer -= absAddr;

                            RegisterStatusUpdateEvent(this, EventArgs.Empty);
                            break;
                        }
                    case 0x06:      //JLE ADDR
                        {
                            var addr = System.BitConverter.ToUInt16(memory.Segment(InstructionPointer, (UInt16)(InstructionPointer + 2)), 0);
                            UInt16 absAddr = (UInt16)System.Math.Abs(addr);

                            if ((Flags & 8) == 8 || (Flags & 1) == 1)
                            {
                                if (addr >= 0)
                                    InstructionPointer += absAddr;
                                else
                                    InstructionPointer -= absAddr;
                            }
                            else
                                InstructionPointer += 2;

                            RegisterStatusUpdateEvent(this, EventArgs.Empty);
                            break;
                        }
                    case 0x07:      //JL ADDR
                        {
                            var addr = System.BitConverter.ToUInt16(memory.Segment(InstructionPointer, (UInt16)(InstructionPointer + 2)), 0);
                            UInt16 absAddr = (UInt16)System.Math.Abs(addr);

                            if ((Flags & 8) == 8)
                            {
                                if (addr >= 0)
                                    InstructionPointer += absAddr;
                                else
                                    InstructionPointer -= absAddr;
                            }
                            else
                                InstructionPointer += 2;

                            RegisterStatusUpdateEvent(this, EventArgs.Empty);
                            break;
                        }
                    case 0x08:      //JGE ADDR
                        {
                            var addr = System.BitConverter.ToUInt16(memory.Segment(InstructionPointer, (UInt16)(InstructionPointer + 2)), 0);
                            UInt16 absAddr = (UInt16)System.Math.Abs(addr);

                            if ((Flags & 4) == 4 || (Flags & 1) == 1)
                            {
                                if (addr >= 0)
                                    InstructionPointer += absAddr;
                                else
                                    InstructionPointer -= absAddr;
                            }
                            else
                                InstructionPointer += 2;

                            RegisterStatusUpdateEvent(this, EventArgs.Empty);
                            break;
                        }
                    case 0x09:      //JG ADDR
                        {
                            var addr = System.BitConverter.ToUInt16(memory.Segment(InstructionPointer, (UInt16)(InstructionPointer + 2)), 0);
                            UInt16 absAddr = (UInt16)System.Math.Abs(addr);

                            if ((Flags & 4) == 4)
                            {
                                if (addr >= 0)
                                    InstructionPointer += absAddr;
                                else
                                    InstructionPointer -= absAddr;
                            }
                            else
                                InstructionPointer += 2;

                            RegisterStatusUpdateEvent(this, EventArgs.Empty);
                            break;
                        }
                    case 0x0a:      //JE ADDR
                        {
                            var addr = System.BitConverter.ToUInt16(memory.Segment(InstructionPointer, (UInt16)(InstructionPointer + 2)), 0);
                            UInt16 absAddr = (UInt16)System.Math.Abs(addr);

                            if ((Flags & 1) == 1)
                            {
                                if (addr >= 0)
                                    InstructionPointer += absAddr;
                                else
                                    InstructionPointer -= absAddr;
                            }
                            else
                                InstructionPointer += 2;

                            RegisterStatusUpdateEvent(this, EventArgs.Empty);
                            break;
                        }
                    case 0x0b:      //JNE ADDR
                        {
                            var addr = System.BitConverter.ToUInt16(memory.Segment(InstructionPointer, (UInt16)(InstructionPointer + 2)), 0);
                            UInt16 absAddr = (UInt16)System.Math.Abs(addr);

                            if ((Flags & 2) == 2)
                            {
                                if (addr >= 0)
                                    InstructionPointer += absAddr;
                                else
                                    InstructionPointer -= absAddr;
                            }
                            else
                                InstructionPointer += 2;

                            RegisterStatusUpdateEvent(this, EventArgs.Empty);
                            break;
                        }
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
