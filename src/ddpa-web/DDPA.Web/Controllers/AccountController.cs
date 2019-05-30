using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DDPA.SQL.Entities;
using DDPA.Service;
using DDPA.Attributes;
using DDPA.DTO;
using static DDPA.Commons.Enums.DDPAEnums;
using AutoMapper;
using DDPA.Web.Extensions;
using DDPA.Web.Models;
using Microsoft.AspNetCore.Http;
using DDPA.Extensions;
using DDPA.Commons.Helper;
using DDPA.Commons.Results;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Localization;
using DDPA.Resources;

namespace DDPA.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ExtendedIdentityUser> _signInManager;
        private readonly UserManager<ExtendedIdentityUser> _userManager;
        private readonly ILogger _logger;
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;
        private readonly IQueryService _queryService;
        private readonly IHtmlLocalizer<SharedResource> _sharedLocalizer;

        public AccountController(SignInManager<ExtendedIdentityUser> signInManager, UserManager<ExtendedIdentityUser> userManager, 
            ILogger<AccountController> logger, IAccountService accountService, IQueryService queryService, IHtmlLocalizer<SharedResource> sharedLocalizer)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _accountService = accountService;
            _queryService = queryService;
            _mapper = this.GetMapper();
            _sharedLocalizer = sharedLocalizer;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            //If a user already logged in, redirect to Dashboard page
            if (HttpContext.Session.GetString(SessionHelper.USER_NAME) != null)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }
        
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            Result result = new Result();
            try
            {
                //if the session USER_NAME is not null, it means that a user is currently logged in
                //redirect the user to Dashboard page
                if (HttpContext.Session.GetString(SessionHelper.USER_NAME) != null)
                {
                    return RedirectToAction("Index", "Dashboard");
                }

                ViewData["ReturnUrl"] = returnUrl;


                if (string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.Password))
                {
                    ModelState.AddModelError(string.Empty, "Invalid Credentials");
                    result.Message = "Invalid username or password.";
                    result.Success = false;
                    return Json(result);
                }

                if (ModelState.IsValid)
                {
                    var user = await _userManager.FindByNameAsync(model.UserName);
                    if (user != null)
                    {
                        var tempRole = await _userManager.GetRolesAsync(user);
                        ViewBag.userName = user.UserName;
                        var user2 = await _userManager.GetUserAsync(User);
                        if (!await _userManager.IsEmailConfirmedAsync(user))
                        {
                            // TODO: create a separate action for resending the confirmation token
                            // string callbackUrl = await SendEmailConfirmationTokenAsync(user.Id, "Confirm your account-Resend");

                            // Uncomment to debug locally  
                            // ViewBag.Link = callbackUrl;
                            // ViewBag.errorMessage = "You must have a confirmed email to log on. "
                            //                     + "The confirmation token has been resent to your email account.";

                            ModelState.AddModelError(string.Empty, "You must have a confirmed email to log on.");
                            return View();
                        }
                        else
                        {
                            bool isSucceeded = false;

                            // This doesn't count login failures towards account lockout
                            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                            var signIn = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
                            isSucceeded = signIn.Succeeded;

                            if (isSucceeded)
                            {
                                var role = await _userManager.GetRolesAsync(user);
                                var modules = await _queryService.GetModules(role.First());
                                var userRole = role.First();

                                //if DPO or ADMIN is logged, add a 0 value to userDept.
                                string userDept = user.DepartmentId ?? "0";

                                string companyName = (await _queryService.GetCompanyInfo() == null) ? "" : (await _queryService.GetCompanyInfo()).Name ?? "";

                                //store in session the neccessary data
                                HttpContext.Session.SetString(SessionHelper.COMPANY_NAME, companyName);
                                HttpContext.Session.SetString(SessionHelper.USER_NAME, user.UserName);
                                HttpContext.Session.SetString(SessionHelper.USER_ID, user.Id.ToString());
                                HttpContext.Session.SetObjectAsJson(SessionHelper.USER, user);
                                HttpContext.Session.SetString(SessionHelper.USER_NAME, user.UserName);
                                HttpContext.Session.SetString(SessionHelper.ROLES, userRole);
                                HttpContext.Session.SetString(SessionHelper.USER_DEPT, userDept);
                                HttpContext.Session.SetObjectAsJson(SessionHelper.MODULES, modules);
                                HttpContext.Session.SetString(SessionHelper.DONE_SETUP, user.DoneSetUp.ToString());


                                //this session will be used for redirecting of the user, specifically for DPO and ADMIN.
                                if (tempRole.First() == "ADMINISTRATOR" || tempRole.First() == "DPO")
                                {
                                    if (user.HasPasswordChanged == false)
                                    {
                                        HttpContext.Session.SetString(SessionHelper.HASPASSWORDCHANGED, "0");
                                    }
                                    else if (user.HasPasswordChanged == true)
                                    {
                                        HttpContext.Session.SetString(SessionHelper.HASPASSWORDCHANGED, "1");
                                    }
                                    //showmodal for user setup if DoneSetup=0
                                    //ShowModal for Userguide if DoneSetup =1
                                    if ((user.DoneSetUp == 0) || (user.DoneSetUp == 1))
                                    {
                                        HttpContext.Session.SetString(SessionHelper.SHOW_MODAL, "1");
                                    }
                                 }
                                else if (tempRole.First() == "USER" || tempRole.First() == "DEPTHEAD")
                                {
                                    //showModal for userguide in Dashboard
                                    if (user.DoneSetUp == 0)
                                    {
                                        HttpContext.Session.SetString(SessionHelper.DONE_SETUP, "1");
                                        HttpContext.Session.SetString(SessionHelper.SHOW_MODAL, "1");
                                    }
                                }
                                //Update user DoneSetup to 2 to disable force setup and userguide note
                                if (HttpContext.Session.GetString(SessionHelper.DONE_SETUP) == "1")
                                {
                                    user.DoneSetUp = 2;
                                    var updateUserSetup = await _userManager.UpdateAsync(user);
                                }

                                // userRights
                                var moduleRights = await _queryService.GetModules("ADMINISTRATOR");
                                List<UserRightsViewModel> rights = new List<UserRightsViewModel>();

                                // get the user rights per module.
                                // note: DPO and ADMIN rights can not be edited.
                                // also DPO and ADMIN are the only one who can edit other's(DEPTHEAD, and USER) rights.
                                foreach (var item in moduleRights)
                                {
                                    var ur = await _queryService.GetUserRights(user.Id.ToString(), item.Id);

                                    var ur_model = new UserRightsViewModel();
                                    ur_model.ModuleId = item.Id;
                                    ur_model.ModuleName = item.Name;
                                    if (ur != null)
                                    {
                                        ur_model.View = ur.View ? 1 : 0;
                                        ur_model.Add = ur.Add ? 1 : 0;
                                        ur_model.Edit = ur.Edit ? 1 : 0;
                                    }
                                    else
                                    {
                                        ur_model.View = 0;
                                        ur_model.Add = 0;
                                        ur_model.Edit = 0;
                                    }
                                    rights.Add(ur_model);
                                }
                                ViewBag.rightsList = rights;

                                HttpContext.Session.SetObjectAsJson(SessionHelper.USER_RIGHTS, rights);

                                result.Message = "Logged in successfully.";
                                result.Success = true;
                                result.IsRedirect = true;
                                result.RedirectUrl = "Dashboard/Index";
                                return Json(result);
                            }

                            else if (!isSucceeded)
                            {
                                result.Message = "Invalid username or password.";
                                result.Success = false;
                                return Json(result);
                            }
                        }
                    }
                    else
                    {
                        result.Message = "Invalid username or password.";
                        result.Success = false;
                        return Json(result);
                    }
                }

                // if program reached here, something failed, redisplay form
                return View();
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling Login: {0}", e.Message);
                throw;
            }            
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        private bool IsSignInUser()
        {
            return _signInManager.IsSignedIn(HttpContext.User);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LogOff()
        {
            HttpContext.Session.Clear();

            _signInManager.SignOutAsync();
            _logger.LogInformation(4, "User logged out.");
            return RedirectToAction(nameof(AccountController.Login), "Account");
        }
        
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            try
            {
                List<DepartmentDTO> departmentList = await _queryService.GetDepartments();
                ViewData["departmentList"] = departmentList;
                return View();
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> Register(AddUserViewModel model)
        {
            Result result = new Result();
            try
            {
                string userRole = HttpContext.Session.GetString(SessionHelper.ROLES) ?? "";
                string userId = HttpContext.Session.GetString(SessionHelper.USER_ID) ?? "";
                model.Role = Role.USER.ToString();
                var user = await _userManager.GetUserAsync(HttpContext.User);

                AddUserDTO dto = _mapper.Map<AddUserDTO>(model);
                if (user == null || user.Id.ToString() == null || user.Id.ToString() == "")
                {
                    var adminUser = await _userManager.FindByNameAsync("superadmin");
                    dto.CreatedBy = userId;
                }
                else
                {
                    dto.CreatedBy = userId;
                }

                result = await _accountService.CreateUser(dto);
            }
            catch (Exception e)
            {
                _logger.LogError("Error Exception on Register: " + e.Message.ToString());
            }

            return Json(result);
        }
    }
}
