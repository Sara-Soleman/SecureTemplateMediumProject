using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;


using ValidationExp = Common.Domain.Exceptions;

namespace Common.Domain.Extensions
{
    /// <summary>
    /// A collection of extension methods used for defensive programming and validation.
    /// These methods allow you to guard code against invalid data (like nulls, negative numbers, or bad strings) 
    /// and throw descriptive exceptions when rules are broken.
    /// </summary>
    public static class Validators
    {
        // Helper method that creates and returns a standardized ValidationException.
        private static ValidationExp.ValidationException Invalid(string message) => new ValidationExp.ValidationException(message);

        #region Null and Default Checks

        // Ensures a value is strictly null; throws an exception if it contains data.
        public static T? EnsureNull<T>(this T? model, [CallerArgumentExpression("model")] string name = "")
            => model == null ? model : throw Invalid($"{name} must be null.");

        // Ensures a value is NOT null; throws an exception if it is.
        public static T EnsureNonNull<T>(this T? model, [CallerArgumentExpression("model")] string name = "")
            => model ?? throw Invalid($"{name} cannot be null.");

        // Ensures a value type (like an int or Guid) is not its default uninitialized state (e.g., 0 or Guid.Empty).
        public static T EnsureNotDefault<T>(this T model, [CallerArgumentExpression("model")] string name = "") where T : struct
        {
            if (model.Equals(default(T)))
                throw Invalid($"{name} cannot be null or default.");
            return model;
        }

        // Overload for nullable value types (e.g., int?) to check for default values safely.
        public static T EnsureNotDefault<T>(this T? model, [CallerArgumentExpression("model")] string name = "") where T : struct
        {
            var value = model.GetValueOrDefault();
            if (value.Equals(default(T)))
                throw Invalid($"{name} cannot be null or default.");
            return value;
        }

        #endregion

        #region Numeric Checks

        // Ensures a number is not equal to zero.
        public static T EnsureNonZero<T>(this T model, [CallerArgumentExpression("model")] string name = "") where T : struct
            => Convert.ToDecimal(model) != 0 ? model : throw Invalid($"{name} cannot be zero.");

        // Ensures a number is strictly greater than zero.
        public static T EnsurePositive<T>(this T model, [CallerArgumentExpression("model")] string name = "") where T : struct
            => model.EnsureNonZero(name).EnsureNonNegative(name);

        // Ensures a number is zero or greater (non-negative).
        public static T EnsureNonNegative<T>(this T model, [CallerArgumentExpression("model")] string name = "") where T : struct
            => Convert.ToDecimal(model) >= 0 ? model : throw Invalid($"{name} cannot be negative.");

        // Ensures a number is strictly greater than a specified minimum threshold.
        public static T EnsureGreaterThan<T>(this T model, T min, [CallerArgumentExpression("model")] string modelExpression = "", [CallerArgumentExpression("min")] string minExpression = "") where T : struct, IComparable<T>
            => model.CompareTo(min) > 0 ? model : throw Invalid($"{modelExpression}={model} must be greater than {minExpression}={min}.");

        // Ensures a number is greater than or equal to a specified minimum threshold.
        public static T EnsureAtLeast<T>(this T model, T min, [CallerArgumentExpression("model")] string modelExpression = "", [CallerArgumentExpression("min")] string minExpression = "") where T : struct, IComparable<T>
            => model.CompareTo(min) >= 0 ? model : throw Invalid($"{modelExpression}={model} must be greater than or equal to {minExpression}={min}.");

        // Ensures a number falls within a specific range (with options to include or exclude boundaries).
        public static T EnsureWithinRange<T>(this T model, T min, T max, bool excludeMin = false, bool excludeMax = false, [CallerArgumentExpression("model")] string modelExpression = "", [CallerArgumentExpression("min")] string minExpression = "", [CallerArgumentExpression("max")] string maxExpression = "") where T : struct, IComparable<T>
        {
            bool tooLow = excludeMin ? model.CompareTo(min) <= 0 : model.CompareTo(min) < 0;
            bool tooHigh = excludeMax ? model.CompareTo(max) >= 0 : model.CompareTo(max) > 0;

            if (tooLow || tooHigh)
                throw Invalid($"{modelExpression}={model} must be between {minExpression}={min} and {maxExpression}={max}.");

            return model;
        }

        #endregion

        #region String Checks

