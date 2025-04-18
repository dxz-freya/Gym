using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Google;
using GymManagement.Data;
using GymManagement.Models;
using GymManagement.Services;
using Microsoft.Extensions.FileProviders;


var builder = WebApplication.CreateBuilder(args);

// ✅ Register services
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
{
  options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
  options.EnableSensitiveDataLogging(); // 🔍 显示 SQL 参数，调试 LINQ 异常
});


builder.Services.AddIdentity<User, IdentityRole>(options =>
{
  options.Password.RequireDigit = true;
  options.Password.RequiredLength = 6;
  options.Password.RequireNonAlphanumeric = true;
  options.Password.RequireUppercase = true;
  options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
  options.LoginPath = "/Account/Login";
  options.AccessDeniedPath = "/Account/AccessDenied";
  options.ExpireTimeSpan = TimeSpan.FromDays(14); // <-- 关键点！
  options.SlidingExpiration = true;               // 可选，刷新时自动续命
  options.Cookie.IsEssential = true;              // 确保不会被阻止
});


builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
      options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
      options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
      options.CallbackPath = "/signin-google";
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<UserService>();

var app = builder.Build();

if (args.Length > 0 && args[0].ToLower() == "seeddata")
{
  bool clearSessions = args.Contains("--clearsessions");

  using var scope = app.Services.CreateScope();
  var services = scope.ServiceProvider;

  await SeedUsers.InitializeAsync(services);
  await SeedSessions.InitializeAsync(services, clearBeforeSeed: clearSessions);

  Console.WriteLine($"✅ Seeding completed {(clearSessions ? "with session cleanup" : "without session cleanup")}.");
  return;
}


// ✅ Web app setup
if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Home/Error");
  app.UseHsts();
}




app.UseHttpsRedirection();
app.UseStaticFiles();

// ✅ Add static file access for profile uploads + disable cache
app.UseStaticFiles(new StaticFileOptions
{
  FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.WebRootPath, "uploads", "profile")),
  RequestPath = "/uploads/profile",
  OnPrepareResponse = ctx =>
  {
    ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
    ctx.Context.Response.Headers.Append("Pragma", "no-cache");
    ctx.Context.Response.Headers.Append("Expires", "0");
  }
});
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ✅ Routes
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Admin}/{action=Dashboard}/{id?}");

app.MapControllerRoute(
    name: "customer",
    pattern: "Customer/{action=Dashboard}/{id?}",
    defaults: new { controller = "Customer" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
