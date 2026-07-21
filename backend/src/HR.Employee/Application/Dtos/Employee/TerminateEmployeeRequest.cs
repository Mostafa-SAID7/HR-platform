namespace HR.Employee.Application.Dtos.Employee;

/// <summary>
/// Terminate employee request DTO.
/// </summary>
public record TerminateEmployeeRequest
{
    public DateTime TerminationDate { get; set; }
    public string? Reason { get; set; }
}
