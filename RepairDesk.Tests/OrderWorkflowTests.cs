using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RepairDesk.Data;
using RepairDesk.Domain;
using RepairDesk.Services;

namespace RepairDesk.Tests;

[NonParallelizable]
public sealed class OrderWorkflowTests
{
    private string _databasePath = null!;
    private AccessService _access = null!;
    private OrderService _orders = null!;
    private InventoryService _inventory = null!;

    [SetUp]
    public void SetUp()
    {
        _databasePath = Path.Combine(Path.GetTempPath(), $"repairdesk-test-{Guid.NewGuid():N}.db");
        Environment.SetEnvironmentVariable("REPAIRDESK_DB_PATH", _databasePath);
        using var db = new RepairDbContext();
        db.Database.EnsureCreated();
        _access = new AccessService();
        _orders = new OrderService(_access);
        _inventory = new InventoryService(_access);
    }

    [TearDown]
    public void TearDown()
    {
        Environment.SetEnvironmentVariable("REPAIRDESK_DB_PATH", null);
        SqliteConnection.ClearAllPools();
        foreach (var suffix in new[] { "", "-wal", "-shm" })
        {
            var path = _databasePath + suffix;
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Test]
    public void ApprovalMovesOrderToRepair()
    {
        var order = CreateDiagnosedOrder();
        _access.CurrentRole = UserRole.Receptionist;

        _orders.Approve(order.Id);

        var saved = _orders.GetOrders().Single(x => x.Id == order.Id);
        Assert.Multiple(() =>
        {
            Assert.That(saved.Status, Is.EqualTo(OrderStatus.InRepair));
            Assert.That(saved.IsApproved, Is.True);
        });
    }

    [Test]
    public void RefusalAllowsIssueWithoutRepair()
    {
        var order = CreateDiagnosedOrder();
        _access.CurrentRole = UserRole.Receptionist;

        _orders.Decline(order.Id);
        _orders.Issue(order.Id);

        var saved = _orders.GetOrders().Single(x => x.Id == order.Id);
        Assert.Multiple(() =>
        {
            Assert.That(saved.Status, Is.EqualTo(OrderStatus.Issued));
            Assert.That(saved.IsApproved, Is.False);
        });
    }

    [Test]
    public void ApprovalRequiresDiagnosis()
    {
        var order = _orders.Create("Клиент", "+70000000000", "Ноутбук", "Модель", "SN", "Не включается");

        Assert.Throws<InvalidOperationException>(() => _orders.Approve(order.Id));
    }

    [Test]
    public void TechnicianCannotCreateOrder()
    {
        _access.CurrentRole = UserRole.Technician;

        Assert.Throws<InvalidOperationException>(() =>
            _orders.Create("Клиент", "+70000000000", "ПК", "Модель", "SN", "Проблема"));
    }

    [Test]
    public void PartUsageUpdatesStockAndTotalAtomically()
    {
        var order = CreateDiagnosedOrder();
        _orders.Approve(order.Id);
        _access.CurrentRole = UserRole.Administrator;
        _inventory.AddPart("SSD", "SSD-01", 2, 200);
        var part = _inventory.GetParts().Single();
        _access.CurrentRole = UserRole.Technician;
        _orders.AddWork(order.Id, "Замена SSD", 500);

        _inventory.UsePart(order.Id, part.Id, 1);
        Assert.Throws<InvalidOperationException>(() => _inventory.UsePart(order.Id, part.Id, 2));

        Assert.Multiple(() =>
        {
            Assert.That(_inventory.GetParts().Single().Stock, Is.EqualTo(1));
            Assert.That(_orders.GetOrders().Single().Total, Is.EqualTo(700));
        });
    }

    private RepairOrder CreateDiagnosedOrder()
    {
        var order = _orders.Create("Клиент", "+70000000000", "Ноутбук", "Модель", "SN", "Не включается");
        _access.CurrentRole = UserRole.Technician;
        _orders.SaveDiagnosis(order.Id, "Неисправен накопитель", "Замена SSD");
        _access.CurrentRole = UserRole.Receptionist;
        return order;
    }
}
