namespace HR.Analytics.Features.Search;

public record SearchEmployeesQuery(string SearchTerm, int PageSize, int PageNumber, Guid TenantId) : IQuery<SearchResultDto<EmployeeAnalyticsDto>>;

public class SearchEmployeesQueryHandler : IRequestHandler<SearchEmployeesQuery, SearchResultDto<EmployeeAnalyticsDto>>
{
    private readonly IElasticsearchService _elasticsearchService;
    private readonly ILogger<SearchEmployeesQueryHandler> _logger;

    public SearchEmployeesQueryHandler(
        IElasticsearchService elasticsearchService,
        ILogger<SearchEmployeesQueryHandler> logger)
    {
        _elasticsearchService = elasticsearchService;
        _logger = logger;
    }

    public async Task<SearchResultDto<EmployeeAnalyticsDto>> Handle(SearchEmployeesQuery request, CancellationToken cancellationToken)
    {
        var searchQuery = new SearchQuery
        {
            SearchTerm = request.SearchTerm,
            IndexName = "employees",
            PageSize = request.PageSize,
            PageNumber = request.PageNumber
        };

        var result = await _elasticsearchService.SearchAsync<EmployeeAnalyticsDto>(searchQuery, cancellationToken);
        
        _logger.LogInformation(
            "Searched employees with term '{SearchTerm}': found {Count} results",
            request.SearchTerm, result.Results.Count);

        return result;
    }
}
