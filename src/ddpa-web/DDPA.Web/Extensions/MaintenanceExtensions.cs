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
    public static class MaintenanceExtensions
    {
        public static IMapper GetMapper(this MaintenanceController account)
        {
            return (new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ExtendedIdentityUser, UserViewModel>();
                cfg.CreateMap<UserViewModel, ExtendedIdentityUser>();
                cfg.CreateMap<ExtendedIdentityUser, UpdateUserViewModel>();
                cfg.CreateMap<UpdateUserViewModel, ExtendedIdentityUser>();
                cfg.CreateMap<UserDTO, UpdateUserViewModel>();
                cfg.CreateMap<UpdateUserViewModel, UserDTO>();
                cfg.CreateMap<UpdateUserDTO, UpdateUserViewModel>();
                cfg.CreateMap<UpdateUserViewModel, UpdateUserDTO>();
                cfg.CreateMap<UserDTO, UserViewModel>();
                cfg.CreateMap<UserViewModel, UserDTO>();
                cfg.CreateMap<UpdateFieldDTO, UpdateFieldViewModel>();
                cfg.CreateMap<UpdateFieldViewModel, UpdateFieldDTO>();
                cfg.CreateMap<AddUserDTO, AddUserViewModel>();
                cfg.CreateMap<AddUserViewModel, AddUserDTO>();
                cfg.CreateMap<AddFieldItemViewModel, FieldItemDTO>();
                cfg.CreateMap<FieldItemDTO, AddFieldItemViewModel>();
                cfg.CreateMap<UpdateFieldItemViewModel, FieldItemDTO>();
                cfg.CreateMap<FieldItemDTO, UpdateFieldItemViewModel>();
                cfg.CreateMap<AddFieldViewModel, AddFieldDTO>();
                cfg.CreateMap<AddFieldDTO, AddFieldViewModel>();
                cfg.CreateMap<DepartmentDTO, DepartmentViewModel>();
                cfg.CreateMap<DepartmentViewModel, DepartmentDTO>();
                cfg.CreateMap<ChangePasswordUserDTO, ChangePasswordUserViewModel>();
                cfg.CreateMap<ChangePasswordUserViewModel, ChangePasswordUserDTO>();
                cfg.CreateMap<UpdateDepartmentDTO, UpdateDepartmentViewModel>();
                cfg.CreateMap<UpdateDepartmentViewModel, UpdateDepartmentDTO>();
                cfg.CreateMap<AddDepartmentDTO, AddDepartmentViewModel>();
                cfg.CreateMap<AddDepartmentViewModel, AddDepartmentDTO>();
                cfg.CreateMap<FieldDTO, FieldViewModel>()
                     .ForMember(x => x.TypeName, opt => opt.MapFrom(src => src.Type.ToString()));
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
                cfg.CreateMap<FieldDatasetViewModel, DatasetFieldDTO>();
                cfg.CreateMap<DatasetFieldDTO, FieldDatasetViewModel>();
                //.ForMember(x => x.Field, opt => opt.Ignore());
                cfg.CreateMap<FieldDTO, FieldViewModel>();
                cfg.CreateMap<FieldViewModel, FieldDTO>();
                cfg.CreateMap<UpdateUserDTO, ChangePasswordUserViewModel>();
                cfg.CreateMap<ChangePasswordUserViewModel, UpdateUserDTO>();
                cfg.CreateMap<ChangeCompanyInfoViewModel, CompanyInfoDTO>();
                cfg.CreateMap<CompanyInfoDTO, ChangeCompanyInfoViewModel>();
            })).CreateMapper();
        }
    }
}
