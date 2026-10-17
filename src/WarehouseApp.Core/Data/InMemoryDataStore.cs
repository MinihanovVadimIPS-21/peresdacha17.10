using WarehouseApp.Core.Models;

namespace WarehouseApp.Core.Data;

public sealed class InMemoryDataStore
{
    private static readonly Lazy<InMemoryDataStore> _instance = new(() => new InMemoryDataStore());

    public static InMemoryDataStore Instance => _instance.Value;

    public List<Organization> Organizations { get; } = new();
    public List<Warehouse> Warehouses { get; } = new();
    public List<Product> Products { get; } = new();
    public List<Invoice> Invoices { get; } = new();

    public int NextOrganizationId { get; set; } = 1;
    public int NextWarehouseId { get; set; } = 1;
    public int NextProductId { get; set; } = 1;
    public int NextInvoiceId { get; set; } = 1;

    public InMemoryDataStore()
    {
        SeedTestData();
    }

    private static string Photo(string fileName) => Path.Combine(AppContext.BaseDirectory, "Images", fileName);

    public void SeedTestData()
    {
        Organizations.Clear();
        Warehouses.Clear();
        Products.Clear();
        Invoices.Clear();

        var org1 = new Organization { Id = NextOrganizationId++, Name = "Рога и копыта, ООО" };
        var org2 = new Organization { Id = NextOrganizationId++, Name = "Пупкин и сыновья, ООО" };
        Organizations.Add(org1);
        Organizations.Add(org2);

        var wh1 = new Warehouse { Id = NextWarehouseId++, OrganizationId = org1.Id, Name = "Склад Рогов и Копыта №1", Address = "г. Москва, ул. Складская, д. 1" };
        var wh2 = new Warehouse { Id = NextWarehouseId++, OrganizationId = org1.Id, Name = "Склад Рогов и Копыта №2", Address = "г. Москва, ул. Складская, д. 2" };
        var wh3 = new Warehouse { Id = NextWarehouseId++, OrganizationId = org2.Id, Name = "Склад Пупкина №1", Address = "г. Санкт-Петербург, пр. Невский, д. 10" };
        Warehouses.Add(wh1);
        Warehouses.Add(wh2);
        Warehouses.Add(wh3);

        Products.Add(new Product
        {
            Id = NextProductId++,
            OrganizationId = org1.Id,
            WarehouseId = wh1.Id,
            Name = "Молоток слесарный",
            PhotoPath = Photo("hammer.png"),
            Category = "Инструменты",
            Manufacturer = "ЗИЛ",
            Supplier = "ООО ИнструментТорг",
            Price = 450m,
            Stock = 25,
            DiscountPercent = 0
        });

        Products.Add(new Product
        {
            Id = NextProductId++,
            OrganizationId = org1.Id,
            WarehouseId = wh1.Id,
            Name = "Отвёртка крестовая",
            PhotoPath = Photo("screwdriver.png"),
            Category = "Инструменты",
            Manufacturer = "Stanley",
            Supplier = "ООО ИнструментТорг",
            Price = 180m,
            Stock = 60,
            DiscountPercent = 10
        });

        Products.Add(new Product
        {
            Id = NextProductId++,
            OrganizationId = org1.Id,
            WarehouseId = wh2.Id,
            Name = "Кабель ВВГ 3x2.5",
            PhotoPath = Photo("cable.png"),
            Category = "Электрика",
            Manufacturer = "Камкабель",
            Supplier = "ООО ЭлектроСнаб",
            Price = 75.5m,
            Stock = 500,
            DiscountPercent = 5
        });

        Products.Add(new Product
        {
            Id = NextProductId++,
            OrganizationId = org2.Id,
            WarehouseId = wh3.Id,
            Name = "Бумага офисная А4",
            PhotoPath = Photo("paper.png"),
            Category = "Канцелярия",
            Manufacturer = "SvetoCopy",
            Supplier = "ООО КанцОпт",
            Price = 320m,
            Stock = 150,
            DiscountPercent = 0
        });
    }
}
