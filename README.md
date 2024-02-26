# PeerBid

The P2P Auction System is a decentralized auction platform that allows participants to initiate auctions, place bids, and conclude transactions using Remote Procedure Calls (RPC) as the primary form of communication.

## Challenge Overview

The challenge involves developing a simple P2P auction system with the following key features:

- **Auction Initialization:** Clients should be able to initiate auctions by specifying the item for sale and the starting price.
- **Bid Handling:** Participants can place bids on ongoing auctions, with bids being broadcasted to all network participants.
- **Auction Conclusion:** Upon auction closure, the system must handle distributed transactions and notify all participants of the auction outcome.

## Technical Requirements

- **P2P Architecture:** Adherence to a decentralized P2P architecture, avoiding traditional client/server models.
- **RPC Communication:** Utilization of JSON RPC 2.0 or gRPC for node communication.
- **Command-Line Interface (CLI):** Implementation of a command-line interface for interaction; a graphical UI is not required.
- **Database:** Usage of SQLite for any database requirements.
- **Compatibility:** Ensure compatibility across multiple operating systems: Linux, OSX, Windows.

## Installation and Setup

To run the P2P Auction System on your machine, follow these steps:

1. Clone the repository:
   ```bash
   git clone https://github.com/matth1010/PeerBid.git
   cd PeerBid

## Usage
Once the system is up and running, you can perform the following actions:

Initiate Auction: Use the CLI to initiate auctions by specifying the item and starting price.
Place Bid: Participate in ongoing auctions by placing bids.
Monitor Auction: Keep track of ongoing auctions and their status.
Handle Auction Conclusion: Upon auction closure, handle distributed transactions and receive notifications of the auction outcome.
Limitations and Future Improvements
While the current implementation meets the basic requirements, there are areas for improvement:

Enhanced Security: Implement encryption and authentication mechanisms to enhance security.
Scalability: Optimize the system for scalability to handle a larger number of participants and auctions.
Error Handling: Enhance error handling to provide better feedback to users in case of failures.
