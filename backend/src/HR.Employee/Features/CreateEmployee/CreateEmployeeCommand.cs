namespace HR.Employee.Features.CreateEmployee;

using HR.Employee.Application.Dtos;

/// <summary>
/// Command to create a new employee.
/// </summary>
public record CreateEmployeeCommand(CreateEmployeeRequest Request, Guid TenantId) : ICommand<EmployeeDetailDto>;

/// <summary>
/// Validator for CreateEmployeeCommand.
/// </summary>
public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(x => x.Request.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.Request.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.Request.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Request.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^\d{10,}$").WithMessage("Phone number must have at least 10 digits");

        RuleFor(x => x.Request.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required")
            .LessThan(DateTime.Today.AddYears(-18)).WithMessage("Employee must be at least 18 years old");

        RuleFor(x => x.Request.Gender)
            .NotEmpty().WithMessage("Gender is required")
            .Must(g => g is "Male" or "Female" or "Other").WithMessage("Invalid gender value");

        RuleFor(x => x.Request.NationalId)
            .NotEmpty().WithMessage("National ID is required")
            .MaximumLength(50).WithMessage("National ID must not exceed 50 characters");

        RuleFor(x => x.Request.HireDate)
            .NotEmpty().WithMessage("Hire date is required")
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Hire date cannot be in the future");

        RuleFor(x => x.Request.DepartmentId)
            .NotEmpty().WithMessage("Department is required");

        RuleFor(x => x.Request.JobTitle)
            .NotEmpty().WithMessage("Job title is required")
            .MaximumLength(256).WithMessage("Job title must not exceed 256 characters");

        RuleFor(x => x.Request.EmploymentType)
            .NotEmpty().WithMessage("Employment type is required")
            .Must(et => et is "Full-time" or "Part-time" or "Contract")
            .WithMessage("Invalid employment type");

        RuleFor(x => x.Request.Salary)
            .GreaterThan(0).WithMessage("Salary must be greater than 0");
    }
}

/// <summary>
/// Handler for CreateEmployeeCommand.
/// </summary>
public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, EmployeeDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateEmployeeCommandHandler> _logger;

    public CreateEmployeeCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateEmployeeCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<EmployeeDetailDto> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;

        // Verify department exists
        var departmentRepository = _unitOfWork.GetRepository<Department>();
        var department = await departmentRepository.GetByIdAsync(req.DepartmentId, cancellationToken);
        if (department is null)
        {
            _logger.LogWarning("Department not found: {DepartmentId}", req.DepartmentId);
            throw new NotFoundException("Department", req.DepartmentId);
        }

        // Check if employee with same email already exists
        var employeeRepository = _unitOfWork.GetRepository<HR.Employee.Domain.Employee>();
        var existingEmployee = await employeeRepository.GetAsQueryable()
            .FirstOrDefaultAsync(e => e.Email == req.Email && e.TenantId == request.TenantId, cancellationToken);

        if (existingEmployee is not null)
        {
            _logger.LogWarning("Employee with email {Email} already exists", req.Email);
            throw new AlreadyExistsException("Employee", req.Email);
        }

        // Create new employee
        var employee = HR.Employee.Domain.Employee.Create(
            firstName: req.FirstName,
            lastName: req.LastName,
            email: req.Email,
            phoneNumber: req.PhoneNumber,
            dateOfBirth: req.DateOfBirth,
            gender: req.Gender,
            nationalId: req.NationalId,
            hireDate: req.HireDate,
            departmentId: req.DepartmentId,
            jobTitle: req.JobTitle,
            employmentType: req.EmploymentType,
            salary: req.Salary,
            tenantId: request.TenantId);

        employee.Address = req.Address;
        employee.City = req.City;
        employee.Country = req.Country;
        employee.PostalCode = req.PostalCode;

        await employeeRepository.AddAsync(employee, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Employee created: {FirstName} {LastName} ({Email})", 
            req.FirstName, req.LastName, req.Email);

        return await MapToDetailDto(employee, department);
    }

    private async Task<EmployeeDetailDto> MapToDetailDto(HR.Employee.Domain.Employee employee, Department department)
    {
        return new EmployeeDetailDto
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            PhoneNumber = employee.PhoneNumber,
            DateOfBirth = employee.DateOfBirth,
            Gender = employee.Gender,
            NationalId = employee.NationalId,
            HireDate = employee.HireDate,
            TerminationDate = employee.TerminationDate,
            DepartmentId = employee.DepartmentId,
            DepartmentName = department.Name,
            ManagerId = employee.ManagerId,
            JobTitle = employee.JobTitle,
            EmploymentType = employee.EmploymentType,
            Salary = employee.Salary,
            Currency = employee.Currency,
            Address = employee.Address,
            City = employee.City,
            Country = employee.Country,
            PostalCode = employee.PostalCode,
            ProfileImageUrl = employee.ProfileImageUrl,
            IsActive = employee.IsActive,
            Status = employee.Status,
            Skills = employee.Skills.Select(s => new EmployeeSkillDto
            {
                Id = s.Id,
                SkillName = s.SkillName,
                ProficiencyLevel = s.ProficiencyLevel
            }).ToList(),
            CreatedOnUtc = employee.CreatedOnUtc,
            UpdatedOnUtc = employee.UpdatedOnUtc
        };
    }
}
