# razor-rest

Nettisivut VSCodella. REST-rajapinta, Razor Pages, SQLite.

- Lataa ja asenna VSCode
- Lataa ja asenna .NET SDK versio 6
- Avaa VSCode ja New Terminal ja kirjoita terminaaliin:
  dotnet new webapp -o razor

## Tietokanta vitsien tallentamiseen
- Lisätään SQLite-tietokanta-paketti kirjoittamalla terminaaliin: dotnet add package Microsoft.Data.Sqlite
- Lisää Program.cs-tiedostoon juuri ennen app.Run(); riviä:
```
using (var connection = new SqliteConnection("Data Source=vitsit.db"))
{
    connection.Open();

    SqliteCommand command = connection.CreateCommand();

    string CreateVitsiTaulu = "CREATE TABLE IF NOT EXISTS Vitsit (Id INTEGER AUTOINCREMENT, Otsikko TEXT, Vitsiteksti TEXT);";
    command.CommandText = CreateVitsiTaulu;
    command.ExecuteNonQuery();

    connection.Close();
}
```
- Nämä rivit tekevät tietokantataulun Vitsit, mihin vitsit tallennetaan.
- Jos taulu Vitsit on jo olemassa, niin tämä ei tee mitään.

## Vitsi-olio
- Luo kansio Models samalle tasolle kuin Pages (Paina oikeaa nappia VSCode:n explorer osan alaosassa tiedostojen alla tyhjässä kohdassa ja New Folder).
- Luo tiedosto Models/Vitsi.cs
- Tämä tiedosto kertoo millainen Vitsi-olio on.
```
namespace MinimalAPIs.Models;
 
public class Vitsi
{
    public int Id { get; set; }
    public string Otsikko { get; set; }
    public string Vitsiteksti { get; set; }
}
```

## Navigointi-vitsi-sivuille
- Kirjoita tiedostoon Pages/Index.cshtml
```
<p><a asp-page="/TeeVitsi">Lähetä vitsi</a></p>
<p><a asp-page="/LueVitsit">Lue vitsit</a></p>
```
- Tällä saadaan linkit TeeVitsi-sivulle ja LueVitsit-sivulle.

## Sivu vitsien tallentamiseen
- Luo tiedosto Pages/TeeVitsi.cshtml
- Tämä tiedosto sisältää HTML-koodin (joka kertoo miltä sivu näyttää) sivulle TeeVitsi, jolla lähetetään vitsi palvelimelle.
```
@page
@model TeeVitsiModel
@{
    ViewData["Title"] = "Home page";
}

<div>
    <form>
        <div class="form-group">
            <label asp-for="Otsikko">Vitsin otsikko</label>
            <input asp-for="Otsikko" class="form-control">
        </div>
        <div class="form-group">
            <label asp-for="Vitsiteksti">Vitsi</label>
            <input asp-for="Vitsiteksti" class="form-control">
        </div>
        <button type="submit" class="btn btn-primary" id="tallenna">Lähetä</button>
    </form>
</div>

@section scripts{
<script type="text/javascript">
    $(function () {

        $('#tallenna').on('click', function (e) {
            e.preventDefault();
            var url = '/rest/vitsit';
            var method = 'post';
            var vitsi = {};
            $.each($(this).closest('form').serializeArray(), function () {
                    vitsi[this.name] = this.value || '';
            });
            $.ajax({
                type: method,
                url: url,
                data: JSON.stringify(vitsi),
                contentType: 'application/json'
            }).done(function () {
                alert('Vitsi tallennettu.')
            });
        });
    });
</script>
}
```
- Lopussa oleva scripti lähettää Vitsi-olion REST-rajapintaan /rest/vitsit  (POST). Tämän teko kerrotaan alempana pidemmällä.


- Luo tiedosto Pages/TeeVitsi.cshtml.cs
- Tämä sivu kertoo Razor Pages:lle, että tällainen TeeVitsi-sivu on olemassa.
```
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

```

## Sivu vitsien lukemiseen
- Luo tiedosto Pages/LueVitsit.cshtml
- Tällä sivulla näytetään vitsit.
```
@page
@model LueVitsitModel
@{
    ViewData["Title"] = "Home page";
}

<div id="vitsit-divi"></div>

@section scripts{
<script type="text/javascript">
    const divi = document.getElementById('vitsit-divi');
    divi.innerHTML = '';

    fetch("/rest/vitsit")
    .then(response => response.json())
    .then(vitsit => {
        for(let i = 0;i < vitsit.length;i++){
            // luodaan div-kentän sisällöksi jokaisen vitsin kohdalla <h1>Otsikko</h1><p>Vitsiteksti</p>
            // Huomaa, että täällä otsikko ja vitsiteksti alkavatkin pienellä kirjaimella
            let h1 = document.createElement('h1');
            h1.innerHTML =  `${vitsit[i].otsikko}`;
            let p = document.createElement('p');
            p.innerHTML =  `${vitsit[i].vitsiteksti}`;

            divi.appendChild(h1);
            divi.appendChild(p);
        }
    });

</script>
}
```
- Lopussa oleva scripti hakee Vitsi-oliot REST-rajapinnasta /rest/vitsit  (GET). Tämän teko kerrotaan alempana pidemmällä.


