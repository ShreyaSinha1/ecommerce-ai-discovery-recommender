using System;

namespace Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public DateTime PurchasedAt { get; private set; }

    private Order() { }

    public Order(Guid id, Guid userId, Guid productId, int quantity)
    {
        Id = id;
        UserId = userId;
        ProductId = productId;
        Quantity = quantity;
        PurchasedAt = DateTime.UtcNow;
    }
}
