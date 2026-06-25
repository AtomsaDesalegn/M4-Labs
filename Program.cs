using TMSAPI; // Ensure this matches your middleware's namespace
using TmsApi.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// =================================================================
// 🛠️ STEP 1: REGISTER SERVICES (The Dependency Injection Container)
// =================================================================
builder.Services.AddControllers();

// 🚀 Fixes: Adds the required internal tools for Auth & Exceptions
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddProblemDetails(); 

// Register your custom middleware service if it has dependencies


// builder.Services.AddTransient<RequestLoggingMiddleware>();



// 🚨 THE CRASHING COMBINATION:
builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// Bind configuration sections and enforce strict data annotation rules on start
builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart(); // 🚀 Force the crash at application boot!

// 2. Turn on the strict validation guardrails

builder.Host.UseDefaultServiceProvider(options =>
{
   options.ValidateScopes = true;
   options.ValidateOnBuild = true; 
});


// =================================================================
// 🏗️ STEP 2: BUILD THE APPLICATION
// =================================================================
var app = builder.Build();


app.MapGet("/api/enrollments/worker-smoke", (EnrollmentWorker worker) =>
{
    worker.ProcessBatch();
    return Results.Ok("processed");
});


// =================================================================
// 🌊 STEP 3: THE MIDDLEWARE PIPELINE (Order Matters Perfectly Here!)
// =================================================================

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