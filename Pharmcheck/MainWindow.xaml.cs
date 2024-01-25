using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Pharmcheck.Entities;
using Pharmcheck.Pages;

namespace Pharmcheck
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly DbPage dbPage = new();
        ParsingPage? parsingPage;
        ImportPage? importPage;
        public MainWindow()
        {
            InitializeComponent();
            CheckBoxDbView.IsChecked = true;
        }

        private void CheckBoxDbView_Checked(object sender, RoutedEventArgs e)
        {
            CheckBoxDbView.IsEnabled = false;
            CheckBoxParsing.IsChecked = false;
            CheckBoxParsing.IsEnabled = true;
            CheckBoxImport.IsChecked = false;
            CheckBoxImport.IsEnabled = true;
            MainFrame.Navigate(dbPage);
        }

        private void CheckBoxParsing_Checked(object sender, RoutedEventArgs e)
        {
            CheckBoxDbView.IsChecked = false;
            CheckBoxDbView.IsEnabled = true;
            CheckBoxParsing.IsEnabled = false;
            CheckBoxImport.IsChecked = false;
            CheckBoxImport.IsEnabled = true;
            parsingPage ??= new ParsingPage();
            MainFrame.Navigate(parsingPage);
        }

        private void CheckBoxImport_Checked(object sender, RoutedEventArgs e)
        {
            CheckBoxDbView.IsChecked = false;
            CheckBoxDbView.IsEnabled = true;
            CheckBoxParsing.IsChecked = false;
            CheckBoxParsing.IsEnabled = true;
            CheckBoxImport.IsEnabled = false;
            importPage ??= new ImportPage();
            MainFrame.Navigate(importPage);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }

    public class Helper
    {
        public static ApplicationContext? db;
        public static ApplicationContext GetDb()
        {
            db ??= new ApplicationContext();
            return db;
        }
    }
}