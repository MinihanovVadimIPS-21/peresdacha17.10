using Microsoft.VisualStudio.TestTools.UnitTesting;
using WarehouseApp.Core.Data;
using WarehouseApp.Core.Exceptions;
using WarehouseApp.Core.Models;
using WarehouseApp.Core.Services;

namespace WarehouseApp.Tests;

[TestClass]
public class InvoiceServiceTests
{
    private InMemoryDataStore _store = null!;
    private ProductService _productService = null!;
    private InvoiceService _service = null!;
    private int _orgId;
    private int _warehouseId;
    private Product _product = null!;

    [TestInitialize]
    public void Setup()
    {
        _store = new InMemoryDataStore();
        _productService = new ProductService(_store);
        _service = new InvoiceService(_store, _productService);

        var warehouse = _store.Warehouses.First(w => _store.Products.Any(p => p.WarehouseId == w.Id));
        _orgId = warehouse.OrganizationId;
        _warehouseId = warehouse.Id;
        _product = _productService.GetByWarehouse(_warehouseId).First();
    }

    [TestMethod]
    public void CreateDraft_ValidData_CreatesInvoiceInDraftStatus()
    {
        var invoice = _service.CreateDraft(_orgId, _warehouseId, InvoiceType.Incoming);
        Assert.AreEqual(InvoiceStatus.Draft, invoice.Status);
        Assert.AreEqual(InvoiceType.Incoming, invoice.Type);
    }

    [TestMethod]
    public void CreateDraft_WarehouseNotBelongingToOrganization_Throws()
    {
        var otherOrg = _store.Organizations.First(o => o.Id != _orgId);
        Assert.ThrowsException<ValidationException>(() =>
            _service.CreateDraft(otherOrg.Id, _warehouseId, InvoiceType.Incoming));
    }

    [TestMethod]
    public void AddItem_ValidData_AddsItemToInvoice()
    {
        var invoice = _service.CreateDraft(_orgId, _warehouseId, InvoiceType.Incoming);
        _service.AddItem(invoice.Id, _product.Id, 5);

        var updated = _service.GetById(invoice.Id);
        Assert.AreEqual(1, updated.Items.Count);
        Assert.AreEqual(5, updated.Items[0].Quantity);
    }

    [TestMethod]
    public void AddItem_SameProductTwice_AccumulatesQuantity()
    {
        var invoice = _service.CreateDraft(_orgId, _warehouseId, InvoiceType.Incoming);
        _service.AddItem(invoice.Id, _product.Id, 3);
        _service.AddItem(invoice.Id, _product.Id, 4);

        var updated = _service.GetById(invoice.Id);
        Assert.AreEqual(1, updated.Items.Count);
        Assert.AreEqual(7, updated.Items[0].Quantity);
    }

    [TestMethod]
    public void AddItem_NonPositiveQuantity_Throws()
    {
        var invoice = _service.CreateDraft(_orgId, _warehouseId, InvoiceType.Incoming);
        Assert.ThrowsException<ValidationException>(() => _service.AddItem(invoice.Id, _product.Id, 0));
    }

    [TestMethod]
    public void RemoveItem_RemovesExistingItem()
    {
        var invoice = _service.CreateDraft(_orgId, _warehouseId, InvoiceType.Incoming);
        _service.AddItem(invoice.Id, _product.Id, 5);
        _service.RemoveItem(invoice.Id, _product.Id);

        var updated = _service.GetById(invoice.Id);
        Assert.AreEqual(0, updated.Items.Count);
    }

    [TestMethod]
    public void Confirm_IncomingInvoice_IncreasesStock()
    {
        var stockBefore = _product.Stock;
        var invoice = _service.CreateDraft(_orgId, _warehouseId, InvoiceType.Incoming);
        _service.AddItem(invoice.Id, _product.Id, 10);

        _service.Confirm(invoice.Id);

        var updatedProduct = _productService.GetById(_product.Id);
        Assert.AreEqual(stockBefore + 10, updatedProduct.Stock);
        Assert.AreEqual(InvoiceStatus.Confirmed, _service.GetById(invoice.Id).Status);
    }

