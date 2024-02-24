using System;
using System.Collections.Generic;
using System.Linq;
using Auction.Data.Models;
using Microsoft.Extensions.Caching.Memory;
using AuctionModel = Auction.Data.Models.Auction;

namespace Auction.Data.Repositories
{
    public class AuctionRepository : IAuctionRepository
    {
        private readonly IMemoryCache _cache;
        private readonly string _auctionsKey = "auctions";

        public AuctionRepository(IMemoryCache cache)
        {
            _cache = cache;
        }

        public void AddAuction(AuctionModel auction)
        {
            var auctions = GetCurrentAuctions();
            auctions.Add(auction);
            SaveData(_auctionsKey, auctions);
        }

        public AuctionModel? GetAuction(string auctionId)
        {
            var auctions = GetCurrentAuctions();
            return auctions.FirstOrDefault(x => x.Id.Equals(auctionId, StringComparison.OrdinalIgnoreCase));
        }

        public void AddBid(AuctionBid bid)
        {
            var auctions = GetCurrentAuctions();
            var auction = auctions.FirstOrDefault(x => x.Id == bid.AuctionId);
            if (auction != null)
            {
                auction.Bids.Add(bid);
                SaveData(_auctionsKey, auctions);
            }
        }

        public void UpdateAuction(AuctionModel auction)
        {
            var auctions = GetCurrentAuctions();
            var existingAuction = auctions.FirstOrDefault(x => x.Id == auction.Id);
            if (existingAuction != null)
            {
                auctions.Remove(existingAuction);
                auctions.Add(auction);
                SaveData(_auctionsKey, auctions);
            }
        }

        public List<AuctionModel> GetCurrentAuctions()
        {
            return _cache.Get<List<AuctionModel>>(_auctionsKey) ?? new List<AuctionModel>();
        }

        private void SaveData<T>(string key, T value)
        {
            _cache.Set(key, value, TimeSpan.FromDays(1));
        }
    }
}