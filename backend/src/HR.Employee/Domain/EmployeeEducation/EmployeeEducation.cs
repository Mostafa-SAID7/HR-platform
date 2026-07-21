namespace HR.Employee.Domain.EmployeeEducation;

using HR.Employee.Domain.Employee;

/// <summary>
/// Employee education entity
/// </summary>
public class EmployeeEducation : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public string InstitutionName { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string FieldOfStudy { get; set; } = string.Empty;
    public DateTime GraduationDate { get; set; }
    public string? Grade { get; set; }
    public Employee? Employee { get; set; }

    private EmployeeEducation() { }

    /// <summary>
    /// Create a new employee education record
    /// </summary>
    public static EmployeeEducation Create(
        Guid employeeId,
        string institutionName,
        string degree,
        string fieldOfStudy,
        DateTime graduationDate,
        string? grade = null,
        Guid? tenantId = null)
    {
        if (graduationDate > DateTime.UtcNow)
            throw new ValidationException("Graduation date cannot be in the future");

        return new EmployeeEducation
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            InstitutionName = institutionName,
            Degree = degree,
            FieldOfStudy = fieldOfStudy,
            GraduationDate = graduationDate,
            Grade = grade,
            TenantId = tenantId ?? Guid.Empty,
            CreatedOnUtc = DateTime.UtcNow
        };
    }
}
