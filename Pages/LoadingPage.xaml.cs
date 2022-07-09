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

namespace Terminal_XP.Pages
{
    /// <summary>
    /// Логика взаимодействия для LoadingPage.xaml
    /// </summary>
    public partial class LoadingPage : Page
    {
        private string directory = "";
        private int deepOfPath = 0;
        private KeyStates prevkeyState;
        private Dictionary<string, ListBoxItem> disks = new Dictionary<string, ListBoxItem>();

        //📂🖹🖻🖺🖾 🖼
        private string passFoler = @"G:\Coding\MyProjects\Terminal_XP\Terminal_XP\Assets\Themes\Fallout\folder.png";
        private string passImage = @"G:\Coding\MyProjects\Terminal_XP\Terminal_XP\Assets\Themes\Fallout\image.png";
        private string passText = @"G:\Coding\MyProjects\Terminal_XP\Terminal_XP\Assets\Themes\Fallout\text.png";
        public LoadingPage()
        {
            InitializeComponent();
            DevicesManager.AddDisk += Add;
            DevicesManager.RemoveDisk += Rem;

            lstB.SelectionMode = SelectionMode.Single;
            lstB.ContextMenu = new ContextMenu();

            lstB.SelectedIndex = 0;

            lstB.PreviewKeyDown += AdditionalKeys;

            DevicesManager.StartLisining();
        }
        private void Add(string text)
        {
            System.Diagnostics.Debug.WriteLine("add: " + text);

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
            {


                directory = text;
                string[] allFiles;
                try
                {
                    allFiles = Directory.GetFiles(directory);
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
                        lstB.Items.Add(lbi);
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

                lblDirectory.Content = directory;
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
            lblDirectory.Content = directory;
            if (IsFolder(path))
            {
                if (deepOfPath == 0)
                {
                    directory = (string)(((ListBoxItem)(lstB.SelectedItem)).Tag);
                    OpenFolder();
                    deepOfPath++;
                }
                else if (deepOfPath >= 1)
                {
                    directory = path;
                    deepOfPath++;
                    OpenFolder();
                }
            }
            else
            {

            }
        }
        private void OpenFolder()
        {
            lblDirectory.Content = directory;
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
                Image im = new Image();

                switch (format)
                {
                    case "txt":
                        im.Source = new BitmapImage(new Uri(passText, UriKind.Relative));
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
                    break;
                default:
                    break;
            }
        }
        private bool IsFolder(string text)
        {
            return !(text.Contains("."));
        }
    }
}
