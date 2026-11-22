using Restoran.Worker;

// Worker Service on eraldi protsess. Käivitamine: dotnet run --project src/Restoran.Worker
var builder = Host.CreateApplicationBuilder(args);

// Salajased võtmed (SMS, e-post, JWT) on failis appsettings.Local.json – see EI lähe GitHubi (.gitignore).
// Näidis: appsettings.Local.example.json. Keskkonnamuutujad ja käsurida jäävad kõige tugevamaks.
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddCommandLine(args);

builder.Services.Configure<ApiOptions>(builder.Configuration.GetSection("Api"));
builder.Services.AddHttpClient<ApiClient>(); // ainult HTTP, mingit DbContexti pole
builder.Services.AddHostedService<ReminderWorker>();

var host = builder.Build();
host.Run();
