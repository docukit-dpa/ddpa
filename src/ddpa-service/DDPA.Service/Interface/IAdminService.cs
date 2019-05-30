using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DDPA.SQL.Entities;
using DDPA.Commons.Results;
using DDPA.DTO;
using Microsoft.AspNetCore.Http;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.Service
{
    public interface IAdminService
    {
        Task<Result> AddFields(List<AddFieldDTO> dto, string userId);

        Task<Result> DeleteFields(string username);

        Task<Result> AddModules(List<AddModuleDTO> dto, string userId);

        Task<Result> AddDocument(AddDocumentDTO dto, List<IFormFile> file, List<IFormFile> datasetfile, string userRole, string userId, string userDept);

        Task<Result> UpdateDocument(UpdateDocumentDTO dto, List<IFormFile> file, List<IFormFile> datasetfile, int subModuleId, string userId);

        Task<Result> AddDatasets(List<AddDatasetDTO> dto, string userId);

        Task<Result> BulkUploadXlsOrXlsx(string name, string folder, string fileExt, string currentUser, string rootFolder, string userRole, string userId, string userDept);

        Task<Result> BulkUploadCsv(string name, string folder, string fileExt, string currentUser, string rootFolder, string userRole, string userId, string userDept, string docDeptId, string docDataSetId);

        Task<Result> EditDocument(UpdateDocumentDTO dto, int subModuleId, string userRole, string userId, string userdept);

        Task<Result> AddToWorkflowInbox(string userRole, string userId, string userdept, string docId, Status status);

        Task<Result> DeleteDataSet(string userRole, string userId, string userDept, string docId);

        Task<Result> AddLogs(LogsDTO dto);

        Task<Result> CreateUserGuideResource(AddResourceDTO dto, string userId);
    }
}