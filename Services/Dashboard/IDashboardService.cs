using HMS.Models.ViewModels;

namespace HMS.Services.Dashboard;

public interface IDashboardService
{
    /// <summary>
    /// Builds the dashboard for a role. Only the panels that role can act on are
    /// populated, so nobody is shown numbers about a section they cannot open.
    /// </summary>
    Task<DashboardViewModel> BuildAsync(string? role, string userName);
}
