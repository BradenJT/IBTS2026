using IBTS2026.Web.Services.ApiClients;

namespace IBTS2026.Web.Components.Pages.Admin;

public partial class Invitations
{
    private List<InvitationModel> _invitations = [];
    private bool _isLoading = true;
    private bool _isProcessing;
    private string? _errorMessage;
    private string? _successMessage;

    // Modal state
    private bool _showInviteModal;
    private string _inviteEmail = string.Empty;
    private string _inviteRole = "User";
    private bool _isSending;
    private string? _modalError;

    protected override async Task OnInitializedAsync()
    {
        await LoadInvitationsAsync();
    }

    private async Task LoadInvitationsAsync()
    {
        _isLoading = true;
        _errorMessage = null;

        try
        {
            var invitations = await InvitationApi.GetInvitationsAsync();
            _invitations = invitations.ToList();
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to load invitations: {ex.Message}";
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void ShowInviteModal()
    {
        _showInviteModal = true;
        _inviteEmail = string.Empty;
        _inviteRole = "User";
        _modalError = null;
    }

    private void CloseInviteModal()
    {
        _showInviteModal = false;
        _inviteEmail = string.Empty;
        _inviteRole = "User";
        _modalError = null;
    }

    private async Task SendInvitation()
    {
        if (string.IsNullOrWhiteSpace(_inviteEmail))
        {
            _modalError = "Please enter an email address.";
            return;
        }

        _isSending = true;
        _modalError = null;

        try
        {
            var result = await InvitationApi.SendInvitationAsync(_inviteEmail, _inviteRole);

            if (result != null)
            {
                _successMessage = $"Invitation sent to {_inviteEmail}!";
                CloseInviteModal();
                await LoadInvitationsAsync();

                // Clear success message after 5 seconds
                _ = Task.Delay(5000).ContinueWith(_ =>
                {
                    _successMessage = null;
                    InvokeAsync(StateHasChanged);
                });
            }
            else
            {
                _modalError = "Failed to send invitation. The email may already be registered or have a pending invitation.";
            }
        }
        catch (Exception ex)
        {
            _modalError = $"Error: {ex.Message}";
        }
        finally
        {
            _isSending = false;
        }
    }

    private async Task ResendInvitation(int id)
    {
        _isProcessing = true;
        _errorMessage = null;
        _successMessage = null;

        try
        {
            var success = await InvitationApi.ResendInvitationAsync(id);

            if (success)
            {
                _successMessage = "Invitation email resent successfully!";
                await LoadInvitationsAsync();

                // Clear success message after 5 seconds
                _ = Task.Delay(5000).ContinueWith(_ =>
                {
                    _successMessage = null;
                    InvokeAsync(StateHasChanged);
                });
            }
            else
            {
                _errorMessage = "Failed to resend invitation.";
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            _isProcessing = false;
        }
    }

    private async Task CancelInvitation(int id)
    {
        _isProcessing = true;
        _errorMessage = null;
        _successMessage = null;

        try
        {
            var success = await InvitationApi.CancelInvitationAsync(id);

            if (success)
            {
                _successMessage = "Invitation cancelled.";
                await LoadInvitationsAsync();

                // Clear success message after 5 seconds
                _ = Task.Delay(5000).ContinueWith(_ =>
                {
                    _successMessage = null;
                    InvokeAsync(StateHasChanged);
                });
            }
            else
            {
                _errorMessage = "Failed to cancel invitation.";
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            _isProcessing = false;
        }
    }

    private static string GetStatusClass(InvitationModel invitation)
    {
        if (invitation.IsUsed) return "used";
        if (!invitation.IsValid) return "expired";
        return "pending";
    }

    private static string GetStatusText(InvitationModel invitation)
    {
        if (invitation.IsUsed) return "Used";
        if (!invitation.IsValid) return "Expired";
        return "Pending";
    }

    private static string GetRowClass(InvitationModel invitation)
    {
        if (invitation.IsUsed) return "used";
        if (!invitation.IsValid) return "expired";
        return "";
    }
}
