﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Diagnostics;

namespace VM
{
    /// <summary>
    /// VM_Screen.xaml 的交互逻辑
    /// </summary>

    public partial class VM_Screen : UserControl
    {
        static public ushort CharsXAxis
        { get; } = 80;

        static public ushort CharsYAxis
        { get; } = 60;

        static public ushort ScreenWidth
        { get; } = (ushort)(CharsXAxis * 12);

        static public ushort ScreenHeight
        { get; } = (ushort)(CharsYAxis * 14);

        private VM_Memory memory;

        public VM_Screen()
        {
            InitializeComponent();
        }

        public void BindingMemory(VM_Memory memory)
        {
            this.memory = memory;
        }

        public void Reset()
        {
            Debug.Assert(memory != null);

            for (var i = VM_Memory.VideoMemoryStartAddr; i < VM_Memory.VideoMemoryStartAddr + VM_Memory.VideoMemorySize; i += 2)
            {
                memory[i] = 32;
                memory[(UInt16)(i + 1)] = 7;
            }
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (memory == null)
                return;

            var bitmap = new Bitmap(ScreenWidth, ScreenHeight);
            var bitmapGraphics = Graphics.FromImage(bitmap);
            var font = new Font("Consolas", 15f, System.Drawing.FontStyle.Bold);
            var xLoc = 0;
            var yLoc = 0;

            for (var i = VM_Memory.VideoMemoryStartAddr; i < VM_Memory.VideoMemoryStartAddr + VM_Memory.VideoMemorySize; i += 2)
            {
                SolidBrush backgroundBrush = null;
                SolidBrush foregroundBrush = null;

                if ((memory[(UInt16)(i + 1)] & 112) == 112)
                    backgroundBrush = new SolidBrush(System.Drawing.Color.Gray);
                if ((memory[(UInt16)(i + 1)] & 112) == 96)
                    backgroundBrush = new SolidBrush(System.Drawing.Color.Brown);
                if ((memory[(UInt16)(i + 1)] & 112) == 80)
                    backgroundBrush = new SolidBrush(System.Drawing.Color.Magenta);
                if ((memory[(UInt16)(i + 1)] & 112) == 64)
                    backgroundBrush = new SolidBrush(System.Drawing.Color.Red);
                if ((memory[(UInt16)(i + 1)] & 112) == 48)
                    backgroundBrush = new SolidBrush(System.Drawing.Color.Cyan);
                if ((memory[(UInt16)(i + 1)] & 112) == 32)
                    backgroundBrush = new SolidBrush(System.Drawing.Color.Green);
                if ((memory[(UInt16)(i + 1)] & 112) == 16)
                    backgroundBrush = new SolidBrush(System.Drawing.Color.Blue);
                if ((memory[(UInt16)(i + 1)] & 112) == 0)
                    backgroundBrush = new SolidBrush(System.Drawing.Color.Black);

                if ((memory[(UInt16)(i + 1)] & 7) == 0)
                {
                    if ((memory[(UInt16)(i + 1)] & 8) == 8)
                    {
                        foregroundBrush = new SolidBrush(System.Drawing.Color.Gray);
                    }
                    else
                    {
                        foregroundBrush = new SolidBrush(System.Drawing.Color.Black);
                    }
                }
                if ((memory[(UInt16)(i + 1)] & 7) == 1)
                {
                    if ((memory[(UInt16)(i + 1)] & 8) == 8)
                    {
                        foregroundBrush = new SolidBrush(System.Drawing.Color.LightBlue);
                    }
                    else
                    {
                        foregroundBrush = new SolidBrush(System.Drawing.Color.Blue);
                    }
                }
                if ((memory[(UInt16)(i + 1)] & 7) == 2)
                {
                    if ((memory[(UInt16)(i + 1)] & 8) == 8)
                    {
                        foregroundBrush = new SolidBrush(System.Drawing.Color.LightGreen);
                    }
                    else
                    {
                        foregroundBrush = new SolidBrush(System.Drawing.Color.Green);
                    }
                }
                if ((memory[(UInt16)(i + 1)] & 7) == 3)
                {
                    if ((memory[(UInt16)(i + 1)] & 8) == 8)
                    {
                        foregroundBrush = new SolidBrush(System.Drawing.Color.LightCyan);
                    }
                    else
                    {
                        foregroundBrush = new SolidBrush(System.Drawing.Color.Cyan);
                    }
                }
                if ((memory[(UInt16)(i + 1)] & 7) == 4)
                {
                    if ((memory[(UInt16)(i + 1)] & 8) == 8)
                    {
                        foregroundBrush = new SolidBrush(System.Drawing.Color.Pink);
                    }
                    else
                    {
                        foregroundBrush = new SolidBrush(System.Drawing.Color.Red);
                    }
                }
                if ((memory[(UInt16)(i + 1)] & 7) == 5)
                {
                    if ((memory[(UInt16)(i + 1)] & 8) == 8)
                    {
                        foregroundBrush = new SolidBrush(System.Drawing.Color.Fuchsia);
                    }
                    else
                    {
                        foregroundBrush = new SolidBrush(System.Drawing.Color.Magenta);
                    }
                }
                if ((memory[(UInt16)(i + 1)] & 7) == 6)
                {
                    if ((memory[(UInt16)(i + 1)] & 8) == 8)
                    {
                        foregroundBrush = new SolidBrush(System.Drawing.Color.Yellow);
                    }
                    else
                    {
                        foregroundBrush = new SolidBrush(System.Drawing.Color.Brown);
                    }
                }
                if ((memory[(UInt16)(i + 1)] & 7) == 7)
                {
                    if ((memory[(UInt16)(i + 1)] & 8) == 8)
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

                if ((xLoc % 960) == 0 && (xLoc != 0))
                {
                    yLoc += 14;
                    xLoc = 0;
                }

                byte[] letter = { memory[i] };
                var s = System.Text.Encoding.ASCII.GetString(letter);
                var pf = new PointF(xLoc - 2, yLoc - 4);

                bitmapGraphics.FillRectangle(backgroundBrush, xLoc, yLoc, 12f, 14f);
                bitmapGraphics.DrawString(s, font, foregroundBrush, pf);
                xLoc += 12;
            }

            var hBitmap = bitmap.GetHbitmap();
            var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            screenBitmap.Source = bitmapSource;
            bitmapGraphics.Dispose();
            bitmap.Dispose();
            DeleteObject(hBitmap);
        }
    }
}
