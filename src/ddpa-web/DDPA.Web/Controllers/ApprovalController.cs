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
using System.Linq;

namespace DDPA.Web.Controllers
{
    public class ApprovalController : Controller
    {
        private readonly SignInManager<ExtendedIdentityUser> _signInManager;
        private readonly UserManager<ExtendedIdentityUser> _userManager;
        private readonly ILogger _logger;
        private readonly IApprovalService _approvalService;
        private readonly IMapper _mapper;
        private readonly IQueryService _queryService;

        public ApprovalController(SignInManager<ExtendedIdentityUser> signInManager, UserManager<ExtendedIdentityUser> userManager,
            ILogger<AccountController> logger, IQueryService queryService, IApprovalService approvalService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _approvalService = approvalService;
            _queryService = queryService;
            _mapper = this.GetMapper();
        }

        [HttpGet]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public IActionResult Index()
        {
            var userRole = HttpContext.Session.GetString(SessionHelper.ROLES);
            
            if (userRole == nameof(Role.USER) || userRole == nameof(Role.DEPTHEAD))
            {
                return RedirectToAction("MyRequest", "Approval");
            }
            else if (userRole == nameof(Role.DPO) || userRole == nameof(Role.ADMINISTRATOR))
            {
                return RedirectToAction("MyApproval", "Approval");
            }

            return View();
        }

        /// <summary>
        /// Get the requested documents of either user or dept head.
        /// Documents added, updated, or deleted by these role are needed an approval from higher roles
        /// </summary>
        /// 
        /// <returns>
        /// The specific document for the respective user
        /// </returns>
        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> GetRequestDocuments()
        {
            var model = new List<ApprovalDocumentViewModel>();
            try
            {
                string userDept = HttpContext.Session.GetString(SessionHelper.USER_DEPT);
                var userRole = HttpContext.Session.GetString(SessionHelper.ROLES);
                var userId = HttpContext.Session.GetString(SessionHelper.USER_ID);
                var requestDocuments = await _queryService.GetRequestDocuments(userDept, userId);
                if (requestDocuments == null)
                    model = new List<ApprovalDocumentViewModel>();
                else
                    model = _mapper.Map<List<ApprovalDocumentViewModel>>(requestDocuments);
            }
            catch (Exception e)
            {
                _logger.LogError("Error Exception on GetRequestDocuments: " + e.Message.ToString());
            }

            return Json(new
            {
                data = model
            });
        }

        /// <summary>
        /// Get the documents that needed approval of the current user.
        /// </summary>
        /// 
        /// <returns>
        /// The specific document for the respective user.
        /// </returns>
        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> GetApprovalDocuments()
        {
            var model = new List<ApprovalDocumentViewModel>();
            try
            {
                string userDept = HttpContext.Session.GetString(SessionHelper.USER_DEPT);
                var userRole = HttpContext.Session.GetString(SessionHelper.ROLES);
                var userId = HttpContext.Session.GetString(SessionHelper.USER_ID);
                var approvalDocuments = await _queryService.GetApprovalDocuments(userRole, userDept, userId);
                if (approvalDocuments == null)
                    model = new List<ApprovalDocumentViewModel>();
                else
                    model = _mapper.Map<List<ApprovalDocumentViewModel>>(approvalDocuments);
            }
            catch (Exception e)
            {
                _logger.LogError("Error Exception on GetApprovalDocuments: " + e.Message.ToString());
            }

            return Json(new
            {
                data = model
            });
        }

        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        [Consumes("application/json")]
        public async Task<IActionResult> ApproveDocuments([FromBody]List<string> documents)
        {
            Result result = new Result();
            if (documents != null)
            {
                string userDept = HttpContext.Session.GetString(SessionHelper.USER_DEPT);
                var userRole = HttpContext.Session.GetString(SessionHelper.ROLES);
                var userId = HttpContext.Session.GetString(SessionHelper.USER_ID);
                result = await _approvalService.ApproveDocuments(documents, userRole, userId);
                TempData["resultMsg"] = result.Message;
                TempData["isSuccess"] = result.Success ? "1" : "0";
            }
            return Json(result);
        }
        
