using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace razor.Pages;

public class TeeVitsiModel : PageModel
{
    public string Otsikko { get; set; }
    public string Vitsiteksti { get; set; }
    private readonly ILogger<TeeVitsiModel> _logger;

    public TeeVitsiModel(ILogger<TeeVitsiModel> logger)
    {
        _logger = logger;
    }
}
