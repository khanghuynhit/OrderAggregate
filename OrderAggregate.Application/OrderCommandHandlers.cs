using MediatR;
using OrderAggregate.Domain;
using OrderAggregate.Infrastructure;


namespace OrderAggregate.Application;

// Commands
public record CreateOrderCommand(Guid CustomerId) : IRequest<OrderResponse>;

public record AddOrderItemCommand(
    Guid OrderId,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity) : IRequest;

public record RemoveOrderItemCommand(Guid OrderId, Guid ProductId) : IRequest;

public record CompleteOrderCommand(Guid OrderId) : IRequest;

public record CancelOrderCommand(Guid OrderId) : IRequest;

// DTOs
public record OrderResponse(
    Guid Id,
    Guid CustomerId,
    string Status,
    decimal TotalAmount,
    List<OrderItemResponse> Items,
    DateTime CreatedAt);

public record OrderItemResponse(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice);

// Handlers
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderResponse> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        var customerId = new CustomerId(request.CustomerId);
        var orderId = new OrderId(Guid.NewGuid());

        var order = Order.Create(orderId, customerId);

        _unitOfWork.Orders.Add(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToResponse(order);
    }

    private OrderResponse MapToResponse(Order order)
    {
        return new OrderResponse(
            order.Id.Value,
            order.CustomerId.Value,
            order.Status.ToString(),
            order.TotalAmount,
            order.Items.Select(x => new OrderItemResponse(
                x.ProductId.Value,
                x.ProductName,
                x.UnitPrice,
                x.Quantity,
                x.TotalPrice)).ToList(),
            order.CreatedAt);
    }
}

public class AddOrderItemCommandHandler : IRequestHandler<AddOrderItemCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddOrderItemCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AddOrderItemCommand request, CancellationToken cancellationToken)
    {
        var orderId = new OrderId(request.OrderId);
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken);

        if (order is null)
            throw new DomainException($"Order {request.OrderId} not found");

        var item = new OrderLineItem(
            new ProductId(request.ProductId),
            request.ProductName,
            request.UnitPrice,
            request.Quantity);

        order.AddItem(item);

        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

       
    }
}

public class RemoveOrderItemCommandHandler : IRequestHandler<RemoveOrderItemCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public RemoveOrderItemCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        RemoveOrderItemCommand request,
        CancellationToken cancellationToken)
    {
        var orderId = new OrderId(request.OrderId);
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken);

        if (order is null)
            throw new DomainException($"Order {request.OrderId} not found");

        order.RemoveItem(new ProductId(request.ProductId));

        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

       
    }
}

public class CompleteOrderCommandHandler : IRequestHandler<CompleteOrderCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public CompleteOrderCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        CompleteOrderCommand request,
        CancellationToken cancellationToken)
    {
        var orderId = new OrderId(request.OrderId);
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken);

        if (order is null)
            throw new DomainException($"Order {request.OrderId} not found");

        order.Complete();

        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

      
    }
}

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public CancelOrderCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        CancelOrderCommand request,
        CancellationToken cancellationToken)
    {
        var orderId = new OrderId(request.OrderId);
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken);

        if (order is null)
            throw new DomainException($"Order {request.OrderId} not found");

        order.Cancel();

        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

       
    }
}