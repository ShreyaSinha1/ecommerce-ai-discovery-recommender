using System;

namespace Application.Common.Models;

public record DiscoveryResponseDto(
    Guid Id,
    string Name,
    string Description,
    string SKU,
    decimal Price,
    string Category,
    double SimilarityScore)
