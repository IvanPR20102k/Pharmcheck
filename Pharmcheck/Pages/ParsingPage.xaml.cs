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
            ComboBoxPharmacies.ItemsSource = Helper.GetDb().Pharmacies.ToList();
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
                List<int> queue = Helper.GetDb().Imports.Where(i => i.PharmacyID == pharmacy.ID).OrderBy(i => i.ID).Last().Products.Select(p => p.ID).ToList();
                switch (pharmacy.Name)
                {
                    case "Аптека легко":
                        foreach (int id in queue)
                        {
                            Product product = Helper.GetDb().Products.Where(p => p.ID == id).Single();
                            string productLink = $"https://aptekalegko.ru/product/{product.ShopID}";
                            var productPage = await Aptekalegko.GetPage(productLink);
                            float price = Aptekalegko.GetPrice(productPage);
                            int status = Aptekalegko.GetStatus(productPage);
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
                            Helper.GetDb().SaveChanges();
                            resultsList.Add(newComparison);
                            //resultsList.Add(Helper.GetDb().Comparisons.Where(c => c.ProductID == product.ID).OrderBy(c => c.ID).Last());
                            DataGridResults.ItemsSource = resultsList;
                            DataGridResults.Items.Refresh();
                            await Task.Run(() => Thread.Sleep(1000));
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
