using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
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
using static System.Net.Mime.MediaTypeNames;
using System.Threading;

namespace Terminal_XP.Frames
{
    //TODO: ИСПРАВИТЬ -> при переход\е на новый уровень в дереве фаылов при первичном нажатии фокус попадает на ListTextBox, хотя он там есть

    public partial class LoadingPage : Page
    {
        private string directory = "";
        private string theme;

        private int deepOfPath = 0;
        private int selectedIndex = 0;

        private KeyStates prevkeyState;

        private Dictionary<string, ListBoxItem> disks = new Dictionary<string, ListBoxItem>();

        private string passFoler;
        private string passImage;
        private string passText;
        private string passAudio;
        private string passVideo;
        private string passDefault;

        private const string asseccFileToReadDisk = "READ_DISK.txt";

        private static NavigationService _NavigationService { get; } = (System.Windows.Application.Current.MainWindow as MainWindow).Frame.NavigationService;

        public LoadingPage(string startDirectory, string theme)
        {
            InitializeComponent();
            // Add actions to devices
            DevicesManager.AddDisk += Add;
            DevicesManager.RemoveDisk += Remove;
            // Preparation in file tree
            directory = startDirectory;
            CalculationOfDeepLevel();
            DisplayDirectory();
            // Preparation of ListTextBox
            lstB.SelectionMode = SelectionMode.Single;
            lstB.SelectedIndex = 0;
            lstB.FocusVisualStyle = null;
            lstB.Focus();

            KeepAlive = true;

            KeyDown += AdditionalKeys;
            KeepAlive = true;

            this.theme = theme;
            passFoler = Addition.Themes + theme + @"/folder.png";
            passImage = Addition.Themes + theme + @"/image.png";
            passText = Addition.Themes + theme + @"/text.png";
            passAudio = Addition.Themes + theme + @"/audio.png";
            passVideo = Addition.Themes + theme + @"/video.png";
            passDefault = Addition.Themes + theme + @"/default.png";
            DevicesManager.StartLisining();

            OpenFolder();
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

                    if (allFiles.Contains(asseccFileToReadDisk))
                    {

                        string diskName;
                        string fullPath;
                        using (StreamReader sr = new StreamReader(text + asseccFileToReadDisk))
                        {
                            fullPath = sr.ReadToEnd();
                            diskName = Path.GetFileNameWithoutExtension(fullPath);
                        }
                        ListBoxItem lbi = new ListBoxItem()
                        {
                            DataContext = new BitmapImage(new Uri(Path.GetFullPath(passFoler))),
                            Content = diskName,
                            Tag = fullPath,
                            Style = (Style)Resources["ImageText"],
                        };
                        if (deepOfPath == 0)
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
            }));

        }
        private void Remove(string text)
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
            directory = (string)((ListBoxItem)lstB.SelectedItem).Tag;
            if (directory.Split('\\')[directory.Split('\\').Length - 1] == "..")
            {
                if (deepOfPath == 0)
                    return;
                deepOfPath--;
                if (deepOfPath == 0)
                {
                    DevicesManager.ClearAllDisks();
                    lstB.Items.Clear();
                    deepOfPath = 0;
                    DisplayDirectory();
                    lstB.SelectedIndex = 0;
                    selectedIndex = lstB.SelectedIndex;
                    return;
                }
                directory = directory.Remove(directory.LastIndexOf("\\"));
                directory = directory.Remove(directory.LastIndexOf("\\"));
                DisplayDirectory();
                OpenFolder();
                return;
            }

            if (IsFolder(directory))
            {
                deepOfPath++;
                DisplayDirectory();
                OpenFolder();
            }
            else
            {
                selectedIndex = lstB.SelectedIndex;
                ExecuteFile();
                lstB.Focus();
            }
        }
        private void OpenFolder()
        {
            FindFolders();
            FindFiles();
            lstB.SelectedIndex = 0;
            selectedIndex = lstB.SelectedIndex;
            lstB.Focus();
        }
        // Look for all files in directory
        private void FindFiles()
        {
            string[] allFiles;
            try
            {
                allFiles = Directory.GetFiles(directory);
            }
            catch (Exception)
            {
                allFiles = new string[0];
            }
            for (int i = 0; i < allFiles.Length; i++)
            {
                string text = Path.GetFileNameWithoutExtension(allFiles[i]);
                string format = Path.GetExtension(allFiles[i]).Remove(0, 1);
                //  🖹🖻🖺

                //if (format == "config")
                //continue;

                ListBoxItem lstBI = new ListBoxItem()
                {
                    Content = text,
                    Tag = directory + "\\" + text + "." + format,
                    Style = (Style)Resources["ImageText"],
                };
                switch (format)
                {
                    case "txt":
                        lstBI.DataContext = new BitmapImage(new Uri(Path.GetFullPath(passText)));
                        break;
                    case "png":
                    case "jpg":
                    case "jepg":
                    case "tiff":
                    case "bmp":
                        lstBI.DataContext = new BitmapImage(new Uri(Path.GetFullPath(passImage)));
                        break;
                    case "wav":
                    case "m4a":
                    case "mp3":
                    case "flac":
                        lstBI.DataContext = new BitmapImage(new Uri(Path.GetFullPath(passAudio)));
                        break;
                    case "gif":
                    case "mp4":
                    case "wmv":
                    case "avi":
                        lstBI.DataContext = new BitmapImage(new Uri(Path.GetFullPath(passVideo)));
                        break;
                    default:
                        lstBI.DataContext = new BitmapImage(new Uri(Path.GetFullPath(passDefault)));
                        break;
                }
                lstB.Items.Add(lstBI);
            }
        }
        // Look for all folders in directory
        private void FindFolders()
        {
            string[] allDirectories;
            try
            {
                allDirectories = Directory.GetDirectories(directory);
            }
            catch (Exception)
            {
                allDirectories = new string[0];
            }
            lstB.Items.Clear();
            if (deepOfPath != 0)
            {
                ListBoxItem lstBI_ = new ListBoxItem()
                {
                    DataContext = new BitmapImage(new Uri(Path.GetFullPath(passFoler))),
                    Content = "..",
                    Tag = directory + "\\..",
                    Style = (Style)Resources["ImageText"],
                };
                lstB.Items.Add(lstBI_);
            }
            for (int i = 0; i < allDirectories.Length; i++)
            {
                string text = Path.GetFileNameWithoutExtension(allDirectories[i]);
                if (text == "System Volume Information")
                {
                    continue;
                }
                ListBoxItem lstBI = new ListBoxItem()
                {
                    DataContext = new BitmapImage(new Uri(Path.GetFullPath(passFoler))),
                    Content = text,
                    Tag = directory + "\\" + text,
                    Style = (Style)Resources["ImageText"],
                };

                lstB.Items.Add(lstBI);
            }
        }
        // All additional keys, which cant be used in System hotkeys
        private void AdditionalKeys(object sender, KeyEventArgs e)
        {
            if (prevkeyState == e.KeyStates)
                return;
            prevkeyState = e.KeyStates;
            switch (e.Key)
            {
                case Key.Enter:
                    lstB_MouseDoubleClick(null, null);
                    break;
                default:
                    break;
            }
        }
        private void OpenFile(bool hachResult)
        {
            if (hachResult)
            {
                // Success in huch
                //MessageBox.Show("Success login");
                _NavigationService.GoBack();
                _NavigationService.Navigate(Addition.GetPageByFilename(directory, theme));
            }
            else
            {
                // Fail in huch
                //MessageBox.Show("Fail login");
                _NavigationService.GoBack();
            }
        }
        private void StartHucking()
        {
            //MessageBox.Show("start huck");
            HackPage hp = new HackPage(theme);
            _NavigationService.Navigate(hp);
            hp.SuccessfullyHacking += OpenFile;
        }
        private void ExecuteFile()
        {
            ConfigDeserializer content;
            if (Directory.GetFiles(directory.Remove(directory.LastIndexOf("\\"))).Contains((string)((ListBoxItem)lstB.SelectedItem).Tag + ".config"))// Looking for .config
            {
                try
                {
                    content = JsonConvert.DeserializeObject<ConfigDeserializer>(File.ReadAllText((string)((ListBoxItem)lstB.SelectedItem).Tag + ".config"));
                    if (!content.HasPassword)
                    {
                        _NavigationService.Navigate(Addition.GetPageByFilename(directory, theme));
                    }
                    else
                    {
                        LoginPage lp = new LoginPage(theme, content.LoginsAndPasswords);

                        _NavigationService.Navigate(lp);
                        lp.LogingIn += OpenFile;
                        lp.StartHuch += StartHucking;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            }
            else
            {
                _NavigationService.Navigate(Addition.GetPageByFilename(directory, theme));
            }
            lstB.SelectedIndex = selectedIndex;
            lstB.Focus();
        }
        // Debug pink line
        private void DisplayDirectory()
        {
            var elems = directory.Split('\\');
            string path = "";
            for (int i = elems.Length - 1; i > elems.Length - 1 - deepOfPath; i--)
            {
                path = elems[i] + "\\" + path;
            }
            lblDirectory.Content = path;
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
                if (item == "TERMINAL TEST DIRECTORIES")
                {
                    schet = true;
                }
            }
        }
        private bool IsFolder(string text)
        {
            return !(text.Contains("."));
        }
        private void Closing()
        {
            DevicesManager.StopLisining();
        }
    }
}
