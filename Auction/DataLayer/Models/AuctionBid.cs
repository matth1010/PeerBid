namespace Auction.Data.Models
{
    public class AuctionBid
    {
        public string? AuctionId { get; set; }
        public double Amount { get; set; }
        public string? Bidder { get; set; }
    }
}
