using Application.Interfaces.Authorization;
using MediatR;

namespace Application.Events.AuthorizationEvents;

public record GetPasswordHash(string Password) : IRequest<byte[]>;

public class GetPasswordHashHandler : IRequestHandler<GetPasswordHash, byte[]>
{
    private readonly IPasswordHasher _passwordHasher;

    public GetPasswordHashHandler(IPasswordHasher passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }

    public async Task<byte[]> Handle(GetPasswordHash request, CancellationToken cancellationToken) =>
        await Task.Run(() => _passwordHasher.HashPassword(request.Password), cancellationToken);
}