using Microsoft.Extensions.Logging;
using DDPA.Commons.Results;
using System.Threading.Tasks;
using DDPA.DTO;
using DDPA.SQL.Entities;
using DDPA.SQL.Repositories;
using System;
using static DDPA.Commons.Enums.DDPAEnums;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using DDPA.Service.Extensions;
using Microsoft.EntityFrameworkCore;
using DDPA.SQL.Repositories.Context;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Internal;

namespace DDPA.Service
{
    public class QueryService : IQueryService
    {
        protected readonly ILogger _logger;
        protected readonly IRepository _repo;
        private readonly IMapper _mapper;
        private readonly UserManager<ExtendedIdentityUser> _userManager;

        public QueryService(ILogger<AccountService> logger, IRepository repo, UserManager<ExtendedIdentityUser> userManager)
        {
            _logger = logger;
            _repo = repo;
            _mapper = this.GetMapper();
            _userManager = userManager;
        }

        public async Task<List<ModuleDTO>> GetModules(string role)
        {
            var result = new List<ModuleDTO>();
            try
            {
                var modules = await _repo.GetAsync<Module>(
                       filter: c => c.isEnabled == true && c.Roles.Contains(role),
                       include: source => source.Include(c => c.SubModule).ThenInclude(c => c.SubModuleField)
                   );
                result = _mapper.Map<List<ModuleDTO>>(modules);
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetModules: {0}", e.Message);
            }

            return result;
        }

        public async Task<UserRightsDTO> GetUserRights(string userId, string moduleId)
        {
            var result = new UserRightsDTO();
            try
            {
                var userRights = await _repo.GetFirstAsync<UserRights>(
                       filter: c => c.ModuleId == Convert.ToInt32(moduleId) && c.UserId == Convert.ToInt32(userId)
                   );
                result = _mapper.Map<UserRightsDTO>(userRights);
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling userRights: {0}", e.Message);
            }

            return result;
        }
        public List<UserDTO> GetUsers()
        {
            var users = new List<UserDTO>();
            try
            {
                // todo, rework: hard coded id 1 and 2
                // This excludes first and second user which is admin and dpo that were added in the seeding process.
                var tempUsers = _userManager.Users.Where(u => u.Id != 1 && u.Id != 2);
                users = _mapper.Map<List<UserDTO>>(tempUsers);
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetUsers: {0}", e.Message);
            }

            return users;
        }

        public async Task<UpdateUserDTO> GetUserById(string id)
        {
            UpdateUserDTO users = new UpdateUserDTO();
            try
            {
                var tempUsers = await _userManager.FindByIdAsync(id);
                var userRole = await _userManager.GetRolesAsync(tempUsers);

                users = _mapper.Map<UpdateUserDTO>(tempUsers);
                users.Role = userRole[0];
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetModules: {0}", e.Message);
            }

            return users;
        }

