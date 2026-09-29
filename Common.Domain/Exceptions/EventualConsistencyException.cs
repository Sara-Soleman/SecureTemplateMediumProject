using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Domain.Exceptions
{
    /// <summary>
    /// A custom exception thrown when an eventual consistency issue occurs in the application 
    /// (e.g., when a background process or database replica takes longer than expected to sync, 
    /// causing a data mismatch or read failure).
    /// Inherits from the standard C# 'Exception' class.
    /// </summary>
    public class EventualConsistencyException : Exception
    {
        // A unique code representing this specific type of error (useful for logging or API error responses).
        public string ErrorCode { get; }

        // A clear description of the error message.
        public string ErrorMessage { get; }

        // A list of additional details or sub-errors providing context on what failed.
        public List<string> Details { get; }

        // Constructor to initialize the custom exception with an error code, message, and optional details.
        public EventualConsistencyException(string errorCode, string errorMessage, List<string>? details = null)
            : base(message: errorMessage) // Passes the error message up to the base Exception class so standard logging works correctly
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;

            // If no details list is provided, create a fresh empty list to avoid null reference exceptions.
            Details = details ?? new();
        }
    }
}
