using AutoMapper;
using DDPA.DTO;
using DDPA.SQL.Entities;

namespace DDPA.Service.Extensions
{
    public static class AdminServiceExtension
    {
        public static IMapper GetMapper(this AdminService admin)
        {
            return (new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<AddFieldDTO, Field>();
                cfg.CreateMap<Field, AddFieldDTO>();
                cfg.CreateMap<FieldItemDTO, FieldItem>();
                cfg.CreateMap<FieldItem, FieldItemDTO>();
                cfg.CreateMap<AddModuleDTO, Module>();
                cfg.CreateMap<Module, AddModuleDTO>();
                cfg.CreateMap<SubModuleField, SubModuleFieldDTO>();
                cfg.CreateMap<SubModuleFieldDTO, SubModuleField>();
                cfg.CreateMap<Document, AddDocumentDTO>();
                cfg.CreateMap<AddDocumentDTO, Document>();
                cfg.CreateMap<DocumentField, DocumentFieldDTO>();
                cfg.CreateMap<DocumentFieldDTO, DocumentField>();
                cfg.CreateMap<SubModule, SubModuleDTO>();
                cfg.CreateMap<SubModuleDTO, SubModule>();
                cfg.CreateMap<UpdateDocumentDTO, Document>();
                cfg.CreateMap<Document, UpdateDocumentDTO>();
                cfg.CreateMap<DatasetField, DatasetFieldDTO>();
                cfg.CreateMap<DatasetFieldDTO, DatasetField>();
                cfg.CreateMap<Dataset, AddDatasetDTO>();
                cfg.CreateMap<AddDatasetDTO, Dataset>();
                cfg.CreateMap<Dataset, DatasetDTO>();
                cfg.CreateMap<DatasetDTO, Dataset>();
                cfg.CreateMap<DocumentDatasetField, DocumentDatasetFieldDTO>();
                cfg.CreateMap<DocumentDatasetFieldDTO, DocumentDatasetField>();
                cfg.CreateMap<LogsDTO, Logs>();


            })).CreateMapper();
        }
    }
}