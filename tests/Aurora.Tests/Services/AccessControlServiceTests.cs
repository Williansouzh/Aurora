using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Services;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace Aurora.Tests.Services;

public class AccessControlServiceTests
{
    private const string ModuleKey = "finances";
    private const string UserId = "user1";

    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IModuleCatalogRepository> _modules = new();
    private readonly Mock<IPlanRepository> _plans = new();
    private readonly Mock<IUserSubscriptionRepository> _subscriptions = new();
    private readonly Mock<IUserModuleOverrideRepository> _overrides = new();
    private readonly Mock<ILifeAreaCatalogRepository> _lifeAreas = new();
    private readonly Mock<ICacheService> _cache = new(); // unconfigured => cache miss, hits repos

    private AccessControlService CreateService() => new(
        _users.Object, _modules.Object, _plans.Object, _subscriptions.Object,
        _overrides.Object, _lifeAreas.Object, _cache.Object);

    private void Setup(
        UserRole role = UserRole.User,
        UserStatus status = UserStatus.Active,
        bool planHasModule = false,
        ModuleReleaseStage stage = ModuleReleaseStage.Released,
        ModuleStatus moduleStatus = ModuleStatus.Enabled,
        List<UserModuleOverride>? overrides = null)
    {
        _users.Setup(x => x.GetByIdAsync(UserId))
            .ReturnsAsync(new User { Id = UserId, Name = "T", Email = "t@t.com", Role = role, Status = status });

        _modules.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new ModuleCatalogItem { Key = ModuleKey, Status = moduleStatus, ReleaseStage = stage, RequiredRole = UserRole.User }]);

        if (planHasModule)
        {
            _subscriptions.Setup(x => x.GetActiveByUserAsync(UserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserSubscription { UserId = UserId, PlanId = "plan1", Status = SubscriptionStatus.Active });
            _plans.Setup(x => x.GetByIdAsync("plan1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Plan { Id = "plan1", Key = "pro", ModuleKeys = [ModuleKey] });
        }

        _overrides.Setup(x => x.GetByUserAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(overrides ?? []);
    }

    [Fact]
    public async Task Plano_com_o_modulo_libera_acesso()
    {
        Setup(planHasModule: true);
        var decision = await CreateService().CanAccessModuleAsync(UserId, ModuleKey);
        decision.IsAllowed.Should().BeTrue();
    }

    [Fact]
    public async Task Sem_plano_o_modulo_fica_bloqueado()
    {
        Setup(planHasModule: false);
        var decision = await CreateService().CanAccessModuleAsync(UserId, ModuleKey);
        decision.IsAllowed.Should().BeFalse();
        decision.Reason.Should().Be(AccessDecisionReason.UpgradeRequired);
    }

    [Fact]
    public async Task SuperAdmin_sempre_tem_acesso()
    {
        Setup(role: UserRole.SuperAdmin, planHasModule: false);
        var decision = await CreateService().CanAccessModuleAsync(UserId, ModuleKey);
        decision.IsAllowed.Should().BeTrue();
    }

    [Fact]
    public async Task Usuario_inativo_e_bloqueado()
    {
        Setup(status: UserStatus.Suspended, planHasModule: true);
        var decision = await CreateService().CanAccessModuleAsync(UserId, ModuleKey);
        decision.IsAllowed.Should().BeFalse();
        decision.Reason.Should().Be(AccessDecisionReason.UserInactive);
    }

    [Fact]
    public async Task Override_deny_bloqueia_mesmo_com_plano()
    {
        Setup(planHasModule: true, overrides:
            [new UserModuleOverride { UserId = UserId, ModuleKey = ModuleKey, Access = ModuleAccess.Deny }]);
        var decision = await CreateService().CanAccessModuleAsync(UserId, ModuleKey);
        decision.IsAllowed.Should().BeFalse();
        decision.Reason.Should().Be(AccessDecisionReason.DeniedByOverride);
    }

    [Fact]
    public async Task Override_readonly_bloqueia_acao_de_escrita()
    {
        Setup(planHasModule: true, overrides:
            [new UserModuleOverride { UserId = UserId, ModuleKey = ModuleKey, Access = ModuleAccess.Readonly }]);
        var decision = await CreateService().CanAccessModuleAsync(UserId, ModuleKey, action: "write");
        decision.IsAllowed.Should().BeFalse();
        decision.Reason.Should().Be(AccessDecisionReason.Readonly);
    }

    [Fact]
    public async Task Modulo_interno_sem_override_fica_bloqueado()
    {
        Setup(planHasModule: true, stage: ModuleReleaseStage.Internal);
        var decision = await CreateService().CanAccessModuleAsync(UserId, ModuleKey);
        decision.IsAllowed.Should().BeFalse();
        decision.Reason.Should().Be(AccessDecisionReason.BetaOnly);
    }
}
