using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using DDPA.Attributes;
using DDPA.Commons.Helper;
using DDPA.Commons.Results;
using DDPA.DTO;
using DDPA.Service;
using DDPA.SQL.Entities;
using DDPA.Web.Extensions;
using DDPA.Web.Models;
using static DDPA.Commons.Enums.DDPAEnums;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http.Headers;
using OfficeOpenXml;
using System.Text;
using CsvHelper;

namespace DDPA.Web.Controllers
{
    public class DatasetController : Controller
    {
        private readonly ILogger _logger;
        private readonly IQueryService _queryService;
        private readonly IMapper _mapper;
        private readonly IAdminService _adminService;
        private readonly IDatasetService _datasetService;

        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly UserManager<ExtendedIdentityUser> _userManager;

        public DatasetController(UserManager<ExtendedIdentityUser> userManager, ILogger<AccountController> logger, IQueryService queryService, IAdminService adminService, IDatasetService datasetService, IHostingEnvironment hostingEnvironment)
        {
            _logger = logger; 
            _queryService = queryService;
            _adminService = adminService;
            _datasetService = datasetService;
            _mapper = this.GetMapper();
            _hostingEnvironment = hostingEnvironment;
            _userManager = userManager;
        }

        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> DataList()
        {
            var model = new List<DatasetViewModel>();
            try
            {
                var userRole = HttpContext.Session.GetString(SessionHelper.ROLES);
                var userDept = HttpContext.Session.GetString(SessionHelper.USER_DEPT);

                if (userRole == nameof(Role.DPO) || userRole == nameof(Role.ADMINISTRATOR))
                {
                    List<DepartmentDTO> departmentList = await _queryService.GetDepartments();
                    ViewData["departmentList"] = departmentList;
                }
                else if (userRole == nameof(Role.DEPTHEAD) || userRole == nameof(Role.USER))
                {
                    ViewData["deptName"] = (await _queryService.GetDepartmentById(userDept)).Name;
                    ViewData["deptId"] = (await _queryService.GetDepartmentById(userDept)).Id;
                }

                List<DatasetDTO> dataSet = await _queryService.GetDataset();
                ViewData["dataSets"] = dataSet;

                var tempDatasets = await _queryService.GetDataset();
                var datasets = _mapper.Map<List<DatasetViewModel>>(tempDatasets);
                if (datasets == null)
                {
                    datasets = new List<DatasetViewModel>();
                }
                model = datasets;
            }
            catch (Exception e)
            {
                _logger.LogError("Error Exception on DataList: " + e.Message.ToString());
            }

            return View(model);
        }

        [HttpGet]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> Collection(string id, string datasetId)
        {
            var model = new DocumentViewModel();
            try
            {
                var document = await _queryService.GetDocumentById(id);
                model = await ConstructDocument(id, "/DDPA/Collection");

                if (!String.IsNullOrEmpty(model.DatasetId.ToString()) && model.DatasetId > 0)
                {
                    datasetId = model.DatasetId.ToString();
                }
               
                if (!String.IsNullOrEmpty(datasetId))
                {
                    var datasetDtl = await _queryService.GetDatasetById(datasetId);
                    if(datasetDtl != null)
                    {
                        model.DatasetName = datasetDtl.Name;
                        model.DatasetId = Convert.ToInt32(datasetId);
                        if (model.FieldDataset.Count() == 0)
                        {
                            var datasetFields = await _queryService.GetCurrentField(datasetId);
                            model.FieldDataset = _mapper.Map<List<FieldDatasetViewModel>>(datasetFields);
                        }
                    }
                }

                var tempDatasets = await _queryService.GetDataset();
                var datasets = _mapper.Map<List<DatasetViewModel>>(tempDatasets);
                if (datasets == null)
                {
                    datasets = new List<DatasetViewModel>();
                }

                model.Datasets = datasets;
            }
            catch(Exception e)
            {
                _logger.LogError("Error Exception on Collection: " + e.Message.ToString());
            }
            return View(model);
        }


        [HttpGet]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> Storage(string id)
        {
            var model = new DocumentViewModel();
            try
            {
                model = await ConstructDocument(id, "/DDPA/Storage");
            }
            catch (Exception e)
            {
                _logger.LogError("Error Exception on Storage: " + e.Message.ToString());
            }
            return View(model);
        }

