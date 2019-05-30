using Microsoft.Extensions.Logging;
using DDPA.Commons.Results;
using System.Threading.Tasks;
using DDPA.DTO;
using DDPA.SQL.Entities;
using DDPA.SQL.Repositories;
using DDPA.Validation;
using System;
using static DDPA.Commons.Enums.DDPAEnums;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace DDPA.Service
{
    public class ApprovalService : IApprovalService
    {
        protected readonly ILogger _logger;
        protected readonly IRepository _repo;
        protected readonly IAdminService _adminService;
        protected readonly IValidationService _validationService;
        protected readonly UserManager<ExtendedIdentityUser> _userManager;

        public ApprovalService(ILogger<AccountService> logger, IRepository repo, UserManager<ExtendedIdentityUser> userManager, IValidationService validationService, IAdminService adminService) 
        {
            _logger = logger;
            _repo = repo;
            _adminService = adminService;
            _userManager = userManager;
            _validationService = validationService;
        }

        public async Task<Result> ApproveDocuments(List<string> ids, string userRole, string userId)
        {
            int count = 0;
            Result result = new Result();

            //approve each document. Add ID to respective approver
            foreach (string id in ids)
            {
                var wfi = await _repo.GetFirstAsync<WorkflowInbox>(
                           filter: (c => c.DocumentId ==id));
                Document approvedDocument = await _repo.GetByIdAsync<Document>(Convert.ToInt32(id));

                try
                {
                    //if the approver is Department Head
                    if (userRole == nameof(Role.DEPTHEAD))
                    {
                        //after approving by Department Head, change the approver to higher authority which is DPO and ADMIN
                        wfi.ApproverRole = nameof(Role.DPO) + "," + nameof(Role.ADMINISTRATOR);
                        _repo.Update(wfi);
                        await _repo.SaveAsync();
                    }

                    //if the approver is DPO, or ADMINISTRATOR
                    else if (userRole == nameof(Role.DPO) || userRole == nameof(Role.ADMINISTRATOR))
                    {
                        //after approving by DPO or ADMIN, remove the item in inbox and change the status of the Document

                        //update status of approved Document

                        approvedDocument.State = State.Approved;
                        if (approvedDocument.RequestType == RequestType.Edit)
                        {
                            var fields = await _repo.GetAsync<DocumentField>(
                                filter: (c => c.DocumentId == Convert.ToInt32(wfi.DocumentId)));
                            var docField = fields.OrderBy(x => x.SubModuleId);

                            foreach (var field in docField)
                            {
                                if (field.IsEdited)
                                {
                                    field.Value = field.NewValue;
                                    field.IsEdited = false;
                                    //TODO:insert to data history the change edited value
                                    field.NewValue = null;

                                    _repo.Update(field);
                                    await _repo.SaveAsync();
                                }
                                approvedDocument.Status = (Status)(field.SubModuleId-1);
                            }
                        }
                       
                        _repo.Update(approvedDocument);
                        await _repo.SaveAsync();
                        
                        //removing item in inbox
                        _repo.Delete(wfi);
                        _repo.Save();

                    }
                    //Add to logs
                    LogsDTO dtoLogs = new LogsDTO();
                    dtoLogs.DocId = approvedDocument.Id.ToString(); ;
                    dtoLogs.DataNumber = approvedDocument.DataNumber;
                    dtoLogs.UserId = userId;
                    dtoLogs.Action = "Approve";
                    dtoLogs.Description = "Document approved by " + userRole;
                    var log = await _adminService.AddLogs(dtoLogs);

                    //count how many documents have been approved
                    count++;
                    result.Message = count.ToString() + " " + (count > 1 ? "documents" : "document") + " " + (count > 1 ? "have" : "has") + " been successfully approved.";
                    result.Success = true;
                }
                catch (Exception e)
                {
                    result.Success = false;
                    result.Message = "Error approving document.";
                    result.ErrorCode = ErrorCode.EXCEPTION;
                    _logger.LogError("Error calling ApproveDocuments: {0}", e.Message);
                }
            }

            return result;
        }

        public async Task<Result> ReworkDocument(string id, string userRole, string userId, string comment)
        {
            Result result = new Result();
            try
            {
                var wfi = await _repo.GetFirstAsync<WorkflowInbox>(
                    filter: c => c.DocumentId == id);

                //update status of rework Document
                Document reworkDocument = await _repo.GetByIdAsync<Document>(Convert.ToInt32(wfi.DocumentId));

                reworkDocument.State = State.Rework;
                _repo.Update(reworkDocument);
                await _repo.SaveAsync();

                //update item in inbox
                wfi.ApproverRole = nameof(Role.USER);
                _repo.Update(wfi);
                await _repo.SaveAsync();
                result.Message = "Document has been successfully sent for rework.";

                //Add to logs
                LogsDTO dtoLogs = new LogsDTO
                {
                    DocId = reworkDocument.Id.ToString(),
                    DataNumber = reworkDocument.DataNumber,
                    UserId = userId,
                    Action = "Rework",
                    Description = "Document reworked by " + userRole,
                    Comment = comment
            };
                
                var log = await _adminService.AddLogs(dtoLogs);

                result.Success = true;
              
            }
            catch (Exception e)
            {
                result.Success = false;
                result.Message = "Error rework document.";
                result.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error calling ReworkDocuments: {0}", e.Message);
            }
            return result;
        }

        public async Task<Result> BulkDeleteDrafts(List<string> id)
        {
            int count = 0;
            Result result = new Result();
            try
            {
                foreach (string tempId in id)
                {
                    Document doc = _repo.GetById<Document>(Convert.ToInt32(tempId));
                    _repo.Delete(doc);

                    WorkflowInbox wf = await _repo.GetFirstAsync<WorkflowInbox>(
                        filter: f => f.DocumentId == tempId);
                    _repo.Delete(wf);
                    _repo.Save();
                    count++;
                    result.Success = true;
                }
            }
            catch (Exception e)
            {
                result.Message = "Error deleting draft.";
                result.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error calling BulkDeleteDrafts: {0}", e.Message);
            }

            return result;
        }
    }
}