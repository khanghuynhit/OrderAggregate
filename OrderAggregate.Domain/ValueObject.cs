using System;
using System.Collections.Generic;
using System.Text;

namespace OrderAggregate.Domain;

public abstract class ValueObject
{
    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
            return false;

        var valueObject = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(valueObject.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(1, (current, obj) =>
                unchecked(current * 23 + (obj?.GetHashCode() ?? 0)));
    }

    protected abstract IEnumerable<object> GetEqualityComponents();
}
