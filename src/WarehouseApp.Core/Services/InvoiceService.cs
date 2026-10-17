using WarehouseApp.Core.Data;
using WarehouseApp.Core.Exceptions;
using WarehouseApp.Core.Models;

namespace WarehouseApp.Core.Services;

public class InvoiceService : IInvoiceService
{
    private readonly InMemoryDataStore _store;
    private readonly IProductService _productService;

    public InvoiceService(InMemoryDataStore store, IProductService productService)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
    }

    public IReadOnlyList<Invoice> GetByWarehouse(int warehouseId) =>
        _store.Invoices.Where(i => i.WarehouseId == warehouseId)
                        .OrderByDescending(i => i.CreatedAt)
                        .ToList();

    public Invoice GetById(int id) =>
        _store.Invoices.FirstOrDefault(i => i.Id == id)
        ?? throw new ValidationException($"Накладная с Id={id} не найдена.");

    public Invoice CreateDraft(int organizationId, int warehouseId, InvoiceType type)
    {
        if (!_store.Warehouses.Any(w => w.Id == warehouseId && w.OrganizationId == organizationId))
            throw new ValidationException("Указанный склад не принадлежит выбранной организации.");

        var invoice = new Invoice
        {
            Id = _store.NextInvoiceId++,
            OrganizationId = organizationId,
            WarehouseId = warehouseId,
            Type = type,
            Status = InvoiceStatus.Draft,
            CreatedAt = DateTime.Now
        };
        _store.Invoices.Add(invoice);
        return invoice;
    }

    public void AddItem(int invoiceId, int productId, int quantity)
    {
        var invoice = GetById(invoiceId);
        EnsureDraft(invoice);

        if (quantity <= 0)
            throw new ValidationException("Количество товара в накладной должно быть больше нуля.");

        var product = _productService.GetById(productId);
        if (product.WarehouseId != invoice.WarehouseId)
            throw new ValidationException("Товар должен принадлежать складу этой накладной.");

        var existing = invoice.Items.FirstOrDefault(i => i.ProductId == productId);
        if (existing != null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            invoice.Items.Add(new InvoiceItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = quantity
            });
        }
    }

    public void RemoveItem(int invoiceId, int productId)
    {
        var invoice = GetById(invoiceId);
        EnsureDraft(invoice);
        invoice.Items.RemoveAll(i => i.ProductId == productId);
    }

    public void Confirm(int invoiceId)
    {
        var invoice = GetById(invoiceId);
        EnsureDraft(invoice);

        if (invoice.Items.Count == 0)
            throw new ValidationException("Нельзя подтвердить пустую накладную без товарных позиций.");

        if (invoice.Type == InvoiceType.Outgoing)
        {
            foreach (var item in invoice.Items)
            {
                var product = _productService.GetById(item.ProductId);
                if (product.Stock < item.Quantity)
                    throw new ValidationException(
                        $"Недостаточно товара \"{product.Name}\" на складе: в наличии {product.Stock}, требуется {item.Quantity}.");
            }
        }

        foreach (var item in invoice.Items)
        {
            var product = _productService.GetById(item.ProductId);
            product.Stock += invoice.Type == InvoiceType.Incoming ? item.Quantity : -item.Quantity;
        }

        invoice.Status = InvoiceStatus.Confirmed;
    }

    public void Delete(int invoiceId)
    {
        var invoice = GetById(invoiceId);
        if (invoice.Status == InvoiceStatus.Confirmed)
            throw new ValidationException("Нельзя удалить подтверждённую накладную — остатки склада уже изменены.");

        _store.Invoices.Remove(invoice);
    }

    private static void EnsureDraft(Invoice invoice)
    {
        if (invoice.Status != InvoiceStatus.Draft)
            throw new ValidationException("Изменять можно только накладную в статусе \"Черновик\".");
    }
}
