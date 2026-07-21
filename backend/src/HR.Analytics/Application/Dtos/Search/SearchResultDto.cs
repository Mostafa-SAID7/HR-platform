namespace HR.Analytics.Application.Dtos.Search;

/// <summary>
/// Search result wrapper.
/// </summary>
public record SearchResultDto<T>
{
    public List<T> Results { get; set; } = new();
    public long TotalCount { get; set; }
    public int PageSize { get; set; }
    public int PageNumber { get; set; }
}
