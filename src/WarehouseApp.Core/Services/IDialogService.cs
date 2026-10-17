using WarehouseApp.Core.Models;
using WarehouseApp.Core.ViewModels;

namespace WarehouseApp.Core.Services;

public interface IDialogService
{
    void ShowMessage(string message, string title = "Готово");
    void ShowError(string message, string title = "Ошибка");
    bool Confirm(string message, string title = "Подтверждение");

    int Choose(string title, string prompt, IReadOnlyList<string> items);

    string? OpenFile(string title, string filter);

    string? ShowOrganizationDialog(string? currentName);

    string? ShowDeleteConfirmDialog(string entityName, string entityKind);

    WarehouseData? ShowWarehouseDialog(WarehouseData? current);

    ProductData? ShowProductDialog(Product? existing);

    void ShowInvoiceEditDialog(InvoiceEditViewModel viewModel);
    void ShowInvoicesListWindow(InvoicesViewModel viewModel);
    void ShowWorkspaceWindow(WorkspaceViewModel viewModel);
}
