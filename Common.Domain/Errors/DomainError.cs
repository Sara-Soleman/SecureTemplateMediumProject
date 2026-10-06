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
        public static DomainError Conflict(string? message = "Conflict") =>
            new(message ?? "Conflict", ErrorType.Conflict);

        // Used when an item or record cannot be found in the database.
        public static DomainError NotFound(string? message = "NotFound") =>
            new(message ?? "NotFound", ErrorType.NotFound);

        // Used when client input is malformed or invalid.
        public static DomainError BadRequest(string? message = "BadRequest") =>
            new(message ?? "BadRequest", ErrorType.BadRequest);

        // Used for validation failures, optionally holding a list of multiple specific error messages.
        public static DomainError Validation(string? message = "Validation", List<string>? errors = null) =>
            new(message ?? "Validation", ErrorType.Validation, errors);

        // Used as a fallback catch-all for unexpected system errors.
        public static DomainError UnExpected(string? message = "UnExpected") =>
            new(message ?? "UnExpected", ErrorType.Unexpected);
        public static DomainError Unauthorized(string? message = "Unauthorized") =>
            new(message ?? "Unauthorized", ErrorType.Unexpected);

        #region Identity Errors
        public static DomainError EmailOrUsernameAlreadyExists(string? message = "EmailOrUsernameAlreadyExists") =>
     new(message ?? "EmailOrUsernameAlreadyExists", ErrorType.Credential);

        public static DomainError InvalidCredentials(string? message = "InvalidCredentials") =>
            new(message ?? "InvalidCredentials", ErrorType.Credential);
        public static DomainError AccountIsInactive(string? message = "AccountIsInactive") =>
            new(message ?? "AccountIsInactive", ErrorType.Credential);
        public static DomainError InvalidOrExpiredPasswordResetToken(string? message = "InvalidOrExpiredPasswordResetToken") =>
            new(message ?? "InvalidOrExpiredPasswordResetToken.", ErrorType.Token);
        public static DomainError InvalidRefreshToken(string? message = "InvalidRefreshToken") =>
            new(message ?? "InvalidRefreshToken", ErrorType.Token);
        public static DomainError SecurityAlertTokenReuseDetected(string? message = "SecurityAlertTokenReuseDetected") =>
            new(message ?? "SecurityAlertTokenReuseDetected", ErrorType.Token);
        public static DomainError UserNotFound(string? message = "UserNotFound") =>
            new(message ?? "UserNotFound", ErrorType.Credential);
        public static DomainError InvalidCurrentPassword(string? message = "InvalidCurrentPassword") =>
            new(message ?? "InvalidCurrentPassword", ErrorType.Credential);
        public static DomainError InvalidMfaCode(string? message = "InvalidMfaCode") =>
            new(message ?? "InvalidMfaCode", ErrorType.Mfa);
        public static DomainError SessionNotFound(string? message = "SessionNotFound") =>
            new(message ?? "SessionNotFound", ErrorType.Session);
        #endregion
        public static DomainError RoleAlreadyExists(string? message = "RoleAlreadyExists") =>
            new(message ?? "RoleAlreadyExists", ErrorType.Role);
        public static DomainError RoleNotFound(string? message = "RoleNotFound") =>
            new(message ?? "RoleNotFound", ErrorType.Role);
        public static DomainError UserAlreadyHasRole(string? message = "UserAlreadyHasRole") =>
            new(message ?? "UserAlreadyHasRole", ErrorType.Role);
        public static DomainError UserRoleNotFound(string? message = "UserRoleNotFound") =>
            new(message ?? "UserRoleNotFound", ErrorType.Role);
        #region Role Errors

        #endregion
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
