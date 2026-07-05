using Principados.Domain.Entities;

namespace Principados.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}
