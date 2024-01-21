using Pharmcheck.Entities;
using System;
using System.Collections.Generic;
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
using Pharmcheck.Parsers;

namespace Pharmcheck.Pages
{
    /// <summary>
    /// Логика взаимодействия для ParsingPage.xaml
    /// </summary>
    public partial class ParsingPage : Page
    {
        List<Product> resultsList = new();
        public ParsingPage()
        {
            InitializeComponent();
            ComboBoxPharmacies.ItemsSource = DbPage.db.Pharmacies.ToList();
        }

        private void ComboBoxPharmacies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxPharmacies.SelectedItem != null) ButtonStart.IsEnabled = true;
            else ButtonStart.IsEnabled = false;
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            if (ComboBoxPharmacies.SelectedItem is not Pharmacy pharmacy) { return; }
            List<Product> queue = pharmacy.Imports.Last().Products;
            switch (pharmacy.Name)
            {
                case "Аптека легко":
                    foreach (var product in queue)
                    {
                        string productLink = $"https://aptekalegko.ru/product/{product.ShopID}";
                        string productPage = Aptekalegko.GetPage(productLink);
                        string output = Aptekalegko.Parse(productPage);
                        Comparison newComparison = new()
                        {
                            Status = ,
                        };
                        product.Comparisons.Add(newComparison);
                        System.Threading.Thread.Sleep(500);
                    }
                    break;

            }
        }
    }
}
