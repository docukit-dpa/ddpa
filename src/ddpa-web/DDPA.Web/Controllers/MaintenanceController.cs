using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DDPA.Attributes;
using DDPA.DTO;
using DDPA.Service;
using DDPA.SQL.Entities;
using DDPA.Web.Models;
using static DDPA.Commons.Enums.DDPAEnums;
using DDPA.Commons.Results;
using DDPA.Web.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;
using DDPA.Commons.Helper;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using OfficeOpenXml;
using System.Text;

namespace DDPA.Web.Controllers
{
    /// <summary>
    /// This Controller is for Administration module.
    /// Administration is named Maintenance before.
    /// Todo: Rename this Controller.
    /// </summary>
    public class MaintenanceController : Controller
    {
        private readonly SignInManager<ExtendedIdentityUser> _signInManager;
        private readonly UserManager<ExtendedIdentityUser> _userManager;
        private readonly ILogger _logger;
        private readonly IMaintenanceService _maintenanceService;
        private readonly IMapper _mapper;
        private readonly IQueryService _queryService;

        public MaintenanceController(SignInManager<ExtendedIdentityUser> signInManager, UserManager<ExtendedIdentityUser> userManager,
            ILogger<AccountController> logger, IQueryService queryService, IMaintenanceService maintenanceService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _maintenanceService = maintenanceService;
            _queryService = queryService;
            _mapper = this.GetMapper();
        }

        [HttpGet]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public IActionResult User()
        {
            HttpContext.Session.SetString(SessionHelper.SHOW_MODAL, "0");
            return View();
        }

        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> GetUser()
        {
            var loggedUser = await _userManager.GetUserAsync(HttpContext.User);
            string userRole = HttpContext.Session.GetString(SessionHelper.ROLES);
            string userId = HttpContext.Session.GetString(SessionHelper.USER_ID);
            var user = _queryService.GetUsers();
            var model = _mapper.Map<List<UserViewModel>>(user);
            

            foreach(UserViewModel item in model)
            {
                var tempDepartment = await _queryService.GetDepartmentById(item.DepartmentId.ToString());
                item.DepartmentId = tempDepartment.Name;
            }

            if (model == null)
            {
                model = new List<UserViewModel>();
            }

            return Json(new
            {
                data = model
            });
        }

