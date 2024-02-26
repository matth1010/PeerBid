using Auction.Data.Models;
using Auction.DataLayer.Interfaces;
using Auction.DataLayer.Models;
using Grpc.Core;
using Microsoft.Data.Sqlite;
using System.Data.SQLite;

public class SQLiteDataManager : ISQLiteDataManager, IDisposable
{
    private readonly SqliteConnection _connection;

    public SQLiteDataManager(string databaseFilePath)
    {
        if (!System.IO.File.Exists(databaseFilePath))
        {
            // If the database file doesn't exist, create a new one
            SQLiteConnection.CreateFile(databaseFilePath);
        }

        // Initialize the SQLite connection with the specified database file path
        string connectionString = $"Data Source={databaseFilePath}";
        _connection = new SqliteConnection(connectionString);
        _connection.Open();

        // Initialize the database tables
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        // Create bidding table if it doesn't exist
        string createTableQuery = @"
            CREATE TABLE IF NOT EXISTS Bidding (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                AuctionId TEXT,
                Product TEXT,
                Bidder TEXT,
                Amount REAL,
                Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
            );";

        using (SqliteCommand command = new SqliteCommand(createTableQuery, _connection))
        {
            command.ExecuteNonQuery();
        }

        // Create auction table if it doesn't exist
        string createAuctionTableQuery = @"
            CREATE TABLE IF NOT EXISTS Auction (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                AuctionId TEXT,
                Item TEXT,
                Price REAL,
                Seller TEXT,
                Status INTEGER,
                StartTime DATETIME,
                EndTime DATETIME
            );";

        using (SqliteCommand command = new SqliteCommand(createAuctionTableQuery, _connection))
        {
            command.ExecuteNonQuery();
        }
    }

    public async Task InsertBid(BidDTO bid)
    {
        // Insert bidding information into the SQLite database
        string insertQuery = "INSERT INTO Bidding (AuctionId, Product, Bidder, Amount) VALUES (@AuctionId, @Product, @Bidder, @Amount)";

        using (SqliteCommand command = new SqliteCommand(insertQuery, _connection))
        {
            command.Parameters.AddWithValue("@AuctionId", bid.AuctionId);
            command.Parameters.AddWithValue("@Product", bid.Product);
            command.Parameters.AddWithValue("@Bidder", bid.Bidder);
            command.Parameters.AddWithValue("@Amount", bid.Amount);

            await command.ExecuteNonQueryAsync();
        }
    }

    public async Task<string> InsertAuction(AuctionItemDTO auction)
    {
        // Generate a unique AuctionId
        auction.AuctionId = Guid.NewGuid().ToString();

        // Insert auction information into the SQLite database
        string insertQuery = "INSERT INTO Auction (AuctionId, Item, Price, Seller, Status, StartTime, EndTime) VALUES (@AuctionId, @Item, @Price, @Seller, @Status, @StartTime, @EndTime)";

        using (SqliteCommand command = new SqliteCommand(insertQuery, _connection))
        {
            command.Parameters.AddWithValue("@AuctionId", auction.AuctionId);
            command.Parameters.AddWithValue("@Item", auction.Item);
            command.Parameters.AddWithValue("@Price", auction.Price);
            command.Parameters.AddWithValue("@Seller", auction.Seller);
            command.Parameters.AddWithValue("@Status", auction.Status);
            command.Parameters.AddWithValue("@StartTime", auction.StartTime);
            command.Parameters.AddWithValue("@EndTime", auction.EndTime);

            // Execute the command
            await command.ExecuteNonQueryAsync();
        }

        // Return the AuctionId of the inserted auction
        return auction.AuctionId;
    }

    public async Task<BidDTO> GetWinningBidForAuction(string auctionId)
    {
        // Retrieve winning bid for the given auction from the SQLite database
        string selectQuery = "SELECT Bidder, Product, Amount, Timestamp FROM Bidding WHERE AuctionId = @AuctionId ORDER BY Amount DESC LIMIT 1";

        using (SqliteCommand command = new SqliteCommand(selectQuery, _connection))
        {
            command.Parameters.AddWithValue("@AuctionId", auctionId);

            using (SqliteDataReader reader = await command.ExecuteReaderAsync())
            {
                if (reader.Read())
                {
                    string bidder = reader.GetString(0);
                    string product = reader.GetString(1);
                    double amount = reader.GetDouble(2);
                    DateTime timestamp = reader.GetDateTime(3);

                    return new BidDTO { AuctionId = auctionId, Product = product, Bidder = bidder, Amount = amount, Timestamp = timestamp };
                }
                else
                {
                    return null;
                }
            }
        }
    }

