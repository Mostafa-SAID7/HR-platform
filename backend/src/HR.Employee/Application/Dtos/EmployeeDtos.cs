namespace HR.Employee.Application.Dtos;

/// <summary>
/// Create employee request DTO.
/// </summary>
public record CreateEmployeeRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public Guid DepartmentId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string EmploymentType { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
}

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

/// <summary>
/// Employee list DTO.
/// </summary>
public record EmployeeListDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
}

/// <summary>
/// Employee detail DTO.
/// </summary>
public record EmployeeDetailDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public Guid? ManagerId { get; set; }
    public string ManagerName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string EmploymentType { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public string Currency { get; set; } = "USD";
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public bool IsActive { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<EmployeeSkillDto> Skills { get; set; } = [];
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
}

/// <summary>
/// Employee skill DTO.
/// </summary>
public record EmployeeSkillDto
{
    public Guid Id { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public int ProficiencyLevel { get; set; }
}

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

/// <summary>
/// Terminate employee request DTO.
/// </summary>
public record TerminateEmployeeRequest
{
    public DateTime TerminationDate { get; set; }
    public string? Reason { get; set; }
}

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
