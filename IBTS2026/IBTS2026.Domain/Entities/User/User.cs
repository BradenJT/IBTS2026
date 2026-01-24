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
        FirstName = firstName;
    }

    public void ChangeLastName(string lastName)
    {
        LastName = lastName;
    }

    public void ChangeEmail(string email)
    {
        Email = email;
    }

    public void ChangeRole(string role)
    {
        Role = role;
    }
}
