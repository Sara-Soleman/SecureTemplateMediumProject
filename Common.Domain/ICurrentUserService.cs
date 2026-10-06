using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Domain
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        string? Username { get; }
    }
}
