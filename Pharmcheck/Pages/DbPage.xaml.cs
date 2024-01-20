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

namespace Pharmcheck.Pages
{
    /// <summary>
    /// Логика взаимодействия для DbPage.xaml
    /// </summary>
    public partial class DbPage : Page
    {
        ApplicationContext db = new();
        public DbPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            db.Database.EnsureCreated();
            //db.Add(new Pharmacy { Name = "Аптека легко" });
            //db.SaveChanges();
            DataGridPharmacies.ItemsSource = db.Pharmacies.ToList();
        }
    }
}
