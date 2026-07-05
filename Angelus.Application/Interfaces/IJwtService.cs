using Angelus.Domain.Entities;

namespace Angelus.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}
