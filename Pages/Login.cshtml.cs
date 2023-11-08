using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

namespace razor.Pages;

public class LoginModel : PageModel
{
    [BindProperty]
    public string Username { get; set; }
    [BindProperty, DataType(DataType.Password)]
    public string Password { get; set; }
    [BindProperty]
    public string Message { get; set; }

    private readonly ILogger<LoginModel> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LoginModel(ILogger<LoginModel> logger, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async void OnGet()
    {
    }

    public async Task<IActionResult> OnPost()
    {

        // katso tietokannasta menikö tunnus ja salasana oikein
        using (var connection = new SqliteConnection("Data Source=vitsit.db"))
        {
            connection.Open();
            SqliteCommand command = connection.CreateCommand();

            command.CommandText = @"SELECT Salasana FROM Tyypit WHERE Tunnus=$tunnus";
            command.Parameters.AddWithValue("$tunnus", Username);
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string salasana = reader.GetString(0);
                    if (Password == salasana)
                    {
                        // oli oikein, kirjaudutaan sisään
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, Username)  
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, "MunAuthScheme");

                        await _httpContextAccessor.HttpContext
                            .SignInAsync("MunAuthScheme",
                                new ClaimsPrincipal(claimsIdentity),
                                new AuthenticationProperties());
                        
                        return RedirectToPage("/Index");
                    }
                }
            }
            connection.Close();
        }
        
        Message = "Käyttäjänimi tai salasana väärin. Yritä uudelleen.";
        return Page();
    }
}
