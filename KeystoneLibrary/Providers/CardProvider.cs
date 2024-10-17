using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Profile;
using Microsoft.EntityFrameworkCore;

namespace KeystoneLibrary.Providers
{
    public class CardProvider : BaseProvider, ICardProvider
    {
        protected IStudentProvider _studentProvider;
        protected ICacheProvider _cacheProvider;
        protected IDateTimeProvider _dateTimeProvider;
        protected IAcademicProvider _academicProvider;
        protected IStudentPhotoProvider _studentPhotoProvider;
        public CardProvider(ApplicationDbContext db,
                            IMapper mapper,
                            IStudentProvider studentProvider,
                            ICacheProvider cacheProvider,
                            IDateTimeProvider dateTimeProvider,
                            IAcademicProvider academicProvider,
                            IStudentPhotoProvider studentPhotoProvider) : base(db, mapper)
        {
            _studentProvider = studentProvider;
            _cacheProvider = cacheProvider;
            _academicProvider = academicProvider;
            _dateTimeProvider = dateTimeProvider;
            _studentPhotoProvider = studentPhotoProvider;
        }

        public async Task<StudentIdCardViewModel> GetStudentIdCardForm(List<Student> students)
        {
            var model = new StudentIdCardViewModel();
            foreach (var item in students)
            {
                var detail = new StudentIdCardFormDetail
                             {
                                 Code = item.Code,
                                 FirstName = item.FirstNameEn,
                                 LastName = item.LastNameEn,
                                 ThaiFirstName = item.FirstNameTh,
                                 ThaiLastName = item.LastNameTh,
                                 AcademicLevel = item.AcademicInformation.AcademicLevel.NameEn,
                                 Faculty = item.AcademicInformation.Faculty.NameEn,
                                 FacultyAbbreviation = item.AcademicInformation.Faculty.Abbreviation,
                                 DepartmentAbbreviation = item.AcademicInformation.Department.Abbreviation,
                                 Barcode = item.Barcode,
                                 ExpiredDate = item.IdCardExpiredDate,
                                 ProfileImageURL = item.ProfileImageURL
                             };
                try
                {
                    detail.ProfileImageURL = await _studentPhotoProvider.GetStudentImg(detail.Code);
                }
                catch (Exception)
                {
                }
                model.FormDetails.Add(detail);
            }

            return model;
        }

