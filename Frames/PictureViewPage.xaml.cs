using System;
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
            LoadImage(filename);
        }

        public void Reload()
        {
            LoadTheme(_theme);
            LoadImage(_filename);
        }

        private void LoadImage(string filename)
        {
            Picture.Source = new BitmapImage(new Uri(filename));
        }

        private void LoadTheme(string theme)
        {
            
        }
    }
}