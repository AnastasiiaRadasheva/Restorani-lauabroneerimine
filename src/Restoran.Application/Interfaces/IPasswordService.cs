using Restoran.Application.Entities;

namespace Restoran.Application.Interfaces;

public interface IPasswordService
{
    string Hash(User user, string password);
    bool Verify(User user, string password);
}
