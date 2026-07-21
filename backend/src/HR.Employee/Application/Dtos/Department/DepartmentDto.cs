namespace HR.Employee.Application.Dtos.Department;

/// <summary>
/// Department DTO.
/// </summary>
public record DepartmentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public Guid? ManagerId { get; set; }
    public int EmployeeCount { get; set; }
}
