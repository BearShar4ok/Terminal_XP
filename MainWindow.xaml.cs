using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using Terminal_XP.Frames;
using Terminal_XP.Classes;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using Terminal_XP.Windows;
using Path = System.IO.Path;
using Newtonsoft.Json;

namespace Terminal_XP
{
    public partial class MainWindow : Window
    {
        private readonly string _theme;

        public MainWindow()
        {
            InitializeComponent();
            ConfigManager.Load();

            _theme = ConfigManager.Config.Theme;

            if (!Addition.IsDebugMod)
            {
                //Topmost = true;
                Cursor = Cursors.None;
            }

            LoadTheme(_theme);
            LoadParams();
        }

        private void LoadTheme(string name)
        {
            Background = new ImageBrush() { ImageSource = new BitmapImage(new Uri(Addition.Themes + name + "/Background.png", UriKind.Relative)) };
        }

        private void LoadParams()
        {
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            ResizeMode = ResizeMode.NoResize;
            AllowsTransparency = true;
            Closing += (obj, e) => DevicesManager.StopListening();

             Addition.NavigationService?.Navigate(new TechnicalViewPage(_theme, new LoadingPage(_theme)));
            //var hw = new HackWindow(_theme, "pas");
            //hw.Show();
        }

        private void TryExecuteMethod(object obj, Type type, string name)
        {
            try
            {
                foreach (var item in type.GetMethods())
                {
                    if (item.Name == name)
                        item.Invoke(obj, default);
                }
            }
            catch { }
        }
    }
}
