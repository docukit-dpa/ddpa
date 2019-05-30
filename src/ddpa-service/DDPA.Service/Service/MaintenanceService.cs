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
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DDPA.Service
{
    public class MaintenanceService : IMaintenanceService
    {
        protected readonly ILogger _logger;
        protected readonly IRepository _repo;
        protected readonly IValidationService _validationService;
        protected readonly UserManager<ExtendedIdentityUser> _userManager;

        public MaintenanceService(ILogger<AccountService> logger, IRepository repo, UserManager<ExtendedIdentityUser> userManager, IValidationService validationService) 
        {
            _logger = logger;
            _repo = repo;
            _userManager = userManager;
            _validationService = validationService;
        }

        public async Task<Result> CreateUser(AddUserDTO dto)
        {
            var response = new Result();
            try
            {
                var valResult = await _validationService.IsValidAccountInfo(dto.UserName, dto.Email, dto.Password, dto.ConfirmPassword);
                if (!valResult.IsValid)
                {
                    response.Message = valResult.Message;
                    response.ErrorCode = ErrorCode.INVALID_INPUT;
                    return response;
                }

                if (string.IsNullOrEmpty(dto.DepartmentId))
                {
                    response.Message = "Please fill in the required fields.";
                    response.ErrorCode = ErrorCode.INVALID_INPUT;
                    return response;
                }

                if (!Enum.IsDefined(typeof(TypeOfNotification), dto.TypeOfNotification))
                {
                    response.Message = "Please fill in the required fields.";
                    response.ErrorCode = ErrorCode.INVALID_INPUT;
                    return response;
                }

                ExtendedIdentityUser user = new ExtendedIdentityUser
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    UserName = dto.UserName,
                    DepartmentId = dto.DepartmentId,
                    CreatedBy = dto.CreatedBy,
                    EmailConfirmed = true,
                    TypeOfNotification = dto.TypeOfNotification,
                    HasPasswordChanged = false
                };

                var result = await _userManager.CreateAsync(user, dto.Password);
                if (result.Succeeded)
                {
                    var addrole = await _userManager.AddToRoleAsync(user, dto.Role);
                    //Default Module Access Rights
                    var modules = _repo.GetAll<Module>();
                   
                    foreach (var item in modules)
                    {
                        var rights = new UserRights();
                        rights.UserId = user.Id;
                        rights.ModuleId = item.Id;
                        rights.View = true;
                        rights.Add = true;
                        rights.Edit = true;
                        if (dto.Role == "USER")
                        {
                            if (item.Name == "Maintenance")
                            {
                                rights.Add = false;
                                rights.Edit = false;
                            }
                            else if (item.Name == "Approval")
                            {
                                rights.View = true;
                                rights.Add = false;
                                rights.Edit = false;
                            }
                        }

                        rights.Delete = false;
                        _repo.Create(rights);
                        await _repo.SaveAsync();
                    };
                    
                    response.Success = true;
                    response.Message = "User has been successfully added.";
                    response.ErrorCode = ErrorCode.DEFAULT;
                }
            }
            catch (Exception e)
            {
                response.Message = "Error adding company";
                response.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error calling AddCompany: {0}", e.Message);
            }

            return response;
        }

        public async Task<Result> UpdateUser(UpdateUserDTO dto)
        {
            var response = new Result();
            try
            {
                //get the updating user data. Need to compare if the username is still the same, if yes, pass validation
                var user = await _repo.GetFirstAsync<ExtendedIdentityUser>(
                    filter: c => c.Id == Convert.ToInt32(dto.Id));

                //if previous and new username are same, do not do username validation you know why :)
                bool doUsernameValidation = (dto.UserName == user.UserName) ? false : true;

                //do not do password validation because it is not included in user update
                bool doPasswordValidation = false;

                //if previous and new email are same, do not do email validation you know why :)
                bool doEmailValidation = (dto.Email == user.Email) ? false : true;

                var valResult = await _validationService.IsValidAccountInfo(username: dto.UserName, email: dto.Email, password: dto.Password, confirmPassword: dto.ConfirmPassword, doUsernameValidation: doUsernameValidation, doEmailValidation: doEmailValidation, doPasswordValidation: doPasswordValidation);
                if (!valResult.IsValid)
                {
                    response.Message = valResult.Message;
                    response.ErrorCode = ErrorCode.INVALID_INPUT;
                    return response;
                }

                if(string.IsNullOrEmpty(dto.DepartmentId) || string.IsNullOrEmpty(dto.Role) || !Enum.IsDefined(typeof(TypeOfNotification), (int)dto.TypeOfNotification))
                {
                    response.Message = valResult.Message;
                    response.ErrorCode = ErrorCode.INVALID_INPUT;
                    return response;
                }

                if (user == null)
                {
                    response.Message = "User doesn't exist.";
                    response.ErrorCode = ErrorCode.DATA_NOT_FOUND;
                    return response;
                }
                
                user.UserName = dto.UserName;
                user.FirstName = dto.FirstName;
                user.LastName = dto.LastName;
                user.Email = dto.Email;
                user.DepartmentId = dto.DepartmentId;
                user.TypeOfNotification = dto.TypeOfNotification;
                
                var result = await _userManager.UpdateAsync(user);

                //get the role if the user to be updated
                var userRole = await _userManager.GetRolesAsync(user);

                //if the previous role is not equal to new role
                if(dto.Role != null && dto.Role != "" && dto.Role != userRole[0])
                {
                    //remove the previous role of the user
                    await _userManager.RemoveFromRoleAsync(user, userRole[0]);

                    //add the new role of the user
                    var x = await _userManager.AddToRoleAsync(user, dto.Role);
                }
                //UserRights
                if(dto.Permissions!=null)
                {
                    var modulePermission = JsonConvert.DeserializeObject<List<UserRightsDTO>>(dto.Permissions);
                      foreach (var fp in modulePermission)
                      {
                        //check if existing
                        Result uRightsresult = new Result();
                        // result = await _repo.GetFirstAsync(_mapper.Map<UserRights>(UserRightsDTO));

                        UserRights userRights = await _repo.GetFirstAsync<UserRights>(
                        filter: c => c.UserId == Convert.ToInt32(dto.Id) && c.ModuleId == Convert.ToInt32(fp.Id));
                        if (userRights != null)
                        {

                            // Id = userRights.Id,
                            userRights.UserId = Convert.ToInt32(userRights.UserId);
                            userRights.ModuleId = Convert.ToInt32(userRights.ModuleId);
                            userRights.View = fp.View;
                            userRights.Add = fp.Add;
                            userRights.Edit = fp.Edit;
                            userRights.Delete = fp.Delete;
                            _repo.Update(userRights);
                            await _repo.SaveAsync();
                        }
                        else
                        {
                            var rights = new UserRights
                            {
                                UserId = Convert.ToInt32(dto.Id),
                                ModuleId = Convert.ToInt32(fp.Id),
                                View = fp.View,
                                Add = fp.Add,
                                Edit = fp.Edit,
                                Delete = fp.Delete
                            };
                            _repo.Create(rights);
                            await _repo.SaveAsync();
                        }
                        
                    }
                    
                }

                if (result.Succeeded)
                {
                    response.Message = "User has been successfully updated.";
                    response.Success = true;
                    _logger.LogInformation("Company User has been updated {0}", user.UserName);
                }
                else
                {
                    response.ErrorCode = ErrorCode.EXCEPTION;
                    _logger.LogError("Error updating the Company User: {0}", result.Errors);
                }
            }

            catch (Exception e)
            {
                response.Success = false;
                response.Message = "Error while updating user";
                response.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error updating user: {0}", e.Message);
            }

            return response;
        }

        public Result DeleteUser(string id)
        {
            Result result = new Result();
            ExtendedIdentityUser user= _repo.GetById<ExtendedIdentityUser>(Convert.ToInt32(id));
            try
            {
                _repo.Delete(user);
                _repo.Save();
                result.Message = "User has been successfully deleted.";
                result.Success = true;
            }
            catch (Exception)
            {

                throw;
            }
            return result;
        }

        public async Task<Result> ChangePasswordUser(ChangePasswordUserDTO dto)
        {
            var response = new Result();
            try
            {
                //find user by given ID
                var user = await _userManager.FindByIdAsync(dto.Id);

                var valResult = await _validationService.IsPasswordValid(dto, user);
                if (!valResult.IsValid)
                {
                    response.Message = valResult.Message;
                    response.ErrorCode = ErrorCode.INVALID_INPUT;
                    return response;
                }
                else
                {
                    user.HasPasswordChanged = true;
                    //change user password
                    var result = await _userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);
                    var verify = false;
                    var hasSMTP = false;

                    if (result.Succeeded)
                    {
                        if (verify && hasSMTP)
                        {
                            //var msg = EmailNotification.ChangePassword(user.FirstName, user.Email);
                            //await _emailSender.SendEmailAsync(user.Email, "Your Password changed", msg);
                        }
                        response.Success = true;
                        response.Message = "Password has been successfully changed.";
                    }
                    else
                    {
                        response.ErrorCode = ErrorCode.DATA_NOT_FOUND;
                        response.Message = "Password is not correct.";
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error updating User: {0}", e.Message);
            }

            return response;
        }

        public async Task<Result> CreateField(AddFieldDTO dto, string userId)
        {
            var result = new Result();
            try
            {
                //TO DO: Do need to add Field Validation? 
                var y = dto.Type;
                if (dto.Classification == 0)
                {
                    result.ErrorCode = ErrorCode.INVALID_INPUT;
                    result.Message = "Please fill in the required field.";
                    result.Success = false;
                    return result;
                }

                //todo: rework validation, validate field name per sub module
                var valResult = await _validationService.IsValidField(dto.Name, false, 0);
                if (!valResult.IsValid)
                {
                    result.Message = valResult.Message;
                    result.ErrorCode = ErrorCode.INVALID_INPUT;
                    return result;
                }
                var field = new Field
                {
                    Name = dto.Name,
                    Purpose = dto.Purpose ?? "",
                    Type = FieldType.TextField,
                    IsDefault = false,
                    IsLifeCycle = false,
                    Classification = dto.Classification,
                    IsRequired = false,
                    CreatedDate = DateTime.Now,
                    CreatedBy = userId
                };
                _repo.Create(field);
                await _repo.SaveAsync();

                result.Id = field.Id.ToString();
                result.Message = "Field has been successfully Added.";
                result.Success = true;
            }
            catch (Exception e)
            {
                result.Message = "Error adding field";
                result.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error calling CreateField: {0}", e.Message);
            }

            return result;
        }

        public async Task<Result> UpdateField(UpdateFieldDTO dto)
        {
            var result = new Result();
            try
            {
                var field = _repo.GetById<Field>(Convert.ToInt32(dto.Id));
                if(field.Name != dto.Name)
                {
                    var validateResult = await _validationService.IsValidField(dto.Name, false, 0);
                    if (!validateResult.IsValid)
                    {
                        result.Message = validateResult.Message;
                        result.ErrorCode = ErrorCode.INVALID_INPUT;
                        return result;
                    }
                }

                if (field == null)
                {
                    result.Message = "Field does not exist.";
                    result.ErrorCode = ErrorCode.DATA_NOT_FOUND;
                    return result;
                }

                field.Name = dto.Name;
                field.Purpose = dto.Purpose ?? "";
                field.Type = FieldType.TextField;
                field.IsRequired = false;
                field.Classification = dto.Classification;

                _repo.Update(field);
                await _repo.SaveAsync();
                result.Message = "Field has been successfully updated.";
                result.Success = true;
            }
            catch (Exception e)
            {
                result.Message = "Error adding field.";
                result.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error calling UpdateField: {0}", e.Message);
            }

            return result;
        }

        public Result DeleteField(string id)
        {
            Result result = new Result();
            Field field = _repo.GetById<Field>(Convert.ToInt32(id));
            try
            {
                _repo.Delete(field);
                _repo.Save();
                result.Message = "Field has been successfully deleted.";
                result.Success = true;
            }
            catch (Exception e)
            {
                result.Message = "Error deleting field.";
                result.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error calling DeleteField: {0}", e.Message);
            }
            return result;
        }

        public async Task<Result> CreateFieldItem(AddFieldItemDTO dto)
        {
            Result result = new Result();
            try
            {
                string itemName = dto.Name;
                ValidationResult valResult = await _validationService.IsValidFieldItemName(dto.FieldId.ToString(), itemName);
                if(!valResult.IsValid)
                {
                    result.Message = valResult.Message;
                    result.ErrorCode = ErrorCode.INVALID_INPUT;
                    return result;
                }
                FieldItem field = new FieldItem
                {
                    FieldId = dto.FieldId,
                    Name = dto.Name,
                    Description = dto.Description
                };
                _repo.Create(field);
                await _repo.SaveAsync();
                result.Message = "Field item has been successfully added.";
                result.Success = true;
                result.Id = "FieldItem";

            }
            catch (Exception e)
            {
                result.Message = "Error adding field item.";
                result.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error calling CreateFieldItem: {0}", e.Message);
            }
            return result;
        }

        public async Task<Result> UpdateFieldItem(UpdateFieldItemDTO dto)
        {
            Result result = new Result();
            try
            {
                //get the data filter by id
                FieldItem fieldItem = _repo.GetById<FieldItem>(dto.Id);
                if(fieldItem.Name != dto.Name)
                {
                    string itemName = dto.Name;
                    ValidationResult valResult = await _validationService.IsValidFieldItemName(dto.FieldId.ToString(), itemName);
                    if (!valResult.IsValid)
                    {
                        result.Message = valResult.Message;
                        result.ErrorCode = ErrorCode.INVALID_INPUT;
                        return result;
                    }
                }

                //update the data
                fieldItem.Name = dto.Name;
                fieldItem.Description = dto.Description;

                //save the updated data
                _repo.Update(fieldItem);
                await _repo.SaveAsync();
                result.Message = "Field item has been successfully updated.";
                result.Success = true;
            }
            catch (Exception e)
            {
                result.Message = "Error updating field item.";
                result.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error calling UpdateFieldItem: {0}", e.Message);
            }
            return result;
        }

        public Result DeleteFieldItem(string id)
        {
            Result result = new Result();
            FieldItem field = _repo.GetById<FieldItem>(Convert.ToInt32(id));
            try
            {
                _repo.Delete(field);
                _repo.Save();
                result.Message = "Field item has been successfully deleted.";
                result.Success = true;
            }
            catch (Exception e)
            {
                result.Message = "Error deleted field item.";
                result.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error calling DeleteFieldItem: {0}", e.Message);
            }
            return result;
        }

        public async Task<Result> AddSubModuleField(AddSubModuleFieldDTO dto)
        {
            Result result = new Result();
            try
            {
                SubModuleField field = new SubModuleField
                {
                    FieldId = dto.FieldId,
                    SubModuleId = dto.SubModuleId,
                    Order = dto.Order
                };

                _repo.Create(field);
                await _repo.SaveAsync();
                result.Message = "Field has been successfully added.";
                result.Success = true;
            }
            catch (Exception e)
            {
                result.Message = "Error adding a submodule field.";
                result.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error calling AddSubModuleField: {0}", e.Message);
            }
            return result;
        }

        public Result BulkDeleteField(List<string> id)
        {
            int count = 0;
            Result result = new Result();
            foreach (string tempId in id)
            {
                Field field = _repo.GetById<Field>(Convert.ToInt32(tempId));
                try
                {
                    _repo.Delete(field);
                    _repo.Save();
                    count++;
                    result.Message = count.ToString() + " " + (count > 1 ? "fields" : "field") + " " + (count > 1 ? "have" : "has") + " been successfully deleted.";
                    result.Success = true;
                }
                catch (Exception e)
                {
                    result.Message = "Error deleting field.";
                    result.ErrorCode = ErrorCode.EXCEPTION;
                    _logger.LogError("Error calling DeleteField: {0}", e.Message);
                }
            }

            return result;
        }

        public Result BulkDeleteUser(List<string> id)
        {
            int count = 0;
            Result result = new Result();
            foreach (string tempId in id)
            {
                ExtendedIdentityUser user = _repo.GetById<ExtendedIdentityUser>(Convert.ToInt32(tempId));
                try
                {
                    _repo.Delete(user);
                    _repo.Save();
                    count++;
                    result.Message = count.ToString() + " " + (count > 1 ? "users" : "user") + " " + (count > 1 ? "have" : "has") + " been successfully deleted.";
                    result.Success = true;
                }
                catch (Exception e)
                {
                    result.Message = "Error deleting user.";
                    result.ErrorCode = ErrorCode.EXCEPTION;
                    _logger.LogError("Error calling BulkDeleteUser: {0}", e.Message);
                }
            }

            return result;
        }
        
        public async Task<Result> CreateLifeCycleField(AddFieldDTO dto, string userId)
        {
            var result = new Result();
            try
            {
                //TO DO: Do need to add Field Validation? 
                var y = dto.Type;
                if (dto.Type == 0 || dto.LifeCycle == 0)
                {
                    result.ErrorCode = ErrorCode.INVALID_INPUT;
                    result.Message = "Please fill in the required fields.";
                    result.Success = false;
                    return result;
                }

                var valResult = await _validationService.IsValidField(dto.Name, true, (int)dto.LifeCycle + 1);
                if (!valResult.IsValid)
                {
                    result.Message = valResult.Message;
                    result.ErrorCode = ErrorCode.INVALID_INPUT;
                    return result;
                }
                var field = new Field
                {
                    Name = dto.Name,
                    Purpose = dto.Purpose ?? "",
                    Type = dto.Type,
                    IsDefault = false,
                    IsLifeCycle = true,
                    Classification = dto.Classification,
                    IsRequired = false,
                    CreatedDate = DateTime.Now,
                    CreatedBy = userId
                };
                _repo.Create(field);
                await _repo.SaveAsync();

                result.Id = field.Id.ToString();
                result.Message = "Field has been successfully Added.";
                result.Success = true;
            }
            catch (Exception e)
            {
                result.Message = "Error adding field";
                result.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error calling CreateField: {0}", e.Message);
            }

            return result;
        }
        
        public async Task<Result> UpdateSubModuleField(UpdateFieldDTO dto)
        {
            var result = new Result();
            try
            {
                var subModuleField = await _repo.GetFirstAsync<SubModuleField>(
                    filter: f => f.FieldId == Convert.ToInt32(dto.Id));

                //todo: hard coded, rework
                subModuleField.SubModuleId = Convert.ToInt32(dto.LifeCycle) + 1;
                //todo: validation

                _repo.Update(subModuleField);
                await _repo.SaveAsync();
                result.Message = "Field has been successfully updated.";
                result.Success = true;
            }
            catch (Exception e)
            {
                result.Message = "Error adding field.";
                result.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error calling UpdateField: {0}", e.Message);
            }

            return result;
        }

        public async Task<Result> UpdateDepartment(UpdateDepartmentDTO dto)
        {
            var result = new Result();
            try
            {
                var department = _repo.GetById<Department>(Convert.ToInt32(dto.Id));
                //check if department new and updated name are the same
                if (department.Name != dto.Name)
                {
                    //check if department name is valid
                    var validateResult = await _validationService.IsValidDepartmentName(dto.Name);
                    if (!validateResult.IsValid)
                    {
                        result.Message = validateResult.Message;
                        result.ErrorCode = ErrorCode.INVALID_INPUT;
                        return result;
                    }
                }

                //check if department was deleted
                if (department == null)
                {
                    result.Message = "Department does not exist.";
                    result.ErrorCode = ErrorCode.DATA_NOT_FOUND;
                    return result;
                }

                department.Name = dto.Name;
                department.Description = dto.Description ?? "";

                _repo.Update(department);
                await _repo.SaveAsync();
                result.Message = "Department has been successfully updated.";
                result.Success = true;
            }
            catch (Exception e)
            {
                result.Message = "Error updating department.";
                result.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error calling UpdateDepartment: {0}", e.Message);
            }

            return result;
        }

        public async Task<Result> CreateDepartment(AddDepartmentDTO dto, string userId)
        {
            var result = new Result();
            try
            {
                var valResult = await _validationService.IsValidDepartmentName(dto.Name);
                if (!valResult.IsValid)
                {
                    result.Message = valResult.Message;
                    result.ErrorCode = ErrorCode.INVALID_INPUT;
                    return result;
                }
                var department = new Department
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    Status = true,

                    //required
                    CreatedDate = DateTime.Now,
                    CreatedBy = userId
                };
                _repo.Create(department);
                await _repo.SaveAsync();
                
                result.Message = "Department has been successfully Added.";
                result.Success = true;
            }
            catch (Exception e)
            {
                result.Message = "Error adding department";
                result.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error calling CreateDepartment: {0}", e.Message);
            }

            return result;
        }

        public Result DeleteDepartment(string id)
        {
            Result result = new Result();
            Department department = _repo.GetById<Department>(Convert.ToInt32(id));
            try
            {
                _repo.Delete(department);
                _repo.Save();
                result.Message = "Department has been successfully deleted.";
                result.Success = true;
            }
            catch (Exception e)
            {
                result.Message = "Error deleting department.";
                result.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error calling DeleteDepartment: {0}", e.Message);
            }
            return result;
        }

        public Result BulkDeleteDepartment(List<string> id)
        {
            int count = 0;
            Result result = new Result();
            foreach (string tempId in id)
            {
                Department department = _repo.GetById<Department>(Convert.ToInt32(tempId));
                try
                {
                    _repo.Delete(department);
                    _repo.Save();
                    count++;
                    result.Message = count.ToString() + " " + (count > 1 ? "departments" : "department") + " " + (count > 1 ? "have" : "has") + " been successfully deleted.";
                    result.Success = true;
                }
                catch (Exception e)
                {
                    result.Message = "Error deleting department.";
                    result.ErrorCode = ErrorCode.EXCEPTION;
                    _logger.LogError("Error calling BulkDeleteDepartment: {0}", e.Message);
                }
            }

            return result;
        }

        public async Task<Result> AddDataset(AddDatasetDTO dto, string userId)
        {
            Result result = new Result();
            try
            {
                ValidationResult valResult = await _validationService.IsValidDataset(dto.Name);
                if (!valResult.IsValid)
                {
                    result.Success = false;
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
                return result;
            }
            catch (Exception e)
            {
                result.Message = "Error adding dataset.";
                result.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error calling AddDataset: {0}", e.Message);
            }

            return result;
        }

        public async Task<Result> UpdateDataset(UpdateDatasetDTO dto)
        {
            Result result = new Result();
            try
            {
                var dataset = await _repo.GetByIdAsync<Dataset>(Convert.ToInt32(dto.Id));
                if (dataset.Name != dto.Name)
                {
                    ValidationResult valResult = await _validationService.IsValidDataset(dto.Name);
                    if (!valResult.IsValid)
                    {
                        result.Success = false;
                        result.Message = valResult.Message;
                        return result;
                    }
                }

                dataset.Name = dto.Name;
                dataset.Description = dto.Description;

                _repo.Update(dataset);
                await _repo.SaveAsync();

                result.Success = true;
                result.Message = "Dataset has been successfully updated.";
                return result;
            }
            catch (Exception e)
            {
                result.Message = "Error updating Dataset.";
                result.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error calling UpdateDataset: {0}", e.Message);
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

                result.Success = true;
                result.Message = "Dataset has been successfully deleted.";
                return result;
            }
            catch (Exception e)
            {
                result.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error calling DeleteDataset: {0}", e.Message);

                result.Success = false;
                result.Message = "Error on deleting data set.";
                return result;
            }
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

        public Result BulkDeleteDataset(List<string> id)
        {
            int count = 0;
            Result result = new Result();
            try
            {
                foreach (string tempId in id)
                {
                    Dataset dataset = _repo.GetById<Dataset>(Convert.ToInt32(tempId));

                    _repo.Delete(dataset);
                    count++;
                }
                _repo.Save();

                result.Success = true;
                result.Message = count.ToString() + " " + (count > 1 ? "datasets" : "dataset") + " " + (count > 1 ? "have" : "has") + " been successfully deleted.";
            }
            catch (Exception e)
            {
                result.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error calling BulkDeleteDataset: {0}", e.Message);

                result.Success = false;
                result.Message = "Error deleting dataset.";
                return result;
            }
            return result;
        }

        public async Task<Result> UpdateLifeCycleField(UpdateFieldDTO dto)
        {
            var result = new Result();
            try
            {
                var field = _repo.GetById<Field>(Convert.ToInt32(dto.Id));
                if (field.Name != dto.Name)
                {
                    var validateResult = await _validationService.IsValidField(dto.Name, true, (int)dto.LifeCycle + 1);
                    if (!validateResult.IsValid)
                    {
                        result.Message = validateResult.Message;
                        result.ErrorCode = ErrorCode.INVALID_INPUT;
                        return result;
                    }
                }

                var submoduleFields = (await _repo.GetAsync<SubModuleField>(
                    filter: f => f.SubModuleId == (int)dto.LifeCycle + 1,
                    include: i => i.Include(ii => ii.Field)
                    )).ToList();
                submoduleFields = submoduleFields.Where(w => w.FieldId != Convert.ToInt32(dto.Id)).ToList();
                if (submoduleFields.Where(w => w.Field.Name == dto.Name).Select(s => s.Field.Name).FirstOrDefault() == dto.Name)
                {
                    result.Message = "The field name is not available.";
                    result.ErrorCode = ErrorCode.INVALID_INPUT;
                    return result;
                }

                if (field == null)
                {
                    result.Message = "Field does not exist.";
                    result.ErrorCode = ErrorCode.DATA_NOT_FOUND;
                    return result;
                }

                field.Name = dto.Name;
                field.Purpose = dto.Purpose ?? "";
                field.Type = dto.Type;
                field.IsRequired = false;
                field.Classification = dto.Classification;

                _repo.Update(field);
                await _repo.SaveAsync();
                result.Message = "Field has been successfully updated.";
                result.Success = true;
            }
            catch (Exception e)
            {
                result.Message = "Error adding field.";
                result.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error calling UpdateField: {0}", e.Message);
            }

            return result;
        }

        public async Task<Result> ChangeCompanyName(UpdateCompanyInfoDTO dto, string userId)
        {
            var result = new Result();
            try
            {
                //find user by given ID
                var user = await _userManager.FindByIdAsync(userId);
                var passwordIsValid = await _userManager.CheckPasswordAsync(user, dto.Password);
                if(!passwordIsValid)
                {
                    result.Success = false;
                    result.Message = "Invalid password.";
                    return result;
                }

                var company = await _repo.GetFirstAsync<Company>();
                if(company == null)
                {
                    var newCompany = new Company
                    {
                        Name = dto.Name,
                        ContactNo = "",
                        Email = "",
                        Description = ""
                    };

                    _repo.Create(newCompany);
                    await _repo.SaveAsync();

                    result.Message = "Company information has been successfully updated.";
                    result.Success = true;
                }
                else if (company != null)
                {
                    company.Name = dto.Name;

                    _repo.Update(company);
                    await _repo.SaveAsync();
                    result.Message = "Company information has been successfully updated.";
                    result.Success = true;
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error updating User: {0}", e.Message);
            }
            return result;
        }

        public async Task<Result> SetUp(string userId)
        {
            Result response = new Result();
            try
            {
                var user = await _repo.GetFirstAsync<ExtendedIdentityUser>(
                filter: c => c.Id == Convert.ToInt32(userId));
                user.DoneSetUp = 1;
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    response.Message = "Sucessfully update user setup.";
                    response.Success = true;
                    _logger.LogInformation("User setup. {0}", user.UserName);
                }
                else
                {
                    response.ErrorCode = ErrorCode.EXCEPTION;
                    _logger.LogError("Error updating user setUp: {0}", result.Errors);
                }
            }

            catch (Exception e)
            {
                response.Success = false;
                response.Message = "Error while updating user setUp";
                response.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error updating user setUp: {0}", e.Message);
            }

            return response;
        }

    }
}