using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Terminal_XP.Classes;

namespace Terminal_XP.Frames
{
    public partial class TextViewPage : Page
    {
        private string _filename;
        private string _theme;
        private bool _update;
        private Mutex _mutex = new Mutex();


        public TextViewPage(string filename, string theme)
        {
            InitializeComponent();
            LoadTheme(theme);
            LoadParams();

            _filename = filename;
            _theme = theme;
            Output.Text = ConfigManager.Config.SpecialSymbol;

            Application.Current.MainWindow.KeyDown += AdditionalKeys;


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
            if (!File.Exists(_filename))
                return;
            
            _update = true;
            
            new Thread(() =>
            {
                using (var stream = File.OpenText(_filename))
                {
                    var text = stream.ReadToEnd();

                    Addition.PrintLines(Output, Dispatcher, ref _update, _mutex,
                        new FragmentText(text,
                            ConfigManager.Config.UsingDelayFastOutput ? ConfigManager.Config.DelayFastOutput : 0));
                    UpdateCarriage();
                }
            }).Start();
        }

        private void LoadTheme(string name)
        {
            Output.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "Assets/Themes/Fallout/#Fallout Regular");
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
                case Key.Escape:
                    Closing();
                    GC.Collect();
                    NavigationService.GoBack();
                    break;
            }
        }
    }
}