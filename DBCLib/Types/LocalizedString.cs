namespace DBCLib
{
    /// <summary>
    /// The purpose of this class is to support reading DBC files with localized strings.
    /// </summary>
    public class LocalizedString
    {
        internal LocalizedString(string str) => String = str;

        /// <summary>
        /// Size of the localized string.
        /// </summary>
        public static readonly int Size = 17;
        /// <summary>
        /// The value of the localized string for the specific dbc local flag.
        /// </summary>
        public string String { get; set; }

        /// <summary>
        /// Used for implicit conversion from string to localized string.
        /// </summary>
        /// <param name="str">The specified string to convert to localized sting.</param>
        public static implicit operator LocalizedString(string str) => new(str);
        /// <summary>
        /// Used for implicit conversion from localized string to string.
        /// </summary>
        /// <param name="str">The specified localized string to convert to sting.</param>
        public static implicit operator string(LocalizedString str) => str.String;
    }
}
