using System;

namespace DBCLib.Exceptions
{
    [Serializable]
    public class InvalidSignatureException : Exception
    {
        public InvalidSignatureException()
        {
        }

        public InvalidSignatureException(string message) : base(message)
        {
        }

        public InvalidSignatureException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