    public async Task<List<AuctionItemDTO>> GetAllAuctions()
    {
        List<AuctionItemDTO> auctions = new List<AuctionItemDTO>();

        // Retrieve all auctions from the SQLite database
        string selectQuery = "SELECT AuctionId, Item, Price, Seller, Status, StartTime, EndTime FROM Auction";

        using (SqliteCommand command = new SqliteCommand(selectQuery, _connection))
        {
            using (SqliteDataReader reader = await command.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    string auctionId = reader.GetString(0);
                    string item = reader.GetString(1);
                    double price = reader.GetDouble(2);
                    string seller = reader.GetString(3);
                    int status = reader.GetInt32(4);
                    DateTime startTime = reader.GetDateTime(5);
                    DateTime endTime = reader.GetDateTime(6);

                    AuctionItemDTO auction = new AuctionItemDTO
                    {
                        AuctionId = auctionId,
                        Item = item,
                        Price = price,
                        Seller = seller,
                        Status = (AuctionStatusCode)status,
                        StartTime = startTime,
                        EndTime = endTime
                    };

                    auctions.Add(auction);
                }
            }
        }

        return auctions;
    }

    public async Task DeleteAuction(string auctionId)
    {
        // Delete auction from the SQLite database
        string deleteQuery = "DELETE FROM Auction WHERE AuctionId = @AuctionId";

        using (SqliteCommand command = new SqliteCommand(deleteQuery, _connection))
        {
            command.Parameters.AddWithValue("@AuctionId", auctionId);
            await command.ExecuteNonQueryAsync();
        }
    }

    public async Task DeleteBidsForAuction(string auctionId)
    {
        // Delete bids associated with the given auction from the SQLite database
        string deleteQuery = "DELETE FROM Bidding WHERE AuctionId = @AuctionId";

        using (SqliteCommand command = new SqliteCommand(deleteQuery, _connection))
        {
            command.Parameters.AddWithValue("@AuctionId", auctionId);
            await command.ExecuteNonQueryAsync();
        }
    }

    public async Task<AuctionItemDTO?> GetAuctionById(string auctionId)
    {
        // Retrieve auction from the SQLite database based on its ID
        string selectQuery = "SELECT AuctionId, Item, Price, Seller, Status, StartTime, EndTime FROM Auction WHERE AuctionId = @AuctionId";

        using (SqliteCommand command = new SqliteCommand(selectQuery, _connection))
        {
            command.Parameters.AddWithValue("@AuctionId", auctionId);

            using (SqliteDataReader reader = await command.ExecuteReaderAsync())
            {
                if (reader.Read())
                {
                    string item = reader.GetString(1);
                    double price = reader.GetDouble(2);
                    string seller = reader.GetString(3);
                    int status = reader.GetInt32(4);
                    DateTime startTime = reader.GetDateTime(5);
                    DateTime endTime = reader.GetDateTime(6);

                    return new AuctionItemDTO
                    {
                        AuctionId = auctionId,
                        Item = item,
                        Price = price,
                        Seller = seller,
                        Status = (AuctionStatusCode)status,
                        StartTime = startTime,
                        EndTime = endTime
                    };
                }
                else
                {
                    return null; // Auction not found
                }
            }
        }
    }

    public async Task UpdateAuctionStatus(string auctionId, double winningBid, AuctionStatusCode status)
    {
        string updateQuery = "UPDATE Auction SET Status = @Status, Price = @WinningBid WHERE AuctionId = @AuctionId";

        using (SqliteCommand command = new SqliteCommand(updateQuery, _connection))
        {
            command.Parameters.AddWithValue("@AuctionId", auctionId);
            command.Parameters.AddWithValue("@Status", (int)status);
            command.Parameters.AddWithValue("@WinningBid", winningBid);

            await command.ExecuteNonQueryAsync();
        }
    }


    public void Dispose()
    {
        _connection.Dispose();
    }
}