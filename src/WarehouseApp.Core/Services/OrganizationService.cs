using WarehouseApp.Core.Data;
using WarehouseApp.Core.Exceptions;
using WarehouseApp.Core.Models;

namespace WarehouseApp.Core.Services;

public class OrganizationService : IOrganizationService
{
    private readonly InMemoryDataStore _store;

    public OrganizationService(InMemoryDataStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public IReadOnlyList<Organization> GetAll() =>
        _store.Organizations.OrderBy(o => o.Name).ToList();

    public Organization GetById(int id) =>
        _store.Organizations.FirstOrDefault(o => o.Id == id)
        ?? throw new ValidationException($"Организация с Id={id} не найдена.");

    public Organization Add(string name)
    {
        name = (name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Наименование компании не может быть пустым.");

        if (_store.Organizations.Any(o => o.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new ValidationException($"Организация с наименованием \"{name}\" уже существует.");

        var org = new Organization { Id = _store.NextOrganizationId++, Name = name };
        _store.Organizations.Add(org);
        return org;
    }

    public void Update(int id, string name)
    {
        name = (name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Наименование компании не может быть пустым.");

        var org = GetById(id);

        if (_store.Organizations.Any(o => o.Id != id && o.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new ValidationException($"Организация с наименованием \"{name}\" уже существует.");

        org.Name = name;
    }

    public void Delete(int id, string confirmationName)
    {
        var org = GetById(id);

        if (!string.Equals((confirmationName ?? string.Empty).Trim(), org.Name, StringComparison.Ordinal))
            throw new ValidationException("Введённое название не совпадает с названием организации. Удаление отменено.");

        var warehouseIds = _store.Warehouses.Where(w => w.OrganizationId == id).Select(w => w.Id).ToList();
        foreach (var whId in warehouseIds)
        {
            _store.Products.RemoveAll(p => p.WarehouseId == whId);
            _store.Invoices.RemoveAll(inv => inv.WarehouseId == whId);
        }
        _store.Warehouses.RemoveAll(w => w.OrganizationId == id);

        _store.Organizations.Remove(org);
    }
}
