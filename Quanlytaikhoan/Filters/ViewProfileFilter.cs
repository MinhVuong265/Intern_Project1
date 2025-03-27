using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace Quanlytaikhoan.Filters
{
    public class ViewProfileFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            throw new NotImplementedException();
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.ActionArguments["id"] as int?;
            if (!userId.HasValue)
            {
                context.Result = new BadRequestObjectResult(new { message = "ID người dùng không hợp lệ." });
                return;
            }

            var idNguoiDungHienTai = int.Parse(context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (!context.HttpContext.User.HasClaim(c => c.Type == "Permission" && c.Value == "ViewProfile") && idNguoiDungHienTai != userId.Value)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
