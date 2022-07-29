using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Navigation;
using System.Windows.Threading;
using Terminal_XP.Frames;

namespace Terminal_XP.Classes
{
    public static class Addition
    {
        // Path to directory with themes 
        public const string Themes = "Assets/Themes/";
        // Path to icons folder in theme
        public const string Icons = "Icons";
        // Path to directory with assets 
        public const string Assets = "Assets";
        // Path to directory with error.log file 
        public const string ErrorFile = "files/Error.log";

        public static LoadingPage LoadingPage = new LoadingPage();
        public static LoginPage LoginPage = new LoginPage();
        public static HackPage HackPage = new HackPage();
        public static TechnicalViewPage TechnicalViewPage = new TechnicalViewPage();
        public static TextViewPage TextViewPage = new TextViewPage();
        public static AudioViewPage AudioViewPage = new AudioViewPage();
        public static VideoViewPage VideoViewPage = new VideoViewPage();
        public static PictureViewPage PictureViewPage = new PictureViewPage();

        // Dubug Mode
        public const bool IsDebugMod = false;

        // NavigationService
        public static NavigationService NavigationService { get; } = (Application.Current.MainWindow as MainWindow)?.Frame.NavigationService;

        // Get files types
        public static readonly string[] Audio = { "wav", "m4a", "mp3", "aac", "flac" };
        public static readonly string[] Image = { "jpeg", "png", "jpg", "tiff", "bmp" };
        public static readonly string[] Video = { "mp4", "gif", "wmv", "avi" };
        public static readonly string[] Text = { "txt" };

        public static void ForEach<T>(this IEnumerable<T> lst, Action<T> action)
        {
            if (action == null)
                return;

            foreach (var item in lst)
            {
                action.Invoke(item);
            }
        }

        public static J Pop<T, J>(this Dictionary<T, J> dct, T key)
        {
            if (!dct.ContainsKey(key))
                return default;

            var item = dct[key];
            dct.Remove(key);

            return item;
        }

        public static ListBoxItem GetFirst(this ListBox listBox) => (ListBoxItem)listBox.SelectedItem;

        public static T FindKey<T, J>(this Dictionary<T, J> dct, J val) =>
            dct.Keys.FirstOrDefault(key => dct[key].Equals(val));

        public static string RemoveLast(this string str, string key)
        {
            return str.Contains(key) ? str.Remove(str.LastIndexOf(key, StringComparison.Ordinal)) : str;
        }

        // Method to print list string to textblock with delay
        public static void PrintLines<T>(T element, Dispatcher dispatcher, ref bool working, Mutex mutex = default, params FragmentText[] TextArray) where T : TextBlock
        {
            foreach (var fragmentText in TextArray)
            {
                foreach (var symbol in fragmentText.Text)
                {
                    if (!working)
                        return;

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

        public static void GoBack(string filename, string theme)
        {
            Addition.LoadingPage.SetParams(filename.Replace($"{Path.GetFileName(filename)}", "").RemoveLast("\\").RemoveLast("/"), theme);
            Addition.NavigationService?.Navigate(Addition.LoadingPage);
        }

        // Get page file by filename
        public static Page GetPageByFilename(string filename, string theme)
        {
            if (File.Exists(filename))
            {
                var exct = Path.GetExtension(filename).Remove(0, 1);
                
                if (Audio.Contains(exct))
                {
                    AudioViewPage.SetParams(filename, theme);
                    return AudioViewPage;
                }

                if (Video.Contains(exct))
                {
                    VideoViewPage.SetParams(filename, theme);
                    return VideoViewPage;
                }

                if (Image.Contains(exct))
                {
                    PictureViewPage.SetParams(filename, theme);
                    return PictureViewPage;
                }

                if (Text.Contains(exct))
                {
                    TextViewPage.SetParams(filename, theme);
                    return TextViewPage;
                }
            }

            if (Directory.Exists(filename))
            {
                LoadingPage.SetParams(filename, theme);
                return LoadingPage;
            }

            return default;
        }
    }
}