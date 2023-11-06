using Microsoft.Data.Sqlite;
using MinimalAPIs.Models;
using RazorAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSingleton<IVitsiService, VitsiService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.MapPost("/rest/vitsit",  (Vitsi vitsi, IVitsiService service) =>
{
    service.TallennaVitsi(vitsi);
    return Results.Ok();
});

app.MapGet("/rest/vitsit",  (IVitsiService service) =>
{
    return Results.Ok(service.HaeVitsit());
});

using (var connection = new SqliteConnection("Data Source=razor.db"))
{
    connection.Open();

    SqliteCommand command = connection.CreateCommand();

    string CreateVitsiTaulu = "CREATE TABLE IF NOT EXISTS Vitsit (Id INTEGER PRIMARY KEY AUTOINCREMENT, Otsikko TEXT, Vitsiteksti TEXT);";
    command.CommandText = CreateVitsiTaulu;
    command.ExecuteNonQuery();

    connection.Close();
}

app.Run();
