using api.EntityFrameworkCore;
using api.Models;
using AutoMapper;
using ECommerceAPI.Models;

namespace api.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<CreateUserDto, User>();
            CreateMap<Category, CategoryDto>();
            CreateMap<CreateCategoryDto, Category>();
            CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Categories));
            CreateMap<CreateProductDto, Product>()
             .ForMember(dest => dest.Categories, opt => opt.Ignore());

            // Add this line to map UpdateUserDto to User
            CreateMap<UpdateUserDto, User>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<UpdateCategoryDto, Category>()
               .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<UpdateProductDto, Product>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<UpdateProductDto, Product>()
                .ForMember(dest => dest.Categories, opt => opt.Ignore());


            // With AutoMapper now configured, your service methods can utilize the IMapper interface to map data models to DTOs and vice versa, simplifying data transformation and reducing boilerplate code.
        }
    }
}