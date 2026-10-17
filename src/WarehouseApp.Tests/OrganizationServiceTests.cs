using Microsoft.VisualStudio.TestTools.UnitTesting;
using WarehouseApp.Core.Data;
using WarehouseApp.Core.Exceptions;
using WarehouseApp.Core.Services;

namespace WarehouseApp.Tests;

[TestClass]
public class OrganizationServiceTests
{
    private InMemoryDataStore _store = null!;
    private OrganizationService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _store = new InMemoryDataStore();
        _service = new OrganizationService(_store);
    }

    [TestMethod]
    public void GetAll_ReturnsSeededOrganizations_OrderedByName()
    {
        var all = _service.GetAll();

        Assert.AreEqual(2, all.Count);
        CollectionAssert.AreEqual(
            all.Select(o => o.Name).OrderBy(n => n).ToList(),
            all.Select(o => o.Name).ToList());
    }

    [TestMethod]
    public void GetById_ExistingId_ReturnsOrganization()
    {
        var existing = _service.GetAll().First();
        var found = _service.GetById(existing.Id);
        Assert.AreEqual(existing.Name, found.Name);
    }

    [TestMethod]
    public void GetById_UnknownId_ThrowsValidationException()
    {
        Assert.ThrowsException<ValidationException>(() => _service.GetById(9999));
    }

    [TestMethod]
    public void Add_ValidName_AddsOrganization()
    {
        var before = _service.GetAll().Count;
        var created = _service.Add("Новая Компания, ООО");

        Assert.AreEqual(before + 1, _service.GetAll().Count);
        Assert.AreEqual("Новая Компания, ООО", created.Name);
        Assert.IsTrue(created.Id > 0);
    }

    [TestMethod]
    public void Add_EmptyName_ThrowsValidationException()
    {
        Assert.ThrowsException<ValidationException>(() => _service.Add("   "));
    }

    [TestMethod]
    public void Add_DuplicateName_ThrowsValidationException()
    {
        var existingName = _service.GetAll().First().Name;
        Assert.ThrowsException<ValidationException>(() => _service.Add(existingName));
    }

    [TestMethod]
    public void Update_ValidName_ChangesName()
    {
        var org = _service.GetAll().First();
        _service.Update(org.Id, "Переименованная Компания");

        var updated = _service.GetById(org.Id);
        Assert.AreEqual("Переименованная Компания", updated.Name);
    }

    [TestMethod]
    public void Update_EmptyName_ThrowsValidationException()
    {
        var org = _service.GetAll().First();
        Assert.ThrowsException<ValidationException>(() => _service.Update(org.Id, ""));
    }

    [TestMethod]
    public void Delete_CorrectConfirmation_RemovesOrganization()
    {
        var org = _service.Add("Компания на удаление");
        _service.Delete(org.Id, "Компания на удаление");

        Assert.ThrowsException<ValidationException>(() => _service.GetById(org.Id));
    }

    [TestMethod]
    public void Delete_WrongConfirmation_ThrowsAndDoesNotRemove()
    {
        var org = _service.Add("Компания под защитой");

        Assert.ThrowsException<ValidationException>(() => _service.Delete(org.Id, "Неверное имя"));
        Assert.IsNotNull(_service.GetById(org.Id));
    }

    [TestMethod]
    public void Delete_CascadesToWarehousesAndProducts()
    {
        var warehouseService = new WarehouseService(_store);
        var productService = new ProductService(_store);

        var org = _service.Add("Компания с складом");
        var wh = warehouseService.Add(org.Id, "Тестовый склад", null);
        productService.Add(org.Id, wh.Id, "Тестовый товар", "Кат", "Пр-ль", "Пост.", 100m, 5, 0, null);

        _service.Delete(org.Id, "Компания с складом");

        Assert.AreEqual(0, warehouseService.GetByOrganization(org.Id).Count);
        Assert.AreEqual(0, productService.GetByWarehouse(wh.Id).Count);
    }
}
