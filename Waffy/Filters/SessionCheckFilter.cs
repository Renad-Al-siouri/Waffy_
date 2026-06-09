// Filters/SessionCheckFilter.cs
// هذا الـ Filter يتحقق من الـ Session في كل Request
// الصفحات المحمية تضع [SessionRequired] فوقها

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Waffy.Filters
{
    public class SessionRequiredAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.Session.GetString("UserId")
                      ?? context.HttpContext.Session.GetInt32("UserId")?.ToString();

            if (string.IsNullOrEmpty(userId))
            {
                // مسجل خروج → أعده لـ Login ولا تسمح له بالرجوع
                var response = context.HttpContext.Response;
                response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                response.Headers["Pragma"] = "no-cache";
                response.Headers["Expires"] = "0";

                context.Result = new RedirectToActionResult("Login", "Account", null);
            }

            base.OnActionExecuting(context);
        }
    }
}