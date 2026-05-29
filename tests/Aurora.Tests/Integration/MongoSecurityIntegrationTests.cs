using Aurora.Application.Features.Auth.Common;
using Aurora.Domain.Entities;
using Aurora.Infrastructure.Persistence.Mongo;
using Aurora.Infrastructure.Persistence.Repositories;
using Aurora.Infrastructure.Security;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Testcontainers.MongoDb;
using Xunit;

namespace Aurora.Tests.Integration;

public class MongoSecurityIntegrationTests
{
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Deve_persistir_email_com_hash_e_valor_criptografado()
    {
        if (!IntegrationTestEnvironment.ShouldRunDockerTests())
        {
            return;
        }

        await using var mongo = new MongoDbBuilder("mongo:7").Build();
        await mongo.StartAsync();

        var context = new MongoContext(Options.Create(new MongoSettings
        {
            ConnectionString = mongo.GetConnectionString(),
            DatabaseName = "aurora_security_tests"
        }));
        var encryption = new AesGcmEncryptionService(Options.Create(new EncryptionSettings
        {
            Key = "integration-test-encryption-key-32",
            HashKey = "integration-test-hash-key-32"
        }));
        var users = new UserRepository(context);

        var user = new User { Name = "Security Test", PasswordHash = "hash" };
        UserSecurityMapper.SetEmail(user, "Person@Test.com", encryption);

        await users.AddAsync(user);

        var fetched = await users.GetByEmailHashAsync(encryption.HashDeterministic("person@test.com"));

        fetched.Should().NotBeNull();
        fetched!.Email.Should().Be("person@test.com");
        fetched.EmailHash.Should().NotBeNullOrWhiteSpace();
        fetched.EmailEncrypted.Should().NotContain("@");
        encryption.Decrypt(fetched.EmailEncrypted).Should().Be("person@test.com");
    }
}
