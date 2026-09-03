using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks; // Nuget package targeting pgvector processing extension

namespace Infrastructure.Persistence.Repositories;
public class VectorRepository : IVectorRepository
{
    private readonly ApplicationDbContext _context;
    public VectorRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<List<(Product Product, double SimilarityScore)>> SearchSimilarProductsAsync(
        float[] queryVector,
        double minSimilarityScore,
        int limit,
        CancellationToken cancellationToken)
    {
        var vector = new Pgvector.Vector(queryVector);

        // Native mathematical distance optimization pipeline inside EF Core 9 Core LINQ API engine
        var dbHits = await _context.Products
            .AsNoTracking()
            .Include(p => p.Embedding)
            .Where(p => p.Embedding != null)
            // CosineDistance implementation optimization via HNSW / IVFFlat graph indices inside Pgvector
            .Select(p => new
            {
                Product = p,
                Distance = p.Embedding!.Vector.CosineDistance(vector)
            })
            .Where(x => (1.0 - x.Distance) >= minSimilarityScore)
            .OrderBy(x => x.Distance)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return dbHits.Select(x => (x.Product, 1.0 - x.Distance)).ToList();
    }
}
