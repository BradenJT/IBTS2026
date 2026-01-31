using IBTS2026.Web.Models;

namespace IBTS2026.Web.Components.Pages.Incidents
{
    public partial class CreateIncident
    {
        private CreateIncidentModel incident = new()
        {
            StatusId = 1,
            PriorityId = 2,
            CreatedByUserId = 0
        };

        private List<PriorityModel> priorities = [];
        private List<StatusModel> statuses = [];
        private List<UserLookupModel> users = [];
        private bool isLoading = true;
        private bool isSubmitting = false;
        private string? errorMessage;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                // Load lookup data first
                var prioritiesTask = LookupApi.GetPrioritiesAsync();
                var statusesTask = LookupApi.GetStatusesAsync();
                var usersTask = LookupApi.GetUsersForDropdownAsync();

                await Task.WhenAll(prioritiesTask, statusesTask, usersTask);

                priorities = await prioritiesTask;
                statuses = await statusesTask;
                users = await usersTask;

                // Auto-select current user as CreatedBy
                var currentUser = await AuthService.GetCurrentUserAsync();
                if (currentUser is not null)
                {
                    incident.CreatedByUserId = currentUser.UserId;
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Error loading form data: {ex.Message}";
            }
            finally
            {
                isLoading = false;
            }
        }

        private async Task HandleSubmit()
        {
            try
            {
                isSubmitting = true;
                errorMessage = null;

                var incidentId = await IncidentApi.CreateIncidentAsync(incident);
                Navigation.NavigateTo($"/incidents/{incidentId}");
            }
            catch (Exception ex)
            {
                errorMessage = $"Error creating incident: {ex.Message}";
            }
            finally
            {
                isSubmitting = false;
            }
        }

        private void Cancel()
        {
            Navigation.NavigateTo("/incidents");
        }
    }
}
