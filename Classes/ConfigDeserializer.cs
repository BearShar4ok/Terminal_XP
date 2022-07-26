using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terminal_XP.Classes
{
    internal class ConfigDeserializer
    {
        public bool CanBeCopied { get; set; } = false;
        public bool CanBeDeleted { get; set; } = false;
        public bool HasPassword { get; set; } = false;
        public List<MyTuple> LoginsAndPasswords { get; set; } = new List<MyTuple>();
    }
    public class MyTuple
    {
        public string Log { get; set; } = "";
        public string Pass { get; set; } = "";
    }
}
