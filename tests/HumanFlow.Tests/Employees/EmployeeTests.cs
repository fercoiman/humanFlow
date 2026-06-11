using HumanFlow.Domain.Employees;

namespace HumanFlow.Tests.Employees;

public sealed class EmployeeTests
{
    [Fact]
    public void full_name_combines_first_and_last_name()
    {
        var employee = new Employee
        {
            TenantId = Guid.NewGuid(),
            EmployeeNumber = "EMP-001",
            FirstName = "Maria",
            LastName = "Gomez"
        };

        Assert.Equal("Maria Gomez", employee.FullName);
    }
}
