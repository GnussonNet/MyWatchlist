using System;

namespace MyWatchlist.Models
{
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
