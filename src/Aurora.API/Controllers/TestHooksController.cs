using Aurora.Application.Abstractions.Security;
using Aurora.Application.Features.Auth.Common;
using Aurora.Infrastructure.Persistence.Mongo;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Aurora.API.Controllers;

[ApiController, Route("__test")]
public class TestHooksController(
    IWebHostEnvironment environment,
    MongoContext context,
    IEncryptionService encryption,
    IMfaCodeGenerator codeGenerator) : ControllerBase
{
    [HttpGet("latest-mfa-code")]
    public async Task<IActionResult> LatestMfaCode([FromQuery] string email)
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        var emailHash = encryption.HashDeterministic(UserSecurityMapper.NormalizeEmail(email));
        var user = await context.Users.Find(x => x.EmailHash == emailHash || x.Email == email).FirstOrDefaultAsync();
        if (user is null)
        {
            return NotFound();
        }

        var challenge = await context.AuthChallenges
            .Find(x => x.UserId == user.Id && x.Purpose == AuthChallengePurposes.EmailMfa && x.ConsumedAt == null)
            .SortByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

        if (challenge is null)
        {
            return NotFound();
        }

        for (var value = 0; value <= 999999; value++)
        {
            var code = value.ToString("D6");
            if (codeGenerator.VerifySecret(code, challenge.CodeHash))
            {
                return Ok(new { challengeId = challenge.Id, code });
            }
        }

        return NotFound();
    }
}
