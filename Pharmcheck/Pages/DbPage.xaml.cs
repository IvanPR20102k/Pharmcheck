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

namespace Pharmcheck.Pages
{
    /// <summary>
    /// Логика взаимодействия для DbPage.xaml
    /// </summary>
    public partial class DbPage : Page
    {
        int pharmacyId = 0;
        int pharmacyIndex = 0;
        string pharmacySearch = string.Empty;
        int importId = 0;
        int importIndex = 0;
        string importSearch = string.Empty;
        int productId = 0;
        int productIndex = 0;
        string productSearch = string.Empty;
        string comparisonSearch = string.Empty;
        public DbPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Helper.GetDb().Database.EnsureCreated();

            if (Helper.GetDb().Pharmacies.FirstOrDefault() ==  null)
            {
                List<string> titles = ["Аптека легко"];
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
                Comparison lastComparison = Helper.GetDb().Comparisons.Where(c => c.ProductID == product.ID).First();
                ProductExport export = new()
                {
                    ProductID = product.ShopID,
                    ProductName = product.Name,
                    PriceMin = product.PriceMin,
                    PriceReal = lastComparison.Price,
                    PriceMax = product.PriceMax,
                    ComparisonDateTime = lastComparison.ComparisonDateTime,
                    IsOutOfBounds = lastComparison.IsOutOfBounds,
                    Shops = lastComparison.ShopsAmount
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
            public int IsOutOfBounds { get; set;}
            public int Shops { get; set;}

        }

        private void DataGridPharmacies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            pharmacyIndex = DataGridPharmacies.SelectedIndex;
            Pharmacy selectedPharmacy = (Pharmacy)DataGridPharmacies.SelectedItem;
            if (selectedPharmacy == null) { return; }
            pharmacyId = selectedPharmacy.ID;
            TextBlockPharmacy.Text = selectedPharmacy.Name;
            Refresh();
        }
        private void DataGridImports_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            importIndex = DataGridImports.SelectedIndex;
            Import selectedImport = (Import)DataGridImports.SelectedItem;
            if (selectedImport == null) { return; }
            importId = selectedImport.ID;
            TextBlockImport.Text = $"Импорт {selectedImport.ID} от {selectedImport.ImportDateTime}";
            Refresh();
        }
        private void DataGridProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            productIndex = DataGridProducts.SelectedIndex;
            Product selectedProduct = (Product)DataGridProducts.SelectedItem;
            if (selectedProduct == null) { return; }
            productId = selectedProduct.ID;
            Refresh();
        }
        private void DataGridPharmacies_TouchDown(object sender, TouchEventArgs e)
        {
            Refresh();
        }
        private void DataGridImports_TouchDown(object sender, TouchEventArgs e)
        {
            Refresh();
        }
        private void DataGridProducts_TouchDown(object sender, TouchEventArgs e)
        {
            Refresh();
        }
        private void Refresh()
        {
            DataGridPharmacies.ItemsSource = Helper.GetDb().Pharmacies.Where(p => p.Name.Contains(pharmacySearch)).ToList();
            DataGridImports.ItemsSource = Helper.GetDb().Imports.Where(i => i.PharmacyID == pharmacyId)
                .Where(i => i.ImportDateTime.Contains(importSearch)).ToList();
            DataGridProducts.ItemsSource = Helper.GetDb().Products.Where(p => p.ImportID == importId)
                .Where(p => p.ShopID.Contains(productSearch) || p.Name.Contains(productSearch)).ToList();
            DataGridComparisons.ItemsSource = Helper.GetDb().Comparisons.Where(c => c.ProductID == productId)
                .Where(c => c.ComparisonDateTime.Contains(comparisonSearch)).ToList();
        }

        private void TextBoxPharmacySearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            pharmacySearch = TextBoxPharmacySearch.Text;
            Refresh();
        }
        private void TextBoxImportSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            importSearch = TextBoxImportSearch.Text;
            Refresh();
        }
        private void TextBoxProductsSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            productSearch = TextBoxProductsSearch.Text;
            Refresh();
        }
        private void TextBoxComparisonsSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            comparisonSearch = TextBoxComparisonsSearch.Text;
            Refresh();
        }
    }
}
