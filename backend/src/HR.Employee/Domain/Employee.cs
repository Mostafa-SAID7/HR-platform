namespace HR.Employee.Domain;

/// <summary>
/// Employee aggregate root.
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
    public string EmploymentType { get; set; } = string.Empty; // Full-time, Part-time, Contract
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
    public string Status { get; set; } = "Active"; // Active, OnLeave, Terminated
    
    // Relations
    public Department? Department { get; set; }
    public ICollection<EmployeeSkill> Skills { get; set; } = new List<EmployeeSkill>();
    public ICollection<EmployeeEducation> Education { get; set; } = new List<EmployeeEducation>();

    /// <summary>
    /// Create a new employee.
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
        string employmentType,
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
            Status = "Active",
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
    /// Update employee information.
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
    /// Terminate employment.
    /// </summary>
    public void Terminate(DateTime terminationDate)
    {
        TerminationDate = terminationDate;
        Status = "Terminated";
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
    /// Add a skill to the employee.
    /// </summary>
    public void AddSkill(string skillName, int proficiencyLevel)
    {
        if (Skills.Any(s => s.SkillName == skillName))
            return;

        Skills.Add(new EmployeeSkill
        {
            Id = Guid.NewGuid(),
            EmployeeId = Id,
            SkillName = skillName,
            ProficiencyLevel = proficiencyLevel,
            TenantId = TenantId,
            CreatedOnUtc = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Remove a skill from the employee.
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
    /// Full name of the employee.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Age of the employee.
    /// </summary>
    public int Age => DateTime.Today.Year - DateOfBirth.Year;
}

/// <summary>
/// Department entity.
/// </summary>
public class Department : AggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? ManagerId { get; set; }
    public string Location { get; set; } = string.Empty;
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public static Department Create(string name, string description, string location, Guid tenantId)
    {
        return new Department
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Location = location,
            TenantId = tenantId,
            CreatedOnUtc = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Employee skill entity.
/// </summary>
public class EmployeeSkill : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public int ProficiencyLevel { get; set; } // 1-5
    public Employee? Employee { get; set; }
}

/// <summary>
/// Employee education entity.
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
}

// ===== DOMAIN EVENTS =====

/// <summary>
/// Domain event raised when an employee is created.
/// </summary>
public record EmployeeCreatedEvent : DomainEvent
{
    public Guid EmployeeId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Domain event raised when an employee is updated.
/// </summary>
public record EmployeeUpdatedEvent : DomainEvent
{
    public Guid EmployeeId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

/// <summary>
/// Domain event raised when an employee is terminated.
/// </summary>
public record EmployeeTerminatedEvent : DomainEvent
{
    public Guid EmployeeId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime TerminationDate { get; set; }
}
