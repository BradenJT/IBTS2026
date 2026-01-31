using IBTS2026.Web.Models.Auth;
using Microsoft.AspNetCore.WebUtilities;

namespace IBTS2026.Web.Components.Pages.Auth;

public partial class Register
{
    private RegisterModel _model = new();
    private bool _isRegistering;
    private string? _errorMessage;
    private string? _successMessage;
    private string? _invitationToken;
    private string? _invitedEmail;
    private bool _isFirstUser;
    private bool _hasValidToken;
    private bool _isInitialized;

    protected override async Task OnInitializedAsync()
    {
        await CheckRegistrationStatusAsync();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender && !_isInitialized)
        {
            _isInitialized = true;

            // Extract token from URL query parameters
            var uri = Navigation.ToAbsoluteUri(Navigation.Uri);
            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("token", out var token))
            {
                _invitationToken = token.FirstOrDefault();
                if (!string.IsNullOrEmpty(_invitationToken))
                {
                    _hasValidToken = true;
                    // We'd need an API call to validate the token and get the invited email
                    // For now, we'll let the server validate it on submit
                }
            }

            StateHasChanged();
        }
    }

    private async Task CheckRegistrationStatusAsync()
    {
        try
        {
            // Check if this is the first user scenario by attempting a check
            // This would ideally be an API endpoint, but we can infer from the registration attempt
            _isFirstUser = await AuthService.IsFirstUserAsync();
        }
        catch
        {
            // If check fails, assume not first user
            _isFirstUser = false;
        }

        // Parse token from URL
        var uri = Navigation.ToAbsoluteUri(Navigation.Uri);
        if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("token", out var token))
        {
            _invitationToken = token.FirstOrDefault();
            if (!string.IsNullOrEmpty(_invitationToken))
            {
                _hasValidToken = true;
                // Optionally validate token and pre-fill email
                var invitationInfo = await AuthService.ValidateInvitationTokenAsync(_invitationToken);
                if (invitationInfo != null)
                {
                    _invitedEmail = invitationInfo.Email;
                    _model.Email = invitationInfo.Email;
                }
                else
                {
                    _hasValidToken = false;
                    _errorMessage = "This invitation link is invalid or has expired.";
                }
            }
        }
    }

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
                _model.LastName,
                _invitationToken);

            if (success)
            {
                _successMessage = "Account created successfully! You can now sign in.";
                _model = new RegisterModel();

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
