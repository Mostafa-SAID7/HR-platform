namespace HR.Recruitment.Domain.OfferLetter.Events;

/// <summary>
/// Domain event raised when an offer letter is accepted
/// </summary>
public record OfferAcceptedEvent(
    Guid Id,
    Guid CandidateId,
    string CandidateName) : IDomainEvent;
