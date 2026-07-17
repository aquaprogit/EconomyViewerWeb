using AutoMapper;
using EconomyViewerWeb.Application.Contracts.Items;
using EconomyViewerWeb.Domain.Entities;

namespace EconomyViewerWeb.Application.Mapping;

public sealed class ItemProfile : Profile
{
    public ItemProfile()
    {
        CreateMap<Item, ItemDto>();
    }
}