        [HttpGet]
        [ServiceFilter(typeof(SharedMessageAttribute))]

        public IActionResult MyRequest()
        {
            return View();
        }

        [HttpGet]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public IActionResult MyApproval()
        {
            return View();
        }


        /// <summary>
        /// Count both the requested and needed-approval documents.
        /// This counts will be displayed in either MyRequest or MyApproval page.
        /// </summary>
        /// 
        /// <returns>
        /// A string separated by comma, with request count in the left, and approval count in the right.
        /// ex: "3,5"
        /// </returns>
        [HttpGet]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<string> CountRequestAndApprovall()
        {
            string counts = "";
            int myRequestCount = 0;
            int myApprovalCount = 0;
            string userId = HttpContext.Session.GetString(SessionHelper.USER_ID);
            string userDept = HttpContext.Session.GetString(SessionHelper.USER_DEPT);
            string userRole = HttpContext.Session.GetString(SessionHelper.ROLES);

            //get the requests count
            if (userRole == nameof(Role.USER) || userRole == nameof(Role.DEPTHEAD) || userRole == nameof(Role.DPO))
            {
                var myRequest = await _queryService.GetRequestDocuments(userDept, userId);
                myRequestCount = myRequest.Count;
            }

            //get the approval count
            if (userRole == nameof(Role.DEPTHEAD) || userRole == nameof(Role.DPO) || userRole == nameof(Role.ADMINISTRATOR))
            {
                var myApproval = await _queryService.GetApprovalDocuments(userRole, userDept, userId);
                myApprovalCount = myApproval.Count;
            }

            counts = myRequestCount.ToString() + "," + myApprovalCount.ToString();
            return counts;
        }

