using Core.Constants;
using Core.Interfaces;
using Infra.Data;
using Infra.Repositories;
using Infra.Services;
using Web.Data;
using Web.Middleware;
using Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
// Identity storage
builder.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));
// Domain storage
builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => 
{
    options.SignIn.RequireConfirmedAccount = false; // Allow external logins
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Configure external authentication providers
builder.Services.AddAuthentication()
    .AddGoogle(googleOptions =>
    {
        var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
        var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        
        if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
        {
            googleOptions.ClientId = googleClientId;
            googleOptions.ClientSecret = googleClientSecret;
            googleOptions.CallbackPath = "/signin-google";
        }
    })
    .AddFacebook(facebookOptions =>
    {
        var facebookAppId = builder.Configuration["Authentication:Facebook:AppId"];
        var facebookAppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
        
        if (!string.IsNullOrEmpty(facebookAppId) && !string.IsNullOrEmpty(facebookAppSecret))
        {
            facebookOptions.AppId = facebookAppId;
            facebookOptions.AppSecret = facebookAppSecret;
            facebookOptions.CallbackPath = "/signin-facebook";
        }
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(builder.Configuration.GetValue<int>("Session:IdleTimeoutMinutes", 30));
    options.Cookie.Name = builder.Configuration["Session:CookieName"] ?? "KavyasCreation.Session";
    options.Cookie.HttpOnly = builder.Configuration.GetValue<bool>("Session:CookieHttpOnly", true);
    options.Cookie.IsEssential = builder.Configuration.GetValue<bool>("Session:CookieIsEssential", true);
});
builder.Services.AddHostedService<StockReservationCleanupService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IWishlistRepository, WishlistRepository>();
builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
builder.Services.AddScoped<IUserAddressRepository, UserAddressRepository>();

// New marketplace repositories
builder.Services.AddScoped<IVendorRepository, VendorRepository>();
builder.Services.AddScoped<IVendorUserRepository, VendorUserRepository>();
builder.Services.AddScoped<IBuyerCompanyRepository, BuyerCompanyRepository>();
builder.Services.AddScoped<IBuyerUserRepository, BuyerUserRepository>();
builder.Services.AddScoped<IVendorBuyerRelationshipRepository, VendorBuyerRelationshipRepository>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Add response compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// Add response caching for better performance
builder.Services.AddResponseCaching();

// Add output caching for Razor Pages
builder.Services.AddOutputCache(options =>
{
    // Default policy: cache for 60 seconds
    options.AddBasePolicy(builder => builder
        .Expire(TimeSpan.FromSeconds(60))
        .Tag("default"));
    
    // Catalog pages: cache for 2 minutes, vary by query string
    options.AddPolicy("catalog", builder => builder
        .Expire(TimeSpan.FromMinutes(2))
        .SetVaryByQuery("categoryId", "search")
        .Tag("catalog"));
});

// Add memory cache
builder.Services.AddMemoryCache();

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("identity-db")
    .AddDbContextCheck<AppDbContext>("app-db");

// Add rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User?.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = builder.Configuration.GetValue<int>("RateLimiting:PermitLimit", 100),
                Window = TimeSpan.FromSeconds(builder.Configuration.GetValue<int>("RateLimiting:WindowSeconds", 60)),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = builder.Configuration.GetValue<int>("RateLimiting:QueueLimit", 10)
            }));
    
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Too many requests. Please try again later.",
            retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter) ? retryAfter.ToString() : null
        }, cancellationToken);
    };
});

// Add CORS
var corsOrigins = builder.Configuration["Cors:AllowedOrigins"] ?? "*";
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (corsOrigins == "*")
        {
            policy.AllowAnyOrigin();
        }
        else
        {
            policy.WithOrigins(corsOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries));
        }
        
        policy.AllowAnyMethod()
              .AllowAnyHeader();
        
        if (builder.Configuration.GetValue<bool>("Cors:AllowCredentials", false))
        {
            policy.AllowCredentials();
        }
    });
});

var app = builder.Build();

// Add global exception handler middleware
app.UseGlobalExceptionHandler();

// Add security headers middleware
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseResponseCompression();

// Add response caching middleware (must be before routing)
app.UseResponseCaching();
app.UseOutputCache();

app.UseHttpsRedirection();
app.UseRouting();

app.UseCors();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapStaticAssets();

// Redirect root to Catalog page (new home)
app.MapGet("/", context =>
{
    context.Response.Redirect("/Store/Catalog");
    return Task.CompletedTask;
});

// Support areas routing
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages()
   .WithStaticAssets();

// Add health check endpoint
app.MapHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
    var identityDb = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (!await TableExistsAsync(identityDb, "AspNetRoles"))
    {
        await identityDb.Database.MigrateAsync();
    }
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbInitializer.SeedAsync(db);

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    
    // Seed all roles
    foreach (var role in Roles.All)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    var adminEmail = builder.Configuration["SeedAdmin:Email"] ?? "admin@local";
    var adminPassword = builder.Configuration["SeedAdmin:Password"] ?? "Admin123!";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser is null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        await userManager.CreateAsync(adminUser, adminPassword);
    }

    if (!await userManager.IsInRoleAsync(adminUser, Roles.Admin))
    {
        await userManager.AddToRoleAsync(adminUser, Roles.Admin);
    }
}

static async Task<bool> TableExistsAsync(DbContext dbContext, string tableName)
{
    var connection = dbContext.Database.GetDbConnection();
    await connection.OpenAsync();
    try
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT 1 FROM sys.tables WHERE name = @name";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "@name";
        parameter.Value = tableName;
        command.Parameters.Add(parameter);
        var result = await command.ExecuteScalarAsync();
        return result is not null;
    }
    finally
    {
        await connection.CloseAsync();
    }
}

app.Run();
