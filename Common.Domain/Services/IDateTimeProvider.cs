using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Domain.Services
{
    /// <summary>
    /// Represents a contract for getting the current date and time.
    /// Using an interface instead of calling DateTime.UtcNow directly makes your code testable, 
    /// allowing you to mock or control time during unit testing.
    /// keeping date generation consistent across your entire application.
    /// </summary>
    public interface IDateTimeProvider
    {
        // Returns the current date and time in Coordinated Universal Time (UTC).
        DateTimeOffset UtcNow();
    }
}


