namespace RepairDesk.Domain;

public enum UserRole { Receptionist, Technician, Administrator }
public enum OrderStatus { Accepted, Diagnosing, AwaitingApproval, InRepair, Ready, Issued, Declined }

public sealed class Customer
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public List<Device> Devices { get; set; } = [];
}

public sealed class Device
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public string Type { get; set; } = "";
    public string Model { get; set; } = "";
    public string SerialNumber { get; set; } = "";
}

public sealed class RepairOrder
{
    public int Id { get; set; }
    public int DeviceId { get; set; }
    public Device Device { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public OrderStatus Status { get; set; } = OrderStatus.Accepted;
    public string Problem { get; set; } = "";
    public string Diagnosis { get; set; } = "";
    public string WorkPlan { get; set; } = "";
    public bool? IsApproved { get; set; }
    public List<RepairWork> Works { get; set; } = [];
    public List<PartUsage> Parts { get; set; } = [];
    public decimal Total => Works.Sum(x => x.Price) + Parts.Sum(x => x.Quantity * x.UnitPrice);
    public string Number => $"Р-{Id:00000}";
    public string StatusName => Status switch
    {
        OrderStatus.Accepted => "Принят",
        OrderStatus.Diagnosing => "Диагностика",
        OrderStatus.AwaitingApproval => "Ожидает согласования",
        OrderStatus.InRepair => "В ремонте",
        OrderStatus.Ready => "Готов к выдаче",
        OrderStatus.Issued => "Выдан",
        OrderStatus.Declined => "Отказ от ремонта",
        _ => "Неизвестен"
    };
    public string CustomerName => Device?.Customer?.FullName ?? "";
    public string DeviceName => Device is null ? "" : $"{Device.Type} {Device.Model}";
}

public sealed class RepairWork
{
    public int Id { get; set; }
    public int RepairOrderId { get; set; }
    public RepairOrder RepairOrder { get; set; } = null!;
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
}

public sealed class SparePart
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Article { get; set; } = "";
    public int Stock { get; set; }
    public decimal Price { get; set; }
}

public sealed class PartUsage
{
    public int Id { get; set; }
    public int RepairOrderId { get; set; }
    public RepairOrder RepairOrder { get; set; } = null!;
    public int SparePartId { get; set; }
    public SparePart SparePart { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string PartName => SparePart?.Name ?? "";
    public decimal Sum => Quantity * UnitPrice;
}
