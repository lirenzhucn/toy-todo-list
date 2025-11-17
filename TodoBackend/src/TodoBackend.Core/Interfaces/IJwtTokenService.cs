namespace TodoBackend.Core.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateToken(object user);
    }
}