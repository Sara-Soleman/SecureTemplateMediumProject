using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Core.Exceptions
{

    /// <summary>
    /// A custom base exception class for application-level errors. 
    /// [Serializable] marks this class so it can be serialized, which is traditionally used 
    /// in .NET for cross-process communication or legacy exception remoting.
    /// </summary>
    [Serializable]
    public class myApplicationException : Exception
    {
        // Default constructor for throwing the exception without a specific message.
        public myApplicationException() { }

        // Constructor to initialize the exception with a descriptive error message.
        public myApplicationException(string message) : base(message) { }

        // Constructor used when wrapping an underlying lower-level exception (inner exception) with a custom message.
        public myApplicationException(string message, Exception inner) : base(message, inner) { }

        // --- Serialization Constructor ---
        // Required for [Serializable] exceptions in traditional .NET to properly 
        // pack and unpack exception data when streaming or serializing across boundaries.
        protected myApplicationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
