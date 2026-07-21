namespace HR.Common.Pagination;

/// <summary>
/// Helper methods for pagination calculations.
/// </summary>
public static class PaginationHelper
{
    /// <summary>
    /// Validate and normalize pagination parameters.
    /// </summary>
    public static (int PageNumber, int PageSize) ValidateAndNormalize(int pageNumber, int pageSize)
    {
        const int minPageNumber = 1;
        const int minPageSize = 1;
        const int maxPageSize = 100;

        pageNumber = pageNumber < minPageNumber ? minPageNumber : pageNumber;
        pageSize = pageSize < minPageSize ? minPageSize : pageSize > maxPageSize ? maxPageSize : pageSize;

        return (pageNumber, pageSize);
    }

    /// <summary>
    /// Calculate the skip count for database queries.
    /// </summary>
    public static int CalculateSkip(int pageNumber, int pageSize)
    {
        return (pageNumber - 1) * pageSize;
    }

    /// <summary>
    /// Calculate total pages from total count and page size.
    /// </summary>
    public static int CalculateTotalPages(int totalCount, int pageSize)
    {
        return (int)Math.Ceiling(totalCount / (double)pageSize);
    }

    /// <summary>
    /// Check if there's a next page.
    /// </summary>
    public static bool HasNextPage(int pageNumber, int totalPages)
    {
        return pageNumber < totalPages;
    }

    /// <summary>
    /// Check if there's a previous page.
    /// </summary>
    public static bool HasPreviousPage(int pageNumber)
    {
        return pageNumber > 1;
    }
}
