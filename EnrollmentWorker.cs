using Microsoft.Extensions.DependencyInjection;

namespace TmsApi.Services;

// ✅ This is your single, modern class definition
public class EnrollmentWorker(IServiceScopeFactory scopeFactory)
{
    public void ProcessBatch()
    {
        using var scope = scopeFactory.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();
        
        // do your work here...
    }
}