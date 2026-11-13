using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using Subapp2.DAL;

var builder = WebApplication.CreateBuilder(args);

// Register API controllers. so ASP.NET can find them and send requests to them.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // When .NET turns our objects into JSON, two entities can point to each other
    // IgnoreCycles stops this from going in an endless loop and crashing the request.
    options.JsonSerializerOptions.ReferenceHandler = 
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddOpenApi();

// Register the database context with SQLite
builder.Services.AddDbContext<GameDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("GameDbContextConnection"));
});

// So the React frontend (running on another localhost port) can call the API
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
        policy.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

// Repository pattern: controllers ask for IQuestionRepository and get a QuestionRepository
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IQuizRepository, QuizRepository>();

// Write warnings and errors to a log file.
var loggerConfiguration = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File($"Logs/api_{DateTime.Now:yyyyMMdd_HHmmss}.log");

var logger = loggerConfiguration.CreateLogger();
builder.Logging.AddSerilog(logger);

var app = builder.Build();

// Create the database, apply migrations and seed questions on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<GameDbContext>();
    context.Database.Migrate();
    DbInit.Initialize(context, app.Environment);
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("CorsPolicy");
app.MapScalarApiReference();
app.MapControllers();

app.Run();
