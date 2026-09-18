using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Domain
{
    /// <summary>
    /// An abstract base class for all Value Objects in Domain-Driven Design (DDD).
    /// Unlike entities, value objects do not have a unique ID. Instead, they are defined 
    /// entirely by the combination of their property values (e.g., two Money objects with 
    /// '10 USD' are considered equal).
    /// </summary>
    public abstract class ValueObject
    {
        // An abstract method that derived classes (like DateRange or Address) must implement.
        // It returns a collection of all the internal properties/components that define this object's unique value.
        protected abstract IEnumerable<object> GetEqualityComponents();

        // Overrides the default Equals method to provide structural equality.
        // Two value objects are equal if they are of the exact same type and all their equality components match.
        public override bool Equals(object obj)
        {
            // If the other object is null or is a different class type, they aren't equal.
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            // Cast the object back to a ValueObject so we can compare its components.
            var other = (ValueObject)obj;

            // SequenceEqual checks if all items in both collections match in the exact same order.
            return this.GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        // Overrides GetHashCode so that value objects can be safely used in hash-based collections 
        // (like dictionaries or hash sets) based on their contents.
        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Select(x => x != null ? x.GetHashCode() : 0) // Get hash code for each component (handling nulls safely)
                .Aggregate((x, y) => x ^ y);                  // Combine all hash codes together using the XOR (^) operator
        }
    }
}