- Luo tiedosto Pages/LueVitsit.cshtml.cs
- Tämä sivu kertoo Razor Pages:lle, että tällainen LueVitsit-sivu on olemassa.
```
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

```

## Palvelu vitsien käsittelyyn (eli vitsin tallennus ja vitsien hakeminen)

- Luo kansio Services samalle tasolle kuin Pages (Paina oikeaa nappia VSCode:n explorer osan alaosassa tiedostojen alla tyhjässä kohdassa ja New Folder).
- Luo tiedosto Services/VitsiService.cs
- Tämä tiedosto sisältää funktiot, joita REST-apista kutsutaan.
```
using Microsoft.Data.Sqlite;
using MinimalAPIs.Models;
namespace RazorAPI.Services
{
    public interface IVitsiService
    {
        void TallennaVitsi(Vitsi vitsi);
        List<Vitsi> HaeVitsit();
    }
    public class VitsiService : IVitsiService
    {
        public void TallennaVitsi(Vitsi vitsi)
        {
            // tallenna kantaan vitsi
            using (var connection = new SqliteConnection("Data Source=vitsit.db"))
            {
                connection.Open();
                SqliteCommand command = connection.CreateCommand();

                command.CommandText = @"INSERT INTO Vitsit (Otsikko, Vitsiteksti) VALUES($otsikko,$teksti)";
                command.Parameters.AddWithValue("$otsikko", vitsi.Otsikko);
                command.Parameters.AddWithValue("$teksti", vitsi.Vitsiteksti);
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public List<Vitsi> HaeVitsit()
        {
            List<Vitsi> vitsit = new List<Vitsi>();

            // hae vitsit tietokannasta
            using (var connection = new SqliteConnection("Data Source=vitsit.db"))
            {
                connection.Open();
                SqliteCommand command = connection.CreateCommand();

                command.CommandText = "SELECT Otsikko, Vitsiteksti FROM Vitsit ORDER BY Otsikko ASC";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string otsikko = reader.GetString(0);
                        string teksti = reader.GetString(1);
                        Vitsi vitsi = new Vitsi();
                        vitsi.Otsikko = otsikko;
                        vitsi.Vitsiteksti = teksti;

                        vitsit.Add(vitsi);
                    }
                }
                connection.Close();
            }

            return vitsit;
        }
    }
}
```

- Lisää Program.cs-tiedostoon builder.Services.AddRazorPages(); rivin jälkeen:
```
builder.Services.AddSingleton<IVitsiService, VitsiService>();
```
- Tämä kertoo Razor Pagesille, että tällainen VitsiService on olemassa.


## REST-rajapinnan luominen

### REST-rajapinta vitsin tallentamiselle

- Lisää Program.cs-tiedostoon ennen tietokantaluontirivejä REST rajapinta vitsin lisäämiselle:
```
app.MapPost("/rest/vitsit",  (Vitsi vitsi, IVitsiService service) =>
{
    service.TallennaVitsi(vitsi);
    return Results.Ok();
});
```
- Kun sivulla TeeVitsi painetaan Lähetä-nappia, niin siellä on kerrottu tämä sama REST-api-osoite, jonne vitsi lähetetään.
- Tämä REST-api-kutsu taas kutsuu VitsiServicen TallennaVitsi-funktiota, joka tallentaa vitsin kantaan.

### REST-rajapinta vitsien lukemiseen

- Lisää Program.cs-tiedostoon yllä olevan jälkeen:
```
app.MapGet("/rest/vitsit",  (IVitsiService service) =>
{
    return Results.Ok(service.HaeVitsit());
});
```
- LueVitsit-sivulla oleva javascript (jQuery) hakee vitsit tästä REST-rajapinnasta.
- Tämä REST-api-kutsu taas kutsuu VitsiServicen HaeVitsit-funktiota, joka hakee vitsit tietokannasta ja palauttaa ne.

## Testaaminen
- Kirjoita terminaaliin: dotnet run
- Jos kaikki meni hyvin, niin sivusto käynnistyy ja sivuston osoite näkyy terminaalissa.
- Esimerkiksi: Now listening on: http://localhost:5254
- Mene nettiselaimella sivulle http://localhost:5254
- Klikkaa Lähetä vitsi-linkkiä.
- Kirjoita otsikko ja vitsi ja paina Lähetä-nappia.
- Jos lähetys onnistui, tulee siitä ilmoitusikkuna.
- Klikkaa sivuston yläreunassa näkyvää Home-linkkiä.
- Klikkaa Lue vitsit-linkkiä.
- Nyt pitäisi näkyä juuri tallennettu vitsi sivulla.

# KIRJAUTUMINEN

## Asetukset

