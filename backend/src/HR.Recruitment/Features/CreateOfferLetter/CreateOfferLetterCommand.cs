namespace HR.Recruitment.Features.CreateOfferLetter;

/// <summary>
/// Create an offer letter for a candidate
/// </summary>
public record CreateOfferLetterCommand(
    CreateOfferLetterRequest Request,
    Guid TenantId) : ICommand<OfferLetterDto>;

/// <summary>
/// Validator for CreateOfferLetterCommand
/// </summary>
public class CreateOfferLetterCommandValidator : AbstractValidator<CreateOfferLetterCommand>
{
    public CreateOfferLetterCommandValidator()
    {
        RuleFor(x => x.Request.JobApplicationId)
            .NotEmpty().WithMessage("Job application ID is required");

        RuleFor(x => x.Request.CandidateId)
            .NotEmpty().WithMessage("Candidate ID is required");

        RuleFor(x => x.Request.OfferSalary)
            .GreaterThan(0).WithMessage("Offer salary must be greater than 0");

        RuleFor(x => x.Request.Department)
            .NotEmpty().WithMessage("Department is required");

        RuleFor(x => x.Request.Position)
            .NotEmpty().WithMessage("Position is required");

        RuleFor(x => x.Request.ProposedStartDate)
            .GreaterThan(DateTime.UtcNow.AddDays(1))
            .WithMessage("Proposed start date must be at least 2 days from now");
    }
}

/// <summary>
/// Handler for CreateOfferLetterCommand
/// </summary>
public class CreateOfferLetterCommandHandler : ICommandHandler<CreateOfferLetterCommand, OfferLetterDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<JobApplication> _jobApplicationRepository;
    private readonly IRepository<OfferLetter> _offerLetterRepository;

    public CreateOfferLetterCommandHandler(
        IUnitOfWork unitOfWork,
        IRepository<JobApplication> jobApplicationRepository,
        IRepository<OfferLetter> offerLetterRepository)
    {
        _unitOfWork = unitOfWork;
        _jobApplicationRepository = jobApplicationRepository;
        _offerLetterRepository = offerLetterRepository;
    }

    public async Task<OfferLetterDto> Handle(CreateOfferLetterCommand request, CancellationToken cancellationToken)
    {
        // Verify job application exists
        var jobApplication = await _jobApplicationRepository.GetByIdAsync(request.Request.JobApplicationId, cancellationToken);
        if (jobApplication == null)
            throw new DomainException("Job application not found");

        // Create offer letter
        var offerLetter = OfferLetter.Create(
            request.Request.JobApplicationId,
            request.Request.CandidateId,
            request.Request.OfferSalary,
            request.Request.Department,
            request.Request.Position,
            request.Request.ProposedStartDate,
            request.Request.Terms);

        // Add to repository
        await _offerLetterRepository.AddAsync(offerLetter, cancellationToken);

        // Update application status
        jobApplication.UpdateStatus(ApplicationStatus.OfferExtended, "Offer letter created");

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return offerLetter.Adapt<OfferLetterDto>();
    }
}
