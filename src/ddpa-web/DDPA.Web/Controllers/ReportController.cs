using System;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using DDPA.Attributes;
using DDPA.Service;
using DDPA.SQL.Entities;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.Web.Controllers
{
    public class ReportController : Controller
    {
        private readonly SignInManager<ExtendedIdentityUser> _signInManager;
        private readonly UserManager<ExtendedIdentityUser> _userManager;
        private readonly ILogger _logger;
        private readonly IQueryService _queryService;

        public ReportController(SignInManager<ExtendedIdentityUser> signInManager, UserManager<ExtendedIdentityUser> userManager,
            ILogger<AccountController> logger, IQueryService queryService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _queryService = queryService;

        }

        [ServiceFilter(typeof(SharedMessageAttribute))]
        public IActionResult Index()
        {

            return View();
        }

        public async Task<IActionResult> ExportCSV(string status)

        {
            int[] array = new int[] { 1, 2, 3, 4, 6 };

            var comlumHeadrs = new StringBuilder();
            var comlumItems = new StringBuilder();
            comlumHeadrs.Append("No,Data Number,");
            foreach (int i in array)
            {
                var Headerfields = await _queryService.GetSubModuleFields(Convert.ToString(i));
                foreach (var subfields in Headerfields)
                {
                    comlumHeadrs.Append(subfields.Name + ",");
                }
            }
            int num = 1;
            var docs = await _queryService.GetAllDocuments();
            foreach (var doc in docs)
            {
                if (status == "Collection")
                {
                    {
                        comlumItems.Append(num + "," + doc.DataNumber + ",");
                        foreach (int i in array)
                        {
                            var submodulefields = await _queryService.GetSubModuleFields(Convert.ToString(i));

                            foreach (var subfields in submodulefields)
                            {

                                foreach (var docfield in doc.DocumentField)
                                {
                                    if ((Convert.ToString(docfield.FieldId) == subfields.Id) && (docfield.SubModuleId == i))
                                        comlumItems.Append(docfield.Value + ",");
                                }

                            }
                        }
                        comlumItems.AppendLine("");
                        num++;
                    }
                }
                else if (status == doc.Status.ToString())
                {
                    comlumItems.Append(num + "," + doc.DataNumber + ",");
                    foreach (int i in array)
                    {
                        var submodulefields = await _queryService.GetSubModuleFields(Convert.ToString(i));

                        foreach (var subfields in submodulefields)
                        {

                            foreach (var docfield in doc.DocumentField)
                            {
                                if ((Convert.ToString(docfield.FieldId) == subfields.Id) && (docfield.SubModuleId == i))
                                    comlumItems.Append(docfield.Value + ",");
                            }

                        }
                    }
                    comlumItems.AppendLine("");
                    num++;
                }
            }

            var FieldItems = new StringBuilder();

            byte[] buffer = Encoding.ASCII.GetBytes($"{string.Join(",", comlumHeadrs)}\r\n{comlumItems.ToString()}");
            return File(buffer, "text/csv", $"Report.csv");


        }
    }
}
