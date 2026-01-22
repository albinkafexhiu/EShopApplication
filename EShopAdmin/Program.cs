using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

// MVC
builder.Services.AddControllersWithViews();

// HttpClient for talking to EShop.Web API
builder.Services.AddHttpClient("eshopApi", client =>
{
    var baseUrl = builder.Configuration["ServiceApiSettings:BaseUrl"];
    if (string.IsNullOrEmpty(baseUrl))
        throw new InvalidOperationException("ServiceApiSettings:BaseUrl is not configured");

    client.BaseAddress = new Uri(baseUrl);
});
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Order}/{action=Index}/{id?}");

app.Run();