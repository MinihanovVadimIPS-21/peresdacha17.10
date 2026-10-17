using System.Windows;

namespace WarehouseApp.Wpf.Views;

public partial class WorkspaceWindow : Window
{
    public WorkspaceWindow()
    {
        InitializeComponent();
    }

    private void CloseMenuItem_Click(object sender, RoutedEventArgs e) => Close();

    private void BackToOrganizations_Click(object sender, RoutedEventArgs e) => Close();
}
