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
using Terminal_XP.Windows;
using System.Threading;
using System.Windows.Media;

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
        private string _currDisk;

        private Dictionary<string, ListBoxItem> _disks = new Dictionary<string, ListBoxItem>();


        public LoadingPage(string theme)
        {
            InitializeComponent();

            // Add actions to devices
            DevicesManager.AddDisk += disk => AddDisk(disk);
            DevicesManager.RemoveDisk += RemoveDisk;

            _theme = theme;

            KeepAlive = true;

            LblInfo.Content = "Доступных дисков нет...";

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
            LoadParams();
            LoadTheme();
            DevicesManager.StartListening();
        }
        private void LoadTheme()
        {
            LblInfo.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), Addition.Themes + _theme + "/#" + ConfigManager.Config.FontName);
        }

        private void LoadParams()
        {
            LblInfo.FontSize = ConfigManager.Config.FontSize;
            LblInfo.Opacity = ConfigManager.Config.Opacity;
            LblInfo.Foreground = (Brush)new BrushConverter().ConvertFromString(ConfigManager.Config.TerminalColor);
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
                    if (Directory.Exists(fullPath))
                    {
                        LblInfo.Content = "";
                        LblInfo.Visibility = Visibility.Hidden;
                        var diskName = Path.GetFileNameWithoutExtension(fullPath);

                        var lbi = new ListBoxItem()
                        {
                            DataContext = new BitmapImage(new Uri(Icons[IconType.Folder])),
                            Content = diskName,
                            Tag = fullPath,
                            Style = (Style)App.Current.FindResource("ImageText"),
                            Foreground = (Brush)new BrushConverter().ConvertFrom(ConfigManager.Config.TerminalColor),
                            FontFamily = LblInfo.FontFamily,
                            FontSize = LblInfo.FontSize,

                        };

                        if (_deepOfPath == 0)
                            LB.Items.Add(lbi);

                        _disks.Add(disk, lbi);
                    }
                    Focus();
                }
                catch { }
            }));
        }

        private void RemoveDisk(string diskName)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                var disk = _disks.Pop(diskName);

                if (LB.Items.Contains(disk))
                    LB.Items.Remove(disk);

                if (_currDisk == diskName)
                {
                    LB.SelectedIndex = 0;
                    _selectedIndex = 0;
                    _currDisk = null;
                    LB.Items.Clear();
                    _disks.Keys.ForEach(x => AddDisk(x, false));
                }
                if (LB.Items.Count == 0)
                {
                    LblInfo.Content = "Доступных дисков нет...";
                    LblInfo.Visibility = Visibility.Visible;
                }
            }));
        }

        private void lstB_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var lbi = (ListBoxItem)LB.SelectedItem;
            if (lbi == null)
                return;

            var directory = lbi.Tag.ToString();

            if (_currDisk == null)
                _currDisk = _disks.FindKey(lbi);

            if (directory.EndsWith(PrevDirText))
            {
                if (_deepOfPath == 0) return;
                _deepOfPath--;

                if (_deepOfPath > 0)
                {
                    OpenFolder(directory.RemoveLast("\\").RemoveLast("\\"));
                    return;
                }

                LB.SelectedIndex = 0;
                _selectedIndex = 0;
                _currDisk = null;
                LB.Items.Clear();
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

        // Look for all files in directory
        private void FindFiles(string directory)
        {
            var files = Directory.GetFiles(directory).ToList();
            var directories = Directory.GetDirectories(directory);

            for (var i = 0; i < files.Count; i++)
            {
                if (files[i].Contains(ExtensionConfig) && (files.Contains(files[i].RemoveLast(ExtensionConfig)) || directories.Contains(files[i].RemoveLast(ExtensionConfig))))// || files.Contains(directories[i].RemoveLast(ExtensionConfig))))
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
                    Style = (Style)App.Current.FindResource("ImageText"),
                    Foreground = (Brush)new BrushConverter().ConvertFrom(ConfigManager.Config.TerminalColor),
                    FontFamily = LblInfo.FontFamily,
                    FontSize = LblInfo.FontSize,

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
                    Style = (Style)App.Current.FindResource("ImageText"),
                    Foreground = (Brush)new BrushConverter().ConvertFrom(ConfigManager.Config.TerminalColor),
                    FontFamily = LblInfo.FontFamily,
                    FontSize = LblInfo.FontSize,

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
                    Style = (Style)App.Current.FindResource("ImageText"),
                    Foreground = (Brush)new BrushConverter().ConvertFrom(ConfigManager.Config.TerminalColor),
                    FontFamily = LblInfo.FontFamily,
                    FontSize = LblInfo.FontSize,

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

        private void GoToFilePage(string directory)
        {
            var nextPage = Addition.GetPageByFilename(directory, _theme);

            if (nextPage != default)
                Addition.NavigationService.Navigate(nextPage);
        }
        private void OpenFolder(string directory)
        {
            if (Directory.GetFiles(directory.RemoveLast(@"\")).Contains(directory + ".config"))
            {
                try
                {
                    var content = JsonConvert.DeserializeObject<ConfigDeserializer>(File.ReadAllText(directory + ".config"));

                    if (!content.HasPassword)
                    {
                        AccessInFolderOpen(directory);
                    }
                    else
                    {
                        //Addition.NavigationService.Navigate(new LoginPage(directory, _theme, content.LoginsAndPasswords));
                        var lw = new LoginWindow(_theme, content.LoginsAndPasswords);
                        if (lw.ShowDialog() == false)
                        {
                            if (lw.ReternedState == State.Access)
                                AccessInFolderOpen(directory);

                            if (lw.ReternedState == State.Huck)
                            {
                                var hw = new HuckWindow(_theme, lw.Password);
                                if (hw.ShowDialog() == false)
                                {
                                    if (hw.ReternedState == State.Access)
                                        AccessInFolderOpen(directory);

                                }
                            }
                        }
                    }
                }
                catch
                {
                    AccessInFolderOpen(directory);
                }
            }
            else
            {
                AccessInFolderOpen(directory);
            }
        }

        private void AccessInFolderOpen(string directory)
        {
            FindFolders(directory);
            FindFiles(directory);

            LB.SelectedIndex = 0;
            _selectedIndex = 0;
            LB.Focus();
        }

        private void ExecuteFile(string directory)
        {
            if (Directory.GetFiles(directory.RemoveLast(@"\")).Contains(directory + ".config"))
            {
                try
                {
                    var content = JsonConvert.DeserializeObject<ConfigDeserializer>(File.ReadAllText(directory + ".config"));

                    if (!content.HasPassword)
                    {
                        Addition.NavigationService.Navigate(Addition.GetPageByFilename(directory, _theme));
                    }
                    else
                    {
                        //Addition.NavigationService.Navigate(new LoginPage(directory, _theme, content.LoginsAndPasswords));
                        var lw = new LoginWindow(_theme, content.LoginsAndPasswords);
                        if (lw.ShowDialog() == false)
                        {
                            if (lw.ReternedState == State.Access)
                                Addition.NavigationService.Navigate(Addition.GetPageByFilename(directory, _theme));

                            if (lw.ReternedState == State.Huck)
                            {
                                var hw = new HuckWindow(_theme, lw.Password);
                                if (hw.ShowDialog() == false)
                                {
                                    if (hw.ReternedState == State.Access)
                                        Addition.NavigationService.Navigate(Addition.GetPageByFilename(directory, _theme));

                                }
                            }
                        }
                    }
                }
                catch
                {
                    GoToFilePage(directory);
                }
            }
            else
            {
                // TODO: Generate config file for this file
                GoToFilePage(directory);
            }
        }

        private static bool IsFolder(string path) => Directory.Exists(path);
    }
}
