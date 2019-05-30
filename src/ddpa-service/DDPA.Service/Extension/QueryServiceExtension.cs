using AutoMapper;
using DDPA.DTO;
using DDPA.SQL.Entities;
using System.Collections.Generic;

namespace DDPA.Service.Extensions
{
    public static class QueryServiceExtension
    {
        public static IMapper GetMapper(this QueryService account)
        {
            return (new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Module, ModuleDTO>();
                cfg.CreateMap<ModuleDTO, Module>();
                cfg.CreateMap<SubModuleFieldDTO, SubModuleField>();
                cfg.CreateMap<SubModuleField, SubModuleFieldDTO>();
                cfg.CreateMap<ExtendedIdentityUser, UserDTO>();
                cfg.CreateMap<UserDTO, ExtendedIdentityUser>();
                cfg.CreateMap<Field, UpdateFieldDTO>();
                cfg.CreateMap<UpdateFieldDTO, Field>();
                cfg.CreateMap<Field, FieldDTO>();
                cfg.CreateMap<FieldDTO, Field>();
                cfg.CreateMap<SubModuleDTO, SubModule>();
                cfg.CreateMap<SubModule, SubModuleDTO>();
                cfg.CreateMap<FieldItem, FieldItemDTO>();
                cfg.CreateMap<FieldItemDTO, FieldItem>();
                cfg.CreateMap<DocumentDTO, Document>();
                cfg.CreateMap<Document, DocumentDTO>();
                cfg.CreateMap<ExtendedIdentityUser, UpdateUserDTO>();
                cfg.CreateMap<UpdateUserDTO, ExtendedIdentityUser>();
                cfg.CreateMap<DocumentFieldDTO, DocumentField>();
                cfg.CreateMap<DocumentField, DocumentFieldDTO>()
                  .ForMember(x => x.File, opt => opt.Ignore());
                cfg.CreateMap<DatasetDTO, Dataset>();
                cfg.CreateMap<Dataset, DatasetDTO>();
                cfg.CreateMap<UpdateDatasetDTO, Dataset>();
                cfg.CreateMap<Dataset, UpdateDatasetDTO>();
                cfg.CreateMap<AddFieldDatasetDTO, Field>();
                cfg.CreateMap<Field, AddFieldDatasetDTO>();
                cfg.CreateMap<DatasetFieldDTO, DatasetField>();
                cfg.CreateMap<DatasetField, DatasetFieldDTO>();
                cfg.CreateMap<AddFieldDatasetDTO, DatasetField>();
                cfg.CreateMap<DatasetField, AddFieldDatasetDTO>();
                cfg.CreateMap<DocumentDatasetField, DocumentDatasetFieldDTO>();
                cfg.CreateMap<DocumentDatasetFieldDTO, DocumentDatasetField>();
                cfg.CreateMap<UserRights, UserRightsDTO>();
                cfg.CreateMap<UserRightsDTO, UserRights>();
                cfg.CreateMap<ApprovalDocumentDTO, Document>();
                cfg.CreateMap<Document, ApprovalDocumentDTO>();
                cfg.CreateMap<Department, DepartmentDTO>();
                cfg.CreateMap<DepartmentDTO, Department>();
                cfg.CreateMap<DocumentDTO, ApprovalDocumentDTO>();
                cfg.CreateMap<ApprovalDocumentDTO, DocumentDTO>();
                cfg.CreateMap<LogsDTO, Logs>();
                cfg.CreateMap<Logs, LogsDTO>();
                cfg.CreateMap<ResourceDTO, Resource>();
                cfg.CreateMap<Resource, ResourceDTO>();
                cfg.CreateMap<Issues, IssueDTO>();
                cfg.CreateMap<CompanyInfoDTO, Company>();
                cfg.CreateMap<Company, CompanyInfoDTO>();

            })).CreateMapper();
        }
    }
}
