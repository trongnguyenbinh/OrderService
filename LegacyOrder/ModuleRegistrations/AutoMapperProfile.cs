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

        CreateMap<CustomerEntity, CustomerDto>();

        CreateMap<CustomerDto, CustomerEntity>();

        CreateMap<CreateCustomerRequest, CustomerEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<UpdateCustomerRequest, CustomerEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        // Order mappings
        CreateMap<OrderEntity, OrderDto>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src =>
                src.Customer.FirstName + " " + src.Customer.LastName))
            .ForMember(dest => dest.CustomerEmail, opt => opt.MapFrom(src => src.Customer.Email))
            .ForMember(dest => dest.CustomerType, opt => opt.MapFrom(src => src.Customer.CustomerType));

        CreateMap<OrderItemEntity, OrderItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.ProductSKU, opt => opt.MapFrom(src => src.Product.SKU));

        CreateMap<OrderItemRequest, OrderItemEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OrderId, opt => opt.Ignore())
            .ForMember(dest => dest.UnitPrice, opt => opt.Ignore())
            .ForMember(dest => dest.LineTotal, opt => opt.Ignore())
            .ForMember(dest => dest.Order, opt => opt.Ignore())
            .ForMember(dest => dest.Product, opt => opt.Ignore());

        // Chat mappings
        CreateMap<ChatSessionEntity, ChatHistoryDto>()
            .ForMember(dest => dest.SessionId, opt => opt.MapFrom(src => src.Id));

        CreateMap<ChatMessageEntity, ChatMessageDto>();
    }
}