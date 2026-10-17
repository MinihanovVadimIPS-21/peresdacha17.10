using System.ComponentModel;
using System.Runtime.CompilerServices;
using WarehouseApp.Core.Exceptions;
using WarehouseApp.Core.Services;

namespace WarehouseApp.Core.ViewModels;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected static bool TryExecute(IDialogService dialogs, Action action)
    {
        try
        {
            action();
            return true;
        }
        catch (ValidationException ex)
        {
            dialogs.ShowError(ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            dialogs.ShowError($"Непредвиденная ошибка: {ex.Message}");
            return false;
        }
    }
}
