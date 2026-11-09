using Microsoft.EntityFrameworkCore;
using Restoran.Application.Interfaces;
using Restoran.Infrastructure.Data;
using Restoran.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddSingleton<IPasswordService, PasswordService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Migratsioonid ja algandmed käivitamisel
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db, scope.ServiceProvider.GetRequiredService<IPasswordService>());
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
