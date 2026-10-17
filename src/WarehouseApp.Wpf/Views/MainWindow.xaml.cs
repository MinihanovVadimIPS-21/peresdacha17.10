using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WarehouseApp.Core.ViewModels;
namespace WarehouseApp.Wpf.Views;
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new OrganizationsViewModel(App.Services);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
    private void ListView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        var item = ItemsControl.ContainerFromElement(sender as ListView, e.OriginalSource as DependencyObject) as ListViewItem;
        if (item != null)
        {
            item.IsSelected = true; 
        }
    }
}