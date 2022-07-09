﻿using System;
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
using Terminal_XP.Pages;
using System.ComponentModel;

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

                DevicesManager.StopLisining();
            };

            Frame.NavigationService.Navigate(new Uri("Pages/LoadingPage.xaml", UriKind.Relative));
        }

        private void ExecuteFile(string filename)
        {
            var exct = Path.GetExtension(filename).Remove(0, 1);

            var audio = new[] { "wav", "m4a", "mp3", "flac" };
            var picture = new[] { "jpeg", "jpg", "tiff", "bmp" };
            var video = new[] { "mp4", "gif", "wmv", "avi" };
            var text = new[] { "txt" };

            if (audio.Contains(exct))
                Frame.NavigationService.Navigate(new AudioViewPage(filename, _theme));

            if (picture.Contains(exct))
                Frame.NavigationService.Navigate(new PictureViewPage(filename, _theme));

            if (text.Contains(exct))
                Frame.NavigationService.Navigate(new TextViewPage(filename, _theme));

            if (video.Contains(exct))
                Frame.NavigationService.Navigate(new VideoViewPage(filename, _theme));
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
