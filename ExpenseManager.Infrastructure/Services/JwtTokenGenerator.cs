using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ExpenseManager.Application.Interfaces;
using ExpenseManager.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ExpenseManager.Infrastructure.Services;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(Guid userId, string email, string role)
    {
        // Read and validate configuration
        var secret = _configuration["JwtSettings:Secret"]
            ?? throw new InvalidOperationException("JwtSettings:Secret is not configured.");
        var issuer = _configuration["JwtSettings:Issuer"]
            ?? throw new InvalidOperationException("JwtSettings:Issuer is not configured.");
        var audience = _configuration["JwtSettings:Audience"]
            ?? throw new InvalidOperationException("JwtSettings:Audience is not configured.");
        var expiryMinutes = int.Parse(_configuration["JwtSettings:ExpiryMinutes"] ?? "60");
        

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
