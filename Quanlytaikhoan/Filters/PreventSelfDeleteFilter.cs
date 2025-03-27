using Microsoft.AspNetCore.Mvc.Filters;
using UserManagementApi.Data;

namespace Quanlytaikhoan.Filters
{
    public class PreventSelfDeleteFilter : IActionFilter
    {
        private readonly UserManagementContext _dbContext;

        public PreventSelfDeleteFilter (UserManagementContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            throw new NotImplementedException();
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            throw new NotImplementedException();
        }
    }
}
