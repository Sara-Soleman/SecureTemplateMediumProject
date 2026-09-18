using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Domain
{
    /// <summary>
    /// An abstract base class for all entities in the application. 
    /// <TModel> is a generic type parameter used to keep IDs strongly-typed to specific models.
    /// It implements IAuditableEntity, meaning it automatically tracks creation and modification times.
    /// </summary>
    public abstract class Entity<TModel> : IAuditableEntity
    {
        // The unique identifier for this entity. 
        // It has a 'get' only, meaning the ID cannot be changed once the entity is created.
        public Id<TModel> Id { get; }

        // The timestamp (in UTC) tracking when this entity was created.
        public DateTimeOffset CreatedAtUtc { get; }

        // The timestamp (in UTC) tracking when this entity was last modified.
        public DateTimeOffset LastModifiedAtUtc { get; }

        // Constructor used when you already have a specific ID (e.g., when loading an existing entity from a database).
        protected Entity(Id<TModel> id)
        {
            Id = id;
            CreatedAtUtc = DateTimeOffset.UtcNow;
            LastModifiedAtUtc = DateTimeOffset.UtcNow;
        }

        // Default constructor used when creating a brand new entity. 
        // The ': this(...)' syntax chains to the constructor above, automatically generating a brand-new unique ID.
        protected Entity() : this(Id<TModel>.New()) { }

        // Overrides the default Equals method. 
        // For domain entities, two objects are considered equal if their IDs match, even if other properties differ.
        public override bool Equals(object? obj)
        {
            // Check if the object is an entity of the same type
            if (obj is Entity<TModel> entity)
            {
                return entity.Id == Id; // Return true if their IDs are equal
            }
            return false;
        }

        // Overrides GetHashCode to match Equals. 
        // This is a best practice in C# whenever you override Equals, especially if entities are stored in hash-based collections like dictionaries.
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
