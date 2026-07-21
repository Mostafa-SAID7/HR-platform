namespace HR.Employee.Application.Dtos.Employee;

/// <summary>
/// Employee skill DTO.
/// </summary>
public record EmployeeSkillDto
{
    public Guid Id { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public int ProficiencyLevel { get; set; }
}
