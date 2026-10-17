using System.Collections.ObjectModel;
using WarehouseApp.Core.Models;
using WarehouseApp.Core.Services;

namespace WarehouseApp.Core.ViewModels;

public class InvoiceEditViewModel : ViewModelBase
{
    private readonly AppServices _s;

    public Invoice Invoice { get; private set; }
    public Warehouse Warehouse { get; }

    public bool IsEditable => Invoice.Status == InvoiceStatus.Draft;

    public string HeaderText =>
        $"{(Invoice.Type == InvoiceType.Incoming ? "Приходная" : "Расходная")} накладная {Invoice.Number}  |  " +
        $"Склад: {Warehouse.Name}  |  Статус: {(IsEditable ? "Черновик" : "Подтверждена")}";

    public ObservableCollection<InvoiceItem> Items { get; } = new();
    public ObservableCollection<Product> AvailableProducts { get; } = new();

    private Product? _selectedProduct;
    public Product? SelectedProduct
    {
        get => _selectedProduct;
        set { if (SetField(ref _selectedProduct, value)) RefreshCommands(); }
    }

    private InvoiceItem? _selectedItem;
    public InvoiceItem? SelectedItem
    {
        get => _selectedItem;
        set { if (SetField(ref _selectedItem, value)) RefreshCommands(); }
    }

    private int _quantity = 1;
    public int Quantity
    {
        get => _quantity;
        set { if (SetField(ref _quantity, value)) RefreshCommands(); }
    }

    public RelayCommand AddItemCommand { get; }
    public RelayCommand RemoveItemCommand { get; }
    public RelayCommand ConfirmCommand { get; }

    public event Action? RequestClose;

    public InvoiceEditViewModel(AppServices services, Invoice invoice, Warehouse warehouse)
    {
        _s = services;
        Invoice = invoice;
        Warehouse = warehouse;

        AddItemCommand = new RelayCommand(AddItem, () => IsEditable && SelectedProduct != null && Quantity > 0);
        RemoveItemCommand = new RelayCommand(RemoveItem, () => IsEditable && SelectedItem != null);
        ConfirmCommand = new RelayCommand(Confirm, () => IsEditable && Items.Count > 0);

        foreach (var p in _s.Products.GetByWarehouse(warehouse.Id))
            AvailableProducts.Add(p);

        ReloadItems();
    }

    private void RefreshCommands()
    {
        AddItemCommand.RaiseCanExecuteChanged();
        RemoveItemCommand.RaiseCanExecuteChanged();
        ConfirmCommand.RaiseCanExecuteChanged();
    }

    private void ReloadItems()
    {
        Invoice = _s.Invoices.GetById(Invoice.Id);
        Items.Clear();
        foreach (var item in Invoice.Items)
            Items.Add(item);
        OnPropertyChanged(nameof(IsEditable));
        OnPropertyChanged(nameof(HeaderText));
        RefreshCommands();
    }

    private void AddItem()
    {
        if (SelectedProduct is null) return;
        var productId = SelectedProduct.Id;
        var qty = Quantity;
        if (TryExecute(_s.Dialogs, () => _s.Invoices.AddItem(Invoice.Id, productId, qty)))
            ReloadItems();
    }

    private void RemoveItem()
    {
        if (SelectedItem is null) return;
        var productId = SelectedItem.ProductId;
        if (TryExecute(_s.Dialogs, () => _s.Invoices.RemoveItem(Invoice.Id, productId)))
            ReloadItems();
    }

    private void Confirm()
    {
        if (!_s.Dialogs.Confirm("Подтвердить накладную? Остатки товаров на складе будут изменены.")) return;
        if (TryExecute(_s.Dialogs, () => _s.Invoices.Confirm(Invoice.Id)))
        {
            ReloadItems();
            _s.Dialogs.ShowMessage("Накладная подтверждена, остатки на складе обновлены.");
            RequestClose?.Invoke();
        }
    }
}
