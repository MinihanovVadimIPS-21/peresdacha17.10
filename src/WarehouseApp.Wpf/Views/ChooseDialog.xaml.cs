using System.Windows;
using System.Windows.Input;
using WarehouseApp.Wpf.Common;

namespace WarehouseApp.Wpf.Views;

public partial class ChooseDialog : Window
{
    public int SelectedIndex => ItemsList.SelectedIndex;

    public ChooseDialog(string title, string prompt, IReadOnlyList<string> items)
    {
        InitializeComponent();
        Title = title;
        PromptLabel.Content = prompt;
        ItemsList.ItemsSource = items;
        if (items.Count > 0) ItemsList.SelectedIndex = 0;
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (ItemsList.SelectedIndex < 0)
        {
            DialogHelper.ShowError("Выберите пункт из списка.");
            return;
        }
        DialogResult = true;
    }

    private void ItemsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (ItemsList.SelectedIndex >= 0) DialogResult = true;
    }
}
