namespace Terminal_XP.Classes
{
    public struct FragmentText
    {
        public string Text;
        public uint Delay;

        public FragmentText(string text, uint delay = 0)
        {
            Text = text;
            Delay = delay;
        }
    }
}