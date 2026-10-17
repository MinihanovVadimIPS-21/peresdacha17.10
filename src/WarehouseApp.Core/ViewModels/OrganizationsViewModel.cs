using System.Collections.ObjectModel;
using WarehouseApp.Core.Models;
using WarehouseApp.Core.Services;

namespace WarehouseApp.Core.ViewModels;

public class OrganizationsViewModel : ViewModelBase
{
    private readonly AppServices _s;

    public ObservableCollection<Organization> Organizations { get; } = new();

    private Organization? _selectedOrganization;
    public Organization? SelectedOrganization
    {
        get => _selectedOrganization;
        set
        {
            if (SetField(ref _selectedOrganization, value))
                RefreshCommands();
        }
    }

    public RelayCommand AddCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand OpenCommand { get; }

    public OrganizationsViewModel(AppServices services)
    {
        _s = services;
        AddCommand = new RelayCommand(Add);
        EditCommand = new RelayCommand(Edit, () => SelectedOrganization != null);
        DeleteCommand = new RelayCommand(Delete, () => SelectedOrganization != null);
        OpenCommand = new RelayCommand(Open, () => SelectedOrganization != null);
        Reload();
    }

    private void RefreshCommands()
    {
        EditCommand.RaiseCanExecuteChanged();
        DeleteCommand.RaiseCanExecuteChanged();
        OpenCommand.RaiseCanExecuteChanged();
    }

    public void Reload()
    {
        var keepId = SelectedOrganization?.Id;
        Organizations.Clear();
        foreach (var org in _s.Organizations.GetAll())
            Organizations.Add(org);
        SelectedOrganization = keepId is null ? null : Organizations.FirstOrDefault(o => o.Id == keepId);
    }

    private void Add()
    {
        var name = _s.Dialogs.ShowOrganizationDialog(null);
        if (name is null) return;
        if (TryExecute(_s.Dialogs, () => _s.Organizations.Add(name))) Reload();
    }

    private void Edit()
    {
        if (SelectedOrganization is null) return;
        var id = SelectedOrganization.Id;
        var name = _s.Dialogs.ShowOrganizationDialog(SelectedOrganization.Name);
        if (name is null) return;
        if (TryExecute(_s.Dialogs, () => _s.Organizations.Update(id, name))) Reload();
    }

    private void Delete()
    {
        if (SelectedOrganization is null) return;
        var org = SelectedOrganization;
        var typed = _s.Dialogs.ShowDeleteConfirmDialog(org.Name, "организацию");
        if (typed is null) return;
        if (TryExecute(_s.Dialogs, () => _s.Organizations.Delete(org.Id, typed))) Reload();
    }

    private void Open()
    {
        if (SelectedOrganization is null) return;
        _s.Dialogs.ShowWorkspaceWindow(new WorkspaceViewModel(_s, SelectedOrganization));
    }
}
