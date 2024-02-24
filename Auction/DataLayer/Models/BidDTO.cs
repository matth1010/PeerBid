using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auction.DataLayer.Models
{
    public class BidDTO
    {
        public string AuctionId { get; set; }
        public string Bidder { get; set; }
        public string Product { get; set; }
        public double Amount { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
