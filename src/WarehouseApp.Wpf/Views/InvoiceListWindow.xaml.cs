using System.Windows;

namespace WarehouseApp.Wpf.Views;

public partial class InvoiceListWindow : Window
{
    public InvoiceListWindow()
    {
        InitializeComponent();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
