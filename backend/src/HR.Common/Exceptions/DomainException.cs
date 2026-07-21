namespace HR.Common.Exceptions;

/// <summary>
/// Base exception for domain-level errors.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when a resource is not found.
/// </summary>
public class NotFoundException : DomainException
{
    public NotFoundException(string resourceName, object key) 
        : base($"{resourceName} with identifier {key} was not found.")
    {
    }

    public NotFoundException(string message) : base(message)
    {
    }
}

/// <summary>
/// Exception thrown when validation fails.
/// </summary>
public class ValidationException : DomainException
{
    public ValidationException(string message) : base(message)
    {
    }

    public ValidationException(IDictionary<string, string[]> failures)
        : base("One or more validation failures have occurred.")
    {
        Failures = failures;
    }

    public IDictionary<string, string[]> Failures { get; } = new Dictionary<string, string[]>();
}

/// <summary>
/// Exception thrown when an operation is not allowed.
/// </summary>
public class ForbiddenException : DomainException
{
    public ForbiddenException(string message) : base(message)
    {
    }
}

/// <summary>
/// Exception thrown when a business rule is violated.
/// </summary>
public class BusinessRuleViolationException : DomainException
{
    public BusinessRuleViolationException(string ruleName, string message) 
        : base($"Business rule violation: {ruleName}. {message}")
    {
        RuleName = ruleName;
    }

    public string RuleName { get; }
}

/// <summary>
/// Exception thrown when a resource already exists.
/// </summary>
public class AlreadyExistsException : DomainException
{
    public AlreadyExistsException(string resourceName, object key) 
        : base($"{resourceName} with identifier {key} already exists.")
    {
    }

    public AlreadyExistsException(string message) : base(message)
    {
    }
}

/// <summary>
/// Exception thrown when an operation fails due to concurrency conflict.
/// </summary>
public class ConcurrencyException : DomainException
{
    public ConcurrencyException(string message) : base(message)
    {
    }

    public ConcurrencyException() : base("The operation failed due to a concurrency conflict.")
    {
    }
}
