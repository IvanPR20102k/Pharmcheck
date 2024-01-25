using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CsvHelper;
using CsvHelper.Configuration;
using Pharmcheck.Entities;

namespace Pharmcheck.Pages
{
    /// <summary>
    /// Логика взаимодействия для ImportPage.xaml
    /// </summary>
    public partial class ImportPage : Page
    {
        public string fileContent = string.Empty;
        public string filePath = string.Empty;
        private List<ProductImport> outputRows = [];
        public ImportPage()
        {
            InitializeComponent();
            ComboBoxPharmacies.ItemsSource = Helper.GetDb().Pharmacies.ToList();
        }

        private void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new()
                {
                    Filter = "CSV files(*.csv)|*.csv|All files(*.*)|*.*",
                    FilterIndex = 1
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    filePath = openFileDialog.FileName;
                    using var reader = new StreamReader(filePath);
                    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        Delimiter = ";",
                    };
                    using var csv = new CsvReader(reader, config);
                    outputRows = csv.GetRecords<ProductImport>().ToList();
                }
                DataGridPreview.ItemsSource = outputRows;
            }
            catch (Exception exception)
            {
                MessageBox.Show(Convert.ToString(exception), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.Yes);
            }
        }

        public class ProductImport
        {
            public string ProductID { get; set; } = null!;
            public string ProductName { get; set; } = null!;
            public int PriceMin { get; set; }
            public int PriceMax { get; set; }
        }

        private void ButtonImport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ComboBoxPharmacies.SelectedItem is not Pharmacy selectedPharmacy) { return; }
                selectedPharmacy = Helper.GetDb().Pharmacies.Where(p => p.ID == selectedPharmacy.ID).First();
                Import newImport = new()
                {
                    PharmacyID = selectedPharmacy.ID,
                    ImportDateTime = File.GetCreationTime(filePath).ToString(),
                };
                Helper.GetDb().Update(selectedPharmacy);
                foreach (var rawProduct in outputRows)
                {
                    Product newProduct = new()
                    {
                        ShopID = rawProduct.ProductID,
                        Name = rawProduct.ProductName,
                        PriceMin = Convert.ToSingle(rawProduct.PriceMin) / 100,
                        PriceMax = Convert.ToSingle(rawProduct.PriceMax) / 100
                    };
                    newImport.Products.Add(newProduct);
                }
                selectedPharmacy.Imports.Add(newImport);
                Helper.GetDb().SaveChanges();
                MessageBox.Show($"Успех! В аптеку {selectedPharmacy.Name} добавлен новый импорт от {newImport.ImportDateTime}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка!\n{ex}");
            }
        }

        private void ComboBoxPharmacies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxPharmacies.SelectedItem != null || ComboBoxPharmacies.SelectedIndex != -1) ButtonImport.IsEnabled = true;
            else ButtonImport.IsEnabled = false;
        }
    }
}
