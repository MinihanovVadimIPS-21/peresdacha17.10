using System.Windows;

namespace WarehouseApp.Wpf.Views;

public partial class DeleteConfirmDialog : Window
{
    private readonly string _expectedName;

    public string EnteredText => ConfirmTextBox.Text;

    public DeleteConfirmDialog(string entityName, string entityKind)
    {
        InitializeComponent();
        _expectedName = entityName;

        MessageText.Text = $"Чтобы удалить {entityKind}, введите её(его) точное название ниже и нажмите \"Удалить\". Это действие необратимо.";
        TargetNameText.Text = entityName;

        Loaded += (_, _) => ConfirmTextBox.Focus();
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (!string.Equals(ConfirmTextBox.Text.Trim(), _expectedName, StringComparison.Ordinal))
        {
            Common.DialogHelper.ShowError("Введённое название не совпадает. Проверьте написание и попробуйте снова.");
            return;
        }

        DialogResult = true;
    }
}
