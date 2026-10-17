using WarehouseApp.Core.Data;

namespace WarehouseApp.Core.Services;

public class AppServices
{
    public IOrganizationService Organizations { get; }
    public IWarehouseService Warehouses { get; }
    public IProductService Products { get; }
    public IInvoiceService Invoices { get; }
    public IDialogService Dialogs { get; }

    public AppServices(IOrganizationService organizations, IWarehouseService warehouses,
        IProductService products, IInvoiceService invoices, IDialogService dialogs)
    {
        Organizations = organizations;
        Warehouses = warehouses;
        Products = products;
        Invoices = invoices;
        Dialogs = dialogs;
    }

    public static AppServices Create(IDialogService dialogs, InMemoryDataStore store)
    {
        var products = new ProductService(store);
        return new AppServices(
            new OrganizationService(store),
            new WarehouseService(store),
            products,
            new InvoiceService(store, products),
            dialogs);
    }
}
