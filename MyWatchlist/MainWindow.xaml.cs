﻿using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Xml;
using System.Xml.Linq;

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
            {
                //MessageBox.Show("File exists!");
                document.Load(xmlFilePath + "\\data.xml");
            }
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
            //GetWatchlist();
            //AddWatchlist();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddWatchlistStock();
            GetWatchListStocks();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {

            //RemoveWatchlistStock();
            //GetWatchListStocks();

            if (lstStocks.SelectedItem != null)
            {
                lstStocks.Items.Remove(lstStocks.SelectedItem);
            }
            else if (lstStocks.Items.Count == 0)
            {

                cbLists.Items.Remove(cbLists.SelectedItem);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnAddWL_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtNewList.Text))
            {
                AddWatchlist(txtNewList.Text);
                GetWatchlist();
                cbLists.SelectedIndex = cbLists.Items.Count - 1;
            }
        }

        private void cbLists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dgStocks.Items.Refresh();
            GetWatchListStocks();
        }

        void GetWatchlist()
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
        }

        void GetWatchListStocks()
        {
            lstStocks.Items.Clear();
            var xmlFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyWatchlist\\data.xml");
            var xdoc = XDocument.Load(xmlFilePath);

            var watchlists = xdoc.Root.Descendants("WATCHLIST").Select(x => new Watchlist(x.Element("NAME").Value));

            var stocks = xdoc.Root.Descendants("STOCK").Select(x => new WatchlistStocks(x.Element("NAME").Value, x.Element("TICKER").Value, double.Parse(x.Element("AVGPRICE").Value), int.Parse(x.Element("SHARES").Value), x.Attribute("list").Value));

            wlStocks.Clear();

            foreach (var stock in stocks)
            {
                if (stock.watchlist.ToString() == cbLists.SelectedItem.ToString())
                {
                    lstStocks.Items.Add(stock.name);
                    wlStocks.Add(new WatchlistStocks(stock.name, stock.ticker, stock.avgPrice, stock.shares, stock.watchlist));
                    dgStocks.ItemsSource = wlStocks;
                    dgStocks.Items.Refresh();
                }
            }
        }

        void AddWatchlist(string name)
        {
            //var xmlFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyWatchlist\\data.xml");
            //var xdoc = XDocument.Load(xmlFilePath);
            //var xelement = new XElement("WATCHLIST", new XAttribute("id", watchlist.id), new XElement("NAME", watchlist.name));
            //xdoc.Root.Add(xelement);
            //xdoc.Save(xmlFilePath);

            var xmlFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyWatchlist\\data.xml");
            var xdoc = XDocument.Load(xmlFilePath);

            var xelement = new XElement("WATCHLIST", new XElement("NAME", name), new XElement("STOCKS"));
            xdoc.Root.Add(xelement);
            xdoc.Save(xmlFilePath);

        }

        void AddWatchlistStock()
        {
            var xmlFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyWatchlist\\data.xml");
            var xdoc = XDocument.Load(xmlFilePath);

            var xelement = new XElement(new XElement("STOCK", new XAttribute("list", cbLists.SelectedItem.ToString()), new XElement("NAME", "Zaptec"), new XElement("TICKER", "ZAP"), new XElement("AVGPRICE", "52,13"), new XElement("SHARES", "30")));
            xdoc.Root.Elements("WATCHLIST").Where(e => e.Element("NAME").Value == cbLists.SelectedItem.ToString()).Single().Element("STOCKS").Add(xelement);
            xdoc.Save(xmlFilePath);
        }

        void RemoveWatchlistStock()
        {
            var xmlFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyWatchlist\\data.xml");
            var xdoc = XDocument.Load(xmlFilePath);

            xdoc.Root.Elements("WATCHLIST").Where(e =>
                e.Element("NAME").Value == cbLists.SelectedItem.ToString()).Single()
                .Element("STOCKS")
                .Elements("STOCK").Where(e =>
                e.Element("TICKER").Value == "ZAP").Single().Remove();
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

    public class Watchlist1
    {
        public int id { get; set; }
        public string name { get; set; }
        public List<WatchlistStocks> stocks { get; set; }

        public Watchlist1(int _id, string _name, List<WatchlistStocks> _stocks)
        {
            id = _id;
            name = _name;
            stocks = _stocks;
        }
    }

    public class WatchlistStocks
    {
        public string name { get; set; }
        public string ticker { get; set; }
        public double avgPrice { get; set; }
        public int shares { get; set; }
        public string watchlist { get; set; }

        public WatchlistStocks(string _name, string _ticker, double _avgPrice, int _shares, string _watchlist)
        {
            name = _name;
            ticker = _ticker;
            avgPrice = _avgPrice;
            shares = _shares;
            watchlist = _watchlist;
        }
    }
}