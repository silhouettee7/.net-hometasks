using System.Text.Encodings.Web;

namespace RESTAuth.Api.CustomSessionAuth;

using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

public class SessionTokenAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IConfiguration config)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var token = Request.Headers["X-Session-Token"].FirstOrDefault();
        if (string.IsNullOrEmpty(token))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var secret = config["Auth:Secret"];
        if (string.IsNullOrEmpty(secret))
        {
            return Task.FromResult(AuthenticateResult.Fail("Auth secret not configured"));
        }

        if (!TryValidateToken(token, secret, out var userId, out var role))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid session token"));
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, userId),
            new(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, SessionTokenDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SessionTokenDefaults.AuthenticationScheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private bool TryValidateToken(string token, string secret, out string telegramChatId, out string role)
    {
        telegramChatId = "";
        role = "";

        var parts = token.Split(':', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3) return false;

        telegramChatId = parts[0];
        role = parts[1];
        var signature = parts[2];

        var payload = $"{telegramChatId}:{role}";
        var expectedSignature = ComputeHmac(payload, secret);

        return signature == expectedSignature;
    }

    private string ComputeHmac(string data, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var bytes = Encoding.UTF8.GetBytes(data);
        var hash = hmac.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
