using System.Collections.ObjectModel;
using WarehouseApp.Core.Models;
using WarehouseApp.Core.Services;

namespace WarehouseApp.Core.ViewModels;

public class InvoicesViewModel : ViewModelBase
{
    private readonly AppServices _s;

    public Organization Organization { get; }
    public Warehouse Warehouse { get; }
    public ObservableCollection<Invoice> Invoices { get; } = new();

    private Invoice? _selectedInvoice;
    public Invoice? SelectedInvoice
    {
        get => _selectedInvoice;
        set
        {
            if (SetField(ref _selectedInvoice, value))
                RefreshCommands();
        }
    }

    public RelayCommand CreateIncomingCommand { get; }
    public RelayCommand CreateOutgoingCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand ConfirmCommand { get; }
    public RelayCommand DeleteCommand { get; }

    public InvoicesViewModel(AppServices services, Organization organization, Warehouse warehouse)
    {
        _s = services;
        Organization = organization;
        Warehouse = warehouse;

        CreateIncomingCommand = new RelayCommand(() => CreateDraft(InvoiceType.Incoming));
        CreateOutgoingCommand = new RelayCommand(() => CreateDraft(InvoiceType.Outgoing));
        EditCommand = new RelayCommand(Edit, () => SelectedInvoice != null);
        ConfirmCommand = new RelayCommand(Confirm, () => SelectedInvoice?.Status == InvoiceStatus.Draft);
        DeleteCommand = new RelayCommand(Delete, () => SelectedInvoice != null);

        Reload();
    }

    private void RefreshCommands()
    {
        EditCommand.RaiseCanExecuteChanged();
        ConfirmCommand.RaiseCanExecuteChanged();
        DeleteCommand.RaiseCanExecuteChanged();
    }

    public void Reload()
    {
        var keepId = SelectedInvoice?.Id;
        Invoices.Clear();
        foreach (var inv in _s.Invoices.GetByWarehouse(Warehouse.Id))
            Invoices.Add(inv);
        SelectedInvoice = keepId is null ? null : Invoices.FirstOrDefault(i => i.Id == keepId);
    }

    private void CreateDraft(InvoiceType type)
    {
        Invoice? created = null;
        if (!TryExecute(_s.Dialogs, () => created = _s.Invoices.CreateDraft(Organization.Id, Warehouse.Id, type)) || created is null)
            return;
        OpenEditor(created);
    }

    private void Edit()
    {
        if (SelectedInvoice != null) OpenEditor(SelectedInvoice);
    }

    private void OpenEditor(Invoice invoice)
    {
        _s.Dialogs.ShowInvoiceEditDialog(new InvoiceEditViewModel(_s, invoice, Warehouse));
        Reload();
    }

    private void Confirm()
    {
        if (SelectedInvoice is null) return;
        var inv = SelectedInvoice;
        if (!_s.Dialogs.Confirm($"Подтвердить накладную {inv.Number}? Остатки на складе будут изменены.")) return;
        if (TryExecute(_s.Dialogs, () => _s.Invoices.Confirm(inv.Id)))
        {
            _s.Dialogs.ShowMessage($"Накладная {inv.Number} подтверждена, остатки обновлены.");
            Reload();
        }
    }

    private void Delete()
    {
        if (SelectedInvoice is null) return;
        var inv = SelectedInvoice;
        if (!_s.Dialogs.Confirm($"Удалить накладную {inv.Number}?")) return;
        if (TryExecute(_s.Dialogs, () => _s.Invoices.Delete(inv.Id)))
            Reload();
    }
}
