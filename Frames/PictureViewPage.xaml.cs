using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Terminal_XP.Classes;

namespace Terminal_XP.Frames
{
    public partial class PictureViewPage : Page
    {
        private string _filename;
        private string _theme;

        public PictureViewPage(string filename, string theme, bool clearPage = false)
        {
            InitializeComponent();

            if (clearPage)
                Addition.NavigationService.Navigated += RemoveLast;

            _filename = filename;
            _theme = theme;

            Application.Current.MainWindow.KeyDown += AdditionalKeys;

            LoadTheme(theme);
            LoadImage();
        }
        
        private void RemoveLast(object obj, NavigationEventArgs e)
        {
            Addition.NavigationService?.RemoveBackEntry();
        }

        public void Closing()
        {
            Addition.NavigationService.Navigated -= RemoveLast;
            Application.Current.MainWindow.KeyDown -= AdditionalKeys;
        }

        public void Reload()
        {
            LoadTheme(_theme);
            LoadImage();
        }

        private void LoadImage()
        {
            if (!File.Exists(_filename))
                return;

            Picture.Source = new BitmapImage(new Uri(_filename));
        }

        private void LoadTheme(string theme)
        {

        }
        
        private void AdditionalKeys(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Closing();
                    Addition.NavigationService?.GoBack();
                    break;
            }

        }
    }
}