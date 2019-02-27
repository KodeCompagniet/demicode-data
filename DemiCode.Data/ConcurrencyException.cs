using System;

namespace DemiCode.Data
{
    /// <summary>
    /// Thrown when a concurrency conflict is detected when data is updated or created.
    /// </summary>
    public class ConcurrencyException : Exception
    {
        public ConcurrencyException() : this((Exception) null)
        {
        }

        public ConcurrencyException(string message) : base(message)
        {
        }

        public ConcurrencyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ConcurrencyException(Exception innerException) : base("ConcurrencyException have occured", innerException)
        {
        }

         
    }
}