namespace LegacyOrder.ModuleRegistrations;

using AutoMapper;
using Domain.Entities;
using Model.Models;
using Model.RequestModels;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<ProductEntity, ProductDto>();

        CreateMap<ProductDto, ProductEntity>();

        CreateMap<CreateProductRequest, ProductEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<UpdateProductRequest, ProductEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
    }
}