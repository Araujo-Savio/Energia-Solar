using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SolarEnergy.Data;
using SolarEnergy.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Entity Framework configuration with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Password settings
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 8;
        options.Password.RequiredUniqueChars = 1;

        // Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // User settings
        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Cookie configuration
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.LoginPath = "/Auth/Login";
    options.AccessDeniedPath = "/Auth/AccessDenied";
    options.SlidingExpiration = true;
});

// Adicionar os serviços
builder.Services.AddScoped<SolarEnergy.Services.ILeadService, SolarEnergy.Services.LeadService>();
builder.Services.AddScoped<SolarEnergy.Services.IReportService, SolarEnergy.Services.ReportService>();
builder.Services.AddScoped<SolarEnergy.Services.IUserSimulationService, SolarEnergy.Services.UserSimulationService>();
builder.Services.AddScoped<SolarEnergy.Services.IUserSimulationMapper, SolarEnergy.Services.UserSimulationMapper>();
builder.Services.AddScoped<SolarEnergy.Services.ISimulationExportService, SolarEnergy.Services.SimulationExportService>();
builder.Services.AddTransient<SolarEnergy.Services.IRazorViewRenderer, SolarEnergy.Services.RazorViewRenderer>();

// Register export services
builder.Services.AddScoped<SolarEnergy.Services.ExportService>();
builder.Services.AddScoped<SolarEnergy.Services.SimpleExportService>();
builder.Services.AddScoped<SolarEnergy.Services.IExportService, SolarEnergy.Services.ExportService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
// Create database and roles if they don't exist
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedDataAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error creating database or roles.");
    }
}

app.Run();

static async Task SeedDataAsync(IServiceProvider services)
{
    var context = services.GetRequiredService<ApplicationDbContext>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    // Apply pending migrations and ensure database schema is up-to-date
    await context.Database.MigrateAsync();

    // Ensure required roles exist
    string[] roleNames = { "Admin", "Company", "Client" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // Ensure admin user exists and is in the Admin role
    const string adminEmail = "admin@solarenergy.com";
    const string adminPassword = "Admin123!";
    const string adminRoleName = "Admin";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Administrador do Sistema",
            UserType = UserType.Administrator,
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Não foi possível criar o usuário admin: {string.Join(",", result.Errors.Select(e => e.Description))}");
        }
    }

    if (!await userManager.IsInRoleAsync(adminUser, adminRoleName))
    {
        await userManager.AddToRoleAsync(adminUser, adminRoleName);
    }
}

// # Comandos que você precisa rodar manualmente:
// dotnet ef migrations add AddCompanyParametersBusinessFields
// dotnet ef database update
