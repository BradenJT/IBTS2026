#nullable enable

namespace IBTS2026.Domain.Entities.Features.Users;

public class UserInvitation
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public int InvitedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }

    public static UserInvitation Create(
        string email,
        string role,
        int invitedByUserId,
        int expirationDays = 7)
    {
        return new UserInvitation
        {
            Email = email,
            Token = Guid.NewGuid().ToString("N"),
            Role = role,
            InvitedByUserId = invitedByUserId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
            IsUsed = false
        };
    }

    public bool IsValid => !IsUsed && DateTime.UtcNow < ExpiresAt;

    public void MarkAsUsed()
    {
        IsUsed = true;
        UsedAt = DateTime.UtcNow;
    }
}
