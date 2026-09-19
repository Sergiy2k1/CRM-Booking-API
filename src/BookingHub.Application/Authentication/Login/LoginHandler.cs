using BookingHub.Application.Abstractions;
using BookingHub.Application.Abstractions.Authentication;
using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Domain.Organizations;

namespace BookingHub.Application.Authentication.Login;

public sealed class LoginHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IOrganizationMemberRepository _organizationMemberRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAccessTokenProvider _accessTokenProvider;
    private readonly IClock _clock;

    public LoginHandler(
        IUserRepository userRepository,
        IOrganizationMemberRepository organizationMemberRepository,
        IOrganizationRepository organizationRepository,
        IPasswordHasher passwordHasher,
        IAccessTokenProvider accessTokenProvider,
        IClock clock)
    {
        _userRepository = userRepository;
        _organizationMemberRepository = organizationMemberRepository;
        _organizationRepository = organizationRepository;
        _passwordHasher = passwordHasher;
        _accessTokenProvider = accessTokenProvider;
        _clock = clock;
    }

    public async Task<LoginResult> HandleAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentException.ThrowIfNullOrWhiteSpace(command.Email);
        ArgumentException.ThrowIfNullOrWhiteSpace(command.Password);

        if (command.OrganizationId == Guid.Empty)
        {
            throw new ArgumentException(
                "Organization id cannot be empty.",
                nameof(command.OrganizationId));
        }

        var normalizedEmail =
            command.Email
                .Trim()
                .ToUpperInvariant();

        var user =
            await _userRepository.GetByNormalizedEmailAsync(
                normalizedEmail,
                cancellationToken);

        if (user is null ||
            !user.IsActive ||
            !_passwordHasher.Verify(
                user.PasswordHash,
                command.Password))
        {
            throw new InvalidCredentialsException();
        }

        var membership =
            await _organizationMemberRepository.GetByOrganizationAndUserAsync(
                command.OrganizationId,
                user.Id,
                cancellationToken);

        if (membership is null ||
            membership.Status != OrganizationMemberStatus.Active)
        {
            throw new InvalidCredentialsException();
        }

        var organization =
            await _organizationRepository.GetByIdAsync(
                command.OrganizationId,
                cancellationToken);

        if (organization is null ||
            organization.Status != OrganizationStatus.Active)
        {
            throw new InvalidCredentialsException();
        }

        var token =
            _accessTokenProvider.Create(
                user.Id,
                organization.Id,
                user.Email,
                membership.Role,
                _clock.UtcNow);

        return new LoginResult(
            token.Value,
            token.ExpiresAtUtc,
            user.Id,
            organization.Id,
            membership.Role);
    }
}
