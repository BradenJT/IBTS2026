#nullable enable

namespace IBTS2026.Domain.Entities;
public partial class User
{
    public static User Create(
        string email,
        string firstName,
        string lastName,
        string role)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty.", nameof(email));

        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be null or empty.", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be null or empty.", nameof(lastName));

        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("Role cannot be null or empty.", nameof(role));

        return new User
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Role = role,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void ChangeFirstName(string firstName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be null or empty.", nameof(firstName));
        FirstName = firstName;
    }

    public void ChangeLastName(string lastName)
    {
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be null or empty.", nameof(lastName));
        LastName = lastName;
    }

    public void ChangeRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("Role cannot be null or empty.", nameof(role));
        Role = role;
    }
}
