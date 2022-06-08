using System;
using System.Windows;
using Terminal_XP.Frames;
using Terminal_XP.Classes;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Path = System.IO.Path;

namespace Terminal_XP
{
    public partial class MainWindow : Window
    {
        private string _theme;
        
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                ConfigManager.Load();
            
                _theme = ConfigManager.Config.Theme;
            
                LoadTheme(_theme);
                LoadParams();

                // Frame.NavigationService.Navigate(new TextViewPage(Addition.Local + "/Test.txt", _theme));
                var image = Path.GetFullPath(Addition.Local + "/Test.jpg");
                Frame.NavigationService.Navigate(new PictureViewPage(image, _theme));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
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
            
            DevicesManager.AddDisk += name => Logger.Debug($"Add disk: {name}");
            DevicesManager.RemoveDisk += name => Logger.Debug($"Remove disk: {name}");
            
            KeyDown += (obj, e) =>
            {
                switch (e.Key)
                {
                    case Key.Escape:
                        Close();
                        break;
                    case Key.R:
                        Frame.NavigationService.Content.GetType().GetMethod("Reload")?.Invoke(Frame.NavigationService.Content, default);
                        break;
                }
            };

            Closing += (obj, e) =>
            {
                Frame.NavigationService.Content.GetType().GetMethod("Closing")?.Invoke(Frame.NavigationService.Content, default);
            };
        }
    }
}
