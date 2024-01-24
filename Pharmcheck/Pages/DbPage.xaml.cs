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
        public static ApplicationContext db = new();
        public DbPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            db.Database.EnsureCreated();

            if (db.Pharmacies.FirstOrDefault() ==  null)
            {
                List<string> titles = ["Аптека легко"];
                foreach (string title in titles)
                {
                    db.Add(new Pharmacy { Name = title });
                }
                db.SaveChanges();
            }

            DataGridPharmacies.ItemsSource = db.Pharmacies.ToList();
            DataGridPharmacies.Items.Refresh();
        }

        private void DataGridPharmacies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataGridPharmacies.SelectedItem is not Pharmacy selectedPharmacy) { return; }
            TextBlockPharmacy.Text = $"{selectedPharmacy.Name} | Поиск:";
            DataGridImports.ItemsSource = db.Imports.Where(Import => Import.PharmacyID == selectedPharmacy.ID).ToList();
            DataGridImports.Items.Refresh();
        }

        private void DataGridImports_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataGridImports.SelectedItem is not Import selectedImport) { return; }
            TextBlockImport.Text = $"Импорт от {selectedImport.ImportDateTime} | Поиск:";
            DataGridProducts.ItemsSource = db.Products.Where(Product => Product.ImportID == selectedImport.ID).ToList();
            DataGridProducts.Items.Refresh();
        }

        private void DataGridProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataGridProducts.SelectedItem is not Product selectedProduct) { return; }
            DataGridComparisons.ItemsSource = db.Comparisons.Where(Comparison => Comparison.ProductID == selectedProduct.ID).ToList();
            DataGridComparisons.Items.Refresh();
        }

        private void ButtonExportTest_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridImports.SelectedItem is not Import selectedImport) { return; }
            List<ProductExport> outputRecords = [];
            foreach (Product product in selectedImport.Products)
            {
                ProductExport export = new()
                {
                    ProductID = product.ShopID,
                    ProductName = product.Name,
                    PriceMin = product.PriceMin,
                    PriceReal = product.Comparisons.First().Price,
                    PriceMax = product.PriceMax,
                    ComparisonDateTime = product.Comparisons.First().ComparisonDateTime,
                    IsOutOfBounds = product.Comparisons.First().IsOutOfBounds,
                    Shops = product.Comparisons.First().ShopsAmount
                };
                outputRecords.Add(export);
            }
            string date = DateTime.Now.ToShortDateString();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using var writer = new StreamWriter(new FileStream($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\ParserOutput{date}.csv", FileMode.OpenOrCreate, FileAccess.ReadWrite), Encoding.GetEncoding(1251));
            //using var writer = new StreamWriter($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\ParserOutput{date}.csv");
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
            };
            using var csvWriter = new CsvWriter(writer, config);

            csvWriter.WriteRecords(outputRecords);
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
    }
}
