using Common.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Common.Domain
{
    /// <summary>
    /// A strongly-typed wrapper around a standard Guid. 
    /// Using 'Id<TModel>' prevents you from accidentally mixing up IDs (e.g., passing a UserId where an OrderId is expected).
    /// 'sealed' means no other class can inherit from this record.
    /// 'record' gives us built-in value equality, immutability, and concise syntax.
    /// </summary>
    public sealed record Id<TModel> : IId, IComparable, IComparable<IId>, IComparable<Guid>, IEquatable<IId>
    {
        // The underlying unique Guid value. 
        // 'init' means it can only be set when the object is created, making this property immutable (read-only afterward).
        public Guid Value { get; init; }

        // --- Constructors ---

        // Constructor that wraps an existing Guid. 
        // 'EnsureNotDefault' checks that the Guid isn't empty/all zeros (Guid.Empty).
        public Id(Guid value) => Value = value.EnsureNotDefault(nameof(value));

        // Default constructor that automatically generates a brand-new random Guid.
        public Id() : this(Guid.NewGuid()) { }

        // --- Factory Methods ---

        // Creates a new ID instance with a random Guid.
        public static Id<TModel> New() => new(Guid.NewGuid());

        // Converts an ID from one model type to another (useful when mapping related entities).
        public static Id<TModel> FromId<TNewModel>(Id<TNewModel> id) => new(id.Value);

        // Creates an ID instance from a raw Guid.
        public static Id<TModel> FromGuid(Guid id) => new(id);

        // Parses a string into a Guid and wraps it inside this ID type.
        public static Id<TModel> FromString(string id) => new(Guid.Parse(id));

        // --- Implicit Conversions ---
        // These allow C# to automatically convert between 'Id<TModel>' and 'Guid' without extra code.

        // Allows converting an optional (nullable) ID to an optional Guid.
        public static implicit operator Guid?(Id<TModel>? id) => id?.Value;

        // Allows converting an ID directly into a standard Guid.
        public static implicit operator Guid(Id<TModel> id) => id.Value;

        // Allows converting a raw Guid directly into your typed ID.
        public static implicit operator Id<TModel>(Guid id) => new(id);

        // --- Comparisons ---
        // These methods let you sort or compare IDs against other IDs, raw Guids, or generic objects.

        public int CompareTo(object? obj)
        {
            if (obj is IId otherId) return CompareTo(otherId);
            if (obj is Guid otherGuid) return CompareTo(otherGuid);
            if (obj == null) return 1;

            throw new ArgumentException("Object must be of type IId or Guid", nameof(obj));
        }

        // Compares this ID with another IId implementation.
        public int CompareTo(IId? other) => other?.Value.CompareTo(Value) ?? 1;

        // Compares this ID with a raw Guid.
        public int CompareTo(Guid other) => Value.CompareTo(other);

        // --- Equality ---
        // Determines if two IId objects are equal by checking their underlying Guid values.
        public bool Equals(IId? other) => other?.Value == Value;
    }

    /*
     * Key Concepts Highlighted:
        Strongly-Typed IDs (Id<TModel>): Instead of using primitive Guid everywhere, this pattern wraps a Guid with a specific model context. It prevents bugs like passing a Product ID into a method that expects a Customer ID, even though both use a Guid underneath.

        C# Records (record): Records are ideal for data-centric objects. They automatically provide value-based equality (two different ID objects with the same Guid are considered equal) and support the init-only property feature for immutability.

        Implicit Operators (implicit operator): This is syntactic sugar that lets you pass your Id<TModel> directly into methods expecting a Guid (and vice-versa) without manually typing .Value every time.

Interface Implementation: By implementing comparison and equality interfaces (IComparable, IEquatable), these IDs can be easily sorted, searched, and compared in lists and databases.
    */
}
