using System.Globalization;
using WarehouseApp.Core.Data;
using WarehouseApp.Core.Exceptions;
using WarehouseApp.Core.Models;

namespace WarehouseApp.Core.Services;

public class ProductService : IProductService
{
    private readonly InMemoryDataStore _store;

    public ProductService(InMemoryDataStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public IReadOnlyList<Product> GetByWarehouse(int warehouseId) =>
        _store.Products.Where(p => p.WarehouseId == warehouseId)
                        .OrderBy(p => p.Name)
                        .ToList();

    public IReadOnlyList<Product> Search(int warehouseId, string? searchText)
    {
        var items = GetByWarehouse(warehouseId);
        if (string.IsNullOrWhiteSpace(searchText))
            return items;

        var text = searchText.Trim();
        return items.Where(p =>
                p.Name.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                p.Category.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                p.Manufacturer.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                p.Supplier.Contains(text, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public Product GetById(int id) =>
        _store.Products.FirstOrDefault(p => p.Id == id)
        ?? throw new ValidationException($"Товар с Id={id} не найден.");

    public Product Add(int organizationId, int warehouseId, string name, string category, string manufacturer,
        string supplier, decimal price, int stock, decimal discountPercent, string? photoPath)
    {
        Validate(name, category, manufacturer, supplier, price, stock, discountPercent);

        if (!_store.Warehouses.Any(w => w.Id == warehouseId && w.OrganizationId == organizationId))
            throw new ValidationException("Указанный склад не принадлежит выбранной организации.");

        var product = new Product
        {
            Id = _store.NextProductId++,
            OrganizationId = organizationId,
            WarehouseId = warehouseId,
            Name = name.Trim(),
            Category = (category ?? string.Empty).Trim(),
            Manufacturer = (manufacturer ?? string.Empty).Trim(),
            Supplier = (supplier ?? string.Empty).Trim(),
            Price = price,
            Stock = stock,
            DiscountPercent = discountPercent,
            PhotoPath = string.IsNullOrWhiteSpace(photoPath) ? null : photoPath
        };
        _store.Products.Add(product);
        return product;
    }

    public void Update(int id, string name, string category, string manufacturer, string supplier,
        decimal price, int stock, decimal discountPercent, string? photoPath)
    {
        Validate(name, category, manufacturer, supplier, price, stock, discountPercent);

        var product = GetById(id);
        product.Name = name.Trim();
        product.Category = (category ?? string.Empty).Trim();
        product.Manufacturer = (manufacturer ?? string.Empty).Trim();
        product.Supplier = (supplier ?? string.Empty).Trim();
        product.Price = price;
        product.Stock = stock;
        product.DiscountPercent = discountPercent;
        product.PhotoPath = string.IsNullOrWhiteSpace(photoPath) ? null : photoPath;
    }

    public void Delete(int id)
    {
        var product = GetById(id);

        var draftInvoice = _store.Invoices
            .FirstOrDefault(inv => inv.Status == InvoiceStatus.Draft
                                   && inv.Items.Any(item => item.ProductId == id));

        if (draftInvoice != null)
            throw new ValidationException(
                $"Нельзя удалить товар \"{product.Name}\": он используется " +
                $"в черновике накладной {draftInvoice.Number}.");

        _store.Products.Remove(product);
    }

    public int ImportFromCsv(int organizationId, int warehouseId, string filePath)
    {
        if (!File.Exists(filePath))
            throw new ValidationException($"Файл \"{filePath}\" не найден.");

        if (!_store.Warehouses.Any(w => w.Id == warehouseId && w.OrganizationId == organizationId))
            throw new ValidationException("Указанный склад не принадлежит выбранной организации.");

        var lines = File.ReadAllLines(filePath)
                         .Where(l => !string.IsNullOrWhiteSpace(l))
                         .ToList();

        var parsed = new List<ProductRow>();
        foreach (var (line, index) in lines.Select((l, i) => (l, i)))
        {
            var cols = line.Split(';');
            if (cols.Length < 7)
            {
                if (index == 0) continue;
                throw new ValidationException($"Строка {index + 1}: ожидается 7 колонок (Наименование;Категория;Производитель;Поставщик;Цена;Остаток;Скидка%).");
            }

            if (!decimal.TryParse(cols[4].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
            {
                if (index == 0) continue;
                throw new ValidationException($"Строка {index + 1}: некорректное значение цены \"{cols[4]}\".");
            }

            if (!int.TryParse(cols[5].Trim(), out var stock))
                throw new ValidationException($"Строка {index + 1}: некорректное значение остатка \"{cols[5]}\".");

            if (!decimal.TryParse(cols[6].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var discount))
                discount = 0;

            try { Validate(cols[0], cols[1], cols[2], cols[3], price, stock, discount); }
            catch (ValidationException ex) { throw new ValidationException($"Строка {index + 1}: {ex.Message}"); }

            parsed.Add(new ProductRow(cols[0].Trim(), cols[1].Trim(), cols[2].Trim(), cols[3].Trim(), price, stock, discount));
        }

        foreach (var r in parsed)
            Add(organizationId, warehouseId, r.Name, r.Category, r.Manufacturer, r.Supplier, r.Price, r.Stock, r.Discount, null);

        return parsed.Count;
    }

    private sealed record ProductRow(string Name, string Category, string Manufacturer, string Supplier,
        decimal Price, int Stock, decimal Discount);

    private static void Validate(string name, string category, string manufacturer,
        string supplier, decimal price, int stock, decimal discountPercent)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Наименование товара не может быть пустым.");
        if (string.IsNullOrWhiteSpace(category))
            throw new ValidationException("Категория не может быть пустой.");
        if (string.IsNullOrWhiteSpace(manufacturer))
            throw new ValidationException("Производитель не может быть пустым.");
        if (string.IsNullOrWhiteSpace(supplier))
            throw new ValidationException("Поставщик не может быть пустым.");
        if (price < 0)
            throw new ValidationException("Цена не может быть отрицательной.");
        if (stock < 0)
            throw new ValidationException("Остаток не может быть отрицательным.");
        if (discountPercent < 0 || discountPercent > 100)
            throw new ValidationException("Скидка должна быть в диапазоне от 0 до 100%.");
    }
}