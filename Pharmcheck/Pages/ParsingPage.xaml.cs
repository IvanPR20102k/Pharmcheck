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
        List<Comparison> resultsList = [];
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
                //Создание очереди для парсинга
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

                            int requestStatus = Aptekalegko.GetRequestStatus(productPage);

                            int shops = Aptekalegko.GetShops(productPage);

                            byte parsingStatus;
                            if (price > product.PriceMin && price < product.PriceMax) parsingStatus = 1;
                            else parsingStatus = 2;

                            Comparison newComparison = new()
                            {
                                RequestStatus = requestStatus,
                                ComparisonDateTime = DateTime.Now.ToString(),
                                Price = price,
                                ShopsAmount = shops,
                                ParsingStatus = parsingStatus
                            };
                            product.Comparisons.Add(newComparison);
                            product.Status = parsingStatus;
                            Helper.GetDb().SaveChanges();
                            resultsList.Add(newComparison);
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

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            resultsList.Clear();
            DataGridResults.Items.Refresh();
        }
    }
}
