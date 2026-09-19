using BookingHub.Application.Abstractions;
using BookingHub.Application.Abstractions.Authentication;
using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Domain.Organizations;
using BookingHub.Domain.Users;

namespace BookingHub.Application.Authentication.RefreshSession;

public sealed class RefreshSessionHandler
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IOrganizationMemberRepository _organizationMemberRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IAccessTokenProvider _accessTokenProvider;
    private readonly IRefreshTokenProvider _refreshTokenProvider;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshSessionHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        IOrganizationMemberRepository organizationMemberRepository,
        IOrganizationRepository organizationRepository,
        IAccessTokenProvider accessTokenProvider,
        IRefreshTokenProvider refreshTokenProvider,
        IGuidGenerator guidGenerator,
        IClock clock,
        IUnitOfWork unitOfWork)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _organizationMemberRepository = organizationMemberRepository;
        _organizationRepository = organizationRepository;
        _accessTokenProvider = accessTokenProvider;
        _refreshTokenProvider = refreshTokenProvider;
        _guidGenerator = guidGenerator;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<RefreshSessionResult> HandleAsync(
        RefreshSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentException.ThrowIfNullOrWhiteSpace(
            command.RefreshToken);

        var utcNow =
            _clock.UtcNow;

        var tokenHash =
            _refreshTokenProvider.Hash(
                command.RefreshToken);

        var currentToken =
            await _refreshTokenRepository.GetByTokenHashAsync(
                tokenHash,
                cancellationToken);

        if (currentToken is null ||
            !currentToken.IsActive(utcNow))
        {
            throw new InvalidRefreshTokenException();
        }

        var user =
            await _userRepository.GetByIdAsync(
                currentToken.UserId,
                cancellationToken);

        if (user is null ||
            !user.IsActive)
        {
            throw new InvalidRefreshTokenException();
        }

        var membership =
            await _organizationMemberRepository.GetByOrganizationAndUserAsync(
                currentToken.OrganizationId,
                user.Id,
                cancellationToken);

        if (membership is null ||
            membership.Status != OrganizationMemberStatus.Active)
        {
            throw new InvalidRefreshTokenException();
        }

        var organization =
            await _organizationRepository.GetByIdAsync(
                currentToken.OrganizationId,
                cancellationToken);

        if (organization is null ||
            organization.Status != OrganizationStatus.Active)
        {
            throw new InvalidRefreshTokenException();
        }

        var accessToken =
            _accessTokenProvider.Create(
                user.Id,
                organization.Id,
                user.Email,
                membership.Role,
                utcNow);

        var generatedRefreshToken =
            _refreshTokenProvider.Generate(
                utcNow);

        var replacementTokenId =
            _guidGenerator.NewGuid();

        var replacementToken =
            RefreshToken.Create(
                replacementTokenId,
                user.Id,
                organization.Id,
                generatedRefreshToken.Hash,
                generatedRefreshToken.ExpiresAtUtc,
                utcNow);

        currentToken.Revoke(
            utcNow,
            replacementTokenId);

        await _refreshTokenRepository.AddAsync(
            replacementToken,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new RefreshSessionResult(
            accessToken.Value,
            accessToken.ExpiresAtUtc,
            generatedRefreshToken.Value,
            generatedRefreshToken.ExpiresAtUtc,
            user.Id,
            organization.Id,
            membership.Role);
    }
}
