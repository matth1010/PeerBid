using System;
using System.Collections.Generic;

namespace Auction.Data.Models
{
    public class Peer
    {
        private readonly string _baseUrl = "localhost";

        public Guid Id { get; private set; }
        public int Port { get; set; }
        public string Name { get; set; }
        public string Address => $"{_baseUrl}:{Port}";
        public Dictionary<string, string> ConnectedPeers { get; set; } = new Dictionary<string, string>(); // Changed from []

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
        }
    }
}