namespace HR.Recruitment.Domain.JobApplication;

/// <summary>
/// Enum representing the status of a job application
/// </summary>
public enum ApplicationStatus
{
    Submitted = 0,
    Shortlisted = 1,
    Rejected = 2,
    Selected = 3,
    Declined = 4,
    OfferExtended = 5
}
