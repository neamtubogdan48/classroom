using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using mvc.Models;

public class BaseController : Controller
{
    protected readonly UserManager<UserAccount> _userManager;

    public BaseController(UserManager<UserAccount> userManager)
    {
        _userManager = userManager;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);

        if (User.Identity.IsAuthenticated)
        {
            var currentUser = _userManager.GetUserAsync(User).Result;
            ViewData["CurrentUser"] = currentUser;
        }
    }
}
