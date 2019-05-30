using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DDPA.Attributes;
using DDPA.SQL.Entities;
using DDPA.Web.Models;

namespace DDPA.Web.Controllers
{
    public class HomeController : Controller
    {
        [ServiceFilter(typeof(SharedMessageAttribute))]
        public IActionResult Index()
        {
            return View();
        }
    }
}
