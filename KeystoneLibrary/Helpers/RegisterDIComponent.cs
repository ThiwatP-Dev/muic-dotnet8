using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace KeystoneLibrary.Helpers
{
    public static class RegisterDIComponent
    {
        public static IServiceCollection RegisterDI(this IServiceCollection services)
        {
            // DI for our providers
            services.AddTransient<IAuthenticationProvider, AuthenticationProvider>();
            services.AddTransient<ISelectListProvider, SelectListProvider>();
            services.AddTransient<IStudentProvider, StudentProvider>();
            services.AddTransient<ICacheProvider, CacheProvider>();
            services.AddTransient<ICardProvider, CardProvider>();
            services.AddTransient<IRegistrationProvider, RegistrationProvider>();
            services.AddTransient<IReceiptProvider, ReceiptProvider>();
            services.AddTransient<IExceptionManager, ExceptionManager>();
            services.AddTransient<IExaminationProvider, ExaminationProvider>();
            services.AddTransient<IFeeProvider, FeeProvider>();
            services.AddTransient<IAcademicProvider, AcademicProvider>();
            services.AddTransient<IWithdrawalProvider, WithdrawalProvider>();
            services.AddTransient<ICurriculumProvider, CurriculumProvider>();
            services.AddTransient<IScheduleProvider, ScheduleProvider>();
            services.AddTransient<ISectionProvider, SectionProvider>();
            services.AddTransient<IAddressProvider, AddressProvider>();
            services.AddTransient<IAdmissionProvider, AdmissionProvider>();
            services.AddTransient<IRoomProvider, RoomProvider>();
            services.AddTransient<IReservationProvider, ReservationProvider>();
            services.AddTransient<IInstructorProvider, InstructorProvider>();
            services.AddTransient<IGradeProvider, GradeProvider>();
            services.AddTransient<IScholarshipProvider, ScholarshipProvider>();
            services.AddTransient<IReportProvider, ReportProvider>();
            services.AddTransient<IDateTimeProvider, DateTimeProvider>();
            services.AddTransient<IPrintingLogProvider, PrintingLogProvider>();
            services.AddTransient<ICalculationProvider, CalculationProvider>();
            services.AddTransient<IMasterProvider, MasterProvider>();
            services.AddTransient<IMenuProvider, MenuProvider>();
            services.AddTransient<IFileProvider, FileProvider>();
            services.AddTransient<IStudentPhotoProvider, StudentPhotoProvider>();
            services.AddTransient<IPrerequisiteProvider, PrerequisiteProvider>();
            services.AddTransient<IPetitionProvider, PetitionProvider>();
            services.AddTransient<IConfigurationProvider, ConfigurationProvider>();
            services.AddTransient<IGraduationProvider, GraduationProvider>();
            services.AddTransient<IEmailProvider, EmailProvider>();
            services.AddTransient<IQuestionnaireProvider, QuestionnaireProvider>();
            services.AddTransient<IVoucherProvider, VoucherProvider>();
            services.AddTransient<IRegistrationCalculationProvider, RegistrationCalculationProvider>();
            services.AddTransient<IEquivalenceProvider, EquivalenceProvider>();
            services.AddTransient<IUserProvider, UserProvider>();
            return services;
        }
    }
}