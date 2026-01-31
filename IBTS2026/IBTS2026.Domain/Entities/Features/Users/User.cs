#nullable enable

namespace IBTS2026.Domain.Entities.Features.Users;

public class User
{
    public int UserId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public DateTime? CreatedAt { get; set; }
    public string? PasswordHash { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LockoutEnd { get; set; }
    public int FailedLoginCount { get; set; }
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// A random value that changes whenever security-sensitive user properties change
    /// (password, role, email, deactivation). Used to invalidate existing tokens.
    /// </summary>
    public string SecurityStamp { get; set; } = Guid.NewGuid().ToString();

    public static User Create(
        string email,
        string firstName,
        string lastName,
        string role)
    {
        return new User
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Role = role,
            IsActive = true,
            FailedLoginCount = 0,
            CreatedAt = DateTime.UtcNow,
            SecurityStamp = Guid.NewGuid().ToString()
        };
    }

    public static User CreateWithPassword(
        string email,
        string firstName,
        string lastName,
        string role,
        string passwordHash)
    {
        return new User
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Role = role,
            PasswordHash = passwordHash,
            IsActive = true,
            FailedLoginCount = 0,
            CreatedAt = DateTime.UtcNow,
            SecurityStamp = Guid.NewGuid().ToString()
        };
    }

    public void ChangeFirstName(string firstName)
    {
        FirstName = firstName;
    }

    public void ChangeLastName(string lastName)
    {
        LastName = lastName;
    }

    public void ChangeEmail(string email)
    {
        Email = email;
        RegenerateSecurityStamp();
    }

    public void ChangeRole(string role)
    {
        Role = role;
        RegenerateSecurityStamp();
    }

    public void SetPassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        RegenerateSecurityStamp();
    }

    /// <summary>
    /// Regenerates the security stamp, invalidating all existing tokens for this user.
    /// </summary>
    public void RegenerateSecurityStamp()
    {
        SecurityStamp = Guid.NewGuid().ToString();
    }

    public void RecordLoginFailure()
    {
        FailedLoginCount++;
        if (FailedLoginCount >= 5)
        {
            LockoutEnd = DateTime.UtcNow.AddMinutes(15);
        }
    }

    public void RecordLoginSuccess()
    {
        FailedLoginCount = 0;
        LockoutEnd = null;
        LastLoginAt = DateTime.UtcNow;
    }

    public bool IsLockedOut => LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;

    public void Lock(TimeSpan duration)
    {
        LockoutEnd = DateTime.UtcNow.Add(duration);
    }

    public void Unlock()
    {
        LockoutEnd = null;
        FailedLoginCount = 0;
    }

    public void Deactivate()
    {
        IsActive = false;
        RegenerateSecurityStamp();
    }

    public void Activate()
    {
        IsActive = true;
    }
}
