using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using ST10335260_PROG6212_POE_Part2.Data;
using ST10335260_PROG6212_POE_Part2.Seeding;
using ST10335260_PROG6212_POE_Part2.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddDbContext<RaceDayDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IBlobStorageService, AzureBlobStorageService>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "RaceDay API",
        Version = "v1",
        Description = "API for managing RaceDay events, categories, enrolments, results and image uploads."
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Configuration.GetValue("Swagger:Enabled", false))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Configuration.GetValue("Database:MigrateOnStartup", true))
{
    await DatabaseInitialiser.InitialiseAsync(app.Services, app.Configuration, app.Logger);
}

app.UseSession();
app.UseAuthorization();
app.MapControllers();

app.Run();
