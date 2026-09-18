using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Domain
{
    /// <summary>
    /// Represents a contract (interface) for entities that need to track their audit history.
    /// Any class that implements this interface promises to provide creation and modification timestamps.
    /// </summary>
    public interface IAuditableEntity
    {
        // Property to store the timestamp (in UTC) when the entity was originally created.
        // It only has a 'get' accessor, meaning its value can be read, but external code cannot change it directly.
        public DateTimeOffset CreatedAtUtc { get; }

        // Property to store the timestamp (in UTC) when the entity was last updated or modified.
        // Like CreatedAtUtc, it is read-only from the outside to ensure data integrity.
        public DateTimeOffset LastModifiedAtUtc { get; }
    }
}
