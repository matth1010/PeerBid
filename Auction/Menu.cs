using Auction.Data.Models;
using Auction.Data.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Auction
{
    public class Menu
    {
        private readonly Peer _peer;
        private readonly IAuctionRepository _auctionRepository;

        public Menu(Peer peer, IAuctionRepository auctionRepository)
        {
            _peer = peer;
            _auctionRepository = auctionRepository;
        }

        public async Task Start()
        {
            string command;
            do
            {
                Console.WriteLine($"\n[{_peer.Name}] [Connected peers: {_peer.ConnectedPeers.Count} @ {DateTime.Now}]");
                Console.WriteLine("Choose an action command: Type 'help' for a list of available commands.");
                command = Console.ReadLine()?.ToLower();

                if (!string.IsNullOrWhiteSpace(command))
                {
                    var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var action = parts[0];
                    var option = parts.Length > 1 ? parts[1] : "";

                    switch (action)
                    {
                        case "auction":
                            await HandleAuctionOption(option);
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

        private async Task HandleAuctionOption(string option)
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
                case "-al":
                    await AuctionListAsync();
                    break;
                case "-bl":
                    await AuctionBidsAsync();
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
            Console.WriteLine("Initializing a new auction...");
            Console.Write("Enter the item description: ");
            var item = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(item))
            {
                Console.WriteLine("Item description cannot be empty.");
                return;
            }

            double price;
            do
            {
                Console.Write("Enter the starting price: $");
            } while (!double.TryParse(Console.ReadLine(), out price) || price <= 0);

            var bid = new AuctionBid { Bidder = _peer.Name, Product = item, Amount = price };

            await _auctionRepository.InitializeAuctionAsync(bid);
            Console.WriteLine("Auction initialized successfully.");
        }

        private async Task AuctionBidAsync()
        {
            Console.WriteLine("Placing a bid...");
            Console.Write("Enter the auction ID: ");
            var auctionId = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(auctionId))
            {
                Console.WriteLine("Invalid auction ID. Please enter a non-empty value.");
                return;
            }

            var auction = await _auctionRepository.GetAuctionByIdAsync(auctionId);
            if (auction == null)
            {
                Console.WriteLine($"Auction with ID '{auctionId}' does not exist.");
                return;
            }

            if (auction.Status != AuctionStatusCode.Open)
            {
                Console.WriteLine($"Auction {auction.Id} is already closed.");
                return;
            }

            double bidAmount;
            var winningBid = await _auctionRepository.GetHighestBid(auctionId);
            do
            {
                if(winningBid != null)
                    Console.Write($"Current winning bid: ${winningBid.Amount}.\n");
                else
                    Console.Write($"Starting bid: ${auction.Price}.\n");

                Console.Write($"Enter your bid amount: $");
            } while (!double.TryParse(Console.ReadLine(), out bidAmount) || bidAmount <= 0);

            if (auction.Price >= bidAmount)
            {
                Console.WriteLine($"Your bid amount must exceed the starting bid for this auction, which is ${auction.Price}.");
                return;
            }

            if (winningBid != null && bidAmount <= winningBid.Amount)
            {
                Console.WriteLine($"Your bid amount must be higher than the current highest bid of ${winningBid.Amount}.");
                return;
            }

            var bid = new AuctionBid { AuctionId = auctionId, Bidder = _peer.Name, Product = auction.Item, Amount = bidAmount };
            await _auctionRepository.AddBidAsync(bid);
            Console.WriteLine("Bid placed successfully.");
        }

        private async Task AuctionComplete()
        {
            Console.WriteLine("Completing an auction...");
            Console.Write("Enter the auction ID: ");
            var auctionId = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(auctionId))
            {
                Console.WriteLine("Invalid auction ID. Please enter a non-empty value.");
                return;
            }

            var auction = await _auctionRepository.GetAuctionByIdAsync(auctionId);
            if (auction == null)
            {
                Console.WriteLine($"Auction with ID '{auctionId}' does not exist.");
                return;
            }

            if (auction.Status != AuctionStatusCode.Open)
            {
                Console.WriteLine($"Illegal status transition in auction {auction.Id}. Can't move from {auction.Status} to {AuctionStatusCode.Closed}");
                return;
            }

            // Check if the current peer is the seller of the auction
            if (auction.Seller != _peer.Name)
            {
                Console.WriteLine($"Only the author of the auction ({auction.Seller}) can complete it.");
                return;
            }

            var winningBid = await _auctionRepository.GetHighestBid(auctionId);
            if (winningBid == null)
            {
                Console.WriteLine($"Auction '{auctionId}' doesn't have any bids yet.");
                return;
            }

            Console.WriteLine($"Auction '{auctionId}' has been completed.");
            Console.WriteLine($"The highest bidder is {winningBid.Bidder} with a bid of ${winningBid.Amount} for product '{winningBid.Product}'.");

            Console.Write("Do you wish to complete the auction now? (Y/N): ");
            var answer = Console.ReadLine()?.ToUpper();

            if (answer != null && answer.Equals("Y"))
            {
                await _auctionRepository.CompleteAuction(auctionId, winningBid);
                Console.WriteLine($"Auction '{auctionId}' completed successfully.");
            }
            else
            {
                Console.WriteLine("Auction completion canceled. Bidding continues...");
            }
        }

        private async Task AuctionListAsync()
        {
            Console.WriteLine("Fetching list of auctions...");
            var auctions = await _auctionRepository.GetCurrentCachedAuctions();

            if (auctions.Any())
            {
                Console.WriteLine("List of current auctions: ");
                foreach (var auction in auctions)
                {
                    string highestBidInfo = auction.GetHighestBid() != null ? $" | Highest Bid: {auction.GetHighestBid().Amount}" : "";
                    Console.WriteLine($" -> ID: {auction.Id} | Author: {auction.Seller} | Item: {auction.Item} | Price: ${auction.Price}{highestBidInfo} | Status: {auction.Status}");
                }
            }
            else
            {
                Console.WriteLine("No auctions found.");
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

        private async Task AuctionBidsAsync()
        {
            Console.Write("Enter the auction ID: ");
            var auctionId = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(auctionId))
            {
                Console.WriteLine("Invalid auction ID. Please enter a non-empty value.");
                return;
            }

            Console.WriteLine($"Fetching bids for auction '{auctionId}'...");
            var auction = await _auctionRepository.GetAuctionByIdAsync(auctionId);

            if (auction != null && auction.Bids.Count > 0)
            {
                // Sort bids by amount (descending order)
                var sortedBids = auction.Bids.OrderByDescending(b => b.Amount).ToList();

                Console.WriteLine($"List of bids for auction '{auctionId}': ");
                foreach (var bid in sortedBids)
                {
                    Console.WriteLine($" -> Bidder: {bid.Bidder} | Product: {bid.Product} | Amount: ${bid.Amount} | Date: {bid.TimeStamp}");
                }
            }
            else
            {
                Console.WriteLine($"No bids found for auction '{auctionId}'.");
            }
        }

        private static void Help()
        {
            Console.WriteLine("List of commands in this CLI: ");
            Console.WriteLine("auction -i -> Initializes a new auction.");
            Console.WriteLine("auction -b -> Places a bid into an existing auction.");
            Console.WriteLine("auction -c -> Completes an ongoing auction.");
            Console.WriteLine("auction -al -> Lists all existing auctions.");
            Console.WriteLine("auction -bl -> Lists all existing auction bids.");
            Console.WriteLine("peer -> Lists all known peers.");
            Console.WriteLine("clear -> Clears the console.");
            Console.WriteLine("exit -> Exits the auction system.");
        }
    }
}