
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Domain.Interfaces;

public interface IVectorRepository
{
    Task<List<(Product Product, double SimilarityScore)>> SearchSimilarProductsAsync(
        float[] queryVector,
        double minSimilarityScore,
        int limit,
        CancellationToken cancellationToken);
}
