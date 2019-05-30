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
using OfficeOpenXml;
using System.Text;
using System.Linq;

namespace DDPA.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly SignInManager<ExtendedIdentityUser> _signInManager;
        private readonly UserManager<ExtendedIdentityUser> _userManager;
        private readonly ILogger _logger;
        private readonly ISummaryService _summaryService;
        private readonly IMapper _mapper;
        private readonly IQueryService _queryService;

        public DashboardController(SignInManager<ExtendedIdentityUser> signInManager, UserManager<ExtendedIdentityUser> userManager,
            ILogger<AccountController> logger, IQueryService queryService, ISummaryService summaryService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _summaryService = summaryService;
            _queryService = queryService;
            _mapper = this.GetMapper();
        }

        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> Index()
        {
            string userId = HttpContext.Session.GetString(SessionHelper.USER_ID);
            string userRole = HttpContext.Session.GetString(SessionHelper.ROLES);
            string userDept = HttpContext.Session.GetString(SessionHelper.USER_DEPT);

            if (userRole == nameof(Role.DPO) || userRole == nameof(Role.ADMINISTRATOR))
            {
                ViewData["userDept"] = "";
            }
            else if (userRole != nameof(Role.DPO))
            {
                var department = await _queryService.GetDepartmentById(userDept);
                ViewData["userDept"] = department.Name;
            }
            HttpContext.Session.SetString(SessionHelper.SHOW_MODAL, "0");
            return View();
        }

        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> GetSummary()
        {
            DashboardSummaryViewModel dashboardSummary = new DashboardSummaryViewModel();
            try
            {
                string userId = HttpContext.Session.GetString(SessionHelper.USER_ID);
                string userRole = HttpContext.Session.GetString(SessionHelper.ROLES);
                string userDept = HttpContext.Session.GetString(SessionHelper.USER_DEPT);

                //todo: make method GetSummaryOfField(fieldId, userRole, userDept) instead of multiple methods for each field
                dashboardSummary.DataSet = _mapper.Map<SummaryItemViewModel>(await _queryService.GetSummaryOfDataSet(userRole, userDept));
                dashboardSummary.Storage = _mapper.Map<SummaryItemViewModel>(await _queryService.GetSummaryOfStorage(userRole, userDept));
                dashboardSummary.ExternalParty = _mapper.Map<SummaryItemViewModel>(await _queryService.GetSummaryOfExternalParty(userRole, userDept));
                dashboardSummary.Issue = _mapper.Map<SummaryItemViewModel>(await _queryService.GetSummaryOfDatasetIssue(userRole, userDept));
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling GetSummary: {0}", e.Message);
            }

            return Json(new
            {
                data = dashboardSummary
            });
        }

        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> SummaryOfDatasetToCSV()
        {
            try
            {
                var date = DateTime.Now.ToString("dd MMM yyyy");
                string userRole = HttpContext.Session.GetString(SessionHelper.ROLES);
                string userDept = HttpContext.Session.GetString(SessionHelper.USER_DEPT);
                string companyName = HttpContext.Session.GetString(SessionHelper.COMPANY_NAME);

                //Initialization of this should be sorted ascending
                List<int> CSVFieldsId = new List<int>
            {
                3, //Collection Purpose
                4, //Data Owner
                5, //Collection Source
                6, //Collection Medium
                13, //Storage
                14, //Define Storage
                18, //Purpose of Use
                19, //User of Data
                29, //Storage
                30 //Retention Period in Months
            };
                CSVFieldsId.Sort();

                //var documents = await _queryService.GetDocumentsByRole(userRole, userDept);
                var documents = (userRole == nameof(Role.DPO) || userRole == nameof(Role.ADMINISTRATOR)) ? await _queryService.GetApprovedDocuments() : await _queryService.GetApprovedDocumentsByDepartment(userDept);
                documents = documents.OrderBy(o => o.DepartmentId).ThenBy(t => t.DataNumber).ToList();

                StringBuilder sb = new StringBuilder();
                string rowData = "";

                //company name
                AppendRowData(ref rowData, companyName);
                sb.AppendLine(rowData);
                rowData = "";

                //csv header
                AppendRowData(ref rowData, "Data Set Owners");
                sb.AppendLine(rowData);
                rowData = "";

                //add report date
                AppendRowData(ref rowData, "Report Date: " + DateTime.Now.ToString("dd MMM yyyy"));
                sb.AppendLine(rowData);
                rowData = "";

                AppendRowData(ref rowData, "Data Set Name");
                AppendRowData(ref rowData, "Department");
                AppendRowData(ref rowData, "Data Sets");

                foreach (int fieldId in CSVFieldsId)
                {
                    var field = await _queryService.GetFieldById(fieldId.ToString());
                    AppendRowData(ref rowData, field.Name);
                }
                sb.AppendLine(rowData);

                if(documents.Count == 0)
                {
                    return File(Encoding.ASCII.GetBytes(sb.ToString()), "text/csv", "DATASET SUMMARY " + date + ".csv");
                }

                //loop through each document
                foreach (DocumentDTO doc in documents)
                {
                    //set rowData to blank. Writing in new line
                    rowData = "";

                    //Set row's Dataset Name field
                    if (!String.IsNullOrEmpty(doc.DatasetId.ToString()))
                    {
                        var datasetName = await _queryService.GetDatasetById(doc.DatasetId.ToString());
                        if (datasetName != null)
                            AppendRowData(ref rowData, datasetName.Name);
                        else
                            AppendRowData(ref rowData, "");
                    }
                    else
                    {
                        AppendRowData(ref rowData, "");
                    }

                    //Set row's Department field
                    if (userRole == nameof(Role.DPO) || userRole == nameof(Role.ADMINISTRATOR))
                    {
                        var department = await _queryService.GetDepartmentById(doc.DepartmentId);
                        AppendRowData(ref rowData, department.Name);
                    }

                    else if (userRole == nameof(Role.USER) || userRole == nameof(Role.DEPTHEAD))
                    {
                        var department = await _queryService.GetDepartmentById(userDept);
                        AppendRowData(ref rowData, department.Name);
                    }

                    //todo: data sets fields
                    if(doc.DatasetId == 0)
                    {
                        AppendRowData(ref rowData, "NO DATASET");
                    }
                    else if (doc.DatasetId != 0)
                    {
                        var datasetFields = await _queryService.GetCurrentField(doc.DatasetId.ToString());
                        
                        if (datasetFields.Count == 0)
                        {
                            AppendRowData(ref rowData, "");
                        }
                        else if (datasetFields.Count != 0)
                        {
                            string tempFields = (await _queryService.GetDatasetById(doc.DatasetId.ToString())).Name + "- ";
                            
                            foreach(DatasetFieldDTO item in datasetFields)
                            {
                                tempFields += item.Field.Name + ", ";
                            }

                            tempFields = tempFields.Remove(tempFields.LastIndexOf(','));
                            AppendRowData(ref rowData, tempFields);
                        }
                    }

                    //loop through each document's field
                    foreach (DocumentFieldDTO field in doc.DocumentField)
                    {
                        if (CSVFieldsId.Contains(field.FieldId))
                        {
                            AppendRowData(ref rowData, field.Value);
                        }
                    }
                    sb.AppendLine(rowData);
                }

                return File(Encoding.ASCII.GetBytes(sb.ToString()), "text/csv", "DATASET SUMMARY " + date + ".csv");
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void AppendRowData(ref string rowData, string data)
        {
            data = "\"" + data + "\"";
            if (rowData == "")
            {
                rowData = data;
            }
            else
            {
                rowData += "," + data;
            }
        }

        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> SummaryOfStorageToCSV(string id)
        {
            try
            {
                var date = DateTime.Now.ToString("dd MMM yyyy");
                string userRole = HttpContext.Session.GetString(SessionHelper.ROLES);
                string userDept = HttpContext.Session.GetString(SessionHelper.USER_DEPT);
                string companyName = HttpContext.Session.GetString(SessionHelper.COMPANY_NAME);

                //Initialization of this should be sorted ascending
                List<int> CSVFieldsId = new List<int>
            {
                10, //OnOffSite
                11, //External Party
                12, //External Party Address
                13, //Storage
                14, //Define Storage
            };
                CSVFieldsId.Sort();

                var documents = (userRole == nameof(Role.DPO) || userRole == nameof(Role.ADMINISTRATOR)) ? await _queryService.GetApprovedDocuments() : await _queryService.GetApprovedDocumentsByDepartment(userDept);
                //todo sort

                StringBuilder sb = new StringBuilder();
                string rowData = "";

                //company name
                AppendRowData(ref rowData, companyName);
                sb.AppendLine(rowData);
                rowData = "";

                //csv header
                AppendRowData(ref rowData, "Storage");
                sb.AppendLine(rowData);
                rowData = "";

                //add report date
                AppendRowData(ref rowData, "Report Date: " + DateTime.Now.ToString("dd MMM yyyy"));
                sb.AppendLine(rowData);
                rowData = "";

                AppendRowData(ref rowData, "Data Set Name");
                AppendRowData(ref rowData, "Department");

                //writing column headers
                foreach (int fieldId in CSVFieldsId)
                {
                    var field = await _queryService.GetFieldById(fieldId.ToString());
                    AppendRowData(ref rowData, field.Name);
                }
                sb.AppendLine(rowData);

                if(documents.Count == 0)
                {
                    return File(Encoding.ASCII.GetBytes(sb.ToString()), "text/csv", "STORAGE SUMMARY " + date + ".csv");
                }

                //loop through each document
                foreach (DocumentDTO doc in documents)
                {
                    //set rowData to blank. Writing in new line
                    rowData = "";

                    //Set row's Dataset Name field
                    if (!String.IsNullOrEmpty(doc.DatasetId.ToString()))
                    {
                        var datasetName = await _queryService.GetDatasetById(doc.DatasetId.ToString());
                        if (datasetName != null)
                            AppendRowData(ref rowData, datasetName.Name);
                        else
                            AppendRowData(ref rowData, "");
                    }
                    else
                    {
                        AppendRowData(ref rowData, "");
                    }

                    //Set row's Department field
                    if (userRole == nameof(Role.DPO) || userRole == nameof(Role.ADMINISTRATOR))
                    {
                        var department = await _queryService.GetDepartmentById(doc.DepartmentId);
                        AppendRowData(ref rowData, department.Name);
                    }

                    else if (userRole == nameof(Role.USER) || userRole == nameof(Role.DEPTHEAD))
                    {
                        var department = await _queryService.GetDepartmentById(userDept);
                        AppendRowData(ref rowData, department.Name);
                    }

                    //loop through each document's field
                    foreach (DocumentFieldDTO field in doc.DocumentField)
                    {
                        if (CSVFieldsId.Contains(field.FieldId))
                        {
                            AppendRowData(ref rowData, field.Value);
                        }
                    }
                    sb.AppendLine(rowData);
                }

                return File(Encoding.ASCII.GetBytes(sb.ToString()), "text/csv", "STORAGE SUMMARY " + date + ".csv");
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> SummaryOfExternalPartyToCSV(string id)
        {
            try
            {
                var date = DateTime.Now.ToString("dd MMM yyyy");
                string userRole = HttpContext.Session.GetString(SessionHelper.ROLES);
                string userDept = HttpContext.Session.GetString(SessionHelper.USER_DEPT);
                string companyName = HttpContext.Session.GetString(SessionHelper.COMPANY_NAME);
                List<int> CSVFieldsId = new List<int>
                {
                    23, //Transferred To
                    24, //External Party
                    25, //Purpose of Transfer
                    26, //Mode of Transfer
                };
                CSVFieldsId.Sort();
                
                var documents = (userRole == nameof(Role.DPO) || userRole == nameof(Role.ADMINISTRATOR)) ? await _queryService.GetApprovedDocuments() : await _queryService.GetApprovedDocumentsByDepartment(userDept);

                StringBuilder sb = new StringBuilder();
                string rowData = "";

                //company name
                AppendRowData(ref rowData, companyName);
                sb.AppendLine(rowData);
                rowData = "";

                //csv header
                AppendRowData(ref rowData, "External Parties");
                sb.AppendLine(rowData);
                rowData = "";

                //add report date
                AppendRowData(ref rowData, "Report Date: " + DateTime.Now.ToString("dd MMM yyyy"));
                sb.AppendLine(rowData);
                rowData = "";

                AppendRowData(ref rowData, "Data Set Name");
                AppendRowData(ref rowData, "Department");

                //writing column headers
                foreach (int fieldId in CSVFieldsId)
                {
                    var field = await _queryService.GetFieldById(fieldId.ToString());
                    AppendRowData(ref rowData, field.Name);
                }
                sb.AppendLine(rowData);

                if(documents.Count == 0)
                {
                    return File(Encoding.ASCII.GetBytes(sb.ToString()), "text/csv", "EXTERNAL PARTY SUMMARY " + date + ".csv");
                }

                //loop through each document
                foreach (DocumentDTO doc in documents)
                {
                    //set rowData to blank. Writing in new line
                    rowData = "";

                    //Set row's Dataset Name field
                    if (!String.IsNullOrEmpty(doc.DatasetId.ToString()))
                    {
                        var datasetName = await _queryService.GetDatasetById(doc.DatasetId.ToString());
                        if (datasetName != null)
                            AppendRowData(ref rowData, datasetName.Name);
                        else
                            AppendRowData(ref rowData, "");
                    }
                    else
                    {
                        AppendRowData(ref rowData, "");
                    }
                    //Set row's Department field
                    if (userRole == nameof(Role.DPO) || userRole == nameof(Role.ADMINISTRATOR))
                    {
                        var department = await _queryService.GetDepartmentById(doc.DepartmentId);
                        AppendRowData(ref rowData, department.Name);
                    }

                    else if (userRole == nameof(Role.USER) || userRole == nameof(Role.DEPTHEAD))
                    {
                        var department = await _queryService.GetDepartmentById(userDept);
                        AppendRowData(ref rowData, department.Name);
                    }

                    //loop through each document's field
                    foreach (DocumentFieldDTO field in doc.DocumentField)
                    {
                        if (CSVFieldsId.Contains(field.FieldId))
                        {
                            AppendRowData(ref rowData, field.Value);
                        }
                    }
                    sb.AppendLine(rowData);
                }

                return File(Encoding.ASCII.GetBytes(sb.ToString()), "text/csv", "EXTERNAL PARTY SUMMARY " + date + ".csv");
            }
            catch (Exception)
            {

                throw;
            }
        }

        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> SummaryOfIssueRiskToCSV()
        {
            try
            {
                var date = DateTime.Now.ToString("dd MMM yyyy");
                string userRole = HttpContext.Session.GetString(SessionHelper.ROLES);
                string userDept = HttpContext.Session.GetString(SessionHelper.USER_DEPT);
                string companyName = HttpContext.Session.GetString(SessionHelper.COMPANY_NAME);

                var issues = await _queryService.GetIssuesByRole(userRole, userDept);
                
                if (userRole == nameof(Role.DPO) || userRole == nameof(Role.ADMINISTRATOR))
                {
                    issues = issues.OrderBy(o => o.DepartmentId).ThenBy(t => t.DataNumber).ToList();
                }
                else if (userRole == nameof(Role.DEPTHEAD) || userRole == nameof(Role.USER))
                {
                    issues = issues.OrderBy(o => o.DepartmentId).ToList();
                }

                StringBuilder sb = new StringBuilder();
                string rowData = "";

                //company name
                AppendRowData(ref rowData, companyName);
                sb.AppendLine(rowData);
                rowData = "";

                //csv header
                AppendRowData(ref rowData, "Notes");
                sb.AppendLine(rowData);
                rowData = "";

                //add report date
                AppendRowData(ref rowData, "Report Date: " + DateTime.Now.ToString("dd MMM yyyy"));
                sb.AppendLine(rowData);
                rowData = "";

                //Column headers
                AppendRowData(ref rowData, "Data Set Name");
                AppendRowData(ref rowData, "Department");
                AppendRowData(ref rowData, "Description");
                AppendRowData(ref rowData, "Date");
                AppendRowData(ref rowData, "Assigned To");
                AppendRowData(ref rowData, "Action");
                AppendRowData(ref rowData, "Status");
                sb.AppendLine(rowData);

                if(issues.Count == 0)
                {
                    return File(Encoding.ASCII.GetBytes(sb.ToString()), "text/csv", "NOTES SUMMARY " + date + ".csv");
                }

                //loop through each issues
                foreach (IssueDTO item in issues)
                {
                    //set rowData to blank. Writing in new line
                    rowData = "";
                    AppendRowData(ref rowData, item.DatasetName);
                    AppendRowData(ref rowData, item.Department);
                    AppendRowData(ref rowData, item.Description);
                    AppendRowData(ref rowData, (item.Date).Value.ToString("dd MMM yyyy"));
                    AppendRowData(ref rowData, item.AssignedTo);
                    AppendRowData(ref rowData, item.Action);
                    AppendRowData(ref rowData, item.Status);
                    
                    sb.AppendLine(rowData);
                }

                return File(Encoding.ASCII.GetBytes(sb.ToString()), "text/csv", "NOTES SUMMARY " + date + ".csv");                                
            }
            catch (Exception)
            {

                throw;
            }
        }

        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> SummaryToCSV()
        {
            try
            {
                var date = DateTime.Now.ToString("dd MMM yyyy");
                string userRole = HttpContext.Session.GetString(SessionHelper.ROLES);
                string userDept = HttpContext.Session.GetString(SessionHelper.USER_DEPT);
                string companyName = HttpContext.Session.GetString(SessionHelper.COMPANY_NAME);
                
                var documents = (userRole == nameof(Role.DPO) || userRole == nameof(Role.ADMINISTRATOR)) ? await _queryService.GetApprovedDocuments() : await _queryService.GetApprovedDocumentsByDepartment(userDept);
                documents = documents.OrderBy(o => o.DepartmentId).ThenBy(t => t.DataNumber).ToList();

                foreach (DocumentDTO doc in documents)
                {
                    doc.DocumentField = doc.DocumentField.ToList().OrderBy(o => o.SubModuleId).ToList();
                }
                StringBuilder sb = new StringBuilder();
                string rowData = "";

                //company name
                AppendRowData(ref rowData, companyName);
                sb.AppendLine(rowData);
                rowData = "";

                //csv header
                AppendRowData(ref rowData, "Summary Report");
                sb.AppendLine(rowData);
                rowData = "";

                //add report date
                AppendRowData(ref rowData, "Report Date: " + DateTime.Now.ToString("dd MMM yyyy"));
                sb.AppendLine(rowData);
                rowData = "";

                AppendRowData(ref rowData, "Data Set Name");
                AppendRowData(ref rowData, "Department");
                AppendRowData(ref rowData, "Data Sets");
                //todo: add dataset fields
                
                if (documents.Count == 0)
                {
                    return File(Encoding.ASCII.GetBytes(sb.ToString()), "text/csv", "DATASET SUMMARY " + date + ".csv");
                }

                foreach (DocumentFieldDTO field in documents[0].DocumentField)
                {
                    AppendRowData(ref rowData, field.Field.Name);
                }
                sb.AppendLine(rowData);

                //loop through each document
                foreach (DocumentDTO doc in documents)
                {
                    //set rowData to blank. Writing in new line
                    rowData = "";

                    //Set row's Dataset Name field
                    if (!String.IsNullOrEmpty(doc.DatasetId.ToString()))
                    {
                        var datasetName = await _queryService.GetDatasetById(doc.DatasetId.ToString());
                        if (datasetName != null)
                        {
                            AppendRowData(ref rowData, datasetName.Name);
                        }
                        else
                        {
                            AppendRowData(ref rowData, "");
                        }
                    }
                    else
                    {
                        AppendRowData(ref rowData, "");
                    }

                    //Set row's Department field
                    if (userRole == nameof(Role.DPO) || userRole == nameof(Role.ADMINISTRATOR))
                    {
                        var department = await _queryService.GetDepartmentById(doc.DepartmentId);
                        AppendRowData(ref rowData, department.Name);
                    }

                    else if (userRole == nameof(Role.USER) || userRole == nameof(Role.DEPTHEAD))
                    {
                        var department = await _queryService.GetDepartmentById(userDept);
                        AppendRowData(ref rowData, department.Name);
                    }

                    //todo: data sets fields
                    if (doc.DatasetId == 0)
                    {
                        AppendRowData(ref rowData, "NO DATASET");
                    }
                    else if (doc.DatasetId != 0)
                    {
                        var datasetFields = await _queryService.GetCurrentField(doc.DatasetId.ToString());

                        if (datasetFields.Count != 0)
                        {
                            string tempFields = (await _queryService.GetDatasetById(doc.DatasetId.ToString())).Name + "- ";

                            foreach (DatasetFieldDTO item in datasetFields)
                            {
                                tempFields += item.Field.Name + ", ";
                            }
                            
                            AppendRowData(ref rowData, tempFields);
                        }
                        else if(datasetFields.Count == 0)
                        {
                            AppendRowData(ref rowData, "NO DATA");
                        }
                    }

                    string valueHolder = "";
                    foreach(DocumentFieldDTO field in documents[0].DocumentField)
                    {
                        //if current sub module field exist in doc's sub module field
                        if(doc.DocumentField.Any(i => i.FieldId == field.FieldId))
                        {
                            valueHolder = doc.DocumentField.Where(w => w.FieldId == field.FieldId).Select(s => s.Value).FirstOrDefault();
                        }

                        else
                        {
                            valueHolder = "";
                        }
                        AppendRowData(ref rowData, valueHolder);
                    }
                    sb.AppendLine(rowData);
                }

                return File(Encoding.ASCII.GetBytes(sb.ToString()), "text/csv", "SUMMARY " + date + ".csv");
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
