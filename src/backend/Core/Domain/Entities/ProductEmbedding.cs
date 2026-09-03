using System;

namespace Domain.Entities;

public class ProductEmbedding
{
    public Guid ProductId { get; private set; }
    public float[] Vector { get; private set; } = [];
    public DateTime UpdatedAt { get; private set; }

    public Product Product { get; private set; } = null!;

    private ProductEmbedding() { }

    public ProductEmbedding(Guid productId, float[] vector)
    {
        if (vector == null || vector.Length != 1536)
            throw new ArgumentException("Vector space must match exactly 1536 dimensions.");

        ProductId = productId;
        Vector = vector;
        UpdatedAt = DateTime.UtcNow;
    }
}
