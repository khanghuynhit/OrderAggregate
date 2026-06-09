using System;
using System.Collections.Generic;
using System.Text;

namespace OrderAggregate.Domain;

public sealed class ProductId : ValueObject
{
    public Guid Value { get; }

    public ProductId(Guid value)
    {
        if (value == Guid.Empty)
            throw new DomainException("ProductId cannot be empty");

        Value = value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator Guid(ProductId productId) => productId.Value;
    public static implicit operator ProductId(Guid value) => new(value);
}
