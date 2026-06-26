using TMSAPI;
using TmsApi.Services;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore; // 👈 Add this at the very top line

var builder = WebApplication.CreateBuilder(args);

// =================================================================
// 🛠️ STEP 1: REGISTER SERVICES (Dependency Injection)
// =================================================================
builder.Services.AddControllers();
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddProblemDetails(); // 🚀 TODO 1: Kept clean (removed duplicate)
builder.Services.AddOpenApi();

// Core Application Services
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// Register the worker as a Hosted Service so it boots automatically
builder.Services.AddSingleton<EnrollmentWorker>();

// Bind & Validate Configuration immediately on boot
builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Guardrails against Scope Creep and invalid DI Graphs
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

// =================================================================
// 🏗️ STEP 2: BUILD THE APPLICATION
// =================================================================
var app = builder.Build();

// =================================================================
// 🌊 STEP 3: THE MIDDLEWARE PIPELINE (Order is Critical)
// =================================================================

// 1. Logging & Error Handling Surface (Catches errors from everything below it)
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseStatusCodePages();  // 🚀 TODO 3: Active globally

// TODO 1: Check if the app is running in Development mode
app.UseStatusCodePages(); // Can stay global to handle bare 404s/401s

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); 
    app.MapScalarApiReference(); 
}
else
{
    // TODO 3: Exception handler lives here so it only protects Production!
    app.UseExceptionHandler(); 
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// =================================================================
// 🎯 STEP 4: ENDPOINTS & CONTROLLERS
// =================================================================

app.MapControllers();

// TODO 4: Map a test route '/api/error' that intentionally throws
// TODO 4: Map a test route '/api/error' that intentionally throws our custom TMS database exception
app.MapGet("/api/error", () =>
{
    throw new TmsDatabaseException("Simulated database failure for ProblemDetails testing");
});

// Minimal API smoke tests
app.MapGet("/api/assessments/result", () => Results.Ok(new
{
    courseCode = "CS-101",
    studentId = "STU-001",
    letterGrade = "A"
})).RequireAuthorization();

app.Run();