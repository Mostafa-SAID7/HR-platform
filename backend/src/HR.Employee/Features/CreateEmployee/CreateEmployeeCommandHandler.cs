namespace HR.Employee.Features.CreateEmployee;

using MediatR;
using HR.Employee.Domain.Employee;
using HR.Employee.Domain.Department;
using HR.Employee.Application.Dtos.Employee;
using HR.Common.Domain.Exceptions;
using HR.Common.Application.Abstractions;

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
        var employeeRepository = _unitOfWork.GetRepository<Employee>();
        var existingEmployee = await employeeRepository.GetAsQueryable()
            .FirstOrDefaultAsync(e => e.Email == req.Email && e.TenantId == request.TenantId, cancellationToken);

        if (existingEmployee is not null)
        {
            _logger.LogWarning("Employee with email {Email} already exists", req.Email);
            throw new AlreadyExistsException("Employee", req.Email);
        }

        // Create new employee
        var employee = Employee.Create(
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

    private async Task<EmployeeDetailDto> MapToDetailDto(Employee employee, Department department)
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
