namespace HR.Employee.Domain.Employee;

using HR.Employee.Domain.Department;
using HR.Employee.Domain.EmployeeSkill;
using HR.Employee.Domain.EmployeeEducation;
using HR.Employee.Domain.Employee.Events;

/// <summary>
/// Employee aggregate root
/// </summary>
public class Employee : AggregateRoot
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    
    // Position & Department
    public Guid DepartmentId { get; set; }
    public Guid? ManagerId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public EmploymentType EmploymentType { get; set; }
    public decimal Salary { get; set; }
    public string Currency { get; set; } = "USD";
    
    // Contact & Location
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    
    // Status
    public bool IsActive { get; set; } = true;
    public EmployeeStatus Status { get; set; }
    
    // Relations
    public Department? Department { get; set; }
    public ICollection<EmployeeSkill> Skills { get; set; } = new List<EmployeeSkill>();
    public ICollection<EmployeeEducation> Education { get; set; } = new List<EmployeeEducation>();

    private Employee() { }

    /// <summary>
    /// Create a new employee
    /// </summary>
    public static Employee Create(
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        DateTime dateOfBirth,
        string gender,
        string nationalId,
        DateTime hireDate,
        Guid departmentId,
        string jobTitle,
        EmploymentType employmentType,
        decimal salary,
        Guid tenantId)
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = phoneNumber,
            DateOfBirth = dateOfBirth,
            Gender = gender,
            NationalId = nationalId,
            HireDate = hireDate,
            DepartmentId = departmentId,
            JobTitle = jobTitle,
            EmploymentType = employmentType,
            Salary = salary,
            IsActive = true,
            Status = EmployeeStatus.Active,
            TenantId = tenantId,
            CreatedOnUtc = DateTime.UtcNow
        };

        // Raise domain event
        employee.AddDomainEvent(new EmployeeCreatedEvent
        {
            EmployeeId = employee.Id,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            TenantId = tenantId
        });

        return employee;
    }

    /// <summary>
    /// Update employee information
    /// </summary>
    public void Update(
        string firstName,
        string lastName,
        string phoneNumber,
        string jobTitle,
        Guid departmentId,
        decimal salary,
        Guid? managerId = null)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        JobTitle = jobTitle;
        DepartmentId = departmentId;
        Salary = salary;
        ManagerId = managerId;
        UpdatedOnUtc = DateTime.UtcNow;

        // Raise domain event
        AddDomainEvent(new EmployeeUpdatedEvent
        {
            EmployeeId = Id,
            FirstName = firstName,
            LastName = lastName,
            TenantId = TenantId
        });
    }

    /// <summary>
    /// Terminate employment
    /// </summary>
    public void Terminate(DateTime terminationDate)
    {
        TerminationDate = terminationDate;
        Status = EmployeeStatus.Terminated;
        IsActive = false;
        UpdatedOnUtc = DateTime.UtcNow;

        // Raise domain event
        AddDomainEvent(new EmployeeTerminatedEvent
        {
            EmployeeId = Id,
            FirstName = FirstName,
            LastName = LastName,
            TerminationDate = terminationDate,
            TenantId = TenantId
        });
    }

    /// <summary>
    /// Add a skill to the employee
    /// </summary>
    public void AddSkill(string skillName, int proficiencyLevel)
    {
        if (Skills.Any(s => s.SkillName == skillName))
            return;

        Skills.Add(EmployeeSkill.Create(Id, skillName, proficiencyLevel, TenantId));
    }

    /// <summary>
    /// Remove a skill from the employee
    /// </summary>
    public void RemoveSkill(string skillName)
    {
        var skill = Skills.FirstOrDefault(s => s.SkillName == skillName);
        if (skill is not null)
        {
            Skills.Remove(skill);
        }
    }

    /// <summary>
    /// Full name of the employee
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Age of the employee
    /// </summary>
    public int Age => DateTime.Today.Year - DateOfBirth.Year;
}
