using WarehouseApp.Core.Models;

namespace WarehouseApp.Core.Services;

public interface IOrganizationService
{
    IReadOnlyList<Organization> GetAll();
    Organization GetById(int id);
    Organization Add(string name);
    void Update(int id, string name);

    void Delete(int id, string confirmationName);
}
