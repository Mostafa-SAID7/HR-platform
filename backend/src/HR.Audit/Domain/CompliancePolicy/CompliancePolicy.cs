namespace HR.Audit.Domain.CompliancePolicy;

/// <summary>
/// Compliance Policy - rules for what should be audited
/// </summary>
public class CompliancePolicy
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public List<string> AuditedEntities { get; set; } = []; // e.g., ["Employee", "Payroll", "Performance"]
    public List<string> CriticalActions { get; set; } = []; // e.g., ["Delete", "Approve", "Process"]
    public int RetentionDays { get; set; } = 2555; // ~7 years for compliance
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    private CompliancePolicy() { }

    /// <summary>
    /// Create a new compliance policy
    /// </summary>
    public static CompliancePolicy Create(
        string name,
        string description,
        List<string> auditedEntities,
        List<string> criticalActions,
        int retentionDays = 2555)
    {
        return new CompliancePolicy
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            AuditedEntities = auditedEntities,
            CriticalActions = criticalActions,
            RetentionDays = retentionDays,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Check if an entity/action combination should be audited
    /// </summary>
    public bool ShouldAudit(string entityType, string action)
    {
        return AuditedEntities.Contains(entityType) || CriticalActions.Contains(action);
    }

    /// <summary>
    /// Check if an action is critical
    /// </summary>
    public bool IsCriticalAction(string action)
    {
        return CriticalActions.Contains(action);
    }

    /// <summary>
    /// Update the policy
    /// </summary>
    public void Update(
        string description,
        List<string> auditedEntities,
        List<string> criticalActions,
        int retentionDays,
        bool isActive)
    {
        Description = description;
        AuditedEntities = auditedEntities;
        CriticalActions = criticalActions;
        RetentionDays = retentionDays;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }
}
