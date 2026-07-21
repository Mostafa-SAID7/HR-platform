namespace HR.Audit.Application.Dtos.ChangeLog;

public record ChangeLogDto(
    DateTime Timestamp,
    string EntityType,
    string Action,
    string? UserEmail,
    Dictionary<string, object>? Changes);
