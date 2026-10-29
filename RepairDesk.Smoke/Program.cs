using Microsoft.EntityFrameworkCore;
using RepairDesk.Data;
using RepairDesk.Domain;
using RepairDesk.Services;

var databasePath = Path.Combine(AppContext.BaseDirectory, "smoke.db");
foreach (var file in new[] { databasePath, databasePath + "-wal", databasePath + "-shm" })
    if (File.Exists(file)) File.Delete(file);
Environment.SetEnvironmentVariable("REPAIRDESK_DB_PATH", databasePath);

using (var db = new RepairDbContext()) db.Database.EnsureCreated();
var access = new AccessService();
var orders = new OrderService(access);
var inventory = new InventoryService(access);
var order = orders.Create("Тестовый клиент", "+70000000000", "Ноутбук", "Модель", "SN1", "Не включается");

access.CurrentRole = UserRole.Technician;
orders.SaveDiagnosis(order.Id, "Неисправен накопитель", "Замена SSD");
access.CurrentRole = UserRole.Receptionist;
orders.Approve(order.Id);
access.CurrentRole = UserRole.Administrator;
inventory.AddPart("SSD", "SSD-TEST", 3, 200);
var part = inventory.GetParts().Single();
access.CurrentRole = UserRole.Technician;
orders.AddWork(order.Id, "Замена накопителя", 500);
inventory.UsePart(order.Id, part.Id, 2);

var rejected = false;
try { inventory.UsePart(order.Id, part.Id, 2); }
catch (InvalidOperationException) { rejected = true; }
if (!rejected) throw new Exception("Проверка остатка не сработала.");
orders.SetReady(order.Id);
access.CurrentRole = UserRole.Receptionist;
orders.Issue(order.Id);

var refusedOrder = orders.Create("Клиент с отказом", "+70000000001", "ПК", "Модель 2", "SN2", "Не загружается");
access.CurrentRole = UserRole.Technician;
orders.SaveDiagnosis(refusedOrder.Id, "Неисправна плата", "Замена платы");
access.CurrentRole = UserRole.Receptionist;
orders.Decline(refusedOrder.Id);
var refused = orders.GetOrders().Single(x => x.Id == refusedOrder.Id);
if (refused.Status != OrderStatus.Declined || refused.IsApproved != false)
    throw new Exception("Отказ клиента не сохранён.");
orders.Issue(refusedOrder.Id);

var saved = orders.GetOrders().Single(x => x.Id == order.Id);
if (saved.Status != OrderStatus.Issued || saved.Total != 900 || inventory.GetParts().Single().Stock != 1)
    throw new Exception("Данные заказа, сумма или остаток неверны.");
Console.WriteLine("OK: согласие и отказ клиента, выдача, сумма и атомарное списание детали.");
