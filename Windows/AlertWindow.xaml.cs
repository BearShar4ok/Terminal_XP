using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Terminal_XP.Classes;

namespace Terminal_XP.Windows
{
    /// <summary>
    /// Логика взаимодействия для AlertWindow.xaml
    /// </summary>
    public partial class AlertWindow : Window
    {
        public AlertWindow()
        {
            InitializeComponent();
        }
        public AlertWindow(string title, string text, string buttonText,string theme) : base()
        {
            titleTextBox.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), Addition.Themes + theme + "/#Fallout Regular");
            textTextBox.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), Addition.Themes + theme + "/#Fallout Regular");
        }
    }
}
