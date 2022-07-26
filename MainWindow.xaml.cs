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

using Path = System.IO.Path;

namespace Terminal_XP
{
    public partial class MainWindow : Window
    {
        private string _theme;

        public MainWindow()
        {
            InitializeComponent();
            ConfigManager.Load();

            _theme = ConfigManager.Config.Theme;

            //Topmost = true;     /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////      ВЕРНУТЬ

            //Cursor = Cursors.None;

            LoadTheme(_theme);
            LoadParams();
            
            //ExecuteFile(Addition.Local + "/Test.flac");
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

            KeyDown += (obj, e) =>
            {
                switch (e.Key)
                {
                    case Key.R: //DEBUG
                        TryExcuteMethod(Frame.NavigationService.Content.GetType(), "Reload");
                        break;
                }
            };
            
            Closing += (obj, e) => DevicesManager.StopLisining();
            
            Frame.NavigationService.Navigate(new LoadingPage("", _theme));//G:\\TERMINAL TEST DIRECTORIES\\E\\йцу G:\\TERMINAL TEST DIRECTORIES\\E\\папка\\Новая папка
            //Frame.NavigationService.Navigate(new HackPage(Path.GetFullPath("Local/Test.jpg"), _theme));
        }

        

        private void TryExcuteMethod(Type type, string name)
        {
            try
            {
                foreach (var item in type.GetMethods())
                {
                    if (item.Name == name)
                        item.Invoke(Frame.NavigationService.Content, default);
                }
            }
            catch { }
        }
    }
}
