using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Restoran.Api.Infrastructure;

/// <summary>
/// Swaggeris näidatakse ainult ülesande põhitoimingud (lauad, broneeringud, menüü, eeltellimus,
/// sisselogimine, õhtud, statistika ja taustateenuse toimingud), et loend oleks lühike ja selge.
/// Ülejäänud toimingud (nt sõnumid, märkmed, teavitused) töötavad API-s edasi, neid lihtsalt ei kuvata.
/// Pärast sisselogimist näitab Swagger ainult neid toiminguid, mida sisseloginud roll teha tohib (nagu veebilehel).
/// </summary>
public static class SwaggerMain
{
    private static readonly HashSet<string> Main = new(StringComparer.OrdinalIgnoreCase)
    {
        // Registreerimine ja sisselogimine
        "POST api/auth/register", "POST api/auth/login", "GET api/auth/me",
        // Restoranid ja lauad
        "GET api/restaurants",
        "GET api/tables/available", "GET api/tables/plan",
        "POST api/tables", "PUT api/tables/{id}", "DELETE api/tables/{id}",
        // Menüü
        "GET api/menu", "GET api/menu/{id}", "POST api/menu", "PUT api/menu/{id}", "DELETE api/menu/{id}",
        // Broneeringud ja eeltellimus
        "POST api/reservations", "GET api/reservations/my", "GET api/reservations", "GET api/reservations/{id}",
        "PATCH api/reservations/{id}/cancel", "PATCH api/reservations/{id}/status", "POST api/reservations/{id}/preorder",
        // Gastronoomilised õhtud
        "GET api/events", "POST api/events/{id}/join", "GET api/events/registrations/my",
        // Statistika
        "GET api/stats",
        // Taustateenus (Worker)
        "GET api/reservations/remind-due", "PATCH api/reservations/{id}/reminded",
        "POST api/reservations/mark-no-show", "POST api/notifications",
    };

    private static readonly string[] AllRoles = { "Customer", "Waiter", "Admin", "Service" };

    public static bool Visible(string docName, ApiDescription api)
    {
        // Marsruudi piirangud ({id:int}) eemaldatakse võrdlemiseks
        var path = System.Text.RegularExpressions.Regex.Replace(api.RelativePath ?? "", @"\{(\w+)(:[^}]*)?\}", "{$1}");
        return Main.Contains($"{api.HttpMethod} {path}");
    }

    /// <summary>
    /// Rollid, kes toimingut teha tohivad (samad [Authorize]/[AllowAnonymous] atribuudid, mida server kontrollib).
    /// "guest" tähendab, et sisselogimist pole vaja.
    /// </summary>
    public static List<string> Roles(ApiDescription api)
    {
        // Registreerimine ja sisselogimine on ainult sisse logimata külalisele (nagu veebilehel)
        if (api.RelativePath is "api/auth/register" or "api/auth/login") return ["guest"];
        // "Minu broneeringud" on ainult kliendil – personalil endal broneeringuid pole, ta näeb kõiki (GET /api/reservations)
        if (api.RelativePath is "api/reservations/my" or "api/events/registrations/my") return ["Customer"];
        var meta = api.ActionDescriptor.EndpointMetadata;
        var rules = meta.OfType<IAuthorizeData>().ToList();
        if (meta.OfType<IAllowAnonymous>().Any() || rules.Count == 0) return ["guest", .. AllRoles];
        return AllRoles.Where(role => rules.All(r => string.IsNullOrWhiteSpace(r.Roles)
                                                     || r.Roles.Split(',').Select(x => x.Trim()).Contains(role))).ToList();
    }

