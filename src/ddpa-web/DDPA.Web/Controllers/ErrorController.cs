using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DDPA.Attributes;
using DDPA.Service;
using DDPA.SQL.Entities;

namespace DDPA.Web.Controllers
{
    public class ErrorController : Controller
    {
        private readonly SignInManager<ExtendedIdentityUser> _signInManager;
        private readonly UserManager<ExtendedIdentityUser> _userManager;
        private readonly ILogger _logger;
        private readonly IAccountService _accountService;


        public ErrorController(SignInManager<ExtendedIdentityUser> signInManager, UserManager<ExtendedIdentityUser> userManager,
            ILogger<AccountController> logger, IAccountService accountService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _accountService = accountService;
        }

        [HttpGet]
        [AllowAnonymous]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public IActionResult Index()
        {
            return View();
        }

       }
}
