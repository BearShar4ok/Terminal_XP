using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Terminal_XP.Frames
{
    public partial class PictureViewPage : Page
    {
        private string _filename;
        private string _theme;
        
        public PictureViewPage(string filename, string theme)
        {
            InitializeComponent();

            _filename = filename;
            _theme = theme;
            
            LoadTheme(theme);
            LoadImage();
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
    }
}