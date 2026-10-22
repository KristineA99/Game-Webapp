using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using Subapp1.DAL;

var builder = WebApplication.CreateBuilder(args);

// Register API controllers. IgnoreCycles prevents crashes if entities
// reference each other (e.g. many-to-many relationships) during JSON serialization.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Configure JSON serialization to use camelCase for property names.
    options.JsonSerializerOptions.ReferenceHandler = 
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
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

// Serilog: write warnings and errors to a log file in the Logs/ folder
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
