using Microsoft.Extensions.Logging;
using DDPA.Commons.Results;
using System.Threading.Tasks;
using DDPA.DTO;
using DDPA.SQL.Entities;
using DDPA.SQL.Repositories;
using System;
using static DDPA.Commons.Enums.DDPAEnums;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;

namespace DDPA.Service
{
    public class ValidationService : IValidationService
    {
        private readonly UserManager<ExtendedIdentityUser> _userManager;
        private readonly IRepository _repo;

        public ValidationService(UserManager<ExtendedIdentityUser> userManager, IRepository repo)
        {
            _userManager = userManager;
            _repo = repo;
        }

        public async Task<ValidationResult> IsValidAccountInfo(string username, string email, string password, string confirmPassword, bool doUsernameValidation = true, bool doEmailValidation = true, bool doPasswordValidation = true)
        {
            var result = new ValidationResult();
            string x = email;

            if (doEmailValidation && !String.IsNullOrEmpty(email))
            {
                //MailAddress 
                if (doEmailValidation && !String.IsNullOrEmpty(email) && await _userManager.FindByEmailAsync(email) != null)
                {
                    result.Message = "The email is not available.";
                    return result;
                }
            }
            

            if(doUsernameValidation && String.IsNullOrEmpty(username))
            {
                result.Message = "The username is required.";
                return result;
            }
            else if (doUsernameValidation && await _userManager.FindByNameAsync(username) != null)
            {
                result.Message = "The username is not available.";
                return result;
            }

            if (doPasswordValidation && password != confirmPassword)
            {
                result.Message = "Password did not match.";
                return result;
            }

            else if (doPasswordValidation && !Commons.Helper.Validation.IsValidPassword(password))
            {
                result.Message = "Invalid password. Password must be at least 8 characters long, contain a number and an uppercase letter.";
                return result;
            }

            // Its valid if reached here
            result.IsValid = true;
            return result;
        }

        public async Task<ValidationResult> IsValidField(string name, bool isLifeCycle, int subModuleID)
        {
            var result = new ValidationResult();

            if (isLifeCycle == true)
            {
                //get all fields with the same name
                var listOfFieldWithSameName = await _repo.GetAsync<Field>(
                    filter: f => f.Name == name
                    );

                bool foundSameSubModuleField = false;

                //loop through each fields
                foreach (Field field in listOfFieldWithSameName.ToList())
                {
                    //using field's id and submoduleid, look for duplicate
                    var tempField = await _repo.GetAsync<SubModuleField>(
                            filter: f => f.FieldId == field.Id && f.SubModuleId == subModuleID);
                    if(tempField.ToList().Count > 0)
                    {
                        foundSameSubModuleField = true;
                    }
                }
                if (foundSameSubModuleField)
                {
                    result.Message = "The field name is not available.";
                    return result;
                }
            }

            else if (isLifeCycle == false)
            {
                if (!String.IsNullOrEmpty(name) && await _repo.GetFirstAsync<Field>(
                    filter: f => f.Name == name && f.IsLifeCycle == false) != null)
                {
                    result.Message = "The field name is not available.";
                    return result;
                }
            }

            // Its valid if reached here
            result.IsValid = true;
            return result;
        }

        public async Task<ValidationResult> IsValidFieldItemName(string fieldId, string name)
        {
            ValidationResult result = new ValidationResult();
            if (!String.IsNullOrEmpty(name) && await _repo.GetFirstAsync<FieldItem>(filter: f => f.Name == name && f.FieldId == Convert.ToInt32(fieldId)) != null)
            {
                result.Message = "The item name is not available.";
                return result;
            }

            // Its valid if reached here
            result.IsValid = true;
            return result;
        }

