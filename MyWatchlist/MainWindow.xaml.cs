using Microsoft.Win32;
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
                MessageBox.Show("File created!");
            }
            else
            {
                MessageBox.Show("File exists!");
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
            lstStocks.Items.Add("Test Stock");
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lstStocks.SelectedItem != null)
            {
                lstStocks.Items.Remove(lstStocks.SelectedItem);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnAddWL_Click(object sender, RoutedEventArgs e)
        {
            cbLists.Items.Add(txtNewList.Text);
            cbLists.SelectedIndex = cbLists.Items.Count - 1;
        }

        private void cbLists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetWatchListStocks();
        }

        void GetWatchlist()
        {
            var xmlFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyWatchlist\\data.xml");
            var xdoc = XDocument.Load(xmlFilePath);

            var watchlists = xdoc.Root.Descendants("WATCHLIST").Select(x => new Watchlist(int.Parse(x.Attribute("id").Value), x.Element("NAME").Value));

            var stocks = xdoc.Root.Descendants("STOCK").Select(x => new WatchlistStocks(x.Element("NAME").Value, double.Parse(x.Element("AVGPRICE").Value), int.Parse(x.Element("SHARES").Value), x.Attribute("list").Value));

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

            var watchlists = xdoc.Root.Descendants("WATCHLIST").Select(x => new Watchlist(int.Parse(x.Attribute("id").Value), x.Element("NAME").Value));

            var stocks = xdoc.Root.Descendants("STOCK").Select(x => new WatchlistStocks(x.Element("NAME").Value, double.Parse(x.Element("AVGPRICE").Value), int.Parse(x.Element("SHARES").Value), x.Attribute("list").Value));

            foreach (var stock in stocks)
            {
                if (stock.watchlist.ToString() == cbLists.SelectedItem.ToString())
                    lstStocks.Items.Add(stock.name);
            }
        }

        void AddWatchlist()
        {
            //var xmlFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyWatchlist\\data.xml");
            //var xdoc = XDocument.Load(xmlFilePath);
            //var xelement = new XElement("WATCHLIST", new XAttribute("id", watchlist.id), new XElement("NAME", watchlist.name));
            //xdoc.Root.Add(xelement);
            //xdoc.Save(xmlFilePath);

            var xmlFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyWatchlist\\data.xml");
            var xdoc = XDocument.Load(xmlFilePath);

            var xelement = new XElement("WATCHLIST", new XAttribute("id", "1"), new XElement("NAME", "My Watchlist"), new XElement("STOCKS", new XElement("STOCK", new XElement("NAME", "SNES"), new XElement("AVGPRICE", "2.14"), new XElement("SHARES", "121"))));
            xdoc.Root.Add(xelement);
            xdoc.Save(xmlFilePath);

        }
    }


    public class Watchlist
    {
        public int id { get; set; }
        public string name { get; set; }

        public Watchlist(int _id, string _name)
        {
            id = _id;
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
        public double avgPrice { get; set; }
        public int shares { get; set; }
        public string watchlist { get; set; }

        public WatchlistStocks(string _name, double _avgPrice, int _shares, string _watchlist)
        {
            name = _name;
            avgPrice = _avgPrice;
            shares = _shares;
            watchlist = _watchlist;
        }
    }
}
