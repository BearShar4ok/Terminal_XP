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
        private bool _stop = true;
        
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
                // Frame.NavigationService.Navigate(new PictureViewPage(Path.GetFullPath(Addition.Local + "/Test.jpg"), _theme));
                // Frame.NavigationService.Navigate(new VideoViewPage(Addition.Local + "/Test.mp4", _theme));
                // Frame.NavigationService.Navigate(new AudioViewPage(Addition.Local + "/Test.mp3", _theme));
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
                    // TODO: Delete in future. In this moment using for test
                    case Key.R:
                        Frame.NavigationService.Content.GetType().GetMethod("Reload")?.Invoke(Frame.NavigationService.Content, default);
                        break;
                    // End TODO
                    case Key.Space:
                        Frame.NavigationService.Content.GetType().GetMethod(_stop ? "Pause" : "Play")?.Invoke(Frame.NavigationService.Content, default);

                        _stop = !_stop;
                        break;
                    case Key.Up:
                    case Key.VolumeUp:
                        Frame.NavigationService.Content.GetType().GetMethod("VolumePlus")?.Invoke(Frame.NavigationService.Content, default);
                        break;
                    case Key.Down:
                    case Key.VolumeDown:
                        Frame.NavigationService.Content.GetType().GetMethod("VolumeMinus")?.Invoke(Frame.NavigationService.Content, default);
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
