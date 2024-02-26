using Auction.Data.Models;
using AuctionModel = Auction.Data.Models.Auction;

namespace Auction.Data.Repositories
{
    public interface IAuctionRepository
    {
        /// <summary>
        /// Initializes an auction asynchronously.
        /// </summary>
        /// <param name="bid">The bid to initialize the auction.</param>
        Task InitializeAuctionAsync(AuctionBid bid);

        /// <summary>
        /// Adds a new auction to the database.
        /// </summary>
        /// <param name="auction">The auction to add.</param>
        void AddAuction(AuctionModel auction);

        /// <summary>
        /// Retrieves an auction by its unique identifier asynchronously.
        /// </summary>
        /// <param name="auctionId">The identifier of the auction.</param>
        /// <returns>The retrieved auction, or null if not found.</returns>
        Task<AuctionModel?> GetAuctionByIdAsync(string auctionId);

        /// <summary>
        /// Retrieves the highest bid for an auction asynchronously.
        /// </summary>
        /// <param name="auctionId">The identifier of the auction.</param>
        /// <returns>The highest bid for the auction, or null if no bids are found.</returns>
        Task<AuctionBid?> GetHighestBid(string auctionId);

        /// <summary>
        /// Adds a new bid to an existing auction asynchronously.
        /// </summary>
        /// <param name="bid">The bid to add.</param>
        Task AddBidAsync(AuctionBid bid);

        /// <summary>
        /// Completes the auction with the winning bid asynchronously.
        /// </summary>
        /// <param name="auctionId">The identifier of the auction.</param>
        /// <param name="winningBid">The winning bid.</param>
        Task CompleteAuction(string auctionId, AuctionBid winningBid);

        /// <summary>
        /// Updates an existing auction asynchronously.
        /// </summary>
        /// <param name="auction">The auction to update.</param>
        Task UpdateAuctionCacheAsync(AuctionModel auction);

        /// <summary>
        /// Gets all current auctions.
        /// </summary>
        /// <returns>A list of all current auctions.</returns>
        Task<List<AuctionModel>> GetCurrentCachedAuctions();
    }
}