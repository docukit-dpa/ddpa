using AutoMapper;
using DDPA.DTO;
using DDPA.SQL.Entities;
using System.Collections.Generic;

namespace DDPA.Service.Extensions
{
    public static class SummaryServiceExtension
    {
        public static IMapper GetMapper(this SummaryService account)
        {
            return (new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Document, DocumentDTO>();
                cfg.CreateMap<DocumentDTO, Document>();
            })).CreateMapper();
        }
    }
}
