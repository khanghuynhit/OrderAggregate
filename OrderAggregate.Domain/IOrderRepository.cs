namespace OrderAggregate.Domain;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(OrderId id, CancellationToken cancellationToken = default);

    Task<IEnumerable<Order>> GetByCustomerIdAsync(
        CustomerId customerId,
        CancellationToken cancellationToken = default);

    void Add(Order order);

    void Update(Order order);

    void Delete(Order order);
}