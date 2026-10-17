namespace WarehouseApp.Core.Services;

public class WarehouseData
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
}

public class ProductData
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Supplier { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public decimal DiscountPercent { get; set; }
    public string? PhotoPath { get; set; }
}
