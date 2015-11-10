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
using System.Windows.Forms;

namespace VM
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    /// 
    enum Register
    {
        UNKNOWN = 0,
        AL = 1,
        AH = 2,
        A = 4,
        B = 8,
        C = 16,
        D = 32
    }

    public partial class MainWindow : Window
    {
        private byte[] memory = new byte[65535];
        private UInt16 startAddr = 0;
        private UInt16 execAddr = 0;
        private UInt16 fileLength = 0;
        private UInt16 programCounter = 0;
        private byte registerAL = 0;
        private byte registerAH = 0;
        private UInt16 registerA = 0;
        private UInt16 registerB = 0;
        private UInt16 registerC = 0;
        private UInt16 registerD = 0;

        public MainWindow()
        {
            InitializeComponent();
            UpdateRegisterStatus();
        }

        private void UpdateRegisterStatus()
        {
            var registerStatus = string.Empty;

            registerStatus = "RegisterAL=#" + registerAL.ToString("X").PadLeft(2, '0');
            registerStatus += "; RegisterAH=#" + registerAH.ToString("X").PadLeft(2, '0');
            registerStatus += "; RegisterA=#" + registerA.ToString("X").PadLeft(4, '0');
            registerStatus += "; RegisterB=#" + registerB.ToString("X").PadLeft(4, '0');
            registerStatus += "; RegisterC=#" + registerC.ToString("X").PadLeft(4, '0');
            registerStatus += "; RegisterD=#" + registerD.ToString("X").PadLeft(4, '0');
            registerStatusLabel.Content = registerStatus;
        }

        private void openMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();

            openFileDialog.DefaultExt = "VM";
            openFileDialog.Filter = "VM Binary File|*.vmbin";
            openFileDialog.FileName = string.Empty;

            openFileDialog.ShowDialog();
            if (openFileDialog.FileName == string.Empty)
                return; 

            var fileStream = new System.IO.FileStream(openFileDialog.FileName, System.IO.FileMode.Open);
            var binaryReader = new System.IO.BinaryReader(fileStream);

            var magicWordBytes = binaryReader.ReadBytes(8);
            var magicWord = System.Text.Encoding.Default.GetString(magicWordBytes);

            if (magicWord != "ZHANGSHU")
            {
                System.Windows.Forms.MessageBox.Show("It is NOT a VM binary file!", "Error!", MessageBoxButtons.OK);
                return;
            }

            execAddr = binaryReader.ReadUInt16();
            fileLength = binaryReader.ReadUInt16();
            startAddr = binaryReader.ReadUInt16();

            ushort i = 0;
            while (binaryReader.PeekChar() != -1)
            {
                memory[startAddr + i] = binaryReader.ReadByte();
                ++i;
            }

            binaryReader.Close();
            fileStream.Close();

            programCounter =(UInt16)(execAddr - 14);
            ExecuteProgram(fileLength - execAddr);
        }

        private void SetAL(byte value)
        {
            registerAL = value;
            var bytes = System.BitConverter.GetBytes(registerA);
            bytes[0] = value;
            registerA = System.BitConverter.ToUInt16(bytes, 0);
        }

        private void SetAH(byte value)
        {
            registerAH = value;
            var bytes = System.BitConverter.GetBytes(registerA);
            bytes[1] = value;
            registerA = System.BitConverter.ToUInt16(bytes, 0);
        }

        private void SetA(UInt16 value)
        {
            registerA = value;
            var bytes = System.BitConverter.GetBytes(registerA);
            registerAL = bytes[0];
            registerAH = bytes[1];
        }

        private void ExecuteProgram(Int32 programLength)
        {
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
                            var fromAddress = System.BitConverter.ToUInt16(memory, programCounter + 1);
                            programCounter += 3;
                            programLength -= 3;
                            MemoryToRegister(registerID, fromAddress);
                            UpdateRegisterStatus();
                            break;
                        }
                    case 0x02:      //STT VALUE R
                        {
                            var toAddress = System.BitConverter.ToUInt16(memory, programCounter);
                            var registerID = (Register)memory[programCounter + 2];
                            programCounter += 3;
                            programLength -= 3;
                            RegisterToMemory(registerID, toAddress);
                            UpdateRegisterStatus();
                            break;
                        }
                    case 0x03:
                        {
                            var registerID = (Register)memory[programCounter];
                            if (registerID == Register.AH || registerID == Register.AL)
                            {
                                if (registerID == Register.AH)
                                    SetAH(memory[programCounter]);
                                else
                                    SetAL(memory[programCounter]);
                                programCounter += 3;
                                programLength += 3;
                                return;
                            }

                            var value = System.BitConverter.ToUInt16(memory, programCounter + 1);
                            switch (registerID)
                            {
                                case Register.A:
                                    SetA(value);
                                    break;
                                case Register.B:
                                    registerB = value;
                                    break;
                                case Register.C:
                                    registerC = value;
                                    break;
                                case Register.D:
                                    registerD = value;
                                    break;
                            }
                            programCounter += 3;
                            programLength -= 3;
                            UpdateRegisterStatus();
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
                {
                    memory[toAddress] = registerAL;
                }
                else
                {
                    memory[toAddress] = registerAH;
                }
            }
            else
            {
                var bytes = new byte[2];
                switch (registerID)
                {
                    // Little Endin
                    case Register.A:
                        bytes = System.BitConverter.GetBytes(registerA);
                        break;
                    case Register.B:
                        bytes = System.BitConverter.GetBytes(registerB);
                        break;
                    case Register.C:
                        bytes = System.BitConverter.GetBytes(registerC);
                        break;
                    case Register.D:
                        bytes = System.BitConverter.GetBytes(registerD);
                        break;
                }

                memory[toAddress] = bytes[0];
                memory[toAddress + 1] = bytes[1];
            }
        }

        private void MemoryToRegister(Register registerID, UInt16 fromAddress)
        {
            if (registerID == Register.AL || registerID == Register.AH)
            {
                if (registerID == Register.AL)
                    SetAL(memory[fromAddress]);
                else
                    SetAH(memory[fromAddress]);
            }
            else
            {
                var value = System.BitConverter.ToUInt16(memory, fromAddress);
                switch (registerID)
                {
                    case Register.A:
                        SetA(value);
                        break;
                    case Register.B:
                        registerB = value;
                        break;
                    case Register.C:
                        registerC = value;
                        break;
                    case Register.D:
                        registerD = value;
                        break;
                }
            }
        }

        private void exitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
