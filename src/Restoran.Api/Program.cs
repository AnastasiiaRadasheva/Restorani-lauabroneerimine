using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Restoran.Api.Infrastructure;
using Restoran.Api.Realtime;
using Restoran.Application.Interfaces;
using Restoran.Infrastructure;
using Restoran.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Salajased võtmed (SMS, e-post, JWT) on failis appsettings.Local.json – see EI lähe GitHubi (.gitignore).
// Näidis: appsettings.Local.example.json. Keskkonnamuutujad ja käsurida jäävad kõige tugevamaks.
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddCommandLine(args);

// Andmebaas, repositooriumid ja teenused
builder.Services.AddRestoran(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddSingleton<ITokenService, JwtTokenService>();

// Reaalajas uuendused (WebSocket /ws/tables)
builder.Services.AddSingleton<TableSocketHub>();
builder.Services.AddSingleton<IRealtimeNotifier>(sp => sp.GetRequiredService<TableSocketHub>());

// JWT autentimine
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key))
        };
        // WebSocket ei saa saata Authorization päist, seepärast token tuleb query-parameetris access_token
        o.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var token = ctx.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(token) && ctx.HttpContext.Request.Path.StartsWithSegments("/ws"))
                    ctx.Token = token;
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

// CORS: React klient
builder.Services.AddCors(o => o.AddPolicy("web", p => p
    .WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:5173"])
    .AllowAnyHeader()
    .AllowAnyMethod()));

// Logimine: iga päring (meetod, aadress, olekukood, kestus)
builder.Services.AddHttpLogging(o =>
{
    o.LoggingFields = HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath |
                      HttpLoggingFields.ResponseStatusCode | HttpLoggingFields.Duration;
    o.CombineLogs = true; // üks rida päringu kohta
});

// Ühtne veakäsitlus (ProblemDetails)
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .ConfigureApiBehaviorOptions(o =>
    {
        // Valideerimisvead saidi keeles (Accept-Language: et/ru/en)
        var defaultFactory = o.InvalidModelStateResponseFactory;
        o.InvalidModelStateResponseFactory = context =>
        {
            var lang = ErrorTranslator.LanguageOf(context.HttpContext);
            foreach (var entry in context.ModelState.Values)
            {
                var translated = entry.Errors.Select(e => ErrorTranslator.TranslateValidation(e.ErrorMessage, lang)).ToList();
                entry.Errors.Clear();
                foreach (var t in translated) entry.Errors.Add(t);
            }
            var result = defaultFactory(context);
            if (result is ObjectResult { Value: ValidationProblemDetails problem })
            {
                problem.Title = lang switch { "ru" => "Неверные данные", "en" => "Invalid data", _ => "Vigased andmed" };
                // JsonResult, et kontrolleri [Produces("application/json")] ei muudaks tüüpi: vead on alati ProblemDetails
                return new JsonResult(problem) { StatusCode = StatusCodes.Status400BadRequest, ContentType = "application/problem+json" };
            }
            return result;
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    // Üks loend ainult põhitoimingutega (vt SwaggerMain); ülejäänud toimingud töötavad API-s edasi
    o.SwaggerDoc("v1", new OpenApiInfo { Title = "Restorani lauabroneerimine API", Version = "v1", Description = "Põhitoimingud: lauad, broneeringud, menüü, eeltellimus, sisselogimine, õhtud, statistika ja taustateenus." });
    o.DocInclusionPredicate(SwaggerMain.Visible);
    o.OperationFilter<SwaggerMain.RolesFilter>();
    var xml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    o.IncludeXmlComments(xml);
    var appXml = Path.Combine(AppContext.BaseDirectory, "Restoran.Application.xml");
    if (File.Exists(appXml)) o.IncludeXmlComments(appXml);

    // Authorize nupp Swaggeris: sisselogimine e-posti ja parooliga (samad kontod mis veebilehel).
    // Swagger saab vastuseks JWT tokeni ja lisab selle ise igale päringule – tokenit pole vaja käsitsi kopeerida.
    o.AddSecurityDefinition("Login", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Description = "Logi sisse e-posti ja parooliga nagu veebilehel. " +
                      "Testkontod: admin@restoran.ee / Admin123!, waiter@restoran.ee / Waiter123!, klient@restoran.ee / Klient123!",
        Flows = new OpenApiOAuthFlows
        {
            Password = new OpenApiOAuthFlow { TokenUrl = new Uri("/api/auth/token", UriKind.Relative), Scopes = new Dictionary<string, string>() }
        }
    });
    o.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Login", doc)] = []
    });
});

var app = builder.Build();

// Migratsioonid ja algandmed käivitamisel
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db, scope.ServiceProvider.GetRequiredService<IPasswordService>());
}

app.UseHttpLogging();
app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseSwagger();
app.UseSwaggerUI(o =>
{
    o.DocumentTitle = "Argent API";
    o.SwaggerEndpoint("/swagger/v1/swagger.json", "Restorani lauabroneerimine API");
    // Sisselogimise aken ainult e-posti ja parooliga, nupul kasutaja nimi, loendis rolli toimingud
    o.HeadContent = SwaggerMain.HeadContent;
    // Sisselogimine jääb meelde ka lehe uuendamisel
    o.EnablePersistAuthorization();
});

app.UseCors("web");
app.UseAuthentication();
app.UseAuthorization();

app.UseWebSockets(new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(30) });

app.MapControllers();
// API juuraadress avab Swaggeri (muidu oleks seal tühi 404-leht)
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

// WebSocketi otspunkt: ühendus nõuab JWT-d (ws://localhost:5000/ws/tables?access_token=...)
app.Map("/ws/tables", async (HttpContext context, TableSocketHub hub) =>
{
    if (!context.WebSockets.IsWebSocketRequest)
        return Results.BadRequest("Ainult WebSocket ühendus.");
    if (context.User.Identity?.IsAuthenticated != true)
        return Results.Unauthorized();

    await hub.HandleAsync(context);
    return Results.Empty;
});

// Külalise vestlus ilma kontota: ws://localhost:5000/ws/support?guest=võti (GUID)
app.Map("/ws/support", async (HttpContext context, TableSocketHub hub) =>
{
    if (!context.WebSockets.IsWebSocketRequest)
        return Results.BadRequest("Ainult WebSocket ühendus.");
    if (!Guid.TryParse(context.Request.Query["guest"], out var key) || key == Guid.Empty)
        return Results.BadRequest("Vigane vestluse võti.");

    await hub.HandleGuestAsync(context, key.ToString("N"));
    return Results.Empty;
});
// mida teha, kui klient kathestab ühenduse (nt lehele&) siis te peate, et WebSocketi ühendus kathestatakse, aga serveris töötab 
//mitu  asjad nadu midddaaaaaaaaaaa
app.Run();
