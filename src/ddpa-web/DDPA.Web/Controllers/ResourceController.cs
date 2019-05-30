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

namespace DDPA.Web.Controllers
{
    public class ResourceController : Controller
    {
        private readonly SignInManager<ExtendedIdentityUser> _signInManager;
        private readonly UserManager<ExtendedIdentityUser> _userManager;
        private readonly ILogger _logger;
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;
        private readonly IQueryService _queryService;
        private readonly IResourceService _resourceService;

        public ResourceController(SignInManager<ExtendedIdentityUser> signInManager, UserManager<ExtendedIdentityUser> userManager, 
            ILogger<AccountController> logger, IAccountService accountService, IQueryService queryService, IResourceService resourceService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _accountService = accountService;
            _queryService = queryService;
            _mapper = this.GetMapper();
            _resourceService = resourceService;
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
        public async Task<IActionResult> GetResources()
        {
            var model = new List<ResourceViewModel>();
            try
            {
                var resources = await _queryService.GetAllResources();
                if (resources == null)
                    model = new List<ResourceViewModel>();
                else
                    model = _mapper.Map<List<ResourceViewModel>>(resources);
            }
            catch (Exception e)
            {
                _logger.LogError("Error Exception on GetResources: " + e.Message.ToString());
            }

            return Json(new
            {
                data = model
            });
        }

        [HttpGet]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public IActionResult AddResource()
        {
            return View();
        }

        [HttpPost]
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public async Task<IActionResult> AddResource(AddResourceViewModel model, IFormFile file)
        {
            Result result = new Result();
            try
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                string userId = HttpContext.Session.GetString(SessionHelper.USER_ID);
                
                result = await _resourceService.CreateResource(_mapper.Map<AddResourceDTO>(model), userId, file);

                if (result.Success)
                {
                    return RedirectToAction("Index", "Resource");
                }

                else
                {
                    return View();
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error Exception on Adding a Resource: " + e.Message.ToString());
            }
            
            
            return Json(result);
        }
    }
}
