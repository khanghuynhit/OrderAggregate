using Xunit;
using OrderAggregate.Domain;
using System;

namespace OrderAggregate.Tests;

public class OrderAggregateInvariantTests
{
    private readonly OrderId _orderId = new(Guid.NewGuid());
    private readonly CustomerId _customerId = new(Guid.NewGuid());

    [Fact]
    public void Create_Should_CreateOrderWithPendingStatus()
    {
        var order = Order.Create(_orderId, _customerId);

        Assert.Equal(_orderId, order.Id);
        Assert.Equal(_customerId, order.CustomerId);
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Equal(0, order.TotalAmount);
        Assert.Empty(order.Items);
    }

    [Fact]
    public void AddItem_Should_AddValidItem()
    {
        var order = Order.Create(_orderId, _customerId);
        var item = new OrderLineItem(
            new ProductId(Guid.NewGuid()),
            "Product 1",
            100m,
            2);

        order.AddItem(item);

        Assert.Single(order.Items);
        Assert.Equal(200m, order.TotalAmount);
    }

    [Fact]
    public void AddItem_Should_ThrowWhen_CancelledOrder()
    {
        var order = Order.Create(_orderId, _customerId);
        var item = new OrderLineItem(new ProductId(Guid.NewGuid()), "P1", 100m, 1);
        order.AddItem(item);
        order.Cancel();

        var newItem = new OrderLineItem(new ProductId(Guid.NewGuid()), "P2", 100m, 1);

        // Chỉ cần verify hệ thống chặn lại bằng đúng Exception là đạt chuẩn
        Assert.Throws<InvalidOrderStateException>(() => order.AddItem(newItem));
    }

    [Fact]
    public void Complete_Should_ChangeStatusToCompleted()
    {
        var order = Order.Create(_orderId, _customerId);
        var item = new OrderLineItem(new ProductId(Guid.NewGuid()), "P1", 100m, 1);
        order.AddItem(item);

        order.Complete();

        Assert.Equal(OrderStatus.Completed, order.Status);
        Assert.NotNull(order.CompletedAt);
    }

    [Fact]
    public void Complete_Should_ThrowWhen_NoItems()
    {
        var order = Order.Create(_orderId, _customerId);

        Assert.Throws<InvalidOrderStateException>(() => order.Complete());
    }

    [Fact]
    public void RemoveItem_Should_ThrowWhen_OnlyOneItemLeft()
    {
        var order = Order.Create(_orderId, _customerId);
        var productId = new ProductId(Guid.NewGuid());
        var item = new OrderLineItem(productId, "P1", 100m, 1);
        order.AddItem(item);

        Assert.Throws<InvalidOrderStateException>(() => order.RemoveItem(productId));
    }
}