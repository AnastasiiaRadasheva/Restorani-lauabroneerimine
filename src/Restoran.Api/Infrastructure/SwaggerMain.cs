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
    /// Swagger UI lisad:
    /// – sisselogimise aknas on ainult e-post ja parool (client_id, client_secret, "basic" ja scope'i tekstid peidetakse) ja eestikeelsed tekstid;
    /// – nupp näitab, kes on sisse logitud ("Logi sisse" / "Nimi (roll) · Logi välja");
    /// – loendis on ainult sisseloginud rolli toimingud; sisse logimata külaline näeb avalikke toiminguid.
    /// Swagger UI enda elemente ei muudeta, tekstid vahetatakse CSS-iga.
    /// </summary>
    public const string HeadContent = """
        <style>
          /* Sisselogimise aken: ainult e-post ja parool */
          .auth-container .scope-def, .auth-container h4, .auth-container h6, .auth-container p:has(> code),
          .auth-container label[for=password_type], .auth-container #password_type,
          .auth-container label[for^=client_], .auth-container input[id^=client_],
          .auth-container .wrapper:has(> label[for^=client_]), .auth-container .wrapper:has(> label[for=password_type]),
          .argent-off { display: none !important; }
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
          .auth-wrapper .btn.authorize[data-argent] span { display: none; }
          .auth-wrapper .btn.authorize[data-argent]::before { content: attr(data-argent); white-space: nowrap; }
        </style>
        <script>
          (function () {
            var ROLE = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
            var NAME = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name';
            var RN = { Customer: 'klient', Waiter: 'teenindaja', Admin: 'administraator', Service: 'taustateenus' };
            var map = null;
            fetch('/swagger/v1/swagger.json').then(function (r) { return r.json(); }).then(function (d) {
              map = {};
              Object.keys(d.paths).forEach(function (p) {
                Object.keys(d.paths[p]).forEach(function (m) { map[m.toUpperCase() + ' ' + p] = d.paths[p][m]['x-roles'] || []; });
              });
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
                op.classList.toggle('argent-off', !!roles && roles.indexOf(role) < 0);
              });
              document.querySelectorAll('.opblock-tag-section').forEach(function (s) {
                var ops = s.querySelectorAll('.opblock');
                if (!ops.length) return;
                s.classList.toggle('argent-off', !Array.prototype.some.call(ops, function (o) { return !o.classList.contains('argent-off'); }));
              });
            }
            setInterval(tick, 300);
          })();
        </script>
        """;
}
