using System.Collections.ObjectModel;
using WarehouseApp.Core.Models;
using WarehouseApp.Core.Services;

namespace WarehouseApp.Core.ViewModels;

public class WorkspaceViewModel : ViewModelBase
{
    private readonly AppServices _s;

    public Organization Organization { get; }
    public ObservableCollection<Warehouse> Warehouses { get; } = new();
    public ObservableCollection<Product> Products { get; } = new();

    private Warehouse? _selectedWarehouse;
    public Warehouse? SelectedWarehouse
    {
        get => _selectedWarehouse;
        set
        {
            if (SetField(ref _selectedWarehouse, value))
            {
                ReloadProducts();
                OnPropertyChanged(nameof(ContextStatus));
                RefreshCommands();
            }
        }
    }

    private Product? _selectedProduct;
    public Product? SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            if (SetField(ref _selectedProduct, value))
                RefreshCommands();
        }
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetField(ref _searchText, value))
                ReloadProducts();
        }
    }

    public string ContextStatus =>
        SelectedWarehouse is null
            ? $"Организация: {Organization.Name}  |  Склад не выбран"
            : $"Организация: {Organization.Name}  |  Склад: {SelectedWarehouse.Name}";

    public RelayCommand AddWarehouseCommand { get; }
    public RelayCommand EditWarehouseCommand { get; }
    public RelayCommand DeleteWarehouseCommand { get; }
    public RelayCommand ImportProductsCommand { get; }

    public RelayCommand AddProductCommand { get; }
    public RelayCommand EditProductCommand { get; }
    public RelayCommand DeleteProductCommand { get; }

    public RelayCommand OpenInvoicesCommand { get; }
    public RelayCommand CreateInvoiceCommand { get; }
    public RelayCommand EditInvoiceCommand { get; }
    public RelayCommand DeleteInvoiceCommand { get; }

    public WorkspaceViewModel(AppServices services, Organization organization)
    {
        _s = services;
        Organization = organization;

        Func<bool> hasWarehouse = () => SelectedWarehouse != null;

        AddWarehouseCommand = new RelayCommand(AddWarehouse);
        EditWarehouseCommand = new RelayCommand(EditWarehouse, hasWarehouse);
        DeleteWarehouseCommand = new RelayCommand(DeleteWarehouse, hasWarehouse);
        ImportProductsCommand = new RelayCommand(ImportProducts, hasWarehouse);

        AddProductCommand = new RelayCommand(AddProduct, hasWarehouse);
        EditProductCommand = new RelayCommand(EditProduct, () => SelectedProduct != null);
        DeleteProductCommand = new RelayCommand(DeleteProduct, () => SelectedProduct != null);

        OpenInvoicesCommand = new RelayCommand(OpenInvoices, hasWarehouse);
        CreateInvoiceCommand = new RelayCommand(CreateInvoice, hasWarehouse);
        EditInvoiceCommand = new RelayCommand(EditInvoice, hasWarehouse);
        DeleteInvoiceCommand = new RelayCommand(DeleteInvoice, hasWarehouse);

        ReloadWarehouses();
    }

    private void RefreshCommands()
    {
        foreach (var c in new[]
        {
            EditWarehouseCommand, DeleteWarehouseCommand, ImportProductsCommand,
            AddProductCommand, EditProductCommand, DeleteProductCommand,
            OpenInvoicesCommand, CreateInvoiceCommand, EditInvoiceCommand, DeleteInvoiceCommand
        })
            c.RaiseCanExecuteChanged();
    }

    public void ReloadWarehouses()
    {
        var keepId = SelectedWarehouse?.Id;
        Warehouses.Clear();
        foreach (var wh in _s.Warehouses.GetByOrganization(Organization.Id))
            Warehouses.Add(wh);

        SelectedWarehouse = Warehouses.FirstOrDefault(w => w.Id == keepId) ?? Warehouses.FirstOrDefault();
    }

    public void ReloadProducts()
    {
        Products.Clear();
        SelectedProduct = null;
        if (SelectedWarehouse is null) return;

        foreach (var p in _s.Products.Search(SelectedWarehouse.Id, SearchText))
            Products.Add(p);
    }

    private void AddWarehouse()
    {
        var data = _s.Dialogs.ShowWarehouseDialog(null);
        if (data is null) return;
        if (TryExecute(_s.Dialogs, () => _s.Warehouses.Add(Organization.Id, data.Name, data.Address)))
            ReloadWarehouses();
    }

    private void EditWarehouse()
    {
        if (SelectedWarehouse is null) return;
        var id = SelectedWarehouse.Id;
        var data = _s.Dialogs.ShowWarehouseDialog(
            new WarehouseData { Name = SelectedWarehouse.Name, Address = SelectedWarehouse.Address });
        if (data is null) return;
        if (TryExecute(_s.Dialogs, () => _s.Warehouses.Update(id, data.Name, data.Address)))
            ReloadWarehouses();
    }

    private void DeleteWarehouse()
    {
        if (SelectedWarehouse is null) return;
        var wh = SelectedWarehouse;
        var count = _s.Products.GetByWarehouse(wh.Id).Count;
        var msg = count == 0
            ? $"Удалить склад \"{wh.Name}\"?"
            : $"Удалить склад \"{wh.Name}\" вместе с товарами ({count}) и накладными?";
        if (!_s.Dialogs.Confirm(msg)) return;
        if (TryExecute(_s.Dialogs, () => _s.Warehouses.Delete(wh.Id)))
            ReloadWarehouses();
    }

    private void ImportProducts()
    {
        if (SelectedWarehouse is null) return;
        var path = _s.Dialogs.OpenFile("Импорт товаров из CSV", "CSV файлы (*.csv)|*.csv|Все файлы (*.*)|*.*");
        if (path is null) return;

        var whId = SelectedWarehouse.Id;
        var imported = 0;
        if (TryExecute(_s.Dialogs, () => imported = _s.Products.ImportFromCsv(Organization.Id, whId, path)))
        {
            ReloadProducts();
            _s.Dialogs.ShowMessage($"Импортировано товаров: {imported}.");
        }
    }

    private void AddProduct()
    {
        if (SelectedWarehouse is null) return;
        var whId = SelectedWarehouse.Id;
        var d = _s.Dialogs.ShowProductDialog(null);
        if (d is null) return;
        if (TryExecute(_s.Dialogs, () => _s.Products.Add(Organization.Id, whId, d.Name, d.Category,
                d.Manufacturer, d.Supplier, d.Price, d.Stock, d.DiscountPercent, d.PhotoPath)))
            ReloadProducts();
    }

    private void EditProduct()
    {
        if (SelectedProduct is null) return;
        var id = SelectedProduct.Id;
        var d = _s.Dialogs.ShowProductDialog(SelectedProduct);
        if (d is null) return;
        if (TryExecute(_s.Dialogs, () => _s.Products.Update(id, d.Name, d.Category, d.Manufacturer,
                d.Supplier, d.Price, d.Stock, d.DiscountPercent, d.PhotoPath)))
            ReloadProducts();
    }

    private void DeleteProduct()
    {
        if (SelectedProduct is null) return;
        var p = SelectedProduct;
        if (!_s.Dialogs.Confirm($"Удалить товар \"{p.Name}\" из списка?")) return;
        if (TryExecute(_s.Dialogs, () => _s.Products.Delete(p.Id)))
            ReloadProducts();
    }

    private void OpenInvoices()
    {
        if (SelectedWarehouse is null) return;
        _s.Dialogs.ShowInvoicesListWindow(new InvoicesViewModel(_s, Organization, SelectedWarehouse));
        ReloadProducts();
    }

    private void CreateInvoice()
    {
        if (SelectedWarehouse is null) return;
        var wh = SelectedWarehouse;
        var idx = _s.Dialogs.Choose("Создать накладную", "Выберите тип накладной:",
            new[] { "Приходная (поступление товара на склад)", "Расходная (списание товара со склада)" });
        if (idx < 0) return;

        var type = idx == 0 ? InvoiceType.Incoming : InvoiceType.Outgoing;
        Invoice? invoice = null;
        if (!TryExecute(_s.Dialogs, () => invoice = _s.Invoices.CreateDraft(Organization.Id, wh.Id, type)) || invoice is null)
            return;

        _s.Dialogs.ShowInvoiceEditDialog(new InvoiceEditViewModel(_s, invoice, wh));
        ReloadProducts();
    }

    private Invoice? PickInvoice(string title)
    {
        if (SelectedWarehouse is null) return null;
        var list = _s.Invoices.GetByWarehouse(SelectedWarehouse.Id);
        if (list.Count == 0)
        {
            _s.Dialogs.ShowMessage("Для выбранного склада накладных пока нет.", title);
            return null;
        }
        var items = list.Select(i =>
            $"{i.Number} | {(i.Type == InvoiceType.Incoming ? "Приход" : "Расход")} | " +
            $"{(i.Status == InvoiceStatus.Draft ? "Черновик" : "Подтверждена")} | " +
            $"{i.CreatedAt:dd.MM.yyyy HH:mm} | позиций: {i.Items.Count}").ToList();
        var idx = _s.Dialogs.Choose(title, "Выберите накладную:", items);
        return idx < 0 ? null : list[idx];
    }

    private void EditInvoice()
    {
        if (SelectedWarehouse is null) return;
        var invoice = PickInvoice("Изменить накладную");
        if (invoice is null) return;
        _s.Dialogs.ShowInvoiceEditDialog(new InvoiceEditViewModel(_s, invoice, SelectedWarehouse));
        ReloadProducts();
    }

    private void DeleteInvoice()
    {
        var invoice = PickInvoice("Удалить накладную");
        if (invoice is null) return;
        if (!_s.Dialogs.Confirm($"Удалить накладную {invoice.Number}?")) return;
        TryExecute(_s.Dialogs, () => _s.Invoices.Delete(invoice.Id));
    }
}
