using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terminal_XP.Classes
{
    internal class ConfigDeserializer
    {
        public int HuckAttempts { get; set; } = 10;
        public bool CanBeHucked { get; set; } = true;
        public bool CanBeChanged { get; set; } = false;
        public bool CanBeCopied { get; set; } = false;
        public bool CanBeDeleted { get; set; } = false;
        public bool HasPassword { get; set; } = false;
        public Dictionary<string,string> LoginsAndPasswords { get; set; } = new Dictionary<string,string>();
    }
}
