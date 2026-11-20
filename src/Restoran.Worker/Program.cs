using Restoran.Worker;

// Worker Service on eraldi protsess. Käivitamine: dotnet run --project src/Restoran.Worker
var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<ApiOptions>(builder.Configuration.GetSection("Api"));
builder.Services.AddHttpClient<ApiClient>(); // ainult HTTP, mingit DbContexti pole
builder.Services.AddHostedService<ReminderWorker>();

var host = builder.Build();
host.Run();
