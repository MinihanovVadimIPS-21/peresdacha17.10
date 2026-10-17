using Microsoft.VisualStudio.TestTools.UnitTesting;
using WarehouseApp.Core.Data;
using WarehouseApp.Core.Exceptions;
using WarehouseApp.Core.Services;

namespace WarehouseApp.Tests;

[TestClass]
public class WarehouseServiceTests
{
    private InMemoryDataStore _store = null!;
    private WarehouseService _service = null!;
    private int _orgId;

    [TestInitialize]
    public void Setup()
    {
        _store = new InMemoryDataStore();
        _service = new WarehouseService(_store);
        _orgId = _store.Organizations.First().Id;
    }

    [TestMethod]
    public void GetByOrganization_ReturnsOnlyMatchingWarehouses()
    {
        var result = _service.GetByOrganization(_orgId);
        Assert.IsTrue(result.All(w => w.OrganizationId == _orgId));
        Assert.IsTrue(result.Count > 0);
    }

    [TestMethod]
    public void GetById_UnknownId_Throws()
    {
        Assert.ThrowsException<ValidationException>(() => _service.GetById(-1));
    }

    [TestMethod]
    public void Add_ValidData_CreatesWarehouse()
    {
        var wh = _service.Add(_orgId, "Новый склад", "Адрес 1");
        Assert.AreEqual(_orgId, wh.OrganizationId);
        Assert.AreEqual("Новый склад", wh.Name);
        Assert.AreEqual("Адрес 1", wh.Address);
    }

    [TestMethod]
    public void Add_EmptyName_Throws()
    {
        Assert.ThrowsException<ValidationException>(() => _service.Add(_orgId, "", null));
    }

    [TestMethod]
    public void Add_UnknownOrganization_Throws()
    {
        Assert.ThrowsException<ValidationException>(() => _service.Add(99999, "Склад", null));
    }

    [TestMethod]
    public void Add_DuplicateNameWithinOrganization_Throws()
    {
        var existing = _service.GetByOrganization(_orgId).First();
        Assert.ThrowsException<ValidationException>(() => _service.Add(_orgId, existing.Name, null));
    }

    [TestMethod]
    public void Update_ValidData_UpdatesFields()
    {
        var wh = _service.Add(_orgId, "Склад для изменения", "Старый адрес");
        _service.Update(wh.Id, "Изменённый склад", "Новый адрес");

        var updated = _service.GetById(wh.Id);
        Assert.AreEqual("Изменённый склад", updated.Name);
        Assert.AreEqual("Новый адрес", updated.Address);
    }

    [TestMethod]
    public void Delete_WarehouseWithoutProducts_Removes()
    {
        var wh = _service.Add(_orgId, "Пустой склад", null);
        _service.Delete(wh.Id);

        Assert.ThrowsException<ValidationException>(() => _service.GetById(wh.Id));
    }

    [TestMethod]
    public void Delete_WarehouseWithProducts_CascadesProductsAndInvoices()
    {
        var wh = _service.GetByOrganization(_orgId).First(w => _store.Products.Any(p => p.WarehouseId == w.Id));
        _store.Invoices.Add(new WarehouseApp.Core.Models.Invoice { Id = 100, OrganizationId = _orgId, WarehouseId = wh.Id });

        _service.Delete(wh.Id);

        Assert.AreEqual(0, _store.Products.Count(p => p.WarehouseId == wh.Id));
        Assert.AreEqual(0, _store.Invoices.Count(i => i.WarehouseId == wh.Id));
        Assert.ThrowsException<ValidationException>(() => _service.GetById(wh.Id));
    }

    [TestMethod]
    public void Delete_UnknownId_Throws()
    {
        Assert.ThrowsException<ValidationException>(() => _service.Delete(99999));
    }
}
