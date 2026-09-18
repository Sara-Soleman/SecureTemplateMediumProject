using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Domain.Errors
{
    /// <summary>
    /// Represents a contract for domain-level errors. 
    /// Any class or record implementing this interface promises to provide 
    /// an error message, an error type category, and an optional list of sub-errors.
    /// </summary>
    public interface IDomainError
    {
        // A descriptive message explaining what went wrong. 
        // The 'init' accessor allows it to be set during object creation, making it immutable afterward.
        string? ErrorMessage { get; init; }

        // The category or classification of the error (using your SmartEnum 'ErrorType').
        ErrorType ErrorType { get; init; }

        // An optional list of specific error strings (commonly used to list multiple validation rule failures at once).
        public List<string>? Errors { get; init; }
    }
}
