using WarehouseApp.Core.Models;

namespace WarehouseApp.Core.Services;

public interface IInvoiceService
{
    IReadOnlyList<Invoice> GetByWarehouse(int warehouseId);
    Invoice GetById(int id);

    Invoice CreateDraft(int organizationId, int warehouseId, InvoiceType type);

    void AddItem(int invoiceId, int productId, int quantity);
    void RemoveItem(int invoiceId, int productId);

    void Confirm(int invoiceId);

    void Delete(int invoiceId);
}
