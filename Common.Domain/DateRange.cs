using Common.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Domain
{
    /// <summary>
    /// Represents a contiguous span of time with a start and end date.
    /// Inherits from 'ValueObject', meaning it is defined entirely by its values 
    /// rather than a unique ID (two DateRanges with the exact same start and end dates are considered identical).
    /// </summary>
    public sealed class DateRange : ValueObject
    {
        // Private constructor ensures that a DateRange can only be created through valid factory methods 
        // and automatically validates that the EndDate is strictly after the StartDate.
        private DateRange(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            StartDate = startDate;
            EndDate = endDate.EnsureGreaterThan(startDate);
        }

        // --- Factory Methods ---

        // Creates a DateRange by parsing start and end date strings.
        public static DateRange FromString(string startDate, string endDate)
        {
            return new DateRange(DateTimeOffset.Parse(startDate), DateTimeOffset.Parse(endDate));
        }

        // Creates a DateRange directly from existing DateTimeOffset values.
        public static DateRange From(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            return new DateRange(startDate, endDate);
        }

        // --- Properties ---

        // The beginning boundary of the time range (read-only).
        public DateTimeOffset StartDate { get; }

        // The ending boundary of the time range (read-only).
        public DateTimeOffset EndDate { get; }

        // --- Equality ---
        // Required by the base ValueObject class. It tells the system which components 
        // to look at when comparing two DateRange objects for equality.
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return StartDate;
            yield return EndDate;
        }

        // --- Behavior / Validation ---

        // Checks if a given timestamp (e.g., current time) falls within this date range, 
        // throwing an exception if it falls outside.
        public void InRange(DateTimeOffset utcNow)
        {
            utcNow.EnsureWithinRange(StartDate, EndDate);
        }
    }
}