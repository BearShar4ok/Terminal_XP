using System;
using System.Windows.Controls;

namespace Terminal_XP.Frames
{
    public partial class VideoViewPage : Page
    {
        private string _filename;
        private string _theme;

        public double Volume
        {
            get => MediaPlayer.Volume;
            set
            {
                MediaPlayer.Volume = Math.Max(Math.Min(value, 1.0), 0.0);
            }
        }

        public VideoViewPage(string filename, string theme)
        {
            InitializeComponent();
            
            _filename = filename;
            _theme = theme;

            //Unloaded += (obj, e) => { Closing(); };
            
            MediaPlayer.MediaEnded += (obj, e) => { 
                MediaPlayer.Position = new TimeSpan(0, 0, 0);
                MediaPlayer.Play();
            };

            MediaPlayer.Focusable = false;

            LoadTheme(theme);
            LoadVideo(filename);
        }

        public void Closing()
        {
            Stop();
        }
        
        public void Reload()
        {
            LoadTheme(_theme);
            LoadVideo(_filename);
        }

        private void LoadVideo(string filename)
        {
            MediaPlayer.Source = new Uri(filename, UriKind.Relative);
            
            Stop();
            Play();
        }

        public void Pause() => MediaPlayer.Pause();
        
        public void Play() => MediaPlayer.Play();
        
        public void Stop() => MediaPlayer.Stop();

        public void VolumePlus() => Volume += 0.01d;
        
        public void VolumeMinus() => Volume -= 0.01d;

        private void LoadTheme(string theme)
        {
            
        }
    }
}