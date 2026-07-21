namespace HR.Employee.Domain.EmployeeSkill;

using HR.Employee.Domain.Employee;

/// <summary>
/// Employee skill entity
/// </summary>
public class EmployeeSkill : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public int ProficiencyLevel { get; set; } // 1-5
    public Employee? Employee { get; set; }

    private EmployeeSkill() { }

    /// <summary>
    /// Create a new employee skill
    /// </summary>
    public static EmployeeSkill Create(
        Guid employeeId,
        string skillName,
        int proficiencyLevel,
        Guid tenantId)
    {
        if (proficiencyLevel < 1 || proficiencyLevel > 5)
            throw new ValidationException("Proficiency level must be between 1 and 5");

        return new EmployeeSkill
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            SkillName = skillName,
            ProficiencyLevel = proficiencyLevel,
            TenantId = tenantId,
            CreatedOnUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Update proficiency level
    /// </summary>
    public void UpdateProficiency(int proficiencyLevel)
    {
        if (proficiencyLevel < 1 || proficiencyLevel > 5)
            throw new ValidationException("Proficiency level must be between 1 and 5");

        ProficiencyLevel = proficiencyLevel;
        LastModifiedOnUtc = DateTime.UtcNow;
    }
}
