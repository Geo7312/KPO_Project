using RepairDesk.Domain;

namespace RepairDesk.Services;

public sealed class AccessService
{
    public UserRole CurrentRole { get; set; } = UserRole.Receptionist;

    public void Require(params UserRole[] allowed)
    {
        if (!allowed.Contains(CurrentRole))
            throw new InvalidOperationException("Для выбранной роли операция недоступна.");
    }
}
