namespace HR.Employee.Application.Mappings;

using Mapster;
using HR.Employee.Domain;
using HR.Employee.Application.Dtos.Department;

/// <summary>
/// Mapping configuration for Department DTOs
/// Centralizes all Mapster configurations for department related mappings
/// </summary>
public class DepartmentMappingConfiguration : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Department aggregate to DTO
        config.NewConfig<Department, DepartmentDto>();
    }
}
