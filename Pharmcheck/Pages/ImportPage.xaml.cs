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
        List<ProductImport> outputRows = [];
        public ImportPage()
        {
            InitializeComponent();
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
                    var reader = new StreamReader(filePath);
                    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        Delimiter = ";",
                    };
                    var csv = new CsvReader(reader, config);
                    var outputRows = csv.GetRecords<ProductImport>();
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
            public required string ProductID { get; set; }
            public required string ProductName { get; set; }
            public int PriceMin { get; set; }
            public int PriceMax { get; set; }
        }
    }
}
