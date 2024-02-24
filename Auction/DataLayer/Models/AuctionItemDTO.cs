using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Auction.Data.Models;

namespace Auction.DataLayer.Models
{
    public class AuctionItemDTO
    {
        public string AuctionId { get; set; }
        public AuctionStatusCode Status { get; set; } = AuctionStatusCode.Unknown;
        public string Item { get; set; }
        public double Price { get; set; }
        public string Seller { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
