using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TmsApi.Services;

public interface IEnrollmentService
{
    Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode);
    Task<EnrollmentRecord?> GetByIdAsync(string id);
    Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
}

public class EnrollmentService : IEnrollmentService
{
    private readonly Dictionary<string, EnrollmentRecord> _store = new();
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(ILogger<EnrollmentService> logger)
    {
        _logger = logger;
    }

    public Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode)
    {
        // 1. Audit Check: Detect duplicate enrollments
        var existing = _store.Values
            .FirstOrDefault(e => e.StudentId == studentId && e.CourseCode == courseCode);

        if (existing is not null)
        {
            // Log as a recoverable Warning with structured query placeholders
            _logger.LogWarning(
                "Duplicate enrollment attempt: Student {StudentId} is already in course {CourseCode} (Record: {EnrollmentId})",
                studentId, courseCode, existing.Id);

            return Task.FromResult(existing);
        }

        // Generates an 8-character unique hex string segment
        var id = Guid.NewGuid().ToString("N")[..8];
        var record = new EnrollmentRecord(id, studentId, courseCode, DateTime.UtcNow);

        _store[id] = record;

        // Log successful business events as Information
        _logger.LogInformation(
            "Enrolled {StudentId} in {CourseCode} record {EnrollmentId}",
            studentId, courseCode, id);

        return Task.FromResult(record);
    }

    public Task<EnrollmentRecord?> GetByIdAsync(string id)
    {
        _store.TryGetValue(id, out var record);
        
        // 2. Audit Check: Warning for missing data targets
        if (record is null)
        {
            _logger.LogWarning("Enrollment {EnrollmentId} not found", id);
        }
        
        return Task.FromResult(record);
    }

    public Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync()
    {
        IReadOnlyList<EnrollmentRecord> all = _store.Values.ToList();
        return Task.FromResult(all);
    }

    public Task<bool> DeleteAsync(string id)
    {
        var removed = _store.Remove(id);
        
        // 3. Audit Check: Information on clear success vs Warning on missing
        if (removed)
        {
            _logger.LogInformation("Deleted enrollment {EnrollmentId}", id);
        }
        else
        {
            _logger.LogWarning("Delete failed: enrollment {EnrollmentId} not found", id);
        }
        
        return Task.FromResult(removed);
    }
}

public record EnrollmentRecord(
    string Id,
    string StudentId,
    string CourseCode,
    DateTime EnrolledAt
);

public class TmsDatabaseException(string message) : Exception(message);