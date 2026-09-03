namespace Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string SKU { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public ProductEmbedding? Embedding { get; private set; }

    private Product() { } // EF Core Constructor

    public Product(Guid id, string name, string description, string sku, decimal price, string category)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty.");
        Id = id;
        Name = name;
        Description = description;
        SKU = sku;
        Price = price;
        Category = category;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void AssignEmbedding(float[] vector)
    {
        Embedding = new ProductEmbedding(Id, vector);
    }
}
