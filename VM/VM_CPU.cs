using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace VM
{
    public enum Register
    {
        UNKNOWN = 0,
        AL = 1,
        AH = 2,
        A = 4,
        B = 8,
        C = 16,
        D = 32
    }

    public enum CPUStatus
    {
        Running,
        Stop,
        Pause
    }

    public partial class VM_CPU: IHardware
    {
        public delegate void RegisterStatusUpdateEventHandler();
        public event RegisterStatusUpdateEventHandler RegisterStatusUpdateEvent;

        public delegate void CPUSpeedUpdateEventHandler();
        public event CPUSpeedUpdateEventHandler CPUSpeedUpdateEvent;

        public delegate void CPURunningChangedEventHandler();
        public event CPURunningChangedEventHandler CPURunningChangedEvent;

        public object mutexSpeed = new object();
        private int speed;
        public int Speed
        {
            get
            {
                return speed;
            }
            set
            {
                speed = value;
                if (CPUSpeedUpdateEvent != null)
                    CPUSpeedUpdateEvent();
            }
        }

        private CPUStatus running;
        public CPUStatus Running
        {
            get
            {
                return running;
            }
            set
            {
                running = value;
                if (CPURunningChangedEvent != null)
                    CPURunningChangedEvent();
            }
        }

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

        public VM_CPU(VM_Memory memory)
        {
            this.memory = memory;
        }

        public void Reset()
        {
            AH = AL = 0;
            A = B = C = D = 0;
            Flags = 0;
            InstructionPointer = 0;
            if (RegisterStatusUpdateEvent != null)
                RegisterStatusUpdateEvent();
        }

        public void ExecuteProgram(UInt16 programPosition, System.Threading.ManualResetEvent pauseEvent)
        {
            Debug.Assert(memory != null);

            InstructionPointer = programPosition;
            bool isProgramEnd = false;

            Running = CPUStatus.Running;
            while (!isProgramEnd)
            {
                var instruction = memory[InstructionPointer];
                ++InstructionPointer;
                lock(mutexSpeed)
                {
                    System.Threading.Thread.Sleep(Speed);
                }
                pauseEvent.WaitOne(System.Threading.Timeout.Infinite);

                switch (instruction)
                {
                    case 0x01:      //LDT R V
                        {
                            var registerID = (Register)memory[InstructionPointer];
                            var bytes = new byte[] { memory[(UInt16)(InstructionPointer + 1)], memory[(UInt16)(InstructionPointer + 2)] };
                            var fromAddress = System.BitConverter.ToUInt16(bytes, 0);
                            InstructionPointer += 3;
                            MemoryToRegister(registerID, fromAddress);
                            if (RegisterStatusUpdateEvent != null)
                                RegisterStatusUpdateEvent();
                            break;
                        }
                    case 0x28:      //LDR R R
                        {
                            var leftRegisterID = (Register)memory[InstructionPointer];
                            var rightRegisterID = (Register)memory[(UInt16)(InstructionPointer + 1)];
                            UInt16 fromAddress = ReadFromRegister(rightRegisterID);
                            InstructionPointer += 2;
                            MemoryToRegister(leftRegisterID, fromAddress);
                            if (RegisterStatusUpdateEvent != null)
                                RegisterStatusUpdateEvent();
                            break;
                        }
                    case 0x02:      //STT V R
                        {
                            var bytes = new byte[] { memory[(UInt16)(InstructionPointer)], memory[(UInt16)(InstructionPointer + 1)] };
                            var toAddress = System.BitConverter.ToUInt16(bytes, 0);
                            var registerID = (Register)memory[(UInt16)(InstructionPointer + 2)];
                            InstructionPointer += 3;
                            RegisterToMemory(registerID, toAddress);
                            break;
                        }
                    case 0x27:      //STT R R
                        {
                            var leftRegisterID = (Register)memory[InstructionPointer];
                            var rightRegisterID = (Register)memory[(UInt16)(InstructionPointer + 1)];
                            UInt16 toAddress = ReadFromRegister(leftRegisterID);
                            InstructionPointer += 2;
                            RegisterToMemory(rightRegisterID, toAddress);
                            break;
                        }
                    case 0x03:      //SET R V
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
                                SetToRegister(registerID, value);
                            }
                            InstructionPointer += 3;
                            if (RegisterStatusUpdateEvent != null)
                                RegisterStatusUpdateEvent();
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
                            if (RegisterStatusUpdateEvent != null)
                                RegisterStatusUpdateEvent();
                            break;
                        }
                    case 0x0d:      //CMP V R
                        {
                            var leftValue = System.BitConverter.ToUInt16(memory.Segment(InstructionPointer, (UInt16)(InstructionPointer + 2)), 0);
                            var registerID = (Register)memory[(UInt16)(InstructionPointer + 2)];

                            UInt16 rightValue = ReadFromRegister(registerID);

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
                            if (RegisterStatusUpdateEvent != null)
                                RegisterStatusUpdateEvent();
                            break;
                        }
                    case 0x0e:      //CMP R V
                        {
                            var registerID = (Register)memory[(UInt16)(InstructionPointer)];
                            var rightValue = System.BitConverter.ToUInt16(memory.Segment((UInt16)(InstructionPointer + 1), (UInt16)(InstructionPointer + 3)), 0); ;

                            UInt16 leftValue = ReadFromRegister(registerID);

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
                            if (RegisterStatusUpdateEvent != null)
                                RegisterStatusUpdateEvent();
                            break;
                        }
                    case 0x0f:      //CMP R R
                        {
                            var leftRegisterID = (Register)memory[InstructionPointer];
                            var rightRegisterID = (Register)memory[(UInt16)(InstructionPointer + 1)];

                            var leftValue = ReadFromRegister(leftRegisterID);
                            var rightValue = ReadFromRegister(rightRegisterID);

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
                            if (RegisterStatusUpdateEvent != null)
                                RegisterStatusUpdateEvent();
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

                            if (RegisterStatusUpdateEvent != null)
                                RegisterStatusUpdateEvent();
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

                            if (RegisterStatusUpdateEvent != null)
                                RegisterStatusUpdateEvent();
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

                            if (RegisterStatusUpdateEvent != null)
                                RegisterStatusUpdateEvent();
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

                            if (RegisterStatusUpdateEvent != null)
                                RegisterStatusUpdateEvent();
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

                            if (RegisterStatusUpdateEvent != null)
                                RegisterStatusUpdateEvent();
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

                            if (RegisterStatusUpdateEvent != null)
                                RegisterStatusUpdateEvent();
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

                            if (RegisterStatusUpdateEvent != null)
                                RegisterStatusUpdateEvent();
                            break;
                        }
                    case 0x11:      //ADD R V
                        {
                            var leftRegisterID = (Register)memory[InstructionPointer];

                            var leftOperand = ReadFromRegister(leftRegisterID);
                            var rightOperand = System.BitConverter.ToUInt16(memory.Segment((UInt16)(InstructionPointer + 1), (UInt16)(InstructionPointer + 3)), 0);
                            var result = leftOperand + rightOperand;

                            if (leftRegisterID == Register.AH || leftRegisterID == Register.AL)
                            {
                                if (result > 0xff)
                                    Flags = (byte)(Flags | 16);
                                SetToRegister(leftRegisterID, (byte)result);
                            }
                            else
                            {
                                if (result > 0xffff)
                                    Flags = (byte)(Flags | 16);
                                SetToRegister(leftRegisterID, (UInt16)result);
                            }

                            InstructionPointer += 3;
                            if (RegisterStatusUpdateEvent != null)
                                RegisterStatusUpdateEvent();
                            break;
                        }
                    default:
                        throw new NotImplementedException();
                }
            }
            Running = CPUStatus.Stop;
        }

        private UInt16 ReadFromRegister(Register registerID)
        {
            switch (registerID)
            {
                case Register.AH:
                    return AH;
                case Register.AL:
                    return AL;
                case Register.A:
                    return A;
                case Register.B:
                    return B;
                case Register.C:
                    return C;
                case Register.D:
                    return D;
                default:
                    return 0;
            }
        }

        private void SetToRegister(Register registerID, byte value)
        {
            Debug.Assert(registerID == Register.AH || registerID == Register.AL);

            switch (registerID)
            {
                case Register.AH:
                    AH = value;
                    break;
                case Register.AL:
                    AL = value;
                    break;
            }
        }

        private void SetToRegister(Register registerID, UInt16 value)
        {
            Debug.Assert(registerID != Register.AH && registerID != Register.AL);

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
