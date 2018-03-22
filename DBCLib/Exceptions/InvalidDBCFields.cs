using System;

namespace DBCLib.Exceptions
{
    [Serializable]
    public class InvalidDBCFields : Exception
    {
        public InvalidDBCFields()
        {
        }

        public InvalidDBCFields(string message) : base(message)
        {
        }

        public InvalidDBCFields(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
