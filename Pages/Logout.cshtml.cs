using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace razor.Pages;

public class LogoutModel : PageModel
{
    private readonly ILogger<LogoutModel> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LogoutModel(ILogger<LogoutModel> logger, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async void OnGet()
    {
        await _httpContextAccessor.HttpContext.SignOutAsync("MunAuthScheme");
    }
}
