using WarehouseApp.Core.Models;

namespace WarehouseApp.Core.Services;

public interface IWarehouseService
{
    IReadOnlyList<Warehouse> GetByOrganization(int organizationId);
    Warehouse GetById(int id);
    Warehouse Add(int organizationId, string name, string? address);
    void Update(int id, string name, string? address);
    void Delete(int id);
}
