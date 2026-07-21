namespace HR.Performance.Features.CreatePerformanceReview;

using HR.Performance.Application.Dtos.PerformanceReview;
using HR.Performance.Application.Mappings;
using HR.Performance.Domain;

/// <summary>
/// Handler for CreatePerformanceReviewCommand.
/// </summary>
public class CreatePerformanceReviewCommandHandler : IRequestHandler<CreatePerformanceReviewCommand, PerformanceReviewDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreatePerformanceReviewCommandHandler> _logger;

    public CreatePerformanceReviewCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CreatePerformanceReviewCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PerformanceReviewDetailDto> Handle(CreatePerformanceReviewCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;

        // Check if review already exists for this employee and period
        var reviewRepository = _unitOfWork.GetRepository<PerformanceReview>();
        var existingReview = await reviewRepository.GetAsQueryable()
            .FirstOrDefaultAsync(r => r.EmployeeId == req.EmployeeId &&
                r.ReviewYear == req.ReviewYear &&
                r.ReviewQuarter == req.ReviewQuarter &&
                r.TenantId == request.TenantId && !r.IsDeleted, cancellationToken);

        if (existingReview is not null)
        {
            _logger.LogWarning("Performance review already exists for employee {EmployeeId} in {Year}Q{Quarter}",
                req.EmployeeId, req.ReviewYear, req.ReviewQuarter);
            throw new AlreadyExistsException("PerformanceReview", $"{req.EmployeeId}-{req.ReviewYear}Q{req.ReviewQuarter}");
        }

        // Create new performance review
        var review = PerformanceReview.Create(
            employeeId: req.EmployeeId,
            reviewerId: req.ReviewerId,
            employeeName: req.EmployeeName,
            reviewerName: req.ReviewerName,
            reviewYear: req.ReviewYear,
            reviewQuarter: req.ReviewQuarter,
            tenantId: request.TenantId);

        await reviewRepository.AddAsync(review, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Performance review created for employee {EmployeeId} ({EmployeeName}) for {Year}Q{Quarter}",
            req.EmployeeId, req.EmployeeName, req.ReviewYear, req.ReviewQuarter);

        return review.ToDetailDto();
    }
}
