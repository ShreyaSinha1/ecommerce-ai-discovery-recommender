using Application.Common.Models;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Application.Features.Recommendations.Queries.GetPersonalizedRecommendations;

public record GetRecommendationsQuery(Guid UserId, int Limit = 6) : IRequest<List<DiscoveryResponseDto>>;

public class GetRecommendationsQueryHandler : IRequestHandler<GetRecommendationsQuery, List<DiscoveryResponseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IVectorRepository _vectorRepository;
    private readonly IOpenAiEmbeddingService _embeddingService;

    public GetRecommendationsQueryHandler(IApplicationDbContext context, IVectorRepository vectorRepository, IOpenAiEmbeddingService embeddingService)
    {
        _context = context;
        _vectorRepository = vectorRepository;
        _embeddingService = embeddingService;
    }

    public async Task<List<DiscoveryResponseDto>> Handle(GetRecommendationsQuery request, CancellationToken cancellationToken)
    {
        // Affinity Matrix Resolver: Query recent purchase telemetry mapping
        var targetProductIds = await _context.Orders
            .Where(o => o.UserId == request.UserId)
            .OrderByDescending(o => o.PurchasedAt)
            .Take(3)
            .Select(o => o.ProductId)
            .ToListAsync(cancellationToken);

        if (!targetProductIds.Any())
        {
            // Fallback strategy: return highest generic top-sellers or system fallback baseline items
            var fallbacks = await _context.Products.Take(request.Limit).ToListAsync(cancellationToken);
            return fallbacks.Select(p => new DiscoveryResponseDto(p.Id, p.Name, p.Description, p.SKU, p.Price, p.Category, 1.0)).ToList();
        }

        // Resolve context vectors for high-affinity historical entries
        var structuralEmbeddings = await _context.ProductEmbeddings
            .Where(pe => targetProductIds.Contains(pe.ProductId))
            .Select(pe => pe.Vector)
            .ToListAsync(cancellationToken);

        // Compute centroid anchor representation vector from baseline tracking footprint
        float[] centroidVector = new float[1536];
        foreach (var vec in structuralEmbeddings)
        {
            for (int i = 0; i < 1536; i++) centroidVector[i] += vec[i];
        }
        for (int i = 0; i < 1536; i++) centroidVector[i] /= structuralEmbeddings.Count;

        var matches = await _vectorRepository.SearchSimilarProductsAsync(centroidVector, 0.40, request.Limit, cancellationToken);

        return matches.Select(m => new DiscoveryResponseDto(
            m.Product.Id, m.Product.Name, m.Product.Description, m.Product.SKU, m.Product.Price, m.Product.Category, m.SimilarityScore
        )).ToList();
    }
}
