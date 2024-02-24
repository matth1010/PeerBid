using Auction.Data.Models;
using AuctionModel = Auction.Data.Models.Auction;

namespace Auction.Data.Repositories
{
    public interface IAuctionRepository
    {
        /// <summary>
        /// Adds a new auction to the db
        /// </summary>
        /// <param name="auction"></param>
        public void AddAuction(AuctionModel auction);

        /// <summary>
        /// Retrieves an auction by it's unique identifier
        /// </summary>
        /// <param name="auctionId"></param>
        /// <returns></returns>
        public AuctionModel? GetAuction(string auctionId);

        /// <summary>
        /// Adds a new bid into an existing auction
        /// </summary>
        /// <param name="bid"></param>
        public void AddBid(AuctionBid bid);

        /// <summary>
        /// Updates an existing auction
        /// </summary>
        /// <param name="auction"></param>
        public void UpdateAuction(AuctionModel auction);

        /// <summary>
        /// Gets all current auctions
        /// </summary>
        /// <returns></returns>
        public List<AuctionModel> GetCurrentAuctions();
    }
}