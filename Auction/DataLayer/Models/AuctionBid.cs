namespace Auction.Data.Models
{
    public class AuctionBid
    {
        public string AuctionId { get; set; }
        public string Product { get; set; }
        public double Amount { get; set; }
        public string Bidder { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
