using IBTS2026.Web.Models.Auth;

namespace IBTS2026.Web.Components.Pages.Auth
{
    public partial class Register
    {
        private RegisterModel _model = new();
        private bool _isRegistering;
        private string? _errorMessage;
        private string? _successMessage;

        private async Task HandleRegisterAsync()
        {
            _errorMessage = null;
            _successMessage = null;

            // Validate required fields
            if (string.IsNullOrWhiteSpace(_model.Email) ||
                string.IsNullOrWhiteSpace(_model.Password) ||
                string.IsNullOrWhiteSpace(_model.FirstName) ||
                string.IsNullOrWhiteSpace(_model.LastName))
            {
                _errorMessage = "Please fill in all fields.";
                return;
            }

            // Validate password match
            if (_model.Password != _model.ConfirmPassword)
            {
                _errorMessage = "Passwords do not match.";
                return;
            }

            // Validate password strength
            if (_model.Password.Length < 8)
            {
                _errorMessage = "Password must be at least 8 characters.";
                return;
            }

            _isRegistering = true;

            try
            {
                var (success, error) = await AuthService.RegisterAsync(
                    _model.Email,
                    _model.Password,
                    _model.FirstName,
                    _model.LastName);

                if (success)
                {
                    _successMessage = "Account created successfully! You can now sign in.";
                    _model = new RegisterModel(); // Clear the form

                    // Redirect to login after a short delay
                    await Task.Delay(2000);
                    Navigation.NavigateTo("/login");
                }
                else
                {
                    _errorMessage = error ?? "Registration failed. Please try again.";
                }
            }
            catch (Exception ex)
            {
                _errorMessage = $"An error occurred: {ex.Message}";
            }
            finally
            {
                _isRegistering = false;
            }
        }
    }
}
