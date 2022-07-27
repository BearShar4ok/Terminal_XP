using System;
using System.IO;
using System.Linq;
using System.Windows;
using Newtonsoft.Json;
using Terminal_XP.Classes;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Navigation;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using Path = System.IO.Path;

namespace Terminal_XP.Frames
{
    public enum IconType { Default, Folder, Image, Text, Audio, Video }
    
    //  🖹🖻🖺

    public partial class LoadingPage : Page
    {
        private const string AccessFileToReadDisk = "READ_DISK.txt";
        private const string PrevDirText = "..";
        private const string SystemFolder = "System Volume Information";
        private const string ExtensionConfig = ".config";
        private readonly Dictionary<IconType, string> Icons;
        
        private string _theme;
        private int _deepOfPath;
        private int _selectedIndex;
        private KeyStates _prevkeyState;
        
        private Dictionary<string, ListBoxItem> _disks = new Dictionary<string, ListBoxItem>();

        private static NavigationService _NavigationService { get; } = (Application.Current.MainWindow as MainWindow)?.Frame.NavigationService;

        public LoadingPage(string theme)
        {
            InitializeComponent();
            
            // Add actions to devices
            DevicesManager.AddDisk += disk => AddDisk(disk);
            DevicesManager.RemoveDisk += RemoveDisk;
            
            _theme = theme;

            KeepAlive = true;
            
            LB.SelectionMode = SelectionMode.Single;
            LB.SelectedIndex = 0;
            LB.FocusVisualStyle = null;
            LB.Focus();

            KeyDown += AdditionalKeys;
            
            Icons = new Dictionary<IconType, string>()
            {
                { IconType.Default, Path.GetFullPath(Addition.Themes + theme +  $@"/{Addition.Icons}/default.png") },
                { IconType.Folder, Path.GetFullPath(Addition.Themes + theme +  $@"/{Addition.Icons}/folder.png") },
                { IconType.Image, Path.GetFullPath(Addition.Themes + theme +  $@"/{Addition.Icons}/image.png") },
                { IconType.Text, Path.GetFullPath(Addition.Themes + theme +  $@"/{Addition.Icons}/text.png") },
                { IconType.Audio, Path.GetFullPath(Addition.Themes + theme +  $@"/{Addition.Icons}/audio.png") },
                { IconType.Video, Path.GetFullPath(Addition.Themes + theme +  $@"/{Addition.Icons}/video.png") }
            };

            DevicesManager.StartListening();
        }

