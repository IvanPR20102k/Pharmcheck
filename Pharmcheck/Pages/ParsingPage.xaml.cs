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
        private readonly List<Comparison> resultsList = [];
        //bool pause = false;
        //bool stop = false;

        public ParsingPage()
        {
            InitializeComponent();
            ComboBoxPharmacies.ItemsSource = Helper.GetDb().Pharmacies.ToList();
        }

        private void ComboBoxPharmacies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadImports();
        }

        private void LoadImports()
        {
            Pharmacy selectedPharmacy = (Pharmacy)ComboBoxPharmacies.SelectedItem;
            if (selectedPharmacy == null) { return; }
            TextBlockPharmacy.Text = selectedPharmacy.Name;
            DataGridImportsParse.ItemsSource = Helper.GetDb().Imports.Where(i => i.PharmacyID == selectedPharmacy.ID)
                .Where(i => i.ImportDateTime.Contains(TextBoxImportSearch.Text)).ToList();
        }

        private async void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ComboBoxPharmacies.SelectedItem is not Pharmacy pharmacy) { return; }
                if (DataGridImportsParse.SelectedItem is not Import import) { return; }
                ComboBoxPharmacies.IsEnabled = false;
                DataGridImportsParse.IsEnabled = false;
                TextBoxImportSearch.IsEnabled = false;
                ButtonImportsRefresh.IsEnabled = false;

                //Создание очереди для парсинга
                List<int> queue = Helper.GetDb().Imports.Where(i => i.ID == import.ID).First().Products.Select(p => p.ID).ToList();
                int queueCount = queue.Count;

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

                            byte parsingStatus = price >= product.PriceMin && price <= product.PriceMax ? (byte)1 : (byte)2;

                            string percentage = "0";
                            if (price < product.PriceMin)
                            {
                                percentage = $"- {(100 - price / (product.PriceMin / 100)).ToString("0.##", System.Globalization.CultureInfo.CurrentCulture)}%";
                            }
                            if (price > product.PriceMax)
                            {
                                percentage = $"+ {(price / (product.PriceMax / 100) - 100).ToString("0.##", System.Globalization.CultureInfo.CurrentCulture)}%";
                            }

                            Comparison newComparison = new()
                            {
                                RequestStatus = requestStatus,
                                ComparisonDateTime = DateTime.Now.ToString(),
                                Price = price,
                                Percentage = percentage,
                                ShopsAmount = shops,
                                ParsingStatus = parsingStatus
                            };
                            product.Comparisons.Add(newComparison);
                            product.Status = parsingStatus;
                            Helper.GetDb().SaveChanges();
                            resultsList.Add(newComparison);
                            queueCount--;
                            TextBlockInQueue.Text = (queueCount).ToString();
                            TextBlockFinished.Text = (resultsList.Count).ToString();
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
            finally
            {
                ComboBoxPharmacies.IsEnabled = true;
                DataGridImportsParse.IsEnabled = true;
                TextBoxImportSearch.IsEnabled = true;
                ButtonImportsRefresh.IsEnabled = true;
            }
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            resultsList.Clear();
            DataGridResults.Items.Refresh();
        }

        private void DataGridImportsParse_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Import selectedImport = (Import)DataGridImportsParse.SelectedItem;
            if (selectedImport == null)
            {
                ButtonStart.IsEnabled = false;
                return;
            }
            TextBlockInQueue.Text = Helper.GetDb().Products.Where(p => p.ImportID == selectedImport.ID).Count().ToString();
            ButtonStart.IsEnabled = true;
        }

        private void ButtonPause_Click(object sender, RoutedEventArgs e)
        {
            //if (pause == false)
            //{
            //    mre.Reset();
            //    ButtonPause.Content = "Продолжить";
            //}
            //else
            //{
            //    mre.Set();
            //    ButtonPause.Content = "Пауза";
            //}
        }

        private void ButtonImportsRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadImports();
        }

        private void TextBoxImportSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadImports();
        }
    }
}
