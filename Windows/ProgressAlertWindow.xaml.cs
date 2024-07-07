using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Terminal_XP.Windows
{
    /// <summary>
    /// Логика взаимодействия для ProgressAlertWindow.xaml
    /// </summary>
    public partial class ProgressAlertWindow : Window
    {
        public ProgressAlertWindow()
        {
            InitializeComponent();
            for (int i = 0; i < 100; i++) {
                
                bar.Value = i;
            }
        }
    }
}
