namespace HR.Employee.Features.UpdateEmployee;

using MediatR;
using HR.Employee.Domain.Employee;
using HR.Employee.Domain.Department;
using HR.Employee.Application.Dtos.Employee;
using HR.Common.Domain.Exceptions;
using HR.Common.Application.Abstractions;

/// <summary>
/// Handler for UpdateEmployeeCommand.
/// </summary>
public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, EmployeeDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateEmployeeCommandHandler> _logger;

    public UpdateEmployeeCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpdateEmployeeCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<EmployeeDetailDto> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
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

        // Verify department exists
        var departmentRepository = _unitOfWork.GetRepository<Department>();
        var department = await departmentRepository.GetByIdAsync(request.Request.DepartmentId, cancellationToken);
        if (department is null)
        {
            _logger.LogWarning("Department not found: {DepartmentId}", request.Request.DepartmentId);
            throw new NotFoundException("Department", request.Request.DepartmentId);
        }

        // Update employee
        employee.Update(
            firstName: request.Request.FirstName,
            lastName: request.Request.LastName,
            phoneNumber: request.Request.PhoneNumber,
            jobTitle: request.Request.JobTitle,
            departmentId: request.Request.DepartmentId,
            salary: request.Request.Salary,
            managerId: request.Request.ManagerId);

        employeeRepository.Update(employee);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Employee updated: {EmployeeId} ({FirstName} {LastName})",
            request.EmployeeId, request.Request.FirstName, request.Request.LastName);

        return MapToDetailDto(employee, department);
    }

    private static EmployeeDetailDto MapToDetailDto(Employee employee, Department department)
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
