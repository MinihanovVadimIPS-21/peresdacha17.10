using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using WarehouseApp.Core.Models;
using WarehouseApp.Core.Services;
using WarehouseApp.Wpf.Common;

namespace WarehouseApp.Wpf.Views;

public partial class ProductEditDialog : Window
{
    private readonly System.Windows.Media.Brush _normalBorder;

    public ProductData Result { get; private set; } = new();

    public ProductEditDialog()
    {
        InitializeComponent();
        _normalBorder = NameTextBox.BorderBrush;
        Loaded += (_, _) => NameTextBox.Focus();
    }

    public void LoadFrom(Product product)
    {
        NameTextBox.Text = product.Name;
        CategoryTextBox.Text = product.Category;
        ManufacturerTextBox.Text = product.Manufacturer;
        SupplierTextBox.Text = product.Supplier;
        PriceTextBox.Text = product.Price.ToString(CultureInfo.InvariantCulture);
        StockTextBox.Text = product.Stock.ToString(CultureInfo.InvariantCulture);
        DiscountTextBox.Text = product.DiscountPercent.ToString(CultureInfo.InvariantCulture);
        PhotoPathTextBox.Text = product.PhotoPath ?? string.Empty;
    }

    private void BrowsePhoto_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Title = "Выберите фото товара",
            Filter = "Изображения (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|Все файлы (*.*)|*.*"
        };
        if (dlg.ShowDialog() == true)
            PhotoPathTextBox.Text = dlg.FileName;
    }

    private void ResetHighlight()
    {
        foreach (var tb in new[] { NameTextBox, PriceTextBox, StockTextBox, DiscountTextBox })
        {
            tb.BorderBrush = _normalBorder;
            tb.BorderThickness = new Thickness(1);
        }
        ErrorText.Text = string.Empty;
    }

    private bool Fail(TextBox field, string message)
    {
        field.BorderBrush = (System.Windows.Media.Brush)FindResource("DangerBrush");
        field.BorderThickness = new Thickness(2);
        ErrorText.Text = message;
        DialogHelper.ShowError(message, "Ошибка валидации");
        field.Focus();
        return false;
    }

    private bool TryParseDecimal(string text, out decimal value) =>
        decimal.TryParse(text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out value) ||
        decimal.TryParse(text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out value);

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        ResetHighlight();

        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
{ Fail(NameTextBox, "Поле \"Наименование\" не может быть пустым."); return; }

        if (!TryParseDecimal(PriceTextBox.Text, out var price))
{ Fail(PriceTextBox, "Поле \"Цена\" должно быть числом."); return; }
        if (price < 0)
{ Fail(PriceTextBox, "Цена не может быть отрицательной."); return; }

        if (!int.TryParse(StockTextBox.Text.Trim(), out var stock) || stock < 0)
{ Fail(StockTextBox, "Поле \"Остаток\" должно быть целым неотрицательным числом."); return; }

        decimal discount = 0;
        if (!string.IsNullOrWhiteSpace(DiscountTextBox.Text) && !TryParseDecimal(DiscountTextBox.Text, out discount))
{ Fail(DiscountTextBox, "Поле \"Скидка\" должно быть числом."); return; }
        if (discount < 0 || discount > 100)
{ Fail(DiscountTextBox, "Скидка должна быть в диапазоне от 0 до 100."); return; }

        Result = new ProductData
        {
            Name = NameTextBox.Text.Trim(),
            Category = CategoryTextBox.Text.Trim(),
            Manufacturer = ManufacturerTextBox.Text.Trim(),
            Supplier = SupplierTextBox.Text.Trim(),
            Price = price,
            Stock = stock,
            DiscountPercent = discount,
            PhotoPath = string.IsNullOrWhiteSpace(PhotoPathTextBox.Text) ? null : PhotoPathTextBox.Text.Trim()
        };
        DialogResult = true;
    }
}
