using Microsoft.AspNetCore.Mvc.Filters;
using Quanlytaikhoan.Services;


namespace Quanlytaikhoan.Filters
{
    public class AuthorizeFilter : IActionFilter
    {
        private readonly IAuthService _authService;
            
        public AuthorizeFilter (IAuthService authService)
        {
            _authService = authService;
        }

        void IActionFilter.OnActionExecuted(ActionExecutedContext context)
        {
          
        }

        void IActionFilter.OnActionExecuting(ActionExecutingContext context)
        {
            throw new NotImplementedException();
        }
    }
}
