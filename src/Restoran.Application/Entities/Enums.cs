namespace Restoran.Application.Entities;

/// <summary>Kasutaja roll süsteemis.</summary>
public enum UserRole
{
    Admin,
    Waiter,
    Customer,
    Service
}

/// <summary>Laua asukoht: saal, terrass või teine korrus.</summary>
public enum TableLocation
{
    Hall,
    Terrace,
    Upstairs
}

/// <summary>Broneeringu olek.</summary>
public enum ReservationStatus
{
    Active,
    Arrived,
    Cancelled,
    NoShow,
    /// <summary>Personali tehtud broneering, mis ootab kliendi makset (makselink saadeti).</summary>
    PendingPayment
}
