using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using RepairDesk.Data;
using RepairDesk.Domain;
using RepairDesk.Services;

namespace RepairDesk.ViewModels;

public sealed record RoleOption(UserRole Role, string Title);

public sealed class MainViewModel : ObservableObject
{
    private readonly AccessService _access = new();
    private readonly OrderService _orders;
    private readonly InventoryService _inventory;
    private readonly SummaryService _summary = new();
    private readonly DocumentService _documents = new();

    public MainViewModel()
    {
        using (var db = new RepairDbContext()) db.Database.EnsureCreated();
        _orders = new OrderService(_access);
        _inventory = new InventoryService(_access);
        CreateOrderCommand = new RelayCommand(() => Run(CreateOrder));
        SaveDiagnosisCommand = new RelayCommand(() => Run(SaveDiagnosis));
        ApproveCommand = new RelayCommand(() => Run(() => ChangeOrder(_orders.Approve)));
        DeclineCommand = new RelayCommand(() => Run(() => ChangeOrder(_orders.Decline)));
        ReadyCommand = new RelayCommand(() => Run(() => ChangeOrder(_orders.SetReady)));
        IssueCommand = new RelayCommand(() => Run(() => ChangeOrder(_orders.Issue)));
        AddWorkCommand = new RelayCommand(() => Run(AddWork));
        AddPartCommand = new RelayCommand(() => Run(AddPart));
        UsePartCommand = new RelayCommand(() => Run(UsePart));
        PrintCommand = new RelayCommand(() => Run(Print));
        RefreshCommand = new RelayCommand(() => Run(() => Refresh()));
        Refresh();
    }

    public IReadOnlyList<RoleOption> Roles { get; } =
    [
        new(UserRole.Receptionist, "Приёмщик"),
        new(UserRole.Technician, "Мастер"),
        new(UserRole.Administrator, "Администратор")
    ];

    private RoleOption _currentRole = new(UserRole.Receptionist, "Приёмщик");
    public RoleOption CurrentRole
    {
        get => _currentRole;
        set { if (value is null) return; _currentRole = value; _access.CurrentRole = value.Role; Raise(); Raise(nameof(CanRecordDecision)); StatusMessage = $"Роль: {value.Title}"; }
    }

    private ObservableCollection<RepairOrder> _orderItems = [];
    public ObservableCollection<RepairOrder> OrderItems { get => _orderItems; private set { _orderItems = value; Raise(); } }
    private ObservableCollection<SparePart> _partItems = [];
    public ObservableCollection<SparePart> PartItems { get => _partItems; private set { _partItems = value; Raise(); } }
    private RepairOrder? _selectedOrder;
    public RepairOrder? SelectedOrder
    {
        get => _selectedOrder;
        set { _selectedOrder = value; Raise(); Raise(nameof(CanRecordDecision)); DiagnosisInput = value?.Diagnosis ?? ""; WorkPlanInput = value?.WorkPlan ?? ""; }
    }
    private SparePart? _selectedPart;
    public SparePart? SelectedPart { get => _selectedPart; set { _selectedPart = value; Raise(); } }

    public bool CanRecordDecision => SelectedOrder?.Status == OrderStatus.AwaitingApproval &&
        CurrentRole.Role is UserRole.Receptionist or UserRole.Administrator;

    private string _statusMessage = "Готово";
    public string StatusMessage { get => _statusMessage; private set { _statusMessage = value; Raise(); } }
    private string _summaryText = "";
    public string SummaryText { get => _summaryText; private set { _summaryText = value; Raise(); } }

    public string CustomerInput { get; set; } = "";
    public string PhoneInput { get; set; } = "";
    public string DeviceTypeInput { get; set; } = "";
    public string DeviceModelInput { get; set; } = "";
    public string SerialInput { get; set; } = "";
    public string ProblemInput { get; set; } = "";

    private string _diagnosisInput = "";
    public string DiagnosisInput { get => _diagnosisInput; set { _diagnosisInput = value; Raise(); } }
    private string _workPlanInput = "";
    public string WorkPlanInput { get => _workPlanInput; set { _workPlanInput = value; Raise(); } }
    public string WorkDescriptionInput { get; set; } = "";
    public string WorkPriceInput { get; set; } = "";
    public string QuantityInput { get; set; } = "1";
    public string PartNameInput { get; set; } = "";
    public string PartArticleInput { get; set; } = "";
    public string PartStockInput { get; set; } = "";
    public string PartPriceInput { get; set; } = "";

    public ICommand CreateOrderCommand { get; }
    public ICommand SaveDiagnosisCommand { get; }
    public ICommand ApproveCommand { get; }
    public ICommand DeclineCommand { get; }
    public ICommand ReadyCommand { get; }
    public ICommand IssueCommand { get; }
    public ICommand AddWorkCommand { get; }
    public ICommand AddPartCommand { get; }
    public ICommand UsePartCommand { get; }
    public ICommand PrintCommand { get; }
    public ICommand RefreshCommand { get; }

    private void Run(Action operation)
    {
        try { operation(); StatusMessage = "Операция выполнена."; }
        catch (Exception error) { StatusMessage = error.Message; }
    }

    private void Refresh(int? selectedId = null)
    {
        selectedId ??= SelectedOrder?.Id;
        OrderItems = new ObservableCollection<RepairOrder>(_orders.GetOrders());
        PartItems = new ObservableCollection<SparePart>(_inventory.GetParts());
        SelectedOrder = OrderItems.FirstOrDefault(x => x.Id == selectedId);
        SummaryText = _summary.GetSummary();
    }

    private void CreateOrder()
    {
        var order = _orders.Create(CustomerInput, PhoneInput, DeviceTypeInput, DeviceModelInput, SerialInput, ProblemInput);
        Refresh(order.Id);
    }

    private int OrderId() => SelectedOrder?.Id ?? throw new InvalidOperationException("Выберите заказ в списке.");
    private void ChangeOrder(Action<int> operation) { var id = OrderId(); operation(id); Refresh(id); }
    private void SaveDiagnosis() { var id = OrderId(); _orders.SaveDiagnosis(id, DiagnosisInput, WorkPlanInput); Refresh(id); }
    private void AddWork() { var id = OrderId(); _orders.AddWork(id, WorkDescriptionInput, ParseMoney(WorkPriceInput)); Refresh(id); }
    private void AddPart() { _inventory.AddPart(PartNameInput, PartArticleInput, ParseInt(PartStockInput), ParseMoney(PartPriceInput)); Refresh(); }
    private void UsePart()
    {
        var id = OrderId();
        var partId = SelectedPart?.Id ?? throw new InvalidOperationException("Выберите запчасть в списке.");
        _inventory.UsePart(id, partId, ParseInt(QuantityInput));
        Refresh(id);
    }
    private void Print() => _documents.Print(SelectedOrder ?? throw new InvalidOperationException("Выберите заказ в списке."));

    private static int ParseInt(string input) => int.TryParse(input, out var value) ? value : throw new ArgumentException("Введите целое число.");
    private static decimal ParseMoney(string input) => decimal.TryParse(input, NumberStyles.Number, CultureInfo.CurrentCulture, out var value)
        ? value : throw new ArgumentException("Введите число для цены.");
}
