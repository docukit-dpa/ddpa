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
using MimeKit;
using Microsoft.Extensions.Options;

namespace DDPA.Web.Controllers
{
    public class SupportController : Controller
    {
        private readonly SignInManager<ExtendedIdentityUser> _signInManager;
        private readonly UserManager<ExtendedIdentityUser> _userManager;
        private readonly ILogger _logger;
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;
        private readonly IQueryService _queryService;
        private readonly ISupportService _supportService;
        private readonly IOptions<SMTPOptions> _SMTPOptions;

        public SupportController(SignInManager<ExtendedIdentityUser> signInManager, UserManager<ExtendedIdentityUser> userManager, 
            ILogger<SupportController> logger, IAccountService accountService, IQueryService queryService, ISupportService supportService,
            IOptions<SMTPOptions> SMTPOptions)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _accountService = accountService;
            _queryService = queryService;
            _mapper = this.GetMapper();
            _supportService = supportService;
            _SMTPOptions = SMTPOptions;
        }

        [HttpGet]
        [AllowAnonymous]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> SendEmail(SendMailViewModel model)
        {
            Result result = new Result();
            try
            {
                result = await _supportService.SendEmail(_mapper.Map<SendMailDTO>(model), _SMTPOptions);

                if(result.Success)
                {
                    result.IsRedirect = true;
                    result.RedirectUrl = "Support/Index";
                }

            }
            catch (Exception e)
            {
                _logger.LogError("Error Exception on SendEmail: " + e.Message.ToString());
            }

            return Json(result);
        }
    }
}
