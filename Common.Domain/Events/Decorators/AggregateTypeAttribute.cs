using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Domain.Events.Decorators
{
    /// <summary>
    /// A custom attribute used to label domain event classes with their corresponding aggregate name 
    /// (e.g., tagging an event with [AggregateType("Order")]).
    /// [AttributeUsage(AttributeTargets.Class)] restricts this attribute so it can only be placed on classes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AggregateTypeAttribute(string aggregateType) : Attribute
    {
        // Stores the name of the aggregate type (read-only once set).
        public string AggregateType { get; } = aggregateType;
    }
}
