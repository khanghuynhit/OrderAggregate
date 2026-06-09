namespace OrderAggregate.Domain;

public sealed class OrderLineItem : ValueObject
{
    public ProductId ProductId { get; private set; }
    public string ProductName { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }

    public OrderLineItem(ProductId productId, string productName, decimal unitPrice, int quantity)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new DomainException("Product name cannot be empty");

        if (unitPrice <= 0)
            throw new DomainException("Unit price must be greater than 0");

        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than 0");

        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public decimal TotalPrice => UnitPrice * Quantity;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ProductId;
        yield return Quantity;
    }
}