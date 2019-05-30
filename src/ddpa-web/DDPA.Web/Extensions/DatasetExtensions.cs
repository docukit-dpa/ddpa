using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DDPA.Web.Controllers;
using DDPA.Web.Models;
using DDPA.DTO;
using DDPA.SQL.Entities;

namespace DDPA.Web.Extensions
{
    public static class DatasetExtensions
    {
        public static IMapper GetMapper(this DatasetController account)
        {
            return (new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<FieldDTO, FieldViewModel>();
                cfg.CreateMap<FieldViewModel, FieldDTO>();
                cfg.CreateMap<FieldItemDTO, FieldItemViewModel>();
                cfg.CreateMap<FieldItemViewModel, FieldItemDTO>();
                cfg.CreateMap<DocumentFieldViewModel, DocumentFieldDTO>();
                cfg.CreateMap<DocumentFieldDTO, DocumentFieldViewModel>();
                cfg.CreateMap<DocumentDTO, DocumentViewModel>();
                cfg.CreateMap<DocumentViewModel, DocumentDTO>();
                cfg.CreateMap<SearchDTO, SearchViewModel>();
                cfg.CreateMap<SearchViewModel, SearchDTO>();
                cfg.CreateMap<UpdateDocumentDTO, DocumentViewModel>();
                cfg.CreateMap<DocumentViewModel, UpdateDocumentDTO>();
                cfg.CreateMap<FieldDatasetViewModel, DatasetFieldDTO>();
                cfg.CreateMap<DatasetFieldDTO, FieldDatasetViewModel>();
                cfg.CreateMap<DocumentDatasetFieldDTO, DocumentDatasetFieldViewModel>();
                cfg.CreateMap<DocumentDatasetFieldViewModel, DocumentDatasetFieldDTO>();
                cfg.CreateMap<DatasetDTO, DatasetViewModel>();
                cfg.CreateMap<DatasetViewModel, DatasetDTO>();
            })).CreateMapper();
        }
    }
}
