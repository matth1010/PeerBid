using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Auction.Application.Auction;
using Auction.Application.Peer;
using Auction.Data.Models;
using Auction.Data.Repositories;
using Auction.Services;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using SQLitePCL;

namespace Auction
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Initialize SQLitePCLRaw
            Batteries.Init();

            var serviceProvider = new ServiceCollection()
                .AddMemoryCache()
                .AddScoped<IPeerRepository, PeerRepository>()
                .AddScoped<IAuctionRepository, AuctionRepository>()
                .BuildServiceProvider();

            var repositories = new
            {
                peer = serviceProvider.GetRequiredService<IPeerRepository>(),
                auction = serviceProvider.GetRequiredService<IAuctionRepository>()
            };

            Console.WriteLine("Welcome to the PeerBid\n\n");
            Console.WriteLine("-------------------------------------");

            Console.Write("Enter name for peer: ");
            var peerName = Console.ReadLine() ?? Guid.NewGuid().ToString();

            Console.Write($"Enter a port for {peerName}: ");
            if (!int.TryParse(Console.ReadLine(), out int peerPort))
            {
                Console.WriteLine("Invalid port. Closing...");
                Console.ReadKey();
                return;
            }

            var peer = new Peer(peerPort, peerName);
            try
            {
                var server = new Server
                {
                    Services =
                    {
                        PeerHandler.BindService(new PeerService(peer)),
                        AuctionHandler.BindService(new AuctionService(repositories.auction))
                    },
                    Ports = { new ServerPort("localhost", peer.Port, ServerCredentials.Insecure) }
                };

                server.Start();
                Console.WriteLine($"Server listening on port: {peer.Port}\n");
            }
            catch (IOException ioEx)
            {
                Console.WriteLine($"Error starting server. Maybe the port is already in use, try a different one. Message: {ioEx.Message}");
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting server: {ex.Message}");
            }

            Console.Write("Enter a port to connect to peer or ENTER to continue: ");
            if (int.TryParse(Console.ReadLine(), out int fellowPeerPort))
                await RetryConnect(peer, fellowPeerPort);

            Console.WriteLine($"Connected peers: {peer.ConnectedPeers.Count}\n");
            Console.Clear();

            var dataManager = new SQLiteDataManager(Environment.CurrentDirectory + "/PeerBid");

            var menu = new Menu(peer, repositories.auction, new AuctionRequestHandler(peer), dataManager);
            menu.Start();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        static async Task RetryConnect(Peer peer, int fellowPeerPort)
        {
            int maxAttempts = 3;
            int attempt = 1;
            bool connected = false;

            while (!connected && attempt <= maxAttempts)
            {
                try
                {
                    await PeerRequestHandler.PingFellowPeer(peer, fellowPeerPort);
                    connected = true;
                }
                catch (Grpc.Core.RpcException ex)
                {
                    Console.WriteLine($"Attempt {attempt} failed: {ex.Message}");
                    Thread.Sleep(1000);
                    attempt++;
                }
            }

            if (!connected)
            {
                Console.WriteLine("Failed to connect after multiple attempts.");
                // Handle the failure appropriately (e.g., exit the application or show an error message)
            }
        }
    }
}