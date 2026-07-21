namespace HR.Analytics.Application.Dtos.Search;

/// <summary>
/// Search query parameters for full-text search.
/// </summary>
public record SearchQuery
{
    public string SearchTerm { get; set; } = string.Empty;
    public string IndexName { get; set; } = string.Empty; // employees, payroll, etc.
    public int PageSize { get; set; } = 20;
    public int PageNumber { get; set; } = 1;
    public Dictionary<string, string>? Filters { get; set; }
}
