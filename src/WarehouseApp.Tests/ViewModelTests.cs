using Microsoft.VisualStudio.TestTools.UnitTesting;
using WarehouseApp.Core.Data;
using WarehouseApp.Core.Models;
using WarehouseApp.Core.Services;
using WarehouseApp.Core.ViewModels;

namespace WarehouseApp.Tests;

[TestClass]
public class OrganizationsViewModelTests
{
    private InMemoryDataStore _store = null!;
    private MockDialogService _dialogs = null!;
    private AppServices _services = null!;
    private OrganizationsViewModel _vm = null!;

    [TestInitialize]
    public void Setup()
    {
        _store = new InMemoryDataStore();
        _dialogs = new MockDialogService();
        _services = AppServices.Create(_dialogs, _store);
        _vm = new OrganizationsViewModel(_services);
    }

    [TestMethod]
    public void Constructor_LoadsOrganizations()
    {
        Assert.AreEqual(_store.Organizations.Count, _vm.Organizations.Count);
    }

    [TestMethod]
    public void Commands_RequireSelection()
    {
        Assert.IsTrue(_vm.AddCommand.CanExecute(null));
        Assert.IsFalse(_vm.EditCommand.CanExecute(null));
        Assert.IsFalse(_vm.DeleteCommand.CanExecute(null));
        Assert.IsFalse(_vm.OpenCommand.CanExecute(null));

        _vm.SelectedOrganization = _vm.Organizations.First();

        Assert.IsTrue(_vm.EditCommand.CanExecute(null));
        Assert.IsTrue(_vm.DeleteCommand.CanExecute(null));
        Assert.IsTrue(_vm.OpenCommand.CanExecute(null));
    }

    [TestMethod]
    public void Add_ValidName_AddsOrganization()
    {
        var before = _vm.Organizations.Count;
        _vm.AddCommand.Execute(null);
        Assert.AreEqual(before + 1, _vm.Organizations.Count);
        Assert.IsTrue(_vm.Organizations.Any(o => o.Name == "Mocked Org"));
    }

    [TestMethod]
    public void Add_Cancelled_DoesNothing()
    {
        _dialogs.OrganizationDialogResult = null;
        var before = _vm.Organizations.Count;
        _vm.AddCommand.Execute(null);
        Assert.AreEqual(before, _vm.Organizations.Count);
    }

    [TestMethod]
    public void Add_EmptyName_ShowsError()
    {
        _dialogs.OrganizationDialogResult = "   ";
        _vm.AddCommand.Execute(null);
        Assert.AreEqual(1, _dialogs.Errors.Count);
    }

    [TestMethod]
    public void Edit_ChangesName()
    {
        _vm.SelectedOrganization = _vm.Organizations.First();
        var id = _vm.SelectedOrganization.Id;
        _dialogs.OrganizationDialogResult = "Новое имя";

        _vm.EditCommand.Execute(null);

        Assert.AreEqual("Новое имя", _services.Organizations.GetById(id).Name);
    }

    [TestMethod]
    public void Delete_CorrectConfirmation_RemovesOrganization()
    {
        _vm.SelectedOrganization = _vm.Organizations.First();
        var org = _vm.SelectedOrganization;
        _dialogs.DeleteConfirmResult = org.Name;

        _vm.DeleteCommand.Execute(null);

        Assert.IsFalse(_vm.Organizations.Any(o => o.Id == org.Id));
    }

    [TestMethod]
    public void Delete_WrongName_ShowsErrorAndKeepsOrganization()
    {
        _vm.SelectedOrganization = _vm.Organizations.First();
        var org = _vm.SelectedOrganization;
        _dialogs.DeleteConfirmResult = "не то название";

        _vm.DeleteCommand.Execute(null);

        Assert.AreEqual(1, _dialogs.Errors.Count);
        Assert.IsTrue(_vm.Organizations.Any(o => o.Id == org.Id));
    }

    [TestMethod]
    public void Delete_Cancelled_KeepsOrganization()
    {
        _vm.SelectedOrganization = _vm.Organizations.First();
        var before = _vm.Organizations.Count;
        _dialogs.DeleteConfirmResult = null;

        _vm.DeleteCommand.Execute(null);

        Assert.AreEqual(before, _vm.Organizations.Count);
    }

    [TestMethod]
    public void Open_PassesSelectedOrganizationAsContext()
    {
        _vm.SelectedOrganization = _vm.Organizations.First();

        _vm.OpenCommand.Execute(null);

        Assert.IsNotNull(_dialogs.ShownWorkspace);
        Assert.AreEqual(_vm.SelectedOrganization.Id, _dialogs.ShownWorkspace!.Organization.Id);
    }
}

