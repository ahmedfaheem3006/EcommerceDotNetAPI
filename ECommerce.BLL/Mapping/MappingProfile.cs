using AutoMapper;
using ECommerce.DAL.Entities;
using ECommerce.BLL.Interfaces;

namespace ECommerce.BLL.Mapping
{
    /// <summary>
    /// AutoMapper Profile: maps between Entities and DTOs
    /// لماذا: نفصل بين الـ Entity (Database model) و الـ DTO (API request/response model)
    /// </summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Category ↔ CategoryDto
            CreateMap<CategoryDto, Category>();
            CreateMap<Category, CategoryDto>();

            // ProductDto ↔ Product
            CreateMap<ProductDto, Product>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
            CreateMap<Product, ProductDto>();
        }
    }
}
