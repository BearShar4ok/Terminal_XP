using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Terminal_XP.Classes;
using System.Windows.Controls.Primitives;
using Microsoft.SqlServer.Server;
using Path = System.IO.Path;

namespace Terminal_XP.Frames
{
    /// <summary>
    /// Логика взаимодействия для LoadingPage.xaml
    /// </summary>
    public partial class LoadingPage : Page
    {
        private string directory = "";
        private string theme;
        private int deepOfPath = 0;
        private KeyStates prevkeyState;
        private Dictionary<string, ListBoxItem> disks = new Dictionary<string, ListBoxItem>();

        //📂🖹🖻🖺🖾 🖼
        private string passFoler = @"G:\Coding\MyProjects\Terminal_XP\Terminal_XP\Assets\Themes\Fallout\folder.png";
        private string passImage = @"G:\Coding\MyProjects\Terminal_XP\Terminal_XP\Assets\Themes\Fallout\image.png";
        private string passText = @"G:\Coding\MyProjects\Terminal_XP\Terminal_XP\Assets\Themes\Fallout\text.png";
        public LoadingPage(string startDirectory, string theme )
        {
            InitializeComponent();
            DevicesManager.AddDisk += Add;
            DevicesManager.RemoveDisk += Rem;

            directory = startDirectory;
            CalculationOfDeepLevel();
            lblDirectory.Content = directory + "         deep: " + deepOfPath;
            this.theme = theme;

            lstB.SelectionMode = SelectionMode.Single;
            lstB.ContextMenu = new ContextMenu();

            lstB.SelectedIndex = 0;

            KeyDown += AdditionalKeys;
            lstB.Focus();

            DevicesManager.StartLisining();
            OpenFolder();
        }
        private void CalculationOfDeepLevel()
        {
            var temp = directory.Split('\\');
            bool schet = false;
            foreach (var item in temp)
            {
                if (schet)
                {
                    deepOfPath++;
                    continue;
                }
                if (item== "TERMINAL TEST DIRECTORIES")
                {
                    schet = true;
                }
            }
        }
        private void Add(string text)
        {
            System.Diagnostics.Debug.WriteLine("add: " + text);

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
            {
                string[] allFiles;
                try
                {
                    allFiles = Directory.GetFiles(text);
                    for (int i = 0; i < allFiles.Length; i++)
                    {
                        allFiles[i] = allFiles[i].Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries)[1];
                    }

                    if (allFiles.Contains("file.txt"))
                    {
                        
                        string diskName;
                        string fullPath;
                        using (StreamReader sr = new StreamReader(text + "file.txt"))
                        {
                            string temp = sr.ReadToEnd();
                            string[] temp2 = temp.Split('\\');
                            diskName = temp2[temp2.Length - 1];
                            
                            fullPath = temp;
                        }
                        ListBoxItem lbi = new ListBoxItem()
                        {
                            DataContext = new BitmapImage(new Uri(passFoler)),
                            Content = diskName,
                            Tag = fullPath,
                            Style = (Style)Resources["ImageText"],
                        };
                        if (deepOfPath==0)
                        {
                            lstB.Items.Add(lbi);
                        }
                       
                        disks.Add(text, lbi);
                    }
                    else
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }

                //lblDirectory.Content = text + "         deep: " + deepOfPath;
            }));

        }
        private void Rem(string text)
        {
            System.Diagnostics.Debug.WriteLine("remove: " + text);

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
            {
                object o = disks[text];
                lstB.Items.Remove(o);
                disks.Remove(text);
            }));
        }

        private void lstB_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string path = (string)(((ListBoxItem)(lstB.SelectedItem)).Tag);
            lblDirectory.Content = directory + "         deep: " + deepOfPath;
            if (IsFolder(path))
            {
                directory = path;
                deepOfPath++;
                OpenFolder();
                
            }
            else
            {
                ExecuteFile();
            }
        }
        private void OpenFolder()
        {
            lblDirectory.Content = directory + "         deep: " + deepOfPath;
            string[] allFiles;
            try
            {
                allFiles = Directory.GetFiles(directory);
            }
            catch (Exception)
            {
                allFiles = new string[0];
            }
            lstB.Items.Clear();
            for (int i = 0; i < allFiles.Length; i++)
            {
                string[] template = allFiles[i].Split('\\');
                string text = (template[template.Length - 1].Split('.'))[0];
                string format = (template[template.Length - 1].Split('.'))[1].ToLower();
                //  🖹🖻🖺
                ListBoxItem lstBI = new ListBoxItem()
                {
                    Content = text,
                    Tag = directory + "\\" + text + "." + format,
                    Style = (Style)Resources["ImageText"],
                };

                switch (format)
                {
                    case "txt":
                        lstBI.DataContext = new BitmapImage(new Uri(passText));
                        break;
                    case "png":
                        lstBI.DataContext = new BitmapImage(new Uri(passImage));
                        break;
                    case "jpg":
                        lstBI.DataContext = new BitmapImage(new Uri(passImage));
                        break;
                    case "bmp":
                        lstBI.DataContext = new BitmapImage(new Uri(passImage));
                        break;
                    default:
                        break;
                }
                lstB.Items.Add(lstBI);
            }
            string[] allDirectories;
            try
            {
                allDirectories = Directory.GetDirectories(directory);
            }
            catch (Exception)
            {
                allDirectories = new string[0];
            }
            for (int i = 0; i < allDirectories.Length; i++)
            {
                string[] template = allDirectories[i].Split('\\');
                string text = (template[template.Length - 1].Split('.'))[0];
                if (text == "System Volume Information")
                {
                    continue;
                }
                ListBoxItem lstBI = new ListBoxItem()
                {
                    DataContext = new BitmapImage(new Uri(passFoler)),
                    Content = text,
                    Tag = directory + "\\" + text,
                    Style = (Style)Resources["ImageText"],
                };

                lstB.Items.Add(lstBI);
            }
            lstB.SelectedIndex = 0;
        }
        private void AdditionalKeys(object sender, KeyEventArgs e)
        {
            if (prevkeyState == e.KeyStates)
            {
                return;
            }
            prevkeyState = e.KeyStates;
            switch (e.Key)
            {
                case Key.Enter:
                    lstB_MouseDoubleClick(null, null);
                    break;
                case Key.Escape:
                    if (deepOfPath==0)
                    {
                        return;
                    }
                    deepOfPath--;
                    if (deepOfPath <= 0)
                    {
                        DevicesManager.ClearAllDisks();
                        lstB.Items.Clear();
                        deepOfPath = 0;
                        return;
                    }
                    directory = directory.Remove(directory.LastIndexOf("\\"));
                    OpenFolder();
                    lblDirectory.Content = directory + "         deep: " + deepOfPath;
                    break;
                default:
                    break;
            }

        }
        private void ExecuteFile()
        {
            var exct = Path.GetExtension(directory).Remove(0, 1);

            var audio = new[] { "wav", "m4a", "mp3", "flac" };
            var picture = new[] { "jpeg", "jpg", "tiff", "bmp" };
            var video = new[] { "mp4", "gif", "wmv", "avi" };
            var text = new[] { "txt" };

            if (audio.Contains(exct))
                NavigationService.Navigate(new AudioViewPage(directory, theme));

            if (picture.Contains(exct))
                NavigationService.Navigate(new PictureViewPage(directory, theme));

            if (text.Contains(exct))
                NavigationService.Navigate(new TextViewPage(directory, theme));

            if (video.Contains(exct))
                NavigationService.Navigate(new VideoViewPage(directory, theme));
        }
        private bool IsFolder(string text)
        {
            return !(text.Contains("."));
        }
    }
}
