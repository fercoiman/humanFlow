using HumanFlow.Domain.Common;

namespace HumanFlow.Domain.Security;

public sealed class PermissionDefinition : Entity
{
    public required string Key { get; set; }
    public required string DisplayName { get; set; }
    public required string Module { get; set; }
}
