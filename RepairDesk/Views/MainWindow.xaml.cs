using System.Windows;
using RepairDesk.ViewModels;

namespace RepairDesk.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
