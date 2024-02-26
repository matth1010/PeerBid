using Auction.Data.Models;
using AuctionModel = Auction.Data.Models.Auction;

namespace Auction.Data.Repositories
{
    public interface IAuctionRepository
    {
        /// <summary>
        /// Initializes Auction
        /// </summary>
        /// <param name="auction"></param>
        public Task InitializeAuctionAsync(AuctionBid bid);

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
        public Task<AuctionModel?> GetAuctionByIdAsync(string auctionId);

        /// <summary>
        /// Retrieves the highest bid
        /// </summary>
        /// <param name="auctionId"></param>
        /// <returns></returns>
        public Task<AuctionBid?> GetHighestBid(string auctionId);

        /// <summary>
        /// Adds a new bid into an existing auction
        /// </summary>
        /// <param name="bid"></param>
        public Task AddBidAsync(AuctionBid bid);

        /// <summary>
        /// Completes the auction with winning bid
        /// </summary>
        /// <param name="auctionId"></param>
        /// <param name="winningBid"></param>
        public Task CompleteAuction(string auctionId, AuctionBid winningBid);

        /// <summary>
        /// Get all active auctions
        /// </summary>
        /// <param name="auction"></param>
        public Task<List<AuctionModel>> GetAllAuctions();

        /// <summary>
        /// Updates an existing auction
        /// </summary>
        /// <param name="auction"></param>
        public void UpdateAuctionCache(AuctionModel auction);

        /// <summary>
        /// Gets all current auctions
        /// </summary>
        /// <returns></returns>
        public List<AuctionModel> GetCurrentCachedAuctions();
    }
}