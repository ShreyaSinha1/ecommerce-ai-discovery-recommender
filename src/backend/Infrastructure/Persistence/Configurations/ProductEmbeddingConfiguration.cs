using Domain.Entities;

namespace Infrastructure.Persistence.Configurations;

public class ProductEmbeddingConfiguration : IEntityTypeConfiguration<ProductEmbedding>
{
    public override void Configure(EntityTypeBuilder<ProductEmbedding> builder)
    {
        builder.HasKey(pe => pe.ProductId);

        // Native .NET 9 Vector Database mapping boundary parameters
        builder.Property(pe => pe.Vector)
            .HasColumnType("vector(1536)");

        builder.HasOne(pe => pe.Product)
            .WithOne(p => p.Embedding)
            .HasForeignKey<ProductEmbedding>(pe => pe.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
