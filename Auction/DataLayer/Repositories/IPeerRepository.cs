using System.Collections.Generic;
using Auction.Data.Models;

namespace Auction.Data.Repositories
{
    public interface IPeerRepository
    {
        /// <summary>
        /// Adds a new peer into the repository.
        /// </summary>
        /// <param name="peer">The peer to add.</param>
        void AddPeer(Peer peer);

        /// <summary>
        /// Gets all the current peers from the repository.
        /// </summary>
        /// <returns>A list of peers.</returns>
        List<Peer> GetPeers();

        /// <summary>
        /// Gets the count of connected peers.
        /// </summary>
        /// <returns>The count of connected peers.</returns>
        int GetConnectedPeersCount();

        /// <summary>
        /// Updates the count of connected peers.
        /// </summary>
        /// <param name="count">The new count of connected peers.</param>
        void UpdateConnectedPeersCount(int count);
    }
}