[TestClass]
public class WorkspaceViewModelTests
{
    private InMemoryDataStore _store = null!;
    private MockDialogService _dialogs = null!;
    private AppServices _services = null!;
    private Organization _org = null!;
    private WorkspaceViewModel _vm = null!;

    [TestInitialize]
    public void Setup()
    {
        _store = new InMemoryDataStore();
        _dialogs = new MockDialogService();
        _services = AppServices.Create(_dialogs, _store);
        _org = _store.Organizations.First();
        _vm = new WorkspaceViewModel(_services, _org);
    }

    [TestMethod]
    public void Constructor_LoadsOnlyWarehousesOfOrganization_AndSelectsFirst()
    {
        Assert.IsTrue(_vm.Warehouses.Count > 0);
        Assert.IsTrue(_vm.Warehouses.All(w => w.OrganizationId == _org.Id));
        Assert.IsNotNull(_vm.SelectedWarehouse);
    }

    [TestMethod]
    public void ContextStatus_ContainsOrganizationAndWarehouse()
    {
        StringAssert.Contains(_vm.ContextStatus, _org.Name);
        StringAssert.Contains(_vm.ContextStatus, _vm.SelectedWarehouse!.Name);
    }

    [TestMethod]
    public void SelectingWarehouse_LoadsItsProductsOnly()
    {
        foreach (var wh in _vm.Warehouses)
        {
            _vm.SelectedWarehouse = wh;
            Assert.IsTrue(_vm.Products.All(p => p.WarehouseId == wh.Id));
        }
    }

    [TestMethod]
    public void SearchText_FiltersProducts()
    {
        var wh = _vm.Warehouses.First(w => _store.Products.Any(p => p.WarehouseId == w.Id));
        _vm.SelectedWarehouse = wh;
        var first = _vm.Products.First();

        _vm.SearchText = first.Name;

        Assert.IsTrue(_vm.Products.Any(p => p.Id == first.Id));
        _vm.SearchText = "несуществующий_товар_zzz";
        Assert.AreEqual(0, _vm.Products.Count);
    }

    [TestMethod]
    public void AddWarehouse_AddsToCurrentOrganization()
    {
        var before = _vm.Warehouses.Count;
        _vm.AddWarehouseCommand.Execute(null);
        Assert.AreEqual(before + 1, _vm.Warehouses.Count);
        Assert.IsTrue(_vm.Warehouses.Any(w => w.Name == "Mocked Warehouse"));
    }

    [TestMethod]
    public void EditWarehouse_ChangesName()
    {
        var id = _vm.SelectedWarehouse!.Id;
        _dialogs.WarehouseDialogResult = new WarehouseData { Name = "Переименованный", Address = "Адрес" };
        _vm.EditWarehouseCommand.Execute(null);
        Assert.AreEqual("Переименованный", _services.Warehouses.GetById(id).Name);
    }

    [TestMethod]
    public void DeleteWarehouse_Confirmed_RemovesWarehouse()
    {
        var wh = _vm.SelectedWarehouse!;
        _vm.DeleteWarehouseCommand.Execute(null);
        Assert.IsFalse(_vm.Warehouses.Any(w => w.Id == wh.Id));
    }

    [TestMethod]
    public void DeleteWarehouse_Cancelled_KeepsWarehouse()
    {
        var wh = _vm.SelectedWarehouse!;
        _dialogs.ConfirmResult = false;
        _vm.DeleteWarehouseCommand.Execute(null);
        Assert.IsTrue(_vm.Warehouses.Any(w => w.Id == wh.Id));
    }

    [TestMethod]
    public void AddProduct_AddsToSelectedWarehouse()
    {
        var before = _vm.Products.Count;
        _vm.AddProductCommand.Execute(null);
        Assert.AreEqual(before + 1, _vm.Products.Count);
        Assert.IsTrue(_vm.Products.Any(p => p.Name == "Mocked Product"));
    }

    [TestMethod]
    public void AddProduct_InvalidData_ShowsError()
    {
        _dialogs.ProductDialogResult = new ProductData { Name = "", Price = 1m };
        _vm.AddProductCommand.Execute(null);
        Assert.AreEqual(1, _dialogs.Errors.Count);
    }

