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
    public static class DashboardExtension
    {
        public static IMapper GetMapper(this DashboardController account)
        {
            return (new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<AddDatasetViewModel, AddDatasetDTO>();
                cfg.CreateMap<AddDatasetDTO, AddDatasetViewModel>();
                cfg.CreateMap<DatasetViewModel, DatasetDTO>();
                cfg.CreateMap<DatasetDTO, DatasetViewModel>();
                cfg.CreateMap<UpdateDatasetViewModel, UpdateDatasetDTO>();
                cfg.CreateMap<UpdateDatasetDTO, UpdateDatasetViewModel>();
                cfg.CreateMap<DatasetViewModel, UpdateDatasetDTO>();
                cfg.CreateMap<UpdateDatasetDTO, DatasetViewModel>();
                cfg.CreateMap<AddFieldDatasetViewModel, AddFieldDatasetDTO>();
                cfg.CreateMap<AddFieldDatasetDTO, AddFieldDatasetViewModel>();
                cfg.CreateMap<SummaryItemViewModel, SummaryItemDTO>();
                cfg.CreateMap<SummaryItemDTO, SummaryItemViewModel>();
            })).CreateMapper();
        }
    }
}
