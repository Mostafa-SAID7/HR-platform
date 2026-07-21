namespace HR.Employee.Application.Mappings;

using Mapster;
using HR.Employee.Domain;
using HR.Employee.Application.Dtos.Employee;

/// <summary>
/// Mapping configuration for Employee DTOs
/// Centralizes all Mapster configurations for employee related mappings
/// </summary>
public class EmployeeMappingConfiguration : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Employee aggregate to DTO
        config.NewConfig<Domain.Employee, EmployeeDto>()
            .Map(dest => dest.Status, src => src.Status);

        // Employee aggregate to detailed DTO
        config.NewConfig<Domain.Employee, EmployeeDetailDto>()
            .Map(dest => dest.Status, src => src.Status);
    }
}
