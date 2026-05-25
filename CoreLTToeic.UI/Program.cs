using CoreLTToeic.Application.Mapping;
using CoreLTToeic.Domain.Entities;
using CoreLTToeic.Infrastructure.Config;
using CoreLTToeic.Infrastructure.Context;
using CoreLTToeic.Infrastructure.Helper;
using CoreLTToeic.UI.Components;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Penman.Blazor.Quill;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

var isDev = builder.Environment.IsDevelopment();
builder.Host.UseSerilog((ctx, services, cfg) =>
{
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .ReadFrom.Services(services)
       .Enrich.FromLogContext()
       .WriteTo.Console()
       .WriteTo.File(
           path: Path.Combine(ctx.HostingEnvironment.ContentRootPath, "logs", "app-.log"),
           rollingInterval: RollingInterval.Day,
           retainedFileCountLimit: 7,
           outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
       )
       .MinimumLevel.Is(isDev ? LogEventLevel.Information : LogEventLevel.Warning)
       .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
       .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning);
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddAntDesign();
builder.Services.AddPenmanQuill();

builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("Smtp"));

builder.Services.AddDbContextFactory<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedEmail = true;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/khong-co-quyen";
});

builder.Services.AddRepository();
builder.Services.AddService();

builder.Services.AddAutoMapper(cfg => { }, typeof(MappingProfile).Assembly);

var contentRoot = builder.Environment.ContentRootPath;
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// --- Minimal API: Đăng nhập (tạo auth cookie) ---
app.MapPost("/api/auth/signin", async (
    HttpRequest request,
    SignInManager<AppUser> signInManager,
    UserManager<AppUser> userManager) =>
{
    var form = await request.ReadFormAsync();
    var username = form["username"].ToString();
    var password = form["password"].ToString();

    static string Err(string msg) => "/login?error=" + Uri.EscapeDataString(msg);

    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        return Results.Redirect(Err("Vui lòng nhập đầy đủ thông tin"));

    var user = await userManager.FindByNameAsync(username);
    if (user == null)
        return Results.Redirect(Err("Tài khoản không tồn tại"));

    if (userManager.Options.SignIn.RequireConfirmedEmail && !await userManager.IsEmailConfirmedAsync(user))
        return Results.Redirect(Err("Email chưa được xác nhận, vui lòng kiểm tra hộp thư"));

    var result = await signInManager.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: false);
    if (!result.Succeeded)
        return Results.Redirect(Err("Tài khoản hoặc mật khẩu không đúng"));

    var roles = await userManager.GetRolesAsync(user);
    return roles.Contains("Admin")
        ? Results.Redirect("/admin/quan-li-de-thi")
        : Results.Redirect("/");
}).DisableAntiforgery();

// --- Minimal API: Đăng xuất ---
app.MapGet("/api/auth/signout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(IdentityConstants.ApplicationScheme);
    return Results.Redirect("/");
});

// --- Minimal API: Xác nhận email ---
app.MapGet("/api/auth/confirmemail", async (
    string userId,
    string token,
    CoreLTToeic.Application.Interfaces.IRepository.IAuthRepository authRepo) =>
{
    var result = await authRepo.ConfirmEmailAsync(userId, token);
    return result.Succeeded
        ? Results.Redirect("/login?confirmed=true")
        : Results.Redirect("/confirm?error=true");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<CoreLTToeic.Infrastructure.Data.Seeders.ToeicTestSeeder>();
    await seeder.SeedAsync(Path.Combine(contentRoot, "SeedData", "toeic_test_1.json"));

    var scoreSeeder = scope.ServiceProvider.GetRequiredService<CoreLTToeic.Infrastructure.Data.Seeders.ScoreConversionSeeder>();
    await scoreSeeder.SeedAsync();

    var adminSeeder = scope.ServiceProvider.GetRequiredService<CoreLTToeic.Infrastructure.Data.Seeders.AdminSeeder>();
    await adminSeeder.SeedAsync();

    var courseToeicSeeder = scope.ServiceProvider.GetRequiredService<CoreLTToeic.Infrastructure.Data.Seeders.CourseToeicSeeder>();
    await courseToeicSeeder.SeedAsync();
}

app.Run();
