namespace HR.Recruitment.Features.CreateOfferLetter;

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
