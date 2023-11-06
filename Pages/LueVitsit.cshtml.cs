using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace razor.Pages;

public class LueVitsitModel : PageModel
{
    public string Otsikko { get; set; }
    public string Vitsiteksti { get; set; }
    
    private readonly ILogger<LueVitsitModel> _logger;

    public LueVitsitModel(ILogger<LueVitsitModel> logger)
    {
        _logger = logger;
    }
}
