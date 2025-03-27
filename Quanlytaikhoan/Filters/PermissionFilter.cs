using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Quanlytaikhoan.Filters
{
    public class PermissionFilter : IActionFilter
    {
        private readonly string _permission;

        public PermissionFilter(string permission)
        {
            _permission = permission;
        }
        void IActionFilter.OnActionExecuted(ActionExecutedContext context)
        {
            if (!context.HttpContext.User.HasClaim(c => c.Type == "Permission" && c.Value == _permission))
            {
                context.Result = new ForbidResult();
            }
        }

        void IActionFilter.OnActionExecuting(ActionExecutingContext context)
        {
            throw new NotImplementedException();
        }
    }
}
