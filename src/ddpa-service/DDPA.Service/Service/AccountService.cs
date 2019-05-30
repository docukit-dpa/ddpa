using Microsoft.Extensions.Logging;
using DDPA.Commons.Results;
using System.Threading.Tasks;
using DDPA.DTO;
using DDPA.SQL.Entities;
using DDPA.SQL.Repositories;
using System;
using static DDPA.Commons.Enums.DDPAEnums;
using Microsoft.AspNetCore.Identity;

namespace DDPA.Service
{
    public class AccountService :  IAccountService
    {
        protected readonly ILogger _logger;
        protected readonly IRepository _repo;
        protected readonly UserManager<ExtendedIdentityUser> _userManager;
        protected readonly IValidationService _validationService;

        public AccountService(ILogger<AccountService> logger, IRepository repo, UserManager<ExtendedIdentityUser> userManager, IValidationService validationService) 
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

                if (String.IsNullOrEmpty(dto.DepartmentId))
                {
                    response.Message = "Please fill in the required fields.";
                    response.ErrorCode = ErrorCode.INVALID_INPUT;
                    return response;
                }

                if (Enum.IsDefined(typeof(TypeOfNotification), dto.TypeOfNotification))
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
                    TypeOfNotification = dto.TypeOfNotification
                };

                var result = await _userManager.CreateAsync(user, dto.Password);
                if (result.Succeeded)
                {
                    var addrole = await _userManager.AddToRoleAsync(user, dto.Role);

                    response.Success = true;
                    response.Message = "User has been successfully added.";
                    response.ErrorCode = ErrorCode.DEFAULT;
                }
            }
            catch (Exception e)
            {
                response.Message = "Error adding user";
                response.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error calling CreateUser: {0}", e.Message);
            }

            return response;
        }
    }
}