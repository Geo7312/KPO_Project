using RepairDesk.Data;
using RepairDesk.Domain;

namespace RepairDesk.Services;

public sealed class SummaryService
{
    public string GetSummary()
    {
        using var db = new RepairDbContext();
        var orders = db.Orders.ToList();
        var workSum = db.Works.ToList().Sum(x => x.Price);
        var partSum = db.PartUsages.ToList().Sum(x => x.Quantity * x.UnitPrice);
        return $"Всего заказов: {orders.Count}\nВ работе: {orders.Count(x => x.Status == OrderStatus.InRepair)}\nГотовы к выдаче: {orders.Count(x => x.Status == OrderStatus.Ready)}\nОтказались от ремонта: {orders.Count(x => x.Status == OrderStatus.Declined)}\nВыданы: {orders.Count(x => x.Status == OrderStatus.Issued)}\nСтоимость работ и деталей: {workSum + partSum:N2} ₽";
    }
}
