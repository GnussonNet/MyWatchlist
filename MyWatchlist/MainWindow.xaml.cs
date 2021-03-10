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
            // Set filter to Json only
            OpenFileDialog oFD = new OpenFileDialog();
            oFD.Filter = "Json files (*.json)|*.json";
            oFD.FilterIndex = 2;

            // If dialog result is ok
            if (oFD.ShowDialog() == true)
            {
                cbLists.Items.Clear();
                lstStocks.Items.Clear();

                // Open file
                StreamReader sr = new StreamReader(oFD.FileName);

                // Add json to a string 
                string json = sr.ReadToEnd();

                // Deserialize json to two lists (1 for watchlists and 1 for stocks in the watchlist)
                List<List<WatchList>> IdListList = JsonConvert.DeserializeObject<List<List<WatchList>>>(json);

                // For every watchlist arrays
                for (int i = 0; i < IdListList.Count; i++)
                {
                    // For every object in the arrays (always 1)
                    for (int x = 0; x < IdListList[i].Count; x++)
                    {
                        // Add new root node for every watchlist in arrays (x could be replaced with a hard coded "0")
                        cbLists.Items.Add(IdListList[i][x].list);

                        //WatchList = new WatchList();

                        // For every stock in stocks array
                        for (int y = 0; y < IdListList[i][x].stocks.Count; y++)
                        {


                            // This is for every stock
                            lstStocks.Items.Add(IdListList[i][x].stocks[y].stock);


                            //// Add every stock in stocks array to a node-level 1 in respective parent
                            //tvLists.Nodes[i].Nodes.Add(IdListList[i][x].stocks[y].stock);

                            //// Add every quantity and avgPrice to a node-level 2 in respective parant
                            //tvLists.Nodes[i].Nodes[y].Nodes.Add(IdListList[i][x].stocks[y].quantity);
                            //tvLists.Nodes[i].Nodes[y].Nodes.Add(IdListList[i][x].stocks[y].avgPrice);
                        }
                    }
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

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

        }
    }

    /// <summary>
    /// Object classes
    /// </summary>
    #region -- Object Classes --

    // Object for all watchlists and their stocks
    public class WatchList
    {
        public string list { get; set; }
        public List<WatchListStocks> stocks { get; set; }
    }

    public class WatchListStocks
    {
        public string stock { get; set; }
        public string quantity { get; set; }
        public string avgPrice { get; set; }
    }
    #endregion
}