        public async Task<List<FieldDTO>> GetAllFields()
        {
            var result = new List<FieldDTO>();
            try
            {
                Dictionary<FieldType, string> fieldTypeName = new Dictionary<FieldType, string>
                {
                    {FieldType.TextField, "Text" },
                    {FieldType.NumericField, "Numeric" },
                    {FieldType.MemoField, "Memo" },
                    {FieldType.ComboField, "Dropdown" },
                    {FieldType.Attachment, "Attachment" },
                    {FieldType.Textarea, "Textarea" },
                    {FieldType.Checkbox, "Checkbox" }
                };
                var fields = await _repo.GetAsync<Field>(
                    filter: f => f.IsLifeCycle == false,
                    include: source => source.Include(x => x.FieldItem));
                result = _mapper.Map<List<FieldDTO>>(fields);

                foreach(FieldDTO item in result)
                {
                    item.TypeName = fieldTypeName[item.Type];
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetAllFields: {0}", e.Message);
            }

            return result;
        }

        public async Task<List<DocumentFieldDTO>> GetAllDocumetFields()
        {
            var result = new List<DocumentFieldDTO>();
            try
            {
                var fields = await _repo.GetAllAsync<DocumentField>(
                    include: source => source.Include(x => x.Field));
                result = _mapper.Map<List<DocumentFieldDTO>>(fields);
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetFields: {0}", e.Message);
            }

            return result;
        }

        public async Task<List<FieldDTO>> GetSubModuleFields(string subModuleId)
        {
            var result = new List<FieldDTO>();
            try
            {
                var module = await _repo.GetAsync<SubModuleField>(
                   filter: c => c.SubModuleId == Convert.ToInt32(subModuleId),
                   include: source => source.Include(c => c.Field).ThenInclude(c => c.FieldItem)
               );

                if (module.Count() > 0)
                {
                    var fields = module.OrderBy(x => x.Order).Select(x => x.Field).ToList();
                    result = _mapper.Map<List<FieldDTO>>(fields);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetModuleFields: {0}", e.Message);
            }

            return result;
        }

        public async Task<SubModuleDTO> GetSubModuleByUrl(string url)
        {
            var result = new SubModuleDTO();
            try
            {
                var module = await _repo.GetFirstAsync<SubModule>(
                   filter: c => c.Url == url
               );

                result = _mapper.Map<SubModuleDTO>(module);
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetSubModuleByUrl: {0}", e.Message);
            }

            return result;
        }

        //get field to be displayed in Update Field view
        public async Task<UpdateFieldDTO> GetFieldById(string id)
        {
            UpdateFieldDTO dto = new UpdateFieldDTO();
            try
            {
                var field = await _repo.GetFirstAsync<Field>(
                    filter: f => f.Id == Convert.ToInt32(id));
                dto = _mapper.Map<UpdateFieldDTO>(field);
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetFields: {0}", e.Message);
            }

            return dto;
        }

        public async Task<FieldDTO> GetFieldTypeById(string fieldId)
        {
            var result = new FieldDTO();
            try
            {
                var module = await _repo.GetFirstAsync<Field>(
                   filter: c => c.Id == Convert.ToInt32(fieldId)
               );

                result = _mapper.Map<FieldDTO>(module);
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetFieldTypeById: {0}", e.Message);
            }

            return result;
        }

        public async Task<List<FieldItemDTO>> GetFieldItemsById(string id)
        {
            var result = new List<FieldItemDTO>();
            try
            {
                string x = id;
                var field = await _repo.GetAsync<FieldItem>(filter: f => f.FieldId == Convert.ToInt32(id));
                result = _mapper.Map<List<FieldItemDTO>>(field);
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetFields: {0}", e.Message);
            }

            return result;
        }

        public async Task<FieldItemDTO> GetFieldItemById(string id)
        {
            FieldItemDTO _return = new FieldItemDTO();
            try
            {
                var fieldItem = await _repo.GetByIdAsync<FieldItem>(Convert.ToInt32(id));
                _return = _mapper.Map<FieldItemDTO>(fieldItem);
            }
            catch (Exception)
            {

                throw;
            }
            return _return;
        }

        public async Task<List<DepartmentDTO>> GetDepartments()
        {
            Result result = new Result();
            List<DepartmentDTO> listOfDepartment = new List<DepartmentDTO>();
            try
            {
                //get all departments
                var department = await _repo.GetAsync<Department>(
                    filter: f => f.Status == true);

                var deptDTO = _mapper.Map<List<DepartmentDTO>>(department);

                foreach (DepartmentDTO tempDepartment in deptDTO)
                {
                    DepartmentDTO populate = new DepartmentDTO
                    {
                        Id = tempDepartment.Id.ToString(),
                        Name = tempDepartment.Name
                    };
                    listOfDepartment.Add(populate);
                }
                return listOfDepartment;
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetDepartments: {0}", e.Message);
            }
            return listOfDepartment;
        }

        public List<string> GetRoles()
        {
            Result result = new Result();
            List<string> roleList = new List<string>();
            try
            {
                
                var role = Enum.GetValues(typeof(Role));
                foreach (var field in role)
                {
                    roleList.Add(field.ToString());
                }
            }
            catch (Exception)
            {

                throw;
            }
            return roleList;
        }

        public async Task<ManyResult<DocumentDTO>> GetDocuments(SearchDTO search, Status status, string userRole, string userDept, string userId)
        {
            var result = new ManyResult<DocumentDTO>();
            try
            {
                var isFiltered = false;
                var dtos = new List<DocumentDTO>();
                var totalCount = 0;
                
                if (status == Status.Collection)
                {
                    //get all documents regardless of department
                    if (userRole == nameof(Role.DPO) || userRole == nameof(Role.ADMINISTRATOR))
                    {
                        var documentFiltered = await _repo.GetAsync<Document>(
                            filter: f => (f.State == State.Approved && f.RequestType != RequestType.Delete)
                            || (f.State != State.Approved && f.RequestType == RequestType.Delete),
                            include: source => source.Include(c => c.DocumentField).ThenInclude(c => c.Field)
                       );
                        totalCount = await _repo.GetCountAsync<Document>();
                        dtos = _mapper.Map<List<DocumentDTO>>(documentFiltered);
                    }

                    //get all documents within the user's department
                    else if (userRole == nameof(Role.DEPTHEAD))
                    {
                        var documentFiltered = await _repo.GetAsync<Document>(
                            filter: f => f.DepartmentId == userDept 
                                && ((f.State == State.Approved && f.RequestType != RequestType.Delete)
                                || (f.State != State.Approved && f.RequestType == RequestType.Delete)),
                            include: source => source.Include(c => c.DocumentField).ThenInclude(c => c.Field)
                       );
                        totalCount = await _repo.GetCountAsync<Document>();
                        dtos = _mapper.Map<List<DocumentDTO>>(documentFiltered);
                    }

                    //get all documents that the user created
                    else if (userRole == nameof(Role.USER))
                    {
                        var documentFiltered = await _repo.GetAsync<Document>(
                            filter: f => f.CreatedBy == userId 
                                && ((f.State == State.Approved && f.RequestType != RequestType.Delete)
                                || (f.State != State.Approved && f.RequestType == RequestType.Delete)),
                           include: source => source.Include(c => c.DocumentField).ThenInclude(c => c.Field)
                       );
                        totalCount = await _repo.GetCountAsync<Document>();
                        dtos = _mapper.Map<List<DocumentDTO>>(documentFiltered);
                    }

                }

                foreach (var dto in dtos)
                {
                    var docField = dto.DocumentField.Where(x => x.SubModuleId == (Convert.ToInt32(status) + 1)).Select(x => x).ToList();
                    dto.DocumentField = docField;
                }

                result.Entities = dtos;
                result.TotalEntities = totalCount;

                if (isFiltered)
                    result.TotalFilteredEntities = dtos.Count();
                else
                    result.TotalFilteredEntities = totalCount;
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetDocuments: {0}", e.Message);
            }

            return result;
        }

        public async Task<List<DocumentDTO>> GetAllDocuments()
        {
            var result = new List<DocumentDTO>();
            try
            {
                var documentfields = await _repo.GetAsync<Document>(
                include: source => source.Include(c => c.DocumentField).ThenInclude(c => c.Field)
                );
                var fields = documentfields.OrderBy(c => c.Id);
                result = _mapper.Map<List<DocumentDTO>>(fields);
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetFields: {0}", e.Message);
            }

            return result;
        }

        public async Task<DocumentDTO> GetDocumentById(string id)
        {
            var result = new DocumentDTO();
            try
            {
                var documents = await _repo.GetAsync<Document>(
                       filter: (c => c.Id == Convert.ToInt32(id)),
                       include: source => source.Include(c => c.DocumentDatasetField).Include(c => c.DocumentField).ThenInclude(c => c.Field).ThenInclude(c => c.FieldItem)
                   );

                if (documents.Count() > 0)
                {
                    result = _mapper.Map<DocumentDTO>(documents.FirstOrDefault());
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetDocuments: {0}", e.Message);
            }

            return result;
        }

        public async Task<DocumentFieldDTO> GetDocumentFieldById(string id)
        {
            var result = new DocumentFieldDTO();
            try
            {
                var docField = await _repo.GetByIdAsync<DocumentField>(Convert.ToInt32(id));

                if (docField != null)
                {
                    result = _mapper.Map<DocumentFieldDTO>(docField);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetDocuments: {0}", e.Message);
            }

            return result;
        }


        public async Task<List<DatasetDTO>> GetDataset()
        {
            var datasets = new List<DatasetDTO>();
            try
            {
                var tempDatasets = await _repo.GetAllAsync<Dataset>();
                datasets = _mapper.Map<List<DatasetDTO>>(tempDatasets);
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetModules: {0}", e.Message);
            }

            return datasets;
        }

        public async Task<UpdateDatasetDTO> GetDatasetById(string id)
        {
            var dataset = new UpdateDatasetDTO();
            try
            {
                var tempDataset = await _repo.GetByIdAsync<Dataset>(Convert.ToInt32(id));
                dataset = _mapper.Map<UpdateDatasetDTO>(tempDataset);
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetModules: {0}", e.Message);
            }

            return dataset;
        }

        //get field to be displayed in Update Field view
        public async Task<List<AddFieldDatasetDTO>> GetAvailableField(string datasetId)
        {
            List<AddFieldDatasetDTO> availFields = new List<AddFieldDatasetDTO>();
            try
            {
                var tempFields = await _repo.GetAsync<Field>(
                        filter: c => c.IsLifeCycle == false);

                availFields = _mapper.Map<List<AddFieldDatasetDTO>>(tempFields);
                var tempDatasetFields = await _repo.GetAsync<DatasetField>(filter: f => f.DatasetId == Convert.ToInt32(datasetId));
                List<DatasetFieldDTO> datasetFields = _mapper.Map<List<DatasetFieldDTO>>(tempDatasetFields);
                foreach (DatasetFieldDTO field in datasetFields)
                {
                    if(availFields.Any(a => a.Id == field.FieldId.ToString()))
                    {
                        availFields.RemoveAll(r => r.Id == field.FieldId.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetFields: {0}", e.Message);
            }

            return availFields;
        }

        public async Task<List<DatasetFieldDTO>> GetCurrentField(string datasetId)
        {
            List<DatasetFieldDTO> addedFields = new List<DatasetFieldDTO>();
            try
            {
                var tempFields = await _repo.GetAsync<DatasetField>(
                         filter: c => c.DatasetId == Convert.ToInt32(datasetId),
                         include: source => source.Include(c => c.Field).ThenInclude(c => c.FieldItem)
                      );

                addedFields = _mapper.Map<List<DatasetFieldDTO>>(tempFields);
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetFields: {0}", e.Message);
            }

            return addedFields;
        }

        public async Task<int> GetSubModuleFieldMaxOrder(int subModuleId)
        {
            var max = 0;
            try
            {
                var subModuleField = await _repo.GetAsync<SubModuleField>(
                         filter: c => c.SubModuleId == Convert.ToInt32(subModuleId)
                      );

                max = subModuleField.Select(x => x.Order).Max();
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetFields: {0}", e.Message);
            }

            return max + 1;
        }

        public async Task<List<FieldDTO>> GetLifeCycleFields()
        {
            var result = new List<FieldDTO>();
            try
            {
                Dictionary<FieldType, string> fieldTypeName = new Dictionary<FieldType, string>
                {
                    {FieldType.TextField, "Text" },
                    {FieldType.NumericField, "Numeric" },
                    {FieldType.MemoField, "Memo" },
                    {FieldType.ComboField, "Dropdown" },
                    {FieldType.Attachment, "Attachment" },
                    {FieldType.Textarea, "Textarea" },
                    {FieldType.Checkbox, "Checkbox" }
                };

                var fields = await _repo.GetAsync<Field>(
                    filter: f => f.IsLifeCycle == true,
                    include: source => source.Include(x => x.FieldItem).Include(x => x.SubModuleField).ThenInclude(x => x.SubModule));
                
                var subModuleField = await _repo.GetAllAsync<SubModuleField>();

                result = _mapper.Map<List<FieldDTO>>(fields);

                foreach(FieldDTO item in result)
                {
                    item.TypeName = fieldTypeName[item.Type];
                }
                
                //list of IDs of all fields
                var listOfResultId = new List<int>(result.Select(s => Convert.ToInt32(s.Id)).ToList());

                //list of fields that is in sub modules
                var tempListOfSubModuleField = await _repo.GetAllAsync<SubModuleField>();

                //sort the IDs first by sub module, then by its order
                tempListOfSubModuleField = tempListOfSubModuleField.OrderBy(o => o.SubModuleId).ThenBy(o => o.Order);

                //remove duplicates, ex: Collection and Storage life cycle has both department, thus this will remove duplicate
                tempListOfSubModuleField = tempListOfSubModuleField.GroupBy(x => x.FieldId).Select(y => y.First());

                //convert to list
                var listOfSubModuleField = _mapper.Map<List<SubModuleFieldDTO>>(tempListOfSubModuleField);

                //get only the list of IDs
                List<int> listOfSubmoduleFieldId = tempListOfSubModuleField.Select(c => c.FieldId).Distinct().ToList();

                //add two list
                //listOfSubmoduleFieldId is the priority list, and add the other fields without making duplicates
                List<int> mainList = listOfSubmoduleFieldId.Union(listOfResultId).ToList();

                //sorted result by created list
                result = result.OrderBy(d => mainList.IndexOf(Convert.ToInt32(d.Id))).ToList();
                foreach (FieldDTO item in result)
                {
                    var tempSubModule = subModuleField.ToList().Where(y => y.FieldId == Convert.ToInt32(item.Id)).First();

                    //todo: hard coded, rework
                    item.LifeCycle = Enum.GetName(typeof(Status), tempSubModule.SubModuleId - 1);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetFields: {0}", e.Message);
            }

            return result;
        }

        public async Task<Status> GetFieldsLifeCycle(string id)
        {
            Status status = new Status();
            UpdateFieldDTO dto = new UpdateFieldDTO();
            try
            {
                var field = await _repo.GetFirstAsync<SubModuleField>(
                    filter: f => f.FieldId == Convert.ToInt32(id));

                //hard coded, rework this
                status = (Status)field.SubModuleId - 1;
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetFields: {0}", e.Message);
            }

            return status;
        }

        public async Task<List<UserRightsDTO>> GetAccessRightsByUser(string userId)
        {
            var result = new List<UserRightsDTO>();
            try
            {
                var modules = await _repo.GetAsync<UserRights>(
                       filter: c => c.UserId == Convert.ToUInt32(userId)
                   );
                result = _mapper.Map<List<UserRightsDTO>>(modules);
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetModules: {0}", e.Message);
            }
            return result;
        }
        
        public async Task<List<ApprovalDocumentDTO>> GetApprovalDocuments(string userRole, string userDept, string userId)
        {
            var result = new List<ApprovalDocumentDTO>();
            try
            {
                IEnumerable<WorkflowInbox> wfi = null;

                //new using workflow inbox
                if (userRole == nameof(Role.DEPTHEAD))
                {
                    wfi = await _repo.GetAsync<WorkflowInbox>(
                        filter: f => f.ApproverRole.Contains(userRole) && f.DepartmentId == userDept);
                }

                else if (userRole == nameof(Role.DPO) || userRole == nameof(Role.ADMINISTRATOR))
                {
                    wfi = await _repo.GetAsync<WorkflowInbox>(
                        filter: f => f.ApproverRole.Contains(userRole));
                }

                List<ApprovalDocumentDTO> approvalDocuments = new List<ApprovalDocumentDTO>();
                foreach(var item in wfi)
                {
                    var tempDocument = await GetDocumentById(item.DocumentId);

                    //check if the document is pending.
                    if(tempDocument.State == State.Pending)
                    {
                        var tempApprovalDocument = _mapper.Map<ApprovalDocumentDTO>(tempDocument);

                        //if userRole is DPO or ADMIN, they have no department
                        var tempDepartment = (item.DepartmentId == "0" || item.DepartmentId == "" || item.DepartmentId == null) ? null : await GetDepartmentById(item.DepartmentId);

                        //change departmentId to its Department Name
                        tempApprovalDocument.DepartmentId = (tempDepartment == null) ? "" : (tempDepartment.Name == "" || tempDepartment.Name == null) ? "" : tempDepartment.Name;

                        //get Dataset Name by datasetId
                        if (!String.IsNullOrEmpty(tempApprovalDocument.DatasetId.ToString()))
                        {
                            var tempDName = await GetDatasetById(tempApprovalDocument.DatasetId.ToString());
                            tempApprovalDocument.DatasetName = (tempDName == null) ? "" : tempDName.Name;
                        }

                        tempApprovalDocument.Status = item.Status;
                        approvalDocuments.Add(tempApprovalDocument);
                    }
                    
                }
                //todo: add approvaldocument details and logs
                result = _mapper.Map<List<ApprovalDocumentDTO>>(approvalDocuments);
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetFields: {0}", e.Message);
            }
            return result;
        }

        public async Task<UpdateDepartmentDTO> GetDepartmentById(string id)
        {
            UpdateDepartmentDTO result = new UpdateDepartmentDTO();
            try
            {
                var tempDepartment = await _repo.GetFirstAsync<Department>(
                    filter: f => f.Id == Convert.ToInt32(id));

                result = _mapper.Map<UpdateDepartmentDTO>(tempDepartment);

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<DepartmentDTO>> GetAllDepartments()
        {
            var result = new List<DepartmentDTO>();
            try
            {
                var departments = await _repo.GetAsync<Department>(
                    filter: f => f.Status == true);
                result = _mapper.Map<List<DepartmentDTO>>(departments);
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetFields: {0}", e.Message);
            }

            return result;
        }

        public async Task<List<ApprovalDocumentDTO>> GetRequestDocuments(string userDept, string userId)
        {
            var result = new List<ApprovalDocumentDTO>();
            try
            {
                IEnumerable<WorkflowInbox> wfi = null;
                
                wfi = await _repo.GetAsync<WorkflowInbox>(
                    filter: f => f.CreatedBy == userId);
                
                List<ApprovalDocumentDTO> requestDocuments = new List<ApprovalDocumentDTO>();
                foreach (var item in wfi)
                {
                    var tempDocument = await GetDocumentById(item.DocumentId);
                    var tempRequestDocument = _mapper.Map<ApprovalDocumentDTO>(tempDocument);

                    //if userRole is DPO or ADMIN, they have no department
                    var tempDepartment = (item.DepartmentId == "0" || item.DepartmentId == "" || item.DepartmentId == null) ? null : await GetDepartmentById(item.DepartmentId);

                    //change departmentId to its Department Name
                    tempRequestDocument.DepartmentId = (tempDepartment == null) ? "" : (tempDepartment.Name == "" || tempDepartment.Name == null) ? "" : tempDepartment.Name;

                    //get Dataset Name by datasetId
                    if (!String.IsNullOrEmpty(tempRequestDocument.DatasetId.ToString()))
                    {
                        var tempDName = await GetDatasetById(tempRequestDocument.DatasetId.ToString());
                        tempRequestDocument.DatasetName = (tempDName == null) ? "" : tempDName.Name;
                    }
                    tempRequestDocument.Status = item.Status;
                    requestDocuments.Add(tempRequestDocument);
                }

                //todo: add approvaldocument details and logs
                result = _mapper.Map<List<ApprovalDocumentDTO>>(requestDocuments);
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetRequestDocuments: {0}", e.Message);
            }
            return result;
        }

        public async Task<ApprovalDocumentDTO> GetRequestDocument(string docId)
        {
            var result = new ApprovalDocumentDTO();
            try
            {
                IEnumerable<WorkflowInbox> wfi = null;

                wfi = await _repo.GetAsync<WorkflowInbox>(
                    filter: f => f.DocumentId == docId);

                ApprovalDocumentDTO ApprovalDocument = new ApprovalDocumentDTO();
                foreach (var item in wfi)
                {
                    var tempDocument = await GetDocumentById(item.DocumentId);
                    ApprovalDocument = _mapper.Map<ApprovalDocumentDTO>(tempDocument);

                    //if userRole is DPO or ADMIN, they have no department
                    var tempDepartment = item.DepartmentId;

                    ApprovalDocument.Status = item.Status;
                    ApprovalDocument.createdBy = item.CreatedBy;
                }

                //todo: add approvaldocument details and logs
                result = ApprovalDocument;
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetRequestDocuments: {0}", e.Message);
            }
            return result;
        }

        public async Task<List<LogsDTO>> GetLogs(string docId)
        {
            List<LogsDTO> result = new List<LogsDTO>();
            try
            {
                //get all logs
                var tempLogs = await _repo.GetAsync<Logs>(
                    filter: f => f.DocId == docId);

                //get dataset name
                var dsetname = "";
                var datasetid = (await GetDocumentById(docId)).DatasetId.ToString();
                if (!String.IsNullOrEmpty(datasetid))
                {
                    var tempDName = await GetDatasetById(datasetid);
                    dsetname = (tempDName == null) ? "" : tempDName.Name;
                }

                result = _mapper.Map<List<LogsDTO>>(tempLogs);
                result.ForEach(x => x.DatasetName = dsetname);
                
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetLogs: {0}", e.Message);
            }
            return result;
        }

        public async Task<List<ResourceDTO>> GetAllResources()
        {
            var result = new List<ResourceDTO>();

            try
            {
                var resource = await _repo.GetAllAsync<Resource>();
                result = _mapper.Map<List<ResourceDTO>>(resource);
            }

            catch (Exception e)
            {
                _logger.LogError("Error calling GetAllResources: {0}", e.Message);
            }

            return result;
        }

        public async Task<SummaryItemDTO> GetSummaryOfDataSet(string userRole, string userDept)
        {
            var result = new SummaryItemDTO();
            try
            {
                if(userRole == nameof(Role.DPO) || userRole == nameof(Role.ADMINISTRATOR))
                {
                    var dataSets = await GetApprovedDocuments();
                    var departmentIds = dataSets.Select(s => s.DepartmentId).Distinct().ToList();
                    
                    if(dataSets.Count > 0)
                    {
                        foreach (string id in departmentIds)
                        {
                            string departmentName = (await GetDepartmentById(id)).Name;
                            string documentCount = (await GetApprovedDocumentsByDepartment(id)).ToList().Count.ToString();

                            result.Label = result.Label + ((result.Label == null || result.Label == "") ? "" : ",") + departmentName;
                            result.Count = result.Count + ((result.Count == null || result.Count == "") ? "" : ",") + documentCount;
                            result.Percentage = "";
                        }
                    }
                    else if (dataSets.Count == 0)
                    {
                        result.Label = "no data";
                    }                    
                }
                else if (userRole == nameof(Role.DEPTHEAD) || userRole == nameof(Role.USER))
                {
                    var documents = (await GetApprovedDocumentsByDepartment(userDept)).Where(w => w.DatasetId != 0).ToList();
                    var dataSetIds = documents.Select(s => s.DatasetId).Distinct().ToList();

                    var documentWithDataCounter = documents.Where(w => w.DatasetId == 0).ToList().Count;

                    if (documents.Count > 0)
                    {
                        foreach (int id in dataSetIds)
                        {
                            string datasetName = (await GetDatasetById(id.ToString())).Name;
                            string documentCount = documents.Where(w => w.DatasetId == id).ToList().Count().ToString();

                            result.Label = result.Label + ((result.Label == null || result.Label == "") ? "" : ",") + datasetName;
                            result.Count = result.Count + ((result.Count == null || result.Count == "") ? "" : ",") + documentCount;
                            result.Percentage = "";
                        }
                    }
                    else if (documents.Count == 0)
                    {
                        result.Label = "no data";
                    }
                    if (documentWithDataCounter > 0)
                    {
                        result.Label = result.Label + ((result.Label == null || result.Label == "") ? "" : ",") + "N/A";
                        result.Count = result.Count + ((result.Count == null || result.Count == "") ? "" : ",") + documentWithDataCounter.ToString();
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetModules: {0}", e.Message);
            }

            return result;
        }

        public async Task<List<SubModuleFieldDTO>> GetAllSubModuleFields()
        {
            var result = new List<SubModuleFieldDTO>();
            try
            {
                var subModulesField = await _repo.GetAsync<SubModuleField>
                (
                   include: source => source.Include(c => c.Field).ThenInclude(c => c.FieldItem)
                );

                subModulesField = subModulesField.OrderBy(o => o.SubModuleId).ThenBy(t => t.FieldId).ToList();

                result = _mapper.Map<List<SubModuleFieldDTO>>(subModulesField);
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetModuleFields: {0}", e.Message);
            }

            return result;
        }

        public async Task<SummaryItemDTO> GetSummaryOfStorage(string userRole, string userDept)
        {
            var result = new SummaryItemDTO();
            try
            {
                //get items of Storage Field, 13 is the ID of Storage field
                var storageItems = await _repo.GetAsync<FieldItem>(
                    filter: f => f.FieldId == 13);

                //count total document with field item
                int totalFieldCount = 0;
                int documentCounter = 0;
                int documentWithoutDataCounter = 0;

                if(userRole == nameof(Role.DPO) || userRole == nameof(Role.ADMINISTRATOR))
                {
                    var storageInDocuments = await _repo.GetAsync<DocumentField>
                    (
                        filter: f => f.FieldId == 13
                        && ((f.Document.State == State.Approved && f.Document.RequestType != RequestType.Delete)
                        || (f.Document.State != State.Approved && f.Document.RequestType == RequestType.Delete))
                    );

                    documentCounter = storageInDocuments.ToList().Count != 0 ? storageInDocuments.ToList().Count : 0;

                    documentWithoutDataCounter = storageInDocuments.Where(w => w.Value == "").ToList().Count;

                    if (documentCounter > 0)
                    {
                        foreach (FieldItem item in storageItems)
                        {
                            int itemCount = 0;
                            string itemName = item.Name;

                            itemCount = storageInDocuments.Where(x => x.Value.Split(',').ToList().Contains(itemName)).ToList().Count;

                            totalFieldCount += itemCount;
                            string documentFieldItemCount = itemCount.ToString();

                            result.Label = result.Label + ((result.Label == null || result.Label == "") ? "" : ",") + itemName;
                            result.Count = result.Count + ((result.Count == null || result.Count == "") ? "" : ",") + documentFieldItemCount;
                            //todo: percentage
                            result.Percentage = "";
                        }

                        //Check if there are document that has no Storage's field data
                        if (documentWithoutDataCounter > 0)
                        {
                            result.Label = result.Label + ((result.Label == null || result.Label == "") ? "" : ",") + "N/A";
                            result.Count = result.Count + ((result.Count == null || result.Count == "") ? "" : ",") + documentWithoutDataCounter.ToString();
                        }
                    }

                    else if(documentCounter == 0)
                    {
                        result.Label = "no data";
                    }                    
                }

                else if (userRole == nameof(Role.DEPTHEAD) || userRole == nameof(Role.USER))
                {
                    //count document with user's department
                    var storageInDocuments = await _repo.GetAsync<DocumentField>
                    (
                        filter: f => f.Document.DepartmentId == userDept && f.FieldId == 13
                        && ((f.Document.State == State.Approved && f.Document.RequestType != RequestType.Delete)
                        || (f.Document.State != State.Approved && f.Document.RequestType == RequestType.Delete))
                    );

                    documentCounter = storageInDocuments.ToList().Count != 0 ? storageInDocuments.ToList().Count : 0;

                    documentWithoutDataCounter = storageInDocuments.Where(w => w.Value == "").ToList().Count;

                    if (documentCounter > 0)
                    {
                        foreach (FieldItem item in storageItems)
                        {
                            int itemCount = 0;
                            string itemName = item.Name;

                            itemCount = storageInDocuments.Where(x => x.Value.Split(',').ToList().Contains(itemName)).ToList().Count;

                            totalFieldCount += itemCount;
                            string documentFieldItemCount = itemCount.ToString();

                            result.Label = result.Label + ((result.Label == null || result.Label == "") ? "" : ",") + itemName;
                            result.Count = result.Count + ((result.Count == null || result.Count == "") ? "" : ",") + documentFieldItemCount;
                            //todo: percentage
                            result.Percentage = "";
                        }

                        //Check if there are document that has no Storage's field data
                        if (documentWithoutDataCounter > 0)
                        {
                            result.Label = result.Label + ((result.Label == null || result.Label == "") ? "" : ",") + "N/A";
                            result.Count = result.Count + ((result.Count == null || result.Count == "") ? "" : ",") + documentWithoutDataCounter.ToString();
                        }
                    }
                    else if(documentCounter == 0)
                    {
                        result.Label = "no data";
                    }                    
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetSummaryStorage: {0}", e.Message);
            }

            return result;
        }
        
        public async Task<SummaryItemDTO> GetSummaryOfExternalParty(string userRole, string userDept)
        {
            var result = new SummaryItemDTO();
            try
            {
                //get items of /External Party field, 24 is the ID of Storage field
                var storageItems = await _repo.GetAsync<FieldItem>(
                    filter: f => f.FieldId == 24);

                //count total document with field item
                int totalFieldCount = 0;
                int documentCounter = 0;
                int documentWithoutDataCounter = 0;

                if (userRole == nameof(Role.DPO) || userRole == nameof(Role.ADMINISTRATOR))
                {
                    var storageInDocuments = await _repo.GetAsync<DocumentField>
                    (
                        filter: f => f.FieldId == 24 && ((f.Document.State == State.Approved && f.Document.RequestType != RequestType.Delete)
                        || (f.Document.State != State.Approved && f.Document.RequestType == RequestType.Delete))
                    );

                    documentCounter = storageInDocuments.ToList().Count != 0 ? storageInDocuments.ToList().Count : 0;

                    documentWithoutDataCounter = storageInDocuments.Where(w => w.Value == "").ToList().Count;

                    if (documentCounter > 0)
                    {
                        foreach (FieldItem item in storageItems)
                        {
                            int itemCount = 0;
                            string itemName = item.Name;

                            itemCount = storageInDocuments.Where(x => x.Value.Split(',').ToList().Contains(itemName)).ToList().Count;

                            totalFieldCount += itemCount;
                            string documentFieldItemCount = itemCount.ToString();

                            result.Label = result.Label + ((result.Label == null || result.Label == "") ? "" : ",") + itemName;
                            result.Count = result.Count + ((result.Count == null || result.Count == "") ? "" : ",") + documentFieldItemCount;
                            result.Percentage = "";
                        }

                        //Check if there are document that has no Storage's field data
                        if (documentWithoutDataCounter > 0)
                        {
                            //todo rework this 2-15-2019
                            result.Label = result.Label + ((result.Label == null || result.Label == "") ? "" : ",") + "N/A";
                            result.Count = result.Count + ((result.Count == null || result.Count == "") ? "" : ",") + documentWithoutDataCounter.ToString();
                        }
                    }
                    else if(documentCounter == 0)
                    {
                        result.Label = "no data";
                    }                    
                }
                else if (userRole == nameof(Role.DEPTHEAD) || userRole == nameof(Role.USER))
                {
                    //count document with user's department
                    var storageInDocuments = await _repo.GetAsync<DocumentField>
                    (
                        filter: f => f.Document.DepartmentId == userDept && f.FieldId == 24
                        && ((f.Document.State == State.Approved && f.Document.RequestType != RequestType.Delete)
                        || (f.Document.State != State.Approved && f.Document.RequestType == RequestType.Delete))
                    );

                    documentCounter = storageInDocuments.ToList().Count != 0 ? storageInDocuments.ToList().Count : 0;

                    documentWithoutDataCounter = storageInDocuments.Where(w => w.Value == "").ToList().Count;

                    if (documentCounter > 0)
                    {
                        foreach (FieldItem item in storageItems)
                        {
                            int itemCount = 0;
                            string itemName = item.Name;

                            itemCount = storageInDocuments.Where(x => x.Value.Split(',').ToList().Contains(itemName)).ToList().Count;

                            totalFieldCount += itemCount;
                            string documentFieldItemCount = itemCount.ToString();

                            result.Label = result.Label + ((result.Label == null || result.Label == "") ? "" : ",") + itemName;
                            result.Count = result.Count + ((result.Count == null || result.Count == "") ? "" : ",") + documentFieldItemCount;
                            //todo: percentage
                            result.Percentage = "";
                        }

                        //Check if there are document that has no Storage's field data
                        if (documentWithoutDataCounter > 0)
                        {
                            result.Label = result.Label + ((result.Label == null || result.Label == "") ? "" : ",") + "N/A";
                            result.Count = result.Count + ((result.Count == null || result.Count == "") ? "" : ",") + documentWithoutDataCounter.ToString();
                        }
                    }
                    else if(documentCounter == 0)
                    {
                        result.Label = "no data";
                    }                    
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetSummaryExternalParty: {0}", e.Message);
            }

            return result;
        }

        public async Task<List<DocumentDTO>> GetDocumentsByRole(string userRole, string userDept)
        {
            var result = new List<DocumentDTO>();
            try
            {
                if(userRole == nameof(Role.DPO) || userRole == nameof(Role.ADMINISTRATOR))
                {
                    var documents = await _repo.GetAsync<Document>
                    (
                        include: source => source.Include(c => c.DocumentField)
                    );

                    //Sort document's fields by field ID
                    //keep the document fields sorted. Sorting is needed for exporting CSV. Sort by ascending
                    foreach (Document doc in documents)
                    {
                        doc.DocumentField = doc.DocumentField.ToList().OrderBy(o => o.FieldId).ToList();
                    }

                    if (documents.Count() > 0)
                    {
                        result = _mapper.Map<List<DocumentDTO>>(documents);
                    }
                }

                else if(userRole == nameof(Role.USER) || userRole == nameof(Role.DEPTHEAD))
                {
                    var documents = await _repo.GetAsync<Document>
                    (
                        filter: f => f.DepartmentId == userDept,
                        include: source => source.Include(c => c.DocumentField)
                    );

                    foreach (Document doc in documents)
                    {
                        doc.DocumentField = doc.DocumentField.ToList().OrderBy(o => o.FieldId).ToList();
                    }

                    if (documents.Count() > 0)
                    {
                        result = _mapper.Map<List<DocumentDTO>>(documents);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetDocuments: {0}", e.Message);
            }

            return result;
        }

        public async Task<SummaryItemDTO> GetSummaryOfDatasetIssue(string userRole, string userDept)
        {
            var result = new SummaryItemDTO();
            try
            {
                if (userRole == nameof(Role.DPO) || userRole == nameof(Role.ADMINISTRATOR))
                {
                    var documentIssue = await _repo.GetAsync<Issues>(
                        filter: f => (f.Document.State == State.Approved && f.Document.RequestType != RequestType.Delete)
                            || (f.Document.State != State.Approved && f.Document.RequestType == RequestType.Delete),
                        include: i => i.Include(c => c.Document));

                    var departmentIds = documentIssue.Select(s => s.DepartmentId).Distinct().ToList();

                    if (documentIssue.Count() > 0)
                    {
                        foreach (string id in departmentIds)
                        {
                            string departmentName = (await GetDepartmentById(id)).Name;
                            var IssueCount = documentIssue.ToList().Where(x => x.DepartmentId == id).Count();

                            result.Label = result.Label + ((result.Label == null || result.Label == "") ? "" : ",") + departmentName;
                            result.Count = result.Count + ((result.Count == null || result.Count == "") ? "" : ",") + IssueCount;
                            result.Percentage = "";
                        }
                    }
                    else if (documentIssue.ToList().Count == 0)
                    {
                        result.Label = "no data";
                    }
                }
                else if (userRole == nameof(Role.DEPTHEAD) || userRole == nameof(Role.USER))
                {

                    var documents = await GetApprovedDocumentsByDepartment(userDept);
                    var dataSetIds = documents.Select(s => s.DatasetId).Distinct().ToList();
                    var issue = await GetIssuesByRole(userRole, userDept);

                    var documentWithDataCounter = documents.Where(w => w.DatasetId == 0).ToList().Count;
                    if (issue.Count == 0)
                    {
                        result.Label = "no data";
                    }
                    else
                    { 
                        if (documents.Count > 0)
                        {
                            foreach (int id in dataSetIds)
                            {
                                var datasetName = await GetDatasetById(id.ToString());

                                if (datasetName != null) {
                                    string issueCounter = issue.Where(w => w.DatasetName == datasetName.Name).ToList().Count().ToString();

                                    result.Label = result.Label + ((result.Label == null || result.Label == "") ? "" : ",") + datasetName.Name;
                                    result.Count = result.Count + ((result.Count == null || result.Count == "") ? "" : ",") + issueCounter;
                                    result.Percentage = "";
                                }
                                else
                                {
                                    string issueCounter = issue.Where(w => w.DatasetName == "").ToList().Count().ToString();
                                    result.Label = result.Label + ((result.Label == null || result.Label == "") ? "" : ",") + "N/A";
                                    result.Count = result.Count + ((result.Count == null || result.Count == "") ? "" : ",") + issueCounter;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetDocuments: {0}", e.Message);
            }

            return result;
        }

        public async Task<List<IssueDTO>> GetIssues(string docId)
        {
            var result = new List<IssueDTO>();
            try
            {
                if (!String.IsNullOrEmpty(docId))
                {
                    var issues = await _repo.GetAsync<Issues>();
                    if (docId != "0")
                    {
                        issues = issues.Where(x => x.DocId == Convert.ToInt32(docId));
                    }
                    result = _mapper.Map<List<IssueDTO>>(issues);
                    foreach (var item in result)
                    {
                        var docInfo = await GetDocumentById(item.DocId.ToString());
                        if(docInfo != null)
                        {
                            if (Convert.ToInt32(docInfo.DepartmentId) > 0)
                            {
                                var dept = await GetDepartmentById(docInfo.DepartmentId);
                                item.Department = dept.Name;
                            }
                            if (!String.IsNullOrEmpty(docInfo.DatasetId.ToString()))
                            {
                                var tempDName = await GetDatasetById(docInfo.DatasetId.ToString());
                                item.DatasetName = (tempDName == null) ? "" : tempDName.Name;
                            }
                        }
                        item.DataNumber = docInfo.DataNumber;

                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetIssues: {0}", e.Message);
            }

            return result;
        }

        public async Task<List<IssueDTO>> GetIssuesByRole(string userRole, string userDept)
        {
            var result = new List<IssueDTO>();
            try
            {
                if (userRole == nameof(Role.DPO) || userRole == nameof(Role.ADMINISTRATOR))
                {
                    var issues = await _repo.GetAllAsync<Issues>();

                    result = _mapper.Map<List<IssueDTO>>(issues);

                    foreach (IssueDTO item in result)
                    {
                        item.Department = (await GetDepartmentById(item.DepartmentId)).Name;
                        item.DataNumber = (await GetDocumentById(item.DocId.ToString())).DataNumber;

                        var docInfo = await GetDocumentById(item.DocId.ToString());
                        if (!String.IsNullOrEmpty(docInfo.DatasetId.ToString()))
                        {
                            var tempDName = await GetDatasetById(docInfo.DatasetId.ToString());
                            item.DatasetName = (tempDName == null) ? "" : tempDName.Name;
                        }
                    }
                }
                else if (userRole == nameof(Role.DEPTHEAD) || userRole == nameof(Role.USER))
                {
                    var issues = await _repo.GetAsync<Issues>(
                        filter: f => f.DepartmentId == userDept);

                    result = _mapper.Map<List<IssueDTO>>(issues);

                    foreach (IssueDTO item in result)
                    {

                        item.Department = (await GetDepartmentById(item.DepartmentId)).Name;
                        item.DataNumber = (await GetDocumentById(item.DocId.ToString())).DataNumber;

                        var docInfo = await GetDocumentById(item.DocId.ToString());
                        if (!String.IsNullOrEmpty(docInfo.DatasetId.ToString()))
                        {
                            var tempDName = await GetDatasetById(docInfo.DatasetId.ToString());
                            item.DatasetName = (tempDName == null) ? "" : tempDName.Name;
                        }

                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetIssues: {0}", e.Message);
            }

            return result;
        }

        public async Task<List<DocumentDTO>> GetAllDatasets(string userRole, string userDept, string userId)
        {
            var result = new List<DocumentDTO>();
            try
            {
                if (userRole == nameof(Role.DPO) || userRole == nameof(Role.ADMINISTRATOR))
                {
                    var documents = await _repo.GetAsync<Document>
                    (
                        filter: f => (f.State == State.Approved && f.RequestType != RequestType.Delete)
                        || (f.State != State.Approved && f.RequestType == RequestType.Delete),
                        include: source => source.Include(c => c.DocumentField).ThenInclude(c => c.Field)
                    );

                    result = _mapper.Map<List<DocumentDTO>>(documents);
                }
                else if (userRole == nameof(Role.DEPTHEAD))
                {
                    var documents = await _repo.GetAsync<Document>
                    (
                        filter: f => f.DepartmentId == userDept
                        && ((f.State == State.Approved && f.RequestType != RequestType.Delete)
                        || (f.State != State.Approved && f.RequestType == RequestType.Delete)),
                        include: source => source.Include(c => c.DocumentField).ThenInclude(c => c.Field)
                    );

                    result = _mapper.Map<List<DocumentDTO>>(documents);
                }
                else if (userRole == nameof(Role.USER))
                {
                    var documents = await _repo.GetAsync<Document>
                    (
                        filter: f => f.CreatedBy == userId
                        && ((f.State == State.Approved && f.RequestType != RequestType.Delete)
                        || (f.State != State.Approved && f.RequestType == RequestType.Delete)),
                        include: source => source.Include(c => c.DocumentField).ThenInclude(c => c.Field)
                    );

                    result = _mapper.Map<List<DocumentDTO>>(documents);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetFields: {0}", e.Message);
            }

            return result;
        }

        public async Task<List<DocumentFieldDTO>> GetSDocumentFieldsBySubModule(string docId, string subModuleId)
        {
            var result = new List<DocumentFieldDTO>();
            try
            {
                var documentFields = (await _repo.GetAsync<DocumentField>(
                    filter: f => f.DocumentId == Convert.ToInt32(docId) && f.SubModuleId == Convert.ToInt32(subModuleId),
                    include: source => source.Include(x => x.Field))).OrderBy(o => o.FieldId).ToList();

                result = _mapper.Map<List<DocumentFieldDTO>>(documentFields);
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetFields: {0}", e.Message);
            }

            return result;
        }

        public async Task<Boolean> DoFileExist(string ResourceFolder, string fileName)
        {
            bool doFileExist = false;
            try
            {
                var file = await _repo.GetAsync<Resource>(
                    filter: f => f.FilePath == ResourceFolder + "/" + fileName);

                if(file.Count() > 0)
                {
                    doFileExist = true;
                }

                else if(file.Count() == 0)
                {
                    doFileExist = false;
                }

                return doFileExist;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<DocumentDTO>> GetApprovedDocuments()
        {
            var result = new List<DocumentDTO>();
            try
            {
                var documents = await _repo.GetAsync<Document>(
                    filter: f => (f.State == State.Approved && f.RequestType != RequestType.Delete)
                    || (f.State != State.Approved && f.RequestType == RequestType.Delete),
                    include: source => source.Include(c => c.DocumentField).ThenInclude(c => c.Field)
                );

                foreach (Document doc in documents)
                {
                    doc.DocumentField = doc.DocumentField.ToList().OrderBy(o => o.FieldId).ToList();
                }

                if (documents.Count() > 0)
                {
                    result = _mapper.Map<List<DocumentDTO>>(documents);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetFields: {0}", e.Message);
            }

            return result;
        }

        public async Task<List<DocumentDTO>> GetApprovedDocumentsByDepartment(string userDept)
        {
            var result = new List<DocumentDTO>();
            try
            {
                var documents = await _repo.GetAsync<Document>(
                    filter: f => f.DepartmentId == userDept
                    && ((f.State == State.Approved && f.RequestType != RequestType.Delete)
                    || (f.State != State.Approved && f.RequestType == RequestType.Delete)),
                    include: source => source.Include(c => c.DocumentField).ThenInclude(c => c.Field)
                );

                foreach (Document doc in documents)
                {
                    doc.DocumentField = doc.DocumentField.ToList().OrderBy(o => o.FieldId).ToList();
                }

                if (documents.Count() > 0)
                {
                    result = _mapper.Map<List<DocumentDTO>>(documents);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetApprovedDocumentsByDepartment: {0}", e.Message);
            }

            return result;
        }

        public async Task<List<DocumentDTO>> GetApprovedDocumentsByUser(string userId)
        {
            var result = new List<DocumentDTO>();
            try
            {
                var documents = await _repo.GetAsync<Document>(
                    filter: f => f.CreatedBy == userId
                    && ((f.State == State.Approved && f.RequestType != RequestType.Delete)
                    || (f.State != State.Approved && f.RequestType == RequestType.Delete)),
                    include: source => source.Include(c => c.DocumentField).ThenInclude(c => c.Field)
                );
                result = _mapper.Map<List<DocumentDTO>>(documents);
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetApprovedDocumentsByUser: {0}", e.Message);
            }

            return result;
        }

        public async Task<List<FieldItemDTO>> GetFieldItems(string id, string value, string selected)
        {
            var result = new List<FieldItemDTO>();
            try
            {
                string x = id;
                var field = await _repo.GetAsync<FieldItem>(filter: f => f.FieldId == Convert.ToInt32(id));
                value = String.IsNullOrEmpty(value) ? "" : value;
                if (!String.IsNullOrEmpty(value))
                {
                    field = await _repo.GetAsync<FieldItem>(filter: f => f.FieldId == Convert.ToInt32(id) && f.Name.ToUpper().Contains(value.ToUpper()));
                }
                if (!String.IsNullOrEmpty(selected))
                {
                    var list = selected.Split(',').ToList();
                    field = await _repo.GetAsync<FieldItem>(filter: f => f.FieldId == Convert.ToInt32(id) && f.Name.ToUpper().Contains(value.ToUpper()) && !list.Contains(f.Name.ToUpper()));
                }
                result = _mapper.Map<List<FieldItemDTO>>(field);
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetFieldItems: {0}", e.Message);
            }

            return result;
        }

        public async Task<CompanyInfoDTO> GetCompanyInfo()
        {
            CompanyInfoDTO result = new CompanyInfoDTO();
            try
            {
                var company = await _repo.GetFirstAsync<Company>();
                result = _mapper.Map<CompanyInfoDTO>(company);
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetCompanyInfo: {0}", e.Message);
            }

            return result;
        }
    }
}