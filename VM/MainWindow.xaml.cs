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

namespace VM
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            screen.Poke(0xa000, 65);
            screen.Poke(0xa001, Convert.ToByte("00011111", 2));
            screen.Poke(0xa002, 66);
            screen.Poke(0xa003, Convert.ToByte("01001111", 2));
            screen.Poke(0xa004, 67);
            screen.Poke(0xa005, Convert.ToByte("00101111", 2));
        }
    }
}
