using System.Windows;

namespace WarehouseApp.Wpf.Views;

public partial class OrganizationEditDialog : Window
{
    public string CompanyName
    {
        get => NameTextBox.Text;
        set => NameTextBox.Text = value;
    }

    public OrganizationEditDialog()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            NameTextBox.Focus();
            NameTextBox.SelectAll();
        };
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            Common.DialogHelper.ShowError("Поле \"Наименование компании\" не может быть пустым.");
            NameTextBox.Focus();
            return;
        }

        DialogResult = true;
    }
}
