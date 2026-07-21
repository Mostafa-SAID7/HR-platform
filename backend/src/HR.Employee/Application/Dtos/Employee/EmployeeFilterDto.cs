namespace HR.Employee.Application.Dtos.Employee;

/// <summary>
/// Employee filter/search DTO.
/// </summary>
public record EmployeeFilterDto : FilterDto
{
    public string? Department { get; set; }
    public string? Status { get; set; }
    public string? EmploymentType { get; set; }
    public bool? IsActive { get; set; }
}
