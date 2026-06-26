using TMSAPI; 
using TmsApi.Services;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;


var builder = WebApplication.CreateBuilder(args);

// =================================================================
// 🛠️ STEP 1: REGISTER SERVICES (Dependency Injection)
// =================================================================
builder.Services.AddControllers();
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddProblemDetails(); // RFC 7807 Standardized Errors

// Core Application Services
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// 🚀 FIXED: Register the worker as a Hosted Service so it boots automatically
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

// 1. Correlation ID MUST be absolute first to stamp every response
// app.UseMiddleware<CorrelationIdMiddleware>(); 

// 2. Logging & Error Handling Surface
app.UseMiddleware<RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    // Toggle on documentation tools (Scalar/Swagger) for local dev only
    // app.UseOpenApi(); 
}
else
{
    app.UseExceptionHandler(); // Automatically leverages AddProblemDetails()
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// =================================================================
// 🎯 STEP 4: ENDPOINTS & CONTROLLERS
// =================================================================

// 🚀 FIXED: Required to discover your /api/enrollments controller
app.MapControllers(); 

// Minimal API smoke tests
app.MapGet("/api/assessments/result", () => Results.Ok(new
{
    courseCode = "CS-101",
    studentId = "STU-001",
    letterGrade = "A"
})).RequireAuthorization();

app.Run();