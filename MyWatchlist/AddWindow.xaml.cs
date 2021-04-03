using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using YahooFinanceApi;

using MyWatchlist.Models;

namespace MyWatchlist
{
    /// <summary>
    /// Interaction logic for AddWindow.xaml
    /// </summary>
    public partial class AddWindow : Window
    {

        // Valid ticker prop
        private Boolean _validTicker;
        public Boolean ValidTicker
        {
            get { return _validTicker; }
            set
            {
                // If valid ticker is true, enable add button
                _validTicker = value;
                if (_validTicker)
                    btnAdd.IsEnabled = true;

                // If valid ticker is false, disable add button
                else
                {
                    btnAdd.IsEnabled = false;
                    txtName.Text = string.Empty;
                }
            }
        }

        public AddWindow(int index)
        {
            InitializeComponent();

            // Open in center of parent window
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Owner = Application.Current.MainWindow;

            // Clear combobox
            cbWL.Items.Clear();

            // Declare xmlfile path
            var xmlFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyWatchlist\\data.xml");

            // Declare xmlfile and open
            var xdoc = XDocument.Load(xmlFilePath);

            // Declare watchlist location in xml file
            var watchlists = xdoc.Root.Descendants("WATCHLIST").Select(x => new Watchlist(x.Element("NAME").Value));

            // Declare stocks location in xml file
            var stocks = xdoc.Root.Descendants("STOCK").Select(x => new WatchlistStocks(x.Element("NAME").Value, x.Element("TICKER").Value, double.Parse(x.Element("AVGPRICE").Value), int.Parse(x.Element("SHARES").Value), x.Attribute("list").Value));

            // For every watchlist add to combobox
            foreach (var watchlist in watchlists)
            {
                cbWL.Items.Add(watchlist.name);
            }

            // Select selected combobox index to same as parent combobox
            cbWL.SelectedIndex = index;

        }

        // For custom titlebar
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

        #region -- Events --
        private async void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                // Declare stock and fields
                var securities = await Yahoo.Symbols(txtTicker.Text).Fields(Field.Symbol, Field.RegularMarketPrice, Field.RegularMarketChange, Field.RegularMarketChangePercent, Field.LongName).QueryAsync();
                
                // Declare the stock
                var indexData = securities[txtTicker.Text];

                // Update textbox with the stock name
                txtName.Text = indexData.LongName;

                // Set validticker to true
                ValidTicker = true;

                // Declare xml file path
                var xmlFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyWatchlist\\data.xml");

                // Declare xml document and open
                var xdoc = XDocument.Load(xmlFilePath);

                // Declare location the the stock in xml file
                var hasElement = xdoc.Root.Elements("WATCHLIST").Where(y =>
                y.Element("NAME").Value == cbWL.SelectedItem.ToString()).Single()
                .Element("STOCKS").Elements("STOCK").Any(yy => yy.Element("TICKER").Value == txtTicker.Text);

                // If the location not exists add stock
                if (!hasElement)
                {
                    var xelement = new XElement(new XElement("STOCK", new XAttribute("list", cbWL.SelectedItem.ToString()), new XElement("NAME", txtName.Text), new XElement("TICKER", txtTicker.Text), new XElement("AVGPRICE", txtAvgPrice.Text), new XElement("SHARES", txtShares.Text)));
                    xdoc.Root.Elements("WATCHLIST").Where(x => x.Element("NAME").Value == cbWL.SelectedItem.ToString()).Single().Element("STOCKS").Add(xelement);

                    // Save xml file
                    xdoc.Save(xmlFilePath);

                    // Set dialogresult to true
                    this.DialogResult = true;
                }
                // If the location exists display error message
                else
                    MessageBox.Show(txtTicker.Text + " already exists in " + cbWL.SelectedItem.ToString() + "!", "MyWatchlist", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                // If errors, disable add button
                ValidTicker = false;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            // Close window
            SystemCommands.CloseWindow(this);
        }

        private void txtAvgPrice_KeyDown(object sender, KeyEventArgs e)
        {
            // Only allow numbers and commas
            if (e.Key == Key.OemComma || e.Key == Key.Decimal)
            {
                // If two commas exists, delete last one
                if (txtAvgPrice.Text.Split(',').Length < 2)
                    e.Handled = false;
                else
                    e.Handled = true;
            }
            // If keypress is any digit, allow them.
            else if (e.Key == Key.D1 || e.Key == Key.NumPad1 || e.Key == Key.D2 || e.Key == Key.NumPad2 || e.Key == Key.D3 || e.Key == Key.NumPad3 || e.Key == Key.D4 || e.Key == Key.NumPad4 || e.Key == Key.D5 || e.Key == Key.NumPad5 || e.Key == Key.D6 || e.Key == Key.NumPad6 || e.Key == Key.D7 || e.Key == Key.NumPad7 || e.Key == Key.D8 || e.Key == Key.NumPad8 || e.Key == Key.D9 || e.Key == Key.NumPad9 || e.Key == Key.D0 || e.Key == Key.NumPad0 || e.Key == Key.Tab)
                e.Handled = false;
            // Else disallow
            else
                e.Handled = true;
        }

        private void txtShares_KeyDown(object sender, KeyEventArgs e)
        {
            // If keypress is any digit allow
            if (e.Key == Key.D1 || e.Key == Key.NumPad1 || e.Key == Key.D2 || e.Key == Key.NumPad2 || e.Key == Key.D3 || e.Key == Key.NumPad3 || e.Key == Key.D4 || e.Key == Key.NumPad4 || e.Key == Key.D5 || e.Key == Key.NumPad5 || e.Key == Key.D6 || e.Key == Key.NumPad6 || e.Key == Key.D7 || e.Key == Key.NumPad7 || e.Key == Key.D8 || e.Key == Key.NumPad8 || e.Key == Key.D9 || e.Key == Key.NumPad9 || e.Key == Key.D0 || e.Key == Key.NumPad0 || e.Key == Key.Tab)
                e.Handled = false;
            // Else disallow
            else
                e.Handled = true;
        }

        private async void txtTicker_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get stock information
                // Declare stock and fields
                var securities = await Yahoo.Symbols(txtTicker.Text).Fields(Field.Symbol, Field.LongName).QueryAsync();
                
                // Declare stock
                var indexData = securities[txtTicker.Text];

                // Update textbox name with stock name
                txtName.Text = indexData.LongName;

                // Set validticker to true
                ValidTicker = true;
            }
            catch (Exception)
            {
                // If any errors set validticker to false
                ValidTicker = false;
            }
        }

        private async void txtTicker_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {                
                // Get stock information
                // Declare stock and fields
                var securities = await Yahoo.Symbols(txtTicker.Text).Fields(Field.Symbol, Field.LongName).QueryAsync();
                
                // Declare stock
                var indexData = securities[txtTicker.Text];

                // Update textbox name with stock name
                txtName.Text = indexData.LongName;

                // Set validticker to true
                ValidTicker = true;
            }
            catch (Exception)
            {
                // If any errors set validticker to false
                ValidTicker = false;
            }
        }
        #endregion
    }
}
