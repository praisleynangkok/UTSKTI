using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SampleSecureWeb.Data;
using SampleSecureWeb.Services;

var builder = WebApplication.CreateBuilder(args);

// Tambahkan layanan ke kontainer
builder.Services.AddControllersWithViews();

// Konfigurasi DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Menambahkan layanan autentikasi dengan cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Redirect otomatis ke login jika belum terautentikasi
        options.AccessDeniedPath = "/Account/AccessDenied"; // Arahkan ke AccessDenied jika akses ditolak
    });

// Menambahkan dependensi untuk akses data
builder.Services.AddScoped<IStudent, StudentData>();
builder.Services.AddScoped<IUser, UserData>();

// Mengambil pengaturan Email
var smtpServer = builder.Configuration["EmailSettings:SmtpServer"];
var smtpPortString = builder.Configuration["EmailSettings:SmtpPort"];
var smtpUser = builder.Configuration["EmailSettings:SmtpUser"];
var smtpPass = builder.Configuration["EmailSettings:SmtpPass"];

// Memastikan nilai tidak null dan parsing port
if (smtpServer == null || smtpPortString == null || smtpUser == null || smtpPass == null)
{
    throw new Exception("Email settings cannot be null.");
}

if (!int.TryParse(smtpPortString, out int smtpPort))
{
    throw new Exception("Invalid SMTP port number.");
}

// Menambahkan EmailService
builder.Services.AddSingleton(new EmailService(
    smtpServer,
    smtpPort,
    smtpUser,
    smtpPass
));

var app = builder.Build();

// Konfigurasi middleware untuk menangani pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.Use(async (context, next) =>
{
    Console.WriteLine($"Request Path: {context.Request.Path}");
    if (!context.User.Identity.IsAuthenticated && !context.Request.Path.StartsWithSegments("/Account"))
    {
        Console.WriteLine("Redirecting to Login.");
        context.Response.Redirect("/Account/Login");
        return; // Hentikan eksekusi lebih lanjut
    }
    await next(); // Lanjutkan ke middleware berikutnya
});

app.UseAuthentication();
app.UseAuthorization();

// Pastikan route default mengarah ke HomeController
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
