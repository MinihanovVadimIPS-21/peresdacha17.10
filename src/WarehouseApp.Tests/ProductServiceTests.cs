using Microsoft.VisualStudio.TestTools.UnitTesting;
using WarehouseApp.Core.Data;
using WarehouseApp.Core.Exceptions;
using WarehouseApp.Core.Services;

namespace WarehouseApp.Tests;

[TestClass]
public class ProductServiceTests
{
    private InMemoryDataStore _store = null!;
    private ProductService _service = null!;
    private int _orgId;
    private int _warehouseId;

    [TestInitialize]
    public void Setup()
    {
        _store = new InMemoryDataStore();
        _service = new ProductService(_store);
        var warehouse = _store.Warehouses.First(w => _store.Products.Any(p => p.WarehouseId == w.Id));
        _orgId = warehouse.OrganizationId;
        _warehouseId = warehouse.Id;
    }

    [TestMethod]
    public void GetByWarehouse_ReturnsOnlyMatchingProducts()
    {
        var result = _service.GetByWarehouse(_warehouseId);
        Assert.IsTrue(result.All(p => p.WarehouseId == _warehouseId));
        Assert.IsTrue(result.Count > 0);
    }

    [TestMethod]
    public void Search_FiltersByNameCaseInsensitive()
    {
        var product = _service.GetByWarehouse(_warehouseId).First();
        var query = product.Name.Substring(0, Math.Min(3, product.Name.Length)).ToUpperInvariant();

        var result = _service.Search(_warehouseId, query);
        Assert.IsTrue(result.Any(p => p.Id == product.Id));
    }

    [TestMethod]
    public void Search_EmptyQuery_ReturnsAll()
    {
        var all = _service.GetByWarehouse(_warehouseId);
        var result = _service.Search(_warehouseId, "");
        Assert.AreEqual(all.Count, result.Count);
    }

    [TestMethod]
    public void Add_ValidData_CreatesProduct()
    {
        var product = _service.Add(_orgId, _warehouseId, "Тестовый товар", "Категория", "Производитель",
            "Поставщик", 99.99m, 10, 15, null);

        Assert.IsTrue(product.Id > 0);
        Assert.AreEqual(99.99m, product.Price);
        Assert.AreEqual(84.99m, product.PriceWithDiscount);
    }

    [TestMethod]
    public void Add_NegativePrice_Throws()
    {
        Assert.ThrowsException<ValidationException>(() =>
            _service.Add(_orgId, _warehouseId, "Товар", "К", "П", "П", -1m, 1, 0, null));
    }

    [TestMethod]
    public void Add_DiscountOutOfRange_Throws()
    {
        Assert.ThrowsException<ValidationException>(() =>
            _service.Add(_orgId, _warehouseId, "Товар", "К", "П", "П", 10m, 1, 150, null));
    }

    [TestMethod]
    public void Add_WarehouseNotBelongingToOrganization_Throws()
    {
        var otherOrg = _store.Organizations.First(o => o.Id != _orgId);
        Assert.ThrowsException<ValidationException>(() =>
            _service.Add(otherOrg.Id, _warehouseId, "Товар", "К", "П", "П", 10m, 1, 0, null));
    }

    [TestMethod]
    public void Update_ValidData_UpdatesFields()
    {
        var product = _service.GetByWarehouse(_warehouseId).First();
        _service.Update(product.Id, "Новое имя", "Новая категория", "Новый пр-ль", "Новый пост.",
            500m, 20, 5, "photo.png");

        var updated = _service.GetById(product.Id);
        Assert.AreEqual("Новое имя", updated.Name);
        Assert.AreEqual(500m, updated.Price);
        Assert.AreEqual(20, updated.Stock);
        Assert.AreEqual("photo.png", updated.PhotoPath);
    }

    [TestMethod]
    public void Delete_RemovesProduct()
    {
        var product = _service.GetByWarehouse(_warehouseId).First();
        _service.Delete(product.Id);

        Assert.ThrowsException<ValidationException>(() => _service.GetById(product.Id));
    }

    [TestMethod]
    public void ImportFromCsv_ValidFile_ImportsAllRows()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllLines(tempFile, new[]
            {
                "Наименование;Категория;Производитель;Поставщик;Цена;Остаток;Скидка%",
                "Товар А;Категория 1;Пр-ль 1;Пост. 1;100.50;10;0",
                "Товар Б;Категория 2;Пр-ль 2;Пост. 2;200;5;10"
            });

            var before = _service.GetByWarehouse(_warehouseId).Count;
            var imported = _service.ImportFromCsv(_orgId, _warehouseId, tempFile);

            Assert.AreEqual(2, imported);
            Assert.AreEqual(before + 2, _service.GetByWarehouse(_warehouseId).Count);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [TestMethod]
    public void ImportFromCsv_MissingFile_Throws()
    {
        Assert.ThrowsException<ValidationException>(() =>
            _service.ImportFromCsv(_orgId, _warehouseId, "не_существует.csv"));
    }

    [TestMethod]
    public void GetById_ExistingId_ReturnsProduct()
    {
        var product = _service.GetByWarehouse(_warehouseId).First();
        Assert.AreEqual(product.Name, _service.GetById(product.Id).Name);
    }

    [TestMethod]
    public void GetById_UnknownId_Throws()
    {
        Assert.ThrowsException<ValidationException>(() => _service.GetById(-5));
    }

    [TestMethod]
    public void Add_EmptyName_Throws()
    {
        Assert.ThrowsException<ValidationException>(() =>
            _service.Add(_orgId, _warehouseId, "  ", "К", "П", "П", 10m, 1, 0, null));
    }

    [TestMethod]
    public void ImportFromCsv_InvalidRow_ImportsNothing()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllLines(tempFile, new[]
            {
                "Товар А;К;П;П;100;10;0",
                "Товар Б;К;П;П;не_число;5;0"
            });
            var before = _service.GetByWarehouse(_warehouseId).Count;

            Assert.ThrowsException<ValidationException>(() => _service.ImportFromCsv(_orgId, _warehouseId, tempFile));
            Assert.AreEqual(before, _service.GetByWarehouse(_warehouseId).Count);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
