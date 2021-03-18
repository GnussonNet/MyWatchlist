using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using YahooFinanceApi;

namespace MyWatchlist
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<WatchlistStocks> wlStocks = new List<WatchlistStocks>();

        public MainWindow()
        {
            InitializeComponent();
            StateChanged += MainWindowStateChangeRaised;

            // The folder for the roaming current user 
            // Combine the base folder with your specific folder....
            // CreateDirectory will check if folder exists and, if not, create it.
            // If folder exists then CreateDirectory will do nothing.
            var xmlFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyWatchlist");
            Directory.CreateDirectory(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyWatchlist"));

            XmlDocument document = new XmlDocument();
            if (!File.Exists(xmlFilePath + "\\data.xml"))
            {
                document.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?><WATCHLISTS></WATCHLISTS>");
                document.Save(xmlFilePath + "\\data.xml");
                //MessageBox.Show("File created!");
            }
            else
                document.Load(xmlFilePath + "\\data.xml");

            //GetIndexPrice();
            GetWatchlist();

        }

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

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(cbLists.SelectedItem.ToString());
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddWindow addwindow = new AddWindow(cbLists.SelectedIndex);
            if (addwindow.ShowDialog() == true)
                GetWatchListStocks();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lstStocks.SelectedItem != null)
            {
                WatchlistStocks stock = (WatchlistStocks)lstStocks.SelectedItems[0];
                if (MessageBox.Show("Are you sure you want to delete " + stock.name + " from " + stock.watchlist + "?", "MyWatchlist", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    RemoveWatchlistStock(stock.ticker, stock.watchlist);
                    GetWatchListStocks();
                }
            }
            else if (lstStocks.Items.Count == 0)
            {

                if (MessageBox.Show("Are you sure you want to delete " + cbLists.SelectedItem.ToString() + "?", "MyWatchlist", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    var xmlFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyWatchlist\\data.xml");
                    var xdoc = XDocument.Load(xmlFilePath);

                    xdoc.Root.Elements("WATCHLIST").Where(x =>
                        x.Element("NAME").Value == cbLists.SelectedItem.ToString()).Single().Remove();
                    xdoc.Save(xmlFilePath);
                    GetWatchlist();
                }
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            //GetWatchlist();
            //GetWatchListStocks();
        }

        private void btnAddWL_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtNewList.Text))
            {
                AddWatchlist(txtNewList.Text);
                GetWatchlist();
                cbLists.SelectedIndex = cbLists.Items.Count - 1;
                txtNewList.Text = string.Empty;
            }
        }

        private void cbLists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dgStocks.Items.Refresh();
            lstStocks.Items.Refresh();
            GetWatchListStocks();
        }

        //async void GetIndexPrice()
        //{
        //    var securities = await Yahoo.Symbols("^OMX", "^OMXSPI", "^IXIC", "^GSPC").Fields(Field.RegularMarketPrice).QueryAsync();
        //    var omxData = securities["^OMX"];
        //    var omxspiData = securities["^OMXSPI"];
        //    var nasdaqData = securities["^IXIC"];
        //    var spData = securities["^GSPC"];
        //    lblOmx.Content = "OMXS: " + omxData.RegularMarketPrice;
        //    lblOmxspi.Content = "OMXSPI: " + omxspiData.RegularMarketPrice;
        //    lblNasdaq.Content = "S&P 500: " + nasdaqData.RegularMarketPrice;
        //    lblSp500.Content = "Nasdaq: " + spData.RegularMarketPrice;
        //}

        public void GetWatchlist()
        {
            cbLists.Items.Clear();
            var xmlFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyWatchlist\\data.xml");
            var xdoc = XDocument.Load(xmlFilePath);

            var watchlists = xdoc.Root.Descendants("WATCHLIST").Select(x => new Watchlist(x.Element("NAME").Value));

            var stocks = xdoc.Root.Descendants("STOCK").Select(x => new WatchlistStocks(x.Element("NAME").Value, x.Element("TICKER").Value, double.Parse(x.Element("AVGPRICE").Value), int.Parse(x.Element("SHARES").Value), x.Attribute("list").Value));

            foreach (var watchlist in watchlists)
            {
                cbLists.Items.Add(watchlist.name);
            }

            if (cbLists.Items.Count > 0)
                cbLists.SelectedIndex = 0;
        }

        public async void GetWatchListStocks()
        {
            if (cbLists.Items.Count > 0)
            {
                //lstStocks.Items.Clear();
                var xmlFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyWatchlist\\data.xml");
                var xdoc = XDocument.Load(xmlFilePath);

                var watchlists = xdoc.Root.Descendants("WATCHLIST").Select(x => new Watchlist(x.Element("NAME").Value));

                var stocks = xdoc.Root.Descendants("STOCK").Select(x => new WatchlistStocks(x.Element("NAME").Value, x.Element("TICKER").Value, double.Parse(x.Element("AVGPRICE").Value.Replace(",", ".")), int.Parse(x.Element("SHARES").Value), x.Attribute("list").Value));

                wlStocks.Clear();

                foreach (var stock in stocks)
                {
                    if (stock.watchlist.ToString() == cbLists.SelectedItem.ToString())
                    {
                        //lstStocks.Items.Add(stock.name);
                        var securities = await Yahoo.Symbols(stock.ticker).Fields(Field.Symbol, Field.RegularMarketPrice, Field.RegularMarketChange, Field.RegularMarketChangePercent).QueryAsync();
                        var stockData = securities[stock.ticker];
                        wlStocks.Add(new WatchlistStocks(stock.name, stock.ticker, stock.avgPrice, stock.shares, stock.watchlist, stockData.RegularMarketPrice, stockData.RegularMarketChange, stockData.RegularMarketChangePercent));
                        lstStocks.ItemsSource = wlStocks;
                        dgStocks.ItemsSource = wlStocks;
                        dgStocks.Items.Refresh();
                        lstStocks.Items.Refresh();
                    }
                }
            }
            dgStocks.Items.Refresh();
            lstStocks.Items.Refresh();
        }

        void AddWatchlist(string name)
        {
            var xmlFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyWatchlist\\data.xml");
            var xdoc = XDocument.Load(xmlFilePath);

            var xelement = new XElement("WATCHLIST", new XElement("NAME", name), new XElement("STOCKS"));
            xdoc.Root.Add(xelement);
            xdoc.Save(xmlFilePath);
        }

        void RemoveWatchlistStock(string ticker, string watchlist)
        {
            var xmlFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyWatchlist\\data.xml");
            var xdoc = XDocument.Load(xmlFilePath);

            xdoc.Root.Elements("WATCHLIST").Where(e =>
                e.Element("NAME").Value == watchlist).Single()
                .Element("STOCKS")
                .Elements("STOCK").Where(e =>
                e.Element("TICKER").Value == ticker).Single().Remove();
            xdoc.Save(xmlFilePath);
        }
    }


    public class Watchlist
    {
        public string name { get; set; }

        public Watchlist(string _name)
        {
            name = _name;
        }
    }


    public class WatchlistStocks
    {
        public string name { get; set; }
        public string ticker { get; set; }
        public double avgPrice { get; set; }
        public int shares { get; set; }
        public string watchlist { get; set; }
        public double currentPrice { get; set; }
        public double todayGainV { get; set; }
        public double todayGainP { get; set; }

        public WatchlistStocks(string _name, string _ticker, double _avgPrice, int _shares, string _watchlist)
        {
            name = _name;
            ticker = _ticker;
            avgPrice = _avgPrice;
            shares = _shares;
            watchlist = _watchlist;
        }

        public WatchlistStocks(string _name, string _ticker, double _avgPrice, int _shares, string _watchlist, double _currentPrice, double _todayGainV, double _todayGainP)
        {
            name = _name;
            ticker = _ticker;
            avgPrice = _avgPrice;
            shares = _shares;
            watchlist = _watchlist;
            currentPrice = _currentPrice;
            todayGainV = Math.Round(_todayGainV, 2);
            todayGainP = Math.Round(_todayGainP, 2);
        }

        public override string ToString()
        {
            return this.name.ToString();
        }
    }
}
