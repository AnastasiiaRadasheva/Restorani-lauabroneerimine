using Restoran.Application.Entities;

namespace Restoran.Application.Services;

/// <summary>Broneerimise reeglid ühes kohas (lihtne testida).</summary>
public static class BookingRules
{
    /// <summary>
    /// Lühim külastus. Laud on broneeritud KOGU ÕHTUKS (kuni sulgemiseni),
    /// kuid viimane saabumine peab olema vähemalt 2 h enne sulgemist.
    /// </summary>
    public static readonly TimeSpan Duration = TimeSpan.FromHours(2);

    /// <summary>Kõige hilisem saabumise aeg (ka siis, kui restoran on avatud ööni).</summary>
    public static readonly TimeOnly LatestArrival = new(23, 30);

    /// <summary>Kui palju laudu võib üks külaline ühel õhtul broneerida.</summary>
    public const int MaxTablesPerEvening = 3;

    /// <summary>Õhtu lõpp. Kui sulgemisaeg on enne avamist (nt 03:00), on see järgmisel päeval.</summary>
    public static DateTime EveningEnd(DateOnly date, TimeOnly opens, TimeOnly closes) =>
        closes > opens ? date.ToDateTime(closes) : date.AddDays(1).ToDateTime(closes);

    /// <summary>Viimane saabumise aeg: 2 h enne sulgemist, kuid mitte hiljem kui 23:30.</summary>
    public static TimeOnly LastArrival(TimeOnly opens, TimeOnly closes)
    {
        var end = closes > opens ? closes.ToTimeSpan() : closes.ToTimeSpan() + TimeSpan.FromDays(1);
        var last = end - Duration;
        return last > LatestArrival.ToTimeSpan() ? LatestArrival : TimeOnly.FromTimeSpan(last);
    }

    /// <summary>Kui palju enne algust saab klient veel broneeringut muuta või tühistada.</summary>
    public static readonly TimeSpan ChangeLock = TimeSpan.FromHours(24);

    /// <summary>Kui kaua oodatakse klienti, enne kui laud vabastatakse (no-show).</summary>
    public static readonly TimeSpan NoShowAfter = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Kaks ajavahemikku kattuvad, kui üks algab enne teise lõppu ja lõpeb pärast teise algust.
    /// Näide: 18:00-20:00 ja 19:00-21:00 kattuvad; 18:00-20:00 ja 20:00-22:00 ei kattu.
    /// </summary>
    public static bool Overlaps(DateTime aStart, DateTime aEnd, DateTime bStart, DateTime bEnd) =>
        aStart < bEnd && aEnd > bStart;

    /// <summary>
    /// Päeva algus (kell 06:00). Broneeritud laud on suletud kogu päevaks; öösel kuni 03:00 kestev
    /// eelmise õhtu broneering ei sulge järgmist päeva.
    /// </summary>
    public static DateTime DayStart(DateTime time) =>
        (time.Hour < 6 ? time.Date.AddDays(-1) : time.Date).AddHours(6);

    /// <summary>Kas broneering hõivab lauda (tühistatud ja no-show ei hõiva).</summary>
    public static bool Occupies(ReservationStatus status) =>
        status is ReservationStatus.Active or ReservationStatus.Arrived or ReservationStatus.PendingPayment;

    /// <summary>
    /// Valib väikseima sobiva vaba laua, et 2 inimest ei võtaks 8-kohalist lauda.
    /// Võrdse suuruse korral eelistatakse väiksema numbriga lauda.
    /// </summary>
    public static DiningTable? PickBestTable(IEnumerable<DiningTable> tables, ISet<int> busyTableIds, int guests) =>
        tables
            .Where(t => t.IsActive && t.Seats >= guests && !busyTableIds.Contains(t.Id))
            .OrderBy(t => t.Seats)
            .ThenBy(t => t.Number)
            .FirstOrDefault();

    /// <summary>Tipptunnid: algus 18:00 kuni 20:00 (kaasa arvatud).</summary>
    public static bool IsPeak(TimeOnly time) => time >= new TimeOnly(18, 0) && time <= new TimeOnly(20, 0);

    /// <summary>
    /// Laua ise valimise tasu: metrdoteli valik on tasuta, ise valides 30 €,
    /// tipptundidel (18:00-20:00) 40 €.
    /// </summary>
    public static decimal SeatFee(bool chosenByGuest, TimeOnly time) =>
        !chosenByGuest ? 0m : IsPeak(time) ? 40m : 30m;

    /// <summary>Klient saab muuta ainult siis, kui alguseni on rohkem kui 24 tundi.</summary>
    public static bool CanCustomerModify(DateTime start, DateTime now) => start - now > ChangeLock;
}
