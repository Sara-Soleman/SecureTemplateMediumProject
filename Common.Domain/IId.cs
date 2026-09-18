using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Domain
{
    public interface IId : IComparable, IComparable<IId>, IComparable<Guid>, IEquatable<IId>
    {
        Guid Value { get; }
    }
}
