using System;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Terminal_XP.Classes
{
    public static class Addition
    {
        public const string Themes = "Assets/Themes/";
        public const string Assets = "Assets";
        public const string Local = "Local";
        public const string ErrorFile = "files/Error.log";
        
        public static T To<T>(this object obj)
        {
            if (obj is T tobj)
                return tobj;

            return default;
        }

        public static void PrintLines<T>(T element, Dispatcher dispatcher,
            params FragmentText[] TextArray) where T : TextBlock
            => PrintLines(element, dispatcher, default, TextArray);

        public static void PrintLines<T>(T element, Dispatcher dispatcher, Mutex mutex = default, params FragmentText[] TextArray) where T : TextBlock
        {
            foreach (var fragmentText in TextArray)
            {
                foreach (var symbol in fragmentText.Text)
                {
                    mutex?.WaitOne();
                    
                    dispatcher.Invoke(DispatcherPriority.Background,
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
    }
}