    /// <summary>Lisab igale toimingule rollid (x-roles), mille järgi Swagger UI näitab sisseloginud rollile tema toiminguid.</summary>
    public class RolesFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Extensions ??= new Dictionary<string, IOpenApiExtension>();
            operation.Extensions["x-roles"] = new JsonNodeExtension(new JsonArray(Roles(context.ApiDescription).Select(r => (JsonNode?)r).ToArray()));
        }
    }

    /// <summary>
    /// Valmis näited päringu parameetritele: Swaggeris piisab toimingu avamisest ja nupust Execute, midagi pole vaja sisestada.
    /// Kuupäev on alati nädala pärast (et see oleks tulevikus ja broneerimise vahemikus).
    /// Kustutamise ja muutmise id jäetakse tühjaks, et päris andmeid kogemata ei muudetaks.
    /// </summary>
    public class ExamplesFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters is null) return;
            var method = context.ApiDescription.HttpMethod;
            var date = DateOnly.FromDateTime(DateTime.Today.AddDays(7)).ToString("yyyy-MM-dd");
            foreach (var p in operation.Parameters.OfType<OpenApiParameter>())
            {
                JsonNode? example = p.Name.ToLowerInvariant() switch
                {
                    "restaurantid" => 1,
                    "date" when p.In == ParameterLocation.Query && method == "GET" && context.ApiDescription.RelativePath!.StartsWith("api/tables") => date,
                    "from" when context.ApiDescription.RelativePath!.StartsWith("api/tables") => date,
                    "time" => "19:00",
                    "guests" => 2,
                    "days" => 7,
                    "page" => 1,
                    "pagesize" => 5,
                    "id" when method == "GET" => 1,
                    _ => null,
                };
                if (example is not null) p.Example = example;
            }
        }
    }

    /// <summary>
    /// Päringu keha näidetes on kuupäev alati nädala pärast (mitte kirjutatud kindel kuupäev, mis võib olla möödas),
    /// et nt broneeringu näidet saaks kohe Execute-nupuga proovida.
    /// </summary>
    public class DatesFilter : ISchemaFilter
    {
        public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema is not OpenApiSchema s || s.Properties is null) return;
            var date = DateOnly.FromDateTime(DateTime.Today.AddDays(7)).ToString("yyyy-MM-dd");
            // Registreerimise näide: iga kord uus e-post ja telefon, et Execute ei annaks "juba olemas"
            if (context.Type == typeof(Restoran.Application.DTOs.RegisterDto))
            {
                var n = DateTime.Now.ToString("MMddHHmmss");
                s.Example = new JsonObject
                {
                    ["name"] = "Mari Maasikas",
                    ["email"] = $"mari.{n}@example.com",
                    ["phone"] = $"+372 5{n.Substring(3, 3)} {n.Substring(6, 4)}",
                    ["password"] = "Parool123!",
                };
                return;
            }
            // Uue broneeringu valmis näide: laua valib süsteem ise, makse testkaardiga
            if (context.Type == typeof(Restoran.Application.DTOs.CreateReservationDto))
            {
                s.Example = new JsonObject
                {
                    ["restaurantId"] = 1,
                    ["date"] = date,
                    ["time"] = "19:00",
                    ["guests"] = 2,
                    ["specialRequest"] = "Aknaalune laud, kui võimalik",
                    ["items"] = new JsonArray(new JsonObject { ["menuItemId"] = 3, ["quantity"] = 2 }),
                    ["serviceLanguage"] = "et",
                    ["card"] = new JsonObject { ["cardHolder"] = "Kati Klient", ["cardNumber"] = "4242 4242 4242 4242", ["expiry"] = "12/28", ["cvc"] = "123" },
                };
                return;
            }
            foreach (var prop in s.Properties.Values.OfType<OpenApiSchema>())
                if (prop.Format == "date" && prop.Example is not null) prop.Example = date;
        }
    }

    /// <summary>Gruppide järjekord ja lühikirjeldused (loogilises järjekorras, mitte tähestiku järgi).</summary>
    private static readonly (string Name, string Description)[] Groups =
    {
        ("Auth", "Registreerimine ja sisselogimine"),
        ("Restaurants", "Restoranid"),
        ("Tables", "Vabad lauad ja laudade haldus"),
        ("Reservations", "Broneeringud, eeltellimus ja taustateenus"),
        ("Menu", "Menüü"),
        ("Events", "Gastronoomilised õhtud"),
        ("Stats", "Statistika (admin)"),
        ("Notifications", "Teavitused (taustateenus)"),
    };

    /// <summary>Järjestus Swaggeris: grupp loogilises järjekorras, grupi sees aadressi ja meetodi (GET, POST, PUT, PATCH, DELETE) järgi.</summary>
    public static string Order(ApiDescription api)
    {
        var group = api.ActionDescriptor.RouteValues.TryGetValue("controller", out var c) ? c : "";
        var i = Array.FindIndex(Groups, g => g.Name == group);
        var method = Array.IndexOf(new[] { "GET", "POST", "PUT", "PATCH", "DELETE" }, api.HttpMethod);
        return $"{(i < 0 ? 99 : i):00} {api.RelativePath} {method}";
    }

    /// <summary>Lisab gruppidele eestikeelsed kirjeldused samas järjekorras.</summary>
    public class GroupsFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument doc, DocumentFilterContext context)
        {
            doc.Tags = new HashSet<OpenApiTag>(Groups.Select(g => new OpenApiTag { Name = g.Name, Description = g.Description }));
        }
    }

    /// <summary>
    /// Swagger UI päringu vahelüli: kui registreerimise kehas on näite e-post (mari.NUMBER@example.com),
    /// asendatakse number ja telefon saatmise hetkel uutega. Kasutaja enda sisestatud andmeid ei muudeta.
    /// </summary>
    public const string RegisterInterceptor = """
        (req) => {
          try {
            if (req.method === 'POST' && /\/api\/auth\/register$/.test(req.url) && typeof req.body === 'string') {
              const b = JSON.parse(req.body);
              if (/^mari\.\d+@example\.com$/.test(b.email || '')) {
                const n = String(Date.now()).slice(-10);
                b.email = 'mari.' + n + '@example.com';
                b.phone = '+372 5' + n.slice(3, 6) + ' ' + n.slice(6, 10);
                req.body = JSON.stringify(b);
              }
            }
          } catch (e) { }
          return req;
        }
        """;

    /// <summary>
    /// Swagger UI lisad:
    /// – sisselogimise aknas on ainult e-post ja parool (client_id, client_secret, "basic" ja scope'i tekstid peidetakse) ja eestikeelsed tekstid;
    /// – nupp näitab, kes on sisse logitud ("Logi sisse" / "Nimi (roll) · Logi välja");
    /// – loendis on ainult sisseloginud rolli toimingud; sisse logimata külaline näeb avalikke toiminguid.
    /// Swagger UI enda elemente ei muudeta, tekstid vahetatakse CSS-iga; toimingud on peidus, kuni roll on kontrollitud.
    /// </summary>
    public const string HeadContent = """
        <style>
          /* Sisselogimise aken: ainult e-post ja parool */
          .auth-container .scope-def, .auth-container h4, .auth-container h6, .auth-container p:has(> code),
          .auth-container label[for=password_type], .auth-container #password_type,
          .auth-container label[for^=client_], .auth-container input[id^=client_],
          .auth-container .wrapper:has(> label[for^=client_]), .auth-container .wrapper:has(> label[for=password_type]),
          .swagger-ui .topbar { display: none !important; }
          /* Toimingud ja grupid on peidus, kuni skript on kontrollinud, et see roll tohib neid näha – võõraid toiminguid ei näe isegi hetkeks */
          .swagger-ui .opblock:not(.argent-on), .swagger-ui .opblock-tag-section:not(.argent-on) { display: none !important; }
          /* Lukuikoonid toimingute juures ära (sisselogimine käib ülemise nupuga) */
          .swagger-ui .opblock .authorization__btn { display: none !important; }
          .modal-ux-header h3 { font-size: 0 !important; }
          .modal-ux-header h3::after { content: 'Sisselogimine'; font-size: 20px; }
          .auth-container label[for=oauth_username], .auth-container label[for=oauth_password] { font-size: 0 !important; }
          .auth-container label[for=oauth_username]::before { content: 'E-post:'; font-size: 12px; }
          .auth-container label[for=oauth_password]::before { content: 'Parool:'; font-size: 12px; }
          .auth-btn-wrapper button { font-size: 0 !important; }
          .auth-btn-wrapper button::after { font-size: 14px; }
          .auth-btn-wrapper button.authorize::after { content: 'Logi sisse'; }
          .auth-btn-wrapper button.authorize[aria-label="Remove authorization"]::after { content: 'Logi välja'; }
          .auth-btn-wrapper button.btn-done::after { content: 'Sulge'; }
          /* Ülemine nupp: kes on sisse logitud */
          .auth-wrapper .btn.authorize span { display: none; }
          .auth-wrapper .btn.authorize::before { content: attr(data-argent); white-space: nowrap; }
        </style>
        <script>
          (function () {
            var ROLE = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
            var NAME = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name';
            var RN = { Customer: 'klient', Waiter: 'teenindaja', Admin: 'administraator', Service: 'taustateenus' };
            var map = null, tags = null;
            fetch('/swagger/v1/swagger.json').then(function (r) { return r.json(); }).then(function (d) {
              map = {};
              Object.keys(d.paths).forEach(function (p) {
                Object.keys(d.paths[p]).forEach(function (m) {
                  var o = d.paths[p][m], roles = o['x-roles'] || [];
                  map[m.toUpperCase() + ' ' + p] = roles;
                  (o.tags || []).forEach(function (t) { tags = tags || {}; tags[t] = (tags[t] || []).concat(roles); });
                });
              });
              tick();
            });
            function user() {
              try {
                var a = JSON.parse(localStorage.getItem('authorized') || '{}');
                var t = a.Login && a.Login.token && a.Login.token.access_token;
                if (!t) return null;
                var p = JSON.parse(decodeURIComponent(escape(atob(t.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')))));
                if (p.exp && p.exp * 1000 < Date.now()) return null;
                return { role: p[ROLE] || p.role || '', name: p[NAME] || p.name || '' };
              } catch (e) { return null; }
            }
            function tick() {
              var u = user();
              var role = u ? u.role : 'guest';
              var btn = document.querySelector('.auth-wrapper .btn.authorize');
              if (btn) {
                var txt = u ? u.name + ' (' + (RN[u.role] || u.role) + ') · Logi välja' : 'Logi sisse';
                if (btn.getAttribute('data-argent') !== txt) btn.setAttribute('data-argent', txt);
              }
              if (!map) return;
              document.querySelectorAll('.opblock').forEach(function (op) {
                var m = op.querySelector('.opblock-summary-method');
                var p = op.querySelector('.opblock-summary-path');
                if (!m || !p) return;
                var roles = map[m.textContent.trim() + ' ' + (p.getAttribute('data-path') || p.textContent.trim())];
                var on = !roles || roles.indexOf(role) >= 0;
                if (op.classList.contains('argent-on') !== on) op.classList.toggle('argent-on', on);
              });
              document.querySelectorAll('.opblock-tag-section').forEach(function (s) {
                // Grupp peidetakse, kui selles pole ühtegi selle rolli toimingut (töötab ka kokku pandud grupi puhul)
                var h = s.querySelector('.opblock-tag');
                var t = h && h.getAttribute('data-tag');
                var on = !t || !tags || !tags[t] || tags[t].indexOf(role) >= 0;
                if (s.classList.contains('argent-on') !== on) s.classList.toggle('argent-on', on);
              });
            }
            // Kontroll kohe, kui Swagger midagi joonistab (enne ekraanile jõudmist), ja varuks iga 300 ms
            new MutationObserver(tick).observe(document.documentElement, { childList: true, subtree: true });
            setInterval(tick, 300);
          })();
        </script>
        """;
}
