using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IBTS2026.Application.Abstractions.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace IBTS2026.Infrastructure.Services;

internal sealed class JwtTokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtTokenService> _logger;

    public JwtTokenService(IConfiguration configuration, ILogger<JwtTokenService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string GenerateToken(int userId, string email, string role)
    {
        _logger.LogInformation(
            "JWT DEBUG: GenerateToken called for UserId={UserId}, Email={Email}, Role='{Role}'",
            userId, email, role);

        var key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT Key is not configured");
        var issuer = _configuration["Jwt:Issuer"] ?? "IBTS2026";
        var audience = _configuration["Jwt:Audience"] ?? "IBTS2026";
        var expirationMinutes = int.TryParse(_configuration["Jwt:ExpirationMinutes"], out var expMin)
            ? expMin
            : 60;

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        _logger.LogInformation("JWT DEBUG: Claims being added to token:");
        foreach (var claim in claims)
        {
            _logger.LogInformation("JWT DEBUG:   Type={ClaimType}, Value={ClaimValue}", claim.Type, claim.Value);
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        _logger.LogInformation("JWT DEBUG: Token generated, length={Length}", tokenString.Length);

        return tokenString;
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var key = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(key))
            return null;

        var issuer = _configuration["Jwt:Issuer"] ?? "IBTS2026";
        var audience = _configuration["Jwt:Audience"] ?? "IBTS2026";

        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }
}
