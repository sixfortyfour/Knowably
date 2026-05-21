using RagQnA.Contracts.Models;

namespace RagQnA.Contracts.Interfaces;

public interface IUpstashVectorClient
{
    Task UpsertAsync(IEnumerable<VectorRecord> records);
    Task<IEnumerable<VectorQueryResult>> QueryAsync(float[] vector, int topK, string? filter = null);
    Task<IEnumerable<VectorRecord>> FetchAsync(IEnumerable<string> ids);
    Task DeleteAsync(IEnumerable<string> ids);
    Task DeleteByFilterAsync(string filter);
    Task<VectorIndexInfo> InfoAsync();
}
