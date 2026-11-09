namespace Restoran.Application.Entities;

/// <summary>Kasutaja roll süsteemis.</summary>
public enum UserRole
{
    Admin,
    Waiter,
    Customer,
    Service
}

/// <summary>Laua asukoht: saal või terrass.</summary>
public enum TableLocation
{
    Hall,
    Terrace
}

/// <summary>Broneeringu olek.</summary>
public enum ReservationStatus
{
    Active,
    Arrived,
    Cancelled,
    NoShow
}
