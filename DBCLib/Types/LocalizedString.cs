namespace DBCLib
{
    public class LocalizedString
    {
        // Size of Localized Strings
        public static readonly int Size = 17;
        public string String { get; set; }

        public LocalizedString(string str)
        {
            String = str;
        }

        public static implicit operator LocalizedString(string str)
        {
            return new LocalizedString(str);
        }

        public static implicit operator string(LocalizedString str)
        {
            return str.String;
        }
    }
}
