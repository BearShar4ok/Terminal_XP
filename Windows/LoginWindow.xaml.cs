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
using System.Windows.Threading;
using System.Xml.Linq;
using Terminal_XP.Classes;

namespace Terminal_XP.Windows
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    
    public partial class LoginWindow : Window
    {
        private const int TbWidth = 400;
        private const string Caret = "_";

        private readonly string _theme;
        private readonly Dictionary<string, string> _database;
        private readonly Mutex _mutex = new Mutex();

        private bool _haveCaretLogin;
        private bool _haveCaretPassword;
        private bool _updateLogin;
        private bool _updatePassword;
        
        public string Password { get;private set; }
        public State ReternedState { get; private set; } = State.None;

        public static RoutedCommand OpenHackPageCommand = new RoutedCommand();
        public LoginWindow(string theme, Dictionary<string, string> dct)
        {
            InitializeComponent();
            //Focus();
           
            _theme = theme;
            _database = dct;

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

            KeyDown += KeyPress;
            TBLogin.PreviewKeyDown += ChangeFocus;
            TBPassword.PreviewKeyDown += ChangeFocus;

            LoadTheme(theme);
            LoadParams();

            TBLogin.Focus();
            OpenHackPageCommand.InputGestures.Add(new KeyGesture(Key.R, ModifierKeys.Control));

            if (Addition.IsDebugMod)
            {
                TBLogin.Text = "login";
                TBPassword.Text = "password";
            }
            else
            {
                Topmost = true;
                Cursor = Cursors.None;
            }
        }
        private void LoadTheme(string theme)
{
            Background = new ImageBrush() { ImageSource = new BitmapImage(new Uri(Addition.Themes + theme + "/Background.png", UriKind.Relative)) };
            var fontFamily = new FontFamily(new Uri("pack://application:,,,/"), Addition.Themes + theme + "/#" + ConfigManager.Config.FontName);

            TBLogin.FontFamily = fontFamily;
            LblLogin.FontFamily = fontFamily;

            TBPassword.FontFamily = fontFamily;
            LblPassword.FontFamily = fontFamily;
        }

        private void LoadParams()
        {
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            ResizeMode = ResizeMode.NoResize;

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

            //var height = MeasureString("@", TBLogin.FontFamily, TBLogin.FontStyle, TBLogin.FontWeight, TBLogin.FontStretch, TBLogin.FontSize).Height;
            //height += 10;
            //
            //TBLogin.Height = height;
            //LblLogin.Height = height;
            //
            //TBPassword.Height = height;
            //LblPassword.Height = height;
            //
            //TBLogin.Width = TbWidth;
            //TBPassword.Width = TbWidth;

            if (Addition.IsDebugMod)
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
        private void KeyPress(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    ReternedState = State.Cancel;
                    Close();
                    break;
                case Key.Enter:
                    if (CheckLoginAndPassword())
                    {
                        ReternedState = State.Access;
                        Close();
                    }
                    else
                    {
                       var alert = new AlertWindow("Уведомление", "Данные введены некоректно. Попробуйте еще раз.", "Закрыть", _theme);
                       alert.Show();
                    }
                    break;
            }
        }
        private void ChangeFocus(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down)
            {
                if (TBLogin.IsFocused)
                    TBPassword.Focus();
                else
                    TBLogin.Focus();
            }
        }
        private bool GetUpdate(TextBox textBox) => textBox == TBLogin ? _updateLogin : _updatePassword;
        private bool GetHaveCaret(TextBox textBox) => textBox == TBLogin ? _haveCaretLogin : _haveCaretPassword;
        private void SetHaveCaret(TextBox textBox, bool val)
        {
            if (textBox == TBLogin)
                _haveCaretLogin = val;

            _haveCaretPassword = val;
        }
        private bool CheckLoginAndPassword()
        {
            var login = TBLogin.Text;
            var password = TBPassword.Text;

            if (login.EndsWith(Caret))
                login = login.Remove(login.Length - 1);

            if (password.EndsWith(Caret))
                password = password.Remove(password.Length - 1);

            return _database.ContainsKey(login) && _database[login] == password;
        }
        private bool CheckLogin()
        {
            var login = TBLogin.Text;

            if (login.EndsWith(Caret))
                login = login.Remove(login.Length - 1);

            return _database.ContainsKey(login);
        }
        private string GetPassword()
        {
            var login = TBLogin.Text;

            if (login.EndsWith(Caret))
                login = login.Remove(login.Length - 1);

            return _database[login];
        }
        private void OpenHackPage(object sender, ExecutedRoutedEventArgs e)
        {
            if (CheckLogin())
            {
                ReternedState = State.Huck;
                Password = GetPassword();
                Close();
            }
            else
            {
                var alert = new AlertWindow("Уведомление", "Логин не найден в базе. Попробуйте еще раз.", "Закрыть", _theme);
                alert.Show();
            }
        }
    }
}
