using Microsoft.EntityFrameworkCore;
using RepairDesk.Data;
using RepairDesk.Domain;

namespace RepairDesk.Services;

public sealed class OrderService(AccessService access)
{
    public List<RepairOrder> GetOrders()
    {
        using var db = new RepairDbContext();
        return db.Orders.AsNoTracking().Include(x => x.Device).ThenInclude(x => x.Customer)
            .Include(x => x.Works).Include(x => x.Parts).ThenInclude(x => x.SparePart)
            .OrderByDescending(x => x.Id).ToList();
    }

    public RepairOrder Create(string customer, string phone, string type, string model, string serial, string problem)
    {
        access.Require(UserRole.Receptionist, UserRole.Administrator);
        if (string.IsNullOrWhiteSpace(customer) || string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(problem))
            throw new ArgumentException("Укажите клиента, тип устройства и описание неисправности.");
        using var db = new RepairDbContext();
        var entity = new Customer { FullName = customer.Trim(), Phone = phone.Trim() };
        var device = new Device { Customer = entity, Type = type.Trim(), Model = model.Trim(), SerialNumber = serial.Trim() };
        var order = new RepairOrder { Device = device, Problem = problem.Trim() };
        db.Orders.Add(order);
        db.SaveChanges();
        return order;
    }

    public void SaveDiagnosis(int id, string diagnosis, string plan)
    {
        access.Require(UserRole.Technician, UserRole.Administrator);
        if (string.IsNullOrWhiteSpace(diagnosis) || string.IsNullOrWhiteSpace(plan))
            throw new ArgumentException("Укажите результат диагностики и план работ.");
        using var db = new RepairDbContext();
        var order = db.Orders.Find(id) ?? throw new InvalidOperationException("Заказ не найден.");
        if (order.Status is OrderStatus.Ready or OrderStatus.Issued or OrderStatus.Declined)
            throw new InvalidOperationException("Для завершённого заказа диагностика закрыта.");
        order.Diagnosis = diagnosis.Trim();
        order.WorkPlan = plan.Trim();
        order.Status = OrderStatus.AwaitingApproval;
        db.SaveChanges();
    }

    public void Approve(int id)
    {
        access.Require(UserRole.Receptionist, UserRole.Administrator);
        using var db = new RepairDbContext();
        var order = db.Orders.Find(id) ?? throw new InvalidOperationException("Заказ не найден.");
        if (order.Status != OrderStatus.AwaitingApproval)
            throw new InvalidOperationException("Сначала сохраните диагностику и план работ.");
        order.IsApproved = true;
        order.Status = OrderStatus.InRepair;
        db.SaveChanges();
    }

    public void Decline(int id)
    {
        access.Require(UserRole.Receptionist, UserRole.Administrator);
        using var db = new RepairDbContext();
        var order = db.Orders.Find(id) ?? throw new InvalidOperationException("Заказ не найден.");
        if (order.Status != OrderStatus.AwaitingApproval)
            throw new InvalidOperationException("Сначала сохраните диагностику и план работ.");
        order.IsApproved = false;
        order.Status = OrderStatus.Declined;
        db.SaveChanges();
    }

    public void SetReady(int id)
    {
        access.Require(UserRole.Technician, UserRole.Administrator);
        using var db = new RepairDbContext();
        var order = db.Orders.Find(id) ?? throw new InvalidOperationException("Заказ не найден.");
        if (order.Status != OrderStatus.InRepair)
            throw new InvalidOperationException("Закончить можно только согласованный ремонт.");
        order.Status = OrderStatus.Ready;
        db.SaveChanges();
    }

    public void Issue(int id)
    {
        access.Require(UserRole.Receptionist, UserRole.Administrator);
        using var db = new RepairDbContext();
        var order = db.Orders.Find(id) ?? throw new InvalidOperationException("Заказ не найден.");
        if (order.Status is not (OrderStatus.Ready or OrderStatus.Declined))
            throw new InvalidOperationException("Выдать можно готовое устройство или устройство после отказа от ремонта.");
        order.Status = OrderStatus.Issued;
        db.SaveChanges();
    }

    public void AddWork(int id, string description, decimal price)
    {
        access.Require(UserRole.Technician, UserRole.Administrator);
        if (string.IsNullOrWhiteSpace(description) || price < 0)
            throw new ArgumentException("Укажите работу и неотрицательную цену.");
        using var db = new RepairDbContext();
        var order = db.Orders.Find(id) ?? throw new InvalidOperationException("Заказ не найден.");
        if (order.Status != OrderStatus.InRepair)
            throw new InvalidOperationException("Работы добавляются после согласования ремонта.");
        db.Works.Add(new RepairWork { RepairOrderId = id, Description = description.Trim(), Price = price });
        db.SaveChanges();
    }
}
