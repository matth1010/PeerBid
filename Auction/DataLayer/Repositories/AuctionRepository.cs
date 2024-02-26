using Auction.Application.Auction;
using Auction.Data.Models;
using Auction.DataLayer.Interfaces;
using Auction.DataLayer.Models;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using AuctionModel = Auction.Data.Models.Auction;

namespace Auction.Data.Repositories
{
    public class AuctionRepository : IAuctionRepository
    {
        private readonly IMemoryCache _cache;
        private readonly AuctionRequestHandler _auctionRequestHandler;
        private readonly ISQLiteDataManager _dataManager;
        private readonly Peer _peer;
        private readonly IMapper _mapper;
        private readonly string _auctionsKey = "auctions";

        public AuctionRepository(IMemoryCache cache, AuctionRequestHandler auctionRequestHandler, ISQLiteDataManager dataManager, Peer peer, IMapper mapper)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _auctionRequestHandler = auctionRequestHandler ?? throw new ArgumentNullException(nameof(auctionRequestHandler));
            _dataManager = dataManager ?? throw new ArgumentNullException(nameof(dataManager));
            _peer = peer ?? throw new ArgumentNullException(nameof(peer));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task InitializeAuctionAsync(AuctionBid bid)
        {
            // Insert auction into database
            var auction = new AuctionItemDTO { Item = bid.Product, Price = bid.Price, Seller = _peer.Name, Status = AuctionStatusCode.Open };
            var auctionId = await _dataManager.InsertAuction(auction);

            await _auctionRequestHandler.Initialize(auctionId, bid.Product, bid.Price, _peer.Name);
        }

        public void AddAuction(AuctionModel auction)
        {
            var auctions = GetCurrentCachedAuctions();
            auctions.Add(auction);
            SaveData(_auctionsKey, auctions);
        }

        public async Task<AuctionModel?> GetAuctionByIdAsync(string auctionId)
        {
            var auctions = GetCurrentCachedAuctions();
            var auction = auctions.FirstOrDefault(x => x.Id.Equals(auctionId, StringComparison.OrdinalIgnoreCase));

            if (auction == null)
            {
                var auctionDto = await _dataManager.GetAuctionById(auctionId);
                auction = _mapper.Map<AuctionModel>(auctionDto);

                if (auction != null) AddAuction(auction);
            }

            return auction;
        }

        public async Task<AuctionBid?> GetHighestBid(string auctionId)
        {
            var auction = await GetAuctionByIdAsync(auctionId);
            if (auction != null) return auction.GetHighestBid();

            var winningBid = await _dataManager.GetWinningBidForAuction(auctionId);
            return _mapper.Map<AuctionBid>(winningBid);

        }

        public async Task AddBidAsync(AuctionBid bid)
        {
            var cachedAuctions = GetCurrentCachedAuctions();
            var auction = cachedAuctions?.FirstOrDefault(x => x.Id == bid.AuctionId);
            if (auction != null)
            {
                auction.Bids.Add(bid);
                SaveData(_auctionsKey, cachedAuctions);
            }

            var bidDto = _mapper.Map<BidDTO>(bid);

            await _dataManager.InsertBid(bidDto);

            await _auctionRequestHandler.PlaceBid(bid.AuctionId, bid.Price, bid.Bidder);
        }

        public async Task CompleteAuction(string auctionId, AuctionBid winningBid)
        {
            await _auctionRequestHandler.Complete(auctionId, winningBid);

            await _dataManager.UpdateAuctionStatus(auctionId, winningBid.Price, AuctionStatusCode.Closed);
            await _dataManager.DeleteBidsForAuction(auctionId);
        }

        public async Task<List<AuctionModel>> GetAllAuctions()
        {
            var cachedAuctions = GetCurrentCachedAuctions();
            if (cachedAuctions.Count > 0) return cachedAuctions;

            var auctions = await _dataManager.GetAllAuctions();
            return _mapper.Map<List<AuctionModel>>(auctions);
        }

        public void UpdateAuctionCache(AuctionModel auction)
        {
            var auctions = GetCurrentCachedAuctions();
            var existingAuction = auctions.FirstOrDefault(x => x.Id == auction.Id);
            if (existingAuction != null)
            {
                auctions.Remove(existingAuction);
                auctions.Add(auction);
                SaveData(_auctionsKey, auctions);
            }
        }

        public List<AuctionModel> GetCurrentCachedAuctions()
        {
            return _cache.Get<List<AuctionModel>>(_auctionsKey) ?? new List<AuctionModel>();
        }

        private void SaveData<T>(string key, T value)
        {
            _cache.Set(key, value, TimeSpan.FromDays(1));
        }
    }
}