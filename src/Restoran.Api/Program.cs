using System.Reflection;
using System.Text.Json.Serialization;
using Restoran.Api.Infrastructure;
using Restoran.Application.Interfaces;
using Restoran.Infrastructure;
using Restoran.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Andmebaas, repositooriumid ja teenused
builder.Services.AddRestoran(builder.Configuration);

// Ühtne veakäsitlus (ProblemDetails)
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new() { Title = "Restorani lauabroneerimine API", Version = "v1" });
    var xml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    o.IncludeXmlComments(xml);
    var appXml = Path.Combine(AppContext.BaseDirectory, "Restoran.Application.xml");
    if (File.Exists(appXml)) o.IncludeXmlComments(appXml);
});

var app = builder.Build();

// Migratsioonid ja algandmed käivitamisel
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db, scope.ServiceProvider.GetRequiredService<IPasswordService>());
}

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
