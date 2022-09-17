using MediatR;
using SmtpForwarder.Application.Interfaces.Authorization;

namespace SmtpForwarder.Application.Events.AuthorizationEvents;

public record ValidatePassword(string Password, byte[] Hash) : IRequest<bool>;

public class ValidatePasswordHandler : IRequestHandler<ValidatePassword, bool>
{

    private readonly IPasswordHasher _passwordHasher;

    public ValidatePasswordHandler(IPasswordHasher passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }

    public async Task<bool> Handle(ValidatePassword request, CancellationToken cancellationToken) =>
        await Task.Run(() => _passwordHasher.Validate(request.Password, request.Hash), cancellationToken);
}