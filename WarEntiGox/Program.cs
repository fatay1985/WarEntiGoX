using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using WarEntiGox.Services;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver; // Session kullanabilmek için ekledik

var builder = WebApplication.CreateBuilder(args);

// MongoDB bağlantısını ekleyelim
builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
    new MongoClient(builder.Configuration.GetConnectionString("MongoDbConnection")));

builder.Services.AddScoped<CompanyService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<ProductCategoryService>();
builder.Services.AddScoped<WarehouseService>();
builder.Services.AddScoped<WarehouseLocationService>();



// Swagger kurulumunu ekleyelim
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WarEntiGox API", Version = "v1" });
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

app.UseCors("AllowAllOrigins");
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
