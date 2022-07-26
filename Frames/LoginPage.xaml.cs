using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Terminal_XP.Classes;
using System.Windows.Controls.Primitives;

namespace Terminal_XP.Frames
{
    public partial class LoginPage : Page
    {
        private const int TBWidth = 400;
        private const bool IsDebugMod = true;

        private string _theme;
        private bool _update = false;
        
        public LoginPage(string theme)
        {
            InitializeComponent();

            _theme = theme;
            
            UpdateCarriage(TBLogin);
            UpdateCarriage(TBPassword);
            
            LoadTheme(theme);
            LoadParams();

            CreateAndSetGrid();

            TBLogin.Focus();
        }

        public void Relaod()
        {
            LoadTheme(_theme);
            LoadParams();
            
            TBLogin.Focus();
        }

        private void LoadTheme(string theme)
        {
            var fontFamily = new FontFamily(new Uri("pack://application:,,,/"), Addition.Themes + theme + "/#Fallout Regular");
            
            TBLogin.FontFamily = fontFamily;
            LblLogin.FontFamily = fontFamily;
            
            TBPassword.FontFamily = fontFamily;
            LblPassword.FontFamily = fontFamily;
        }

        private void LoadParams()
        {
            TBLogin.Text = "";
            TBPassword.Text = "";
            
            TBLogin.FontSize = ConfigManager.Config.FontSize;
            TBLogin.Opacity = ConfigManager.Config.Opacity;
            TBLogin.Foreground = (Brush)new BrushConverter().ConvertFromString(ConfigManager.Config.TerminalColor);
            
            LblLogin.FontSize = ConfigManager.Config.FontSize;
            LblLogin.Opacity = ConfigManager.Config.Opacity;
            LblLogin.Foreground = (Brush)new BrushConverter().ConvertFromString(ConfigManager.Config.TerminalColor);
            
            
            TBPassword.FontSize = ConfigManager.Config.FontSize;
            TBPassword.Opacity = ConfigManager.Config.Opacity;
            TBPassword.Foreground = (Brush)new BrushConverter().ConvertFromString(ConfigManager.Config.TerminalColor);
            
            LblPassword.FontSize = ConfigManager.Config.FontSize;
            LblPassword.Opacity = ConfigManager.Config.Opacity;
            LblPassword.Foreground = (Brush)new BrushConverter().ConvertFromString(ConfigManager.Config.TerminalColor);
            
            var height = MeasureString("@", TBLogin.FontFamily, TBLogin.FontStyle, TBLogin.FontWeight, TBLogin.FontStretch, TBLogin.FontSize).Height;
            height += 10;
            
            TBLogin.Height = height;
            LblLogin.Height = height;
            
            TBPassword.Height = height;
            LblPassword.Height = height;

            TBLogin.Width = TBWidth;
            TBPassword.Width = TBWidth;

            if (IsDebugMod)
            {
                TBLogin.BorderThickness = new Thickness(1);
                LblLogin.BorderThickness = new Thickness(1);
                TBPassword.BorderThickness = new Thickness(1);
                LblPassword.BorderThickness = new Thickness(1);
                
                TBLogin.BorderBrush = Brushes.Fuchsia;
                LblLogin.BorderBrush = Brushes.Fuchsia;
                TBPassword.BorderBrush = Brushes.Fuchsia;
                LblPassword.BorderBrush = Brushes.Fuchsia;
            }
        }

        private void CreateAndSetGrid()
        {
            Main.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(LblLogin.Height) });
            Main.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(10) });
            Main.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(LblPassword.Height) });
            
            Main.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(Math.Max(GetSizeLbl(LblLogin).Width, GetSizeLbl(LblPassword).Width) + 15) });
            Main.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(10) });
            Main.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(TBWidth) });
            
            Grid.SetRow(LblLogin, 0);
            Grid.SetRow(TBLogin, 0);
            Grid.SetRow(LblPassword, 2);
            Grid.SetRow(TBPassword, 2);
            
            Grid.SetColumn(LblLogin, 0);
            Grid.SetColumn(TBLogin, 2);
            Grid.SetColumn(LblPassword, 0);
            Grid.SetColumn(TBPassword, 2);
        }
        
        private void UpdateCarriage(TextBox textBox)
        {
            new Thread(() =>
            {
                while (_update)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Background,
                        new Action(() =>
                        {
                            if (textBox.Text.Length > 0 && textBox.Text[textBox.Text.Length - 1].ToString() == ConfigManager.Config.SpecialSymbol)
                                textBox.Text = textBox.Text.Remove(textBox.Text.Length - 1);
                            else
                                textBox.Text += ConfigManager.Config.SpecialSymbol;
                        }));

                    Thread.Sleep((int)ConfigManager.Config.DelayUpdateCarriage);
                }
            }).Start();
        }

        private static Size GetSizeLbl(Label lbl) => MeasureString(lbl.Content.ToString(), lbl.FontFamily, lbl.FontStyle,
            lbl.FontWeight, lbl.FontStretch, lbl.FontSize);
        
        private static Size MeasureString(string candidate, FontFamily font, FontStyle style, FontWeight weight, FontStretch stretch, double fontsize)
        {
            var formattedText = new FormattedText(
                candidate,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(font, style, weight, stretch),
                fontsize,
                Brushes.Black);

            return new Size(formattedText.Width, formattedText.Height);
        }
    }
}