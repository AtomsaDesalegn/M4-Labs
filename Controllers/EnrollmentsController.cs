using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TmsApi.Services;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/enrollments")]
public class EnrollmentsController(IEnrollmentService enrollmentService) : ControllerBase
{
    // GET /api/enrollments -> Returns all enrollment records
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var enrollments = await enrollmentService.GetAllAsync();
        return Ok(enrollments);
    }

    // GET /api/enrollments/{id} -> Returns one record or 404
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var record = await enrollmentService.GetByIdAsync(id);
        return record is not null ? Ok(record) : NotFound();
    }

    // POST /api/enrollments -> Creates and returns 201 with Location header
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEnrollmentRequest request)
    {
        var record = await enrollmentService.EnrollAsync(request.StudentId, request.CourseCode);
        return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
    }

    // DELETE /api/enrollments/{id} -> Returns 204 or 404
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await enrollmentService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}

// Request DTO Model for incoming JSON payloads
public record CreateEnrollmentRequest(string StudentId, string CourseCode);