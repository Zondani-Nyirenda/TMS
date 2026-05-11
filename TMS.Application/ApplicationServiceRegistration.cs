using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TMS.Application.Interfaces;
using TMS.Application.Services;

namespace TMS.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // AutoMapper - Clean & Recommended
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Application Services
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IExamService, ExamService>();

        return services;
    }
}