using Microsoft.EntityFrameworkCore;
using RepairDesk.Data;
using RepairDesk.Domain;

namespace RepairDesk.Services;

public sealed class InventoryService(AccessService access)
{
    public List<SparePart> GetParts()
    {
        using var db = new RepairDbContext();
        return db.SpareParts.AsNoTracking().OrderBy(x => x.Name).ToList();
    }

    public void AddPart(string name, string article, int stock, decimal price)
    {
        access.Require(UserRole.Administrator);
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(article) || stock < 0 || price < 0)
            throw new ArgumentException("Укажите наименование, артикул, неотрицательные остаток и цену.");
        using var db = new RepairDbContext();
        if (db.SpareParts.Any(x => x.Article == article.Trim()))
            throw new InvalidOperationException("Запчасть с таким артикулом уже существует.");
        db.SpareParts.Add(new SparePart { Name = name.Trim(), Article = article.Trim(), Stock = stock, Price = price });
        db.SaveChanges();
    }

    public void UsePart(int orderId, int partId, int quantity)
    {
        access.Require(UserRole.Technician, UserRole.Administrator);
        if (quantity <= 0) throw new ArgumentException("Количество должно быть больше нуля.");
        using var db = new RepairDbContext();
        using var transaction = db.Database.BeginTransaction();
        var order = db.Orders.Find(orderId) ?? throw new InvalidOperationException("Заказ не найден.");
        var part = db.SpareParts.Find(partId) ?? throw new InvalidOperationException("Запчасть не найдена.");
        if (order.Status != OrderStatus.InRepair)
            throw new InvalidOperationException("Списание возможно только во время ремонта.");
        if (part.Stock < quantity) throw new InvalidOperationException("Недостаточно деталей на складе.");
        part.Stock -= quantity;
        db.PartUsages.Add(new PartUsage { RepairOrderId = orderId, SparePartId = partId, Quantity = quantity, UnitPrice = part.Price });
        db.SaveChanges();
        transaction.Commit();
    }
}
