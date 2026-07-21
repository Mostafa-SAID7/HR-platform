namespace HR.Analytics.Application.Services;

/// <summary>
/// Interface for Elasticsearch operations.
/// </summary>
public interface IElasticsearchService
{
    /// <summary>
    /// Index a document in Elasticsearch.
    /// </summary>
    Task<bool> IndexDocumentAsync<T>(string indexName, T document, string documentId, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Search documents in Elasticsearch.
    /// </summary>
    Task<SearchResultDto<T>> SearchAsync<T>(SearchQuery query, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Delete a document from Elasticsearch.
    /// </summary>
    Task<bool> DeleteDocumentAsync(string indexName, string documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create index if not exists.
    /// </summary>
    Task<bool> CreateIndexIfNotExistsAsync(string indexName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk index documents.
    /// </summary>
    Task<int> BulkIndexAsync<T>(string indexName, List<(string Id, T Document)> documents, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Get index statistics.
    /// </summary>
    Task<Dictionary<string, object>> GetIndexStatsAsync(string indexName, CancellationToken cancellationToken = default);
}
