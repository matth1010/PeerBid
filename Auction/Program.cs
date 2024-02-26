using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Auction.Application.Auction;
using Auction.Application.Peer;
using Auction.Data.Models;
using Auction.Data.Repositories;
using Auction.DataLayer.Interfaces;
using Auction.DataLayer.Repositories.Profiles;
using Auction.Services;
using AutoMapper;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using SQLitePCL;

namespace Auction
{
    class Program
    {
        static void Main(string[] args)
        {
            // Initialize SQLitePCL
            Batteries.Init();

            Console.WriteLine("Welcome to PeerBid\n");
            Console.WriteLine("-------------------------------------");

            // Initialize services and start the server
            InitializeAndStartServer().GetAwaiter().GetResult();
        }

        static async Task InitializeAndStartServer()
        {
            Console.Clear();

            Console.WriteLine("Welcome to PeerBid\n");
            Console.WriteLine("-------------------------------------");

            // Read peer name and port
            var peerName = ReadInput("Enter name for peer: ") ?? Guid.NewGuid().ToString();
            if (!int.TryParse(ReadInput($"Enter a port for {peerName}: "), out int peerPort))
            {
                Console.WriteLine("Invalid port. Closing...");
                return;
            }

            var peer = new Peer(peerPort, peerName);

            var serviceProvider = ConfigureServices(peer);

            await StartServer(serviceProvider, peer);
        }

        static IServiceProvider ConfigureServices(Peer peer)
        {
            var dbFilePath = Path.Combine(Environment.CurrentDirectory, "PeerBid");

            return new ServiceCollection()
                .AddMemoryCache()
                .AddAutoMapper(typeof(AuctionMappingProfile).Assembly)
                .AddScoped<IPeerRepository, PeerRepository>()
                .AddScoped(provider => peer)
                .AddScoped<IAuctionRepository, AuctionRepository>()
                .AddScoped<ISQLiteDataManager, SQLiteDataManager>(provider => new SQLiteDataManager(dbFilePath))
                .AddScoped(provider => new AuctionRequestHandler(peer))
                .BuildServiceProvider();
        }

        static async Task StartServer(IServiceProvider serviceProvider, Peer peer)
        {
            try
            {
                var server = new Server
                {
                    Services =
                    {
                        PeerHandler.BindService(new PeerService(peer)),
                        AuctionHandler.BindService(new AuctionService(serviceProvider.GetRequiredService<IAuctionRepository>()))
                    },
                    Ports = { new ServerPort("localhost", peer.Port, ServerCredentials.Insecure) }
                };

                server.Start();
                Console.WriteLine($"Server listening on port: {peer.Port}\n");

                // Connect to other peers if desired
                await ConnectToPeers(serviceProvider, peer);

                Console.Clear();

                var repositories = new
                {
                    peer = serviceProvider.GetRequiredService<IPeerRepository>(),
                    auction = serviceProvider.GetRequiredService<IAuctionRepository>()
                };

                var menu = new Menu(peer, repositories.auction);
                await menu.Start();

                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
            catch (IOException ioEx)
            {
                Console.WriteLine($"Error starting server. Maybe the port is already in use. Message: {ioEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting server: {ex.Message}");
            }
        }

        static async Task ConnectToPeers(IServiceProvider serviceProvider, Peer peer)
        {
            var fellowPeerPortInput = ReadInput("Enter a port to connect to peer or press ENTER to continue: ");
            if (!int.TryParse(fellowPeerPortInput, out int fellowPeerPort))
                return;

            // Update connected peers
            var peerRepo = serviceProvider.GetRequiredService<IPeerRepository>();
            Console.WriteLine($"Connected peers: {peerRepo.GetConnectedPeersCount()}");

            await RetryConnect(peer, fellowPeerPort);

            async Task RetryConnect(Peer peer, int fellowPeerPort)
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
                    catch (RpcException ex)
                    {
                        Console.WriteLine($"Attempt {attempt} failed: {ex.Message}");
                        Thread.Sleep(1000);
                        attempt++;
                    }
                }

                if (connected)
                {
                    // Update connected peers count after successful connection
                    Console.WriteLine("Connected to peer successfully.");
                    peerRepo.UpdateConnectedPeersCount(1);
                }
                else
                {
                    Console.WriteLine("Failed to connect after multiple attempts.");
                    // Handle the failure appropriately (e.g., exit the application or show an error message)
                }
            }
        }

        static string ReadInput(string message)
        {
            Console.Write(message);
            return Console.ReadLine();
        }
    }
}