        [HttpGet]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> AddUser()
        {
            try
            {
                //TO DO: get current user role and department
                //filter department
                List<DepartmentDTO> departmentList = await _queryService.GetDepartments();
                ViewData["departmentList"] = departmentList;
                List<string> roleList = _queryService.GetRoles();
                if(ViewBag.userRole != "ADMINISTRATOR")
                {
                    roleList.Remove("ADMINISTRATOR");
                    roleList.Remove("DPO");
                }
                
                ViewData["roleList"] = roleList;

                
                return View();
            }
            catch (Exception)
            {

                throw;
            }

        }

        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> AddUser(AddUserViewModel model)
        {
            Result result = new Result();
            try
            {
                string userRole = HttpContext.Session.GetString(SessionHelper.ROLES) ?? "";
                string userId = HttpContext.Session.GetString(SessionHelper.USER_ID) ?? "";
                var user = await _userManager.GetUserAsync(HttpContext.User);
                AddUserDTO dto = _mapper.Map<AddUserDTO>(model);
                dto.CreatedBy = userId;
                result = await _maintenanceService.CreateUser(dto);

                //redirect to user list after add
                if(result.Success)
                {
                    result.IsRedirect = true;
                    result.RedirectUrl = "Maintenance/User";
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error Exception on AddUser: " + e.Message.ToString());
            }

            return Json(result);
        }

        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> GetModuleRights(string id, string role)
        {
            var moduleRights = await _queryService.GetModules(role);
            var modules = _mapper.Map<List<ModuleViewModel>>(moduleRights);
            List<UserRightsViewModel> modelf = new List<UserRightsViewModel>();
            if (modules != null)
            {
                foreach(var item in moduleRights)
                {
                    var model = new UserRightsViewModel();
                    model.Id = item.Id;
                    model.ModuleName = item.Display;
                    var userRights = await _queryService.GetUserRights(id ,item.Id);
                    if(userRights == null)
                    {
                        model.View = 0;
                        model.Add = 0;
                        model.Edit = 0;
                    }
                    else
                    {
                        model.View = userRights.View? 1 : 0;
                        model.Add = userRights.Add ? 1 : 0;
                        model.Edit = userRights.Edit ? 1 : 0;
                    }
                    if (!item.View)
                    {
                       model.View = 2;
                    }
                    if (!item.Add)
                    {
                        model.Add = 2;
                    }
                    if (!item.Edit)
                    {
                        model.Edit = 2;
                    }
                    modelf.Add(model);
                }
            }
            

            return Json(new
            {
                data = modelf
            });
        }

        [HttpGet]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> UpdateUser(string id)
        {
            //get the data of user 
            var user = await _queryService.GetUserById(id);

            //get the departments and save it be shown in the view
            List<DepartmentDTO> departmentList = await _queryService.GetDepartments();
            ViewData["departmentList"] = departmentList;

            //get the Role and save it be shown in the view
            List<string> roleList = _queryService.GetRoles();
            roleList.Remove("ADMINISTRATOR");
            roleList.Remove("DPO");
            ViewData["roleList"] = roleList;
            var model = _mapper.Map<UpdateUserViewModel>(user);

            return View(model);
        }


        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> UpdateUser(UpdateUserViewModel model)
        {
            Result result = new Result();
            try
            {
                result = await _maintenanceService.UpdateUser(_mapper.Map<UpdateUserDTO>(model));
                //redirect to user list after update
                if (result.Success)
                {
                    result.IsRedirect = true;
                    result.RedirectUrl = "Maintenance/User";
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error Exception on UpdateUser: " + e.Message.ToString());
            }

            return Json(result);
        }

        public IActionResult DeleteUser(string id)
        {
            Result result = new Result();

            result = _maintenanceService.DeleteUser(id);
            TempData["resultMsg"] = result.Message;
            TempData["isSuccess"] = result.Success ? "1" : "0";
            if (result.Success)
            {
                return RedirectToAction("User", "Maintenance");
            }

            else if (!result.Success)
            {

            }
            return View();
        }

        [HttpGet]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> ChangePasswordUser()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            string userId = HttpContext.Session.GetString(SessionHelper.USER_ID);
            var data = await _queryService.GetUserById(user.Id.ToString());
            var model = _mapper.Map<ChangePasswordUserViewModel>(data);
            return View(model);
        }

        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> ChangePasswordUser(ChangePasswordUserViewModel model)
        {
            Result result = new Result();

            //get current logged user
            var user = await _userManager.GetUserAsync(HttpContext.User);

            //get logged user's ID
            model.Id = user.Id.ToString();
            result = await _maintenanceService.ChangePasswordUser(_mapper.Map<ChangePasswordUserDTO>(model));
            TempData["resultMsg"] = result.Message;
            if (result.Success)
            {
                HttpContext.Session.Clear();
                await _signInManager.SignOutAsync();
                _logger.LogInformation(4, "User logged out.");
                result.IsRedirect = true;
                result.RedirectUrl = "";
                return Json(result);
            }

            else if (!result.Success)
            {
                return Json(result);
            }
            return View();
        }

        [ServiceFilter(typeof(SharedMessageAttribute))]
        public IActionResult Field()
        {
            return View();
        }

        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> GetFields()
        {
            var model = new List<FieldViewModel>();
            try
            {
                var fields = await _queryService.GetAllFields();
                if (fields == null)
                    model = new List<FieldViewModel>();
                else
                    model = _mapper.Map<List<FieldViewModel>>(fields);
            }
            catch (Exception e)
            {
                _logger.LogError("Error Exception on GetFields: " + e.Message.ToString());
            }

            return Json(new
            {
                data = model
            });
        }

        [HttpGet]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public IActionResult AddField()
        {
            return View();
        }

        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> AddField(AddFieldViewModel model)
        {
            Result result = new Result();
            var user = await _userManager.GetUserAsync(HttpContext.User);
            string userId = HttpContext.Session.GetString(SessionHelper.USER_ID);
            result = await _maintenanceService.CreateField(_mapper.Map<AddFieldDTO>(model), userId);
            //redirect to user list after add
            if (result.Success)
            {
                result.IsRedirect = true;
                result.RedirectUrl = "Maintenance/Field";
            }
            
            return Json(result);
        }

        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> AddFieldModal(AddFieldViewModel model)
        {
            Result result = new Result();
            try
            {
                if (model.Type == 0 || String.IsNullOrEmpty(model.Name))
                {
                    result.ErrorCode = ErrorCode.INVALID_INPUT;
                    result.Message = "Please fill in the required field";
                    result.Success = false;
                    return Json(result);
                }

                var user = await _userManager.GetUserAsync(HttpContext.User);
                result = await _maintenanceService.CreateField(_mapper.Map<AddFieldDTO>(model), user.ToString());

                var subModuleField = new AddSubModuleFieldDTO();
                subModuleField.FieldId = Convert.ToInt32(result.Id);
                subModuleField.SubModuleId = model.SubModuleId;
                subModuleField.Order = await _queryService.GetSubModuleFieldMaxOrder(model.SubModuleId);

                result = await _maintenanceService.AddSubModuleField(subModuleField);
            }
            catch (Exception e)
            {
                _logger.LogError("Error Exception on AddFieldModal: " + e.Message.ToString());
            }
            return Json(result);
        }


        [HttpGet]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> UpdateField(string id)
        {
            try
            {
                var data = await _queryService.GetFieldById(id);
                var model = _mapper.Map<UpdateFieldViewModel>(data);
                return View(model);
            }

            catch (Exception e)
            {
                _logger.LogError("Error calling UpdateField: {0}", e.Message);
            }
            return View();
        }


        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> UpdateField(UpdateFieldViewModel model)
        {
            Result result = new Result();
            try
            {
                result = await _maintenanceService.UpdateField(_mapper.Map<UpdateFieldDTO>(model));

                //redirect to user list after update of field
                if (result.Success)
                {
                    result.IsRedirect = true;
                    result.RedirectUrl = "Maintenance/Field";
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling UpdateField: {0}", e.Message);
            }

            return Json(result);
        }

        public IActionResult DeleteField(string id)
        {
            Result result = new Result();

            result = _maintenanceService.DeleteField(id);
            TempData["resultMsg"] = result.Message;
            TempData["isSuccess"] = result.Success ? "1" : "0";
            if (result.Success)
            {
                return RedirectToAction("Field", "Maintenance");
            }

            return View();
        }

        public async Task<IActionResult> GetFieldItemsById(string id)
        {
            var model = new List<FieldItemViewModel>();
            try
            {
                var fields = await _queryService.GetFieldItemsById(id);
                if (fields == null)
                    model = new List<FieldItemViewModel>();
                else
                    model = _mapper.Map<List<FieldItemViewModel>>(fields);
            }
            catch (Exception e)
            {
                _logger.LogError("Error Exception on GetFields: " + e.Message.ToString());
            }

            return Json(new
            {
                data = model
            });
        }

        [HttpGet]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> AddFieldItem(int id)
        {
            var model = new AddFieldItemViewModel();

            var field = await _queryService.GetFieldById(id.ToString());

            HttpContext.Session.SetString("fieldId", id.ToString());
            model.FieldId = Convert.ToInt32(HttpContext.Session.GetString("fieldId"));

            HttpContext.Session.SetString("fieldName", field.Name);
            ViewData["fieldName"] = HttpContext.Session.GetString("fieldName");

            return View(model);
        }

        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> AddFieldItem(AddFieldItemViewModel model)
        {
            int fieldId = model.FieldId;
            Result result = new Result();
            try
            {
                result = await _maintenanceService.CreateFieldItem(_mapper.Map<AddFieldItemDTO>(model));
                TempData["resultMsg"] = result.Message;
                ViewData["fieldName"] = HttpContext.Session.GetString("fieldName");
            }
            catch (Exception e)
            {
                _logger.LogError("Error Exception on GetFields: " + e.Message.ToString());
            }
            return Json(result);
        }

        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> AddFieldItemModal(AddFieldItemViewModel model)
        {
            int fieldId = model.FieldId;
            Result result = new Result();
            try
            {
                if (model.FieldId == 0 || String.IsNullOrEmpty(model.Name))
                {
                    result.ErrorCode = ErrorCode.INVALID_INPUT;
                    result.Message = "Please fill in the required field";
                    result.Success = false;
                    return View();
                }

                result = await _maintenanceService.CreateFieldItem(_mapper.Map<AddFieldItemDTO>(model));
            }
            catch (Exception e)
            {
                _logger.LogError("Error Exception on AddFieldItemModal: " + e.Message.ToString());
            }
            return new EmptyResult();
        }

        [HttpGet]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> UpdateFieldItem(string id)
        {
            try
            {
                ViewData["fieldName"] = HttpContext.Session.GetString("fieldName");
                FieldItemDTO _return = await _queryService.GetFieldItemById(id);
                UpdateFieldItemViewModel model = _mapper.Map<UpdateFieldItemViewModel>(_return);
                return View(model);
            }
            catch (Exception)
            {
                throw;
            }

        }

        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> UpdateFieldItem(UpdateFieldItemViewModel model)
        {
            Result result = new Result();
            try
            {
                result = await _maintenanceService.UpdateFieldItem(_mapper.Map<UpdateFieldItemDTO>(model));
                ViewData["fieldName"] = HttpContext.Session.GetString("fieldName");
                TempData["resultMsg"] = result.Message;
                if (result.Success)
                {
                    return RedirectToAction("AddFieldItem", "Maintenance", new { id = model.FieldId });
                }
                else if (!result.Success)
                {
                    return View(model);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return RedirectToAction("AddFieldItem", "Maintenance", new { id = model.FieldId });
        }

        public IActionResult DeleteFieldItem(string id, string fieldId)
        {
            Result result = new Result();
            try
            {
                result = _maintenanceService.DeleteFieldItem(id);
                TempData["resultMsg"] = result.Message;

                if (result.Success)
                {
                    return RedirectToAction("AddFieldItem", "Maintenance", new { id = fieldId });
                }
            }
            catch (Exception)
            {

                throw;
            }
            return View();
        }

        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        [Consumes("application/json")]
        public IActionResult BulkDeleteField([FromBody]List<string> listOfFields)
        {
            Result result = new Result();
            if (listOfFields != null)
            {
                result = _maintenanceService.BulkDeleteField(listOfFields);
                TempData["resultMsg"] = result.Message;
                TempData["isSuccess"] = result.Success ? "1" : "0";
            }
            return Json(result);
        }
        
        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        [Consumes("application/json")]
        public IActionResult BulkDeleteUser([FromBody]List<string> listOfUsers)
        {
            Result result = new Result();
            if (listOfUsers != null)
            {
                result = _maintenanceService.BulkDeleteUser(listOfUsers);
                TempData["resultMsg"] = result.Message;
                TempData["isSuccess"] = result.Success ? "1" : "0";
            }
            return Json(result);
        }
        
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public IActionResult LifeCycleField()
        {
            return View();
        }


        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> GetLifeCycleFields()
        {
            var model = new List<FieldViewModel>();
            try
            {
                var fields = await _queryService.GetLifeCycleFields();

                fields.ForEach(x =>
                {
                    if(x.LifeCycle != null && x.LifeCycle != "")
                    {
                        if (x.LifeCycle.ToLower() == "usage")
                        {
                            x.LifeCycle = "Use";
                        }

                        else if (x.LifeCycle.ToLower() == "transfer")
                        {
                            x.LifeCycle = "Disclosure";
                        }
                    }
                });
                if (fields == null)
                    model = new List<FieldViewModel>();
                else
                    model = _mapper.Map<List<FieldViewModel>>(fields);
            }
            catch (Exception e)
            {
                _logger.LogError("Error Exception on GetFields: " + e.Message.ToString());
            }

            return Json(new
            {
                data = model
            });
        }

        [HttpGet]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public IActionResult AddLifeCycleField()
        {
            return View();
        }

        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> AddLifeCycleField(AddFieldViewModel model)
        {
            Result result = new Result();
            var user = await _userManager.GetUserAsync(HttpContext.User);
            string userId = HttpContext.Session.GetString(SessionHelper.USER_ID);
            result = await _maintenanceService.CreateLifeCycleField(_mapper.Map<AddFieldDTO>(model), userId);

            if(!result.Success)
            {
                return Json(result);
            }

            var subModuleField = new AddSubModuleFieldDTO();
            subModuleField.FieldId = Convert.ToInt32(result.Id);

            //hard coded, rework this
            subModuleField.SubModuleId = Convert.ToInt32(model.LifeCycle) + 1;

            subModuleField.Order = await _queryService.GetSubModuleFieldMaxOrder(model.SubModuleId);

            result = await _maintenanceService.AddSubModuleField(subModuleField);
            //redirect to user list after add
            if (result.Success)
            {
                result.IsRedirect = true;
                result.RedirectUrl = "Maintenance/LifeCycleField";
            }
            return Json(result);
        }

        [HttpGet]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> UpdateLifeCycleField(string id)
        {
            try
            {
                var data = await _queryService.GetFieldById(id);
                var lifeCycle = await _queryService.GetFieldsLifeCycle(id);
                data.LifeCycle = lifeCycle;
                var model = _mapper.Map<UpdateFieldViewModel>(data);
                return View(model);
            }

            catch (Exception e)
            {
                _logger.LogError("Error calling UpdateLifeCycleField: {0}", e.Message);
            }
            return View();
        }

        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> UpdateLifeCycleField(UpdateFieldViewModel model)
        {
            Result result = new Result();
            Result result2 = new Result();
            try
            {
                result = await _maintenanceService.UpdateLifeCycleField(_mapper.Map<UpdateFieldDTO>(model));
                if (result.Success)
                {
                    result2 = await _maintenanceService.UpdateSubModuleField(_mapper.Map<UpdateFieldDTO>(model));
                    result.IsRedirect = true;
                    result.RedirectUrl = "Maintenance/LifeCycleField";
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling UpdateLifeCycleField: {0}", e.Message);
            }

            return Json(result);
        }

        public IActionResult DeleteLifeCycleField(string id)
        {
            Result result = new Result();

            result = _maintenanceService.DeleteField(id);
            TempData["resultMsg"] = result.Message;
            TempData["isSuccess"] = result.Success ? "1" : "0";
            if (result.Success)
            {
                return RedirectToAction("LifeCycleField", "Maintenance");
            }

            return View();
        }

        [HttpGet]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public IActionResult Department()
        {
            return View();
        }

        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> GetDepartments()
        {
            var model = new List<DepartmentViewModel>();
            try
            {
                var departments = await _queryService.GetAllDepartments();
                if (departments == null)
                    model = new List<DepartmentViewModel>();
                else
                    model = _mapper.Map<List<DepartmentViewModel>>(departments);
            }
            catch (Exception e)
            {
                _logger.LogError("Error Exception on GetFields: " + e.Message.ToString());
            }

            return Json(new
            {
                data = model
            });
        }

        [HttpGet]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> UpdateDepartment(string id)
        {
            try
            {
                var data = await _queryService.GetDepartmentById(id);
                var model = _mapper.Map<UpdateDepartmentViewModel>(data);
                return View(model);
            }

            catch (Exception e)
            {
                _logger.LogError("Error Exception on Register: " + e.Message.ToString());
            }
            return View();
        }

        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> UpdateDepartment(UpdateDepartmentViewModel model)
        {
            Result result = new Result();
            try
            {
                result = await _maintenanceService.UpdateDepartment(_mapper.Map<UpdateDepartmentDTO>(model));

                //redirect to user list after update of field
                if (result.Success)
                {
                    result.IsRedirect = true;
                    result.RedirectUrl = "Maintenance/Department";
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling UpdateDepartment: {0}", e.Message);
            }

            return Json(result);
        }

        [HttpGet]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public IActionResult AddDepartment()
        {
            return View();
        }

        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> AddDepartment(AddDepartmentViewModel model)
        {
            Result result = new Result();
            string userId = HttpContext.Session.GetString(SessionHelper.USER_ID);
            result = await _maintenanceService.CreateDepartment(_mapper.Map<AddDepartmentDTO>(model), userId);
            //redirect to user list after add
            if (result.Success)
            {
                result.IsRedirect = true;
                result.RedirectUrl = "Maintenance/Department";
            }
            return Json(result);
        }

        public IActionResult DeleteDepartment(string id)
        {
            Result result = new Result();

            result = _maintenanceService.DeleteDepartment(id);
            TempData["resultMsg"] = result.Message;
            TempData["isSuccess"] = result.Success ? "1" : "0";
            if (result.Success)
            {
                return RedirectToAction("Department", "Maintenance");
            }

            return View();
        }

        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        [Consumes("application/json")]
        public IActionResult BulkDeleteDepartment([FromBody]List<string> listOfFields)
        {
            Result result = new Result();
            if (listOfFields != null)
            {
                result = _maintenanceService.BulkDeleteDepartment(listOfFields);
                TempData["resultMsg"] = result.Message;
                TempData["isSuccess"] = result.Success ? "1" : "0";
            }
            return Json(result);
        }
        
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public IActionResult Dataset()
        {
            return View();
        }

        [ServiceFilter(typeof(SharedMessageAttribute))]
        public IActionResult AddDataset()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> AddDataset(AddDatasetViewModel model)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            string userId = HttpContext.Session.GetString(SessionHelper.USER_ID) ?? "";
            Result result = new Result();
            result = await _maintenanceService.AddDataset(_mapper.Map<AddDatasetDTO>(model), userId);
            if (result.Success)
            {
                result.IsRedirect = true;
                result.RedirectUrl = "Maintenance/Dataset";
                return Json(result);
            }

            else
            {
                return Json(result);
            }
        }

        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> GetDatasets()
        {
            var tempDatasets = await _queryService.GetDataset();
            var templates = _mapper.Map<List<DatasetViewModel>>(tempDatasets);
            if (templates == null)
            {
                templates = new List<DatasetViewModel>();
            }

            return Json(new
            {
                data = templates
            });
        }

        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> UpdateDataset(string id)
        {
            Result result = new Result();
            var tempDataset = await _queryService.GetDatasetById(id);
            var dataset = _mapper.Map<UpdateDatasetViewModel>(tempDataset);

            return View(dataset);
        }

        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> UpdateDataset(UpdateDatasetViewModel model)
        {
            Result result = new Result();
            try
            {
                result = await _maintenanceService.UpdateDataset(_mapper.Map<UpdateDatasetDTO>(model));

                if(result.Success)
                {
                    result.IsRedirect = true;
                    result.RedirectUrl = "Maintenance/Dataset";
                }
                return Json(result);                
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling UpdateDataset: {0}", e.Message);
            }

            return Json(result);
        }

        [ServiceFilter(typeof(SharedMessageAttribute))]
        public IActionResult DeleteDataset(string id)
        {
            Result result = new Result();
            try
            {
                result = _maintenanceService.DeleteDataset(id);
                if (result.Success)
                {
                    result.IsRedirect = true;
                    result.RedirectUrl = "Maintenance/Dataset";
                }

                return Json(result);
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling DeleteDataset: {0}", e.Message);
            }

            return Json(result);
        }

        [HttpGet]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> AddFieldDataset(string id)
        {
            var dataset = await _queryService.GetDatasetById(id);
            DatasetViewModel model = _mapper.Map<DatasetViewModel>(dataset);
            TempData["dataSetName"] = model.Name;
            return View(model);
        }

        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> GetAvailableField(string id)
        {
            List<AddFieldDatasetDTO> tempAvailFields = await _queryService.GetAvailableField(id);
            var availFields = _mapper.Map<List<AddFieldDatasetViewModel>>(tempAvailFields);

            if (availFields == null)
            {
                availFields = new List<AddFieldDatasetViewModel>();
            }

            return Json(new
            {
                data = availFields

            });
        }

        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> GetCurrentField(string id)
        {
            var tempAvailFields = await _queryService.GetCurrentField(id);
            var availFields = _mapper.Map<List<FieldDatasetViewModel>>(tempAvailFields);
            if (availFields == null)
            {
                availFields = new List<FieldDatasetViewModel>();
            }

            return Json(new
            {
                data = availFields
            });
        }

        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> AddFieldToDataset(string datasetId, string fieldId)
        {
            Result result = new Result();
            result = await _maintenanceService.AddFieldToDataset(datasetId, fieldId);
            TempData["resultMsg"] = result.Message;
            if (result.Success)
            {
                return RedirectToAction("AddFieldDataset", "Maintenance", new { id = datasetId });
            }

            return RedirectToAction("AddFieldDataset", "Maintenance");
        }

        [ServiceFilter(typeof(SharedMessageAttribute))]
        public IActionResult DeleteDatasetField(string datasetId, string id)
        {
            Result result = new Result();

            result = _maintenanceService.DeleteDatasetField(id);
            TempData["resultMsg"] = result.Message;
            if (result.Success)
            {
                return RedirectToAction("AddFieldDataset", "Maintenance", new { id = datasetId });
            }

            else if (!result.Success)
            {

            }
            return RedirectToAction("AddFieldDataset", "Maintenance", new { id = datasetId });
        }

        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> DownloadDatasetExcel()
        {
            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
            var subModulesField = await _queryService.GetAllSubModuleFields();

            string tempListOfCollectionField = "";

            //next column for the dataset field
            int nextColumn = 0;
            for (int i = 1; i <= subModulesField.Count; i++)
            {
                tempListOfCollectionField += subModulesField[i - 1].Field.Name + ",";
                nextColumn++;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(tempListOfCollectionField);
            return File(System.Text.Encoding.ASCII.GetBytes(sb.ToString()), "text/csv", "Bulk Import Template.csv");
        }

        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        [Consumes("application/json")]
        public IActionResult BulkDeleteDataset([FromBody]List<string> listToDelete)
        {
            Result result = new Result();
            try
            {
                if (listToDelete != null)
                {
                    result = _maintenanceService.BulkDeleteDataset(listToDelete);
                    if (result.Success)
                    {
                        result.IsRedirect = true;
                        result.RedirectUrl = "Maintenance/Dataset";
                    }
                }
                return Json(result);
            }
            catch (Exception e)
            {
                result.ErrorCode = ErrorCode.EXCEPTION;
                result.Success = false;
                result.Message = "Error on deleting data set.";

                _logger.LogError("Error on calling BulkDeleteDataset: {0}", e.Message);
                return Json(result);
            }           
        }

        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        [Consumes("application/json")]
        public async Task<IActionResult> GetDataSetName([FromBody]string docId)
        {
            string dataSetName = "";
            try
            {
                int dataSetId = (await _queryService.GetDocumentById(docId)).DatasetId;

                if(dataSetId  == 0)
                {
                    dataSetName = "NO DATA SET";
                }
                else if (dataSetId != 0)
                {
                    dataSetName = (await _queryService.GetDatasetById(dataSetId.ToString())).Name + " Fields";
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error on calling GetDataSetName: {0}", e.Message);
                throw;
            }
           
            return Json(dataSetName);
        }

        [HttpGet]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> ChangeCompanyName()
        {
            try
            {
                ChangeCompanyInfoViewModel model = new ChangeCompanyInfoViewModel();
                var data = await _queryService.GetCompanyInfo();
                if(model == null)
                    model = new ChangeCompanyInfoViewModel();
                else
                    model = _mapper.Map<ChangeCompanyInfoViewModel>(data);
                return View(model);
            }
            catch (Exception e)
            {
                _logger.LogError("Error on calling ChangeCompanyName: {0}", e.Message);
                throw;
            }
        }

        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> ChangeCompanyName(ChangeCompanyInfoViewModel model)
        {
            Result result = new Result();
            try
            {
                string userId = HttpContext.Session.GetString(SessionHelper.USER_ID);
                result = await _maintenanceService.ChangeCompanyName(_mapper.Map<UpdateCompanyInfoDTO>(model), userId);
                TempData["resultMsg"] = result.Message;
                if (result.Success)
                {
                    HttpContext.Session.SetString(SessionHelper.COMPANY_NAME, (await _queryService.GetCompanyInfo()).Name);
                    result.IsRedirect = true;
                    result.RedirectUrl = "Dashboard/Index";
                }
                return Json(result);
            }
            catch (Exception e)
            {
                _logger.LogError("Error on calling ChangeCompanyName: {0}", e.Message);
                throw;
            }
        }
      
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> SkipSetup()
        {
            Result result = new Result();
            try
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                result = await _maintenanceService.SetUp(user.Id.ToString());
                if (result.Success)
                {
                    HttpContext.Session.SetString(SessionHelper.DONE_SETUP, "1");
                    HttpContext.Session.SetString(SessionHelper.SHOW_MODAL, "1");

                    return RedirectToAction("Index", "Dashboard");

                }
                return RedirectToAction("User", "Maintenance");
            }
            catch (Exception e)
            {
                _logger.LogError("Error on calling SkipSetup: {0}", e.Message);
                throw;
            }
        }

        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> DoneSetup()
        {
            Result result = new Result();
            try
            {
                var users = _queryService.GetUsers();
                if (users.Count > 0) {
               
                var user = await _userManager.GetUserAsync(HttpContext.User);
                result = await _maintenanceService.SetUp(user.Id.ToString());
                }
                if (result.Success)
                {
                    HttpContext.Session.SetString(SessionHelper.DONE_SETUP, "1");
                    HttpContext.Session.SetString(SessionHelper.SHOW_MODAL, "1");
                    return RedirectToAction("Index", "Dashboard");
                }
                return RedirectToAction("User", "Maintenance");
            }
            catch (Exception e)
            {
                _logger.LogError("Error on calling SkipSetup: {0}", e.Message);
                throw;
            }
        }
    }
}