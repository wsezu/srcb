using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Voeg HttpClient toe
builder.Services.AddHttpClient();
builder.Services.AddControllers();

var app = builder.Build();

// Serve statische bestanden (UI)
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();