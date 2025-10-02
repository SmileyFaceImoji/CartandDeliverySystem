using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CartandDeliverySystem.Data;
using CartandDeliverySystem.Services;
using Stripe;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString)
                   .EnableDetailedErrors()
                   .EnableSensitiveDataLogging());
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<CartService>();
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });


        // Add Stripe configuration
        var stripeSettings = builder.Configuration.GetSection("Stripe");
        StripeConfiguration.ApiKey = stripeSettings["SecretKey"];
        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseSession();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Products}/{action=Index}/{id?}");

        app.MapRazorPages();

        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();
            try
            {
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
                await EnsureRolesAsync(roleManager, logger);
                await EnsureAdminUserAsync(userManager, logger);
                await EnsureWorkerUsersAsync(userManager, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding the DB.");
            }
        }

        app.Run();
    }

    private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
    {
        var roles = new[] { "Admin", "User", "Worker" , "Driver"};

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                if (result.Succeeded)
                {
                    logger.LogInformation($"Role '{role}' created successfully.");
                }
                else
                {
                    logger.LogError($"Error creating role '{role}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }

    private static async Task EnsureAdminUserAsync(UserManager<IdentityUser> userManager, ILogger logger)
    {
        string email = "admin@admin.admin";
        string password = "@Password26";

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                logger.LogInformation("Admin user created successfully.");
                var roleResult = await userManager.AddToRoleAsync(user, "Admin");
                if (roleResult.Succeeded)
                {
                    logger.LogInformation("Admin user assigned to Admin role successfully.");
                }
                else
                {
                    logger.LogError($"Error assigning Admin role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                logger.LogError($"Error creating admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            logger.LogInformation("Admin user already exists.");
        }
    }

    private static async Task EnsureWorkerUsersAsync(UserManager<IdentityUser> userManager, ILogger logger)
    {
        var workers = new List<(string Email, string Password)>
        {
            ("worker1@green.com", "Password@123"),
            ("worker2@green.com", "Password@123"),
            ("worker3@green.com", "Password@123"),
            ("worker4@green.com", "Password@123"),
            ("worker5@green.com", "Password@123")
        };

        foreach (var (email, password) in workers)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    logger.LogInformation($"Worker user '{email}' created.");
                    var roleResult = await userManager.AddToRoleAsync(user, "Worker");
                    if (roleResult.Succeeded)
                    {
                        logger.LogInformation($"Worker user '{email}' assigned to 'Worker' role.");
                    }
                    else
                    {
                        logger.LogError($"Failed assigning 'Worker' role to '{email}': {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    logger.LogError($"Failed to create worker '{email}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                logger.LogInformation($"Worker user '{email}' already exists.");
            }
        }
    }
}
