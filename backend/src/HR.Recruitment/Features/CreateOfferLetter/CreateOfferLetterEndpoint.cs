namespace HR.Recruitment.Features.CreateOfferLetter;

using MediatR;
using HR.Common.Domain;
using HR.Recruitment.Application.Dtos.OfferLetter;

/// <summary>
/// Endpoint for creating an offer letter
/// </summary>
public static class CreateOfferLetterEndpoint
{
    public static async Task<IResult> Handle(
        CreateOfferLetterRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Results.BadRequest("Invalid tenant");

        var command = new CreateOfferLetterCommand(request, tenantId);
        var result = await mediator.Send(command, cancellationToken);
        return Results.Created($"/recruitment/offer-letters/{result.Id}", ApiResponse<OfferLetterDto>.Created(result));
    }
}
