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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
    }
}
