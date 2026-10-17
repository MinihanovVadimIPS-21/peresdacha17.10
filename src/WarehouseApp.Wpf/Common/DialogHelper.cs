using System.Windows;

namespace WarehouseApp.Wpf.Common;

public static class DialogHelper
{
    public static void ShowError(string message, string title = "Ошибка") =>
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);

    public static bool Confirm(string message, string title = "Подтверждение") =>
        MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

    public static void ShowInfo(string message, string title = "Готово") =>
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
}
