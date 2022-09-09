namespace Application.Interfaces.Authorization;

public interface IPasswordHasher
{
    byte[] HashPassword(string password);

    bool Validate(string password, byte[] hash);
    
}