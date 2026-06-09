using System;
using System.Collections.Generic;
using System.Text;

namespace OrderAggregate.Domain;

public sealed record OrderCreatedDomainEvent(OrderId OrderId, CustomerId CustomerId)
    : DomainEvent;

public sealed record OrderItemAddedDomainEvent(
    OrderId OrderId,
    ProductId ProductId,
    int Quantity,
    decimal TotalPrice)
    : DomainEvent;

public sealed record OrderItemRemovedDomainEvent(OrderId OrderId, ProductId ProductId)
    : DomainEvent;

public sealed record OrderCompletedDomainEvent(OrderId OrderId, decimal FinalAmount)
    : DomainEvent;

public sealed record OrderCancelledDomainEvent(OrderId OrderId)
    : DomainEvent;