        // Ensures a string is not null or empty ("").
        public static string EnsureNonEmpty(this string? model, [CallerArgumentExpression("model")] string name = "")
            => !string.IsNullOrEmpty(model) ? model : throw Invalid($"{name} cannot be empty.");

        // Ensures a string is not null, empty, or made up entirely of whitespace.
        public static string EnsureNonBlank(this string? model, [CallerArgumentExpression("model")] string name = "")
            => !string.IsNullOrWhiteSpace(model) ? model : throw Invalid($"{name} cannot be blank.");

        // Ensures a string matches a specific Regular Expression (Regex) pattern.
        public static string EnsureMatchesPattern(this string model, string pattern, [CallerArgumentExpression("model")] string name = "")
            => Regex.IsMatch(model, pattern) ? model : throw Invalid($"{name} does not match the required pattern.");

        // Validates that a string is a properly formatted image URL using a regex check.
        public static string EnsureImageUrl(this string model, [CallerArgumentExpression("model")] string name = "")
            => Regex.IsMatch(model, "^https?:\\/\\/.*\\/.*\\.(png|gif|webp|jpeg|jpg)\\??.*$") ? model : throw Invalid($"{name} is not a valid image url.");

        // Validates that a string is a legitimate email address format using .NET's built-in attribute.
        public static string EnsureValidEmail(this string email, [CallerArgumentExpression("email")] string name = "")
            => new EmailAddressAttribute().IsValid(email) ? email : throw Invalid($"{email} is not a valid email address.");

        // Ensures a string has an exact specified character length.
        public static string EnsureExactLength(this string model, int length, [CallerArgumentExpression("model")] string name = "")
            => model.Length == length ? model : throw Invalid($"{name} must have exactly {length} characters, but it has {model.Length}.");

        // Ensures a string's length falls within a specific minimum and maximum character limit.
        public static string EnsureLengthInRange(this string model, int minLength, int maxLength, [CallerArgumentExpression("model")] string name = "")
        {
            if (model.Length < minLength || model.Length > maxLength)
                throw Invalid($"{name} length must be between {minLength} and {maxLength} characters, but it has {model.Length}.");
            return model;
        }

        #endregion

        #region Collection Checks

        // Ensures a collection (like a List or Array) is not null and contains at least one item.
        public static T EnsureNonEmpty<T>(this T? collection, [CallerArgumentExpression("collection")] string name = "") where T : ICollection
            => collection?.Count > 0 ? collection : throw Invalid($"{name} cannot be empty.");

        #endregion

        #region Enum Checks

        // Ensures a given Enum value actually exists defined within its Enum type.
        public static T EnsureEnumValueDefined<T>(this T model, [CallerArgumentExpression("model")] string name = "") where T : Enum
            => Enum.IsDefined(typeof(T), model) ? model : throw Invalid($"{name} is not a valid {typeof(T).Name} value.");

        // Validates an integer representation against an Enum type.
        public static T EnsureEnumValueDefined<T>(this int model, [CallerArgumentExpression("model")] string name = "") where T : Enum
            => Enum.IsDefined(typeof(T), model) ? (T)Enum.ToObject(typeof(T), model) : throw Invalid($"{model} is not a valid {typeof(T).Name} value.");

        // Validates a string representation against an Enum type.
        public static T EnsureEnumValueDefined<T>(this string model, [CallerArgumentExpression("model")] string name = "") where T : Enum
            => Enum.IsDefined(typeof(T), model) ? (T)Enum.Parse(typeof(T), model) : throw Invalid($"{model} is not a valid {typeof(T).Name} value.");

        #endregion

        #region Dictionary Checks

        // Ensures a specified key exists inside a dictionary, returning its value if found or throwing an error if missing.
        public static TValue EnsureKeyExists<TKey, TValue>(this IDictionary<TKey, TValue> model, TKey key, [CallerArgumentExpression("model")] string name = "")
        {
            model.EnsureNonNull(name);
            key.EnsureNonNull();

            if (!model.ContainsKey(key))
                throw Invalid($"{key} does not exist inside {name} value.");
            return model[key];
        }
        #endregion

        #region Boolean Checks

        // Ensures a boolean value is strictly true.
        public static bool? EnsureTrue(this bool model, [CallerArgumentExpression("model")] string name = "")
           => model == true ? model : throw Invalid($"{name} must be true.");

        // Ensures a boolean value is strictly false.
        public static bool? EnsureFalse(this bool model, [CallerArgumentExpression("model")] string name = "")
           => model == false ? model : throw Invalid($"{name} must be false.");
        #endregion
    }
}
