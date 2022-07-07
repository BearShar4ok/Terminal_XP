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
using Terminal.Classes;
using System.Windows.Controls.Primitives;

namespace Terminal.Pages
{
    /// <summary>
    /// Логика взаимодействия для LoadingPage.xaml
    /// </summary>
    public partial class LoadingPage : Page
    {
        private string directory = "";
        private bool isFlashCard = true;
        private int deepOfPath = 0;

        //📂🖹🖻🖺🖾 🖼
        private string passFoler = @"G:\Coding\MyProjects\Terminal_XP\Terminal_XP\Assets\Themes\Fallout\folder.png";
        private string passImage =  @"G:\Coding\MyProjects\Terminal_XP\Terminal_XP\Assets\Themes\Fallout\image.png";
        private string passText = @"G:\Coding\MyProjects\Terminal_XP\Terminal_XP\Assets\Themes\Fallout\text.png";
        public LoadingPage()
        {
            InitializeComponent();
            // G:/Coding/MyProjects/Terminal/Resources
            // ../../../Resources
            DevicesManager.AddDisk += Add;
            DevicesManager.RemoveDisk += Rem;

            //LoadingPage l = new LoadingPage();

            lstB.SelectionMode = SelectionMode.Single;
            lstB.ContextMenu = new ContextMenu();

            lstB.SelectedIndex = 0;

            lstB.PreviewKeyDown += AdditionalKeys;

            DevicesManager.StartLisining();
        }
        private void Add(string text)
        {
            // Log.Logger.Information("add: {0}", text);
            System.Diagnostics.Debug.WriteLine("add: " + text);

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => lstB.Items.Add(new ListBoxItem()
            {
                Tag = new PropertyPath(passFoler),
                Content = text,
                Style = (Style)Resources["ImageText"],
            })));
        }
        private void Rem(string text)
        {
            System.Diagnostics.Debug.WriteLine("remove: " + text);
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => lstB.Items.Remove(text)));
        }

        private void lstB_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //MessageBox.Show(lstB.SelectedItem.ToString());
            
            string path = (string)(((ListBoxItem)(lstB.SelectedItem)).Content);
           

            if (deepOfPath==0)
            {
                directory = path;
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
                        StreamReader s = new StreamReader(directory + "file.txt");
                        directory = s.ReadLine();
                        deepOfPath++;
                        OpenFolder();
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
            }
            else if (deepOfPath >=1)
            {
                directory += "\\" + path;
                deepOfPath++;
                OpenFolder();
            }
            

            //MessageBox.Show(directory);
        }
        private void OpenFolder()
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
                    Style = (Style)Resources["ImageText"],
                };
                Image im = new Image();
                
                switch (format)
                {
                    case "txt":
                        im.Source = new BitmapImage(new Uri(passText, UriKind.Relative));
                        lstBI.Tag = passText;
                        break;
                    case "png":
                        lstBI.Tag = new Uri(passImage);
                        break;
                    case "jpg":
                        lstBI.Tag = new Uri(passImage);
                        break;
                    case "bmp":
                        lstBI.Tag = new Uri(passImage);
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
                    Tag = new PropertyPath( passFoler),
                    Content = text,
                    Style = (Style)Resources["ImageText"],
                };

                lstB.Items.Add(lstBI);
            }
            lstB.SelectedIndex = 0;
        }
        private void AdditionalKeys(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    lstB_MouseDoubleClick(null, null);
                    break;
                case Key.Escape:
                    //lstB_MouseDoubleClick(null, null); //       e/adsadad/asdsadas/
                    //directory.Split("/")
                    //MessageBox.Show(directory);
                    deepOfPath--;
                    if (deepOfPath==0)
                    {
                        DevicesManager.ClearAllDisks();
                        lstB.Items.Clear();
                        return;
                    }
                    directory = directory.Remove(directory.LastIndexOf("\\"));
                    directory = directory.Remove(directory.LastIndexOf("\\"));
                    directory += "\\";

                    OpenFolder();
                    break;
                default:
                    break;
            }
        }
    }
}
