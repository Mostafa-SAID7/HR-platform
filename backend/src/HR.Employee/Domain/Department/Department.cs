namespace HR.Employee.Domain.Department;

using HR.Employee.Domain.Employee;

/// <summary>
/// Department aggregate root
/// </summary>
public class Department : AggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? ManagerId { get; set; }
    public string Location { get; set; } = string.Empty;
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();

    private Department() { }

    /// <summary>
    /// Create a new department
    /// </summary>
    public static Department Create(
        string name,
        string description,
        string location,
        Guid tenantId,
        Guid? managerId = null)
    {
        return new Department
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Location = location,
            ManagerId = managerId,
            TenantId = tenantId,
            CreatedOnUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Update department information
    /// </summary>
    public void Update(string name, string description, string location, Guid? managerId = null)
    {
        Name = name;
        Description = description;
        Location = location;
        ManagerId = managerId;
        UpdatedOnUtc = DateTime.UtcNow;
    }
}
