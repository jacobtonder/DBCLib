using System;

namespace DBCLib.Exceptions
{
    /// <summary>
    /// Exception thrown when the field count of the specified DBC file does not match the field count of the specified DBC type.
    /// </summary>
    [Serializable]
    public class InvalidDBCFields : Exception
    {
        internal InvalidDBCFields()
        {
        }

        internal InvalidDBCFields(string message) : base(message)
        {
        }

        internal InvalidDBCFields(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
