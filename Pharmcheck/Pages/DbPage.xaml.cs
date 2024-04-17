using Pharmcheck.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
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

namespace Pharmcheck.Pages
{
    /// <summary>
    /// Логика взаимодействия для DbPage.xaml
    /// </summary>
    public partial class DbPage : Page
    {
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
            DataGridProducts.ItemsSource = null;
            DataGridComparisons.ItemsSource = null;
            TextBlockImport.Text = "Выберите импорт";
        }
        private void LoadProducts()
        {
            Import selectedImport = (Import)DataGridImports.SelectedItem;
            if (selectedImport == null) { return; }
            TextBlockImport.Text = $"Импорт {selectedImport.ID} от {selectedImport.ImportDateTime}";

            var pr = Helper.GetDb().Products.Where(p => p.ImportID == selectedImport.ID)
                .Where(p => p.ShopID.Contains(TextBoxProductsSearch.Text) || p.Name.Contains(TextBoxProductsSearch.Text)).ToList();

            TButtonProductAll.Content = $" Все: {pr.Count} ";
            int none = 0, green = 0, red = 0, error = 0;
            foreach ( Product p in pr )
            {
                switch ( p.Status )
                {
                    case 0: none++; break;
                    case 1: green++; break;
                    case 2: red++; break;
                    case 3: error++; break;
                }
            }
            TButtonProductGray.Content = $" Нет данных: {none} ";
            TButtonProductGreen.Content = $" Соответствует: {green} ";
            TButtonProductRed.Content = $" Не соответствует: {red} ";
            TButtonProductYellow.Content = $" Ошибка: {error} ";

            DataGridProducts.ItemsSource = pr;
            DataGridComparisons.ItemsSource = null;
            //ProductGridResize();
        }
        private void LoadComparisons()
        {
            Product selectedProduct = (Product)DataGridProducts.SelectedItem;
            if (selectedProduct == null) { return; }
            DataGridComparisons.ItemsSource = Helper.GetDb().Comparisons.Where(c => c.ProductID == selectedProduct.ID)
                .Where(c => c.ComparisonDateTime.Contains(TextBoxComparisonsSearch.Text)).ToList();
            TButtonComparisonAll.Content = $" Все: {DataGridComparisons.Items.Count} ";
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
    }
}
