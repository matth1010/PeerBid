using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auction.DataLayer.Models
{
    public interface IDataManager
    {
        Task InsertBid(BidDTO bid);
        Task<string> InsertAuction(AuctionItemDTO auction);
        Task<BidDTO> GetWinningBidForAuction(string auctionId);
        Task<List<AuctionItemDTO>> GetAllAuctions();
        Task DeleteAuction(string auctionId);
        Task DeleteBidsForAuction(string auctionId);
        Task<AuctionItemDTO?> GetAuctionById(string auctionId);
    }
}