        [HttpGet]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> ViewDetails(string id, string status)
        {
            var userId = HttpContext.Session.GetString(SessionHelper.USER_ID);
            var model = new DocumentViewModel();
            try
            {
                var document = await _queryService.GetRequestDocument(id);
                model = await ConstructDocument(id, "/DDPA/"+status);

                var tempDatasets = await _queryService.GetDataset();
                var datasets = _mapper.Map<List<DatasetViewModel>>(tempDatasets);
                if (datasets == null)
                {
                    datasets = new List<DatasetViewModel>();
                }

                model.Datasets = datasets;

                var title = model.Status.ToString();
                ViewData["Title"] = title;
                ViewData["statusName"] = title.ToLower();
                ViewData["userAction"] = "View";
                if ((userId == document.createdBy) && (document.State == State.Rework))
                {
                    ViewData["userAction"] = "Edit";
                }
                else if ((userId != document.createdBy) && (document.State == State.Pending))
                {
                    ViewData["userAction"] = "Process";
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error Exception on Collection: " + e.Message.ToString());
            }       

            return View(model);
        }

        private async Task<DocumentViewModel> ConstructDocument(string id, string url)
        {
            var model = new DocumentViewModel();
            try
            {
                var subModule = await _queryService.GetSubModuleByUrl(url);
                var subModuleField = await _queryService.GetSubModuleFields(subModule.Id);
                var fields = _mapper.Map<List<FieldViewModel>>(subModuleField);
                var datasetFields = new List<FieldDatasetViewModel>();
                model.FieldDataset = new List<FieldDatasetViewModel>();

                if (!String.IsNullOrEmpty(id))
                {
                    var document = await _queryService.GetDocumentById(id);
                    model = _mapper.Map<DocumentViewModel>(document);
                    foreach (var field in fields)
                    {
                        var docField = model.DocumentField.Where(x => x.FieldId == (Convert.ToInt32(field.Id)) && x.SubModuleId == (Convert.ToInt32(subModule.Id)));
                        if (docField.Count() > 0)
                        {
                            if (docField.FirstOrDefault().IsEdited)
                            {
                                field.Value = docField.FirstOrDefault().NewValue;
                            }
                            else
                            {
                                field.Value = docField.FirstOrDefault().Value;
                            }
                        }
                    }
                    model.DatasetId = document.DatasetId;
                    if (model.DatasetId > 0)
                    {
                        var dto = await _queryService.GetCurrentField(model.DatasetId.ToString());
                        datasetFields = _mapper.Map<List<FieldDatasetViewModel>>(dto);
                        foreach (var datasetField in datasetFields)
                        {
                            var docDatasetField = model.DocumentDatasetField.Where(x => x.FieldId == (Convert.ToInt32(datasetField.FieldId)) && x.DatasetId == document.DatasetId && x.DocumentId == document.Id).Select(x => x.Value);
                            if (docDatasetField.Count() > 0)
                            {
                                datasetField.Value = docDatasetField.FirstOrDefault();
                            }
                        }
                    }

                    model.DatasetId = document.DatasetId;
                }
                model.Field = fields;
                model.SubModuleId = subModule.Id;
                model.FieldDataset = datasetFields;

                if (subModule.Name.Contains(nameof(Status.Collection)))
                    model.Status = Status.Collection;
                else if (subModule.Name.Contains(nameof(Status.Archival)))
                    model.Status = Status.Archival;
                else if (subModule.Name.Contains(nameof(Status.Disposal)))
                    model.Status = Status.Disposal;
                else if (subModule.Name.Contains(nameof(Status.Storage)))
                    model.Status = Status.Storage;
                else if (subModule.Name.Contains(nameof(Status.Transfer)))
                    model.Status = Status.Transfer;
                else if (subModule.Name.Contains(nameof(Status.Usage)))
                    model.Status = Status.Usage;

                model.Datasets = new List<DatasetViewModel>();
            }
            catch (Exception e)
            {
                _logger.LogError("Error Exception on ConstructDocument: " + e.Message.ToString());
            }
            return model;
        }

        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> ReworkDocument(LogsViewModel model)
        {
            Result result = new Result();
            try
            {
                string userDept = HttpContext.Session.GetString(SessionHelper.USER_DEPT);
                var userRole = HttpContext.Session.GetString(SessionHelper.ROLES);
                var userId = HttpContext.Session.GetString(SessionHelper.USER_ID);
                result = await _approvalService.ReworkDocument(model.DocId, userRole, userId, model.Comment);
                TempData["resultMsg"] = result.Message;
                TempData["isSuccess"] = result.Success ? "1" : "0";

            }
            catch (Exception e)
            {
                _logger.LogError("Error Exception on AddFieldModal: " + e.Message.ToString());
            }
            result.IsRedirect = true;
            result.RedirectUrl = "Approval/MyApproval";
             return Json(result);
        }

        [HttpGet]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> GetLogs(string docId)
        {
            var model = new List<LogsViewModel>();
            try
            {
                string userDept = HttpContext.Session.GetString(SessionHelper.USER_DEPT);
                var userRole = HttpContext.Session.GetString(SessionHelper.ROLES);
                var userId = HttpContext.Session.GetString(SessionHelper.USER_ID);
                var logs = await _queryService.GetLogs(docId);
                if (logs == null)
                    model = new List<LogsViewModel>();
                else
                    model = _mapper.Map<List<LogsViewModel>>(logs);
            }
            catch (Exception e)
            {
                _logger.LogError("Error Exception on GetLogs: " + e.Message.ToString());
            }

            return Json(new
            {
                data = model
            });
        }

        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        [Consumes("application/json")]
        public async Task<IActionResult> BulkDeleteDrafts([FromBody]List<string> listToDelete)
        {
            Result result = new Result();
            if (listToDelete != null)
            {
                result = await _approvalService.BulkDeleteDrafts(listToDelete);
                TempData["resultMsg"] = result.Message;
                TempData["isSuccess"] = result.Success ? "1" : "0";
            }
            return Json(result);
        }
    }
}