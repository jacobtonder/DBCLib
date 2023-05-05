using System;

namespace DBCLib.Exceptions
{
    /// <summary>
    /// Exception thrown when the signature of the specified DBC file does not match the signature of the specified DBC instance.
    /// </summary>
    [Serializable]
    public class InvalidSignatureException : Exception
    {
        internal InvalidSignatureException()
        {
        }

        internal InvalidSignatureException(string message) : base(message)
        {
        }

        internal InvalidSignatureException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
