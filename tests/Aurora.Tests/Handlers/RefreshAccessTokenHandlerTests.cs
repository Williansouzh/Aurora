using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Abstractions.Security;
using Aurora.Application.Features.Auth.Common;
using Aurora.Application.Features.Auth.Refresh;
using Aurora.Domain.Entities;
using Aurora.Domain.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace Aurora.Tests.Handlers;

public class RefreshAccessTokenHandlerTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IJwtTokenService> _jwt = new();
    private readonly Mock<IEncryptionService> _encryption = new();
    private readonly Mock<IAuditService> _audit = new();
    private readonly IDateTimeProvider _clock = new TestDateTimeProvider();

    private RefreshTokenHandler CreateHandler() =>
        new(_refreshTokenRepo.Object, _userRepo.Object, _jwt.Object, _clock, _encryption.Object, _audit.Object);

    private static readonly string ValidRawToken = "valid-raw-token-abc123";
    private static readonly string ValidHash = "hashed-refresh-token";

    private static User MakeUser() => new() { Id = "user1", Name = "Test", Email = "test@test.com" };

    private RefreshToken MakeToken(bool isRevoked = false, DateTime? expiresAt = null) => new()
    {
        Id = "tok1",
        TokenHash = ValidHash,
        UserId = "user1",
        ExpiresAt = expiresAt ?? DateTime.UtcNow.AddDays(7),
        IsRevoked = isRevoked,
    };

    private sealed class TestDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }

    private void SetupHappyPath(RefreshToken token)
    {
        _encryption.Setup(x => x.HashDeterministic(ValidRawToken)).Returns(ValidHash);
        _encryption.Setup(x => x.HashDeterministic(It.Is<string>(v => v != ValidRawToken))).Returns("new-refresh-hash");
        _audit.Setup(x => x.RecordAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _refreshTokenRepo.Setup(r => r.GetByHashAsync(ValidHash)).ReturnsAsync(token);
        _userRepo.Setup(r => r.GetByIdAsync("user1")).ReturnsAsync(MakeUser());
        _refreshTokenRepo.Setup(r => r.RevokeAsync("tok1")).Returns(Task.CompletedTask);
        _refreshTokenRepo.Setup(r => r.AddAsync(It.IsAny<RefreshToken>())).Returns(Task.CompletedTask);
        _jwt.Setup(j => j.Generate(It.IsAny<User>())).Returns("new-access-token");
        _jwt.Setup(j => j.ExpiresInSeconds).Returns(900);
    }

    [Fact]
    public async Task Deve_retornar_novo_access_token_para_refresh_token_valido()
    {
        var token = MakeToken();
        SetupHappyPath(token);

        var result = await CreateHandler().Handle(new RefreshTokenCommand(ValidRawToken), default);

        result.AccessToken.Should().Be("new-access-token");
        result.ExpiresIn.Should().Be(900);
        result.RawRefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Deve_revogar_token_antigo_apos_rotation()
    {
        var token = MakeToken();
        SetupHappyPath(token);

        await CreateHandler().Handle(new RefreshTokenCommand(ValidRawToken), default);

        _refreshTokenRepo.Verify(r => r.RevokeAsync("tok1"), Times.Once);
        _refreshTokenRepo.Verify(r => r.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
    }

    [Fact]
    public async Task Deve_retornar_erro_para_refresh_token_expirado()
    {
        var expiredToken = MakeToken(expiresAt: DateTime.UtcNow.AddSeconds(-1));
        _encryption.Setup(x => x.HashDeterministic(ValidRawToken)).Returns(ValidHash);
        _refreshTokenRepo.Setup(r => r.GetByHashAsync(ValidHash)).ReturnsAsync(expiredToken);

        var act = () => CreateHandler().Handle(new RefreshTokenCommand(ValidRawToken), default);

        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("*expirado*");
    }

    [Fact]
    public async Task Deve_retornar_erro_para_refresh_token_revogado()
    {
        var revokedToken = MakeToken(isRevoked: true);
        _encryption.Setup(x => x.HashDeterministic(ValidRawToken)).Returns(ValidHash);
        _audit.Setup(x => x.RecordAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _refreshTokenRepo.Setup(r => r.GetByHashAsync(ValidHash)).ReturnsAsync(revokedToken);

        var act = () => CreateHandler().Handle(new RefreshTokenCommand(ValidRawToken), default);

        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("*revogado*");
    }

    [Fact]
    public async Task Deve_retornar_erro_para_refresh_token_inexistente()
    {
        _refreshTokenRepo.Setup(r => r.GetByHashAsync(It.IsAny<string>()))
            .ReturnsAsync((RefreshToken?)null);

        var act = () => CreateHandler().Handle(new RefreshTokenCommand("token-falso"), default);

        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("*invalido*");
    }
}
