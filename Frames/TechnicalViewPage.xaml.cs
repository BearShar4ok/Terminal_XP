
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
    public partial class TechnicalViewPage 
    {
        private string _theme;
        private bool _update;
        private Page nextPage;
        private Mutex _mutex = new Mutex();


        public TechnicalViewPage(string theme, Page nextPage, bool clearPage = false)
        {
            InitializeComponent();

            if (clearPage)
                Addition.NavigationService.Navigated += RemoveLast;

            this.nextPage = nextPage;

            LoadTheme(theme);
            LoadParams();

            Output.Text = ConfigManager.Config.SpecialSymbol;

            Application.Current.MainWindow.KeyDown += AdditionalKeys;

            LoadText();
        }
        
        private void RemoveLast(object obj, NavigationEventArgs e)
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

        private void LoadText()
        {
            _update = true;

            new Thread(() =>
            {
                using (var stream = File.OpenText(Path.GetFullPath(Addition.Themes + _theme + $@"/Hello.txt")))
                {
                    var text = stream.ReadToEnd();

                    FragmentText fr = new FragmentText(text,
                            ConfigManager.Config.UsingDelayFastOutput ? (uint)40 : 0);
                    try
                    {
                        Addition.PrintLines(Output, Scroller, Dispatcher, ref _update, _mutex, fr);
                    }
                    catch (Exception ex)
                    {
                        string a = ex.Message.ToString();
                    }
                   
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
                    Addition.NavigationService.Navigate(nextPage);
                    break;
            }
        }
    }
}