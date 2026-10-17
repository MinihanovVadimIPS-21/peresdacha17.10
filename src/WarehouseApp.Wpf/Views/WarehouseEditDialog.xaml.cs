using System.Windows;

namespace WarehouseApp.Wpf.Views;

public partial class WarehouseEditDialog : Window
{
    public string WarehouseName
    {
        get => NameTextBox.Text;
        set => NameTextBox.Text = value;
    }

    public string Address
    {
        get => AddressTextBox.Text;
        set => AddressTextBox.Text = value;
    }

    public WarehouseEditDialog()
    {
        InitializeComponent();
        Loaded += (_, _) => NameTextBox.Focus();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            Common.DialogHelper.ShowError("Поле \"Наименование склада\" не может быть пустым.");
            NameTextBox.Focus();
            return;
        }

        DialogResult = true;
    }
}
