using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using YahooFinanceApi;

using MyWatchlist.Models;

namespace MyWatchlist
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Declare WatchlistStocks list
        List<WatchlistStocks> wlStocks = new List<WatchlistStocks>();

        // Declare xmlfile path (the file that stores all data)
        string xmlFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyWatchlist");

        // Declare xmlfile path and file name
        string xmlFileFullPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyWatchlist\\data.xml");

        public MainWindow()
        {
            InitializeComponent();

            // Subscribe StateChanged to State changed event
            StateChanged += MainWindowStateChangeRaised;

            // CreateDirectory will check if folder exists and, if not, create it.
            // If folder exists then CreateDirectory will do nothing.
            Directory.CreateDirectory(xmlFilePath);

            // Declare new document
            XmlDocument document = new XmlDocument();

            // Create a xml file if not exists
            if (!File.Exists(xmlFileFullPath))
            {
                document.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?><WATCHLISTS></WATCHLISTS>");
                document.Save(xmlFileFullPath);
            }
            else
                // If exist load it
                document.Load(xmlFileFullPath);

            GetIndexPrice();
            GetWatchlist();

        }

        // This is for the custom titlebar
        #region --- ChromeWindow ---
        // Can execute
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        // Minimize
        private void CommandBinding_Executed_Minimize(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        // Maximize
        private void CommandBinding_Executed_Maximize(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(this);
        }

        // Restore
        private void CommandBinding_Executed_Restore(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this);
        }

        // Close
        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        // State change
        private void MainWindowStateChangeRaised(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                MainWindowBorder.BorderThickness = new Thickness(8);
                RestoreButton.Visibility = Visibility.Visible;
                MaximizeButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                MainWindowBorder.BorderThickness = new Thickness(0);
                RestoreButton.Visibility = Visibility.Collapsed;
                MaximizeButton.Visibility = Visibility.Visible;
            }
        }
        #endregion

        #region -- Events --
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Declare and open a new add window
            AddWindow addwindow = new AddWindow(cbLists.SelectedIndex);

            // If new stock added, refresh watchliststocks
            if (addwindow.ShowDialog() == true)
                GetWatchListStocks();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            // If any stock is selected
            if (lstStocks.SelectedItem != null)
            {
                // Remove and update stocks
                WatchlistStocks stock = (WatchlistStocks)lstStocks.SelectedItems[0];
                if (MessageBox.Show("Are you sure you want to delete " + stock.name + " from " + stock.watchlist + "?", "MyWatchlist", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    RemoveWatchlistStock(stock.ticker, stock.watchlist);
                    GetWatchListStocks();
                }
            }
            // If no stock selected
            else if (lstStocks.Items.Count == 0)
            {
                // Remove and update watchlists
                if (MessageBox.Show("Are you sure you want to delete " + cbLists.SelectedItem.ToString() + "?", "MyWatchlist", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    var xdoc = XDocument.Load(xmlFileFullPath);

                    // Remove watchlist with name of selected combobox items name
                    xdoc.Root.Elements("WATCHLIST").Where(x =>
                        x.Element("NAME").Value == cbLists.SelectedItem.ToString()).Single().Remove();
                    xdoc.Save(xmlFileFullPath);
                    GetWatchlist();
                }
            }
        }

        private void btnAddWL_Click(object sender, RoutedEventArgs e)
        {
            // If textbox has chars, add new watchlist and update list
            if (!string.IsNullOrWhiteSpace(txtNewList.Text))
            {
                AddWatchlist(txtNewList.Text);
                GetWatchlist();

                // Select new watchlist
                cbLists.SelectedIndex = cbLists.Items.Count - 1;

                // Clear textbox
                txtNewList.Text = string.Empty;
            }
        }

        private void cbLists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Update datagrid and listview and stocks
            dgStocks.Items.Refresh();
            lstStocks.Items.Refresh();
            GetWatchListStocks();
        }
        #endregion

        #region -- Methods --

        // Get index prices and Market change percent
        async void GetIndexPrice()
        {
            // Declare tickers and fields
            var securities = await Yahoo.Symbols("^OMX", "^OMXSPI", "^IXIC", "^GSPC").Fields(Field.RegularMarketPrice, Field.RegularMarketChangePercent).QueryAsync();

            // Declare every index 
            var omxData = securities["^OMX"];
            var omxspiData = securities["^OMXSPI"];
            var nasdaqData = securities["^IXIC"];
            var spData = securities["^GSPC"];

            // Add to each lbl
            lblOmx.Content = "OMXS: " + omxData.RegularMarketPrice + "(" + Math.Round(omxData.RegularMarketChangePercent, 2) +"%)";
            lblOmxspi.Content = "OMXSPI: " + omxspiData.RegularMarketPrice + "(" + Math.Round(omxspiData.RegularMarketChangePercent, 2) + "%)";
            lblNasdaq.Content = "S&P 500: " + nasdaqData.RegularMarketPrice + "(" + Math.Round(nasdaqData.RegularMarketChangePercent, 2) + "%)";
            lblSp500.Content = "Nasdaq: " + spData.RegularMarketPrice + "(" + Math.Round(spData.RegularMarketChangePercent, 2) + "%)";
        }

        // Get watchlists
        public void GetWatchlist()
        {
            // Clear combobox
            cbLists.Items.Clear();

            // Declare and open xml document
            var xdoc = XDocument.Load(xmlFileFullPath);

            // Declare watchlists location in xml document
            var watchlists = xdoc.Root.Descendants("WATCHLIST").Select(x => new Watchlist(x.Element("NAME").Value));

            // Declare stocks location in xml document
            var stocks = xdoc.Root.Descendants("STOCK").Select(x => new WatchlistStocks(x.Element("NAME").Value, x.Element("TICKER").Value, double.Parse(x.Element("AVGPRICE").Value), int.Parse(x.Element("SHARES").Value), x.Attribute("list").Value));

            // Add every watchlist to combobox
            foreach (var watchlist in watchlists)
                cbLists.Items.Add(watchlist.name);

            // If combobox includes watchlists, select first watchlist
            if (cbLists.Items.Count > 0)
                cbLists.SelectedIndex = 0;
        }

        // Get stocks for each watchlist
        public async void GetWatchListStocks()
        {
            // If any watchlist exists
            if (cbLists.Items.Count > 0)
            {
                // Declare xml document and open
                var xdoc = XDocument.Load(xmlFileFullPath);

                // Declare watchlists location in xml document
                var watchlists = xdoc.Root.Descendants("WATCHLIST").Select(x => new Watchlist(x.Element("NAME").Value));

                // Declare stocks location in xml document
                var stocks = xdoc.Root.Descendants("STOCK").Select(x => new WatchlistStocks(x.Element("NAME").Value, x.Element("TICKER").Value, double.Parse(x.Element("AVGPRICE").Value.Replace(",", ".")), int.Parse(x.Element("SHARES").Value), x.Attribute("list").Value));

                // Clear WatchlistStocks list
                wlStocks.Clear();

                // Add every stock to respective watchlist
                foreach (var stock in stocks)
                {
                    // If stock parameter equals the selected watchlist, add to datagrid and listview
                    if (stock.watchlist.ToString() == cbLists.SelectedItem.ToString())
                    {
                        // Declare tickers and fields
                        var securities = await Yahoo.Symbols(stock.ticker).Fields(Field.Symbol, Field.RegularMarketPrice, Field.RegularMarketChange, Field.RegularMarketChangePercent, Field.Currency).QueryAsync();
                        var stockData = securities[stock.ticker];

                        // Add stock to WatchlistStocks list
                        wlStocks.Add(new WatchlistStocks(stock.name, stock.ticker, stock.avgPrice, stock.shares, stock.watchlist, stockData.RegularMarketPrice, stockData.RegularMarketChange, stockData.RegularMarketChangePercent));

                        // Declare listview and datagrid source to WatchlistStocks list
                        lstStocks.ItemsSource = wlStocks;
                        dgStocks.ItemsSource = wlStocks;

                        // Refresh datagrid and listview
                        dgStocks.Items.Refresh();
                        lstStocks.Items.Refresh();
                    }
                }
            }
            // Refresh datagrid and listview
            dgStocks.Items.Refresh();
            lstStocks.Items.Refresh();
        }

        void AddWatchlist(string name)
        {
            // Declare xml document and open
            var xdoc = XDocument.Load(xmlFileFullPath);

            // Add watchlist to xml document
            var xelement = new XElement("WATCHLIST", new XElement("NAME", name), new XElement("STOCKS"));
            xdoc.Root.Add(xelement);

            // Save xml file
            xdoc.Save(xmlFileFullPath);
        }

        void RemoveWatchlistStock(string ticker, string watchlist)
        {
            // Declare xml document and open
            var xdoc = XDocument.Load(xmlFileFullPath);

            // Remove stock where stocklist name equals selected item in combobox and ticker srquals selected stock in listview
            xdoc.Root.Elements("WATCHLIST").Where(e =>
                e.Element("NAME").Value == watchlist).Single()
                .Element("STOCKS")
                .Elements("STOCK").Where(e =>
                e.Element("TICKER").Value == ticker).Single().Remove();

            // Save xml file
            xdoc.Save(xmlFileFullPath);
        }
        #endregion

    }
}
