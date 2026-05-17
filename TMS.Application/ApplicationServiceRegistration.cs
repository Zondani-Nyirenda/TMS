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
        // AutoMapper — scans this assembly for all Profile subclasses
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // FluentValidation — registers all validators in this assembly
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Application services
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<ITutorService, TutorService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IClassService, ClassService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IExamService, ExamService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<IStudyMaterialService, StudyMaterialService>();
        services.AddScoped<IContentService, ContentService>();

        return services;
    }
}