namespace DBCLib
{
    public class LocalizedString
    {
        public LocalizedString(string str) => String = str;

        // Size of Localized Strings
        public static readonly int Size = 17;
        public string String { get; set; }

        public static implicit operator LocalizedString(string str) => new(str);
        public static implicit operator string(LocalizedString str) => str.String;
    }
}
