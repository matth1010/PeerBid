using Auction.Application.Auction;
using Auction.Data.Models;
using Auction.Data.Repositories;
using Auction.DataLayer.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Auction
{
    public class Menu
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly Peer _peer;
        private readonly IAuctionRepository _auctionRepository;
        private readonly AuctionRequestHandler _auctionRequestHandler;
        private readonly SQLiteDataManager _dataManager;

        public Menu(Peer peer, IAuctionRepository auctionRepository, AuctionRequestHandler auctionRequestHandler, SQLiteDataManager dataManager)
        {
            _peer = peer;
            _auctionRepository = auctionRepository;
            _auctionRequestHandler = auctionRequestHandler;
            _dataManager = dataManager;
        }

        public void Start()
        {
            string? command;
            do
            {
                Console.WriteLine($"\n[{_peer.Name}] [Connected peers: {_peer.ConnectedPeers.Count} @ {DateTime.Now}]");
                Console.WriteLine("Choose an action command: Type 'help' for a list of available commands.");
                command = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(command))
                {
                    var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var action = parts[0].ToLower();
                    var option = parts.Length > 1 ? parts[1].ToLower() : "";

                    switch (action)
                    {
                        case "auction":
                            HandleAuctionOption(option);
                            break;
                        case "peer":
                            PeersList();
                            break;
                        case "help":
                            Help();
                            break;
                        case "exit":
                            Console.WriteLine("Exiting the auction system...");
                            break;
                        default:
                            Console.WriteLine("Invalid command. Type 'help' for a list of available commands.");
                            break;
                    }
                }

            } while (!string.Equals(command, "exit", StringComparison.OrdinalIgnoreCase));
        }

        private async void HandleAuctionOption(string option)
        {
            switch (option)
            {
                case "-i":
                    await AuctionInitialize();
                    break;
                case "-b":
                   await AuctionBidAsync();
                    break;
                case "-c":
                    await AuctionComplete();
                    break;
                case "-l":
                    await AuctionListAsync();
                    break;
                case "clear":
                    Console.Clear();
                    break;
                default:
                    Console.WriteLine("Invalid auction command. Type 'help' for a list of available commands.");
                    break;
            }
        }

        private async Task AuctionInitialize()
        {
            Console.Write("Enter the item: ");
            var item = Console.ReadLine();

            if (string.IsNullOrEmpty(item))
            {
                Console.WriteLine("Invalid item description/title");
                return;
            }

            Console.Write("Enter the price: ");
            if (!double.TryParse(Console.ReadLine(), out double price))
            {
                Console.WriteLine("Invalid price");
                return;
            }

            // Insert auction into database
            var auction = new AuctionItemDTO { Item = item, Price = price, Seller = _peer.Name };
            var auctionId = await _dataManager.InsertAuction(auction);

            Console.WriteLine($"\n >> {_peer.Name} initialized auction with id '{auctionId}' for item '{item}' at ${price}");
        }

        private async Task AuctionBidAsync()
        {
            Console.Write($"Enter the auction id: ");
            var auctionId = Console.ReadLine();

            if (string.IsNullOrEmpty(auctionId))
            {
                Console.WriteLine("Invalid auction id. Please enter a non-empty value.");
                return;
            }

            // Check if the auction exists
            var auction = await _dataManager.GetAuctionById(auctionId);
            if (auction == null)
            {
                Console.WriteLine($"Auction with id '{auctionId}' does not exist.");
                return;
            }

            Console.Write($"Enter the bid amount: ");
            if (!double.TryParse(Console.ReadLine(), out double bidAmount))
            {
                Console.WriteLine("Invalid bid amount");
                return;
            }

            // Check if the bid amount is higher than the current highest bid
            var winningBid = await _dataManager.GetWinningBidForAuction(auctionId);
            if (winningBid != null && bidAmount <= winningBid.Amount)
            {
                Console.WriteLine($"Your bid amount must be higher than the current highest bid of {winningBid.Amount}.");
                return;
            }

            // Insert bid into database
            var bid = new BidDTO { AuctionId = auctionId, Bidder = _peer.Name, Product = auction.Item, Amount = bidAmount };
            await _dataManager.InsertBid(bid);

            await _auctionRequestHandler.PlaceBid(auctionId, bidAmount, _peer.Name);
        }

        private async Task AuctionComplete()
        {
            Console.Write($"Enter the auction id: ");
            var auctionId = Console.ReadLine();

            if (string.IsNullOrEmpty(auctionId))
            {
                Console.WriteLine("Invalid auction id");
                return;
            }

            // Retrieve auction and winning bid from database
            var auction = await _dataManager.GetAuctionById(auctionId);
            if (auction == null)
            {
                Console.WriteLine("Auction not found");
                return;
            }

            auction.Status = AuctionStatusCode.Closed;

            var winningBid = await _dataManager.GetWinningBidForAuction(auctionId);
            if (winningBid == null)
            {
                Console.WriteLine($"Auction {auctionId} doesn't have any bids yet");
                return;
            }

            Console.WriteLine($"Auction {auctionId}'s highest bidder is {winningBid.Bidder} with {winningBid.Amount} for product '{winningBid.Product}'");
            Console.Write("Do you wish to complete the auction now? (Y/N) > ");

            var answer = Console.ReadLine();
            var acceptedAnswers = new string[] { "Y", "N" };

            if (string.IsNullOrEmpty(answer) || !acceptedAnswers.Contains(answer.ToUpper()))
            {
                Console.WriteLine("Invalid answer");
                return;
            }

            if (answer.Equals("Y", StringComparison.InvariantCultureIgnoreCase))
            {
               await _auctionRequestHandler.Complete(auction.AuctionId, winningBid);
            }
            else if (answer.Equals("N", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine("Bidding continues...");
            }

            // Cleanup: Remove auction and bids from database
            _dataManager.DeleteAuction(auctionId);
            _dataManager.DeleteBidsForAuction(auctionId);
        }

        private async Task AuctionListAsync()
        {
            var auctions = await _dataManager.GetAllAuctions();
            Console.WriteLine("List of current auctions: ");
            foreach (var auction in auctions)
            {
                Console.WriteLine($" -> ID: {auction.AuctionId} | Author: {auction.Seller} | Item: {auction.Item} | Status: {auction.Status}");
            }
        }

        private void PeersList()
        {
            Console.WriteLine("List of connected peers: ");
            foreach (var peer in _peer.ConnectedPeers)
            {
                Console.WriteLine($" -> {peer.Value} @ {peer.Key}");
            }
        }

        private static void Help()
        {
            Console.WriteLine("List of commands in this CLI: ");
            Console.WriteLine("auction -i -> Initializes a new auction. An auction ID will be provided. Keep it close.");
            Console.WriteLine("auction -b -> Places a new bid into an existing auction.");
            Console.WriteLine("auction -c -> Completes an ongoing auction.");
            Console.WriteLine("auction -l -> Lists all the existing auctions.");
            Console.WriteLine("peer -> Lists all the existing known peers.");
            Console.WriteLine("clear -> Clears the console");
        }
    }
}