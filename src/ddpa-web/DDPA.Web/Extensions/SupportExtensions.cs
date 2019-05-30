using AutoMapper;
using DDPA.Web.Controllers;
using DDPA.Web.Models;
using DDPA.DTO;
using DDPA.SQL.Entities;

namespace DDPA.Web.Extensions
{
    public static class SupportExtensions
    {
        public static IMapper GetMapper(this SupportController account)
        {
            return (new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<SendMailDTO, SendMailViewModel>();
                cfg.CreateMap<SendMailViewModel, SendMailDTO>();
            })).CreateMapper();
        }
    }
}
