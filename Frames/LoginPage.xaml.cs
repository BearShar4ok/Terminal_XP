using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Terminal_XP.Classes;

namespace Terminal_XP.Frames
{
    public partial class LoginPage : Page
    {
        private const int TbWidth = 400;
        private const string Caret = "_";
        private const bool IsDebugMod = false;

        private readonly string _theme;
        private readonly Dictionary<string, string> _loging;

        private bool _haveCaretLogin;
        private bool _haveCaretPassword;
        private bool _updateLogin;
        private bool _updatePassword;
        private readonly Mutex _mutex = new Mutex();

        public LoginPage(string theme, Dictionary<string, string> dct)
        {
            InitializeComponent();

            _theme = theme;
            _loging = dct;

            TBLogin.GotFocus += (obj, e) =>
            {
                _updateLogin = true;
                UpdateCarriage(TBLogin);
            };

            TBLogin.LostFocus += (obj, e) =>
            {
                _updateLogin = false;
            };
            
            TBPassword.GotFocus += (obj, e) =>
            {
                _updatePassword = true;
                UpdateCarriage(TBPassword);
            };

            TBPassword.LostFocus += (obj, e) =>
            {
                _updatePassword = false;
            };
            
            LoadTheme(theme);
            LoadParams();

            CreateAndSetGrid();

            TBLogin.Focus();
        }

        public void Closing()
        {
            _updateLogin = false;
            _updatePassword = false;
        }

        public void Relaod()
        {
            LoadTheme(_theme);
            LoadParams();
            
            _updateLogin = false;
            _updatePassword = false;
            
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

            TBLogin.Width = TbWidth;
            TBPassword.Width = TbWidth;

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
            Main.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(TbWidth) });
            
            Grid.SetRow(LblLogin, 0);
            Grid.SetRow(TBLogin, 0);
            Grid.SetRow(LblPassword, 2);
            Grid.SetRow(TBPassword, 2);
            
            Grid.SetColumn(LblLogin, 0);
            Grid.SetColumn(TBLogin, 2);
            Grid.SetColumn(LblPassword, 0);
            Grid.SetColumn(TBPassword, 2);
        }

        private void KeyPress(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Closing();
                    NavigationService?.GoBack();
                    break;
                case Key.Enter:
                    // Check right login and password
                    CheckLogin();
                    break;
            }
        }

        private bool CheckLogin()
        {
            var login = TBLogin.Text;
            var password = TBPassword.Text;

            if (login.EndsWith(Caret))
                login = login.Remove(login.Length - 1);

            if (password.EndsWith(Caret))
                password = password.Remove(password.Length - 1);
            
            if (_loging.ContainsKey(login) && _loging[login] == password)
                return true;
            
            return false;
        }
        
        private void UpdateCarriage(TextBox textBox)
        {
            new Thread(() =>
            {
                _mutex.WaitOne();
                
                while (GetUpdate(textBox))
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Background,
                        new Action(() =>
                        {
                            if (textBox.Text.Length > 0 && textBox.Text.EndsWith(Caret) && GetHaveCaret(textBox))
                            {
                                textBox.Text = textBox.Text.Remove(textBox.Text.Length - 1);
                                textBox.CaretIndex = textBox.Text.Length;
                                SetHaveCaret(textBox, false);
                            }
                            else
                            {
                                textBox.Text += Caret;
                                textBox.CaretIndex = textBox.Text.Length - 1;
                                SetHaveCaret(textBox, true);
                            }
                        })
                    );

                    Thread.Sleep((int)ConfigManager.Config.DelayUpdateCarriage);
                } 
                
                Dispatcher.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                    {
                        if (textBox.Text.Length > 0 && textBox.Text.EndsWith(Caret) && GetHaveCaret(textBox))
                        {
                            textBox.Text = textBox.Text.Remove(textBox.Text.Length - 1);
                            textBox.CaretIndex = textBox.Text.Length;
                            SetHaveCaret(textBox, false);
                        }
                    })
                );
                
                _mutex.ReleaseMutex();
            }).Start();
        }

        private bool GetHaveCaret(TextBox textBox) => textBox == TBLogin ? _haveCaretLogin : _haveCaretPassword;

        private bool GetUpdate(TextBox textBox) => textBox == TBLogin ? _updateLogin : _updatePassword;

        private void SetHaveCaret(TextBox textBox, bool val)
        {
            if (textBox == TBLogin)
                _haveCaretLogin = val;

            _haveCaretPassword = val;
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