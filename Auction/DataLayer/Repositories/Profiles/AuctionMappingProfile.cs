using Auction.DataLayer.Models;
using AutoMapper;

namespace Auction.DataLayer.Repositories.Profiles
{
    public class AuctionMappingProfile : Profile
    {
        public AuctionMappingProfile()
        {
            CreateMap<AuctionItemDTO, Data.Models.Auction>()
             .ForMember(d => d.Id, opt => opt.MapFrom((s, d) => s.AuctionId));

            CreateMap<Data.Models.Auction, AuctionItemDTO>()
             .ForMember(d => d.AuctionId, opt => opt.MapFrom((s, d) => s.Id));

            CreateMap<BidDTO, Data.Models.AuctionBid>();
            CreateMap<Data.Models.AuctionBid, BidDTO>();
        }
    }
}
