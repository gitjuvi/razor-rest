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
using (var connection = new SqliteConnection("Data Source=razor.db"))
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
- Jos tietokanta razor.db on jo olemassa, niin tämä ei tee mitään.

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
            using (var connection = new SqliteConnection("Data Source=razor.db"))
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
            using (var connection = new SqliteConnection("Data Source=razor.db"))
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

