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
using AngleSharp.Html.Dom;

namespace Pharmcheck.Pages
{
    /// <summary>
    /// Логика взаимодействия для ParsingPage.xaml
    /// </summary>
    public partial class ParsingPage : Page
    {
        List<Comparison> resultsList = new();
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

        private async void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ComboBoxPharmacies.SelectedItem is not Pharmacy pharmacy) { return; }
                List<Product> queue = pharmacy.Imports.First().Products;
                switch (pharmacy.Name)
                {
                    case "Аптека легко":
                        foreach (var product in queue)
                        {
                            string productLink = $"https://aptekalegko.ru/product/{product.ShopID}";
                            var productPage = await Aptekalegko.GetPage(productLink);
                            float price = Aptekalegko.GetPrice(productPage);
                            int status = 400;
                            if (price > 0) status = 200; 
                            int flag;
                            if (price > product.PriceMax || price < product.PriceMin) flag = 1;
                            else flag = 0;
                            int shops = Aptekalegko.GetShops(productPage);
                            Comparison newComparison = new()
                            {
                                Status = status,
                                ComparisonDateTime = DateTime.Now.ToString(),
                                Price = price,
                                IsOutOfBounds = flag,
                                ShopsAmount = shops,
                            };
                            product.Comparisons.Add(newComparison);
                            DbPage.db.SaveChanges();
                            resultsList.Add(DbPage.db.Comparisons.Where(Comparison => Comparison.ProductID == product.ID).First());
                            DataGridResults.ItemsSource = resultsList;
                            DataGridResults.Items.Refresh();
                            await Task.Run(() => Thread.Sleep(5000));
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
