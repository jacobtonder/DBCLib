using System;

namespace DBCLib.Exceptions
{
    /// <summary>
    /// Exception thrown when the field count of the specified DBC file does not match the field count of the specified DBC type.
    /// </summary>
    [Serializable]
    public class InvalidDBCFieldsException : Exception
    {
        internal InvalidDBCFieldsException()
        {
        }

        internal InvalidDBCFieldsException(string message) : base(message)
        {
        }

        internal InvalidDBCFieldsException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
