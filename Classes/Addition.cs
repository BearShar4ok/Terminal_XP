using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using System.Windows.Threading;
using Terminal_XP.Frames;

namespace Terminal_XP.Classes
{
    public static class Addition
    {
        public const string Themes = "Assets/Themes/";
        public const string Assets = "Assets";
        public const string ErrorFile = "files/Error.log";
        
        public static void ForEach<T>(this IEnumerable<T> lst, Action<T> action)
        {
            if (action == null)
                return;
            
            foreach (var item in lst)
            {
                action?.Invoke(item);
            }
        }
        
        public static T To<T>(this object obj)
        {
            if (obj is T tobj)
                return tobj;

            return default;
        }

        public static void PrintLines<T>(T element, Dispatcher dispatcher, Mutex mutex = default, params FragmentText[] TextArray) where T : TextBlock
        {
            foreach (var fragmentText in TextArray)
            {
                foreach (var symbol in fragmentText.Text)
                {
                    mutex?.WaitOne();
                    
                    dispatcher.BeginInvoke(DispatcherPriority.Background,
                    new Action(() =>
                    {
                        if (element.Text.Length > 0 && element.Text[element.Text.Length - 1].ToString() == ConfigManager.Config.SpecialSymbol)
                            element.Text = element.Text.Insert(element.Text.Length - 1, symbol.ToString());
                        else
                            element.Text += symbol.ToString();
                    }));
                    
                    mutex?.ReleaseMutex();

                    if (fragmentText.Delay > 0)
                        Thread.Sleep((int)fragmentText.Delay);
                }
            }
        }

        public static Page GetPageByFilename(string filename, string theme)
        {
            var exct = Path.GetExtension(filename).Remove(0, 1);

            var audio = new[] { "wav", "m4a", "mp3", "flac" };
            var picture = new[] { "jpeg", "png", "jpg", "tiff", "bmp" };
            var video = new[] { "mp4", "gif", "wmv", "avi" };
            var text = new[] { "txt" };

            if (audio.Contains(exct))
                return new AudioViewPage(filename, theme);

            if (picture.Contains(exct))
                return new PictureViewPage(filename, theme);

            if (text.Contains(exct))
                return new TextViewPage(filename, theme);

            if (video.Contains(exct))
                return new VideoViewPage(filename, theme);

            return default;
        }
    }
}