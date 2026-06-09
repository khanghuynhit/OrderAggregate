using System;
using System.Collections.Generic;
using System.Text;

namespace OrderAggregate.Domain;

public sealed class CustomerId : ValueObject
{
    public Guid Value { get; }

    public CustomerId(Guid value)
    {
        if (value == Guid.Empty)
            throw new DomainException("CustomerId cannot be empty");
        Value = value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator Guid(CustomerId id) => id.Value;
    public static implicit operator CustomerId(Guid value) => new(value);
}



