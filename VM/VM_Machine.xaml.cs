using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace VM
{
    /// <summary>
    /// VM_Machine.xaml 的交互逻辑
    /// </summary>
    /// 

    public partial class VM_Machine : Window
    {
        private System.Windows.Controls.MenuItem[] speedMenuItems;
        private System.Threading.Thread program;
        private System.Threading.ManualResetEvent pauseEvent;
        private UInt16 programPosition;
        private string programFileName;

        private VM_CPU cpu;
        private VM_Memory memory;
        private VM_BIOS bios;

        public VM_Machine()
        {
            InitializeComponent();

            memory = new VM_Memory();
            memory.ScreenUpdateEvent += () => 
            {
                screen.Dispatcher.Invoke(()=>
                {
                    screen.InvalidateVisual();
                });
            };

            screen.BindingMemory(memory);

            cpu = new VM_CPU(memory);
            cpu.RegisterStatusUpdateEvent += () =>
            {
                var registerStatus = string.Empty;

                registerStatus = "AL=#" + cpu.AL.ToString("X").PadLeft(2, '0');
                registerStatus += "; AH=#" + cpu.AH.ToString("X").PadLeft(2, '0');
                registerStatus += "; A=#" + cpu.A.ToString("X").PadLeft(4, '0');
                registerStatus += "; B=#" + cpu.B.ToString("X").PadLeft(4, '0');
                registerStatus += "; C=#" + cpu.C.ToString("X").PadLeft(4, '0');
                registerStatus += "; D=#" + cpu.D.ToString("X").PadLeft(4, '0');
                registerStatus += "; IP=#" + cpu.InstructionPointer.ToString("X").PadLeft(4, '0');
                registerStatus += "; F=#" + cpu.Flags.ToString("X").PadLeft(2, '0');

                registerStatusLabel.Dispatcher.Invoke(() => 
                {
                    registerStatusLabel.Content = registerStatus;
                });
            };
            cpu.CPUSpeedUpdateEvent += ChangeMachineStatusLabel;
            cpu.CPURunningChangedEvent += ChangeMachineStatusLabel;
            cpu.CPURunningChangedEvent += () =>
            {
                if (cpu.Running == CPUStatus.Running)
                {
                    programPauseButton.Dispatcher.Invoke(()=> { programPauseButton.IsEnabled = true; });
                    programResumeButton.Dispatcher.Invoke(() => { programResumeButton.IsEnabled = false; });
                    programRestartButton.Dispatcher.Invoke(() => { programRestartButton.IsEnabled = true; });
                }
                else
                {
                    if (cpu.Running == CPUStatus.Stop)
                    {
                        programPauseButton.Dispatcher.Invoke(() => { programPauseButton.IsEnabled = false; });
                        programResumeButton.Dispatcher.Invoke(() => { programResumeButton.IsEnabled = false; });
                        programRestartButton.Dispatcher.Invoke(() => { programRestartButton.IsEnabled = true; });
                    }
                    else
                    {
                        programPauseButton.Dispatcher.Invoke(() => { programPauseButton.IsEnabled = false; });
                        programResumeButton.Dispatcher.Invoke(() => { programResumeButton.IsEnabled = true; });
                        programRestartButton.Dispatcher.Invoke(() => { programRestartButton.IsEnabled = true; });
                    }
                }

            };

            bios = new VM_BIOS(cpu, memory, screen);
            bios.Reset();

            cpu.Running = CPUStatus.Stop;
            programRestartButton.IsEnabled = false;

            speedMenuItems = new System.Windows.Controls.MenuItem[] { speed1HzMenuItem, speed2HzMenuItem, speed4HzMenuItem, speedRealTimeItem };
            speedRealTimeItem.IsChecked = true;
        }

        private void ChangeMachineStatusLabel()
        {
            var machineStatus = string.Empty;

            machineStatus = "CPU-Speed=" + (cpu.Speed == 0 ? "Real Time" : ((double)1000 / cpu.Speed).ToString("0.00") + " Hz");
            machineStatus += "; CPU-State=" + Enum.GetName(typeof(CPUStatus), cpu.Running);

            machineStatusLabel.Dispatcher.Invoke(() =>
            {
                machineStatusLabel.Content = machineStatus;
            });
        }

        private void LoadProgram()
        {
            var fileStream = new System.IO.FileStream(programFileName, System.IO.FileMode.Open);
            var binaryReader = new System.IO.BinaryReader(fileStream);

            var magicWordBytes = binaryReader.ReadBytes(8);
            var magicWord = System.Text.Encoding.Default.GetString(magicWordBytes);

            if (magicWord != "ZHANGSHU")
            {
                System.Windows.Forms.MessageBox.Show("It is NOT a VM binary file!", "Error!", MessageBoxButtons.OK);
                return;
            }

            var offset = binaryReader.ReadUInt16();
            var length = binaryReader.ReadUInt16();

            var position = binaryReader.ReadUInt16();          //where the program starts in memory

            ushort i = 0;
            while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
            {
                memory[(UInt16)(position + i)] = binaryReader.ReadByte();
                ++i;
            }

            binaryReader.Close();
            fileStream.Close();

            programPosition = (UInt16)(position + offset - 14);
        }

        private void openMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();

            openFileDialog.DefaultExt = "VM";
            openFileDialog.Filter = "VM Binary File|*.vmbin";
            openFileDialog.FileName = string.Empty;

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return;

            programFileName = openFileDialog.FileName;

            lock (bios)
            {
                bios.Reset();
            }

            LoadProgram();
            pauseEvent = new System.Threading.ManualResetEvent(true);
            program = new System.Threading.Thread(() => 
            {
                cpu.ExecuteProgram(programPosition, pauseEvent);
            });
            program.Start();
        }


        private void exitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void speedMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            var current = sender as System.Windows.Controls.MenuItem;
            current.IsEnabled = false;
            foreach(var item in speedMenuItems)
            {
                if (item != current)
                {
                    item.IsChecked = false;
                    item.IsEnabled = true;
                }
            }

            lock(cpu.mutexSpeed)
            {
                switch (current.Tag as string)
                {
                    case "0.25":
                        cpu.Speed = 4000;
                        break;
                    case "0.5":
                        cpu.Speed = 2000;
                        break;
                    case "1":
                        cpu.Speed = 1000;
                        break;
                    case "2":
                        cpu.Speed = 500;
                        break;
                    case "4":
                        cpu.Speed = 250;
                        break;
                    case "5":
                        cpu.Speed = 0;
                        break;
                }
            }
        }

        private void programPauseButton_Click(object sender, RoutedEventArgs e)
        {
            cpu.Running = CPUStatus.Pause;
            pauseEvent.Reset();
        }

        private void programResumeButton_Click(object sender, RoutedEventArgs e)
        {
            cpu.Running = CPUStatus.Running;
            pauseEvent.Set();
        }

        private void programRestartButton_Click(object sender, RoutedEventArgs e)
        {
            if (program != null)
            {
                program.Abort();
                program = null;
            }

            lock(bios)
            {
                bios.Reset();
            }

            LoadProgram();

            cpu.Running = CPUStatus.Running;
            pauseEvent = new System.Threading.ManualResetEvent(true);
            program = new System.Threading.Thread(() =>
            {
                cpu.ExecuteProgram(programPosition, pauseEvent);
            });
            program.Start();
        }
    }
}
