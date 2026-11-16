namespace Restoran.Application.Interfaces;

/// <summary>Praegune aeg. Liidesena, et ühiktestides saaks aega ette anda.</summary>
public interface IClock
{
    DateTime Now { get; }
}
