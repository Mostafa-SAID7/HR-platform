namespace HR.Employee.Application.Dtos.Employee;

/// <summary>
/// Update employee request DTO.
/// </summary>
public record UpdateEmployeeRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
    public decimal Salary { get; set; }
    public Guid? ManagerId { get; set; }
}