- Editoi Program.cs-tiedostoa
- Muuta rivi builder.Services.AddRazorPages();
```
builder.Services.AddRazorPages(options =>  
{  
    options.Conventions.AuthorizeFolder("/").AllowAnonymousToPage("/Login");   
});
```
- Tällä kerrotaan, että jokainen sivu vaatii sisäänkirjautuneen käyttäjän paitsi Login

- Lisää äskeisen rivin alle tämä:
```
builder.Services.AddAuthentication("MunAuthScheme")
    .AddCookie("MunAuthScheme", options => {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";        
        options.AccessDeniedPath = "/AccessDenied";
    });
builder.Services.AddHttpContextAccessor();
```
- Tämä kertoo, että tallennetaan autentikoitu käyttäjä keksiin MunAuthScheme, login-sivu on Login, Logout-sivu on Logout ja AccessDenied-sivu on virhe-sivu jos ei ole kirjautunut.
- AccessDenied-sivua ei ole pakko tehdä, jos ei halua.

- Varmista, että nämä molemmat rivit löytyvät Program.cs-tiedostosta:
```
app.UseAuthentication();
app.UseAuthorization();
```
- Nämä rivit kertovat, että käyttäjän kirjautuminen asetetaan päälle.
- Lisää käyttäjä-taulu tietokantaan, jotta salasana voidaan hakea sieltä (sinne voi myös tallentaa saltin mikäli tällainen on käytössä ja hakea kirjautumisvaiheessa)
- Muokkaa tietokannan luonti Program.cs-tiedostossa esimerkiksi tällaiseksi:
```
    string CreateVitsiTaulu = "CREATE TABLE IF NOT EXISTS Vitsit (Id INTEGER PRIMARY KEY AUTOINCREMENT, Otsikko TEXT, Vitsiteksti TEXT); "
        + "CREATE TABLE IF NOT EXISTS Tyypit (Id INTEGER PRIMARY KEY AUTOINCREMENT, Tunnus TEXT, Salasana TEXT);";
    command.CommandText = CreateVitsiTaulu;
    command.ExecuteNonQuery();
```

## Sisäänkirjautumissivu
- Tee tiedosto Pages/Login.cshtml
```
@page
@model LoginModel
@{
    ViewData["Title"] = "Home page";
}

<div class="text-center">
    <h1 class="display-4">Login</h1>
</div>

<div>
    <p style="color:red;">@Model.Message</p>
</div>
<form method="post">
    <div class="form-group">
        <label asp-for="Username" class="col-form-label col-md-2"></label>
        <div class="col-md-10">
            <input asp-for="Username" />
        </div>
    </div>
    <div class="form-group">
        <label asp-for="Password" class="col-form-label col-md-2"></label>
        <div class="col-md-10">
            <input asp-for="Password" />
        </div>
    </div>
    <div class="form-group">
        <button class="btn btn-outline-primary btn-sm">Log in</button>
    </div>
</form>
```

- Tee tiedosto Login.cshtml.cs
```
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

```

## Uloskirjautumissivu
- Tee tiedosto Pages/Logout.cshtml
```
@page
@model LogoutModel
@{
    ViewData["Title"] = "Home page";
}

<div class="text-center">
    <h1 class="display-4">Bye bye!</h1>
</div>
```

- Tee tiedosto Pages/Logout.cshtml.cs
```
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
```

## Lisää Logout-linkki jonnekin
- Voit lisätä esimerkiksi ylänavigointiin logout-linkin
- Lisää tiedostoon Pages/Shared/_Layout.cshtml logout-linkki
```
<li class="nav-item">
    <a class="nav-link text-dark" asp-area="" asp-page="/Logout">Logout</a>
</li>
```

## Login-linkkiä ei välttämättä tarvitse
- Sivusto ohjautuu itsestään Login-sivulle, jos käyttäjä ei ole kirjautunut sisään.

## Näytä käyttäjätunnus sivuilla
- Voit lisätä Pages/Index.cshtml-sivulle käyttäjätunnuksen näkyviin
```
<div class="text-center">
    <h1 class="display-4">Welcome @User.Identity.Name</h1>
    <p>Learn about <a href="https://docs.microsoft.com/aspnet/core">building Web apps with ASP.NET Core</a>.</p>
    <p><a asp-page="/TeeVitsi">Lähetä vitsi</a></p>
    <p><a asp-page="/LueVitsit">Lue vitsit</a></p>
</div>
```

## Jatkokehitystä
- Kun käyttäjä on kirjautunut sisään, niin käyttäjätunnus pystytään aina selvittämään kun esimerkiksi vitsi lisätään tietokantaan.
- Tällöin vitsille voi asettaa tämän käyttäjän tekijäksi

## Kirjautumisen testaus
- Avaa kaksi eri selainta... esimerkiksi Chrome ja Edge ja kirjaudu molemmilla sisään eri käyttäjänä, tällöin molemmilla näkyy oma käyttäjätunnus etusivulla.
- Logout-sivulla kun käy, ei enää pitäisi päästä mitenkään muille sivuille.
