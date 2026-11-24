namespace Restoran.Application.Common;

/// <summary>
/// Telefoninumbri võrdlemine: arvesse lähevad ainult numbrid ja Eesti suunakood 372 jäetakse ära,
/// seega "+372 5555 1234" ja "55551234" on sama number.
/// </summary>
public static class PhoneNumber
{
    public static string Digits(string? phone)
    {
        var d = new string((phone ?? "").Where(char.IsDigit).ToArray());
        if (d.StartsWith("00")) d = d[2..];
        if (d.StartsWith("372") && d.Length > 8) d = d[3..];
        return d;
    }

    public static bool Same(string? a, string? b)
    {
        var x = Digits(a);
        return x.Length > 0 && x == Digits(b);
    }
}
