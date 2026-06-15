using HumanFlow.Domain.Common;

namespace HumanFlow.Domain.Employees;

public sealed class SalaryHistory : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid EmployeeId { get; set; }

    /// <summary>Fecha desde la cual rige este sueldo.</summary>
    public DateOnly EffectiveDate { get; set; }

    /// <summary>Moneda en la que se pactó el sueldo (ARS o USD).</summary>
    public bool IsUSD { get; set; }

    /// <summary>Importe bruto en la moneda pactada.</summary>
    public decimal GrossAmount { get; set; }

    /// <summary>Tipo de cambio de referencia utilizado.</summary>
    public ExchangeRateType ExchangeRateType { get; set; } = ExchangeRateType.Oficial;

    /// <summary>Valor del tipo de cambio al momento del registro.</summary>
    public decimal ExchangeRate { get; set; }

    /// <summary>Equivalente en ARS, almacenado para inmutabilidad histórica.</summary>
    public decimal GrossAmountARS { get; set; }

    /// <summary>Equivalente en USD, almacenado para inmutabilidad histórica.</summary>
    public decimal GrossAmountUSD { get; set; }

    public SalaryChangeReason ChangeReason { get; set; } = SalaryChangeReason.Ingreso;

    public string? Notes { get; set; }
}
