namespace HR.Analytics.Application.Services;

using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;

/// <summary>
/// Elasticsearch service implementation.
/// </summary>
public class ElasticsearchService : IElasticsearchService
{
    private readonly ElasticsearchClient _client;
    private readonly ILogger<ElasticsearchService> _logger;

    public ElasticsearchService(ElasticsearchClient client, ILogger<ElasticsearchService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<bool> IndexDocumentAsync<T>(string indexName, T document, string documentId, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var result = await _client.IndexAsync(indexName, document, d => d.Id(documentId), cancellationToken);
            
            if (result.IsSuccess())
            {
                _logger.LogInformation("Document indexed successfully in {IndexName} with ID {DocumentId}", indexName, documentId);
                return true;
            }

            _logger.LogWarning("Failed to index document in {IndexName}: {Error}", indexName, result.DebugInformation);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while indexing document in {IndexName}", indexName);
            return false;
        }
    }

    public async Task<SearchResultDto<T>> SearchAsync<T>(SearchQuery query, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var from = (query.PageNumber - 1) * query.PageSize;
            
            var searchRequest = new SearchRequest(query.IndexName)
            {
                From = from,
                Size = query.PageSize,
                Query = new BoolQuery
                {
                    Must = new List<Query>
                    {
                        new MultiMatchQuery
                        {
                            Query = query.SearchTerm,
                            Fields = new Fields("*") // Search all fields
                        }
                    }
                },
                Highlight = new Highlight
                {
                    Fields = new Dictionary<Field, HighlightField>
                    {
                        { "*", new HighlightField() }
                    }
                }
            };

            var result = await _client.SearchAsync<T>(searchRequest, cancellationToken);

            if (!result.IsSuccess())
            {
                _logger.LogWarning("Search failed: {Error}", result.DebugInformation);
                return new SearchResultDto<T> { PageNumber = query.PageNumber, PageSize = query.PageSize };
            }

            var documents = result.Documents?.ToList() ?? new();
            var totalCount = result.Total;

            _logger.LogInformation("Search completed: found {Count} documents in {IndexName}", documents.Count, query.IndexName);

            return new SearchResultDto<T>
            {
                Results = documents,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during search in {IndexName}", query.IndexName);
            return new SearchResultDto<T> { PageNumber = query.PageNumber, PageSize = query.PageSize };
        }
    }

    public async Task<bool> DeleteDocumentAsync(string indexName, string documentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _client.DeleteAsync(indexName, documentId, cancellationToken);
            
            if (result.IsSuccess())
            {
                _logger.LogInformation("Document deleted from {IndexName}", indexName);
                return true;
            }

            _logger.LogWarning("Failed to delete document from {IndexName}: {Error}", indexName, result.DebugInformation);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while deleting document from {IndexName}", indexName);
            return false;
        }
    }

    public async Task<bool> CreateIndexIfNotExistsAsync(string indexName, CancellationToken cancellationToken = default)
    {
        try
        {
            var existsResponse = await _client.Indices.ExistsAsync(indexName, cancellationToken);
            
            if (existsResponse.Exists)
            {
                _logger.LogInformation("Index {IndexName} already exists", indexName);
                return true;
            }

            var createResponse = await _client.Indices.CreateAsync(indexName, c => c
                .Settings(s => s
                    .NumberOfShards(1)
                    .NumberOfReplicas(0))
                .Mappings(m => m
                    .Properties(p => p
                        .Text(t => t.EmployeeName)
                        .Keyword(k => k.Department)
                        .Date(d => d.JoinDate))), cancellationToken);

            if (createResponse.IsSuccess())
            {
                _logger.LogInformation("Index {IndexName} created successfully", indexName);
                return true;
            }

            _logger.LogWarning("Failed to create index {IndexName}: {Error}", indexName, createResponse.DebugInformation);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while creating index {IndexName}", indexName);
            return false;
        }
    }

    public async Task<int> BulkIndexAsync<T>(string indexName, List<(string Id, T Document)> documents, CancellationToken cancellationToken = default) where T : class
    {
        if (documents.Count == 0)
        {
            return 0;
        }

        try
        {
            var bulkDescriptor = new BulkRequest(indexName);
            var bulkOperations = new List<IBulkOperation>();

            foreach (var (id, doc) in documents)
            {
                bulkOperations.Add(new BulkIndexOperation<T>(doc) { Id = id });
            }

            var bulkRequest = new BulkRequest { Operations = bulkOperations };
            var result = await _client.BulkAsync(bulkRequest, cancellationToken);

            if (result.IsSuccess())
            {
                _logger.LogInformation("Bulk indexed {Count} documents in {IndexName}", documents.Count, indexName);
                return documents.Count;
            }

            _logger.LogWarning("Bulk indexing partially failed: {Error}", result.DebugInformation);
            return result.Items?.Count(i => i.IsSuccess()) ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during bulk indexing in {IndexName}", indexName);
            return 0;
        }
    }

    public async Task<Dictionary<string, object>> GetIndexStatsAsync(string indexName, CancellationToken cancellationToken = default)
    {
        try
        {
            var statsResponse = await _client.Indices.StatsAsync(indexName, cancellationToken);
            
            if (!statsResponse.IsSuccess())
            {
                _logger.LogWarning("Failed to get stats for {IndexName}", indexName);
                return new Dictionary<string, object>();
            }

            var stats = new Dictionary<string, object>
            {
                { "IndexName", indexName },
                { "DocumentCount", statsResponse.Indices?[indexName]?.Primaries?.Docs?.Count ?? 0 },
                { "SizeInBytes", statsResponse.Indices?[indexName]?.Primaries?.Store?.SizeInBytes ?? 0 }
            };

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while getting stats for {IndexName}", indexName);
            return new Dictionary<string, object>();
        }
    }
}