    [TestMethod]
    public void EditProduct_ChangesFields()
    {
        _vm.SelectedWarehouse = _vm.Warehouses.First(w => _store.Products.Any(p => p.WarehouseId == w.Id));
        _vm.SelectedProduct = _vm.Products.First();
        var id = _vm.SelectedProduct.Id;

        _vm.EditProductCommand.Execute(null);

        Assert.AreEqual("Mocked Product", _services.Products.GetById(id).Name);
    }

    [TestMethod]
    public void DeleteProduct_RemovesProduct()
    {
        _vm.SelectedWarehouse = _vm.Warehouses.First(w => _store.Products.Any(p => p.WarehouseId == w.Id));
        _vm.SelectedProduct = _vm.Products.First();
        var id = _vm.SelectedProduct.Id;

        _vm.DeleteProductCommand.Execute(null);

        Assert.IsFalse(_vm.Products.Any(p => p.Id == id));
    }

    [TestMethod]
    public void ImportProducts_FromCsv_AddsProductsAndReportsCount()
    {
        var file = Path.GetTempFileName();
        try
        {
            File.WriteAllLines(file, new[] { "Т1;К;П;П;10;1;0", "Т2;К;П;П;20;2;5" });
            _dialogs.OpenFileResult = file;
            var before = _vm.Products.Count;

            _vm.ImportProductsCommand.Execute(null);

            Assert.AreEqual(before + 2, _vm.Products.Count);
            Assert.IsTrue(_dialogs.Messages.Any(m => m.Contains("2")));
        }
        finally { File.Delete(file); }
    }

    [TestMethod]
    public void CreateInvoice_Cancelled_DoesNothing()
    {
        _dialogs.ChooseResult = -1;
        _vm.CreateInvoiceCommand.Execute(null);
        Assert.AreEqual(0, _dialogs.InvoiceEditorsShown);
    }

    [TestMethod]
    public void CreateInvoice_Incoming_OpensEditorWithDraft()
    {
        _dialogs.ChooseResult = 0;
        _vm.CreateInvoiceCommand.Execute(null);

        Assert.AreEqual(1, _dialogs.InvoiceEditorsShown);
        Assert.AreEqual(1, _services.Invoices.GetByWarehouse(_vm.SelectedWarehouse!.Id).Count);
    }

    [TestMethod]
    public void EditInvoice_OpensEditorForPickedInvoice()
    {
        _services.Invoices.CreateDraft(_org.Id, _vm.SelectedWarehouse!.Id, InvoiceType.Incoming);
        _dialogs.ChooseResult = 0;

        _vm.EditInvoiceCommand.Execute(null);

        Assert.AreEqual(1, _dialogs.InvoiceEditorsShown);
    }

    [TestMethod]
    public void DeleteInvoice_RemovesPickedDraft()
    {
        var wh = _vm.SelectedWarehouse!;
        _services.Invoices.CreateDraft(_org.Id, wh.Id, InvoiceType.Incoming);
        _dialogs.ChooseResult = 0;

        _vm.DeleteInvoiceCommand.Execute(null);

        Assert.AreEqual(0, _services.Invoices.GetByWarehouse(wh.Id).Count);
    }

    [TestMethod]
    public void OpenInvoices_ShowsInvoicesWindow()
    {
        _vm.OpenInvoicesCommand.Execute(null);
        Assert.AreEqual(1, _dialogs.InvoiceListsShown);
    }
}

[TestClass]
public class InvoiceViewModelsTests
{
    private InMemoryDataStore _store = null!;
    private MockDialogService _dialogs = null!;
    private AppServices _services = null!;
    private Organization _org = null!;
    private Warehouse _wh = null!;
    private Product _product = null!;

    [TestInitialize]
    public void Setup()
    {
        _store = new InMemoryDataStore();
        _dialogs = new MockDialogService();
        _services = AppServices.Create(_dialogs, _store);
        _wh = _store.Warehouses.First(w => _store.Products.Any(p => p.WarehouseId == w.Id));
        _org = _store.Organizations.First(o => o.Id == _wh.OrganizationId);
        _product = _services.Products.GetByWarehouse(_wh.Id).First();
    }

    private InvoiceEditViewModel NewEditor(InvoiceType type)
    {
        var inv = _services.Invoices.CreateDraft(_org.Id, _wh.Id, type);
        return new InvoiceEditViewModel(_services, inv, _wh);
    }

