using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BroomErp.Application.Common.Interfaces;
using BroomErp.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BroomErp.Infrastructure.Services;

public class JwtProvider : IJwtProvider
{
    private readonly IConfiguration _config;

    public JwtProvider(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(int userId, Role role, string email)
    {
        var secret = _config["Jwt:Secret"] ?? "BroomErpSuperSecretKeyMustBe32CharsLongAtLeast!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role.ToString()),
            new Claim("sub", userId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "BroomErp",
            audience: _config["Jwt:Audience"] ?? "BroomErpApp",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool Verify(string password, string passwordHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch
        {
            return false;
        }
    }
}

public class PdfGenerator : IPdfGenerator
{
    public Task<byte[]> GenerateQuotationPdfAsync(int quotationId, CancellationToken cancellationToken = default)
    {
        var content = $"%PDF-1.4\n% Broom ERP Quotation #{quotationId}\n1 0 obj\n<< /Title (Quotation #{quotationId}) >>\nendobj\ntrailer\n<< /Root 1 0 R >>\n%%EOF";
        return Task.FromResult(Encoding.UTF8.GetBytes(content));
    }

    public Task<byte[]> GenerateInvoicePdfAsync(int invoiceId, CancellationToken cancellationToken = default)
    {
        var content = $"%PDF-1.4\n% Broom ERP Invoice #{invoiceId}\n1 0 obj\n<< /Title (Invoice #{invoiceId}) >>\nendobj\ntrailer\n<< /Root 1 0 R >>\n%%EOF";
        return Task.FromResult(Encoding.UTF8.GetBytes(content));
    }
}