        [HttpGet]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> Usage(string id)
        {
            var model = new DocumentViewModel();
            try
            {
                model = await ConstructDocument(id, "/DDPA/Usage");
            }
            catch (Exception e)
            {
                _logger.LogError("Error Exception on Usage: " + e.Message.ToString());
            }
            return View(model);
        }

        [HttpGet]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> Transfer(string id)
        {
            var model = new DocumentViewModel();
            try
            {
                model = await ConstructDocument(id, "/DDPA/Transfer");
            }
            catch (Exception e)
            {
                _logger.LogError("Error Exception on Transfer: " + e.Message.ToString());
            }
            return View(model);
        }

        [HttpGet]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> Archival(string id)
        {
            var model = new DocumentViewModel();
            try
            {
                model = await ConstructDocument(id, "/DDPA/Archival");
            }
            catch (Exception e)
            {
                _logger.LogError("Error Exception on Archival: " + e.Message.ToString());
            }
            return View(model);
        }

        [HttpGet]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> Disposal(string id)
        {
            var model = new DocumentViewModel();
            try
            {
                model = await ConstructDocument(id, "/DDPA/Disposal");
            }
            catch (Exception e)
            {
                _logger.LogError("Error Exception on Disposal: " + e.Message.ToString());
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCollection(DocumentViewModel document, List<IFormFile> file, List<IFormFile> datasetfile)
        {
            var response = new Result();
            try
            {
                string userDept = "";
                var currentUser = HttpContext.Session.GetString(SessionHelper.USER_NAME);
                var userId = HttpContext.Session.GetString(SessionHelper.USER_ID);
                var userRole = HttpContext.Session.GetString(SessionHelper.ROLES);

                if(userRole == nameof(Role.DPO) || userRole == nameof(Role.ADMINISTRATOR))
                {
                    userDept = document.DepartmentId;
                }

                else if (userRole == nameof(Role.DEPTHEAD) || userRole == nameof(Role.USER))
                {
                    userDept = HttpContext.Session.GetString(SessionHelper.USER_DEPT);
                }
                document.DocumentField = JsonConvert.DeserializeObject<List<DocumentFieldViewModel>>(document.JsonDocumentField);
                document.DocumentDatasetField = JsonConvert.DeserializeObject<List<DocumentDatasetFieldViewModel>>(document.JsonDatasetField);

                //adding new data set
                if (string.IsNullOrEmpty(document.Id.ToString()) || document.Id == 0)
                {
                    var doc = _mapper.Map<AddDocumentDTO>(document);
                    var dto = new AddDocumentDTO();
                    dto = doc;
                    dto.Status = Status.Collection;
                    dto.DocumentField = doc.DocumentField;
                    dto.DatasetId = document.DatasetId;

                    response = await _adminService.AddDocument(dto, file, datasetfile, userRole, userId, userDept);
                }
                //updating data set
                else
                {
                    var doc = _mapper.Map<UpdateDocumentDTO>(document);
                    var dto = new UpdateDocumentDTO();
                    dto = doc;
                    dto.DocumentField = doc.DocumentField;
                    dto.DatasetId = document.DatasetId;

                    //todo: UpdateDocument() and EditDocument() need merging.
                    if (userRole == nameof(Role.DPO) || userRole == nameof(Role.ADMINISTRATOR))
                    {
                        response = await _adminService.EditDocument(dto, Convert.ToInt32(document.SubModuleId), userRole, userId, userDept);
                    }

                    else if (userRole == nameof(Role.DEPTHEAD) || userRole == nameof(Role.USER))
                    {
                        response = await _adminService.EditDocument(dto, Convert.ToInt32(document.SubModuleId), userRole, userId, userDept);
                    }
                }
            }
            catch (Exception e) 
            {
                _logger.LogError("Error Exception on Adding a Collection: " + e.Message.ToString());
            }
            return Json(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetField(int status)
        {
            var model = new List<FieldViewModel>();
            try
            {
                var url = "/DDPA/Collection";
                if (status != 0)
                {
                    url = String.Format("/DDPA/{0}", (Status)status);
                }
                
                var subModule = await _queryService.GetSubModuleByUrl(url);
                var subModuleField = await _queryService.GetSubModuleFields(subModule.Id);
                model = _mapper.Map<List<FieldViewModel>>(subModuleField);

                //todo: hard coded, rework
                if((Status)status == Status.Archival)
                {
                    FieldViewModel retention = new FieldViewModel();
                    retention.FieldItem = null;
                    retention.Id = "71";
                    retention.IsDefault = true;
                    retention.IsRequired = true;
                    retention.Name = "Due Date";
                    retention.Type = FieldType.TextField;
                    retention.TypeName = null;
                    retention.Value = null;
                    model.Insert(0, retention);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error while retrieving out a fields. {0} - {1}", e.Message, e.InnerException);
            }

            return Json(new
            {
                data = model
            });
        }

        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> GetDocuments()
        {
            Dictionary<string, int> datasetTableFieldsId = new Dictionary<string, int>
            {
                {"DataOwner", 4 },
                {"Storage", 13 },
                {"PurposeOfUse", 18 },
                {"OutsideSingapore", 22 },
                {"RetentionPeriod", 30 },
                {"DisposalMethod", 33 }
            };
            var model = new List<DatasetTableViewModel>();
            try
            {
                string userRole = HttpContext.Session.GetString(SessionHelper.ROLES);
                string userDept = HttpContext.Session.GetString(SessionHelper.USER_DEPT);
                string userId = HttpContext.Session.GetString(SessionHelper.USER_ID);
                var datasets = await _queryService.GetAllDatasets(userRole, userDept, userId);
                if (datasets == null)
                    model = new List<DatasetTableViewModel>();
                else
                    foreach(DocumentDTO item in datasets)
                    {
                        DatasetTableViewModel tempModel = new DatasetTableViewModel();
                        tempModel.Id = item.Id.ToString();
                        tempModel.CreatedDate = Convert.ToDateTime(item.CreatedDate).ToString("dd-MM-yyyy");
                        tempModel.DataNumber = item.DataNumber;
                        if (!String.IsNullOrEmpty(item.DatasetId.ToString()) && item.DatasetId != 0)
                        {
                            var tempDName = await _queryService.GetDatasetById(item.DatasetId.ToString());
                            tempModel.DatasetName = (tempDName == null) ? "" : tempDName.Name;
                        }
                        tempModel.Department = (await _queryService.GetDepartmentById(item.DepartmentId)).Name;
                        tempModel.DataOwner = item.DocumentField.Where(w => w.FieldId == datasetTableFieldsId["DataOwner"]).Select(s => s.Value).ToList().FirstOrDefault().ToString();
                        tempModel.Storage = item.DocumentField.Where(w => w.FieldId == datasetTableFieldsId["Storage"]).Select(s => s.Value).ToList().FirstOrDefault().ToString();
                        tempModel.PurposeOfUse = item.DocumentField.Where(w => w.FieldId == datasetTableFieldsId["PurposeOfUse"]).Select(s => s.Value).ToList().FirstOrDefault().ToString();
                        tempModel.OutsideSingapore = item.DocumentField.Where(w => w.FieldId == datasetTableFieldsId["OutsideSingapore"]).Select(s => s.Value).ToList().FirstOrDefault().ToString();
                        tempModel.RetentionPeriod = item.DocumentField.Where(w => w.FieldId == datasetTableFieldsId["RetentionPeriod"]).Select(s => s.Value).ToList().FirstOrDefault().ToString();
                        tempModel.DisposalMethod = item.DocumentField.Where(w => w.FieldId == datasetTableFieldsId["DisposalMethod"]).Select(s => s.Value).ToList().FirstOrDefault().ToString();

                        model.Add(tempModel);
                    }
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
                    foreach(var field in fields)
                    {
                        var docField = model.DocumentField.Where(x => x.FieldId == (Convert.ToInt32(field.Id)) && x.SubModuleId == (Convert.ToInt32(subModule.Id))).Select(x => x.Value);
                        if(docField.Count() > 0)
                        {
                            field.Value = docField.FirstOrDefault();
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
                else if(subModule.Name.Contains(nameof(Status.Disposal)))
                    model.Status = Status.Disposal;
                else if(subModule.Name.Contains(nameof(Status.Storage)))
                    model.Status = Status.Storage;
                else if(subModule.Name.Contains(nameof(Status.Transfer)))
                    model.Status = Status.Transfer;
                else if(subModule.Name.Contains(nameof(Status.Usage)))
                    model.Status = Status.Usage;

                model.Datasets = new List<DatasetViewModel>();
            }
            catch (Exception e)
            {
                _logger.LogError("Error Exception on ConstructDocument: " + e.Message.ToString());
            }
            return model;
        }

        public async Task<IActionResult> DownloadAttachment(string id)
        {
            var docField = await _queryService.GetDocumentFieldById(id);
            if (!String.IsNullOrEmpty(docField.FilePath))
            {
                var path = docField.FilePath;

                var contentDisposition = new Microsoft.Net.Http.Headers.ContentDispositionHeaderValue("attachment") { FileName = docField.Value };
                Response.Headers[HeaderNames.ContentDisposition] = contentDisposition.ToString();

                var response = new FileStreamResult(new FileStream(path, FileMode.Open), "application/octet-stream");
                return response;
            }
            return Json("File not found");
        }

        [HttpGet]
        [Consumes("application/json")]
        public async Task<IActionResult> GetFieldById(string id)
        {
            var suggestions = await _queryService.GetFieldItemsById(id);
            var fieldList = suggestions.Select(x => x).ToList();

            return Json(new
            {
                data = fieldList
            });
        }

        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> UploadExcelFile(string bulkDeptId, string bulkDataSetId)
        {
            Result result = new Result();
            try
            {
                if(bulkDeptId == null)
                {
                    bulkDeptId = HttpContext.Session.GetString(SessionHelper.USER_DEPT) ?? "0";
                }

                if (bulkDataSetId == null)
                {
                    bulkDataSetId = "0";
                }

                string rootFolder = _hostingEnvironment.WebRootPath;
                var currentUser = HttpContext.Session.GetString(SessionHelper.USER_NAME);
                string UserRole = HttpContext.Session.GetString(SessionHelper.ROLES);
                string userId = HttpContext.Session.GetString(SessionHelper.USER_ID);
                string userDept = (bulkDeptId == "0") ? HttpContext.Session.GetString(SessionHelper.USER_DEPT) : bulkDeptId;

                var newFileName = string.Empty;

                if (HttpContext.Request.Form.Files != null)
                {
                    var fileName = string.Empty;
                    string PathDB = string.Empty;

                    var files = HttpContext.Request.Form.Files;

                    foreach (var file in files)
                    {
                        if (file.Length > 0)
                        {
                            //Getting FileName
                            fileName = System.Net.Http.Headers.ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');

                            //Assigning Unique Filename (Guid)
                            var myUniqueFileName = Convert.ToString(Guid.NewGuid());

                            var tempFolder = "tempFolder";

                            //Getting file Extension
                            var FileExtension = Path.GetExtension(fileName);

                            // concating  FileName + FileExtension
                            newFileName = myUniqueFileName + FileExtension;

                            // Combines two strings into a path.
                            if(!Directory.Exists(Path.Combine(_hostingEnvironment.WebRootPath, tempFolder)))
                            {
                                Directory.CreateDirectory(Path.Combine(_hostingEnvironment.WebRootPath, tempFolder));
                            }

                            fileName = Path.Combine(_hostingEnvironment.WebRootPath, tempFolder) + $@"\{newFileName}";

                            //Getting file Extension
                            FileExtension = Path.GetExtension(fileName);

                            // if you want to store path of folder in database
                            PathDB = tempFolder + "/" + newFileName;

                            using (FileStream fs = System.IO.File.Create(fileName))
                            {
                                file.CopyTo(fs);
                                fs.Flush();
                            }

                            if (FileExtension == ".xls" || FileExtension == ".xlsx")
                            {
                                result = await _adminService.BulkUploadXlsOrXlsx(myUniqueFileName, tempFolder, FileExtension, currentUser, rootFolder, UserRole, userId, userDept);
                            }

                            else if (FileExtension == ".csv")
                            {
                                result = await _adminService.BulkUploadCsv(myUniqueFileName, tempFolder, FileExtension, currentUser, rootFolder, UserRole, userId, userDept, bulkDeptId, bulkDataSetId);
                            }
                        }
                    }
                    if (System.IO.File.Exists(fileName))
                    {
                        System.IO.File.Delete(@fileName);
                    }
                }
                //TO DO: change to json result
                if (result.Success)
                {
                    result.Message = "Data sets have been successfully uploaded.";
                }
                TempData["bulkUploadMessage"] = result.Message;
                TempData["isSuccess"] = result.Success ? "1" : "0";
                return RedirectToAction("DataList", "Dataset");
            }
            catch (Exception e)
            {
                TempData["bulkUploadMessage"] = e.Message;
                TempData["isSuccess"] = "0";
                return RedirectToAction("DataList", "Dataset");
            }
            
        }

        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> DeleteDataSet(string docId)
        {
            Result result = new Result();
            try
            {
                string userRole = HttpContext.Session.GetString(SessionHelper.ROLES);
                string userId = HttpContext.Session.GetString(SessionHelper.USER_ID);
                string userDept = HttpContext.Session.GetString(SessionHelper.USER_DEPT);
                result = await _adminService.DeleteDataSet(userRole, userId, userDept, docId);
                //redirect to user list after update
                if (result.Success)
                {
                    result.IsRedirect = true;
                    result.RedirectUrl = "Dataset/DataList";
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error Exception on DeleteDataSet: " + e.Message.ToString());
            }

            return Json(result);
        }

        [HttpGet]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> Data(string id, string datasetId, string modalDepartmentId)
        {
            var model1 = new List<DocumentViewModel>();

            try
            {
                List<DepartmentDTO> departmentList = await _queryService.GetDepartments();
                ViewData["departmentList"] = departmentList;
                for (int i = 2; i <= 7; i++)
                {
                    var model = new DocumentViewModel();
                    var subModuleField = await _queryService.GetSubModuleFields(i.ToString());
                    var fields = _mapper.Map<List<FieldViewModel>>(subModuleField);
                    var datasetFields = new List<FieldDatasetViewModel>();
                    model.FieldDataset = new List<FieldDatasetViewModel>();

                    if (!String.IsNullOrEmpty(id))
                    {
                        var document = await _queryService.GetDocumentById(id);
                        model = _mapper.Map<DocumentViewModel>(document);
                        foreach (var field in fields)
                        {
                            var docField = model.DocumentField.Where(x => x.FieldId == (Convert.ToInt32(field.Id)) && x.SubModuleId == (Convert.ToInt32(i.ToString())));
                            if (docField.Count() > 0)
                            {
                                if (docField.First().IsEdited)
                                {
                                    field.Value = docField.First().NewValue;
                                }else
                                {
                                    field.Value = docField.First().Value;
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
                    model.SubModuleId = i.ToString();
                    model.FieldDataset = datasetFields;

                    if (i == 2)
                        model.Status = Status.Collection;
                    else if (i == 3)
                        model.Status = Status.Storage;
                    else if (i == 4)
                        model.Status = Status.Usage;
                    else if (i == 5)
                        model.Status = Status.Transfer;
                    else if (i == 6)
                        model.Status = Status.Archival;
                    else if (i == 7)
                        model.Status = Status.Disposal;

                    model.Datasets = new List<DatasetViewModel>();

                    if (!String.IsNullOrEmpty(model.DatasetId.ToString()) && model.DatasetId > 0)
                    {
                        datasetId = model.DatasetId.ToString();
                    }

                    if (!String.IsNullOrEmpty(datasetId))
                    {
                        var datasetDtl = await _queryService.GetDatasetById(datasetId);
                        if (datasetDtl != null)
                        {
                            model.DatasetName = datasetDtl.Name;
                            model.DatasetId = Convert.ToInt32(datasetId);
                            if (model.FieldDataset.Count() == 0)
                            {
                                var datasetFields1 = await _queryService.GetCurrentField(datasetId);
                                model.FieldDataset = _mapper.Map<List<FieldDatasetViewModel>>(datasetFields1);
                            }
                        }
                    }
                    var tempDatasets = await _queryService.GetDataset();
                    var datasets = _mapper.Map<List<DatasetViewModel>>(tempDatasets);
                    if (datasets == null)
                    {
                        datasets = new List<DatasetViewModel>();
                    }
                    model.Datasets = datasets;
                    model.DepartmentId = modalDepartmentId ?? "0";
                    model1.Add(model);
                    //For rework
                    if(id != "0")
                    {
                        var userId = HttpContext.Session.GetString(SessionHelper.USER_ID);
                        var document = await _queryService.GetRequestDocument(id);
                        var title = model.Status.ToString();
                        ViewData["Title"] = title;
                        ViewData["statusName"] = title.ToLower();
                        ViewData["userAction"] = "View";
                        if ((userId == document.createdBy) && (model.State == State.Rework))
                        {
                            ViewData["userAction"] = "Edit";
                        }
                        else if ((userId != document.createdBy) && (model.State == State.Pending))
                        {
                            ViewData["userAction"] = "Process";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error Exception on ConstructDocument: " + e.Message.ToString());
            }
            return View(model1);
        }

        [HttpGet]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public IActionResult Issues(string id)
        {
            var model = new List<DatasetViewModel>();
            try
            {
                return View();
            }

            catch (Exception e)
            {
                _logger.LogError("Error Exception on DataList: " + e.Message.ToString());
            }

            return View();
        }

        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> AddIssue(IssuesViewModel model)
        {
            Result result = new Result();
            try
            {
                var userDept = HttpContext.Session.GetString(SessionHelper.USER_DEPT);
                var userId = HttpContext.Session.GetString(SessionHelper.USER_ID);
                var userRole = HttpContext.Session.GetString(SessionHelper.ROLES);
                var issue = _mapper.Map<IssueDTO>(model);
                result = await _datasetService.AddIssue(issue, userId, userDept, userRole);

            }
            catch (Exception e)
            {
                _logger.LogError("Error Exception on DeleteDataSet: " + e.Message.ToString());
            }
            return new EmptyResult();
        }

        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> GetIssues(string id)
        {
            var model = new List<IssuesViewModel>();
            try
            {
                var issues = await _queryService.GetIssues(id);
                if (issues == null)
                    model = new List<IssuesViewModel>();
                else
                    model = _mapper.Map<List<IssuesViewModel>>(issues);
            }
            catch (Exception e)
            {
                _logger.LogError("Error Exception on GetIssues: " + e.Message.ToString());
            }

            return Json(new
            {
                data = model
            });
        }

        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> GetDatasets()
        {
            var model = new List<DatasetViewModel>();
            try
            {
                var datasets = await _queryService.GetDataset();
                if (datasets == null)
                    model = new List<DatasetViewModel>();
                else
                    model = _mapper.Map<List<DatasetViewModel>>(datasets);
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

        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> GetDatasetViewFields(string docId, string subModuleId)
        {
            var model = new List<DocumentFieldViewModel>();
            try
            {
                var documentFields = await _queryService.GetSDocumentFieldsBySubModule(docId, subModuleId);
                if (documentFields == null)
                    model = new List<DocumentFieldViewModel>();
                else
                    model = _mapper.Map<List<DocumentFieldViewModel>>(documentFields);
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

        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> GetDatasetTemplateView(string docId)
        {
            var model = new List<FieldDatasetViewModel>();
            try
            {
                var datasetId = (await _queryService.GetDocumentById(docId)).DatasetId;
                if(datasetId != 0)
                {
                    var tempAvailFields = await _queryService.GetCurrentField(datasetId.ToString());
                    var availFields = _mapper.Map<List<FieldDatasetViewModel>>(tempAvailFields);
                    if (availFields == null)
                        model = new List<FieldDatasetViewModel>();
                    else
                        model = _mapper.Map<List<FieldDatasetViewModel>>(availFields);
                    if (availFields == null)
                    {
                        availFields = new List<FieldDatasetViewModel>();
                    }
                }
                return Json(new
                {
                    data = model
                });
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> EditIssue(IssuesViewModel model)
        {
            Result result = new Result();
            try
            {
                var userDept = HttpContext.Session.GetString(SessionHelper.USER_DEPT);
                var userId = HttpContext.Session.GetString(SessionHelper.USER_ID);
                var issue = _mapper.Map<IssueDTO>(model);
                result = await _datasetService.EditIssue(issue, userId, userDept);
                result.Id = "EditIssue";
            }
            catch (Exception e)
            {
                _logger.LogError("Error Exception on DeleteDataSet: " + e.Message.ToString());
            }

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetFieldItems(string id, string value, string selected)
        {
            var suggestions = await _queryService.GetFieldItems(id, value, selected);
            var fieldList = suggestions.Select(x => x).ToList();

            return Json(new
            {
                data = fieldList
            });
        }
    }
}
