namespace HR.Employee.Features.GetEmployeeById;

using MediatR;
using HR.Employee.Domain.Employee;
using HR.Employee.Application.Dtos.Employee;
using HR.Common.Domain.Exceptions;
using HR.Common.Application.Abstractions;

/// <summary>
/// Handler for GetEmployeeByIdQuery.
/// </summary>
public class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, EmployeeDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetEmployeeByIdQueryHandler> _logger;

    public GetEmployeeByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetEmployeeByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<EmployeeDetailDto> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
    {
        var employeeRepository = _unitOfWork.GetRepository<Employee>();
        var employee = await employeeRepository.GetAsQueryable()
            .Include(e => e.Department)
            .Include(e => e.Skills)
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId && e.TenantId == request.TenantId, cancellationToken);

        if (employee is null)
        {
            _logger.LogWarning("Employee not found: {EmployeeId}", request.EmployeeId);
            throw new NotFoundException("Employee", request.EmployeeId);
        }

        return MapToDetailDto(employee);
    }

    private static EmployeeDetailDto MapToDetailDto(Employee employee)
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
            DepartmentName = employee.Department?.Name ?? string.Empty,
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
