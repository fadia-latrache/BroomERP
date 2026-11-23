using BroomErp.Domain.Common;
using BroomErp.Domain.Enums;

namespace BroomErp.Application.Common.Interfaces;

public interface ICurrentUserContext
{
    int? UserId { get; }
    string? Email { get; }
    Role? Role { get; }
    bool IsAuthenticated { get; }
}

public interface IJwtProvider
{
    string GenerateToken(int userId, Role role, string email);
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}

public interface ISequenceGenerator
{
    Task<string> NextAsync(string prefix, CancellationToken cancellationToken = default);
}

public interface IPdfGenerator
{
    Task<byte[]> GenerateQuotationPdfAsync(int quotationId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateInvoicePdfAsync(int invoiceId, CancellationToken cancellationToken = default);
}
