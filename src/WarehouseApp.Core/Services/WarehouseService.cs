using WarehouseApp.Core.Data;
using WarehouseApp.Core.Exceptions;
using WarehouseApp.Core.Models;

namespace WarehouseApp.Core.Services;

public class WarehouseService : IWarehouseService
{
    private readonly InMemoryDataStore _store;

    public WarehouseService(InMemoryDataStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public IReadOnlyList<Warehouse> GetByOrganization(int organizationId) =>
        _store.Warehouses.Where(w => w.OrganizationId == organizationId)
                          .OrderBy(w => w.Name)
                          .ToList();

    public Warehouse GetById(int id) =>
        _store.Warehouses.FirstOrDefault(w => w.Id == id)
        ?? throw new ValidationException($"Склад с Id={id} не найден.");

    public Warehouse Add(int organizationId, string name, string? address)
    {
        name = (name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Наименование склада не может быть пустым.");

        if (!_store.Organizations.Any(o => o.Id == organizationId))
            throw new ValidationException("Указанная организация не найдена.");

        if (_store.Warehouses.Any(w => w.OrganizationId == organizationId &&
                                        w.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new ValidationException($"У этой организации уже есть склад \"{name}\".");

        var warehouse = new Warehouse
        {
            Id = _store.NextWarehouseId++,
            OrganizationId = organizationId,
            Name = name,
            Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim()
        };
        _store.Warehouses.Add(warehouse);
        return warehouse;
    }

    public void Update(int id, string name, string? address)
    {
        name = (name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Наименование склада не может быть пустым.");

        var warehouse = GetById(id);

        if (_store.Warehouses.Any(w => w.Id != id && w.OrganizationId == warehouse.OrganizationId &&
                                        w.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new ValidationException($"У этой организации уже есть склад \"{name}\".");

        warehouse.Name = name;
        warehouse.Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
    }

    public void Delete(int id)
    {
        var warehouse = GetById(id);

        _store.Products.RemoveAll(p => p.WarehouseId == id);
        _store.Invoices.RemoveAll(inv => inv.WarehouseId == id);
        _store.Warehouses.Remove(warehouse);
    }
}
