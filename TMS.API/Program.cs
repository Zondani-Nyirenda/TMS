using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using TMS.API.Extensions;
using TMS.API.Filters;
using TMS.API.Middleware;
using TMS.Application;
using TMS.Infrastructure;
using TMS.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ══════════════════════════════════════════════════════════════════════════════
// SERVICE REGISTRATIONS
// ══════════════════════════════════════════════════════════════════════════════

// ── Clean Architecture layers ────────────────────────────────────────────────
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// ── API Controllers ─────────────────────────────────────────────────────────
builder.Services
    .AddControllers(options =>
    {
        options.Filters.Add<ValidationFilter>(); // auto-validates all requests
    })
    .AddJsonOptions(options =>
    {
        // Serialize enums as strings
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    })
    .AddFluentValidation(fv =>
    {
        // Fixed: Use a non-static type or assembly directly
        fv.RegisterValidatorsFromAssembly(
            typeof(TMS.Application.ApplicationServiceRegistration).Assembly);

        fv.AutomaticValidationEnabled = true;
    });

// ── JWT Authentication ──────────────────────────────────────────────────────
builder.Services.AddJwtAuthentication(builder.Configuration);

// ── Swagger ─────────────────────────────────────────────────────────────────
builder.Services.AddSwaggerWithJwt();
builder.Services.AddEndpointsApiExplorer();

// ── CORS ────────────────────────────────────────────────────────────────────
builder.Services.AddTmsCors(builder.Configuration);

// ── Health checks ───────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database");

// ── HTTP Context ────────────────────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();


// ══════════════════════════════════════════════════════════════════════════════
// PIPELINE (MIDDLEWARE ORDER MATTERS)
// ══════════════════════════════════════════════════════════════════════════════
var app = builder.Build();

// ── Seed database on startup ────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

// ── Development tools ───────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TMS API v1");
        c.RoutePrefix = string.Empty; 
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
    });
}

// ── Security headers ────────────────────────────────────────────────────────
app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

// ── Global exception handler (must be early in pipeline) ────────────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();

// ── CORS ────────────────────────────────────────────────────────────────────
app.UseCors("TmsPolicy");

// ── Auth ────────────────────────────────────────────────────────────────────
app.UseAuthentication();
app.UseAuthorization();

// ── Endpoints ───────────────────────────────────────────────────────────────
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();