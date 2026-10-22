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

// Register repositories for dependency injection
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IQuizRepository, QuizRepository>();

var app = builder.Build();

// Create a service scope so the database context can be used during startup.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<GameDbContext>();

    // Apply any pending EF Core migrations to the database.
    context.Database.Migrate();

    // Add initial data if the database contains no questions.
    DbInit.Initialize(context, app.Environment);
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

app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
