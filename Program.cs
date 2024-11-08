using Asp.Net_MvcWeb_Pj3.Aptech.Models; // Thay thế 'YourNamespace' bằng namespace thực tế nơi DataContext được định nghĩa
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Thêm các dịch vụ vào container.
builder.Services.AddControllersWithViews();

// Cho phép cập nhật lại trang web nếu tệp *.cshtml bị sửa nội dung
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

// Cấu hình kết nối SQL Azure
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Thêm dịch vụ phiên
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Cấu hình HTTP client để sử dụng proxy QuotaGuard Static
builder.Services.AddHttpClient("QuotaGuardStatic", client =>
{
    var proxy = new WebProxy(Environment.GetEnvironmentVariable("QUOTAGUARDSTATIC_URL"));
    var httpClientHandler = new HttpClientHandler
    {
        Proxy = proxy,
        UseProxy = true,
    };
    client.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory-Sample");
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    Proxy = new WebProxy(Environment.GetEnvironmentVariable("QUOTAGUARDSTATIC_URL")),
    UseProxy = true,
});

var app = builder.Build();

// Cấu hình pipeline xử lý HTTP.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseSession(); // Sử dụng middleware phiên

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=AdminPatient}/{action=Index}/{id?}");

app.Run();