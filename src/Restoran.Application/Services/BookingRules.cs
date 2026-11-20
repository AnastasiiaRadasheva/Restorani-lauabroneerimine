using Restoran.Application.Entities;

namespace Restoran.Application.Services;

/// <summary>Broneerimise reeglid ühes kohas (lihtne testida).</summary>
public static class BookingRules
{
    /// <summary>Ühe broneeringu kestus.</summary>
    public static readonly TimeSpan Duration = TimeSpan.FromHours(2);

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

    /// <summary>Kas broneering hõivab lauda (tühistatud ja no-show ei hõiva).</summary>
    public static bool Occupies(ReservationStatus status) =>
        status is ReservationStatus.Active or ReservationStatus.Arrived;

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
    /// Laua ise valimise tasu: metrdoteli valik on tasuta, ise valides 10 €,
    /// tipptundidel (18:00-20:00) 30 €.
    /// </summary>
    public static decimal SeatFee(bool chosenByGuest, TimeOnly time) =>
        !chosenByGuest ? 0m : IsPeak(time) ? 30m : 10m;

    /// <summary>Klient saab muuta ainult siis, kui alguseni on rohkem kui 24 tundi.</summary>
    public static bool CanCustomerModify(DateTime start, DateTime now) => start - now > ChangeLock;
}
