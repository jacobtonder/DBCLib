using System;
using System.Runtime.Serialization;

namespace DBCLib.Exceptions
{
    /// <summary>
    /// Exception thrown when the DBC file is not loaded.
    /// </summary>
    [Serializable]
    public class DBCFileNotLoadedException : Exception
    {
        internal DBCFileNotLoadedException()
        {
        }

        internal DBCFileNotLoadedException(string message) : base(message)
        {
        }

        internal DBCFileNotLoadedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}