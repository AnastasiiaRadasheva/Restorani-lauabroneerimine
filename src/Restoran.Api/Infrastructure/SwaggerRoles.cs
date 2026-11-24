using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Restoran.Api.Infrastructure;

/// <summary>
/// Swaggeri dokumendid rollide kaupa: igas on ainult need toimingud, mida see roll ka veebilehel teha saab
/// (nt klient ei näe laudade haldust). Kontrollitakse samu [Authorize]/[AllowAnonymous] atribuute, mida server kasutab.
/// Pärast sisselogimist valib Swagger ise sisseloginud kasutaja rolli dokumendi (vt HeadScript).
/// </summary>
public static class SwaggerRoles
{
    public record Doc(string Name, string Title, string? Role, string Description);

    public static readonly Doc[] Docs =
    {
        new("kulaline", "Külaline (sisse logimata)", null, "Toimingud, mida saab teha sisse logimata: restoranid, menüü, vabad ajad, registreerimine ja sisselogimine."),
        new("klient", "Klient", "Customer", "Sisse logitud kliendi toimingud: oma broneeringud, eeltellimus, makse, sõnumid, õhtud, profiil."),
        new("teenindaja", "Teenindaja", "Waiter", "Teenindaja toimingud: laudade plaan, kalender, broneeringute nimekiri, telefonibroneering, küsimused."),
        new("admin", "Administraator", "Admin", "Administraatori toimingud: kõik teenindaja toimingud ning restoranide, laudade, menüü ja õhtute haldus."),
        new("teenus", "Taustateenus (Worker)", "Service", "Taustateenuse toimingud: meeldetuletused, saabumata külalised, maksmata broneeringud."),
        new("v1", "Kõik toimingud", "*", "Kogu API. Server kontrollib õigusi ka siin – võõra rolli toiming tagastab 403."),
    };

    public static bool Visible(string docName, ApiDescription api)
    {
        var doc = Docs.FirstOrDefault(d => d.Name == docName);
        if (doc is null) return false;
        if (doc.Role == "*") return true;

        var meta = api.ActionDescriptor.EndpointMetadata;
        if (meta.OfType<IAllowAnonymous>().Any()) return true;
        var rules = meta.OfType<IAuthorizeData>().ToList();
        if (rules.Count == 0) return true;   // avalik toiming
        if (doc.Role is null) return false;  // külaline ei näe sisselogimist nõudvaid toiminguid

        // Kõik [Authorize] tingimused (kontrolleril ja meetodil) peavad sobima
        return rules.All(r => string.IsNullOrWhiteSpace(r.Roles)
                              || r.Roles.Split(',').Select(x => x.Trim()).Contains(doc.Role));
    }

    /// <summary>
    /// Swagger UI lisad: peidab OAuth2 aknast tarbetud väljad ja avab sisseloginud rolli dokumendi.
    /// Rolli vahetumisel laaditakse leht uuesti (?urls.primaryName=...), et sisselogimise aken oleks puhas.
    /// </summary>
    public static string HeadContent
    {
        get
        {
            var titles = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string>
            {
                [""] = Docs[0].Title,
                ["Customer"] = Docs.First(d => d.Role == "Customer").Title,
                ["Waiter"] = Docs.First(d => d.Role == "Waiter").Title,
                ["Admin"] = Docs.First(d => d.Role == "Admin").Title,
                ["Service"] = Docs.First(d => d.Role == "Service").Title,
            });
            return """
                <style>
                  /* Sisselogimiseks piisab e-postist ja paroolist (client_id ja client_secret pole vaja) */
                  .auth-container label[for=password_type], .auth-container #password_type,
                  .auth-container label[for=client_id_password], .auth-container #client_id_password,
                  .auth-container label[for=client_secret_password], .auth-container #client_secret_password { display: none !important; }
                </style>
                <script>
                  (function () {
                    var TITLES = __TITLES__;
                    var ROLE = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
                    function role() {
                      try {
                        var a = JSON.parse(localStorage.getItem('authorized') || '{}');
                        var t = a.Login && a.Login.token && a.Login.token.access_token;
                        if (!t) return '';
                        var p = JSON.parse(decodeURIComponent(escape(atob(t.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')))));
                        return p[ROLE] || p.role || '';
                      } catch (e) { return ''; }
                    }
                    function sync() {
                      var r = role();
                      var seen = sessionStorage.getItem('argent-swagger-role');
                      if (seen === r) return;            // roll ei muutunud – käsitsi valitud dokument jääb alles
                      sessionStorage.setItem('argent-swagger-role', r);
                      var want = TITLES[r] || TITLES[''];
                      var now = new URLSearchParams(location.search).get('urls.primaryName');
                      if (seen === null && now === want) return;
                      location.replace(location.pathname + '?urls.primaryName=' + encodeURIComponent(want));
                    }
                    sync();
                    setInterval(sync, 700);
                  })();
                </script>
                """.Replace("__TITLES__", titles);
        }
    }
}