    [TestMethod]
    public void Editor_AddItem_AddsPositionAndEnablesConfirm()
    {
        var vm = NewEditor(InvoiceType.Incoming);
        Assert.IsFalse(vm.ConfirmCommand.CanExecute(null));

        vm.SelectedProduct = vm.AvailableProducts.First();
        vm.Quantity = 4;
        vm.AddItemCommand.Execute(null);

        Assert.AreEqual(1, vm.Items.Count);
        Assert.AreEqual(4, vm.Items[0].Quantity);
        Assert.IsTrue(vm.ConfirmCommand.CanExecute(null));
    }

    [TestMethod]
    public void Editor_RemoveItem_RemovesPosition()
    {
        var vm = NewEditor(InvoiceType.Incoming);
        vm.SelectedProduct = vm.AvailableProducts.First();
        vm.AddItemCommand.Execute(null);
        vm.SelectedItem = vm.Items.First();

        vm.RemoveItemCommand.Execute(null);

        Assert.AreEqual(0, vm.Items.Count);
    }

    [TestMethod]
    public void Editor_ConfirmIncoming_IncreasesStockAndRequestsClose()
    {
        var before = _product.Stock;
        var vm = NewEditor(InvoiceType.Incoming);
        var closed = false;
        vm.RequestClose += () => closed = true;
        vm.SelectedProduct = vm.AvailableProducts.First(p => p.Id == _product.Id);
        vm.Quantity = 5;
        vm.AddItemCommand.Execute(null);

        vm.ConfirmCommand.Execute(null);

        Assert.AreEqual(before + 5, _services.Products.GetById(_product.Id).Stock);
        Assert.IsTrue(closed);
        Assert.IsFalse(vm.IsEditable);
    }

    [TestMethod]
    public void Editor_ConfirmOutgoing_InsufficientStock_ShowsError()
    {
        var vm = NewEditor(InvoiceType.Outgoing);
        vm.SelectedProduct = vm.AvailableProducts.First(p => p.Id == _product.Id);
        vm.Quantity = _product.Stock + 1000;
        vm.AddItemCommand.Execute(null);

        vm.ConfirmCommand.Execute(null);

        Assert.AreEqual(1, _dialogs.Errors.Count);
        Assert.IsTrue(vm.IsEditable);
    }

    [TestMethod]
    public void List_LoadsInvoicesOfWarehouse()
    {
        _services.Invoices.CreateDraft(_org.Id, _wh.Id, InvoiceType.Incoming);
        var vm = new InvoicesViewModel(_services, _org, _wh);
        Assert.AreEqual(1, vm.Invoices.Count);
    }

    [TestMethod]
    public void List_CreateIncoming_CreatesDraftAndOpensEditor()
    {
        var vm = new InvoicesViewModel(_services, _org, _wh);

        vm.CreateIncomingCommand.Execute(null);

        Assert.AreEqual(1, _dialogs.InvoiceEditorsShown);
        Assert.AreEqual(1, vm.Invoices.Count);
        Assert.AreEqual(InvoiceType.Incoming, vm.Invoices[0].Type);
    }

    [TestMethod]
    public void List_Confirm_AppliesStockChange()
    {
        var before = _product.Stock;
        var inv = _services.Invoices.CreateDraft(_org.Id, _wh.Id, InvoiceType.Incoming);
        _services.Invoices.AddItem(inv.Id, _product.Id, 7);
        var vm = new InvoicesViewModel(_services, _org, _wh);
        vm.SelectedInvoice = vm.Invoices.First();

        vm.ConfirmCommand.Execute(null);

        Assert.AreEqual(before + 7, _services.Products.GetById(_product.Id).Stock);
        Assert.AreEqual(InvoiceStatus.Confirmed, vm.Invoices[0].Status);
    }

    [TestMethod]
    public void List_Delete_RemovesDraft_AndConfirmedIsRejected()
    {
        var draft = _services.Invoices.CreateDraft(_org.Id, _wh.Id, InvoiceType.Incoming);
        var vm = new InvoicesViewModel(_services, _org, _wh);
        vm.SelectedInvoice = vm.Invoices.First(i => i.Id == draft.Id);
        vm.DeleteCommand.Execute(null);
        Assert.AreEqual(0, vm.Invoices.Count);

        var confirmed = _services.Invoices.CreateDraft(_org.Id, _wh.Id, InvoiceType.Incoming);
        _services.Invoices.AddItem(confirmed.Id, _product.Id, 1);
        _services.Invoices.Confirm(confirmed.Id);
        vm.Reload();
        vm.SelectedInvoice = vm.Invoices.First();
        vm.DeleteCommand.Execute(null);

        Assert.AreEqual(1, _dialogs.Errors.Count);
        Assert.AreEqual(1, vm.Invoices.Count);
    }
}
