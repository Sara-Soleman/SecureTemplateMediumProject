using Ardalis.SmartEnum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Domain.Errors
{
    /// <summary>
    /// A Smart Enum representing different categories of errors in the application. 
    /// Using Ardalis.SmartEnum allows enums to behave like full-featured classes, 
    /// supporting inheritance, encapsulation, and rich behavior instead of just basic numbers.
    /// </summary>
    public abstract class ErrorType(string name, int value) : SmartEnum<ErrorType>(name, value)
    {
        // Public static fields representing the available error types throughout the application.
        public static readonly ErrorType Conflict = new ConflictEnum();
        public static readonly ErrorType NotFound = new NotFoundEnum();
        public static readonly ErrorType BadRequest = new BadRequestEnum();
        public static readonly ErrorType Validation = new ValidationEnum();
        public static readonly ErrorType Unexpected = new UnexpectedEnum();
        public static readonly ErrorType Unauthorized = new UnauthorizedEnum();
        public static readonly ErrorType Forbidden = new ForbiddenEnum();

        //Identity
        #region  Identity Errors
        public static readonly ErrorType Password = new Passworderror();
        public static readonly ErrorType Mfa = new Mfaerror();
        public static readonly ErrorType Credential = new CredentialError();
        public static readonly ErrorType Token = new TokenError();
        public static readonly ErrorType Session = new SessionError();

        #endregion

        #region Role Errors
        public static readonly ErrorType Role = new RoleError();
        #endregion
        // --- Nested Classes ---
        // Each specific error type is defined as a private nested class 
        // that inherits from ErrorType and passes its unique string name and integer value to the base constructor.

        private class ConflictEnum : ErrorType
        {
            public ConflictEnum() : base("Conflict", 0) { }
        }

        private class NotFoundEnum : ErrorType
        {
            public NotFoundEnum() : base("NotFound", 1) { }
        }

        private class BadRequestEnum : ErrorType
        {
            public BadRequestEnum() : base("BadRequest", 2) { }
        }

        private class ValidationEnum : ErrorType
        {
            public ValidationEnum() : base("Validation", 3) { }
        }

        private class UnexpectedEnum : ErrorType
        {
            public UnexpectedEnum() : base("Unexpected", 4) { }
        }
        private class UnauthorizedEnum : ErrorType
        {
            public UnauthorizedEnum() : base("Unauthorized", 11) { }
        }
        private class ForbiddenEnum : ErrorType
        {
            public ForbiddenEnum() : base("Forbidden", 12) { }
        }
        private class Passworderror : ErrorType
        {
            public Passworderror() : base("Password error", 5) { }
        }
        private class Mfaerror : ErrorType
        {
            public Mfaerror() : base("MFA error", 6) { }
        }
        private class CredentialError : ErrorType
        {
            public CredentialError() : base("Credential error", 7) { }
        }
        private class TokenError : ErrorType
        {
            public TokenError() : base("Token error", 8) { }
        }
        private class SessionError : ErrorType
        {
            public SessionError() : base("Session error", 9) { }
        }
        private class RoleError : ErrorType
        {
            public RoleError() : base("Role error", 10) { }
        }
    }
}
