namespace Auction.Data.Models
{
    public class AuctionBid
    {
        public string AuctionId { get; set; }
        public string Product { get; set; }
        public double Price { get; set; }
        public string Bidder { get; set; }
    }
}
