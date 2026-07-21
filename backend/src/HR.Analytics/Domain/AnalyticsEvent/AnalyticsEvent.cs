namespace HR.Analytics.Domain.AnalyticsEvent;

/// <summary>
/// Analytics event for data warehouse ingestion.
/// Aggregate root for tracking domain events from all services.
/// </summary>
public class AnalyticsEvent : AggregateRoot
{
    public string EventType { get; set; } = string.Empty; // EmployeeCreated, PayrollProcessed, etc.
    public string EntityType { get; set; } = string.Empty; // Employee, Payroll, etc.
    public Guid EntityId { get; set; }
    public Dictionary<string, object> EventData { get; set; } = new();
    public DateTime EventTimestamp { get; set; }
    public bool SyncedToElasticsearch { get; set; }
    public DateTime? ElasticsearchSyncTime { get; set; }
}
