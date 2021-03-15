using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace MyWatchlist
{
    /// <summary>
    /// Interaction logic for AddWindow.xaml
    /// </summary>
    public partial class AddWindow : Window
    {
        public AddWindow(int index)
        {
            InitializeComponent();

            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Owner = Application.Current.MainWindow;

            cbWL.Items.Clear();
            var xmlFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyWatchlist\\data.xml");
            var xdoc = XDocument.Load(xmlFilePath);

            var watchlists = xdoc.Root.Descendants("WATCHLIST").Select(x => new Watchlist(x.Element("NAME").Value));

            var stocks = xdoc.Root.Descendants("STOCK").Select(x => new WatchlistStocks(x.Element("NAME").Value, x.Element("TICKER").Value, double.Parse(x.Element("AVGPRICE").Value), int.Parse(x.Element("SHARES").Value), x.Attribute("list").Value));

            foreach (var watchlist in watchlists)
            {
                cbWL.Items.Add(watchlist.name);
            }
            cbWL.SelectedIndex = index;

        }

        #region --- ChromeWindow ---
        // Can execute
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        // Close
        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }
        #endregion

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var xmlFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyWatchlist\\data.xml");
            var xdoc = XDocument.Load(xmlFilePath);

            var xelement = new XElement(new XElement("STOCK", new XAttribute("list", cbWL.SelectedItem.ToString()), new XElement("NAME", txtName.Text), new XElement("TICKER", txtTicker.Text), new XElement("AVGPRICE", txtAvgPrice.Text), new XElement("SHARES", txtShares.Text)));
            xdoc.Root.Elements("WATCHLIST").Where(x => x.Element("NAME").Value == cbWL.SelectedItem.ToString()).Single().Element("STOCKS").Add(xelement);
            xdoc.Save(xmlFilePath);

            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void txtAvgPrice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.OemComma || e.Key == Key.Decimal)
            {
                if (txtAvgPrice.Text.Split(',').Length < 2)
                    e.Handled = false;
                else
                    e.Handled = true;
            }
            else if (e.Key == Key.D1 || e.Key == Key.NumPad1 || e.Key == Key.D2 || e.Key == Key.NumPad2 || e.Key == Key.D3 || e.Key == Key.NumPad3 || e.Key == Key.D4 || e.Key == Key.NumPad4 || e.Key == Key.D5 || e.Key == Key.NumPad5 || e.Key == Key.D6 || e.Key == Key.NumPad6 || e.Key == Key.D7 || e.Key == Key.NumPad7 || e.Key == Key.D8 || e.Key == Key.NumPad8 || e.Key == Key.D9 || e.Key == Key.NumPad9 || e.Key == Key.D0 || e.Key == Key.NumPad0 || e.Key == Key.Tab)
                e.Handled = false;
            else
                e.Handled = true;
        }

        private void txtShares_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.D1 || e.Key == Key.NumPad1 || e.Key == Key.D2 || e.Key == Key.NumPad2 || e.Key == Key.D3 || e.Key == Key.NumPad3 || e.Key == Key.D4 || e.Key == Key.NumPad4 || e.Key == Key.D5 || e.Key == Key.NumPad5 || e.Key == Key.D6 || e.Key == Key.NumPad6 || e.Key == Key.D7 || e.Key == Key.NumPad7 || e.Key == Key.D8 || e.Key == Key.NumPad8 || e.Key == Key.D9 || e.Key == Key.NumPad9 || e.Key == Key.D0 || e.Key == Key.NumPad0 || e.Key == Key.Tab)
                e.Handled = false;
            else
                e.Handled = true;
        }
    }
}
