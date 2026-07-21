namespace HR.Tests.Unit.Employee;

/// <summary>
/// Unit tests for Employee domain aggregate and commands.
/// Tests cover: creation, updates, termination, skills, and validation.
/// </summary>
public class EmployeeCommandTests
{
    private readonly Guid _departmentId = Guid.NewGuid();
    private readonly Guid _tenantId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidParameters_CreatesEmployee()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var email = "john@example.com";
        var phoneNumber = "1234567890";
        var dateOfBirth = DateTime.Now.AddYears(-30);
        var gender = "Male";
        var nationalId = "12345678";
        var hireDate = DateTime.Now.AddYears(-5);
        var jobTitle = "Software Engineer";
        var employmentType = "Full-time";
        var salary = 100000m;

        // Act
        var employee = HR.Employee.Domain.Employee.Create(
            firstName, lastName, email, phoneNumber, dateOfBirth, gender, nationalId,
            hireDate, _departmentId, jobTitle, employmentType, salary, _tenantId);

        // Assert
        Assert.NotNull(employee);
        Assert.NotEqual(Guid.Empty, employee.Id);
        Assert.Equal(firstName, employee.FirstName);
        Assert.Equal(lastName, employee.LastName);
        Assert.Equal(email, employee.Email);
        Assert.Equal(phoneNumber, employee.PhoneNumber);
        Assert.Equal(gender, employee.Gender);
        Assert.Equal(nationalId, employee.NationalId);
        Assert.Equal(hireDate, employee.HireDate);
        Assert.Equal(_departmentId, employee.DepartmentId);
        Assert.Equal(jobTitle, employee.JobTitle);
        Assert.Equal(employmentType, employee.EmploymentType);
        Assert.Equal(salary, employee.Salary);
        Assert.True(employee.IsActive);
        Assert.Equal("Active", employee.Status);
        Assert.Equal(_tenantId, employee.TenantId);
    }

    [Fact]
    public void Create_PublishesEmployeeCreatedEvent()
    {
        // Arrange
        var dateOfBirth = DateTime.Now.AddYears(-30);
        var hireDate = DateTime.Now.AddYears(-5);

        // Act
        var employee = HR.Employee.Domain.Employee.Create(
            "John", "Doe", "john@example.com", "1234567890", dateOfBirth,
            "Male", "12345678", hireDate, _departmentId, "Engineer", "Full-time", 100000m, _tenantId);

        // Assert
        Assert.NotEmpty(employee.DomainEvents);
        var createdEvent = employee.DomainEvents.OfType<EmployeeCreatedEvent>().FirstOrDefault();
        Assert.NotNull(createdEvent);
        Assert.Equal(employee.Id, createdEvent.EmployeeId);
        Assert.Equal("John", createdEvent.FirstName);
        Assert.Equal("Doe", createdEvent.LastName);
        Assert.Equal("john@example.com", createdEvent.Email);
    }

    [Fact]
    public void Update_WithValidData_UpdatesEmployee()
    {
        // Arrange
        var dateOfBirth = DateTime.Now.AddYears(-30);
        var hireDate = DateTime.Now.AddYears(-5);
        var employee = HR.Employee.Domain.Employee.Create(
            "John", "Doe", "john@example.com", "1234567890", dateOfBirth,
            "Male", "12345678", hireDate, _departmentId, "Engineer", "Full-time", 100000m, _tenantId);

        var newDepartmentId = Guid.NewGuid();
        var newManagerId = Guid.NewGuid();

        // Act
        employee.Update("Jane", "Smith", "9876543210", "Senior Engineer", newDepartmentId, 120000m, newManagerId);

        // Assert
        Assert.Equal("Jane", employee.FirstName);
        Assert.Equal("Smith", employee.LastName);
        Assert.Equal("9876543210", employee.PhoneNumber);
        Assert.Equal("Senior Engineer", employee.JobTitle);
        Assert.Equal(newDepartmentId, employee.DepartmentId);
        Assert.Equal(120000m, employee.Salary);
        Assert.Equal(newManagerId, employee.ManagerId);
    }

    [Fact]
    public void Update_PublishesEmployeeUpdatedEvent()
    {
        // Arrange
        var dateOfBirth = DateTime.Now.AddYears(-30);
        var hireDate = DateTime.Now.AddYears(-5);
        var employee = HR.Employee.Domain.Employee.Create(
            "John", "Doe", "john@example.com", "1234567890", dateOfBirth,
            "Male", "12345678", hireDate, _departmentId, "Engineer", "Full-time", 100000m, _tenantId);

        // Act
        employee.Update("Jane", "Smith", "9876543210", "Senior Engineer", _departmentId, 120000m);

        // Assert
        var updatedEvent = employee.DomainEvents.OfType<EmployeeUpdatedEvent>().FirstOrDefault();
        Assert.NotNull(updatedEvent);
        Assert.Equal(employee.Id, updatedEvent.EmployeeId);
        Assert.Equal("Jane", updatedEvent.FirstName);
        Assert.Equal("Smith", updatedEvent.LastName);
    }

    [Fact]
    public void Terminate_WithValidDate_TerminatesEmployee()
    {
        // Arrange
        var dateOfBirth = DateTime.Now.AddYears(-30);
        var hireDate = DateTime.Now.AddYears(-5);
        var employee = HR.Employee.Domain.Employee.Create(
            "John", "Doe", "john@example.com", "1234567890", dateOfBirth,
            "Male", "12345678", hireDate, _departmentId, "Engineer", "Full-time", 100000m, _tenantId);

        Assert.True(employee.IsActive);
        Assert.Equal("Active", employee.Status);

        var terminationDate = DateTime.Now;

        // Act
        employee.Terminate(terminationDate);

        // Assert
        Assert.False(employee.IsActive);
        Assert.Equal("Terminated", employee.Status);
        Assert.Equal(terminationDate, employee.TerminationDate);
    }

    [Fact]
    public void Terminate_PublishesEmployeeTerminatedEvent()
    {
        // Arrange
        var dateOfBirth = DateTime.Now.AddYears(-30);
        var hireDate = DateTime.Now.AddYears(-5);
        var employee = HR.Employee.Domain.Employee.Create(
            "John", "Doe", "john@example.com", "1234567890", dateOfBirth,
            "Male", "12345678", hireDate, _departmentId, "Engineer", "Full-time", 100000m, _tenantId);

        var terminationDate = DateTime.Now;

        // Act
        employee.Terminate(terminationDate);

        // Assert
        var terminatedEvent = employee.DomainEvents.OfType<EmployeeTerminatedEvent>().FirstOrDefault();
        Assert.NotNull(terminatedEvent);
        Assert.Equal(employee.Id, terminatedEvent.EmployeeId);
        Assert.Equal("John", terminatedEvent.FirstName);
        Assert.Equal("Doe", terminatedEvent.LastName);
        Assert.Equal(terminationDate, terminatedEvent.TerminationDate);
    }

    [Fact]
    public void AddSkill_WithValidSkill_AddsSkillToEmployee()
    {
        // Arrange
        var dateOfBirth = DateTime.Now.AddYears(-30);
        var hireDate = DateTime.Now.AddYears(-5);
        var employee = HR.Employee.Domain.Employee.Create(
            "John", "Doe", "john@example.com", "1234567890", dateOfBirth,
            "Male", "12345678", hireDate, _departmentId, "Engineer", "Full-time", 100000m, _tenantId);

        // Act
        employee.AddSkill("C#", 5);

        // Assert
        Assert.Single(employee.Skills);
        var skill = employee.Skills.First();
        Assert.Equal("C#", skill.SkillName);
        Assert.Equal(5, skill.ProficiencyLevel);
    }

    [Fact]
    public void AddSkill_WithDuplicateSkill_DoesNotDuplicate()
    {
        // Arrange
        var dateOfBirth = DateTime.Now.AddYears(-30);
        var hireDate = DateTime.Now.AddYears(-5);
        var employee = HR.Employee.Domain.Employee.Create(
            "John", "Doe", "john@example.com", "1234567890", dateOfBirth,
            "Male", "12345678", hireDate, _departmentId, "Engineer", "Full-time", 100000m, _tenantId);

        // Act
        employee.AddSkill("C#", 5);
        employee.AddSkill("C#", 4); // Attempt to add same skill

        // Assert
        Assert.Single(employee.Skills);
        Assert.Equal(5, employee.Skills.First().ProficiencyLevel); // Original proficiency
    }

    [Fact]
    public void AddSkill_WithMultipleSkills_AddsAllSkills()
    {
        // Arrange
        var dateOfBirth = DateTime.Now.AddYears(-30);
        var hireDate = DateTime.Now.AddYears(-5);
        var employee = HR.Employee.Domain.Employee.Create(
            "John", "Doe", "john@example.com", "1234567890", dateOfBirth,
            "Male", "12345678", hireDate, _departmentId, "Engineer", "Full-time", 100000m, _tenantId);

        // Act
        employee.AddSkill("C#", 5);
        employee.AddSkill("SQL", 4);
        employee.AddSkill("JavaScript", 3);

        // Assert
        Assert.Equal(3, employee.Skills.Count);
        Assert.Contains(employee.Skills, s => s.SkillName == "C#");
        Assert.Contains(employee.Skills, s => s.SkillName == "SQL");
        Assert.Contains(employee.Skills, s => s.SkillName == "JavaScript");
    }

    [Fact]
    public void RemoveSkill_WithExistingSkill_RemovesSkill()
    {
        // Arrange
        var dateOfBirth = DateTime.Now.AddYears(-30);
        var hireDate = DateTime.Now.AddYears(-5);
        var employee = HR.Employee.Domain.Employee.Create(
            "John", "Doe", "john@example.com", "1234567890", dateOfBirth,
            "Male", "12345678", hireDate, _departmentId, "Engineer", "Full-time", 100000m, _tenantId);

        employee.AddSkill("C#", 5);
        employee.AddSkill("SQL", 4);
        Assert.Equal(2, employee.Skills.Count);

        // Act
        employee.RemoveSkill("C#");

        // Assert
        Assert.Single(employee.Skills);
        Assert.Equal("SQL", employee.Skills.First().SkillName);
    }

    [Fact]
    public void RemoveSkill_WithNonExistentSkill_DoesNothing()
    {
        // Arrange
        var dateOfBirth = DateTime.Now.AddYears(-30);
        var hireDate = DateTime.Now.AddYears(-5);
        var employee = HR.Employee.Domain.Employee.Create(
            "John", "Doe", "john@example.com", "1234567890", dateOfBirth,
            "Male", "12345678", hireDate, _departmentId, "Engineer", "Full-time", 100000m, _tenantId);

        employee.AddSkill("C#", 5);

        // Act & Assert - Should not throw
        employee.RemoveSkill("NonExistent");
        Assert.Single(employee.Skills); // Original skill still there
    }

    [Fact]
    public void FullName_ReturnsCorrectFormat()
    {
        // Arrange
        var dateOfBirth = DateTime.Now.AddYears(-30);
        var hireDate = DateTime.Now.AddYears(-5);
        var employee = HR.Employee.Domain.Employee.Create(
            "John", "Doe", "john@example.com", "1234567890", dateOfBirth,
            "Male", "12345678", hireDate, _departmentId, "Engineer", "Full-time", 100000m, _tenantId);

        // Act
        var fullName = employee.FullName;

        // Assert
        Assert.Equal("John Doe", fullName);
    }

    [Fact]
    public void Age_CalculatesCorrectly()
    {
        // Arrange
        var dateOfBirth = DateTime.Now.AddYears(-30);
        var hireDate = DateTime.Now.AddYears(-5);
        var employee = HR.Employee.Domain.Employee.Create(
            "John", "Doe", "john@example.com", "1234567890", dateOfBirth,
            "Male", "12345678", hireDate, _departmentId, "Engineer", "Full-time", 100000m, _tenantId);

        // Act
        var age = employee.Age;

        // Assert
        Assert.Equal(30, age);
    }
}
