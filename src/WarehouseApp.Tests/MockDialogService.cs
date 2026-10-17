using WarehouseApp.Core.Models;
using WarehouseApp.Core.Services;
using WarehouseApp.Core.ViewModels;

namespace WarehouseApp.Tests;

public class MockDialogService : IDialogService
{
    public bool ConfirmResult { get; set; } = true;
    public string? OrganizationDialogResult { get; set; } = "Mocked Org";
    public string? DeleteConfirmResult { get; set; }
    public WarehouseData? WarehouseDialogResult { get; set; } = new() { Name = "Mocked Warehouse", Address = "Mocked Address" };
    public ProductData? ProductDialogResult { get; set; } =
        new() { Name = "Mocked Product", Category = "Cat", Manufacturer = "Man", Supplier = "Sup", Price = 10m, Stock = 3, DiscountPercent = 0 };
    public int ChooseResult { get; set; } = 0;
    public string? OpenFileResult { get; set; }

    public List<string> Errors { get; } = new();
    public List<string> Messages { get; } = new();
    public WorkspaceViewModel? ShownWorkspace { get; private set; }
    public int InvoiceEditorsShown { get; private set; }
    public int InvoiceListsShown { get; private set; }
    public Action<InvoiceEditViewModel>? OnInvoiceEditShown { get; set; }

    public void ShowMessage(string message, string title = "Готово") => Messages.Add(message);
    public void ShowError(string message, string title = "Ошибка") => Errors.Add(message);
    public bool Confirm(string message, string title = "Подтверждение") => ConfirmResult;
    public int Choose(string title, string prompt, IReadOnlyList<string> items) => ChooseResult;
    public string? OpenFile(string title, string filter) => OpenFileResult;
    public string? ShowOrganizationDialog(string? currentName) => OrganizationDialogResult;
    public string? ShowDeleteConfirmDialog(string entityName, string entityKind) => DeleteConfirmResult;
    public WarehouseData? ShowWarehouseDialog(WarehouseData? current) => WarehouseDialogResult;
    public ProductData? ShowProductDialog(Product? existing) => ProductDialogResult;

    public void ShowInvoiceEditDialog(InvoiceEditViewModel viewModel)
    {
        InvoiceEditorsShown++;
        OnInvoiceEditShown?.Invoke(viewModel);
    }

    public void ShowInvoicesListWindow(InvoicesViewModel viewModel) => InvoiceListsShown++;
    public void ShowWorkspaceWindow(WorkspaceViewModel viewModel) => ShownWorkspace = viewModel;
}
