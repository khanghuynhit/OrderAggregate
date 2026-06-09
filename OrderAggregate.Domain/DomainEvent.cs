using System;
using System.Collections.Generic;
using System.Text;

namespace OrderAggregate.Domain;

public abstract record DomainEvent(DateTime OccurredAt = default)
{
    public DateTime OccurredAt { get; init; } =
        OccurredAt == default ? DateTime.UtcNow : OccurredAt;

    public Guid Id { get; init; } = Guid.NewGuid();
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}
