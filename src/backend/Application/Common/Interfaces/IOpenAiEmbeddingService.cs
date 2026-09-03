
using System.Threading.Tasks;
using System.Threading;

public interface IOpenAiEmbeddingService
{
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken);
}
