using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using DDPA.Commons.Helper;

namespace DDPA.Attributes
{
    public class SessionTimeoutAttribute : ActionFilterAttribute
    {
        public SessionTimeoutAttribute()
        {

        }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            if (filterContext.HttpContext.Session.GetString(SessionHelper.USER_NAME) == null)
            {
                if (filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    // For AJAX requests, return result as a simple string,
                    // and inform calling JavaScript code that a user should be redirected.
                    filterContext.Result = new JsonResult("SessionTimeout");
                }
                else
                {
                    //_signInManager.SignOutAsync();

                    // For round-trip requests,
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                    {
                        controller = "Account",
                        action = "Login"
                    }));
                }
            }
        }
    }
}