using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Subapp1.DAL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register the database context and use SQLite, with the connection string from appsettings.json
builder.Services.AddDbContext<GameDbContext>(options =>
{
    options.UseSqlite(builder.Configuration["ConnectionStrings:GameDbContextConnection"]);
});

// When a class asks for IQuestionRepository, give it a QuestionRepository (one new instance per HTTP request)
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();

var app = builder.Build();

// Create a service scope so the database context can be used during startup.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<GameDbContext>();

    // Apply any pending EF Core migrations to the database.
    context.Database.Migrate();

    // Add initial data if the database contains no questions.
    DbInit.Initialize(context);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
