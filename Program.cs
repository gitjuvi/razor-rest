using Microsoft.Data.Sqlite;
using MinimalAPIs.Models;
using RazorAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(options =>  
{  
    options.Conventions.AuthorizeFolder("/").AllowAnonymousToPage("/Login");   
});
builder.Services.AddSingleton<IVitsiService, VitsiService>();

// Authentication
builder.Services.AddAuthentication("MunAuthScheme")
    .AddCookie("MunAuthScheme", options => {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";        
        options.AccessDeniedPath = "/AccessDenied";
    });
builder.Services.AddHttpContextAccessor();

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

// Remember both of these to get authentication
app.UseAuthentication();
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

using (var connection = new SqliteConnection("Data Source=vitsit.db"))
{
    connection.Open();

    SqliteCommand command = connection.CreateCommand();

    string CreateVitsiTaulu = "CREATE TABLE IF NOT EXISTS Vitsit (Id INTEGER PRIMARY KEY AUTOINCREMENT, Otsikko TEXT, Vitsiteksti TEXT); "
        + "CREATE TABLE IF NOT EXISTS Tyypit (Id INTEGER PRIMARY KEY AUTOINCREMENT, Tunnus TEXT, Salasana TEXT);";
    command.CommandText = CreateVitsiTaulu;
    command.ExecuteNonQuery();

    connection.Close();
}

app.Run();
