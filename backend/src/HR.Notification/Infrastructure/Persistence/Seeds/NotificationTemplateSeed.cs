namespace HR.Notification.Infrastructure.Persistence.Seeds;

using Microsoft.EntityFrameworkCore;
using HR.Notification.Domain;

/// <summary>
/// Seed data configuration for NotificationTemplate aggregate
/// </summary>
public static class NotificationTemplateSeed
{
    /// <summary>
    /// Seeds initial notification template data for development and testing
    /// </summary>
    public static void Seed(ModelBuilder modelBuilder)
    {
        var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var templates = new List<object>();

        // Employee notification templates
        templates.Add(CreateTemplate(Guid.NewGuid(), "EmployeeCreated", "New Employee Created", NotificationType.EmployeeCreated, 
            "Welcome to {{CompanyName}}", "Employee {{EmployeeName}} has been created in the system.", tenantId));

        templates.Add(CreateTemplate(Guid.NewGuid(), "PerformanceReviewDue", "Performance Review Due", NotificationType.PerformanceReviewDue,
            "Performance Review Reminder", "Your performance review for {{EmployeeName}} is due on {{DueDate}}.", tenantId));

        templates.Add(CreateTemplate(Guid.NewGuid(), "LeaveApproved", "Leave Request Approved", NotificationType.LeaveApproved,
            "Leave Approved", "Your leave request from {{StartDate}} to {{EndDate}} has been approved.", tenantId));

        templates.Add(CreateTemplate(Guid.NewGuid(), "LeaveRejected", "Leave Request Rejected", NotificationType.LeaveRejected,
            "Leave Rejected", "Your leave request from {{StartDate}} to {{EndDate}} has been rejected. Reason: {{Reason}}", tenantId));

        templates.Add(CreateTemplate(Guid.NewGuid(), "PayslipGenerated", "Payslip Generated", NotificationType.PayslipGenerated,
            "Your Payslip is Ready", "Your payslip for {{Month}} {{Year}} has been generated and is ready for download.", tenantId));

        // Recruitment notification templates
        templates.Add(CreateTemplate(Guid.NewGuid(), "InterviewScheduled", "Interview Scheduled", NotificationType.InterviewScheduled,
            "Interview Scheduled", "Your interview for {{Position}} is scheduled on {{InterviewDate}} at {{InterviewTime}}.", tenantId));

        templates.Add(CreateTemplate(Guid.NewGuid(), "OfferExtended", "Job Offer Extended", NotificationType.OfferExtended,
            "Congratulations!", "We are pleased to extend a job offer for {{Position}}. Please review the details and respond by {{ResponseDeadline}}.", tenantId));

        templates.Add(CreateTemplate(Guid.NewGuid(), "ApplicationRejected", "Application Status Update", NotificationType.ApplicationRejected,
            "Application Status", "Thank you for your interest in {{Position}}. Unfortunately, we have decided to move forward with another candidate.", tenantId));

        templates.Add(CreateTemplate(Guid.NewGuid(), "SystemAlert", "System Alert", NotificationType.SystemAlert,
            "System Alert", "{{AlertMessage}}", tenantId));

        modelBuilder.Entity<NotificationTemplate>().HasData(templates.ToArray());
    }

    private static object CreateTemplate(Guid id, string name, string description, NotificationType type, 
        string titleTemplate, string contentTemplate, Guid tenantId)
    {
        return new
        {
            Id = id,
            Name = name,
            Description = description,
            Type = type,
            TitleTemplate = titleTemplate,
            ContentTemplate = contentTemplate,
            VariableMappings = new Dictionary<string, string>(),
            IsActive = true,
            TenantId = tenantId,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        };
    }
}
