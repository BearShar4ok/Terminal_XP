using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Terminal_XP.Classes;

namespace Terminal_XP.Frames
{
    public partial class AudioViewPage : Page
    {
        // Count 
        public const int CntSymbol = 64;
        public const char CharAllLen = '=';
        public const char CharCurrLen = '>';
        
        private string _filename;
        private string _theme;
        private bool _stop;
        private MediaPlayer _player = new MediaPlayer();
        private DispatcherTimer _timer = new DispatcherTimer(DispatcherPriority.Input);
        private bool _loaded;

        // Set player's volume from 0 to 1
        public double Volume
        {
            get => _player.Volume;
            set
            {
                _player.Volume = Math.Max(Math.Min(value, 1.0), 0.0);
            }
        }

        public AudioViewPage(string filename, string theme)
        {
            InitializeComponent();

            _filename = filename;
            _theme = theme;

            // Init timer to update progress bar
            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += UpdateProgressBar;
            _timer.Start();

            // Event to restart playing audio
            _player.MediaEnded += (obj, e) =>
            {
                _player.Position = new TimeSpan(0, 0, 0);
                _player.Play();
            };
            
            // Init Progress bar, when audio loaded
            _player.MediaOpened += (obj, e) =>
            {
                ProgressBar.Text = $"[{new string(CharAllLen, CntSymbol)}]";
                _loaded = true;
            };

            Application.Current.MainWindow.KeyDown += AdditionalKeys;

            LoadTheme(theme);
            LoadAudio();
        }

        // Method to invoke when page closing
        public void Closing()
        {
            Application.Current.MainWindow.KeyDown -= AdditionalKeys;
            _loaded = false;

            _timer.Stop();
            Stop();
            _player.Close();
        }

        // Method for relaod page
        public void Reload()
        {
            LoadTheme(_theme);
            LoadAudio();
        }
        
        private void LoadTheme(string name)
        {

        }
        
        public void Play() => _player.Play();

        public void Stop() => _player.Stop();

        public void Pause() => _player.Pause();

        public void VolumePlus() => Volume += 0.01d;

        public void VolumeMinus() => Volume -= 0.01d;

        // Method to load audio
        private void LoadAudio()
        {
            if (!File.Exists(_filename))
                return;

            // Stopping last audio file
            Stop();
            _player.Close();

            // Open and play audio
            _player.Open(new Uri(_filename, UriKind.Relative));
            _player.Play();
        }

        private void UpdateProgressBar(object sender, EventArgs e)
        {
            if (!_loaded)
                return;

            var nowTime = _player.Position.TotalSeconds;
            // if nowTime > TotalSeconds, set last 
            nowTime = nowTime > _player.NaturalDuration.TimeSpan.TotalSeconds ? _player.NaturalDuration.TimeSpan.TotalSeconds : nowTime;

            // Get index symbol to change
            var ind = (int)Math.Ceiling(nowTime / _player.NaturalDuration.TimeSpan.TotalSeconds * CntSymbol);
            // Set new line to progress bar
            ProgressBar.Text = $"[{new string(CharCurrLen, ind)}{new string(CharAllLen, CntSymbol - ind)}]";
        }
        
        private void AdditionalKeys(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    // Closing page and go to loading page
                    Closing();
                    NavigationService?.GoBack();
                    break;
                case Key.Space:
                    if (_stop)
                        Pause();
                    else
                        Play();
                    _stop = !_stop;
                    break;
                case Key.Up:
                case Key.VolumeUp:
                    VolumePlus();
                    break;
                case Key.Down:
                case Key.VolumeDown:
                    VolumeMinus();
                    break;
            }

        }
    }
}