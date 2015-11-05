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
using System.Drawing;

namespace VM
{
    /// <summary>
    /// VMScreen.xaml 的交互逻辑
    /// </summary>

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
            InvalidateVisual();
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

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            var bitmap = new Bitmap((int)this.Width, (int)this.Height);
            var bitmapGraphics = Graphics.FromImage(bitmap);
            var font = new Font("Courier New", 10f, System.Drawing.FontStyle.Regular);
            var xLoc = 0;
            var yLoc = 0;

            for (var i = 0; i < 4000; i += 2)
            {
                SolidBrush backgroundBrush = null;
                SolidBrush foregroundBrush = null;

                if ((screenMemory[i + 1] & 112) == 112)
                    backgroundBrush = new SolidBrush(System.Drawing.Color.Gray);
                if ((screenMemory[i + 1] & 112) == 96)
                    backgroundBrush = new SolidBrush(System.Drawing.Color.Brown);
                if ((screenMemory[i + 1] & 112) == 80)
                    backgroundBrush = new SolidBrush(System.Drawing.Color.Magenta);
                if ((screenMemory[i + 1] & 112) == 64)
                    backgroundBrush = new SolidBrush(System.Drawing.Color.Red);
                if ((screenMemory[i + 1] & 112) == 48)
                    backgroundBrush = new SolidBrush(System.Drawing.Color.Cyan);
                if ((screenMemory[i + 1] & 112) == 32)
                    backgroundBrush = new SolidBrush(System.Drawing.Color.Green);
                if ((screenMemory[i + 1] & 112) == 16)
                    backgroundBrush = new SolidBrush(System.Drawing.Color.Blue);
                if ((screenMemory[i + 1] & 112) == 0)
                    backgroundBrush = new SolidBrush(System.Drawing.Color.Black);

                if ((screenMemory[i + 1] & 7) == 0)
                {
                    if ((screenMemory[i + 1] & 8) == 8)
                    {
                        foregroundBrush = new SolidBrush(System.Drawing.Color.Gray);
                    }
                    else
                    {
                        foregroundBrush = new SolidBrush(System.Drawing.Color.Black);
                    }
                }
                if ((screenMemory[i + 1] & 7) == 1)
                {
                    if ((screenMemory[i + 1] & 8) == 8)
                    {
                        foregroundBrush = new SolidBrush(System.Drawing.Color.LightBlue);
                    }
                    else
                    {
                        foregroundBrush = new SolidBrush(System.Drawing.Color.Blue);
                    }
                }
                if ((screenMemory[i + 1] & 7) == 2)
                {
                    if ((screenMemory[i + 1] & 8) == 8)
                    {
                        foregroundBrush = new SolidBrush(System.Drawing.Color.LightGreen);
                    }
                    else
                    {
                        foregroundBrush = new SolidBrush(System.Drawing.Color.Green);
                    }
                }
                if ((screenMemory[i + 1] & 7) == 3)
                {
                    if ((screenMemory[i + 1] & 8) == 8)
                    {
                        foregroundBrush = new SolidBrush(System.Drawing.Color.LightCyan);
                    }
                    else
                    {
                        foregroundBrush = new SolidBrush(System.Drawing.Color.Cyan);
                    }
                }
                if ((screenMemory[i + 1] & 7) == 4)
                {
                    if ((screenMemory[i + 1] & 8) == 8)
                    {
                        foregroundBrush = new SolidBrush(System.Drawing.Color.Pink);
                    }
                    else
                    {
                        foregroundBrush = new SolidBrush(System.Drawing.Color.Red);
                    }
                }
                if ((screenMemory[i + 1] & 7) == 5)
                {
                    if ((screenMemory[i + 1] & 8) == 8)
                    {
                        foregroundBrush = new SolidBrush(System.Drawing.Color.Fuchsia);
                    }
                    else
                    {
                        foregroundBrush = new SolidBrush(System.Drawing.Color.Magenta);
                    }
                }
                if ((screenMemory[i + 1] & 7) == 6)
                {
                    if ((screenMemory[i + 1] & 8) == 8)
                    {
                        foregroundBrush = new SolidBrush(System.Drawing.Color.Yellow);
                    }
                    else
                    {
                        foregroundBrush = new SolidBrush(System.Drawing.Color.Brown);
                    }
                }
                if ((screenMemory[i + 1] & 7) == 7)
                {
                    if ((screenMemory[i + 1] & 8) == 8)
                    {
                        foregroundBrush = new SolidBrush(System.Drawing.Color.White);
                    }
                    else
                    {
                        foregroundBrush = new SolidBrush(System.Drawing.Color.Gray);
                    }
                }
                if (backgroundBrush == null)
                    backgroundBrush = new SolidBrush(System.Drawing.Color.Black);
                if (foregroundBrush == null)
                    foregroundBrush = new SolidBrush(System.Drawing.Color.Gray);

                if ((xLoc % 640) == 0 && (xLoc != 0))
                {
                    yLoc += 1;
                    xLoc = 0;
                }

                var s = System.Text.Encoding.ASCII.GetString(screenMemory, i, 1);
                var pf = new PointF(xLoc, yLoc);

                bitmapGraphics.FillRectangle(backgroundBrush, xLoc + 2, yLoc + 2, 8f, 11f);
                bitmapGraphics.DrawString(s, font, foregroundBrush, pf);
                xLoc += 8;
            }
            drawingContext.DrawBrush
            bitmapGraphics.Dispose();
            bitmap.Dispose();
        }
    }
}
