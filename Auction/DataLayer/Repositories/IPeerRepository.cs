using Auction.Data.Models;

namespace Auction.Data.Repositories
{
    public interface IPeerRepository
    {
        /// <summary>
        /// Adds a new peer into the db
        /// </summary>
        /// <param name="peer"></param>
        public void AddPeer(Peer peer);

        /// <summary>
        /// Gets all the current peers
        /// </summary>
        /// <returns></returns>
        public List<Peer> GetPeers();
    }
}
