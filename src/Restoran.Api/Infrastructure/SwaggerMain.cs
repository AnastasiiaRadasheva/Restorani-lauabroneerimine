using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Restoran.Api.Infrastructure;

/// <summary>
/// Swaggeris näidatakse ainult ülesande põhitoimingud (lauad, broneeringud, menüü, eeltellimus,
/// sisselogimine, õhtud, statistika ja taustateenuse toimingud), et loend oleks lühike ja selge.
/// Ülejäänud toimingud (nt sõnumid, märkmed, teavitused) töötavad API-s edasi, neid lihtsalt ei kuvata.
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

    public static bool Visible(string docName, ApiDescription api)
    {
        // Marsruudi piirangud ({id:int}) eemaldatakse võrdlemiseks
        var path = System.Text.RegularExpressions.Regex.Replace(api.RelativePath ?? "", @"\{(\w+)(:[^}]*)?\}", "{$1}");
        return Main.Contains($"{api.HttpMethod} {path}");
    }

    /// <summary>Sisselogimiseks piisab e-postist ja paroolist – client_id ja client_secret väljad peidetakse.</summary>
    public const string HeadContent = """
        <style>
          .auth-container label[for=password_type], .auth-container #password_type,
          .auth-container label[for=client_id_password], .auth-container #client_id_password,
          .auth-container label[for=client_secret_password], .auth-container #client_secret_password { display: none !important; }
        </style>
        """;
}
