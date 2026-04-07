using Hangfire.Dashboard;

namespace CourseManagementApi.Hangfire;

public sealed class HangfireAllowAllAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true;
}
