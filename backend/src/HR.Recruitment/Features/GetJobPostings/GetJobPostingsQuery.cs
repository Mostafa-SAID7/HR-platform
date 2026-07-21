namespace HR.Recruitment.Features.GetJobPostings;

/// <summary>
/// Get all job postings with filtering and pagination
/// </summary>
public record GetJobPostingsQuery(
    JobPostingFilterDto Filter,
    Guid TenantId) : IQuery<PaginatedResult<JobPostingListDto>>;

/// <summary>
/// Handler for GetJobPostingsQuery
/// </summary>
public class GetJobPostingsQueryHandler : IQueryHandler<GetJobPostingsQuery, PaginatedResult<JobPostingListDto>>
{
    private readonly IQueryRepository _queryRepository;

    public GetJobPostingsQueryHandler(IQueryRepository queryRepository)
    {
        _queryRepository = queryRepository;
    }

    public async Task<PaginatedResult<JobPostingListDto>> Handle(GetJobPostingsQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;

        // Build SQL query with Dapper for better performance
        var query = @"
            SELECT 
                jp.id,
                jp.title,
                jp.department,
                jp.status::text,
                jp.posted_date,
                COUNT(ja.id) as application_count
            FROM job_postings jp
            LEFT JOIN job_applications ja ON jp.id = ja.job_posting_id
            WHERE jp.status != @ArchivedStatus
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@ArchivedStatus", JobPostingStatus.Archived.ToString());

        // Add search filter
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            query += " AND (jp.title ILIKE @SearchTerm OR jp.description ILIKE @SearchTerm)";
            parameters.Add("@SearchTerm", $"%{filter.SearchTerm}%");
        }

        // Add department filter
        if (!string.IsNullOrWhiteSpace(filter.Department))
        {
            query += " AND jp.department = @Department";
            parameters.Add("@Department", filter.Department);
        }

        // Add status filter
        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            query += " AND jp.status::text = @Status";
            parameters.Add("@Status", filter.Status);
        }

        query += " GROUP BY jp.id, jp.title, jp.department, jp.status, jp.posted_date";
        query += " ORDER BY jp.posted_date DESC";

        // Get total count
        var countQuery = query.Replace("SELECT jp.id, jp.title, jp.department, jp.status::text, jp.posted_date, COUNT(ja.id) as application_count", "SELECT COUNT(DISTINCT jp.id)");
        var totalCount = await _queryRepository.ExecuteScalarAsync<int>(countQuery, parameters, cancellationToken);

        // Apply pagination
        var skip = (filter.Page - 1) * filter.PageSize;
        query += $" LIMIT @Limit OFFSET @Offset";
        parameters.Add("@Limit", filter.PageSize);
        parameters.Add("@Offset", skip);

        // Execute query
        var postings = await _queryRepository.QueryAsync<JobPostingListDto>(query, parameters, cancellationToken);

        return new PaginatedResult<JobPostingListDto>(
            postings.ToList(),
            totalCount,
            filter.Page,
            filter.PageSize);
    }
}
