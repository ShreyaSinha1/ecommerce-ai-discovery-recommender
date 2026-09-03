using Application.Common.Models;
using Domain.Interfaces;
using MediatR;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Application.Features.Discovery.Queries.GetSemanticSearch;

public class GetSemanticSearchQueryHandler : IRequestHandler<GetSemanticSearchQuery, List<DiscoveryResponseDto>>
{
    private readonly IOpenAiEmbeddingService _embeddingService;
    private readonly IVectorRepository _vectorRepository;

    public GetSemanticSearchQueryHandler(IOpenAiEmbeddingService embeddingService, IVectorRepository vectorRepository)
    {
        _embeddingService = embeddingService;
        _vectorRepository = vectorRepository;
    }

    public async Task<List<DiscoveryResponseDto>> Handle(GetSemanticSearchQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SearchText))
            return new List<DiscoveryResponseDto>();

        // 1. Convert input search term into numerical multi-dimensional embedding space vector
        float[] searchVector = await _embeddingService.GenerateEmbeddingAsync(request.SearchText, cancellationToken);

        // 2. Perform Cosine Distance calculation natively inside Postgres engine
        var results = await _vectorRepository.SearchSimilarProductsAsync(
            searchVector,
            request.MinConfidence,
            request.MaxResults,
            cancellationToken);

        // 3. Project to tracking tracking metric payload
        return results.Select(r => new DiscoveryResponseDto(
            r.Product.Id,
            r.Product.Name,
            r.Product.Description,
            r.Product.SKU,
            r.Product.Price,
            r.Product.Category,
            r.SimilarityScore
        )).ToList();
    }
}
