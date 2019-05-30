using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DDPA.SQL.Entities;
using DDPA.Commons.Results;
using DDPA.DTO;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.Service
{
    public interface IQueryService
    {
        /// <summary>
        /// Get all the modules that is for the current user's role. This includes its sub modules and sub modules' field.
        /// </summary>
        /// <param name="role">Role of the user.</param>
        /// <returns>All modules for the role given.</returns>
        Task<List<ModuleDTO>> GetModules(string role);

        /// <summary>
        /// Get all the users but exclude admin and dpo.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>All users except admin and dpo.</returns>
        List<UserDTO> GetUsers();

        /// <summary>
        /// Get specific user.
        /// </summary>
        /// <param name="id">User id</param>
        /// <returns>User information</returns>
        Task<UpdateUserDTO> GetUserById(string id);

        /// <summary>
        /// Get all data set fields.
        /// </summary>
        /// <returns>All fields</returns>
        Task<List<FieldDTO>> GetAllFields();
        
        /// <summary>
        /// Get fields of the specific sub module. 
        /// Ex: Sub module Collection in Data Sets module has fields of Collection Source, Collection medium etc.
        /// </summary>
        /// <param name="subModuleId">Sub module Id</param>
        /// <returns>Fields of the specific sub module.</returns>
        Task<List<FieldDTO>> GetSubModuleFields(string subModuleId);

        Task<SubModuleDTO> GetSubModuleByUrl(string url);

        /// <summary>
        /// Get specific Field.
        /// </summary>
        /// <param name="id">Field Id</param>
        /// <returns>Field</returns>
        Task<UpdateFieldDTO> GetFieldById(string id);

        /// <summary>
        /// Get specific Field.
        /// </summary>
        /// <param name="fieldId">Field Id</param>
        /// <returns>Field</returns>
        Task<FieldDTO> GetFieldTypeById(string fieldId);

        /// <summary>
        /// Get items of a Field.
        /// </summary>
        /// <param name="id">Field Id</param>
        /// <returns>Items of a Field</returns>
        Task<List<FieldItemDTO>> GetFieldItemsById(string id);

        Task<ManyResult<DocumentDTO>> GetDocuments(SearchDTO search, Status status, string userRole, string userDept, string userId);

        /// <summary>
        /// Get all data sets regardless of status.
        /// </summary>
        /// <returns>All data sets</returns>
        Task<List<DocumentDTO>> GetAllDocuments();

        /// <summary>
        /// Get all document's fields.
        /// </summary>
        /// <returns>Document's fields</returns>
        Task<List<DocumentFieldDTO>> GetAllDocumetFields();

        /// <summary>
        /// Get specific field item.
        /// </summary>
        /// <param name="id">Field item Id</param>
        /// <returns></returns>
        Task<FieldItemDTO> GetFieldItemById(string id);

        Task<List<DepartmentDTO>> GetDepartments();

        List<string> GetRoles();

        Task<DocumentDTO> GetDocumentById(string id);
        
        Task<DocumentFieldDTO> GetDocumentFieldById(string id);

        Task<List<DatasetDTO>> GetDataset();

        Task<UpdateDatasetDTO> GetDatasetById(string id);

        Task<List<AddFieldDatasetDTO>> GetAvailableField(string datasetId);

        Task<List<DatasetFieldDTO>> GetCurrentField(string datasetId);

        Task<int> GetSubModuleFieldMaxOrder(int subModuleId);
        
        Task<List<FieldDTO>> GetLifeCycleFields();

        Task<Status> GetFieldsLifeCycle(string id);

        Task<UserRightsDTO> GetUserRights(string userId, string moduleId);
        
        Task<List<UserRightsDTO>> GetAccessRightsByUser(string userId);
        
        Task<List<ApprovalDocumentDTO>> GetApprovalDocuments(string userRole, string userDept, string userId);

        Task<UpdateDepartmentDTO> GetDepartmentById(string id);

        Task<List<DepartmentDTO>> GetAllDepartments();

        Task<List<ApprovalDocumentDTO>> GetRequestDocuments(string userDept, string userId);

        Task<ApprovalDocumentDTO> GetRequestDocument(string docId);

        Task<List<LogsDTO>> GetLogs(string docId);

        Task<List<ResourceDTO>> GetAllResources();

        Task<List<SubModuleFieldDTO>> GetAllSubModuleFields();

        Task<SummaryItemDTO> GetSummaryOfDataSet(string userRole, string userDept);

        Task<SummaryItemDTO> GetSummaryOfStorage(string userRole, string userDept);

        Task<SummaryItemDTO> GetSummaryOfExternalParty(string userRole, string userDept);

        Task<List<DocumentDTO>> GetDocumentsByRole(string userRole, string userDept);

        Task<SummaryItemDTO> GetSummaryOfDatasetIssue(string userRole, string userDept);

        Task<List<IssueDTO>> GetIssues(string docId);

        Task<List<IssueDTO>> GetIssuesByRole(string userRole, string userDept);
        
        Task<List<DocumentDTO>> GetAllDatasets(string userRole, string userDept, string userId);
        
        Task<List<DocumentFieldDTO>> GetSDocumentFieldsBySubModule(string docId, string subModuleId);

        Task<Boolean> DoFileExist(string ResourceFolder, string fileName);

        Task<List<DocumentDTO>> GetApprovedDocuments();

        Task<List<DocumentDTO>> GetApprovedDocumentsByDepartment(string userRole);

        Task<List<DocumentDTO>> GetApprovedDocumentsByUser(string userId);
        
        /// <summary>
        /// Get company info.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Company Info</returns>
        Task<CompanyInfoDTO> GetCompanyInfo();

        Task<List<FieldItemDTO>> GetFieldItems(string id, string value, string selected);
    }
}