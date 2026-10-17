using System.Windows;
using Microsoft.Win32;
using WarehouseApp.Core.Models;
using WarehouseApp.Core.Services;
using WarehouseApp.Core.ViewModels;
using WarehouseApp.Wpf.Common;
using WarehouseApp.Wpf.Views;

namespace WarehouseApp.Wpf.Services;

public class DialogService : IDialogService
{
    private static Window? ActiveWindow =>
        Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);

    public void ShowMessage(string message, string title = "Готово") => DialogHelper.ShowInfo(message, title);

    public void ShowError(string message, string title = "Ошибка") => DialogHelper.ShowError(message, title);

    public bool Confirm(string message, string title = "Подтверждение") => DialogHelper.Confirm(message, title);

    public int Choose(string title, string prompt, IReadOnlyList<string> items)
    {
        var dlg = new ChooseDialog(title, prompt, items) { Owner = ActiveWindow };
        return dlg.ShowDialog() == true ? dlg.SelectedIndex : -1;
    }

    public string? OpenFile(string title, string filter)
    {
        var dlg = new OpenFileDialog { Title = title, Filter = filter };
        return dlg.ShowDialog() == true ? dlg.FileName : null;
    }

    public string? ShowOrganizationDialog(string? currentName)
    {
        var dlg = new OrganizationEditDialog
        {
            Title = currentName is null ? "Новая организация" : "Изменить организацию",
            Owner = ActiveWindow
        };
        if (currentName != null) dlg.CompanyName = currentName;
        return dlg.ShowDialog() == true ? dlg.CompanyName : null;
    }

    public string? ShowDeleteConfirmDialog(string entityName, string entityKind)
    {
        var dlg = new DeleteConfirmDialog(entityName, entityKind) { Owner = ActiveWindow };
        return dlg.ShowDialog() == true ? dlg.EnteredText : null;
    }

    public WarehouseData? ShowWarehouseDialog(WarehouseData? current)
    {
        var dlg = new WarehouseEditDialog
        {
            Title = current is null ? "Новый склад" : "Изменить склад",
            Owner = ActiveWindow
        };
        if (current != null)
        {
            dlg.WarehouseName = current.Name;
            dlg.Address = current.Address ?? string.Empty;
        }
        return dlg.ShowDialog() == true
            ? new WarehouseData { Name = dlg.WarehouseName, Address = dlg.Address }
            : null;
    }

    public ProductData? ShowProductDialog(Product? existing)
    {
        var dlg = new ProductEditDialog
        {
            Title = existing is null ? "Новый товар" : "Изменить товар",
            Owner = ActiveWindow
        };
        if (existing != null) dlg.LoadFrom(existing);
        return dlg.ShowDialog() == true ? dlg.Result : null;
    }

    public void ShowInvoiceEditDialog(InvoiceEditViewModel viewModel)
    {
        var dlg = new InvoiceEditDialog { DataContext = viewModel, Owner = ActiveWindow };
        dlg.ShowDialog();
    }

    public void ShowInvoicesListWindow(InvoicesViewModel viewModel)
    {
        var win = new InvoiceListWindow { DataContext = viewModel, Owner = ActiveWindow };
        win.ShowDialog();
    }

    public void ShowWorkspaceWindow(WorkspaceViewModel viewModel)
    {
        var win = new WorkspaceWindow { DataContext = viewModel };
        win.Show();
    }
}
