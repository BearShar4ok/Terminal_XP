using System;
using System.Diagnostics;
using System.IO;
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
    public partial class TechnicalViewPage : Page
    {
        private string _theme;
        private bool _update;
        private Page _nextPage;
        private Action _setParams;
        private Mutex _mutex = new Mutex();

        public TechnicalViewPage()
        {
            InitializeComponent();
        }

        public void SetParams(Page nextPage, Action setParams, string theme)
        {
            _nextPage = nextPage;
            _setParams = setParams;
            Output.Text = ConfigManager.Config.SpecialSymbol;

            Application.Current.MainWindow.KeyDown += AdditionalKeys;

            LoadTheme(theme);
            LoadParams();
            LoadText();
        }

        public void Closing()
        {
            _update = false;
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

        private void LoadText()
        {
            _update = true;

            new Thread(() =>
            {
                using (var stream = File.OpenText(Path.GetFullPath(Addition.Themes + _theme + $@"/Hello.txt")))
                {
                    var text = stream.ReadToEnd();

                    var fr = new FragmentText(text,
                            ConfigManager.Config.UsingDelayFastOutput ? (uint)40 : 0);
                    Addition.PrintLines(Output, Dispatcher, ref _update, _mutex, fr);
                   
                    UpdateCarriage();
                }
            }).Start();
        }

        private void LoadTheme(string theme)
        {
            _theme = theme;
            Output.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), Addition.Themes + theme + "/#" + ConfigManager.Config.FontName);
        }

        private void LoadParams()
        {
            Output.FontSize = ConfigManager.Config.FontSize;
            Output.Opacity = ConfigManager.Config.Opacity;
            Output.Foreground = (Brush)new BrushConverter().ConvertFromString(ConfigManager.Config.TerminalColor);
        }

        private void UpdateCarriage()
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

        private void AdditionalKeys(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    Closing();
                    _setParams?.Invoke();
                    Addition.NavigationService.Navigate(_nextPage);
                    break;
            }
        }
    }
}