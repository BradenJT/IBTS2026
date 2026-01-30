namespace IBTS2026.Web.Models
{
    public sealed record UserModel(
        int UserId,
        string Email,
        string FirstName,
        string LastName,
        string Role,
        DateTime? CreatedAt = null);

    /// <summary>
    /// Simplified user model for dropdown/lookup purposes.
    /// </summary>
    public sealed record UserLookupModel(
        int UserId,
        string FullName);

    public sealed record CreateUserModel
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public sealed record UpdateUserModel
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
