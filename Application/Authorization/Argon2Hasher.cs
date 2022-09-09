using System.Text;
using Application.Interfaces.Authorization;
using Isopoh.Cryptography.Argon2;

namespace Application.Authorization;

internal class Argon2Hasher : IPasswordHasher
{

    public byte[] HashPassword(string password) =>
        Encoding.UTF8.GetBytes(Argon2.Hash(password));

    public bool Validate(string password, byte[] hash) =>
        Argon2.Verify(Encoding.UTF8.GetString(hash), password);
}