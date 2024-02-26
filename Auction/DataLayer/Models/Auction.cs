using System.Collections.Generic;
using System.Linq;

namespace Auction.Data.Models
{
    public class Auction
    {
        public string Id { get; set; } = string.Empty;
        public string? Item { get; set; }
        public double Price { get; set; }
        public string Seller { get; set; } = string.Empty;
        public AuctionStatusCode Status { get; set; } = AuctionStatusCode.Unknown;
        public List<AuctionBid> Bids { get; set; } = new List<AuctionBid>();

        public Auction() { }

        public AuctionBid? GetHighestBid()
        {
            return Bids
                .OrderByDescending(x => x.Amount)
                .FirstOrDefault();
        }
    }
}