        private void AddDisk(string disk, bool addToList = true)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                try
                {
                    if (!addToList)
                    {
                        LB.Items.Add(_disks[disk]);
                        return;
                    }
                    
                    var allFiles = Directory.GetFiles(disk).Select(Path.GetFileName).ToArray();
                    
                    if (!allFiles.Contains(AccessFileToReadDisk)) return;

                    var fullPath = File.ReadAllText(disk + AccessFileToReadDisk);
                    var diskName = Path.GetFileNameWithoutExtension(fullPath);
                
                    var lbi = new ListBoxItem()
                    {
                        DataContext = new BitmapImage(new Uri(Icons[IconType.Folder])),
                        Content = diskName,
                        Tag = fullPath,
                        Style = (Style)Resources["ImageText"],
                    };

                    if (_deepOfPath == 0)
                        LB.Items.Add(lbi);
                    
                    _disks.Add(disk, lbi);
                }
                catch { }
            }));
        }
        
        private void RemoveDisk(string text)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                LB.Items.Remove(_disks.Pop(text));
            }));
        }
        
        private void lstB_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var lbi = (ListBoxItem) LB.SelectedItem;
            var directory = lbi.Tag.ToString();
            
            if (directory.EndsWith(PrevDirText))
            {
                if (_deepOfPath == 0) return;
                _deepOfPath--;

                if (_deepOfPath > 0)
                {
                    OpenFolder(directory.RemoveLast("\\").RemoveLast("\\"));
                    return;
                }

                LB.Items.Clear();
                LB.SelectedIndex = 0;
                _selectedIndex = 0;
                _disks.Keys.ForEach(x => AddDisk(x, false));
                return;
            }

            if (IsFolder(directory))
            {
                _deepOfPath++;
                OpenFolder(directory);
            }
            else
            {
                ExecuteFile(directory);
            }
        }
        
        private void OpenFolder(string directory)
        {
            FindFolders(directory);
            FindFiles(directory);
            
            LB.SelectedIndex = 0;
            _selectedIndex = 0;
            LB.Focus();
        }
        
        // Look for all files in directory
        private void FindFiles(string directory)
        {
            var files = Directory.GetFiles(directory).ToList();

            for (var i = 0; i < files.Count; i++)
            {
                if (files[i].Contains(ExtensionConfig) && files.Contains(files[i].RemoveLast(ExtensionConfig)))
                {
                    files.RemoveAt(i);
                    i--;
                }
            }
            
            foreach (var file in files)
            {
                var filename = Path.GetFileName(file);
                var name = Path.GetFileNameWithoutExtension(file);
                var extension = Path.GetExtension(file).Remove(0, 1);

                var lbi = new ListBoxItem()
                {
                    Content = name,
                    Tag = $@"{directory}\{filename}",
                    Style = (Style)Resources["ImageText"]
                };

                if (Addition.Text.Contains(extension))
                    lbi.DataContext = new BitmapImage(new Uri(Icons[IconType.Text]));
                else if (Addition.Image.Contains(extension))
                    lbi.DataContext = new BitmapImage(new Uri(Icons[IconType.Image]));
                else if (Addition.Audio.Contains(extension))
                    lbi.DataContext = new BitmapImage(new Uri(Icons[IconType.Audio]));
                else if (Addition.Video.Contains(extension))
                    lbi.DataContext = new BitmapImage(new Uri(Icons[IconType.Video]));
                else
                    lbi.DataContext = new BitmapImage(new Uri(Icons[IconType.Default]));
                
                LB.Items.Add(lbi);
            }
        }
        
        // Look for all folders in directory
        private void FindFolders(string directory)
        {
            LB.Items.Clear();
            var allDirectories = Directory.GetDirectories(directory);

            if (_deepOfPath > 0)
            {
                var lbi = new ListBoxItem()
                {
                    DataContext = new BitmapImage(new Uri(Icons[IconType.Folder])),
                    Content = PrevDirText,
                    Tag = $@"{directory}\{PrevDirText}",
                    Style = (Style)Resources["ImageText"]
                };
                
                LB.Items.Add(lbi);
            }
            
            foreach (var dir in allDirectories)
            {
                var name = Path.GetFileNameWithoutExtension(dir);
                if (name == SystemFolder) continue;

                var lbi = new ListBoxItem()
                {
                    DataContext = new BitmapImage(new Uri(Icons[IconType.Folder])),
                    Content = name,
                    Tag = $@"{directory}\{name}",
                    Style = (Style)Resources["ImageText"]
                };

                LB.Items.Add(lbi);
            }
        }
        
        // All additional keys, which cant be used in System hotkeys
        private void AdditionalKeys(object sender, KeyEventArgs e)
        {
            if (_prevkeyState == e.KeyStates) return;

            switch (e.Key)
            {
                case Key.Enter:
                    lstB_MouseDoubleClick(null, null);
                    break;
            }
            
            
            _prevkeyState = e.KeyStates;
        }
        
        private void OpenFile(string directory, bool hachResult)
        {
            if (hachResult)
            {
                _NavigationService.GoBack();
                _NavigationService.Navigate(Addition.GetPageByFilename(directory, _theme));
            }
            else
            {
                _NavigationService.GoBack();
            }
        }
        
        private void StartHacking(string directory)
        {
            var hp = new HackPage(_theme);
            _NavigationService.Navigate(hp);
            hp.SuccessfullyHacking += result => OpenFile(directory, result);
        }
        
        private void ExecuteFile(string directory)
        {
            if (Directory.GetFiles(directory.RemoveLast(@"\")).Contains(directory + ".config"))
            {
                var content = JsonConvert.DeserializeObject<ConfigDeserializer>(File.ReadAllText(directory + ".config"));
                
                if (!content.HasPassword)
                {
                    _NavigationService.Navigate(Addition.GetPageByFilename(directory, _theme));
                }
                else
                {
                    var lp = new LoginPage(_theme, content.LoginsAndPasswords);

                    _NavigationService.Navigate(lp);
                    lp.LogingIn += result => OpenFile(directory, result);
                    lp.StartHuch += () => StartHacking(directory);
                }
            }
            else
            {
                var nextPage = Addition.GetPageByFilename(directory, _theme);
                
                if (nextPage != default)
                    _NavigationService.Navigate(nextPage);
            }
            
            LB.SelectedIndex = _selectedIndex;
            LB.Focus();
        }

        private static bool IsFolder(string path) => !(path.Contains("."));
    }
}
