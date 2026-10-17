namespace WarehouseApp.Core.Models;

public class Product
{
    public int Id { get; set; }

    public int OrganizationId { get; set; }

    public int WarehouseId { get; set; }

    public string? PhotoPath { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Manufacturer { get; set; } = string.Empty;

    public string Supplier { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public decimal DiscountPercent { get; set; }

    public decimal PriceWithDiscount => Math.Round(Price * (1 - DiscountPercent / 100m), 2);
}
