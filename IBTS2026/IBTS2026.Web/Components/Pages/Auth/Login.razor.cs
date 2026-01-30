using IBTS2026.Web.Models.Auth;

namespace IBTS2026.Web.Components.Pages.Auth
{
    public partial class Login
    {
        private LoginModel _model = new();
        private bool _isLoggingIn;
        private string? _errorMessage;

        private async Task HandleLoginAsync()
        {
            if (string.IsNullOrWhiteSpace(_model.Email) || string.IsNullOrWhiteSpace(_model.Password))
            {
                _errorMessage = "Please enter both email and password.";
                return;
            }

            _isLoggingIn = true;
            _errorMessage = null;

            try
            {
                var (success, error) = await AuthService.LoginAsync(_model.Email, _model.Password);

                if (success)
                {
                    Navigation.NavigateTo("/incidents", forceLoad: true);
                }
                else
                {
                    _errorMessage = error ?? "Invalid email or password.";
                }
            }
            catch (Exception ex)
            {
                _errorMessage = $"An error occurred: {ex.Message}";
            }
            finally
            {
                _isLoggingIn = false;
            }
        }
    }
}
