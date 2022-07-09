using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace Terminal_XP.Classes
{
    public static class DevicesManager
    {
        public static event Action<string> AddDisk;
        public static event Action<string> RemoveDisk;
        public static Action ClearAllDisks;

        private static List<string> disks = new List<string>();

        private static bool isActive;

        private const int Delay = 1000;
        private static Thread thread;
        
        public static void StartLisining()
        {
            isActive = true;
            ClearAllDisks += DisksClear;
            thread = new Thread(Update);
            thread.Start();
        }

        public static void StopLisining()
        {
            isActive = false;
        }
        private static void DisksClear()
        {
            disks.Clear();
        }

        private static void Update()
        {
            try
            {
                while (isActive)
            {
                var drives = DriveInfo.GetDrives();
                    var tempDisks = new List<string>();

                foreach (var disk in drives)
                {
                        tempDisks.Add(disk.Name);

                        if (!disks.Contains(disk.Name))
                        {
                            if (AddDisk != null)
                            {
                                AddDisk.Invoke(disk.Name);
                }
                            disks.Add(disk.Name);
                        }
                    }

                    for (int i = 0; i < disks.Count; i++)
                    {
                        if (!tempDisks.Contains(disks[i]))
                        {
                            if (RemoveDisk!=null)
                {
                                RemoveDisk.Invoke(disks[i]);
                            }
                            disks.RemoveAt(i);
                    i--;
                }
                    }

                Thread.Sleep(Delay);
            }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            
        }
    }
}