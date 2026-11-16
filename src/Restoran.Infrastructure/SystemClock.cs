using Restoran.Application.Interfaces;

namespace Restoran.Infrastructure;

public class SystemClock : IClock
{
    public DateTime Now => DateTime.Now;
}
