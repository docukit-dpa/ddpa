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

namespace DDPA.Service
{
    public class DatasetService : IDatasetService
    {
        protected readonly ILogger _logger;
        protected readonly IRepository _repo;
        protected readonly IValidationService _validationService;
        protected readonly UserManager<ExtendedIdentityUser> _userManager;
        private readonly IQueryService _queryService;

        public DatasetService(ILogger<DatasetService> logger, IRepository repo, UserManager<ExtendedIdentityUser> userManager, IValidationService validationService, IQueryService queryService) 
        {
            _logger = logger;
            _repo = repo;
            _userManager = userManager;
            _validationService = validationService;
            _queryService = queryService;
        }

        public async Task<Result> AddDataset(AddDatasetDTO dto, string userId)
        {
            Result result = new Result();
            try
            {
                ValidationResult valResult = await _validationService.IsValidDataset(dto.Name);
                if (!valResult.IsValid)
                {
                    result.Message = valResult.Message;
                    return result;
                }

                Dataset dataset = new Dataset
                {
                    CreatedBy = userId,
                    Name = dto.Name,
                    Description = dto.Description
                };

                _repo.Create(dataset);
                await _repo.SaveAsync();
                result.Message = "Dataset has been successfully added.";
                result.Success = true;
            }
            catch (Exception)
            {

                throw;
            }
            return result;
        }

        public async Task<Result> UpdateDataset(UpdateDatasetDTO dto)
        {
            Result result = new Result();
            try
            {
                var dataset = await _repo.GetByIdAsync<Dataset>(Convert.ToInt32(dto.Id));
                if(dataset.Name != dto.Name)
                {
                    ValidationResult valResult = await _validationService.IsValidDataset(dto.Name);
                    if (!valResult.IsValid)
                    {
                        result.Message = valResult.Message;
                        return result;
                    }
                }

                dataset.Name = dto.Name;
                dataset.Description = dto.Description;

                _repo.Update(dataset);
                await _repo.SaveAsync();
                
                result.Message = "Dataset has been successfully updated.";
                result.Success = true;
            }
            catch (Exception)
            {

                throw;
            }
            return result;
        }

        public Result DeleteDataset(string id)
        {
            Result result = new Result();
            Dataset dataset = _repo.GetById<Dataset>(Convert.ToInt32(id));
            try
            {
                _repo.Delete(dataset);
                _repo.Save();
                result.Message = "Dataset has been successfully deleted.";
                result.Success = true;
            }
            catch (Exception e)
            {
                result.Message = "Error deleting dataset.";
                result.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error calling DeleteDataset: {0}", e.Message);
            }
            return result;
        }

        public async Task<Result> AddFieldToDataset(string datasetId, string fieldId)
        {
            Result result = new Result();
            DatasetField field = new DatasetField
            {
                DatasetId = Convert.ToInt32(datasetId),
                FieldId = Convert.ToInt32(fieldId)
            };

            _repo.Create(field);
            await _repo.SaveAsync();
            result.Message = "Field has been successfully added in the dataset.";
            result.Success = true;
            return result;
        }

        public Result DeleteDatasetField(string id)
        {
            Result result = new Result();
            DatasetField field = _repo.GetById<DatasetField>(Convert.ToInt32(id));
            try
            {
                _repo.Delete(field);
                _repo.Save();
                result.Message = "Field has been successfully deleted in the dataset.";
                result.Success = true;
            }
            catch (Exception e)
            {
                result.Message = "Error deleting field.";
                result.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error calling DeleteDatasetField: {0}", e.Message);
            }
            return result;
        }

        public async Task<Result> AddIssue(IssueDTO dto, string userId, string userDept, string userRole)
        {
            Result result = new Result();
            try
            {
                if(userRole == nameof(Role.DPO) || userRole == nameof(Role.ADMINISTRATOR))
                {
                    userDept = (await _queryService.GetDocumentById(dto.DocId.ToString())).DepartmentId;
                }
                Issues issues = new Issues
                {
                    CreatedBy = userId,
                    CreatedDate = DateTime.UtcNow,
                    DocId = dto.DocId,
                    DepartmentId = userDept,
                    Issue = dto.Issue,
                    SeverityLevel = dto.SeverityLevel,
                    Description = dto.Description,
                    Date= dto.Date,
                    AssignedTo = dto.AssignedTo,
                    Action  = dto.Action,
                    Status = dto.Status
                };

                _repo.Create(issues);
                await _repo.SaveAsync();
                result.Message = "Issue has been successfully added.";
                result.IsRedirect = false;
                result.Id = "Issue";
                result.Success = true;
            }
            catch (Exception)
            {
                throw;
            }
            return result;
        }

        public async Task<Result> EditIssue(IssueDTO dto, string userId, string userDept)
        {
            Result result = new Result();
            try
            {
                Issues issue = _repo.GetById<Issues>(Convert.ToInt32(dto.Id));
                issue.Issue = dto.Issue;
                issue.SeverityLevel = dto.SeverityLevel;
                issue.Description = dto.Description;
                issue.AssignedTo = dto.AssignedTo;
                issue.Date = dto.Date;
                issue.Action = dto.Action;
                issue.Status = dto.Status;
                issue.ModifiedBy = userId;
                issue.ModifiedDate = DateTime.UtcNow;

                _repo.Update(issue);
                await _repo.SaveAsync();
                result.Message = "Issue has been successfully updated.";
                result.Success = true;
            }
            catch (Exception)
            {
                throw;
            }
            return result;
        }

    }
}