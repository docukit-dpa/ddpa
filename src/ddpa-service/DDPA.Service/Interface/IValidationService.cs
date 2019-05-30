using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DDPA.SQL.Entities;
using DDPA.Commons.Results;
using DDPA.DTO;

namespace DDPA.Service
{
    public interface IValidationService
    {
        Task<ValidationResult> IsValidAccountInfo(string username, string email, string password, string confirmPassword, bool doUsernameValidation = true, bool doEmailValidation = true, bool doPasswordValidation = true);

        Task<ValidationResult> IsValidField(string name, bool isLifeCycle, int subModuleID);

        Task<ValidationResult> IsValidFieldItemName(string fieldId, string name);

        Task<ValidationResult> IsPasswordValid(ChangePasswordUserDTO dto, ExtendedIdentityUser user);

        Task<ValidationResult> IsValidDataset(string name);

        Task<ValidationResult> IsDataNumberExist(string dataNumber);

        Task<ValidationResult> IsDataNumberExist(string dataNumber, int id);

        Task<ValidationResult> FieldItemExist(string fieldId, string name);

        Task<ValidationResult> IsValidDepartmentName(string name);
    }
}