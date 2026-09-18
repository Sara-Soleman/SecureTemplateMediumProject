using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Domain.Exceptions
{
    /// <summary>
    /// A custom exception thrown when data validation rules fail in the application.
    /// [Serializable] marks this class so it can be serialized (converted into a byte stream or format), 
    /// which is sometimes required for legacy remoting, cross-process communication, or specific serialization libraries.
    /// </summary>
    [Serializable]
    public class ValidationException : Exception
    {
        // A read-only list of validation error messages, allowing callers to see all failures at once.
        public IReadOnlyList<string> Errors { get; }

        // Constructor to initialize the exception with a collection of multiple validation errors.
        public ValidationException(IEnumerable<string> errors)
            : base("Validation failed.") // Passes a default error message up to the base Exception class
        {
            // Safely converts the incoming errors into a list, or initializes an empty list if null.
            Errors = errors?.ToList() ?? new List<string>();
        }

        // Constructor to initialize the exception with a single validation error message.
        public ValidationException(string error)
            : base(error) // Uses the provided error string as the main exception message
        {
            // Initializes an empty list since no multi-error collection was provided.
            Errors = new List<string>();
        }

        // Constructor used when wrapping another exception (inner exception) with a custom message.
        public ValidationException(string message, Exception inner)
            : base(message, inner) // Passes both the message and the root cause exception up to the base class
        {
            // Automatically adds the inner exception's message to the errors list for convenience.
            Errors = new List<string>() { inner.Message };
        }

        // --- Serialization Support ---
        // In traditional .NET, custom exceptions that are marked [Serializable] require a special 
        // serialization constructor to properly pack and unpack exception details across boundaries.

        protected ValidationException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
            // Retrieves the stored 'Errors' list from the serialization stream during deserialization.
            Errors = (IReadOnlyList<string>)info.GetValue(nameof(Errors), typeof(IReadOnlyList<string>))
                     ?? new List<string>();
        }

        // Overrides GetObjectData to ensure that our custom 'Errors' list is correctly included 
        // when the exception object is serialized.
        public override void GetObjectData(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Errors), Errors, typeof(IReadOnlyList<string>));
        }
    }
}
