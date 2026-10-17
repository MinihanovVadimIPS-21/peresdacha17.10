using System.Windows;
using WarehouseApp.Core.Data;
using WarehouseApp.Core.Services;
using WarehouseApp.Wpf.Services;

namespace WarehouseApp.Wpf;

public partial class App : Application
{
    public static AppServices Services { get; } =
        AppServices.Create(new DialogService(), InMemoryDataStore.Instance);
}
