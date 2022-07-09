using System;
using System.Linq;
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

            ConfigManager.Load();

            _theme = ConfigManager.Config.Theme;

            LoadTheme(_theme);
            LoadParams();

            ExecuteFile(Addition.Local + "/Test.flac");
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
                        TryExcuteMethod(Frame.NavigationService.Content.GetType(), "Reload");
                        break;
                    // End TODO
                    case Key.Space:
                        TryExcuteMethod(Frame.NavigationService.Content.GetType(), _stop ? "Pause" : "Play");

                        _stop = !_stop;
                        break;
                    case Key.Up:
                    case Key.VolumeUp:
                        TryExcuteMethod(Frame.NavigationService.Content.GetType(), "VolumePlus");
                        break;
                    case Key.Down:
                    case Key.VolumeDown:
                        TryExcuteMethod(Frame.NavigationService.Content.GetType(), "VolumeMinus");
                        break;
                }
            };

            Closing += (obj, e) =>
            {
                TryExcuteMethod(Frame.NavigationService.Content.GetType(), "Closing");
            };
        }

        private void ExecuteFile(string filename)
        {
            var exct = Path.GetExtension(filename).Remove(0, 1);

            var picture = new[] { "jpeg", "jpg", "tiff", "bmp" };
            var video = new[] { "mp4", "gif", "wmv", "avi" };
            var text = new[] { "txt" };
            var audio = new[] { "wav", "m4a", "mp3", "flac" };

            if (picture.Contains(exct))
                Frame.NavigationService.Navigate(new PictureViewPage(filename, _theme));

            if (text.Contains(exct))
                Frame.NavigationService.Navigate(new TextViewPage(filename, _theme));

            if (video.Contains(exct))
                Frame.NavigationService.Navigate(new VideoViewPage(filename, _theme));

            if (audio.Contains(exct))
                Frame.NavigationService.Navigate(new AudioViewPage(filename, _theme));
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
            catch{}
        }
    }
}