    [TestMethod]
    public void Confirm_OutgoingInvoice_DecreasesStock()
    {
        var stockBefore = _product.Stock;
        var invoice = _service.CreateDraft(_orgId, _warehouseId, InvoiceType.Outgoing);
        _service.AddItem(invoice.Id, _product.Id, 2);

        _service.Confirm(invoice.Id);

        var updatedProduct = _productService.GetById(_product.Id);
        Assert.AreEqual(stockBefore - 2, updatedProduct.Stock);
    }

    [TestMethod]
    public void Confirm_OutgoingInvoice_InsufficientStock_Throws()
    {
        var invoice = _service.CreateDraft(_orgId, _warehouseId, InvoiceType.Outgoing);
        _service.AddItem(invoice.Id, _product.Id, _product.Stock + 1000);

        Assert.ThrowsException<ValidationException>(() => _service.Confirm(invoice.Id));
    }

    [TestMethod]
    public void Confirm_EmptyInvoice_Throws()
    {
        var invoice = _service.CreateDraft(_orgId, _warehouseId, InvoiceType.Incoming);
        Assert.ThrowsException<ValidationException>(() => _service.Confirm(invoice.Id));
    }

    [TestMethod]
    public void Confirm_AlreadyConfirmedInvoice_Throws()
    {
        var invoice = _service.CreateDraft(_orgId, _warehouseId, InvoiceType.Incoming);
        _service.AddItem(invoice.Id, _product.Id, 1);
        _service.Confirm(invoice.Id);

        Assert.ThrowsException<ValidationException>(() => _service.Confirm(invoice.Id));
    }

    [TestMethod]
    public void Delete_DraftInvoice_Removes()
    {
        var invoice = _service.CreateDraft(_orgId, _warehouseId, InvoiceType.Incoming);
        _service.Delete(invoice.Id);

        Assert.ThrowsException<ValidationException>(() => _service.GetById(invoice.Id));
    }

    [TestMethod]
    public void Delete_ConfirmedInvoice_Throws()
    {
        var invoice = _service.CreateDraft(_orgId, _warehouseId, InvoiceType.Incoming);
        _service.AddItem(invoice.Id, _product.Id, 1);
        _service.Confirm(invoice.Id);

        Assert.ThrowsException<ValidationException>(() => _service.Delete(invoice.Id));
    }

    [TestMethod]
    public void GetByWarehouse_ReturnsOnlyInvoicesOfThatWarehouse()
    {
        var invoice = _service.CreateDraft(_orgId, _warehouseId, InvoiceType.Incoming);

        var list = _service.GetByWarehouse(_warehouseId);

        Assert.IsTrue(list.Any(i => i.Id == invoice.Id));
        Assert.IsTrue(list.All(i => i.WarehouseId == _warehouseId));
        Assert.AreEqual(0, _service.GetByWarehouse(99999).Count);
    }

    [TestMethod]
    public void GetById_ExistingAndUnknown()
    {
        var invoice = _service.CreateDraft(_orgId, _warehouseId, InvoiceType.Outgoing);

        Assert.AreEqual(invoice.Id, _service.GetById(invoice.Id).Id);
        Assert.ThrowsException<ValidationException>(() => _service.GetById(99999));
    }

    [TestMethod]
    public void AddItem_ToConfirmedInvoice_Throws()
    {
        var invoice = _service.CreateDraft(_orgId, _warehouseId, InvoiceType.Incoming);
        _service.AddItem(invoice.Id, _product.Id, 1);
        _service.Confirm(invoice.Id);

        Assert.ThrowsException<ValidationException>(() => _service.AddItem(invoice.Id, _product.Id, 1));
    }

    [TestMethod]
    public void Confirm_OutgoingInvoice_InsufficientStock_DoesNotChangeAnyStock()
    {
        var invoice = _service.CreateDraft(_orgId, _warehouseId, InvoiceType.Outgoing);
        _service.AddItem(invoice.Id, _product.Id, 1);
        var other = _productService.GetByWarehouse(_warehouseId).FirstOrDefault(p => p.Id != _product.Id);
        if (other != null) _service.AddItem(invoice.Id, other.Id, other.Stock + 1000);
        else _service.AddItem(invoice.Id, _product.Id, _product.Stock + 1000);

        var stockBefore = _product.Stock;
        Assert.ThrowsException<ValidationException>(() => _service.Confirm(invoice.Id));
        Assert.AreEqual(stockBefore, _productService.GetById(_product.Id).Stock);
    }
}
