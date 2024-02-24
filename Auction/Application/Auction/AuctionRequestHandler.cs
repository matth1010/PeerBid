using Auction.Data.Models;
using Auction.DataLayer.Models;
using Grpc.Core;
using PeerModel = Auction.Data.Models.Peer;

namespace Auction.Application.Auction
{
    public class AuctionRequestHandler
    {
        private readonly PeerModel _authorPeer;

        public AuctionRequestHandler(PeerModel authorPeer)
        {
            _authorPeer = authorPeer;
        }

        /// <summary>
        /// Initializes a new auction.
        /// </summary>
        /// <param name="item">The item to be auctioned.</param>
        /// <param name="price">The starting price of the auction.</param>
        /// <param name="author">The author or creator of the auction.</param>
        public async Task Initialize(string item, double price, string author)
        {
            var auctionId = Guid.NewGuid().ToString("N").ToUpper(); // Use proper GUID format
            foreach (var connectedPeer in _authorPeer.ConnectedPeers)
            {
                try
                {
                    var channel = new Channel(connectedPeer.Key, ChannelCredentials.Insecure);
                    var client = new AuctionHandler.AuctionHandlerClient(channel);
                    await client.InitializeAsync(new AuctionData
                    {
                        AuctionId = auctionId,
                        Item = item,
                        Price = price,
                        Author = author
                    });

                    await channel.ShutdownAsync();
                }
                catch (RpcException ex)
                {
                    Console.WriteLine($"Failed to connect to peer {connectedPeer.Key}: {ex.Status}");
                }
            }
        }

        /// <summary>
        /// Places a bid in an auction.
        /// </summary>
        /// <param name="auctionId">The ID of the auction to place the bid in.</param>
        /// <param name="amount">The amount of the bid.</param>
        /// <param name="author">The author of the bid.</param>
        public async Task PlaceBid(string auctionId, double amount, string author)
        {
            foreach (var connectedPeer in _authorPeer.ConnectedPeers)
            {
                try
                {
                    var channel = new Channel(connectedPeer.Key, ChannelCredentials.Insecure);
                    var client = new AuctionHandler.AuctionHandlerClient(channel);
                    await client.PlaceBidAsync(new BidData
                    {
                        AuctionId = auctionId,
                        Amount = amount,
                        Bidder = author
                    });

                    await channel.ShutdownAsync();
                }
                catch (RpcException ex)
                {
                    Console.WriteLine($"Failed to connect to peer {connectedPeer.Key}: {ex.Status}");
                }
            }
        }

        /// <summary>
        /// Completes an auction.
        /// </summary>
        /// <param name="auctionId">The ID of the auction to complete.</param>
        public async Task Complete(string auctionId, BidDTO highestBid)
        {
            foreach (var connectedPeer in _authorPeer.ConnectedPeers)
            {
                try
                {
                    var channel = new Channel(connectedPeer.Key, ChannelCredentials.Insecure);
                    var client = new AuctionHandler.AuctionHandlerClient(channel);
                    await client.CompleteAsync(new CompletionData
                    {
                        AuctionId = auctionId,
                        HighestBidder = highestBid.Bidder,
                        Price = highestBid.Amount
                    });

                    await channel.ShutdownAsync();
                }
                catch (RpcException ex)
                {
                    Console.WriteLine($"Failed to connect to peer {connectedPeer.Key}: {ex.Status}");
                }
            }
        }
    }
}