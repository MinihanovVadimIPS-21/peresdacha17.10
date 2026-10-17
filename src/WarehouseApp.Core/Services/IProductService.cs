using WarehouseApp.Core.Models;

namespace WarehouseApp.Core.Services;

public interface IProductService
{
    IReadOnlyList<Product> GetByWarehouse(int warehouseId);
    IReadOnlyList<Product> Search(int warehouseId, string? searchText);
    Product GetById(int id);

    Product Add(int organizationId, int warehouseId, string name, string category, string manufacturer,
        string supplier, decimal price, int stock, decimal discountPercent, string? photoPath);

    void Update(int id, string name, string category, string manufacturer, string supplier,
        decimal price, int stock, decimal discountPercent, string? photoPath);

    void Delete(int id);

    int ImportFromCsv(int organizationId, int warehouseId, string filePath);
}
