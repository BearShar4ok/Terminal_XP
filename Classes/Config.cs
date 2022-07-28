namespace Terminal_XP.Classes
{
    public class Config
    {
        public string Theme { get; set; }
        public ushort FontSize { get; set; }
        public float Opacity { get; set; }
        public string TerminalColor { get; set; }
        public bool UsingDelayFastOutput { get; set; }
        public uint DelayFastOutput { get; set; }
        public uint DelayUpdateCarriage { get; set; }
        public string SpecialSymbol { get; set; }
        public string[] WordsForHacking { get; set; }
        public uint RatioSpawnWords { get; set; }
        public uint LengthHackString { get; set; }
        public uint CountLivesForHacking { get; set; }
        public bool DifficultyInfo { get; set; }
    }
}