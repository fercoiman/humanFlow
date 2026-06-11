using HumanFlow.Domain.Common;

namespace HumanFlow.Domain.Employees;

public sealed class EmployeeDocument : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid EmployeeId { get; set; }
    public required string DocumentType { get; set; }
    public required string FileName { get; set; }
    public required string StorageKey { get; set; }
    public string? ContentType { get; set; }
    public long? SizeInBytes { get; set; }
}
