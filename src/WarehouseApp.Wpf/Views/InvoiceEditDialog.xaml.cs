using System.Windows;
using WarehouseApp.Core.ViewModels;

namespace WarehouseApp.Wpf.Views;

public partial class InvoiceEditDialog : Window
{
    public InvoiceEditDialog()
    {
        InitializeComponent();
        DataContextChanged += (_, e) =>
        {
            if (e.NewValue is InvoiceEditViewModel vm)
                vm.RequestClose += () => Close();
        };
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
