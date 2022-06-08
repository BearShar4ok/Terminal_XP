using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace Terminal_XP.Classes
{
    public static class DevicesManager
    {
        public static event Action<string> AddDisk;
        public static event Action<string> RemoveDisk;

        private static List<string> _disks = new List<string>();

        private static bool _isActive;

        private const int Delay = 1000;
        
        public static void StartListening()
        {
            _isActive = true;
            new Thread(Update).Start();
        }

        public static void StopListening()
        {
            _isActive = false;
        }

        private static void Update()
        {
            while (_isActive)
            {
                var drives = DriveInfo.GetDrives();
                var disks = new List<string>();

                foreach (var disk in drives)
                {
                    disks.Add(disk.Name);

                    if (_disks.Contains(disk.Name)) 
                        continue;
                    
                    AddDisk?.Invoke(disk.Name);
                    _disks.Add(disk.Name);
                }

                for (var i = 0; i < _disks.Count; i++)
                {
                    if (disks.Contains(_disks[i])) 
                        continue;
                    
                    RemoveDisk?.Invoke(_disks[i]);
                    _disks.RemoveAt(i);
                    i--;
                }

                Thread.Sleep(Delay);
            }
        }
    }
}