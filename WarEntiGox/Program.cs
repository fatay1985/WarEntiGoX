using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using WarEntiGox.Models;
using WarEntiGox.Services;
using static Microsoft.AspNetCore.Builder.WebApplication;
using Microsoft.AspNetCore.Http; // Session kullanabilmek için ekledik

var builder = CreateBuilder(args);

// MongoDB bağlantısını ekleyelim
builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
    new MongoClient(builder.Configuration.GetConnectionString("MongoDbConnection")));

builder.Services.AddScoped<CompanyService>();
builder.Services.AddScoped<UserService>();

// Swagger kurulumunu ekleyelim
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TradeOcean API", Version = "v1" });

    // Sadece ApiController attribute'lu controller'ları Swagger'a dahil et
    c.DocInclusionPredicate((_, api) =>
        api.ActionDescriptor is ControllerActionDescriptor descriptor &&
        descriptor.ControllerTypeInfo.GetCustomAttributes(typeof(ApiControllerAttribute), true).Any());
});
  
// MVC ve Razor Pages desteği
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Session yapılandırmasını ekleyelim
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // 30 dakika session süresi
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Swagger'ı sadece geliştirme ortamında aktif edelim
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "WarEntiGoX API v1");
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Session middleware aktif edelim
app.UseSession();

app.UseAuthorization();

// MVC ve API route yapılandırması
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "login", // Giriş sayfası için route ekleyelim
    pattern: "mvc/Login/{action=Index}/{id?}");

app.MapControllers();

app.Run();
