namespace HR.Common.Dto;

/// <summary>
/// Base DTO for all data transfer objects.
/// </summary>
public abstract record BaseDto
{
    public Guid Id { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    public Guid? UpdatedBy { get; set; }
}

/// <summary>
/// DTO for list responses.
/// </summary>
public record ListDto
{
    public Guid Id { get; set; }
    public DateTime CreatedOnUtc { get; set; }
}

/// <summary>
/// DTO for detailed entity responses.
/// </summary>
public abstract record DetailDto : BaseDto
{
}

/// <summary>
/// DTO for create/update requests.
/// </summary>
public abstract record CreateUpdateDto
{
}

/// <summary>
/// Generic pagination request DTO.
/// </summary>
public record PaginationDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public void Validate()
    {
        if (PageNumber < 1)
            PageNumber = 1;

        if (PageSize < 1)
            PageSize = 10;

        if (PageSize > 100)
            PageSize = 100;
    }
}

/// <summary>
/// Generic search/filter DTO.
/// </summary>
public record FilterDto : PaginationDto
{
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
}
