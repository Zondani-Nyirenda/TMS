using AutoMapper;
using TMS.Application.DTOs.Auth;
using TMS.Application.DTOs.Attendance;
using TMS.Application.DTOs.Course;
using TMS.Application.DTOs.Exam;
using TMS.Application.DTOs.Payment;
using TMS.Application.DTOs.Student;
using TMS.Application.DTOs.Tutor;
using TMS.Domain.Entities;

namespace TMS.Application.Mappings;

public class StudentMappingProfile : Profile
{
    public StudentMappingProfile()
    {
        CreateMap<Student, StudentDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName))
            .ForMember(d => d.Age, o => o.MapFrom(s => s.Age))
            .ForMember(d => d.Street, o => o.MapFrom(s => s.Address != null ? s.Address.Street : null))
            .ForMember(d => d.City, o => o.MapFrom(s => s.Address != null ? s.Address.City : null))
            .ForMember(d => d.Province, o => o.MapFrom(s => s.Address != null ? s.Address.Province : null))
            .ForMember(d => d.Country, o => o.MapFrom(s => s.Address != null ? s.Address.Country : null))
            .ForMember(d => d.ActiveClassCount, o => o.Ignore())
            .ForMember(d => d.OutstandingBalance, o => o.Ignore())
            .ForMember(d => d.AttendancePercentage, o => o.Ignore());

        CreateMap<Student, StudentSummaryDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName))
            .ForMember(d => d.ActiveClassCount, o => o.Ignore())
            .ForMember(d => d.OutstandingBalance, o => o.Ignore());
    }
}

public class TutorMappingProfile : Profile
{
    public TutorMappingProfile()
    {
        CreateMap<Tutor, TutorDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName))
            .ForMember(d => d.AssignedCourses, o => o.Ignore())
            .ForMember(d => d.ActiveClassCount, o => o.Ignore());

        CreateMap<Tutor, TutorSummaryDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName))
            .ForMember(d => d.ActiveClassCount, o => o.Ignore());
    }
}

public class CourseMappingProfile : Profile
{
    public CourseMappingProfile()
    {
        CreateMap<Course, CourseDto>()
            .ForMember(d => d.ActiveClassCount, o => o.Ignore())
            .ForMember(d => d.TotalEnrolledStudents, o => o.Ignore())
            .ForMember(d => d.AssignedTutors, o => o.Ignore());

        CreateMap<Class, ClassDto>()
            .ForMember(d => d.CourseName, o => o.MapFrom(s => s.Course != null ? s.Course.Name : string.Empty))
            .ForMember(d => d.CourseCode, o => o.MapFrom(s => s.Course != null ? s.Course.Code : string.Empty))
            .ForMember(d => d.TutorName, o => o.MapFrom(s => s.Tutor != null ? s.Tutor.FullName : string.Empty))
            .ForMember(d => d.EnrolledCount, o => o.MapFrom(s => s.EnrolledCount))
            .ForMember(d => d.IsFull, o => o.MapFrom(s => s.IsFull));
    }
}

public class AttendanceMappingProfile : Profile
{
    public AttendanceMappingProfile()
    {
        CreateMap<Attendance, AttendanceDto>()
            .ForMember(d => d.StudentName, o => o.MapFrom(s => s.Student != null ? s.Student.FullName : string.Empty))
            .ForMember(d => d.StudentNumber, o => o.MapFrom(s => s.Student != null ? s.Student.StudentNumber : string.Empty))
            .ForMember(d => d.ClassName, o => o.MapFrom(s => s.Class != null ? s.Class.Name : string.Empty));
    }
}

public class PaymentMappingProfile : Profile
{
    public PaymentMappingProfile()
    {
        CreateMap<FeeStructure, FeeStructureDto>()
            .ForMember(d => d.CourseName, o => o.MapFrom(s => s.Course != null ? s.Course.Name : string.Empty));

        CreateMap<InvoiceLineItem, InvoiceLineItemDto>()
            .ForMember(d => d.LineTotal, o => o.MapFrom(s => s.LineTotal));

        CreateMap<Invoice, InvoiceDto>()
            .ForMember(d => d.StudentName, o => o.MapFrom(s => s.Student != null ? s.Student.FullName : string.Empty))
            .ForMember(d => d.StudentNumber, o => o.MapFrom(s => s.Student != null ? s.Student.StudentNumber : string.Empty))
            .ForMember(d => d.BalanceAmount, o => o.MapFrom(s => s.BalanceAmount));

        CreateMap<Payment, PaymentDto>()
            .ForMember(d => d.StudentName, o => o.MapFrom(s => s.Student != null ? s.Student.FullName : string.Empty))
            .ForMember(d => d.InvoiceNumber, o => o.MapFrom(s => s.Invoice != null ? s.Invoice.InvoiceNumber : string.Empty));
    }
}

public class ExamMappingProfile : Profile
{
    public ExamMappingProfile()
    {
        CreateMap<Exam, ExamDto>()
            .ForMember(d => d.ClassName, o => o.MapFrom(s => s.Class != null ? s.Class.Name : string.Empty))
            .ForMember(d => d.CourseName, o => o.MapFrom(s => s.Class != null && s.Class.Course != null ? s.Class.Course.Name : string.Empty))
            .ForMember(d => d.ResultCount, o => o.MapFrom(s => s.Results.Count))
            .ForMember(d => d.AverageScore, o => o.Ignore())
            .ForMember(d => d.PassRate, o => o.Ignore());

        CreateMap<Result, ResultDto>()
            .ForMember(d => d.ExamTitle, o => o.MapFrom(s => s.Exam != null ? s.Exam.Title : string.Empty))
            .ForMember(d => d.StudentName, o => o.MapFrom(s => s.Student != null ? s.Student.FullName : string.Empty))
            .ForMember(d => d.StudentNumber, o => o.MapFrom(s => s.Student != null ? s.Student.StudentNumber : string.Empty))
            .ForMember(d => d.TotalMarks, o => o.MapFrom(s => s.Exam != null ? s.Exam.TotalMarks : 0))
            .ForMember(d => d.Percentage, o => o.MapFrom(s => s.Percentage));
    }
}

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<ApplicationUser, UserDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName));
    }
}
