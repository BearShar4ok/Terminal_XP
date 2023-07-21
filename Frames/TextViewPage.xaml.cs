using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using Terminal_XP.Classes;

namespace Terminal_XP.Frames
{
    public partial class TextViewPage : Page
    {
        protected string _filename;
        protected string _theme;
        protected bool _update;
        protected Mutex _mutex = new Mutex();
        protected bool _isItCommand;


        public TextViewPage(string filename, string theme, bool clearPage = false, bool isItCommand = false)
        {
            InitializeComponent();

            if (clearPage)
                Addition.NavigationService.Navigated += RemoveLast;

            LoadTheme(theme);
            LoadParams();

            _filename = filename;
            _theme = theme;
            _isItCommand = isItCommand;
            Output.Text = ConfigManager.Config.SpecialSymbol;

            Application.Current.MainWindow.KeyDown += AdditionalKeys;
            Scroller.Focus();
            LoadText();
        }

        protected void RemoveLast(object obj, NavigationEventArgs e)
        {
            Addition.NavigationService?.RemoveBackEntry();
        }

        public void Closing()
        {
            _update = false;
            Addition.NavigationService.Navigated -= RemoveLast;
            Application.Current.MainWindow.KeyDown -= AdditionalKeys;
        }

        public void Reload()
        {
            ConfigManager.Load();
            LoadParams();
            LoadTheme(_theme);

            _update = false;

            _mutex?.WaitOne();
            Output.Text = ConfigManager.Config.SpecialSymbol;
            _mutex?.ReleaseMutex();

            LoadText();
        }

        protected void LoadText()
        {
            if (!File.Exists(_filename))
                return;

            _update = true;

            new Thread(() =>
            {
                using (var stream = File.OpenText(_filename))
                {
                    var text = stream.ReadToEnd();
                    if (_isItCommand)
                    {
                        var t = text.Split('\n').ToList();
                        RequestSender.SendGet(t[0].Replace("\r",""));
                        t.RemoveAt(0);
                        text = string.Join("\n", t.ToArray());
                    }

                    Addition.PrintLines(Output, Scroller, Dispatcher, ref _update, _mutex,
                        new FragmentText(text,
                            ConfigManager.Config.UsingDelayFastOutput ? ConfigManager.Config.DelayFastOutput : 0));
                    UpdateCarriage();
                }
            }).Start();
        }

        protected void LoadTheme(string theme)
        {
            Output.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), Addition.Themes + theme + "/#" + ConfigManager.Config.FontName);
        }

        protected void LoadParams()
        {
            Output.FontSize = ConfigManager.Config.FontSize;
            Output.Opacity = ConfigManager.Config.Opacity;
            Output.Foreground = (Brush)new BrushConverter().ConvertFromString(ConfigManager.Config.TerminalColor);
        }

        protected void UpdateCarriage()
        {
            new Thread(() =>
            {
                while (_update)
                {
                    _mutex?.WaitOne();

                    Dispatcher.BeginInvoke(DispatcherPriority.Background,
                    new Action(() =>
                    {
                        if (Output.Text.Length > 0 && Output.Text[Output.Text.Length - 1].ToString() == ConfigManager.Config.SpecialSymbol)
                            Output.Text = Output.Text.Remove(Output.Text.Length - 1);
                        else
                            Output.Text += ConfigManager.Config.SpecialSymbol;
                    }));

                    _mutex?.ReleaseMutex();

                    Thread.Sleep((int)ConfigManager.Config.DelayUpdateCarriage);

                }
            }).Start();
        }

        protected void AdditionalKeys(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Closing();
                    Addition.NavigationService.GoBack();
                    break;
                case Key.Enter:
                    Scroller.Focus();
                    break;
            }
        }
    }
}