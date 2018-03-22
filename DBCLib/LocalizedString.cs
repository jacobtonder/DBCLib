namespace DBCLib
{
    public class LocalizedString
    {
        // Size of Localized Strings
        public static readonly uint Size = 16;
        public string String { get; set; }

        public LocalizedString(string s)
        {
            String = s;
        }

        public static implicit operator LocalizedString(string s)
        {
            return new LocalizedString(s);
        }

        public static implicit operator string(LocalizedString s)
        {
            return s.String;
        }
    }
}
