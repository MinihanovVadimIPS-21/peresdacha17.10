namespace WarehouseApp.Core.Models;

public class Organization
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public override string ToString() => Name;
}