        public async Task<ValidationResult> IsPasswordValid(ChangePasswordUserDTO dto, ExtendedIdentityUser user)
        {
            ValidationResult result = new ValidationResult();

            if (dto.NewPassword != dto.ConfirmPassword)
            {
                result.Message = "New and confirm password did not match.";
                return result;
            }
            else if (!Commons.Helper.Validation.IsValidPassword(dto.NewPassword))
            {
                result.Message = "Invalid password. Password must be at least 8 characters long, contain a number and an uppercase letter.";
                return result;
            }

            //see if user filled old password is same as current password
            var hasSameOldPassword = await _userManager.CheckPasswordAsync(user, dto.OldPassword);

            //if filled old pass and current pass did not match
            if (!hasSameOldPassword)
            {
                result.Message = "Invalid old password.";
                return result;
            }

            else if(hasSameOldPassword && dto.OldPassword == dto.NewPassword)
            {
                result.Message = "New password must be different.";
                return result;
            }

            // Its valid if reached here
            result.IsValid = true;
            return result;
        }

        public async Task<ValidationResult> IsValidDataset(string name)
        {
            var result = new ValidationResult();

            if (String.IsNullOrEmpty(name))
            {
                result.Message = "The dataset name is required.";
                return result;
            }

            //for blank dataset
            if (name.ToUpper() == "BLANK DATASET" || name.ToUpper() == "BLANK")
            {
                result.Message = "The dataset name is not available.";
                return result;
            }

            //if the dataset name already exist
            if (!String.IsNullOrEmpty(name) && await _repo.GetFirstAsync<Dataset>(filter: f => f.Name == name) != null)
            {
                result.Message = "The dataset name is not available.";
                return result;
            }

            // Its valid if reached here
            result.IsValid = true;
            return result;
        }

        public async Task<ValidationResult> IsDataNumberExist(string dataNumber)
        {
            var result = new ValidationResult();
            var doc = await _repo.GetAsync<Document>(filter: f => f.DataNumber.ToUpper().TrimStart().TrimEnd() == dataNumber.ToUpper().TrimStart().TrimEnd());
            if (!String.IsNullOrEmpty(dataNumber) && doc.Count() > 0)
            {
                result.Message = "The data number already exist";
                return result;
            }

            // Its valid if reached here
            result.IsValid = true;
            return result;
        }

        public async Task<ValidationResult> IsDataNumberExist(string dataNumber, int id)
        {
            var result = new ValidationResult();
            var doc = await _repo.GetAsync<Document>(filter: f => f.Id != id  && f.DataNumber.ToUpper().TrimStart().TrimEnd() == dataNumber.ToUpper().TrimStart().TrimEnd());
            if (!String.IsNullOrEmpty(dataNumber) && doc.Count() > 0)
            {
                result.Message = "The data number already exist";
                return result;
            }

            // Its valid if reached here
            result.IsValid = true;
            return result;
        }

        public async Task<ValidationResult> FieldItemExist(string fieldId, string name)
        {
            ValidationResult result = new ValidationResult();
            if (!String.IsNullOrEmpty(name) && await _repo.GetFirstAsync<FieldItem>(filter: f => f.Name == name && f.FieldId == Convert.ToInt32(fieldId)) != null)
            {
                result.IsValid = true;
                return result;
            }

            // Item does not exist of reached here
            result.IsValid = false;
            result.Message = "item does not exist";
            return result;
        }

        public async Task<ValidationResult> IsValidDepartmentName(string name)
        {
            ValidationResult result = new ValidationResult();
            if (!string.IsNullOrEmpty(name))
            {
                var tempDepartment = await _repo.GetFirstAsync<Department>(filter: f => f.Name == name);
                //if name exist
                if (tempDepartment != null)
                {
                    //if name exist, and status is 1
                    if(tempDepartment.Status == true)
                    {
                        result.IsValid = false;
                        result.Message = "The department name is not available.";
                        return result;
                    }

                    //if name exist, and status is 0
                    else if (tempDepartment.Status == false)
                    {
                        result.IsValid = false;
                        result.Message = "The department name exist but disabled.";
                        return result;
                    }
                }

                else if (tempDepartment == null)
                {
                    result.IsValid = true;
                    return result;
                }
            }

            if (!string.IsNullOrEmpty(name) && await _repo.GetFirstAsync<Department>(filter: f => f.Name == name && f.Name == name) == null)
            {
                result.IsValid = true;
                return result;
            }

            // Item does not exist of reached here
            result.IsValid = false;
            result.Message = "The department name is not available.";
            return result;
        }
    }
}