using DemoMVC.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddDbContext<MvcAjaxCrud.Data.AppDbContext>(options => options.UseSqlite(connectionString));

builder.Services
    .AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.EnsureCreatedAsync();

    var identitySql = new[]
    {
        "CREATE TABLE IF NOT EXISTS AspNetRoles (Id TEXT NOT NULL CONSTRAINT PK_AspNetRoles PRIMARY KEY, Name TEXT NULL, NormalizedName TEXT NULL, ConcurrencyStamp TEXT NULL)",
        "CREATE TABLE IF NOT EXISTS AspNetUsers (Id TEXT NOT NULL CONSTRAINT PK_AspNetUsers PRIMARY KEY, UserName TEXT NULL, NormalizedUserName TEXT NULL, Email TEXT NULL, NormalizedEmail TEXT NULL, EmailConfirmed INTEGER NOT NULL, PasswordHash TEXT NULL, SecurityStamp TEXT NULL, ConcurrencyStamp TEXT NULL, PhoneNumber TEXT NULL, PhoneNumberConfirmed INTEGER NOT NULL, TwoFactorEnabled INTEGER NOT NULL, LockoutEnd TEXT NULL, LockoutEnabled INTEGER NOT NULL, AccessFailedCount INTEGER NOT NULL)",
        "CREATE TABLE IF NOT EXISTS AspNetRoleClaims (Id INTEGER NOT NULL CONSTRAINT PK_AspNetRoleClaims PRIMARY KEY AUTOINCREMENT, RoleId TEXT NOT NULL, ClaimType TEXT NULL, ClaimValue TEXT NULL, CONSTRAINT FK_AspNetRoleClaims_AspNetRoles_RoleId FOREIGN KEY (RoleId) REFERENCES AspNetRoles (Id) ON DELETE CASCADE)",
        "CREATE TABLE IF NOT EXISTS AspNetUserClaims (Id INTEGER NOT NULL CONSTRAINT PK_AspNetUserClaims PRIMARY KEY AUTOINCREMENT, UserId TEXT NOT NULL, ClaimType TEXT NULL, ClaimValue TEXT NULL, CONSTRAINT FK_AspNetUserClaims_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE)",
        "CREATE TABLE IF NOT EXISTS AspNetUserLogins (LoginProvider TEXT NOT NULL, ProviderKey TEXT NOT NULL, ProviderDisplayName TEXT NULL, UserId TEXT NOT NULL, CONSTRAINT PK_AspNetUserLogins PRIMARY KEY (LoginProvider, ProviderKey), CONSTRAINT FK_AspNetUserLogins_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE)",
        "CREATE TABLE IF NOT EXISTS AspNetUserRoles (UserId TEXT NOT NULL, RoleId TEXT NOT NULL, CONSTRAINT PK_AspNetUserRoles PRIMARY KEY (UserId, RoleId), CONSTRAINT FK_AspNetUserRoles_AspNetRoles_RoleId FOREIGN KEY (RoleId) REFERENCES AspNetRoles (Id) ON DELETE CASCADE, CONSTRAINT FK_AspNetUserRoles_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE)",
        "CREATE TABLE IF NOT EXISTS AspNetUserTokens (UserId TEXT NOT NULL, LoginProvider TEXT NOT NULL, Name TEXT NOT NULL, Value TEXT NULL, CONSTRAINT PK_AspNetUserTokens PRIMARY KEY (UserId, LoginProvider, Name), CONSTRAINT FK_AspNetUserTokens_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE)",
        "CREATE UNIQUE INDEX IF NOT EXISTS RoleNameIndex ON AspNetRoles (NormalizedName)",
        "CREATE INDEX IF NOT EXISTS IX_AspNetRoleClaims_RoleId ON AspNetRoleClaims (RoleId)",
        "CREATE INDEX IF NOT EXISTS IX_AspNetUserClaims_UserId ON AspNetUserClaims (UserId)",
        "CREATE INDEX IF NOT EXISTS IX_AspNetUserLogins_UserId ON AspNetUserLogins (UserId)",
        "CREATE INDEX IF NOT EXISTS IX_AspNetUserRoles_RoleId ON AspNetUserRoles (RoleId)",
        "CREATE INDEX IF NOT EXISTS EmailIndex ON AspNetUsers (NormalizedEmail)",
        "CREATE UNIQUE INDEX IF NOT EXISTS UserNameIndex ON AspNetUsers (NormalizedUserName)",
        "CREATE TABLE IF NOT EXISTS __EFMigrationsHistory (MigrationId TEXT NOT NULL CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY, ProductVersion TEXT NOT NULL)",
        "INSERT OR IGNORE INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ('20261126060216_AddIdentityTables', '10.0.12')"
    };

    foreach (var sql in identitySql)
    {
        await dbContext.Database.ExecuteSqlRawAsync(sql);
    }

    var productDbContext = scope.ServiceProvider.GetRequiredService<MvcAjaxCrud.Data.AppDbContext>();
    await productDbContext.Database.EnsureCreatedAsync();
    await productDbContext.Database.ExecuteSqlRawAsync(
        "CREATE TABLE IF NOT EXISTS Products (Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL, Price TEXT NOT NULL, Stock INTEGER NOT NULL);");

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    foreach (var role in new[] { "Admin", "User" })
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    var adminEmail = "admin@demomvc.local";
    var admin = await userManager.FindByEmailAsync(adminEmail);
    if (admin == null)
    {
        admin = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(admin, "Admin@123456");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
    else if (!await userManager.IsInRoleAsync(admin, "Admin"))
    {
        await userManager.AddToRoleAsync(admin, "Admin");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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


app.Run();
