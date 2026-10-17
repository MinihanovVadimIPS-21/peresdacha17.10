namespace WarehouseApp.Core.Models;

public enum InvoiceType
{
    Incoming,
    Outgoing
}

public enum InvoiceStatus
{
    Draft,
    Confirmed
}

public class InvoiceItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class Invoice
{
    public int Id { get; set; }

    public int OrganizationId { get; set; }

    public int WarehouseId { get; set; }

    public InvoiceType Type { get; set; }

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public string Number => $"{(Type == InvoiceType.Incoming ? "ПРИХ" : "РАСХ")}-{Id:D4}";

    public string TypeDisplay => Type == InvoiceType.Incoming ? "Приход" : "Расход";

    public string StatusDisplay => Status == InvoiceStatus.Draft ? "Черновик" : "Подтверждена";

    public List<InvoiceItem> Items { get; set; } = new();
}
