using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DDPA.Web.Controllers;
using DDPA.Web.Models;
using DDPA.DTO;
using DDPA.SQL.Entities;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.Web.Extensions
{
    public static class ApprovalExtensions
    {
        public static IMapper GetMapper(this ApprovalController account)
        {
            return (new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ApprovalDocumentViewModel, ApprovalDocumentDTO>();
                cfg.CreateMap<ApprovalDocumentDTO, ApprovalDocumentViewModel>();
                cfg.CreateMap<DocumentDTO, DocumentViewModel>();
                cfg.CreateMap<DocumentViewModel, DocumentDTO>();
                cfg.CreateMap<LogsDTO, LogsViewModel>();
                cfg.CreateMap<LogsViewModel, LogsDTO>();
                cfg.CreateMap<DatasetFieldDTO, FieldDatasetViewModel>();
                cfg.CreateMap<FieldDatasetViewModel, DatasetFieldDTO>();

            })).CreateMapper();
        }
    }
}
