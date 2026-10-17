namespace WarehouseApp.Core.Models;

public class Warehouse
{
    public int Id { get; set; }

    public int OrganizationId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Address { get; set; }

    public override string ToString() => Name;
}
