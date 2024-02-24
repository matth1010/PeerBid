using Auction.Data.Models;
using Grpc.Core;

namespace Auction.Services
{
    public class PeerService : PeerHandler.PeerHandlerBase
    {
        private readonly Peer _peer;

        public PeerService(Peer peer)
        {
            _peer = peer;
        }

        public override async Task<PingResponse> Ping(PingRequest request, ServerCallContext context)
        {
            _peer.AddConnectedPeerIfNotExists(request.RequestingPeer.Address, request.RequestingPeer.Name);
            var response = new PingResponse
            {
                KnownPeers =
                {
                    _peer.ConnectedPeers.Select(i => new KnownPeer
                    {
                        Address = i.Key,
                        Name = i.Value
                    })
                }
            };

            return response;
        }
    }
}
