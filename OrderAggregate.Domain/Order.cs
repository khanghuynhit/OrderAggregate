namespace OrderAggregate.Domain;

public sealed class Order : AggregateRoot
{
    private readonly List<OrderLineItem> _items = new();

    public OrderId Id { get; private set; }
    public CustomerId CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public IReadOnlyList<OrderLineItem> Items => _items.AsReadOnly();

    private Order() { }

    public static Order Create(OrderId id, CustomerId customerId)
    {
        var order = new Order
        {
            Id = id,
            CustomerId = customerId,
            Status = OrderStatus.Pending,
            TotalAmount = 0,
            CreatedAt = DateTime.UtcNow
        };

        order.RaiseDomainEvent(new OrderCreatedDomainEvent(order.Id, order.CustomerId));

        return order;
    }

    public void AddItem(OrderLineItem item)
    {
        if (Status == OrderStatus.Cancelled)
            throw new InvalidOrderStateException("Cannot add items to a cancelled order");

        if (Status == OrderStatus.Completed)
            throw new InvalidOrderStateException("Cannot add items to a completed order");

        if (item == null)
            throw new ArgumentNullException(nameof(item));

        if (_items.Any(x => x.ProductId == item.ProductId))
            throw new DomainException($"Product {item.ProductId} already exists in this order");

        _items.Add(item);
        RecalculateTotal();

        RaiseDomainEvent(new OrderItemAddedDomainEvent(
            this.Id,
            item.ProductId,
            item.Quantity,
            item.TotalPrice));
    }

    public void RemoveItem(ProductId productId)
    {
        if (Status == OrderStatus.Cancelled || Status == OrderStatus.Completed)
            throw new InvalidOrderStateException("Cannot modify completed or cancelled orders");

        var item = _items.FirstOrDefault(x => x.ProductId == productId);
        if (item == null)
            throw new DomainException("Item not found in order");

        if (_items.Count == 1)
            throw new InvalidOrderStateException("Order must contain at least one item");

        _items.Remove(item);
        RecalculateTotal();

        RaiseDomainEvent(new OrderItemRemovedDomainEvent(this.Id, productId));
    }

    public void Complete()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOrderStateException($"Cannot complete order in {Status} status");

        if (!_items.Any())
            throw new InvalidOrderStateException("Cannot complete order without items");

        if (TotalAmount <= 0)
            throw new InvalidOrderStateException("Order total must be greater than 0");

        Status = OrderStatus.Completed;
        CompletedAt = DateTime.UtcNow;

        RaiseDomainEvent(new OrderCompletedDomainEvent(this.Id, this.TotalAmount));
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Completed)
            throw new InvalidOrderStateException("Cannot cancel a completed order");

        if (Status == OrderStatus.Cancelled)
            throw new InvalidOrderStateException("Order is already cancelled");

        Status = OrderStatus.Cancelled;

        RaiseDomainEvent(new OrderCancelledDomainEvent(this.Id));
    }

    private void RecalculateTotal()
    {
        TotalAmount = _items.Sum(x => x.TotalPrice);
    }

    public override object GetId() => Id;
}