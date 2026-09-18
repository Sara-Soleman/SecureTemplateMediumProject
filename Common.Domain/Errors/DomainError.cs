using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Domain.Errors
{
    /// <summary>
    /// Represents a structured domain-level error or failure in the application.
    /// It implements 'IDomainError' and uses the C# record feature for clean, immutable data holding.
    /// </summary>
    public record DomainError : IDomainError
    {
        // --- Static Factory Methods ---
        // These methods make it easy to create specific types of errors quickly 
        // while providing sensible default messages if none are supplied.

        // Used when something clashes with existing data (e.g., trying to register an email that already exists).
        public static DomainError Conflict(string? message = "The data provided conflicts with existing data.") =>
            new(message ?? "The data provided conflicts with existing data.", ErrorType.Conflict);

        // Used when an item or record cannot be found in the database.
        public static DomainError NotFound(string? message = "The requested item could not be found.") =>
            new(message ?? "The requested item could not be found.", ErrorType.NotFound);

        // Used when client input is malformed or invalid.
        public static DomainError BadRequest(string? message = "Invalid request or parameters.") =>
            new(message ?? "Invalid request or parameters.", ErrorType.BadRequest);

        // Used for validation failures, optionally holding a list of multiple specific error messages.
        public static DomainError Validation(string? message = "Validation Failed.", List<string>? errors = null) =>
            new(message ?? "Validation Failed.", ErrorType.Validation, errors);

        // Used as a fallback catch-all for unexpected system errors.
        public static DomainError UnExpected(string? message = "Unexpected error happened.") =>
            new(message ?? "Something when wrong.", ErrorType.Unexpected);


        public static DomainError EmailOrUsernameAlreadyExists(string? message = "The Email Or Username Already Exists.") =>
            new(message ?? "The Email Or Username Already Exists.", ErrorType.NotFound);

        public static DomainError InvalidCredentials(string? message = "Invalid Credentials.") =>
            new(message ?? "Invalid Credentials.", ErrorType.NotFound);
        public static DomainError AccountIsInactive(string? message = "Account Is Inactive.") =>
            new(message ?? "Account Is Inactive.", ErrorType.NotFound);
        public static DomainError InvalidOrExpiredPasswordResetToken(string? message = "Invalid Or Expired Password Reset Token.") =>
            new(message ?? "Invalid Or Expired Password Reset Token.", ErrorType.NotFound);
        public static DomainError InvalidRefreshToken(string? message = "Invalid Refresh Token.") =>
            new(message ?? "Invalid Refresh Token.", ErrorType.NotFound);
        public static DomainError SecurityAlertTokenReuseDetected(string? message = "Token reuse detected.") =>
            new(message ?? "Token reuse detected.", ErrorType.NotFound);
        public static DomainError UserNotFound(string? message = "User not found.") =>
            new(message ?? "User not found.", ErrorType.NotFound);
        public static DomainError InvalidCurrentPassword(string? message = "Invalid Current Password.") =>
            new(message ?? "Invalid Current Password.", ErrorType.NotFound);
        public static DomainError InvalidMfaCode(string? message = "Invalid MFA Code.") =>
            new(message ?? "Invalid MFA Code.", ErrorType.NotFound);
        public static DomainError SessionNotFound(string? message = "Session Not Found.") =>
            new(message ?? "Session Not Found.", ErrorType.NotFound);


        // --- Constructor ---
        // The constructor is marked 'private' so developers are forced to use the 
        // clean static factory methods above rather than typing 'new DomainError(...)' directly.
        private DomainError(string? message, ErrorType errorType, List<string>? errors = null)
        {
            ErrorMessage = message;
            ErrorType = errorType;
            // If no list of errors is provided, initialize a fresh, empty list to prevent null reference errors.
            Errors = errors ?? new List<string>();
        }

        // --- Properties ---
        // Using 'init' instead of 'set' means these properties can only be assigned when the object is created, 
        // making the error object completely immutable (read-only) afterward.

        // A descriptive message explaining what went wrong.
        public string? ErrorMessage { get; init; }

        // The category of the error (e.g., NotFound, Conflict, Validation).
        public ErrorType ErrorType { get; init; }

        // An optional list of sub-errors (commonly used for holding multiple validation rule failures at once).
        public List<string>? Errors { get; init; }
    }
}
