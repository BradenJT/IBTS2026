namespace IBTS2026.Api.Contracts.Users
{
    public sealed class UpdateUserRequest
    {
        public int UsertId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role {  get; set; } = string.Empty;
    }
}
