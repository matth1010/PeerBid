using System;
using System.Collections.Generic;

namespace Auction.Data.Models
{
    public class Peer
    {
        private readonly string _baseUrl = "localhost";
        private readonly HashSet<string> _notifiedPeers = new HashSet<string>();

        public Guid Id { get; private set; }
        public int Port { get; set; }
        public string Name { get; set; }
        public string Address => $"{_baseUrl}:{Port}";
        public Dictionary<string, string> ConnectedPeers { get; set; } = new Dictionary<string, string>();

        public Peer(int port, string name)
        {
            Id = Guid.NewGuid();
            Port = port;
            Name = name;

            AddConnectedPeerIfNotExists(Address, Name);
        }

        public void AddConnectedPeerIfNotExists(string address, string name)
        {
            if (ConnectedPeers.ContainsKey(address))
                return;

            ConnectedPeers.Add(address, name);

            foreach (var peer in ConnectedPeers)
            {
                // Check if the notification has already been sent to this peer
                if (peer.Key != address && !_notifiedPeers.Contains(peer.Key))
                {
                    // Send a message to other peers informing them about the new peer
                    Console.WriteLine($"New peer connected on port {address.Split(':')[1]}: Peer '{name}' joined the network.");
                    Console.WriteLine($"[{Name}] [Connected peers: {ConnectedPeers.Count} @ {DateTime.Now}]");

                    _notifiedPeers.Add(peer.Key); // Add the peer to the notified list
                }
            }
        }
    }
}