        public async Task<StudentIdCardDetail> GetStudentSubstitudedCard(StudentIdCardViewModel viewModel)
        {
            var student = _db.Students.Include(x => x.Title)
                                      .Include(x => x.AcademicInformation)
                                          .ThenInclude(x => x.Faculty)
                                      .Include(x => x.AcademicInformation)
                                          .ThenInclude(x => x.Department)
                                      .AsNoTracking()
                                      .SingleOrDefault(x => x.Code == viewModel.Code);
            
            if (student is null)
            {
                return new StudentIdCardDetail();
            }

            var model = new StudentIdCardDetail
            {
                TitleId = student.TitleId,
                Title = student.Title.NameEn,
                FirstName = viewModel.StudentIdCardDetail == null ? student.FirstNameEn
                                                                  : viewModel.StudentIdCardDetail?.FirstName ?? "N/A",
                LastName = viewModel.StudentIdCardDetail == null ? student.LastNameEn
                                                                 : viewModel.StudentIdCardDetail?.LastName ?? "N/A",
                StudentId = student.Id,
                AcademicLevelId = student.AcademicInformation?.AcademicLevelId ?? 0,
                FacultyId = student.AcademicInformation?.FacultyId ?? 0,
                FacultyName = student.AcademicInformation?.Faculty?.NameEn ?? "",
                DepartmentId = student.AcademicInformation?.DepartmentId ?? 0,
                DepartmentName = student.AcademicInformation?.Department?.NameEn ?? "",
                Code = student.Code,
                ImagePath = student.ProfileImageURL,
                ExaminationTypeName = viewModel.ExaminationType,
                ProfileImageURL = student.ProfileImageURL
            };

            try
            {
                model.ProfileImageURL = await _studentPhotoProvider.GetStudentImg(model.Code);
            }
            catch (Exception)
            {
            }

            var currentTerm = _cacheProvider.GetCurrentTerm(student.AcademicInformation.AcademicLevelId);
            if (currentTerm is null)
            {
                return model;
            }

            var examDate = _dateTimeProvider.ConvertStringToDateTime(viewModel.ExaminationDate);
            if (!examDate.HasValue)
            {
                return model;
            }

            var query = _db.RegistrationCourses.Include(x => x.Course)
                                               .Include(x => x.Section)
                                                   .ThenInclude(x => x.MainInstructor)
                                               .AsNoTracking()
                                               .Where(x => x.StudentId == student.Id
                                                           && x.TermId == currentTerm.Id
                                                           && x.CourseId == viewModel.CourseId
                                                           && x.Status != "d");

            if (viewModel.ExaminationType == "Midterm")
            {
                query = query.Where(x => x.Section.MidtermDate.HasValue 
                                         && x.Section.MidtermStart.HasValue
                                         && x.Section.MidtermEnd.HasValue
                                         && x.Section.MidtermDate.Value >= examDate.Value.Date
                                         && x.Section.MidtermDate.Value < examDate.Value.AddDays(1).Date);
            }
            else
            {
                query = query.Where(x => x.Section.FinalDate.HasValue 
                                         && x.Section.FinalStart.HasValue
                                         && x.Section.FinalEnd.HasValue
                                         && x.Section.FinalDate.Value >= examDate.Value.Date
                                         && x.Section.FinalDate.Value < examDate.Value.AddDays(1).Date);
            }

            var registrationCourse = query.FirstOrDefault();
            
            if (registrationCourse is null)
            {
                return model;
            }

            model.CourseId = registrationCourse.CourseId;
            model.CourseCode = registrationCourse.Course?.Code ?? "N/A";
            model.CourseName = registrationCourse.Course?.NameEnAndCredit ?? "N/A";
            model.InstructorName = registrationCourse.Section?.MainInstructor?.FullNameEn ?? "N/A";
            model.SectionId = registrationCourse.SectionId ?? 0;
            model.SectionNumber = registrationCourse.SectionNumber ?? "N/A";
            model.ExaminationDate = viewModel.ExaminationType == "Midterm" ? 
                                    $"{ registrationCourse.Section?.DayOfWeekMidterm } { registrationCourse.Section?.MidtermString }"
                                    : $"{ registrationCourse.Section?.DayOfWeekFinal } { registrationCourse.Section?.FinalString }";
            model.MidtermStart = registrationCourse.Section?.MidtermStart?.ToString(StringFormat.TimeSpan);
            model.MidtermEnd = registrationCourse.Section?.MidtermEnd?.ToString(StringFormat.TimeSpan);
            model.FinalStart = registrationCourse.Section?.FinalStart?.ToString(StringFormat.TimeSpan);
            model.FinalEnd = registrationCourse.Section?.FinalEnd?.ToString(StringFormat.TimeSpan);

            return model;
        }

        public DateTime GetCardExpiration(Guid studentId, long academicLevelId, long? facultyId, long? departmentId, DateTime? cardCreatedDate)
        {
            var byAcademicLevels = _db.CardExpirationOptions.Where(x => x.AcademicLevelId == academicLevelId
                                                                        && (x.DepartmentId == null || x.DepartmentId == departmentId)
                                                                        && (x.FacultyId == null || x.FacultyId == facultyId));
            
            int year = 0;
            if (byAcademicLevels.Any())
            {
                year = byAcademicLevels.OrderByDescending(x => x.DepartmentId)
                                       .ThenByDescending(x => x.FacultyId)
                                       .FirstOrDefault()
                                       .ValidityYear;
            }
            
            var expiredDate = cardCreatedDate.HasValue ? cardCreatedDate.Value.AddYears(year) : default(DateTime);

            return expiredDate;
        }
    }
}