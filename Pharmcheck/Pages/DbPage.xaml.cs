using Pharmcheck.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CsvHelper;
using System.IO;
using System.Globalization;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore;
using System.Windows.Data;
using System.Windows.Media;
using System.CodeDom;

namespace Pharmcheck.Pages
{
    /// <summary>
    /// Логика взаимодействия для DbPage.xaml
    /// </summary>
    public partial class DbPage : Page
    {
        int productSelection = 0, comparisonSelection = 0;
        public DbPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Helper.GetDb().Database.EnsureCreated();
            Helper.GetDb().Pharmacies.Load();
            Helper.GetDb().Imports.Load();
            Helper.GetDb().Products.Load();
            Helper.GetDb().Comparisons.Load();

            if (Helper.GetDb().Pharmacies.FirstOrDefault() ==  null)
            {
                string[] titles = ["Аптека легко"];
                foreach (string title in titles)
                {
                    Helper.GetDb().Add(new Pharmacy { Name = title });
                }
                Helper.GetDb().SaveChanges();
            }

            DataGridPharmacies.ItemsSource = Helper.GetDb().Pharmacies.ToList();
            DataGridPharmacies.Items.Refresh();
        }

        private void ButtonExportTest_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridImports.SelectedItem is not Import import) { return; }
            List<ProductExport> outputRecords = [];
            foreach (Product product in import.Products)
            {
                Comparison lastComparison = Helper.GetDb().Comparisons.Where(c => c.ProductID == product.ID).OrderBy(c => c.ComparisonDateTime).Last();
                ProductExport export = new()
                {
                    ProductID = product.ShopID,
                    ProductName = product.Name,
                    PriceMin = product.PriceMin,
                    PriceReal = lastComparison.Price,
                    PriceMax = product.PriceMax,
                    Percentage = lastComparison.Percentage,
                    ComparisonDateTime = lastComparison.ComparisonDateTime,
                    Shops = lastComparison.ShopsAmount,
                    Status = lastComparison.ParsingStatus
                };
                outputRecords.Add(export);
            }
            string date = DateTime.Now.ToShortDateString();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string name = import.Pharmacy.Name;
            string path = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\{name}_{date}.csv";
            using var writer = new StreamWriter(new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite), Encoding.GetEncoding(1251));
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
            };
            using var csvWriter = new CsvWriter(writer, config);

            csvWriter.WriteRecords(outputRecords);

            MessageBox.Show($"Успех! Файл с именем {name}_{date} успешно создан.");
        }

        private class ProductExport
        {
            public string ProductID { get; set; } = null!;
            public string ProductName { get; set; } = null!;
            public float PriceMin { get; set;}
            public float PriceReal { get; set;}
            public float PriceMax { get; set;}
            public required string Percentage { get; set;}
            public string ComparisonDateTime { get; set; } = null!;
            public int Shops { get; set; }
            public int Status { get; set; }

        }

        private void DataGridPharmacies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadImports();
        }
        private void DataGridImports_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadProducts();
        }
        private void DataGridProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadComparisons();
        }

        private void TextBoxPharmacySearch_TextChanged(object sender, TextChangedEventArgs e)
        {
        }
        private void TextBoxImportSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadImports();
        }
        private void TextBoxProductsSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadProducts();
        }
        private void TextBoxComparisonsSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadComparisons();
        }

        private void ButtonImportsRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadImports();
        }
        private void ButtonProductsRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadProducts();
        }
        private void ButtonComparisonsRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadComparisons();
        }

        private void LoadImports()
        {
            Pharmacy selectedPharmacy = (Pharmacy)DataGridPharmacies.SelectedItem;
            if (selectedPharmacy == null) { return; }
            TextBlockPharmacy.Text = selectedPharmacy.Name;
            DataGridImports.ItemsSource = Helper.GetDb().Imports.Where(i => i.PharmacyID == selectedPharmacy.ID)
                .Where(i => i.ImportDateTime.Contains(TextBoxImportSearch.Text)).ToList();
            DataGridComparisons.ItemsSource = null;
            TextBlockMin.Text = "";
            TextBlockMax.Text = "";
        }
        private void LoadProducts()
        {
            Import selectedImport = (Import)DataGridImports.SelectedItem;
            if (selectedImport == null) { return; }
            TextBlockImport.Text = $"Импорт {selectedImport.ID} от {selectedImport.ImportDateTime}";

            var pr = Helper.GetDb().Products.Where(p => p.ImportID == selectedImport.ID)
                .Where(p => p.ShopID.Contains(TextBoxProductsSearch.Text) || p.Name.Contains(TextBoxProductsSearch.Text)).ToList();

            RButtonProductAll.Content = $" Все: {pr.Count} ";
            int none = 0, green = 0, red = 0, error = 0;
            foreach ( Product p in pr )
            {
                switch (p.Status)
                {
                    case 0: none++; break;
                    case 1: green++; break;
                    case 2: red++; break;
                    case 3: error++; break;
                }
            }
            RButtonProductGray.Content = $" Нет данных: {none} ";
            RButtonProductGreen.Content = $" Соответствует: {green} ";
            RButtonProductRed.Content = $" Не соответствует: {red} ";
            RButtonProductYellow.Content = $" Ошибка: {error} ";

            switch (productSelection)
            {
                case 0: DataGridProducts.ItemsSource = pr; break;
                case 1: DataGridProducts.ItemsSource = pr.Where(p => p.Status == 0); break;
                case 2: DataGridProducts.ItemsSource = pr.Where(p => p.Status == 1); break;
                case 3: DataGridProducts.ItemsSource = pr.Where(p => p.Status == 2); break;
                case 4: DataGridProducts.ItemsSource = pr.Where(p => p.Status == 3); break;
            }
            //ProductGridResize();
        }
        private void LoadComparisons()
        {
            Product selectedProduct = (Product)DataGridProducts.SelectedItem;
            if (selectedProduct == null) { return; }
            RButtonComparisonAll.Content = $" Все: {DataGridComparisons.Items.Count} ";

            TextBlockMin.Text = selectedProduct.PriceMin.ToString();
            TextBlockMax.Text = selectedProduct.PriceMax.ToString();

            var co = Helper.GetDb().Comparisons.Where(c => c.ProductID == selectedProduct.ID)
                .Where(c => c.ComparisonDateTime.Contains(TextBoxComparisonsSearch.Text)).ToList();

            RButtonComparisonAll.Content = $" Все: {co.Count} ";
            int green = 0, red = 0, error = 0;
            foreach (Comparison c in co)
            {
                switch (c.ParsingStatus)
                {
                    case 1: green++; break;
                    case 2: red++; break;
                    case 3: error++; break;
                }
            }
            RButtonComparisonGreen.Content = $" Соответствует: {green} ";
            RButtonComparisonRed.Content = $" Не соответствует: {red} ";
            RButtonComparisonYellow.Content = $" Ошибка: {error} ";

            switch (comparisonSelection)
            {
                case 0: DataGridComparisons.ItemsSource = co; break;
                case 1: DataGridComparisons.ItemsSource = co.Where(с => с.ParsingStatus == 1); break;
                case 2: DataGridComparisons.ItemsSource = co.Where(с => с.ParsingStatus == 2); break;
                case 3: DataGridComparisons.ItemsSource = co.Where(с => с.ParsingStatus == 3); break;
            }
        }

        //private void ProductGridResize()
        //{
        //    GridLengthConverter gridLengthConverter = new();
        //    if (ColumnProduct.ActualWidth < SelectionBarProduct.ActualWidth + 10)
        //    {
        //        ColumnProduct.Width = (GridLength)gridLengthConverter.ConvertFrom(SelectionBarProduct.ActualWidth + 10);
        //    }
        //    else
        //    {
        //        ColumnProduct.Width = (GridLength)gridLengthConverter.ConvertFrom("*");
        //    }
        //}

        private void Page_Initialized(object sender, EventArgs e)
        {
            
        }

        private void RButtonProductAll_Checked(object sender, RoutedEventArgs e)
        {
            productSelection = 0;
            LoadProducts();
        }
        private void RButtonProductGray_Checked(object sender, RoutedEventArgs e)
        {
            productSelection = 1;
            LoadProducts();
        }
        private void RButtonProductGreen_Checked(object sender, RoutedEventArgs e)
        {
            productSelection = 2;
            LoadProducts();
        }
        private void RButtonProductRed_Checked(object sender, RoutedEventArgs e)
        {
            productSelection = 3;
            LoadProducts();
        }
        private void RButtonProductYellow_Checked(object sender, RoutedEventArgs e)
        {
            productSelection = 4;
            LoadProducts();
        }

        private void RButtonComparisonAll_Checked(object sender, RoutedEventArgs e)
        {
            comparisonSelection = 0;
            LoadComparisons();
        }
        private void RButtonComparisonGreen_Checked(object sender, RoutedEventArgs e)
        {
            comparisonSelection = 1;
            LoadComparisons();
        }
        private void RButtonComparisonRed_Checked(object sender, RoutedEventArgs e)
        {
            comparisonSelection = 2;
            LoadComparisons();
        }
        private void RButtonComparisonYellow_Checked(object sender, RoutedEventArgs e)
        {
            comparisonSelection = 3;
            LoadComparisons();
        }
    }

    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                (byte)0 => System.Windows.Media.Brushes.Gray,
                (byte)1 => new SolidColorBrush(Color.FromRgb(0, 205, 20)), //Зелёный
                (byte)2 => new SolidColorBrush(Color.FromRgb(205, 0, 0)), //Красный
                (byte)3 => new SolidColorBrush(Color.FromRgb(255, 248, 75)), //Жёлтый
                _ => System.Windows.Media.Brushes.Blue,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
