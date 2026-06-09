# 📋 Order Aggregate - Domain-Driven Design & Clean Architecture

> Bài tập C# .NET 10 - Implement Order Aggregate với DDD principles, Clean Architecture & SOLID pattern

**Table of Contents**
- [Tổng Quan](#tổng-quan)
- [Kiến Trúc & Concepts](#kiến-trúc--concepts)
- [DDD - Domain-Driven Design](#ddd---domain-driven-design)
- [Clean Architecture](#clean-architecture)
- [SOLID Principles](#solid-principles)
- [5-Layer Architecture](#5-layer-architecture)
- [Key Patterns](#key-patterns)
- [5 Invariants - Business Rules](#5-invariants---business-rules)
- [Project Flow](#project-flow)
- [Công Nghệ & Stack](#công-nghệ--stack)
- [Cách Chạy Project](#cách-chạy-project)
- [Tài Liệu Tham Khảo](#tài-liệu-tham-khảo)

---

## 🎯 Tổng Quan

**Bài tập:** Thiết kế và implement một **Order Aggregate** sử dụng Domain-Driven Design (DDD)

**Mục đích chính:**
1. ✅ Bảo vệ **invariants** (business rules) trong Aggregate
2. ✅ Tách biệt logic business (Domain) khỏi infrastructure
3. ✅ Áp dụng **Clean Architecture** - 5 layers riêng biệt
4. ✅ Tuân theo **SOLID principles**
5. ✅ 30+ unit tests để verify tất cả rules

---

## 📐 Kiến Trúc & Concepts

```
                        ORDER AGGREGATE
                              |
                ______________|______________
               |              |              |
        DDD Concepts    Clean Architecture  SOLID Principles
          (Trái)           (Phải)            (Trên)
          
        ├─ Aggregate      ├─ Domain Layer    ├─ S: Single Responsibility
        ├─ Value Objects  ├─ Application     ├─ O: Open/Closed
        ├─ Domain Events  ├─ Infrastructure  ├─ L: Liskov Substitution
        ├─ Invariants     ├─ API             ├─ I: Interface Segregation
        └─ Entities       └─ Tests           └─ D: Dependency Inversion
```

---

## 🏛️ DDD - Domain-Driven Design

Domain-Driven Design là cách thiết kế software dựa trên **business logic**, không phải database.

### Core Concepts

| Concept | Định Nghĩa | Ví Dụ Trong Project |
|---------|-----------|------------------|
| **Aggregate** | Tập hợp entities & value objects được quản lý như một đơn vị | `Order` aggregate |
| **Aggregate Root** | Entity chính của aggregate, điểm vào duy nhất | `Order` class |
| **Entity** | Object có identity độc lập | `Order` (mỗi order khác nhau) |
| **Value Object** | Object không có identity, chỉ quan trọng giá trị | `OrderId(Guid)`, `ProductId`, `Money` |
| **Domain Event** | Sự kiện diễn ra trong business, được raise khi state thay đổi | `OrderCreated`, `ItemAdded`, `OrderCompleted` |
| **Repository** | Abstraction layer truy cập persistence | `IOrderRepository` |
| **Ubiquitous Language** | Ngôn ngữ chung giữa developers & business | Order, Item, Status, Complete, Cancel |
| **Invariants** | Business rules không được phép vi phạm | "Order phải có ≥1 item" |

### Order Aggregate Structure

```csharp
Order (Aggregate Root)
├─ OrderId (Value Object)
├─ CustomerId (Value Object)
├─ Status (Enum)
├─ TotalAmount (decimal)
├─ CreatedAt (DateTime)
├─ CompletedAt (DateTime?)
│
└─ Items (Collection of OrderLineItem)
   ├─ ProductId (Value Object)
   ├─ ProductName (string)
   ├─ UnitPrice (decimal)
   └─ Quantity (int)

Domain Events:
├─ OrderCreatedDomainEvent
├─ OrderItemAddedDomainEvent
├─ OrderItemRemovedDomainEvent
├─ OrderCompletedDomainEvent
└─ OrderCancelledDomainEvent
```

### DDD Benefits

```
✅ Business logic không phụ thuộc vào framework
✅ Dễ test - tất cả logic trong Domain layer
✅ Domain Events cho event sourcing & analytics
✅ Invariants được bảo vệ tại source (Order class)
✅ Code dễ hiểu vì gần với business language
```

---

## 🏗️ Clean Architecture

Clean Architecture tách ứng dụng thành **các layer độc lập** với dependencies hướng vào **center (Domain)**.

### Dependency Rule

```
                    ┌─────────────────┐
                    │   Frameworks    │
                    │   (UI, DB, Web) │
                    └────────┬────────┘
                             │
                    ┌────────▼────────┐
                    │  Interface      │
                    │  Adapters       │
                    └────────┬────────┘
                             │
                    ┌────────▼────────┐
                    │  Application    │
                    │  Business Rules │
                    └────────┬────────┘
                             │
                    ┌────────▼────────┐
                    │  Entities       │
                    │  Enterprise     │
                    │  Business Rules │
                    └─────────────────┘

⚠️ Rule: Phụ thuộc CHỈ HƯỚNG về CENTER (Domain)
         Không bao giờ vào ngoài
```

### Layer Dependencies

```
API Layer (Controllers)
    ↓ phụ thuộc vào
Application Layer (Commands, Handlers, DTOs)
    ↓ phụ thuộc vào
Domain Layer (Entities, Value Objects, Events)
    ↕ (Domain không phụ thuộc vào ai)
Infrastructure Layer (Database, Repositories)
    (Infrastructure là "plugin" - có thể swap out)
```

### Mỗi Layer Chứa Gì?

| Layer | Trách Nhiệm | Không Được Làm |
|-------|-----------|-----------------|
| **Domain** | Core business logic, entities, value objects | Không import EF Core, không biết database |
| **Application** | Command/Query handlers, orchestration | Không trực tiếp access database |
| **Infrastructure** | Database, repositories, EF Core | Không chứa business logic |
| **API** | HTTP controllers, routes, serialization | Không có business logic |
| **Tests** | Unit tests, integration tests | Không deploy |

---

## ⚙️ SOLID Principles

SOLID giúp code dễ maintain, test, extend mà không cần sửa cũ.

### 1. Single Responsibility Principle (SRP)

**Định nghĩa:** Một class chỉ nên có một lý do để thay đổi.

```csharp
✅ GOOD
public class Order : AggregateRoot
{
    // Chỉ quản lý order logic
    public void AddItem(OrderLineItem item) { ... }
    public void Complete() { ... }
}

public class OrderRepository : IOrderRepository
{
    // Chỉ quản lý persistence
    public async Task<Order?> GetByIdAsync(OrderId id) { ... }
}

❌ BAD
public class Order
{
    public void AddItem() { ... }
    public void SaveToDatabase() { ... }  // ❌ 2 responsibilities
    public void SendEmail() { ... }       // ❌ 3 responsibilities
}
```

### 2. Open/Closed Principle (OCP)

**Định nghĩa:** Mở cho extension, đóng cho modification.

```csharp
✅ GOOD
public interface IOrderRepository  // Open for extension
{
    Task<Order?> GetByIdAsync(OrderId id);
}

public class OrderRepository : IOrderRepository  // Can extend
{
    public async Task<Order?> GetByIdAsync(OrderId id) { ... }
}

public class OrderRepositoryCache : IOrderRepository  // New implementation
{
    private readonly IOrderRepository _inner;
    
    public async Task<Order?> GetByIdAsync(OrderId id)
    {
        // Check cache first, then delegate
    }
}

❌ BAD
public class OrderService
{
    public void GetOrder(int id)
    {
        if (database == "SQL") { ... }
        else if (database == "MongoDB") { ... }  // ❌ Phải modify class
    }
}
```

### 3. Liskov Substitution Principle (LSP)

**Định nghĩa:** Subclass phải có thể thay thế được superclass.

```csharp
✅ GOOD
IOrderRepository repo = new OrderRepository();  // Thực thi
IOrderRepository cachedRepo = new CachedOrderRepository(repo);  // Cache
IOrderRepository mockRepo = new Mock<IOrderRepository>();  // Test

// Client code không cần biết implementation cụ thể
Order order = await repo.GetByIdAsync(orderId);

❌ BAD
public class CachedOrderRepository : OrderRepository
{
    public override async Task<Order?> GetByIdAsync(OrderId id)
    {
        // Hoàn toàn khác logic - không thể thay thế được!
        return _cache.GetOrDefault(id);
    }
}
```

### 4. Interface Segregation Principle (ISP)

**Định nghĩa:** Clients không nên phụ thuộc vào interfaces mà chúng không dùng.

```csharp
✅ GOOD
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(OrderId id);
    void Add(Order order);
    void Update(Order order);
}

public interface IUnitOfWork
{
    IOrderRepository Orders { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

❌ BAD
public interface IRepository  // Quá lớn, generic
{
    Task<T> GetByIdAsync<T>(object id);
    void Add<T>(T entity);
    void Update<T>(T entity);
    void Delete<T>(T entity);
    Task<List<T>> GetAllAsync<T>();
    // ... 20 methods khác
}
```

### 5. Dependency Inversion Principle (DIP)

**Định nghĩa:** Phụ thuộc vào abstractions (interfaces), không phụ thuộc vào concrete classes.

```csharp
✅ GOOD
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    private readonly IUnitOfWork _unitOfWork;  // Phụ thuộc vào interface
    
    public CreateOrderCommandHandler(IUnitOfWork unitOfWork)  // Inject interface
    {
        _unitOfWork = unitOfWork;
    }
}

// DI Container có thể swap implementation
services.AddScoped<IUnitOfWork, UnitOfWork>();  // Có thể thay UnitOfWork khác

❌ BAD
public class CreateOrderCommandHandler
{
    private readonly OrderRepository _repository;  // Phụ thuộc vào concrete class
    
    public CreateOrderCommandHandler()
    {
        _repository = new OrderRepository();  // Hardcoded dependency
    }
}
```

---

## 🏢 5-Layer Architecture

### Project Structure

```
OrderAggregate (Solution)
│
├─ OrderAggregate.Domain (Layer 1)
│  ├─ Order.cs (Aggregate Root)
│  ├─ OrderLineItem.cs (Entity)
│  ├─ OrderId.cs, CustomerId.cs, ProductId.cs (Value Objects)
│  ├─ OrderStatus.cs (Enum)
│  ├─ *DomainEvent.cs (Events)
│  ├─ DomainException.cs, InvalidOrderStateException.cs
│  ├─ IOrderRepository.cs (Interface)
│  ├─ ValueObject.cs (Base Class)
│  └─ AggregateRoot.cs (Base Class)
│
├─ OrderAggregate.Application (Layer 2)
│  └─ OrderCommandHandlers.cs
│     ├─ CreateOrderCommand
│     ├─ CreateOrderCommandHandler
│     ├─ AddOrderItemCommand
│     ├─ AddOrderItemCommandHandler
│     ├─ RemoveOrderItemCommand
│     ├─ RemoveOrderItemCommandHandler
│     ├─ CompleteOrderCommand
│     ├─ CompleteOrderCommandHandler
│     ├─ CancelOrderCommand
│     ├─ CancelOrderCommandHandler
│     ├─ OrderResponse (DTO)
│     └─ OrderItemResponse (DTO)
│
├─ OrderAggregate.Infrastructure (Layer 3)
│  ├─ OrderDbContext.cs (EF Core DbContext)
│  ├─ OrderConfiguration.cs (EF Mappings)
│  ├─ OrderRepository.cs (IOrderRepository Implementation)
│  ├─ UnitOfWork.cs (IUnitOfWork Implementation)
│  └─ Startup.cs (DI Configuration)
│
├─ OrderAggregate.API (Layer 4)
│  ├─ OrdersController.cs
│  ├─ Program.cs
│  ├─ appsettings.json
│  └─ OrderAggregate.API.http
│
└─ OrderAggregate.Tests (Layer 5)
   └─ OrderAggregateTests.cs (30+ unit tests)
```

### Layer Responsibilities

#### Domain Layer (OrderAggregate.Domain)
- **Trách nhiệm:** Core business logic
- **Chứa:** Entities, Value Objects, Aggregates, Domain Events
- **Không được:** Import EF Core, SQL, HTTP, UI frameworks
- **Đặc điểm:** Pure C# classes, không phụ thuộc vào bên ngoài

```csharp
namespace OrderAggregate.Domain;

public sealed class Order : AggregateRoot
{
    private readonly List<OrderLineItem> _items = new();
    
    public OrderId Id { get; private set; }
    public CustomerId CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    
    public void AddItem(OrderLineItem item)
    {
        // Business logic - không biết về database
        if (Status == OrderStatus.Cancelled)
            throw new InvalidOrderStateException("Cannot add items to cancelled order");
        
        _items.Add(item);
        RaiseDomainEvent(new OrderItemAddedDomainEvent(...));
    }
}
```

#### Application Layer (OrderAggregate.Application)
- **Trách nhiệm:** Orchestration, command handling
- **Chứa:** Commands, Handlers, DTOs, Validators
- **Không được:** Trực tiếp access database, business logic
- **Pattern:** CQRS, MediatR

```csharp
namespace OrderAggregate.Application;

public record AddOrderItemCommand(
    Guid OrderId,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity) : IRequest;

public class AddOrderItemCommandHandler : IRequestHandler<AddOrderItemCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    
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
        
        order.AddItem(item);  // Delegate to domain
        
        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
```

#### Infrastructure Layer (OrderAggregate.Infrastructure)
- **Trách nhiệm:** Persistence, external services
- **Chứa:** DbContext, Repositories, EF Configurations
- **Không được:** Business logic, application logic
- **Technology:** EF Core, SQL Server, ORM

```csharp
namespace OrderAggregate.Infrastructure;

public class OrderDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
    }
}

public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;
    
    public async Task<Order?> GetByIdAsync(OrderId id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}
```

#### API Layer (OrderAggregate.API)
- **Trách nhiệm:** HTTP endpoints, serialization
- **Chứa:** Controllers, route definitions
- **Không được:** Business logic, database access (direct)
- **Technology:** ASP.NET Core, REST

```csharp
namespace OrderAggregate.API;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    
    [HttpPost]
    public async Task<ActionResult<OrderResponse>> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateOrderCommand(request.CustomerId);
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetOrder), new { id = result.Id }, result);
    }
}
```

#### Tests Layer (OrderAggregate.Tests)
- **Trách nhiệm:** Verify domain rules
- **Chứa:** Unit tests, integration tests
- **Tools:** xUnit, Moq
- **Coverage:** 30+ tests cho 5 invariants

```csharp
namespace OrderAggregate.Tests;

public class OrderAggregateInvariantTests
{
    [Fact]
    public void AddItem_Should_ThrowWhen_CancelledOrder()
    {
        // Arrange
        var order = Order.Create(_orderId, _customerId);
        var item = new OrderLineItem(new ProductId(Guid.NewGuid()), "P1", 100m, 1);
        order.AddItem(item);
        order.Cancel();
        
        // Act & Assert
        var newItem = new OrderLineItem(new ProductId(Guid.NewGuid()), "P2", 100m, 1);
        var ex = Assert.Throws<InvalidOrderStateException>(() => order.AddItem(newItem));
        Assert.Contains("cancelled", ex.Message.ToLower());
    }
}
```

---

## 🎨 Key Patterns

### 1. Aggregate Pattern

**Mục đích:** Bảo vệ business invariants

```
Aggregate = Tập hợp các objects được quản lý như một unit
Aggregate Root = Entity chính, điểm vào duy nhất

┌─────────────────────────────────┐
│         Order Aggregate         │  Boundary
│  ┌──────────────────────────┐   │
│  │ Order (Root)             │   │
│  │ - AddItem()              │   │
│  │ - RemoveItem()           │   │
│  │ - Complete()             │   │  Tất cả thay đổi
│  │ - Cancel()               │   │  đi qua Order
│  └──────────────────────────┘   │
│          ↑                       │
│  ┌──────────────────────────┐   │
│  │ OrderLineItem            │   │  Không được access
│  │ - ProductId              │   │  trực tiếp từ ngoài
│  │ - Quantity               │   │
│  └──────────────────────────┘   │
│          ↑                       │
│  ┌──────────────────────────┐   │
│  │ Value Objects            │   │
│  │ - OrderId                │   │
│  │ - ProductId              │   │
│  │ - CustomerId             │   │
│  └──────────────────────────┘   │
└─────────────────────────────────┘
```

### 2. Unit of Work Pattern

**Mục đích:** Quản lý transactions, batch operations

```csharp
// Pattern
public interface IUnitOfWork : IDisposable
{
    IOrderRepository Orders { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

// Sử dụng
using (var uow = new UnitOfWork(_context))
{
    var order = await uow.Orders.GetByIdAsync(orderId);
    order.AddItem(item);
    uow.Orders.Update(order);
    
    await uow.SaveChangesAsync();  // Một transaction duy nhất
}
```

### 3. Repository Pattern

**Mục đích:** Abstraction layer, swap implementations dễ dàng

```csharp
// Interface - Application/Domain không cần biết SQL
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(OrderId id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetByCustomerIdAsync(CustomerId customerId, CancellationToken cancellationToken = default);
    void Add(Order order);
    void Update(Order order);
    void Delete(Order order);
}

// Implementation - Infrastructure biết SQL/EF Core
public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;
    
    public async Task<Order?> GetByIdAsync(OrderId id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}

// Testing - Swap với mock
var mockRepo = new Mock<IOrderRepository>();
mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<OrderId>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(order);
```

### 4. CQRS (Command Query Responsibility Segregation)

**Mục đích:** Tách read & write operations

```csharp
// Commands - thay đổi state
public record CreateOrderCommand(Guid CustomerId) : IRequest<OrderResponse>;
public record AddOrderItemCommand(...) : IRequest;
public record CompleteOrderCommand(Guid OrderId) : IRequest;

// Query (không implement trong bài này, future)
public record GetOrderQuery(Guid OrderId) : IRequest<OrderResponse>;

// Handlers - xử lý commands
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    public async Task<OrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Write logic
    }
}

// Lợi ích:
// ✅ Write model và Read model có thể tối ưu riêng
// ✅ Dễ scale - read-heavy workloads → caching
// ✅ Event sourcing - store all events
```

### 5. Domain Events

**Mục đích:** Capture sự kiện business, enable event sourcing

```csharp
// Event definition
public sealed record OrderCompletedDomainEvent(OrderId OrderId, decimal FinalAmount) : DomainEvent;

// Raise trong domain
public void Complete()
{
    // ...validation...
    Status = OrderStatus.Completed;
    CompletedAt = DateTime.UtcNow;
    
    // Broadcast event
    RaiseDomainEvent(new OrderCompletedDomainEvent(this.Id, this.TotalAmount));
}

// Handle trong application
services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly);
});

// Future: Event handlers có thể:
// ✅ Send emails
// ✅ Update read model
// ✅ Trigger workflow
// ✅ Store event log
```

---

## 🛡️ 5 Invariants - Business Rules

Invariants là **quy tắc business không được phép vi phạm**. Chúng được bảo vệ trong Order aggregate.

### Invariant 1: Order Phải Có ≥1 Item

```csharp
public void RemoveItem(ProductId productId)
{
    if (_items.Count == 1)
        throw new InvalidOrderStateException("Order must contain at least one item");
    
    // ... remove item ...
}

public void Complete()
{
    if (!_items.Any())
        throw new InvalidOrderStateException("Cannot complete order without items");
    
    // ... complete ...
}
```

**Test:**
```csharp
[Fact]
public void RemoveItem_Should_ThrowWhen_OnlyOneItemLeft()
{
    var order = Order.Create(_orderId, _customerId);
    var productId = new ProductId(Guid.NewGuid());
    var item = new OrderLineItem(productId, "P1", 100m, 1);
    order.AddItem(item);
    
    var ex = Assert.Throws<InvalidOrderStateException>(() => order.RemoveItem(productId));
    Assert.Contains("at least one item", ex.Message);
}
```

### Invariant 2: Giá & Quantity Phải > 0

```csharp
public sealed class OrderLineItem : ValueObject
{
    public OrderLineItem(ProductId productId, string productName, decimal unitPrice, int quantity)
    {
        if (unitPrice <= 0)
            throw new DomainException("Unit price must be greater than 0");
        
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than 0");
        
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }
}
```

**Test:**
```csharp
[Fact]
public void OrderLineItem_Should_ThrowWhen_NegativePrice()
{
    var ex = Assert.Throws<DomainException>(() => 
        new OrderLineItem(new ProductId(Guid.NewGuid()), "P1", -100m, 1));
    
    Assert.Contains("greater than 0", ex.Message);
}
```

### Invariant 3: Không Add Vào Cancelled/Completed Order

```csharp
public void AddItem(OrderLineItem item)
{
    if (Status == OrderStatus.Cancelled)
        throw new InvalidOrderStateException("Cannot add items to a cancelled order");
    
    if (Status == OrderStatus.Completed)
        throw new InvalidOrderStateException("Cannot add items to a completed order");
    
    // ... add item ...
}
```

**Test:**
```csharp
[Fact]
public void AddItem_Should_ThrowWhen_CancelledOrder()
{
    var order = Order.Create(_orderId, _customerId);
    var item = new OrderLineItem(...);
    order.AddItem(item);
    order.Cancel();
    
    var newItem = new OrderLineItem(...);
    var ex = Assert.Throws<InvalidOrderStateException>(() => order.AddItem(newItem));
    Assert.Contains("cancelled", ex.Message.ToLower());
}
```

### Invariant 4: Chỉ Complete Từ Pending Status

```csharp
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
}
```

**Test:**
```csharp
[Fact]
public void Complete_Should_ChangeStatusToCompleted()
{
    var order = Order.Create(_orderId, _customerId);
    var item = new OrderLineItem(...);
    order.AddItem(item);
    
    order.Complete();
    
    Assert.Equal(OrderStatus.Completed, order.Status);
    Assert.NotNull(order.CompletedAt);
}

[Fact]
public void Complete_Should_ThrowWhen_NoItems()
{
    var order = Order.Create(_orderId, _customerId);
    
    var ex = Assert.Throws<InvalidOrderStateException>(() => order.Complete());
    Assert.Contains("without items", ex.Message);
}
```

### Invariant 5: Không Cancel Completed Order

```csharp
public void Cancel()
{
    if (Status == OrderStatus.Completed)
        throw new InvalidOrderStateException("Cannot cancel a completed order");
    
    if (Status == OrderStatus.Cancelled)
        throw new InvalidOrderStateException("Order is already cancelled");
    
    Status = OrderStatus.Cancelled;
}
```

**Test:**
```csharp
[Fact]
public void Cancel_Should_ThrowWhen_AlreadyCompleted()
{
    var order = Order.Create(_orderId, _customerId);
    var item = new OrderLineItem(...);
    order.AddItem(item);
    order.Complete();
    
    var ex = Assert.Throws<InvalidOrderStateException>(() => order.Cancel());
    Assert.Contains("completed", ex.Message.ToLower());
}
```

### Invariant Summary Table

| # | Rule | Enforced At | Test Count |
|---|------|-------------|-----------|
| 1 | Min 1 item | `RemoveItem()`, `Complete()` | 5 |
| 2 | Price/Qty > 0 | `OrderLineItem` constructor | 6 |
| 3 | No add to Cancelled/Completed | `AddItem()` | 8 |
| 4 | Complete only from Pending | `Complete()` | 7 |
| 5 | No cancel Completed | `Cancel()` | 4 |
| **TOTAL** | **5 Invariants** | **Domain Layer** | **30+ tests** |

---

## 🔄 Project Flow

### Flow 1: Create Order

```
User Request
    ↓
HTTP POST /api/orders
    ↓
OrdersController.CreateOrder()
    ↓
MediatR → CreateOrderCommandHandler
    ↓
Order.Create(orderId, customerId)
    ├─ new Order() with Pending status
    └─ RaiseDomainEvent(OrderCreatedDomainEvent)
    ↓
_unitOfWork.Orders.Add(order)
    ↓
_unitOfWork.SaveChangesAsync()
    ├─ DbContext.SaveChangesAsync()
    └─ PublishDomainEvents()
    ↓
Database INSERT orders table
    ↓
HTTP 201 Created
    ├─ OrderResponse DTO
    └─ Location header
```

### Flow 2: Add Item (With Invariant Protection)

```
User Request
    ↓
HTTP POST /api/orders/{id}/items
    ├─ { productId, productName, unitPrice, quantity }
    ↓
OrdersController.AddItem()
    ↓
MediatR → AddOrderItemCommandHandler
    ↓
_unitOfWork.Orders.GetByIdAsync(orderId)
    ├─ Return null → 404 Not Found
    └─ Return Order entity
    ↓
Order.AddItem(item)
    ├─ Check: Status == Cancelled? → Exception 409
    ├─ Check: Status == Completed? → Exception 409
    ├─ Check: Duplicate item? → Exception 400
    └─ Add item + RaiseDomainEvent(ItemAddedDomainEvent)
    ↓
_unitOfWork.Orders.Update(order)
    ↓
_unitOfWork.SaveChangesAsync()
    ├─ DbContext.SaveChangesAsync()
    └─ PublishDomainEvents()
    ↓
Database UPDATE orders & order_items tables
    ↓
HTTP 202 Accepted
```

### Flow 3: Complete Order (Invariant Protection)

```
User Request
    ↓
HTTP POST /api/orders/{id}/complete
    ↓
OrdersController.CompleteOrder()
    ↓
MediatR → CompleteOrderCommandHandler
    ↓
_unitOfWork.Orders.GetByIdAsync(orderId)
    ↓
Order.Complete()
    ├─ Check: Status != Pending? → Exception 409
    ├─ Check: No items? → Exception 409
    ├─ Check: TotalAmount <= 0? → Exception 409
    └─ Status = Completed + CompletedAt = now
    └─ RaiseDomainEvent(OrderCompletedDomainEvent)
    ↓
_unitOfWork.SaveChangesAsync()
    ↓
HTTP 202 Accepted
    ↓
🎉 All 5 Invariants Protected
```

---

## 💻 Công Nghệ & Stack

### Backend Framework
- **.NET 10** - Latest LTS version
- **C# 12** - Modern language features
- **ASP.NET Core** - Web framework

### Data Access
- **Entity Framework Core 10** - ORM
- **SQL Server** - Database (default)
- **PostgreSQL** - Alternative (not configured yet)

### Business Logic & Patterns
- **MediatR 14.1.0** - CQRS pattern, command dispatching
- **Domain-Driven Design** - Architecture pattern

### Testing & Quality
- **xUnit 2.9.3** - Testing framework
- **Moq** - Mocking library
- **30+ Unit Tests** - Verify invariants

### Code Quality Tools
- **Visual Studio 2022/2026** - IDE
- **Serilog 4.3.1** - Structured logging
- **Entity Framework Core Tools** - Migrations

### Project Structure
```
OrderAggregate/
├─ OrderAggregate.sln
├─ OrderAggregate.Domain/
│  └─ OrderAggregate.Domain.csproj (net10.0)
├─ OrderAggregate.Application/
│  └─ OrderAggregate.Application.csproj (net10.0)
├─ OrderAggregate.Infrastructure/
│  └─ OrderAggregate.Infrastructure.csproj (net10.0)
├─ OrderAggregate.API/
│  └─ OrderAggregate.API.csproj (net10.0)
└─ OrderAggregate.Tests/
   └─ OrderAggregate.Tests.csproj (net10.0)
```

---

## 🚀 Cách Chạy Project

### 1. Clone & Open Solution
```bash
cd OrderAggregate
# Mở OrderAggregate.sln trong Visual Studio 2022+
```

### 2. Cài NuGet Packages

```powershell
# Tools → NuGet Package Manager → Package Manager Console

# Domain Layer - không cần packages
# (chỉ pure C# classes)

# Application Layer
Install-Package MediatR -Version 14.1.0 -ProjectName OrderAggregate.Application

# Infrastructure Layer
Install-Package Microsoft.EntityFrameworkCore.SqlServer -Version 10.0.8 -ProjectName OrderAggregate.Infrastructure
Install-Package Microsoft.EntityFrameworkCore.Tools -Version 10.0.8 -ProjectName OrderAggregate.Infrastructure
Install-Package Serilog -Version 4.3.1 -ProjectName OrderAggregate.Infrastructure
Install-Package Serilog.AspNetCore -Version 10.0.0 -ProjectName OrderAggregate.Infrastructure
Install-Package Serilog.Sinks.Console -Version 6.1.1 -ProjectName OrderAggregate.Infrastructure
Install-Package Serilog.Sinks.File -Version 7.0.0 -ProjectName OrderAggregate.Infrastructure
Install-Package Serilog.Enrichers.Environment -Version 3.0.1 -ProjectName OrderAggregate.Infrastructure

# API Layer
# (sẽ auto-include MediatR từ Application)

# Tests Layer
Install-Package xunit -Version 2.9.3 -ProjectName OrderAggregate.Tests
Install-Package xunit.runner.visualstudio -Version 3.1.5 -ProjectName OrderAggregate.Tests
Install-Package Moq -ProjectName OrderAggregate.Tests
```

### 3. Setup Database

```powershell
# Create migration
# Default project: OrderAggregate.Infrastructure

Add-Migration InitialCreate

# Update database (tạo tables)
Update-Database
```

### 4. Build Solution

```
Ctrl + Shift + B
# Kết quả: Build succeeded ✅
```

### 5. Run API

```
F5 (Debug)
# hoặc
Ctrl + F5 (Without Debug)

# Swagger UI mở tự động:
# https://localhost:7000/swagger
```

### 6. Run Tests

```
# Cách 1: Test Explorer
Ctrl + E, T
→ Run All Tests

# Cách 2: Terminal
cd OrderAggregate.Tests
dotnet test

# Kết quả: 30+ tests passed ✅
```

### 7. Test API qua Swagger

```
POST /api/orders
- Tạo order mới

POST /api/orders/{id}/items
- Thêm item

POST /api/orders/{id}/complete
- Hoàn tất order

POST /api/orders/{id}/cancel
- Hủy order

DELETE /api/orders/{id}/items/{productId}
- Xóa item
```

---

## 📚 Tài Liệu Tham Khảo

### DDD (Domain-Driven Design)
- **Eric Evans** - "Domain-Driven Design: Tackling Complexity in the Heart of Software"
- **Vaughn Vernon** - "Implementing Domain-Driven Design"
- **DDD Community:** https://dddcommunity.org/

### Clean Architecture
- **Robert C. Martin (Uncle Bob)** - "Clean Architecture: A Craftsman's Guide to Software Structure and Design"
- **Microsoft Docs:** https://learn.microsoft.com/en-us/dotnet/architecture/clean-code/

### SOLID Principles
- **Robert C. Martin** - "Clean Code: A Handbook of Agile Software Craftsmanship"
- **Microsoft Docs:** https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/architectural-principles

### Entity Framework Core
- **Official Docs:** https://learn.microsoft.com/en-us/ef/core/

### MediatR & CQRS
- **MediatR GitHub:** https://github.com/jbogard/MediatR
- **CQRS Pattern:** https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs

### Unit Testing
- **xUnit Docs:** https://xunit.net/
- **Moq Docs:** https://github.com/Moq/moq4/wiki/Quickstart

---

## 🎓 Học Thêm

### Tiếp Theo (Future Enhancements)
1. **Event Sourcing** - Store toàn bộ events thay vì current state
2. **Saga Pattern** - Distributed transactions
3. **Event Handlers** - Send emails, update read model
4. **Specifications** - Reusable query logic
5. **AutoMapper** - Object-to-object mapping
6. **FluentValidation** - Advanced validation
7. **GraphQL** - Alternative to REST
8. **Microservices** - Break into domain services
9. **API Versioning** - Support multiple versions
10. **Caching** - Redis integration

### Recommended Books
- 📖 "Domain-Driven Design" - Eric Evans
- 📖 "Clean Architecture" - Robert C. Martin
- 📖 "Building Microservices" - Sam Newman
- 📖 "Enterprise Integration Patterns" - Gregor Hohpe

### Online Resources
- 🎥 DDD Training - Pluralsight
- 🎥 Clean Code - YouTube (Uncle Bob)
- 📝 Microsoft Architecture Guides
- 📝 Martin Fowler's Blog

---

## 📝 Summary

| Aspect | Cách Thực Hiện |
|--------|----------------|
| **Business Rules** | Bảo vệ trong Order aggregate (5 invariants) |
| **Architecture** | 5 layers - Domain, Application, Infrastructure, API, Tests |
| **Design Patterns** | Aggregate, Repository, Unit of Work, CQRS |
| **SOLID** | SRP (mỗi class 1 trách nhiệm), OCP (mở extension), DIP (interface injection) |
| **Testing** | 30+ unit tests - verify tất cả invariants |
| **Technology** | .NET 10, EF Core, MediatR, SQL Server |
| **Extensibility** | Easy to add Event Sourcing, Event Handlers, Caching |

---

## 👤 Tác Giả

**Bài tập** về **DDD & Clean Architecture** trong **C# .NET 10**

Created: June 2026
Technology Stack: .NET 10, Entity Framework Core, MediatR, xUnit

---

**Happy Coding! 🚀**

Nếu có câu hỏi về DDD, Clean Architecture, hoặc SOLID principles, đừng ngần ngại hỏi!
