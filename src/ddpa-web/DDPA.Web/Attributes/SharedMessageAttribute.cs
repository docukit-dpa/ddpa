using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using DDPA.Commons.Helper;
using DDPA.Extensions;
using DDPA.SQL.Entities;
using DDPA.Web.Models;
using System.Collections.Generic;

namespace DDPA.Attributes
{
    public class SharedMessageAttribute : ActionFilterAttribute
    {
        public SharedMessageAttribute()
        {

        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            if (filterContext.HttpContext.Session.GetString(SessionHelper.USER_NAME) != null)
            {
                var controller = filterContext.Controller as Controller;
                if (controller != null)
                {
                    controller.ViewData.Add("Modules", filterContext.HttpContext.Session.GetObjectFromJson<List<ModuleViewModel>>(SessionHelper.MODULES));
                    controller.ViewData.Add("userRole", filterContext.HttpContext.Session.GetString(SessionHelper.ROLES));
                    controller.ViewData.Add("user", filterContext.HttpContext.Session.GetString(SessionHelper.USER_NAME));
                    controller.ViewData.Add("companyName", filterContext.HttpContext.Session.GetString(SessionHelper.COMPANY_NAME));

                    var user = filterContext.HttpContext.Session.GetObjectFromJson<ExtendedIdentityUser>(SessionHelper.USER);
                    if (user != null)
                    {
                        controller.ViewData.Add("FirstName", user.FirstName);
                        controller.ViewData.Add("LastName", user.LastName);
                        controller.ViewData.Add("UserRights", filterContext.HttpContext.Session.GetObjectFromJson<List<UserRightsViewModel>>(SessionHelper.USER_RIGHTS));

                    }
                    //Enable or Disable Modules
                    var LoginState = "";
                    if (filterContext.HttpContext.Session.GetString(SessionHelper.HASPASSWORDCHANGED) == "0")
                    {
                        LoginState = "changePass";
                    }
                    else
                    {
                        if (filterContext.HttpContext.Session.GetString(SessionHelper.DONE_SETUP) == "0")
                        {
                            LoginState = "userSetup";

                        }
                        else if (filterContext.HttpContext.Session.GetString(SessionHelper.DONE_SETUP) == "1")
                        {
                            LoginState = "userGuideInResource";
                        }
                        //to show modal for setup and usergudie once
                        if (filterContext.HttpContext.Session.GetString(SessionHelper.SHOW_MODAL) == "1")
                        {
                            controller.ViewData.Add("showModal", "1");
                        }
                        else
                        {
                            controller.ViewData.Add("showModal", "0");
                        }
                    }
                    controller.ViewData.Add("LoginState", LoginState);
                }
                //if the logged user is admin and haven't change his password yet
                string currentAction = controller.ControllerContext.RouteData.Values["action"].ToString();
                string currentController = controller.ControllerContext.RouteData.Values["controller"].ToString();

                if (filterContext.HttpContext.Session.GetString(SessionHelper.HASPASSWORDCHANGED) == "0")
                {
                    //if current url is .../Maintenance/ChangePasswordUser, do this to avoid loop
                    if (currentController == "Maintenance" && currentAction == "ChangePasswordUser")
                    {

                    }
                    else
                    {
                        filterContext.Result = new RedirectResult("~/Maintenance/ChangePasswordUser");
                    }
                    return;
                }
                else
                {
                    if (filterContext.HttpContext.Session.GetString(SessionHelper.DONE_SETUP) == "0")
                    {
                        if (currentController == "Maintenance")
                        {
                            
                        }
                        else
                        {
                            filterContext.Result = new RedirectResult("~/Maintenance/User");
                        }
                        return;
                    }
                }
                //Check UserRights
                var uright = (filterContext.HttpContext.Session.GetObjectFromJson<List<UserRightsViewModel>>(SessionHelper.USER_RIGHTS)).Find(x => x.ModuleName == currentController && x.View == 0);
                var urole = controller.ViewData["userRole"].ToString();
                if (uright != null && ( urole != "DPO" || urole == "ADMINISTRATOR"))
                {
                    var tempModule = uright.ModuleName;
                    if (currentController == tempModule)
                    {
                        filterContext.Result = new RedirectResult("~/Error/Index");
                        return;
                    }                                 
                }
                else
                {
                    var umodule = (filterContext.HttpContext.Session.GetObjectFromJson<List<ModuleViewModel>>(SessionHelper.MODULES)).Find(m => m.Name == currentController && m.SubModule.Count > 0); ;
                    if (umodule != null)
                    {
                        if(umodule.SubModule.Exists(sm => sm.Name == currentAction && (!sm.Roles.Contains(urole)))){
                            filterContext.Result = new RedirectResult("~/Error/Index");
                            return;
                        }
                    }
                }
            }
            else if(filterContext.HttpContext.Session.GetString(SessionHelper.USER_NAME) == null)
            {
                filterContext.Result = new RedirectResult("~/Account/Login");
                return;
            }
        }
    }
}