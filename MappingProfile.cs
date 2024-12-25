using AutoMapper;
using Mixvel.Contracts;
using Mixvel.Interfaces;
using System;

namespace Mixvel
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<SearchRequest, ProviderOneSearchRequest>()
                .ForMember(dest => dest.From, opt => opt.MapFrom(src => src.Origin))
                .ForMember(dest => dest.To, opt => opt.MapFrom(src => src.Destination))
                .ForMember(dest => dest.DateFrom, opt => opt.MapFrom(src => src.OriginDateTime))
                .ForMember(dest => dest.DateTo, opt =>
                {
                    opt.PreCondition(src => src.Filters?.DestinationDateTime != null);
                    opt.MapFrom(src => src.Filters.DestinationDateTime);
                })
                .ForMember(dest => dest.MaxPrice, opt =>
                {
                    opt.PreCondition(src => src.Filters?.MaxPrice != null);
                    opt.MapFrom(src => src.Filters.MaxPrice);
                }); 
            CreateMap<SearchRequest, ProviderTwoSearchRequest>()
                .ForMember(dest => dest.Departure, opt => opt.MapFrom(src => src.Origin))
                .ForMember(dest => dest.Arrival, opt => opt.MapFrom(src => src.Destination))
                .ForMember(dest => dest.DepartureDate, opt => opt.MapFrom(src => src.OriginDateTime))
                .ForMember(dest => dest.MinTimeLimit, opt =>
                {
                    opt.PreCondition(src => src.Filters?.MinTimeLimit != null);
                    opt.MapFrom(src => src.Filters.MinTimeLimit);
                });

            CreateMap<ProviderOneSearchResponse, SearchResponse>();
            CreateMap<ProviderTwoSearchResponse, SearchResponse>();

            CreateMap<ProviderOneRoute, Mixvel.Interfaces.Route>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.Origin, opt => opt.MapFrom(src => src.From))
                .ForMember(dest => dest.Destination, opt => opt.MapFrom(src => src.To))
                .ForMember(dest => dest.OriginDateTime, opt => opt.MapFrom(src => src.DateFrom))
                .ForMember(dest => dest.DestinationDateTime, opt => opt.MapFrom(src => src.DateTo));

            CreateMap<ProviderTwoRoute, Mixvel.Interfaces.Route>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.Origin, opt => opt.MapFrom(src => src.Departure.Point))
                .ForMember(dest => dest.Destination, opt => opt.MapFrom(src => src.Arrival.Point))
                .ForMember(dest => dest.OriginDateTime, opt => opt.MapFrom(src => src.Departure.Date))
                .ForMember(dest => dest.DestinationDateTime, opt => opt.MapFrom(src => src.Arrival.Date));
        }
    }
}
