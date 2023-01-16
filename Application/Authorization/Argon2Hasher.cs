using System.Text;
using Isopoh.Cryptography.Argon2;
using SmtpForwarder.Application.Interfaces.Authorization;

namespace SmtpForwarder.Application.Authorization;

internal class Argon2Hasher : IPasswordHasher
{

    public byte[] HashPassword(string password) =>
        Encoding.UTF8.GetBytes(Argon2.Hash(password, timeCost: 2, memoryCost: 37888));

    public bool Validate(string password, byte[] hash) =>
        Argon2.Verify(Encoding.UTF8.GetString(hash), password);
}