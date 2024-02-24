using System.Collections.Generic;
using Auction.Data.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Auction.Data.Repositories
{
    public class PeerRepository : IPeerRepository
    {
        private readonly IMemoryCache _cache;
        private readonly string _peersKey = "peers";

        public PeerRepository(IMemoryCache cache)
        {
            _cache = cache;
        }

        public void AddPeer(Peer peer)
        {
            List<Peer> peers;
            if (!_cache.TryGetValue(_peersKey, out peers))
            {
                peers = new List<Peer>();
            }
            peers.Add(peer);
            _cache.Set(_peersKey, peers);
        }

        public List<Peer> GetPeers()
        {
            return _cache.Get<List<Peer>>(_peersKey) ?? new List<Peer>();
        }
    }
}