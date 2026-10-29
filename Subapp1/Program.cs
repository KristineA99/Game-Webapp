using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Subapp1.DAL;

var builder = WebApplication.CreateBuilder(args);

// Configure server-side logging with Serilog.
// Logs are written to both the console and a log file.
var loggerConfiguration = new LoggerConfiguration()
        .MinimumLevel.Information()
        .WriteTo.Console()
        .WriteTo.File($"Logs/app_{DateTime.Now:yyyyMMdd_HHmmss}.log");

var logger = loggerConfiguration.CreateLogger();
builder.Logging.AddSerilog(logger);


// Add services to the container.
builder.Services.AddControllersWithViews();

// Add session support for storing temporary game state between requests.
builder.Services.AddSession();

// Register the database context and use SQLite, with the connection string from appsettings.json
builder.Services.AddDbContext<GameDbContext>(options =>
{
    options.UseSqlite(builder.Configuration["ConnectionStrings:GameDbContextConnection"]);
});

// Register ASP.NET Core Identity and store users in GameDbContext
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // no email confirmation needed for local testing
})
.AddRoles<IdentityRole>() // Enables roles such as "Admin" for user authorization
.AddEntityFrameworkStores<GameDbContext>();

// Redirect users who are not logged in to our own login page
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied"; // Redirect to AccessDenied page if user lacks required role
});

// Session support for storing temporary game state and user data between requests.
// options added as in the course demo
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register repositories for dependency injection
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<IGameResultRepository, GameResultRepository>();

var app = builder.Build();

// Create a service scope so the database context can be used during startup.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<GameDbContext>();

    // Apply any pending EF Core migrations to the database.
    context.Database.Migrate();

    // Add initial data if the database contains no questions.
    DbInit.Initialize(context, app.Environment);

    //Create the Admin role and the admin user
    await IdentitySeeder.SeedAsync(scope.ServiceProvider, builder.Configuration);

    // In development only: give the admin user some example quiz attempts,
    // so the progress page and its filters can be tested without playing many games first.
    if (app.Environment.IsDevelopment())
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var adminEmail = builder.Configuration["AdminUser:Email"];
        var admin = string.IsNullOrEmpty(adminEmail) ? null : await userManager.FindByEmailAsync(adminEmail);
        if (admin != null)
        {
            DbInit.SeedSampleGameResults(context, admin);
        }
    }
}

// Configure exception handling based on the environment.
if (app.Environment.IsDevelopment())
{
    // Show detailed exception information during development.
    app.UseDeveloperExceptionPage();
}
else
{
    // Show a user-friendly error page in production.
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession(); // must be placed before UseAuthentication

app.UseAuthentication(); // Reads the login cookie and identifies the user
app.UseAuthorization();  // checks [Authorize] attributes

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
