using AutoMapper;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Curriculums;
using KeystoneLibrary.Models.DataModels.Scholarship;
using KeystoneLibrary.Models.DataModels.MasterTables;
using KeystoneLibrary.Models.DataModels.Profile;
using KeystoneLibrary.Models.Report;
using KeystoneLibrary.Models.Schedules;
using KeystoneLibrary.Models.DataModels.Admission;

namespace KeystoneLibrary.Services
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Section, SectionViewModel>().ForMember(x => x.CourseCode, opt => opt.MapFrom(x => x.Course.Code))
                                                  .ForMember(x => x.CourseName, opt => opt.MapFrom(x => x.Course.NameEnAndCredit));

            // Add as many of these lines as you need to map your objects
            // You have to declare <A,B> which mean convert A -> B
            CreateMap<Student, StudentSearchViewModel>().ForMember(x => x.NationalityNameEn, opt => opt.MapFrom(x => x.Nationality.NameEn))
                                                        .ForMember(x => x.NationalityNameTh, opt => opt.MapFrom(x => x.Nationality.NameTh))
                                                        .ForMember(x => x.Credit, opt => opt.MapFrom(x => x.AcademicInformation.CreditComp))
                                                        .ForMember(x => x.GPA, opt => opt.MapFrom(x => x.AcademicInformation.GPA.ToString(StringFormat.GeneralDecimal)))
                                                        .ForMember(x => x.FacultyNameEn, opt => opt.MapFrom(x => x.AcademicInformation.Faculty.NameEn))
                                                        .ForMember(x => x.FacultyNameTh, opt => opt.MapFrom(x => x.AcademicInformation.Faculty.NameTh))
                                                        .ForMember(x => x.FacultyId, opt => opt.MapFrom(x => x.AcademicInformation.Faculty.Id))
                                                        .ForMember(x => x.DepartmentId, opt => opt.MapFrom(x => x.AcademicInformation.Department.Id))
                                                        .ForMember(x => x.DepartmentCode, opt => opt.MapFrom(x => x.AcademicInformation.Department.Code))
                                                        .ForMember(x => x.DepartmentNameEn, opt => opt.MapFrom(x => x.AcademicInformation.Department.NameEn))
                                                        .ForMember(x => x.DepartmentNameTh, opt => opt.MapFrom(x => x.AcademicInformation.Department.NameTh));
            
            CreateMap<Student, SearchAdmissionStudentViewModel>().ForMember(x => x.Nationality, opt => opt.MapFrom(x => x.Nationality == null ? "N/A" : x.Nationality.NameEn))
                                                                 .ForMember(x => x.Faculty, opt => opt.MapFrom(x => x.AdmissionInformation == null ? "N/A" : x.AdmissionInformation.Faculty.NameEn))
                                                                 .ForMember(x => x.Department, opt => opt.MapFrom(x => x.AdmissionInformation == null ? "N/A" : x.AdmissionInformation.Department.NameEn))
                                                                 .ForMember(x => x.AcademicLevel, opt => opt.MapFrom(x => x.AdmissionInformation == null ? "N/A" : x.AdmissionInformation.AdmissionTerm.AcademicLevel.NameEn))
                                                                 .ForMember(x => x.Round, opt => opt.MapFrom(x => x.AdmissionInformation == null ? 0 : x.AdmissionInformation.AdmissionRound.Round))
                                                                 .ForMember(x => x.Term, opt => opt.MapFrom(x => x.AdmissionInformation == null ? "N/A" : x.AdmissionInformation.AdmissionTerm.TermText))
                                                                 .ForMember(x => x.Batch, opt => opt.MapFrom(x => x.AcademicInformation == null ? "N/A" : x.AcademicInformation.Batch.ToString()));

            CreateMap<Instructor, InstructorViewModel>().ForMember(x => x.FacultyNameEn, opt => opt.MapFrom(x => x.InstructorWorkStatus.Faculty.NameEn))
                                                        .ForMember(x => x.FacultyNameTh, opt => opt.MapFrom(x => x.InstructorWorkStatus.Faculty.NameTh))
                                                        .ForMember(x => x.DepartmentNameEn, opt => opt.MapFrom(x => x.InstructorWorkStatus.Department.NameEn))
                                                        .ForMember(x => x.DepartmentNameTh, opt => opt.MapFrom(x => x.InstructorWorkStatus.Department.NameTh))
                                                        .ForMember(x => x.DepartmentCode, opt => opt.MapFrom(x => x.InstructorWorkStatus.Department.Code))
                                                        .ForMember(x => x.Email, opt => opt.MapFrom(x => x.Email))
                                                        .ForMember(x => x.Type, opt => opt.MapFrom(x => x.InstructorWorkStatus.InstructorType.NameEn));

            CreateMap<Instructor, InstructorInfoViewModel>().ForMember(x => x.TypeId, opt => opt.MapFrom(x => x.InstructorWorkStatus.InstructorTypeId))
                                                            .ForMember(x => x.OfficeRoom, opt => opt.MapFrom(x => x.InstructorWorkStatus.OfficeRoom))
                                                            .ForMember(x => x.AdminPosition, opt => opt.MapFrom(x => x.InstructorWorkStatus.AdminPosition))
                                                            .ForMember(x => x.AcademicPosition, opt => opt.MapFrom(x => x.InstructorWorkStatus.AcademicPosition))
                                                            .ForMember(x => x.RankId, opt => opt.MapFrom(x => x.InstructorWorkStatus.InstructorRankingId))
                                                            .ForMember(x => x.AcademicLevelId, opt => opt.MapFrom(x => x.InstructorWorkStatus.AcademicLevelId))
                                                            .ForMember(x => x.FacultyId, opt => opt.MapFrom(x => x.InstructorWorkStatus.FacultyId))
                                                            .ForMember(x => x.DepartmentId, opt => opt.MapFrom(x => x.InstructorWorkStatus.DepartmentId))
                                                            .ForMember(x => x.Remark, opt => opt.MapFrom(x => x.InstructorWorkStatus.Remark));

            CreateMap<InstructorInfoViewModel, Instructor>().ForMember(x => x.Id, opt => opt.Ignore());

            CreateMap<InstructorInfoViewModel, InstructorWorkStatus>().ForMember(x => x.InstructorTypeId, opt => opt.MapFrom(x => x.TypeId))
                                                                      .ForMember(x => x.InstructorRankingId, opt => opt.MapFrom(x => x.RankId))
                                                                      .ForMember(x => x.Id, opt => opt.Ignore());
            
            CreateMap<AcademicCalendar, AcademicCalendarViewModel>().ForMember(x => x.Title, opt => opt.MapFrom(x => $"[{ (x.AcademicLevel == null ? "All" : x.AcademicLevel.NameEn) }] { x.Event.NameEn }"))
                                                                    .ForMember(x => x.Start, opt => opt.MapFrom(x => x.StartedAt))
                                                                    .ForMember(x => x.End, opt => opt.MapFrom(x => x.EndedAt))
                                                                    .ForMember(x => x.AcademicLevel, opt => opt.MapFrom(x => x.AcademicLevel == null ? "All" : x.AcademicLevel.NameEn))
                                                                    .ForMember(x => x.Remark, opt => opt.MapFrom(x => x.Remark ?? ""));

            CreateMap<Student, ChangeCurriculumViewModel>().ForMember(x => x.Faculty, opt => opt.MapFrom(x => x.AcademicInformation.Faculty.NameEn))
                                                           .ForMember(x => x.Department, opt => opt.MapFrom(x => x.AcademicInformation.Department.NameEn))
                                                        //    .ForMember(x => x.Minor, opt => opt.MapFrom(x => x.AcademicInformation.Minor.NameEn))
                                                        //    .ForMember(x => x.Concentration, opt => opt.MapFrom(x => x.AcademicInformation.Concentration.NameEn))
                                                           .ForMember(x => x.CurriculumId, opt => opt.MapFrom(x => x.AcademicInformation.CurriculumVersionId))
                                                           .ForMember(x => x.Curriculum, opt => opt.MapFrom(x => x.AcademicInformation.CurriculumVersion.CodeAndName))
                                                           .ForMember(x => x.AdmissionType, opt => opt.MapFrom(x => x.AdmissionInformation.AdmissionType.NameEn))
                                                           .ForMember(x => x.FullName, opt => opt.MapFrom(x => x.FullNameEn))
                                                           .ForMember(x => x.GraduatedClass, opt => opt.MapFrom(x => x.GraduationInformation.Class))
                                                           .ForMember(x => x.Program, opt => opt.MapFrom(x => x.AcademicInformation.AcademicProgram.NameEn))
                                                           .ForMember(x => x.StudentId, opt => opt.MapFrom(x => x.Id));

            CreateMap<Student, SearchDeleteStudentViewModel>().ForMember(x => x.Faculty, opt => opt.MapFrom(x => x.AcademicInformation.Faculty.NameEn))
                                                              .ForMember(x => x.Department, opt => opt.MapFrom(x => x.AcademicInformation.Department.NameEn))
                                                              .ForMember(x => x.AcademicLevelId, opt => opt.MapFrom(x => x.AcademicInformation.AcademicLevelId))
                                                              .ForMember(x => x.FacultyId, opt => opt.MapFrom(x => x.AcademicInformation.FacultyId))
                                                              .ForMember(x => x.DepartmentId, opt => opt.MapFrom(x => x.AcademicInformation.DepartmentId))
                                                              .ForMember(x => x.Code, opt => opt.MapFrom(x => x.CodeInt))
                                                              .ForMember(x => x.StudentStatus, opt => opt.MapFrom(x => x.StudentStatusText))
                                                              .ForMember(x => x.StudentId, opt => opt.MapFrom(x => x.Id));

            CreateMap<StudyPlanDetailViewModel, StudyPlan>().ForMember(x => x.Term, opt => opt.MapFrom(x => x.Term));

            CreateMap<StudyPlan, StudyPlan>();

            CreateMap<CoursePlan, StudyCourse>().ForMember(x => x.NameTh, opt => opt.MapFrom(x => x.NameEn));

            CreateMap<Course, StudyCourse>().ForMember(x => x.CourseId, opt => opt.MapFrom(x => x.Id))
                                            .ForMember(x => x.NameEn, opt => opt.MapFrom(x => x.CourseAndCredit))
                                            .ForMember(x => x.Id, opt => opt.Ignore());

            CreateMap<Instructor, SearchInstructorScheduleViewModel>().ForMember(x => x.AcademicLevel, opt => opt.MapFrom(x => x.InstructorWorkStatus.AcademicLevel.NameEn))
                                                                      .ForMember(x => x.Faculty, opt => opt.MapFrom(x => x.InstructorWorkStatus.Faculty.NameEn))
                                                                      .ForMember(x => x.Department, opt => opt.MapFrom(x => x.InstructorWorkStatus.Department.NameEn))
                                                                      .ForMember(x => x.NameEn, opt => opt.MapFrom(x => x.FullNameEn))
                                                                      .ForMember(x => x.NameTh, opt => opt.MapFrom(x => x.FullNameTh));

            CreateMap<Instructor, InstructorScheduleViewModel>().ForMember(x => x.Code, opt => opt.MapFrom(x => x.Code))
                                                                .ForMember(x => x.NameEn, opt => opt.MapFrom(x => x.FullNameEn))
                                                                .ForMember(x => x.NameTh, opt => opt.MapFrom(x => x.FullNameTh));

            CreateMap<Student, StudentScheduleViewModel>().ForMember(x => x.Code, opt => opt.MapFrom(x => x.Code))
                                                          .ForMember(x => x.Name, opt => opt.MapFrom(x => x.FullNameEn))
                                                          .ForMember(x => x.AcademicLevel, opt => opt.MapFrom(x => x.AcademicInformation.AcademicLevel.NameEn))
                                                          .ForMember(x => x.Faculty, opt => opt.MapFrom(x => x.AcademicInformation.Faculty.NameEn))
                                                          .ForMember(x => x.Department, opt => opt.MapFrom(x => x.AcademicInformation.Department.NameEn))
                                                          .ForMember(x => x.Advisor, opt => opt.MapFrom(x => x.AcademicInformation.Advisor.FullNameEn))
                                                          .ForMember(x => x.CurriculumVersion, opt => opt.MapFrom(x => x.AcademicInformation.CurriculumVersion.NameEn))
                                                          .ForMember(x => x.RegistrationCourses, opt => opt.MapFrom(x => x.RegistrationCourses));

            CreateMap<InstructorSection, ClassScheduleTimeViewModel>().ForMember(x => x.Day, opt => opt.MapFrom(x => x.SectionDetail.Day))
                                                                      .ForMember(x => x.DayOfWeek, opt => opt.MapFrom(x => x.SectionDetail.Dayofweek))
                                                                      .ForMember(x => x.StartTime, opt => opt.MapFrom(x => x.SectionDetail.StartTime))
                                                                      .ForMember(x => x.EndTime, opt => opt.MapFrom(x => x.SectionDetail.EndTime))
                                                                      .ForMember(x => x.Room, opt => opt.MapFrom(x => x.SectionDetail.Room.NameEn))
                                                                      .ForMember(x => x.Instructors, opt => opt.MapFrom(x => x.SectionDetail.InstructorIds))
                                                                      .ForMember(x => x.InstructorNameEn, opt => opt.MapFrom(x => x.Instructor.FullNameEn))
                                                                      .ForMember(x => x.InstructorNameTh, opt => opt.MapFrom(x => x.Instructor.FullNameTh))
                                                                      .ForMember(x => x.Period, opt => opt.MapFrom(x => x.Period))
                                                                      .ForMember(x => x.Hours, opt => opt.MapFrom(x => x.HoursText));

            CreateMap<Invoice, InvoiceViewModel>().ForMember(x => x.InvoiceId, opt => opt.MapFrom(x => x.Id))
                                                  .ForMember(x => x.TermId, opt => opt.MapFrom(x => x.TermId))
                                                  .ForMember(x => x.AcademicLevelId, opt => opt.MapFrom(x => x.Term.AcademicLevelId))
                                                  .ForMember(x => x.Type, opt => opt.MapFrom(x => x.TypeText))
                                                  .ForMember(x => x.Term, opt => opt.MapFrom(x => x.Term.TermText))
                                                  .ForMember(x => x.AcademicYear, opt => opt.MapFrom(x => x.Term.AcademicYear + 1))
                                                  .ForMember(x => x.StudentCode, opt => opt.MapFrom(x => x.Student.Code))
                                                  .ForMember(x => x.FullName, opt => opt.MapFrom(x => x.Student.FullNameEn))
                                                  .ForMember(x => x.InvoiceNumber, opt => opt.MapFrom(x => x.Number))
                                                  .ForMember(x => x.Program, opt => opt.MapFrom(x => x.Student.AcademicInformation.Department.NameEn))
                                                  .ForMember(x => x.TotalAmount, opt => opt.MapFrom(x => x.TotalAmountText))
                                                  .ForMember(x => x.InvoiceItems, opt => opt.Ignore());

            CreateMap<Receipt, ReceiptPreviewViewModel>().ForMember(x => x.ReceiptId, opt => opt.MapFrom(x => x.Id))
                                                         .ForMember(x => x.TermId, opt => opt.MapFrom(x => x.TermId))
                                                         .ForMember(x => x.AcademicLevelId, opt => opt.MapFrom(x => x.Term.AcademicLevelId))
                                                         .ForMember(x => x.Term, opt => opt.MapFrom(x => x.Term.TermText))
                                                         .ForMember(x => x.StudentCode, opt => opt.MapFrom(x => x.Student.Code))
                                                         .ForMember(x => x.FullName, opt => opt.MapFrom(x => 
                                                            x.Student.Nationality != null ? 
                                                            (
                                                                "Thai" == x.Student.Nationality.NameEn && !string.IsNullOrEmpty(x.Student.FirstNameTh) ? x.Student.FullNameTh : x.Student.FullNameEn
                                                            ) 
                                                            : x.Student.FullNameEn
                                                            ))
                                                         .ForMember(x => x.Program, opt => opt.MapFrom(x => x.Student.AcademicInformation.Department.NameEn))
                                                         .ForMember(x => x.InvoiceNumber, opt => opt.MapFrom(x => x.Invoice.Number))
                                                         .ForMember(x => x.ReceiptNumber, opt => opt.MapFrom(x => x.Number))
                                                         .ForMember(x => x.TotalAmount, opt => opt.MapFrom(x => x.TotalAmountText))
                                                         .ForMember(x => x.IsPrinted, opt => opt.MapFrom(x => x.IsPrint))
                                                         .ForMember(x => x.PaidAt, opt => opt.MapFrom(x => x.CreatedAtText))
                                                         .ForMember(x => x.PrintedBy, opt => opt.MapFrom(x => x.PrintedByFullName));
            
            CreateMap<Room, RoomScheduleViewModel>().ForMember(x => x.Name, opt => opt.MapFrom(x => x.NameEn))
                                                    .ForMember(x => x.CampusNameEn, opt => opt.MapFrom(x => x.Building.Campus.NameEn))
                                                    .ForMember(x => x.CampusNameTh, opt => opt.MapFrom(x => x.Building.Campus.NameTh))
                                                    .ForMember(x => x.BuildingNameEn, opt => opt.MapFrom(x => x.Building.NameEn))
                                                    .ForMember(x => x.BuildingNameTh, opt => opt.MapFrom(x => x.Building.NameTh));

            CreateMap<SectionDetail, ClassScheduleTimeViewModel>().ForMember(x => x.Day, opt => opt.MapFrom(x => x.Day))
                                                                  .ForMember(x => x.DayOfWeek, opt => opt.MapFrom(x => x.Dayofweek))
                                                                  .ForMember(x => x.StartTime, opt => opt.MapFrom(x => x.StartTime))
                                                                  .ForMember(x => x.EndTime, opt => opt.MapFrom(x => x.EndTime))
                                                                  .ForMember(x => x.StartTime, opt => opt.MapFrom(x => x.StartTime))
                                                                  .ForMember(x => x.CourseCode, opt => opt.MapFrom(x => x.Section.Course.Code))
                                                                  .ForMember(x => x.SectionNumber, opt => opt.MapFrom(x => x.Section.Number))
                                                                  .ForMember(x => x.SectionId, opt => opt.MapFrom(x => x.SectionId))
                                                                  .ForMember(x => x.Room, opt => opt.MapFrom(x => x.Room == null ? "" : x.Room.NameEn))
                                                                  .ForMember(x => x.InstructorNameEn, opt => opt.MapFrom(x => x.InstructorSections == null
                                                                                                                              ? "" : string.Join(", ", x.InstructorSections.Select(y => y.Instructor.FullNameEn))))
                                                                  .ForMember(x => x.InstructorNameTh, opt => opt.MapFrom(x => x.InstructorSections == null
                                                                                                                              ? "" : string.Join(", ", x.InstructorSections.Select(y => y.Instructor.FullNameTh))));

            CreateMap<Section, CoursesSeatAvailable>().ForMember(x => x.CourseCode, opt => opt.MapFrom(x => x.Course.CourseAndCredit))
                                                      .ForMember(x => x.SectionNumber, opt => opt.MapFrom(x => x.Number))
                                                      .ForMember(x => x.SectionId, opt => opt.MapFrom(x => x.Id))
                                                      .ForMember(x => x.SeatPayment, opt => opt.MapFrom(x => x.RegistrationCourses.Any() ?
                                                                                                             x.RegistrationCourses.Count(y => y.IsPaid) : 0))
                                                      .ForMember(x => x.SeatWithdraw, opt => opt.MapFrom(x => x.RegistrationCourses.Count(y => y.Withdrawals != null)))
                                                      .ForMember(x => x.SectionDetails, opt => opt.MapFrom(x => x.SectionDetails))
                                                      .ForMember(x => x.InstructorIds, opt => opt.MapFrom(x => x.SectionDetails == null ? new List<long>() : x.SectionDetails.SelectMany(y => y.InstructorIdList)
                                                                                                                                                                             .ToList()));

            CreateMap<RegistrationCondition, RegistrationCondition>().ForMember(x => x.IsGraduating, opt => opt.MapFrom(x => string.IsNullOrEmpty(x.IsGraduatingText)
                                                                                                                             ? null : (bool?)Convert.ToBoolean(x.IsGraduatingText)))
                                                                     .ForMember(x => x.IsAthlete, opt => opt.MapFrom(x => string.IsNullOrEmpty(x.IsAthleteText)
                                                                                                                          ? null : (bool?)Convert.ToBoolean(x.IsAthleteText)));

            CreateMap<Incident, StudentIncident>().ForMember(x => x.IncidentId, opt => opt.MapFrom(x => x.Id))
                                                  .ForMember(x => x.Id, opt => opt.Ignore())
                                                  .ForMember(x => x.StudentId, opt => opt.Ignore())
                                                  .ForMember(x => x.UpdatedAt, opt => opt.Ignore())
                                                  .ForMember(x => x.UpdatedBy, opt => opt.Ignore())
                                                  .ForMember(x => x.CreatedAt, opt => opt.Ignore())
                                                  .ForMember(x => x.CreatedBy, opt => opt.Ignore());

            CreateMap<StudentIncident, StudentIncident>().ForMember(x => x.CreatedAt, opt => opt.Ignore())
                                                         .ForMember(x => x.CreatedBy, opt => opt.Ignore());

            CreateMap<Course, CoursePlan>();

            CreateMap<Section, TransferViewModel>().ForMember(x => x.CourseId, opt => opt.MapFrom(x => x.CourseId))
                                                   .ForMember(x => x.CourseCode, opt => opt.MapFrom(x => x.Course.Code))
                                                   .ForMember(x => x.CourseNameEn, opt => opt.MapFrom(x => x.Course.NameEnAndCredit))
                                                   .ForMember(x => x.CourseNameTh, opt => opt.MapFrom(x => x.Course.NameThAndCredit))
                                                   .ForMember(x => x.PreviousSectionId, opt => opt.MapFrom(x => x.Id))
                                                   .ForMember(x => x.SectionNumber, opt => opt.MapFrom(x => x.Number))
                                                   .ForMember(x => x.AcademicLevelId, opt => opt.MapFrom(x => x.Term.AcademicLevelId))
                                                   .ForMember(x => x.TermId, opt => opt.MapFrom(x => x.TermId))
                                                   .ForMember(x => x.Students, opt => opt.MapFrom(x => x.RegistrationCourses));

            CreateMap<CurriculumVersion, AcademicInformation>().ForMember(x => x.CurriculumVersionId, opt => opt.MapFrom(x => x.Id))
                                                               .ForMember(x => x.FacultyId, opt => opt.MapFrom(x => x.Curriculum.FacultyId))
                                                               .ForMember(x => x.DepartmentId, opt => opt.MapFrom(x => x.Curriculum.DepartmentId));
                                                            //    .ForMember(x => x.SpecializationGroupId, opt => opt.Ignore());

            CreateMap<Student, RegistrationViewModel>().ForMember(x => x.Code, opt => opt.MapFrom(x => x.Code))
                                                       .ForMember(x => x.Title, opt => opt.MapFrom(x => x.Title.NameEn))
                                                       .ForMember(x => x.StudentId, opt => opt.MapFrom(x => x.Id))
                                                       .ForMember(x => x.FirstName, opt => opt.MapFrom(x => x.FirstNameEn))
                                                       .ForMember(x => x.LastName, opt => opt.MapFrom(x => x.LastNameEn))
                                                       .ForMember(x => x.Program, opt => opt.MapFrom(x => x.AcademicInformation.AcademicProgram.NameEn))
                                                       .ForMember(x => x.AdmissionType, opt => opt.MapFrom(x => x.AdmissionInformation.AdmissionType.NameEn))
                                                       .ForMember(x => x.Faculty, opt => opt.MapFrom(x => x.AcademicInformation.Faculty.NameEn))
                                                       .ForMember(x => x.Department, opt => opt.MapFrom(x => x.AcademicInformation.Department.CodeAndName))
                                                       .ForMember(x => x.CurriculumVersion, opt => opt.MapFrom(x => x.AcademicInformation.CurriculumVersion.CodeAndName))
                                                       .ForMember(x => x.StudentFeeType, opt => opt.MapFrom(x => x.StudentFeeType.NameEn))
                                                       .ForMember(x => x.StudentFeeGroup, opt => opt.MapFrom(x => x.StudentFeeGroup.Name))
                                                       .ForMember(x => x.Nationality, opt => opt.MapFrom(x => x.Nationality.NameEn))
                                                       .ForMember(x => x.GPA, opt => opt.MapFrom(x => x.AcademicInformation.GPA))
                                                       .ForMember(x => x.MaximumCredit, opt => opt.MapFrom(x => x.AcademicInformation.MaximumCredit))
                                                       .ForMember(x => x.MinimumCredit, opt => opt.MapFrom(x => x.AcademicInformation.MinimumCredit))
                                                       .ForMember(x => x.Advisor, opt => opt.MapFrom(x => x.AcademicInformation.Advisor.FullNameEn))
                                                       .ForMember(x => x.IsGraduating, opt => opt.MapFrom(x => x.GraduatingRequest != null))
                                                       .ForMember(x => x.IsMaintainedStatus, opt => opt.MapFrom(x => x.MaintenanceStatuses.Any(y => y.TermId == x.RegistrationTermId
                                                                                                                                                    && y.IsActive)));

            CreateMap<RegistrationCourse, AddingViewModel>().ForMember(x => x.CourseCodeAndName, opt => opt.MapFrom(x => x.Course.CourseAndCredit))
                                                            .ForMember(x => x.Credit, opt => opt.MapFrom(x => x.Course.Credit))
                                                            .ForMember(x => x.CreditText, opt => opt.MapFrom(x => x.Course.CreditText))
                                                            .ForMember(x => x.RegistrationCourseId, opt => opt.MapFrom(x => x.Id))
                                                            .ForMember(x => x.RegistrationCredit, opt => opt.MapFrom(x => x.Course.RegistrationCredit))
                                                            .ForMember(x => x.PaymentCredit, opt => opt.MapFrom(x => x.Course.PaymentCredit))
                                                            .ForMember(x => x.SectionNumber, opt => opt.MapFrom(x => x.Section.Number))
                                                            .ForMember(x => x.MainInstructor, opt => opt.MapFrom(x => x.Section.MainInstructorId == null ? string.Empty : x.Section.MainInstructor.FullNameEn));

            CreateMap<ExaminationPeriod, Section>();

            CreateMap<Term, Section>().ForMember(x => x.TermId, opt => opt.MapFrom(x => x.Id))
                                      .ForMember(x => x.OpenedAt, opt => opt.MapFrom(x => x.StartedAt))
                                      .ForMember(x => x.ClosedAt, opt => opt.MapFrom(x => x.EndedAt))
                                      .ForMember(x => x.Id, opt => opt.Ignore());

            CreateMap<Course, Section>().ForMember(x => x.Course, opt => opt.MapFrom(x => x))
                                        .ForMember(x => x.CourseId, opt => opt.MapFrom(x => x.Id))
                                        .ForMember(x => x.Id, opt => opt.Ignore());

            CreateMap<Receipt, CanceledReceiptReportViewModel>().ForMember(x => x.Code, opt => opt.MapFrom(x => x.Student.Code))
                                                                .ForMember(x => x.FullName, opt => opt.MapFrom(x => x.Student.FullNameEn))
                                                                .ForMember(x => x.ReceiptNumber, opt => opt.MapFrom(x => x.Number))
                                                                .ForMember(x => x.CanceledAt, opt => opt.MapFrom(x => x.UpdatedAt))
                                                                .ForMember(x => x.CanceledBy, opt => opt.MapFrom(x => x.UpdatedBy));

            CreateMap<CheatingStatus, CheatingStatusDetail>().ForMember(x => x.Term, opt => opt.MapFrom(x => x.Term.TermText))
                                                             .ForMember(x => x.CourseCode, opt => opt.MapFrom(x => x.RegistrationCourse.Course.Code))
                                                             .ForMember(x => x.CourseName, opt => opt.MapFrom(x => x.RegistrationCourse.Course.NameEn))
                                                             .ForMember(x => x.SectionNumber, opt => opt.MapFrom(x => x.RegistrationCourse.SectionNumber))
                                                             .ForMember(x => x.ExaminationType, opt => opt.MapFrom(x => x.ExaminationType.NameEn))
                                                             .ForMember(x => x.Incident, opt => opt.MapFrom(x => x.Incident.NameEn))
                                                             .ForMember(x => x.FromTerm, opt => opt.MapFrom(x => x.FromTerm.TermText))
                                                             .ForMember(x => x.ToTerm, opt => opt.MapFrom(x => x.ToTerm.TermText));

            CreateMap<LatePaymentTransaction, LatePaymentViewModel>().ForMember(x => x.StudentCodeAndName, opt => opt.MapFrom(x => x.Student.CodeAndName))
                                                                     .ForMember(x => x.StudentId, opt => opt.MapFrom(x => x.Student.Id))
                                                                     .ForMember(x => x.StudentFullName, opt => opt.MapFrom(x => x.Student.FullNameEn))
                                                                     .ForMember(x => x.TermId, opt => opt.MapFrom(x => x.TermId))
                                                                     .ForMember(x => x.FinishedRegistration, opt => opt.MapFrom(x => x.Student.RegistrationCourses == null ? "No" : "Yes"));

            CreateMap<Section, AttendanceSheetReportViewModel>().ForMember(x => x.SubjectCodeAndName, opt => opt.MapFrom(x => x.Course.CourseAndCredit))
                                                                .ForMember(x => x.SectionNumber, opt => opt.MapFrom(x => x.Number))
                                                                .ForMember(x => x.AcademicTerm, opt => opt.MapFrom(x => x.Term.AcademicTerm))
                                                                .ForMember(x => x.AcademicYear, opt => opt.MapFrom(x => x.Term.AcademicYear))
                                                                .ForMember(x => x.AcademicNextYear, opt => opt.MapFrom(x => x.Term.AcademicYear + 1))
                                                                .ForMember(x => x.MainInstructor, opt => opt.MapFrom(x => x.MainInstructor))
                                                                .ForMember(x => x.Credit, opt => opt.MapFrom(x => x.Course.CreditText))
                                                                .ForMember(x => x.SectionId, opt => opt.MapFrom(x => x.Id));

            CreateMap<PreviousSchool, StudentByPreviousSchoolViewModel>().ForMember(x => x.ProvinceOrState, opt => opt.MapFrom(x => x.Province == null ? x.State.NameEn : x.Province.NameEn))
                                                                         .ForMember(x => x.Country, opt => opt.MapFrom(x => x.Country.NameEn))
                                                                         .ForMember(x => x.SchoolTerritory, opt => opt.MapFrom(x => x.SchoolTerritory.NameEn))
                                                                         .ForMember(x => x.SchoolType, opt => opt.MapFrom(x => x.SchoolType.NameEn))
                                                                         .ForMember(x => x.TotalStudent, opt => opt.MapFrom(x => x.AdmissionInformations.Count()));

            CreateMap<Section, ExaminationSchedule>().ForPath(x => x.Midterm.Start, opt => opt.MapFrom(x => x.MidtermStart))
                                                     .ForPath(x => x.Midterm.End, opt => opt.MapFrom(x => x.MidtermEnd))
                                                     .ForPath(x => x.Midterm.Date, opt => opt.MapFrom(x => x.MidtermDate))
                                                     .ForPath(x => x.Midterm.Room, opt => opt.MapFrom(x => x.MidtermRoom))
                                                     .ForPath(x => x.Final.Start, opt => opt.MapFrom(x => x.FinalStart))
                                                     .ForPath(x => x.Final.End, opt => opt.MapFrom(x => x.FinalEnd))
                                                     .ForPath(x => x.Final.Date, opt => opt.MapFrom(x => x.FinalDate))
                                                     .ForPath(x => x.Final.Room, opt => opt.MapFrom(x => x.FinalRoom));

            CreateMap<Section, GradeSection>().ForMember(x => x.CourseCode, opt => opt.MapFrom(x => x.Course.Code))
                                              .ForMember(x => x.CourseName, opt => opt.MapFrom(x => x.Course.NameEnAndCredit))
                                              .ForMember(x => x.SectionNumber, opt => opt.MapFrom(x => x.Number + (x.ChildrenSections != null && x.ChildrenSections.Any() ? String.Format(" ({0})", String.Join(",", x.ChildrenSections.Select(y => y.Number))) : "")))
                                              .ForMember(x => x.SectionId, opt => opt.MapFrom(x => x.Id))
                                              .ForMember(x => x.ChildrenSectionIds, opt => opt.MapFrom(x => x.ChildrenSections != null && x.ChildrenSections.Any() ? String.Join(",", x.ChildrenSections.Select(y => y.Id)) : ""))
                                              .ForMember(x => x.TotalStudent, opt => opt.MapFrom(x => x.RegistrationCourses == null
                                                                                                 ? 0 : (x.RegistrationCourses.Where(y => y.Status == "a" || y.Status == "r").Count() + x.ChildrenSections.Sum(y => y.RegistrationCourses.Where(z => z.Status == "a" || z.Status == "r").Count()))))
                                              .ForMember(x => x.TotalWithdrawal, opt => opt.MapFrom(x => x.RegistrationCourses == null
                                                                                                         ? 0 : (x.RegistrationCourses.Count(y => y.Withdrawals.Any()) + x.ChildrenSections.Sum(y => y.RegistrationCourses.Count(z => z.Withdrawals.Any())))));

            CreateMap<RegistrationCourse, StudentBySectionViewModel>().ForMember(x => x.CourseCode, opt => opt.MapFrom(x => x.Course.Code))
                                                                      .ForMember(x => x.SectionNumber, opt => opt.MapFrom(x => x.Section.Number))
                                                                      .ForMember(x => x.StudentCode, opt => opt.MapFrom(x => x.Student.Code))
                                                                      .ForMember(x => x.StudentName, opt => opt.MapFrom(x => x.Student.FullNameEn))
                                                                      .ForMember(x => x.RegistrationCourseId, opt => opt.MapFrom(x => x.Id))
                                                                      .ForMember(x => x.IsWithdrawal, opt => opt.MapFrom(x => x.Withdrawals.Any()))
                                                                      .ForMember(x => x.IsGradePublished, opt => opt.MapFrom(x => x.IsGradePublished));

            // CreateMap<StudentScore, StudentBySectionViewModel>().ForMember(x => x.StudentScoreId, opt => opt.MapFrom(x => x.Id))
            //                                                     .ForMember(x => x.CourseId, opt => opt.MapFrom(x => x.RegistrationCourse.CourseId))
            //                                                     .ForMember(x => x.CourseCode, opt => opt.MapFrom(x => x.RegistrationCourse.Course.Code))
            //                                                     .ForMember(x => x.SectionId, opt => opt.MapFrom(x => x.RegistrationCourse.SectionId))
            //                                                     .ForMember(x => x.SectionNumber, opt => opt.MapFrom(x => x.RegistrationCourse.Section.Number))
            //                                                     .ForMember(x => x.StudentCode, opt => opt.MapFrom(x => x.Student.Code))
            //                                                     .ForMember(x => x.StudentName, opt => opt.MapFrom(x => x.Student.FullNameEn))
            //                                                     .ForMember(x => x.RegistrationCourseId, opt => opt.MapFrom(x => x.RegistrationCourseId))
            //                                                     .ForMember(x => x.Scores, opt => opt.MapFrom(x => string.IsNullOrEmpty(x.Score)
            //                                                                                                       ? new List<Allocation>()
            //                                                                                                       : JsonConvert.DeserializeObject<List<Allocation>>(x.Score)))
            //                                                     .ForMember(x => x.IsWithdrawal, opt => opt.MapFrom(x => x.IsWithdrawal))
            //                                                     .ForMember(x => x.IsCheating, opt => opt.MapFrom(x => x.IsCheating))
            //                                                     .ForMember(x => x.IsPaid, opt => opt.MapFrom(x => x.RegistrationCourse.IsPaid))
            //                                                     .ForMember(x => x.IsGradePublished, opt => opt.MapFrom(x => x.RegistrationCourse.IsGradePublished))
            //                                                     .ForMember(x => x.CurriculumVersionId, opt => opt.MapFrom(y => y.CurriculumnVersionId));

            // CreateMap<StudentScore, StudentGradeRecord>().ForMember(x => x.StudentCode, opt => opt.MapFrom(x => x.Student.Code))
            //                                              .ForMember(x => x.StudentScoreId, opt => opt.MapFrom(x => x.Id))
            //                                              .ForMember(x => x.CourseCode, opt => opt.MapFrom(x => x.RegistrationCourse.Course.Code))
            //                                              .ForMember(x => x.CourseName, opt => opt.MapFrom(x => x.RegistrationCourse.Course.NameEn))
            //                                              .ForMember(x => x.SectionNumber, opt => opt.MapFrom(x => x.RegistrationCourse.Section.Number))
            //                                              .ForMember(x => x.StudentName, opt => opt.MapFrom(x => x.Student.FullNameEn))
            //                                              .ForMember(x => x.Grade, opt => opt.MapFrom(x => x.Grade.Name))
            //                                              .ForMember(x => x.RoundedScore, opt => opt.MapFrom(x => decimal.ToInt16(x.TotalScore)))
            //                                              .ForMember(x => x.IsPaid, opt => opt.MapFrom(x => x.RegistrationCourse.IsPaid))
            //                                              .ForMember(x => x.Scores, opt => opt.MapFrom(x => JsonConvert.DeserializeObject<List<Allocation>>(x.Score)));
            
            CreateMap<Student, ScholarshipProfileViewModel>().ForMember(x => x.StudentId, opt => opt.MapFrom(x => x.Id))
                                                             .ForMember(x => x.StudentCode, opt => opt.MapFrom(x => x.Code))
                                                             .ForMember(x => x.FirstName, opt => opt.MapFrom(x => x.FirstNameEn))
                                                             .ForMember(x => x.LastName, opt => opt.MapFrom(x => x.LastNameEn))
                                                             .ForMember(x => x.ProfileImageURL, opt => opt.MapFrom(x => x.ProfileImageURL))
                                                             .ForMember(x => x.GPA, opt => opt.MapFrom(x => x.AcademicInformation.GPA))
                                                             .ForMember(x => x.CreditEarned, opt => opt.MapFrom(x => x.AcademicInformation.CreditComp))
                                                             .ForMember(x => x.RegistrationCredit, opt => opt.MapFrom(x => x.AcademicInformation.CreditEarned))
                                                             .ForMember(x => x.Program, opt => opt.MapFrom(x => x.AcademicInformation.AcademicProgram == null
                                                                                                                ? "N/A" : x.AcademicInformation.AcademicProgram.NameEn))
                                                             .ForMember(x => x.Faculty, opt => opt.MapFrom(x => x.AcademicInformation.Faculty == null
                                                                                                                ? "N/A" : x.AcademicInformation.Faculty.NameEn))
                                                             .ForMember(x => x.Department, opt => opt.MapFrom(x => x.AcademicInformation.Department == null
                                                                                                                   ? "N/A" : x.AcademicInformation.Department.NameEn))
                                                             .ForMember(x => x.Curriculum, opt => opt.MapFrom(x => x.AcademicInformation.CurriculumVersion.Curriculum == null
                                                                                                                   ? "N/A" : x.AcademicInformation.CurriculumVersion.Curriculum.NameEn))
                                                             .ForMember(x => x.CurriculumVersion, opt => opt.MapFrom(x => x.AcademicInformation.CurriculumVersion == null
                                                                                                                   ? "N/A" : x.AcademicInformation.CurriculumVersion.NameEn))
                                                            //  .ForMember(x => x.Minor, opt => opt.MapFrom(x => x.AcademicInformation.Minor == null
                                                            //                                                   ? "N/A" : x.AcademicInformation.Minor.NameEn))
                                                            //  .ForMember(x => x.Concentration, opt => opt.MapFrom(x => x.AcademicInformation.Concentration == null
                                                            //                                                           ? "N/A" : x.AcademicInformation.Concentration.NameEn))
                                                             .ForMember(x => x.IsFinishedRegistration, opt => opt.MapFrom(x => x.RegistrationCourses.Any()));
                                                
            CreateMap<Student, ScholarshipStudentViewModel>().ForMember(x => x.StudentId, opt => opt.MapFrom(x => x.Id))
                                                             .ForMember(x => x.StartStudentBatch, opt => opt.MapFrom(x => x.AcademicInformation.Batch))
                                                             .ForMember(x => x.EndStudentBatch, opt => opt.MapFrom(x => x.AcademicInformation.Batch))
                                                             .ForMember(x => x.StartTermId, opt => opt.MapFrom(x => x.AdmissionInformation.AdmissionTermId))
                                                             .ForMember(x => x.EndTermId, opt => opt.MapFrom(x => x.AdmissionInformation.AdmissionTermId))
                                                             .ForMember(x => x.ScolarshipIds, opt => opt.MapFrom(x => x.ScholarshipStudents.Select(y => y.ScholarshipId)
                                                                                                                                           .ToList()));

            CreateMap<ScholarshipStudent, ScholarshipFinancialTransactionDetail>().ForMember(x => x.Id, opt => opt.MapFrom(x => x.Id))
                                                                                  .ForMember(x => x.StudentCode, opt => opt.MapFrom(x => x.Student.Code))
                                                                                  .ForMember(x => x.StudentName, opt => opt.MapFrom(x => x.Student.FullNameEn))
                                                                                  .ForMember(x => x.Division, opt => opt.MapFrom(x => x.Student.AcademicInformation.Faculty.NameEn))
                                                                                  .ForMember(x => x.Major, opt => opt.MapFrom(x => x.Student.AcademicInformation.Department.NameEn))
                                                                                  .ForMember(x => x.Scholarship, opt => opt.MapFrom(x => x.Scholarship.NameEn))
                                                                                  .ForMember(x => x.Condition, opt => opt.MapFrom(x => x.Scholarship.Remark))
                                                                                  .ForMember(x => x.FinancialTransactions, opt => opt.Ignore());

            CreateMap<Student, StudentProbationDetail>().ForMember(x => x.StudentId, opt => opt.MapFrom(x => x.Id))
                                                        .ForMember(x => x.StudentCode, opt => opt.MapFrom(x => x.Code))
                                                        .ForMember(x => x.StudentTitle, opt => opt.MapFrom(x => x.Title.NameEn))
                                                        .ForMember(x => x.StudentFirstName, opt => opt.MapFrom(x => x.FirstNameEn))
                                                        .ForMember(x => x.StudentMidName, opt => opt.MapFrom(x => x.MidNameEn))
                                                        .ForMember(x => x.StudentLastName, opt => opt.MapFrom(x => x.LastNameEn))
                                                        .ForMember(x => x.StudentGPA, opt => opt.MapFrom(x => x.AcademicInformation.GPA))
                                                        .ForMember(x => x.StudentEmail, opt => opt.MapFrom(x => x.Email))
                                                        .ForMember(x => x.ProbationTime, opt => opt.MapFrom(x => x.StudentProbations.Count()))
                                                        .ForMember(x => x.AdvisorName, opt => opt.MapFrom(x => x.AcademicInformation.Advisor.FullNameEn))
                                                        .ForMember(x => x.AdvisorEmail, opt => opt.MapFrom(x => x.AcademicInformation.Advisor.Email));

            CreateMap<DismissStudent, DismissStudentViewModel>().ForMember(x => x.Code, opt => opt.MapFrom(x => x.Student.Code))
                                                                .ForMember(x => x.FullName, opt => opt.MapFrom(x => x.Student.FullNameEn))
                                                                .ForMember(x => x.Faculty, opt => opt.MapFrom(x => x.Student.AcademicInformation.Faculty.NameEn))
                                                                .ForMember(x => x.Department, opt => opt.MapFrom(x => x.Student.AcademicInformation.Department.NameEn))
                                                                .ForMember(x => x.Probation, opt => opt.MapFrom(x => x.Probation.ProbationGPA))
                                                                .ForMember(x => x.AcademicLevelId, opt => opt.MapFrom(x => x.Term.AcademicLevelId))
                                                                .ForMember(x => x.GPA, opt => opt.MapFrom(x => x.Student.AcademicInformation.GPA))
                                                                .ForMember(x => x.CreditEarned, opt => opt.MapFrom(x => x.Student.AcademicInformation.CreditEarned))
                                                                .ForMember(x => x.Advisor, opt => opt.MapFrom(x => x.Student.AcademicInformation.Advisor.FullNameEn))
                                                                .ForMember(x => x.AcademicLevel, opt => opt.MapFrom(x => x.Term.AcademicLevel.NameEn))
                                                                .ForMember(x => x.Curriculum, opt => opt.MapFrom(x => x.Student.AcademicInformation.CurriculumVersion.Curriculum.NameEn))
                                                                .ForMember(x => x.CurriculumVersion, opt => opt.MapFrom(x => x.Student.AcademicInformation.CurriculumVersion.NameEn))
                                                                .ForMember(x => x.Term, opt => opt.MapFrom(x => x.Term.TermText));
            
            CreateMap<Student, ExtendedStudentDetail>().ForMember(x => x.StudentId, opt => opt.MapFrom(x => x.Id))
                                                       .ForMember(x => x.StudentCode, opt => opt.MapFrom(x => x.Code))
                                                       .ForMember(x => x.StudentName, opt => opt.MapFrom(x => x.FullNameEn))
                                                       .ForMember(x => x.Department, opt => opt.MapFrom(x => x.AcademicInformation.Department.NameEn))
                                                       .ForMember(x => x.AdmissionTerm, opt => opt.MapFrom(x => x.AdmissionInformation.AdmissionTerm.TermText))
                                                       .ForMember(x => x.CreditComp, opt => opt.MapFrom(x => x.AcademicInformation.CreditComp))
                                                       .ForMember(x => x.StudentEmail, opt => opt.MapFrom(x => x.Email));

            // CreateMap<StudentScore, GradeMaintenanceViewModel>().ForMember(x => x.CourseCode, opt => opt.MapFrom(x => x.RegistrationCourse.Course.Code))
            //                                                     .ForMember(x => x.CourseName, opt => opt.MapFrom(x => x.RegistrationCourse.Course.NameEn))
            //                                                     .ForMember(x => x.SectionNumber, opt => opt.MapFrom(x => x.RegistrationCourse.Section.Number))
            //                                                     .ForMember(x => x.Grade, opt => opt.MapFrom(x => x.Grade.Name))
            //                                                     .ForMember(x => x.StudentScoreId, opt => opt.MapFrom(x => x.Id))
            //                                                     .ForMember(x => x.StudentFullName, opt => opt.MapFrom(x => x.Student.FullNameEn))
            //                                                     .ForMember(x => x.CurriculumName, opt => opt.MapFrom(x => x.Student.AcademicInformation.CurriculumVersion.Curriculum.NameEn));

            CreateMap<GradingLog, GradingLogDetail>().ForMember(x => x.Course, opt => opt.MapFrom(x => x.StudentRawScore.RegistrationCourse.Course.NameEn))
                                                     .ForMember(x => x.Section, opt => opt.MapFrom(x => x.StudentRawScore.RegistrationCourse.Section.Number))
                                                     .ForMember(x => x.PreviousGrade, opt => opt.MapFrom(x => string.IsNullOrEmpty(x.PreviousGrade)
                                                                                                              ? "-" : x.PreviousGrade))
                                                     .ForMember(x => x.CurrentGrade, opt => opt.MapFrom(x => string.IsNullOrEmpty(x.CurrentGrade)
                                                                                                             ? "-" : x.CurrentGrade))
                                                     .ForMember(x => x.ApprovedAt, opt => opt.MapFrom(x => x.ApprovedAtText));
            
            CreateMap<Barcode, PublishedGradeReportViewModel>().ForMember(x => x.Course, opt => opt.MapFrom(x => x.Course.CourseAndCredit))
                                                               .ForMember(x => x.SectionIds, opt => opt.MapFrom(x => x.SectionIds == null
                                                                                                                     ? null: JsonConvert.DeserializeObject<List<long>>(x.SectionIds)))
                                                               .ForMember(x => x.GeneratedAt, opt => opt.MapFrom(x => x.GeneratedAtText))
                                                               .ForMember(x => x.PublishedAt, opt => opt.MapFrom(x => x.PublishedAtText));

            CreateMap<AdmissionExamination, AdmissionExamination>().ForMember(x => x.Id, opt => opt.Ignore())
                                                                   .ForMember(x => x.FacultyId, opt => opt.Ignore())
                                                                   .ForMember(x => x.DepartmentId, opt => opt.Ignore());

            CreateMap<AdmissionExamination, AdmissionExaminationViewModel>().ForMember(x => x.AdmissionTermId, opt => opt.MapFrom(x => x.AdmissionRound.AdmissionTermId))
                                                                            .ForMember(x => x.AdmissionRound, opt => opt.MapFrom(x => x.AdmissionRound.TermRoundText))
                                                                            .ForMember(x => x.AcademicLevel, opt => opt.MapFrom(x => x.AcademicLevel.NameEn))
                                                                            .ForMember(x => x.Faculty, opt => opt.MapFrom(x => x.Faculty.CodeAndName))
                                                                            .ForMember(x => x.Department, opt => opt.MapFrom(x => x.Department == null ? "All" : x.Department.CodeAndName));

            CreateMap<AdmissionExaminationViewModel, AdmissionExamination>().ForMember(x => x.Id, opt => opt.Ignore())
                                                                            .ForMember(x => x.FacultyId, opt => opt.Ignore())
                                                                            .ForMember(x => x.DepartmentId, opt => opt.Ignore())
                                                                            .ForMember(x => x.CreatedAt, opt => opt.Ignore())
                                                                            .ForMember(x => x.CreatedBy, opt => opt.Ignore())
                                                                            .ForMember(x => x.AdmissionExaminationSchedules, opt => opt.Ignore());

            CreateMap<VerificationLetter, VerificationLetterViewModel>().ForMember(x => x.RunningNumber, opt => opt.MapFrom(x => x.RunningNumberYear))
                                                                        .ForMember(x => x.Signatory, opt => opt.MapFrom(x => x.Signatory.FullNameTh))
                                                                        .ForMember(x => x.SignatoryPosition, opt => opt.MapFrom(x => x.Signatory.PositionTh));

            CreateMap<VerificationStudent, VerificationStudentViewModel>().ForMember(x => x.Code, opt => opt.MapFrom(x => x.Student.Code))
                                                                          .ForMember(x => x.FullName, opt => opt.MapFrom(x => x.Student.FullNameTh))
                                                                          .ForMember(x => x.PreviousSchool, opt => opt.MapFrom(x => x.Student.AdmissionInformation.PreviousSchool == null
                                                                                                                                    ? "" : x.Student.AdmissionInformation.PreviousSchool.NameTh));

            CreateMap<ExemptedExaminationScore, ExemptedExaminationScoreDetail>().ForMember(x => x.ExemptedExaminationId, opt => opt.MapFrom(x => x.ExemptedAdmissionExaminationId))
                                                                                 .ForMember(x => x.AdmissionTypeId, opt => opt.MapFrom(x => x.AdmissionTypeId));

            CreateMap<Student, ResignStudentViewModel>().ForMember(x => x.StudentCode, opt => opt.MapFrom(x => x.Code))
                                                        .ForMember(x => x.TitleEn, opt => opt.MapFrom(x => x.Title.NameEn))
                                                        .ForMember(x => x.FirstNameEn, opt => opt.MapFrom(x => x.FirstNameEn))
                                                        .ForMember(x => x.MidNameEn, opt => opt.MapFrom(x => x.MidNameEn))
                                                        .ForMember(x => x.LastNameEn, opt => opt.MapFrom(x => x.LastNameEn))
                                                        .ForMember(x => x.FacultyNameEn, opt => opt.MapFrom(x => x.AcademicInformation.Faculty.ShortNameEn))
                                                        .ForMember(x => x.DepartmentNameEn, opt => opt.MapFrom(x => x.AcademicInformation.Department.ShortNameEn))
                                                        .ForMember(x => x.GPA, opt => opt.MapFrom(x => x.AcademicInformation.GPA))
                                                        .ForMember(x => x.CreditEarned, opt => opt.MapFrom(x => x.AcademicInformation.CreditEarned ?? 0))
                                                        .ForMember(x => x.AcademicLevel, opt => opt.MapFrom(x => x.AcademicInformation.AcademicLevel.NameEn))
                                                        .ForMember(x => x.Curriculum, opt => opt.MapFrom(x => x.AcademicInformation.CurriculumVersion.Curriculum.NameEn))
                                                        .ForMember(x => x.CurriculumVersion, opt => opt.MapFrom(x => x.AcademicInformation.CurriculumVersion.NameEn))
                                                        .ForMember(x => x.AdmissionTerm, opt => opt.MapFrom(x => x.AdmissionInformation.AdmissionTerm.TermText))
                                                        .ForMember(x => x.StudentId, opt => opt.MapFrom(x => x.Id));

            CreateMap<Invoice, Receipt>().ForMember(x => x.Id, opt => opt.Ignore())
                                         .ForMember(x => x.RunningNumber, opt => opt.Ignore())
                                         .ForMember(x => x.Number, opt => opt.Ignore())
                                         .ForMember(x => x.Round, opt => opt.MapFrom(x => x.Type))
                                         .ForMember(x => x.InvoiceId, opt => opt.MapFrom(x => x.Id));
            
            CreateMap<InvoiceItem, ReceiptItem>().ForMember(x => x.Id, opt => opt.Ignore())
                                                 .ForMember(x => x.InvoiceItemId, opt => opt.MapFrom(x => x.Id));

            CreateMap<RoomSlot, TeachingScheduleDetail>().ForMember(x => x.Term, opt => opt.MapFrom(x => x.Term.TermText))
                                                         .ForMember(x => x.CourseCode, opt => opt.MapFrom(x => x.SectionSlot.Section.Course.Code))
                                                         .ForMember(x => x.CourseName, opt => opt.MapFrom(x => x.SectionSlot.Section.Course.NameEn))
                                                         .ForMember(x => x.Section, opt => opt.MapFrom(x => x.SectionSlot.Section.Number))
                                                         .ForMember(x => x.Day, opt => opt.MapFrom(x => x.Dayofweek))
                                                         .ForMember(x => x.Time, opt => opt.MapFrom(x => x.Time))
                                                         .ForMember(x => x.SectionInstructor, opt => opt.MapFrom(x => x.SectionSlot.Instructor.FullNameEn))
                                                         .ForMember(x => x.ExaminationInstructor, opt => opt.MapFrom(x => x.ExaminationReservation.Instructor.FullNameEn))
                                                         .ForMember(x => x.SeatUsed, opt => opt.MapFrom(x => x.SectionSlot.Section.SeatUsed))
                                                         .ForMember(x => x.RoomReservationName, opt => opt.MapFrom(x => x.RoomReservation.Name))
                                                         .ForMember(x => x.UsingType, opt => opt.MapFrom(x => x.UsingTypeText));
                                         
            CreateMap<CourseGroup, CourseGroup>().ForMember(x => x.Id, opt => opt.Ignore())
                                                 .ForMember(x => x.CourseGroupId, opt => opt.Ignore());
            
            CreateMap<CurriculumCourse, CurriculumCourse>().ForMember(x => x.Id, opt => opt.Ignore())
                                                           .ForMember(x => x.CourseGroupId, opt => opt.Ignore());

            // GradeTemplate conversion (All fields, that we need, have the same name. No need for .ForMember to be assigned)
            CreateMap<GradeTemplate, GradeTemplateViewModel>();
            CreateMap<GradeTemplateViewModel, GradeTemplate>();
            
            CreateMap<ScholarshipBudget, ScholarshipBudget>().ForMember(x => x.Id, opt => opt.Ignore())
                                                             .ForMember(x => x.ScholarshipId, opt => opt.Ignore());

            CreateMap<AdmissionInformation, AdmissionInformation>().ForMember(x => x.Id, opt => opt.Ignore());

            CreateMap<RequiredDocument, StudentDocument>().ForMember(x => x.Id, opt => opt.Ignore())
                                                          .ForMember(x => x.RequiredDocumentId, opt => opt.MapFrom(x => x.Id));

            CreateMap<Student, BlacklistedStudent>();
            
            CreateMap<RegistrationApplicationViewModel, Student>();
            CreateMap<RegistrationApplicationViewModel, AdmissionInformation>();
            CreateMap<RegistrationApplicationViewModel, StudentAddress>();
            CreateMap<Student, RegistrationApplicationViewModel>();
            CreateMap<AdmissionInformation, RegistrationApplicationViewModel>();
            CreateMap<StudentAddress, RegistrationApplicationViewModel>();

            CreateMap<Student, AdvisorViewModel>().ForMember(x => x.StudentId, opt => opt.MapFrom(x => x.Id))
                                                  .ForMember(x => x.StudentCode, opt => opt.MapFrom(x => x.Code))
                                                  .ForMember(x => x.StudentName, opt => opt.MapFrom(x => x.FullNameEn))
                                                  .ForMember(x => x.AcademicLevel, opt => opt.MapFrom(x => x.AcademicInformation.AcademicLevel.NameEn))
                                                  .ForMember(x => x.Faculty, opt => opt.MapFrom(x => x.AcademicInformation.Faculty.NameEn))
                                                  .ForMember(x => x.Department, opt => opt.MapFrom(x => x.AcademicInformation.Department.NameEn))
                                                  .ForMember(x => x.Curriculum, opt => opt.MapFrom(x => x.AcademicInformation.CurriculumVersion == null
                                                                                                        ? "" : x.AcademicInformation.CurriculumVersion.Curriculum.NameEn))
                                                  .ForMember(x => x.CurriculumVersion, opt => opt.MapFrom(x => x.AcademicInformation.CurriculumVersion == null
                                                                                                               ? "" : x.AcademicInformation.CurriculumVersion.NameEn))
                                                  .ForMember(x => x.CreditComplete, opt => opt.MapFrom(x => x.AcademicInformation.CreditComp))
                                                  .ForMember(x => x.CreditEarned, opt => opt.MapFrom(x => x.AcademicInformation.CreditEarned))
                                                  .ForMember(x => x.CreditTransfer, opt => opt.MapFrom(x => x.AcademicInformation.CreditTransfer))
                                                  .ForMember(x => x.GPA, opt => opt.MapFrom(x => x.AcademicInformation.GPA));

            CreateMap<GraduationInformation, GraduatedStudentReportDetail>().ForMember(x => x.Code, opt => opt.MapFrom(x => x.Student.Code))
                                                                            .ForMember(x => x.Name, opt => opt.MapFrom(x => x.Student.FullNameEn))
                                                                            .ForMember(x => x.GraduatedTerm, opt => opt.MapFrom(x => x.Term.TermText))
                                                                            .ForMember(x => x.GraduatedDate, opt => opt.MapFrom(x => x.GraduatedAtText))
                                                                            .ForMember(x => x.GraduatedClass, opt => opt.MapFrom(x => x.ClassInt))
                                                                            .ForMember(x => x.Honor, opt => opt.MapFrom(x => x.AcademicHonor.NameEn))
                                                                            .ForMember(x => x.StudentStatus, opt => opt.MapFrom(x => x.Student.StudentStatusText));
                                                                            
            CreateMap<Section, KeystoneLibrary.Models.Api.SectionViewModel>().ForMember(x => x.KSSectionId, opt => opt.MapFrom(x => x.Id))
                                                                             .ForMember(x => x.KSParentSectionId, opt => opt.MapFrom(x => x.ParentSectionId))
                                                                             .ForMember(x => x.KSCourseId, opt => opt.MapFrom(x => x.CourseId))
                                                                             .ForMember(x => x.KSSemesterId, opt => opt.MapFrom(x => x.TermId))
                                                                             .ForMember(x => x.MidtermRoomName, opt => opt.MapFrom(x => x.MidtermRoomId == null ? string.Empty : x.MidtermRoom.NameEn))
                                                                             .ForMember(x => x.FinalRoomName, opt => opt.MapFrom(x => x.FinalRoomId == null ? string.Empty : x.FinalRoom.NameEn))
                                                                             .ForMember(x => x.MidtermDate, opt => opt.MapFrom(x => x.MidtermDate == null ? (DateTime?)null : x.MidtermDate.Value.ToUniversalTime()))
                                                                             .ForMember(x => x.FinalDate, opt => opt.MapFrom(x => x.FinalDate == null ? (DateTime?)null : x.FinalDate.Value.ToUniversalTime()));

            CreateMap<SectionDetail, KeystoneLibrary.Models.Api.SectionDetailViewModel>().ForMember(x => x.KSSectionDetailId, opt => opt.MapFrom(x => x.Id))
                                                                                         .ForMember(x => x.InstructorCode, opt => opt.MapFrom(x => x.InstructorId == null ? string.Empty : x.Instructor.Code))
                                                                                         .ForMember(x => x.RoomName, opt => opt.MapFrom(x => x.RoomId == null ? string.Empty : x.Room.NameEn))
                                                                                         .ForMember(x => x.Day, opt => opt.MapFrom(x => x.Day + 1))
                                                                                         .ForMember(x => x.StartTime, opt => opt.MapFrom(x => x.StartTime))
                                                                                         .ForMember(x => x.EndTime, opt => opt.MapFrom(x => x.EndTime));                                                       
            
            CreateMap<SectionSlot, KeystoneLibrary.Models.Api.SectionSlotViewModel>().ForMember(x => x.StartedAt, opt => opt.MapFrom(x => (x.Date.Date + x.StartTime).ToUniversalTime()))
                                                                                     .ForMember(x => x.EndedAt, opt => opt.MapFrom(x => (x.Date.Date + x.EndTime).ToUniversalTime()))
                                                                                     .ForMember(x => x.InstructorCode, opt => opt.MapFrom(x => x.InstructorId == null ? string.Empty : x.Instructor.Code))
                                                                                     .ForMember(x => x.Status, opt => opt.MapFrom(x => x.StatusText.ToUpper()))
                                                                                     .ForMember(x => x.RoomName, opt => opt.MapFrom(x => x.RoomId == null ? string.Empty : x.Room.NameEn));
            

            CreateMap<SectionDetail, SectionDetail>().ForMember(x => x.Id, opt => opt.Ignore())
                                                     .ForMember(x => x.SectionId, opt => opt.Ignore());
            CreateMap<Section, Section>().ForMember(x => x.Id, opt => opt.Ignore());

            CreateMap<ScheduleViewModel, ScheduleViewModel>().ForMember(x => x.ScheduleTimes, opt => opt.Ignore());
            CreateMap<ClassScheduleTimeViewModel, ClassScheduleTimeViewModel>();

            #region Certificate
            CreateMap<Student, CertificationViewModel>().ForMember(x => x.AcademicLevelId, opt => opt.MapFrom(x => x.AcademicInformation.AcademicLevelId))
                                                        .ForMember(x => x.StudentId, opt => opt.MapFrom(x => x.Id))
                                                        .ForMember(x => x.StudentCode, opt => opt.MapFrom(x => x.Code))
                                                        .ForMember(x => x.Gender, opt => opt.MapFrom(x => x.Gender))
                                                        .ForMember(x => x.FacultyId, opt => opt.MapFrom(x => x.AcademicInformation.FacultyId))
                                                        .ForMember(x => x.DepartmentId, opt => opt.MapFrom(x => x.AcademicInformation.DepartmentId))
                                                        .ForMember(x => x.GPA, opt => opt.MapFrom(x => x.AcademicInformation.GPA))
                                                        .ForMember(x => x.CreditComp, opt => opt.MapFrom(x => x.AcademicInformation.CreditComp))
                                                        .ForMember(x => x.CreditEarned, opt => opt.MapFrom(x => x.AcademicInformation.CreditEarned))
                                                        .ForMember(x => x.IsAdmissionStudent, opt => opt.MapFrom(x => x.RegistrationCourses == null
                                                                                                                      || x.RegistrationCourses.All(y => y.Course.IsIntensiveCourse)
                                                                                                                      || !x.RegistrationCourses.Any(y => y.Status == "a" || y.Status == "r")))
                                                        .ForMember(x => x.TotalCredit, opt => opt.MapFrom(x => x.AcademicInformation.CurriculumVersion == null ? 0
                                                                                                                                                               : x.AcademicInformation.CurriculumVersion.TotalCredit))
                                                        .ForMember(x => x.GraduatedAt, opt => opt.MapFrom(x => x.GraduationInformation == null ? null
                                                                                                                                               : x.GraduationInformation.GraduatedAt))
                                                        .ForMember(x => x.GraduatedYear, opt => opt.MapFrom(x => x.GraduationInformation == null ? (int?)null
                                                                                                                                                 : x.GraduationInformation.Term.AcademicYear))
                                                        .ForMember(x => x.AdmissionYear, opt => opt.MapFrom(x => x.AdmissionInformation == null ? 0
                                                                                                                                                : x.AdmissionInformation.AdmissionTerm.AcademicYear))
                                                        .ForMember(x => x.AdmissionDate, opt => opt.MapFrom(x => x.AdmissionInformation == null ? new DateTime()
                                                                                                                                                : x.AdmissionInformation.AdmissionDate));

            CreateMap<CertificationViewModel, PrintingLog>().ForMember(x => x.FirstName, opt => opt.MapFrom(x => x.StudentFirstName))
                                                            .ForMember(x => x.LastName, opt => opt.MapFrom(x => x.StudentLastName))
                                                            .ForMember(x => x.Year, opt => opt.MapFrom(x => x.DocumentYear));

            CreateMap<UpdateTrackingNumberViewModel, Criteria>();
            #endregion

            #region Fee
            CreateMap<ReceiptItem, RefundDetail>().ForMember(x => x.CourseName, opt => opt.MapFrom(x => x.InvoiceItem.Course == null ? "" : x.InvoiceItem.Course.NameEn))
                                                  .ForMember(x => x.CourseId, opt => opt.MapFrom(x => x.InvoiceItem == null ? 0 : x.InvoiceItem.CourseId))
                                                  .ForMember(x => x.SectionNumber, opt => opt.MapFrom(x => x.InvoiceItem.Section == null ? "" : x.InvoiceItem.Section.Number))
                                                  .ForMember(x => x.SectionId, opt => opt.MapFrom(x => x.InvoiceItem == null ? 0 : x.InvoiceItem.SectionId))
                                                  .ForMember(x => x.FeeItemName, opt => opt.MapFrom(x => x.InvoiceItem.FeeItem == null ? "" : x.InvoiceItem.FeeItem.NameEn))
                                                  .ForMember(x => x.ReceiptItemId, opt => opt.MapFrom(x => x.Id));
            
            CreateMap<Receipt, FinanceOtherFeeViewModel>().ForMember(x => x.ReceiptNumber, opt => opt.MapFrom(x => x.Number))
                                                          .ForMember(x => x.StudentCode, opt => opt.MapFrom(x => x.Student.Code))
                                                          .ForMember(x => x.StudentName, opt => opt.MapFrom(x => x.Student.FirstNameTh == null ? x.Student.FullNameTh
                                                                                                                                               : x.Student.FullNameEn))
                                                          .ForMember(x => x.InvoiceNumber, opt => opt.MapFrom(x => x.Invoice.Number))
                                                          .ForMember(x => x.TotalAmount, opt => opt.MapFrom(x => x.TotalAmountText))
                                                          .ForMember(x => x.Date, opt => opt.MapFrom(x => x.CreatedAtText))
                                                          .ForMember(x => x.Time, opt => opt.MapFrom(x => x.CreatedAt.ToString(StringFormat.TimeSpan)))
                                                          .ForMember(x => x.PrintedBy, opt => opt.MapFrom(x => x.PrintedBy))
                                                          .ForMember(x => x.ReceiptItems, opt => opt.Ignore());
            #endregion
        }
    }
}