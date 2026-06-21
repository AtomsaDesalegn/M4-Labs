using TMSAPI; // Ensure this matches your middleware's namespace

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// 1. First (Outer Wrapper) - Tracks everything from the absolute start
app.UseMiddleware<RequestLoggingMiddleware>();

// 2. Exception Handler - Catches any unhandled errors down the line
app.UseExceptionHandler("/error");

// 3. HTTPS Redirection - Forces secure connections
app.UseHttpsRedirection();

// 4. Routing - Matches the URL to an endpoint
app.UseRouting();

// 5. Authentication - Identifies WHO the user is
app.UseAuthentication();

// 6. Authorization - Checks WHAT the user is allowed to do
app.UseAuthorization();

// 7. Last - The actual Endpoint protected by authorization rules
app.MapGet("/api/assessments/result", () => Results.Ok(new
{
    courseCode = "CS-101",
    studentId = "STU-001",
    letterGrade = "A"
})).RequireAuthorization(); // Secured so only logged-in users can reach it

app.Run();
