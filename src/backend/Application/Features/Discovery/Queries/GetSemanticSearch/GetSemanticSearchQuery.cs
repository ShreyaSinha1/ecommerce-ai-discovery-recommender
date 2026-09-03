using MediatR;
using System.Collections.Generic;

namespace Application.Features.Discovery.Queries.GetSemanticSearch;
public record GetSemanticSearchQuery(
    string SearchText,
    double MinConfidence = 0.65,
    int MaxResults = 12) : IRequest<List<DiscoveryResponseDto>>
