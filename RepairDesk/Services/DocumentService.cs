using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using RepairDesk.Domain;

namespace RepairDesk.Services;

public sealed class DocumentService
{
    public void Print(RepairOrder order)
    {
        var dialog = new PrintDialog();
        if (dialog.ShowDialog() != true) return;

        var document = new FlowDocument { PagePadding = new Thickness(55), FontFamily = new FontFamily("Times New Roman"), FontSize = 14 };
        document.Blocks.Add(new Paragraph(new Run(order.Status == OrderStatus.Issued ? "АКТ ВЫДАЧИ УСТРОЙСТВА" : "АКТ ПРИЁМА УСТРОЙСТВА")) { FontSize = 18, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
        Add($"Заказ {order.Number} от {order.CreatedAt:dd.MM.yyyy HH:mm}");
        Add($"Клиент: {order.CustomerName}; телефон: {order.Device.Customer.Phone}");
        Add($"Устройство: {order.DeviceName}; серийный номер: {order.Device.SerialNumber}");
        Add($"Неисправность со слов клиента: {order.Problem}");
        if (!string.IsNullOrWhiteSpace(order.Diagnosis)) Add($"Диагностика: {order.Diagnosis}");
        if (!string.IsNullOrWhiteSpace(order.WorkPlan)) Add($"План работ: {order.WorkPlan}");
        if (order.Status == OrderStatus.Issued)
        {
            foreach (var work in order.Works) Add($"Работа: {work.Description} — {work.Price:N2} ₽");
            foreach (var part in order.Parts) Add($"Деталь: {part.PartName}, {part.Quantity} шт. — {part.Sum:N2} ₽");
            Add($"Итого: {order.Total:N2} ₽");
        }
        Add("Подпись сотрудника: ____________________    Подпись клиента: ____________________");
        dialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator, $"Заказ {order.Number}");

        void Add(string text) => document.Blocks.Add(new Paragraph(new Run(text)));
    }
}
