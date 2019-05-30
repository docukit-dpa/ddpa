using AutoMapper;
using DDPA.DTO;
using DDPA.SQL.Entities;

namespace DDPA.Service.Extensions
{
    public static class AccountServiceExtension
    {
        public static IMapper GetMapper(this AccountService account)
        {
            return (new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ExtendedIdentityUser, UserDTO>();
                cfg.CreateMap<UserDTO, ExtendedIdentityUser>();
            })).CreateMapper();
        }
    }
}
