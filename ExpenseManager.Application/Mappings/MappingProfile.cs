using AutoMapper;
using ExpenseManager.Application.DTOs;
using ExpenseManager.Domain.Entities;

namespace ExpenseManager.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserResponseDto>();

            // Password is hashed in the service layer before being assigned to PasswordHash
            CreateMap<RegisterRequestDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());
            CreateMap<LoginRequestDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

            CreateMap<Expense, ExpenseResponseDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email));
            CreateMap<CreateExpenseDto, Expense>();
            CreateMap<UpdateExpenseDto, Expense>();

            CreateMap<Category, CategoryResponseDto>();
            CreateMap<CreateCategoryDto, Category>();
        }
    }
}
