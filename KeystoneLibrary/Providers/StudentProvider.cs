using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Admission;
using KeystoneLibrary.Models.DataModels.MasterTables;
using KeystoneLibrary.Models.DataModels.Profile;
using KeystoneLibrary.Models.DataModels.Scholarship;
using Microsoft.EntityFrameworkCore;
using KeystoneLibrary.Enumeration;
using ThaiNationalIDCard.NET;
using ThaiNationalIDCard.NET.Models;

namespace KeystoneLibrary.Providers
{
    public class StudentProvider : BaseProvider, IStudentProvider
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IAdmissionProvider _admissionProvider;
        private readonly IAcademicProvider _academicProvider;
        protected readonly IUserProvider _userProvider;
        public StudentProvider(ApplicationDbContext db,
                               IDateTimeProvider dateTimeProvider,
                               IAdmissionProvider admissionProvider,
                               IAcademicProvider academicProvider,
                               IUserProvider userProvider
                               ) : base(db)
        {
            _dateTimeProvider = dateTimeProvider;
            _admissionProvider = admissionProvider;
            _academicProvider = academicProvider;
            _userProvider = userProvider;
        }

        public bool IsActiveStudentGraduationInformation(GraduationInformation model)
        {
            var isActive = _db.GraduationInformations.Any(x => x.Id != model.Id
                                                               && x.StudentId == model.StudentId
                                                               && x.IsActive);
            return isActive;
        }

        public bool IsExistStudent(string code, string status = "s")
        {
            return _db.Students.Any(x => x.Code == code
                                         && x.StudentStatus == status);
        }

        public bool IsExistStudentCodeAndStatus(string code, string status)
        {
            return _db.Students.IgnoreQueryFilters()
                               .Any(x => x.Code == code
                                         && (x.StudentStatus == status || string.IsNullOrEmpty(status)));
        }

        public bool IsExistStudentExceptAdmission(Guid id)
        {
            return _db.Students.IgnoreQueryFilters()
                               .Any(x => x.Id == id
                                         && x.StudentStatus != "a");
        }

        public bool IsExistAllStudent(string code)
        {
            return _db.Students.Any(x => x.Code == code);
        }

        public bool IsExistStudentExceptAdmission(string code)
        {
            return _db.Students.IgnoreQueryFilters()
                               .Any(x => x.Code == code
                                         && x.StudentStatus != "a");
        }

        public Student GetStudentById(Guid id)
        {
            var student = _db.Students.Include(x => x.AcademicInformation)
                                      .Include(x => x.Title)
                                      .Include(x => x.AdmissionInformation)
                                      .Include(x => x.StudentAddresses)
                                      .SingleOrDefault(x => x.Id == id);
            return student;
        }

        public Student GetStudentByCode(string code)
        {
            var student = _db.Students.Include(x => x.AcademicInformation)
                                          .ThenInclude(x => x.Faculty)
                                      .Include(x => x.AcademicInformation)
                                          .ThenInclude(x => x.Department)
                                      .Include(x => x.AcademicInformation)
                                          .ThenInclude(x => x.CurriculumVersion)
                                      .Include(x => x.Title)
                                      .Include(x => x.AdmissionInformation)
                                      .Include(x => x.CurriculumInformations)
                                      .Include(x => x.Nationality)
                                      .Include(x => x.StudentFeeType)
                                      .IgnoreQueryFilters()
                                      .SingleOrDefault(x => x.Code == code);
            return student;
        }

        public Student GetStudentAndAdmissionStudentByCode(string code)
        {
            var student = _db.Students.Include(x => x.AcademicInformation)
                                          .ThenInclude(x => x.AcademicLevel)
                                      .Include(x => x.AcademicInformation)
                                          .ThenInclude(x => x.Faculty)
                                      .Include(x => x.AcademicInformation)
                                          .ThenInclude(x => x.Department)
                                      .Include(x => x.AcademicInformation)
                                          .ThenInclude(x => x.CurriculumVersion)
                                      .Include(x => x.AdmissionInformation)
                                          .ThenInclude(x => x.AdmissionTerm)
                                      .Include(x => x.StudentFeeGroup)
                                      .Where(x => x.StudentStatus == "a"
                                                  || x.StudentStatus == "s")
                                      .SingleOrDefault(x => x.Code == code);
            return student;
        }

        public Student GetStudentInformationByCode(string code)
        {
            var student = _db.Students.Include(x => x.Nationality)
                                      .Include(x => x.AcademicInformation)
                                          .ThenInclude(x => x.Faculty)
                                      .Include(x => x.AcademicInformation)
                                          .ThenInclude(x => x.Department)
                                      .Include(x => x.AcademicInformation)
                                          .ThenInclude(x => x.CurriculumVersion)
                                          .ThenInclude(x => x.Curriculum)
                                      .Include(x => x.AcademicInformation)
                                          .ThenInclude(x => x.AcademicLevel)
                                      .Include(x => x.AcademicInformation)
                                          .ThenInclude(x => x.Advisor)
                                          .ThenInclude(x => x.Title)
                                      .Include(x => x.StudentFeeGroup)
                                      .Include(x => x.AdmissionInformation)
                                          .ThenInclude(x => x.PreviousSchool)
                                      .Include(x => x.AdmissionInformation)
                                          .ThenInclude(x => x.AdmissionTerm)
                                      .Include(x => x.AdmissionInformation)
                                          .ThenInclude(x => x.AdmissionType)
                                      .Include(x => x.GraduatingRequest)
                                      .Include(x => x.GraduationInformations)
                                          .ThenInclude(x => x.Term)
                                      .Include(x => x.GraduationInformations)
                                          .ThenInclude(x => x.CurriculumInformation)
                                          .ThenInclude(x => x.CurriculumVersion)
                                          .ThenInclude(x => x.Curriculum)
                                      .Include(x => x.GraduationInformations)
                                          .ThenInclude(x => x.AcademicHonor)
                                      .Include(x => x.ParentInformations)
                                          .ThenInclude(x => x.Relationship)
                                      .Include(x => x.StudentAddresses)
                                          .ThenInclude(x => x.Country)
                                      .Include(x => x.StudentAddresses)
                                          .ThenInclude(x => x.City)
                                      .Include(x => x.StudentAddresses)
                                          .ThenInclude(x => x.State)
                                      .Include(x => x.StudentAddresses)
                                          .ThenInclude(x => x.Province)
                                      .Include(x => x.StudentAddresses)
                                          .ThenInclude(x => x.District)
                                      .Include(x => x.StudentAddresses)
                                          .ThenInclude(x => x.Subdistrict)
                                      .Include(x => x.CheatingStatuses)
                                          .ThenInclude(x => x.Term)
                                      .Include(x => x.CheatingStatuses)
                                          .ThenInclude(x => x.FromTerm)
                                      .Include(x => x.CheatingStatuses)
                                          .ThenInclude(x => x.ToTerm)
                                      .Include(x => x.CheatingStatuses)
                                          .ThenInclude(x => x.RegistrationCourse)
                                          .ThenInclude(x => x.Section)
                                          .ThenInclude(x => x.Course)
                                      .Include(x => x.CheatingStatuses)
                                          .ThenInclude(x => x.ExaminationType)
                                      .Include(x => x.CheatingStatuses)
                                          .ThenInclude(x => x.Incident)
                                      .Include(x => x.MaintenanceStatuses)
                                          .ThenInclude(x => x.Term)
                                      .Include(x => x.MaintenanceStatuses)
                                          .ThenInclude(x => x.MaintenanceFee)
                                      .Include(x => x.StudentIncidents)
                                          .ThenInclude(x => x.Incident)
                                      .Include(x => x.RegistrationCourses)
                                          .ThenInclude(x => x.Course)
                                      .Include(x => x.Title)
                                      .Include(x => x.StudentStatusLogs)
                                      .Include(x => x.CurriculumInformations)
                                          .ThenInclude(x => x.Department)
                                          .ThenInclude(x => x.Faculty)
                                      .Include(x => x.CurriculumInformations)
                                          .ThenInclude(x => x.CurriculumVersion)
                                      .Include(x => x.CurriculumInformations)
                                          .ThenInclude(x => x.StudyPlan)
                                      .Include(x => x.CurriculumInformations)
                                          .ThenInclude(x => x.SpecializationGroupInformations)
                                          .ThenInclude(x => x.SpecializationGroup)
                                      .Include(x => x.StudentProbations)
                                          .ThenInclude(x => x.Term)
                                      .Include(x => x.StudentProbations)
                                          .ThenInclude(x => x.Probation)
                                      .Include(x => x.ScholarshipStudents)
                                      .Include(x => x.StudentDocuments)
                                      .Include(x => x.StudentExemptedExamScores)
                                      .Include(x => x.StudentFeeType)
                                      .IgnoreQueryFilters()
                                      .SingleOrDefault(x => x.Code == code
                                                            && x.StudentStatus != "a");
            return student;
        }

        public Student GetStudentInformationByCitizenCard()
        {
            Student student = new Student();
            ThaiNationalIDCardReader cardReader = new ThaiNationalIDCardReader();
            PersonalPhoto personalPhoto = cardReader.GetPersonalPhoto();
            var nameEn = personalPhoto.EnglishPersonalInfo;
            student.TitleId = _db.Titles.FirstOrDefault(x => x.NameEn.ToLower().Contains(nameEn.Prefix.ToLower()))?.Id ?? 0;
            student.FirstNameEn = nameEn.FirstName;
            student.MidNameEn = nameEn.MiddleName;
            student.LastNameEn = nameEn.LastName;
            var nameTh = personalPhoto.ThaiPersonalInfo;
            student.FirstNameTh = nameTh.FirstName;
            student.MidNameTh = nameTh.MiddleName;
            student.LastNameTh = nameTh.LastName;
            student.Gender = Convert.ToInt32(personalPhoto.Sex);
            student.BirthDate = personalPhoto.DateOfBirth;
            student.NationalityId = _db.Nationalities.FirstOrDefault(x => x.NameEn.ToLower().Contains("thai"))?.Id ?? 0;
            student.CitizenNumber = personalPhoto.CitizenID;
            student.ProfileImage64 = personalPhoto.Photo;
            var address = personalPhoto.AddressInfo;
            student.StudentAddresses = new List<StudentAddress>
                                       {
                                           new StudentAddress
                                           {
                                               HouseNumber = address.HouseNo,
                                               Moo = address.VillageNo,
                                               SoiTh = address.Road,
                                               CountryId = _db.Countries.FirstOrDefault(x => x.NameEn.ToLower().Contains("thailand"))?.Id ?? 0,
                                               ProvinceId = _db.Provinces.FirstOrDefault(x => x.NameTh.Contains(address.Province))?.Id ?? 0,
                                               DistrictId = _db.Districts.FirstOrDefault(x => x.NameTh.Contains(address.District))?.Id ?? 0,
                                               SubdistrictId = _db.Subdistricts.FirstOrDefault(x => x.NameTh.Contains(address.SubDistrict))?.Id ?? 0
                                           }
                                       };

            return student;
        }

        public StudentCertificate GetStudentCertificate(long Id)
        {
            var studentCertificate = _db.StudentCertificates.Include(x => x.Title)
                                                            .Include(x => x.Student)
                                                                .ThenInclude(x => x.AcademicInformation)
                                                                .ThenInclude(x => x.Faculty)
                                                            .Include(x => x.Student)
                                                                .ThenInclude(x => x.AcademicInformation)
                                                                .ThenInclude(x => x.Department)
                                                            .Include(x => x.Term)
                                                                .ThenInclude(x => x.AcademicLevel)
                                                            .Include(x => x.DistributionMethod)
                                                            .SingleOrDefault(x => x.Id == Id);
            return studentCertificate;
        }

        public string GetStudentCodeById(Guid Id)
        {
            var studentCode = _db.Students.SingleOrDefault(x => x.Id == Id).Code;
            return studentCode;
        }

        public Student GetStudentInformationById(Guid id)
        {
            var student = _db.Students.Include(x => x.Nationality)
                                      .Include(x => x.AcademicInformation)
                                          .ThenInclude(x => x.Faculty)
                                      .Include(x => x.AcademicInformation)
                                          .ThenInclude(x => x.Department)
                                      .Include(x => x.AcademicInformation)
                                          .ThenInclude(x => x.CurriculumVersion)
                                          .ThenInclude(x => x.Curriculum)
                                      .Include(x => x.AcademicInformation)
                                          .ThenInclude(x => x.AcademicLevel)
                                      .Include(x => x.AcademicInformation)
                                          .ThenInclude(x => x.Advisor)
                                          .ThenInclude(x => x.Title)
                                      .Include(x => x.AdmissionInformation)
                                          .ThenInclude(x => x.PreviousSchool)
                                      .Include(x => x.GraduationInformations)
                                          .ThenInclude(x => x.Term)
                                      .Include(x => x.GraduationInformations)
                                          .ThenInclude(x => x.CurriculumInformation)
                                          .ThenInclude(x => x.CurriculumVersion)
                                          .ThenInclude(x => x.Curriculum)
                                      .Include(x => x.GraduationInformations)
                                          .ThenInclude(x => x.AcademicHonor)
                                      .Include(x => x.StudentFeeGroup)
                                      .Include(x => x.AdmissionInformation)
                                          .ThenInclude(x => x.AdmissionTerm)
                                      .Include(x => x.AdmissionInformation)
                                          .ThenInclude(x => x.AdmissionType)
                                      .Include(x => x.AdmissionInformation)
                                          .ThenInclude(x => x.AdmissionRound)
                                      .Include(x => x.ParentInformations)
                                          .ThenInclude(x => x.Relationship)
                                      .Include(x => x.StudentAddresses)
                                          .ThenInclude(x => x.Country)
                                      .Include(x => x.StudentAddresses)
                                          .ThenInclude(x => x.City)
                                      .Include(x => x.StudentAddresses)
                                          .ThenInclude(x => x.State)
                                      .Include(x => x.StudentAddresses)
                                          .ThenInclude(x => x.Province)
                                      .Include(x => x.StudentAddresses)
                                          .ThenInclude(x => x.District)
                                      .Include(x => x.StudentAddresses)
                                          .ThenInclude(x => x.Subdistrict)
                                      .Include(x => x.CheatingStatuses)
                                          .ThenInclude(x => x.Term)
                                      .Include(x => x.CheatingStatuses)
                                          .ThenInclude(x => x.FromTerm)
                                      .Include(x => x.CheatingStatuses)
                                          .ThenInclude(x => x.ToTerm)
                                      .Include(x => x.CheatingStatuses)
                                          .ThenInclude(x => x.RegistrationCourse)
                                          .ThenInclude(x => x.Section)
                                          .ThenInclude(x => x.Course)
                                      .Include(x => x.CheatingStatuses)
                                          .ThenInclude(x => x.ExaminationType)
                                      .Include(x => x.CheatingStatuses)
                                          .ThenInclude(x => x.Incident)
                                      .Include(x => x.MaintenanceStatuses)
                                          .ThenInclude(x => x.Term)
                                      .Include(x => x.MaintenanceStatuses)
                                          .ThenInclude(x => x.MaintenanceFee)
                                      .Include(x => x.StudentIncidents)
                                          .ThenInclude(x => x.Incident)
                                      .Include(x => x.StudentStatusLogs)
                                      .Include(x => x.CurriculumInformations)
                                          .ThenInclude(x => x.Department)
                                          .ThenInclude(x => x.Faculty)
                                      .Include(x => x.CurriculumInformations)
                                          .ThenInclude(x => x.CurriculumVersion)
                                               .ThenInclude(x => x.Curriculum)
                                      .Include(x => x.CurriculumInformations)
                                          .ThenInclude(x => x.StudyPlan)
                                      .Include(x => x.CurriculumInformations)
                                          .ThenInclude(x => x.SpecializationGroupInformations)
                                          .ThenInclude(x => x.SpecializationGroup)
                                      .Include(x => x.Title)
                                      .Include(x => x.StudentProbations)
                                          .ThenInclude(x => x.Term)
                                      .Include(x => x.StudentProbations)
                                          .ThenInclude(x => x.Probation)
                                      .Include(x => x.ScholarshipStudents)
                                          .ThenInclude(x => x.Scholarship)
                                      .Include(x => x.StudentDocuments)
                                      .Include(x => x.StudentExemptedExamScores)
                                      .IgnoreQueryFilters()
                                      .SingleOrDefault(x => x.Id == id
                                                            && x.StudentStatus != "a");
            return student;
        }

        public string GetNationalitiesByIds(List<long> ids)
        {
            var nationality = _db.Nationalities.Where(x => ids.Contains(x.Id))
                                               .Select(x => x.NameEn)
                                               .ToList();

            return String.Join(", ", nationality);
        }

        public List<RegistrationCourse> GetRegistrationCourseByStudentId(Guid id, long termId)
        {
            var courses = _db.RegistrationCourses.Include(x => x.Section)
                                                 .Include(x => x.Course)
                                                 .Where(x => x.IsPaid
                                                             && x.TermId == termId
                                                             && x.StudentId == id
                                                             && (x.Status == "a"
                                                                 || x.Status == "r"))
                                                 .ToList();
            return courses;
        }
        public List<ResignStudentViewModel> GetResignStudents(Criteria criteria)
        {
            var models = _db.ResignStudents.Where(x => criteria.AcademicLevelId == x.Student.AcademicInformation.AcademicLevelId
                                                        && (criteria.TermId == 0
                                                            || criteria.TermId == x.TermId)
                                                        && (string.IsNullOrEmpty(criteria.Code)
                                                            || criteria.Code == x.Student.Code)
                                                        && (string.IsNullOrEmpty(criteria.CodeAndName)
                                                            || x.Student.FirstNameEn.Contains(criteria.CodeAndName))
                                                        && (string.IsNullOrEmpty(criteria.CodeAndName)
                                                            || x.Student.LastNameEn.Contains(criteria.CodeAndName))
                                                        && (criteria.FacultyId == 0
                                                            || criteria.FacultyId == x.Student.AcademicInformation.FacultyId)
                                                        && (criteria.DepartmentId == 0
                                                            || criteria.DepartmentId == x.Student.AcademicInformation.DepartmentId)
                                                        && (criteria.ResignReasonId == 0
                                                            || criteria.ResignReasonId == x.ResignReasonId)
                                                        && (criteria.StartStudentBatch == null
                                                            || criteria.StartStudentBatch == 0
                                                            || criteria.StartStudentBatch <= x.Student.AcademicInformation.Batch)
                                                        && (criteria.EndStudentBatch == null
                                                            || criteria.EndStudentBatch == 0
                                                            || criteria.EndStudentBatch >= x.Student.AcademicInformation.Batch))
                                           .Select(x => new ResignStudentViewModel
                                           {
                                               StudentCode = x.Student.Code,
                                               TitleEn = x.Student.Title.NameEn,
                                               FirstNameEn = x.Student.FirstNameEn,
                                               MidNameEn = x.Student.MidNameEn,
                                               LastNameEn = x.Student.LastNameEn,
                                               FacultyNameEn = x.Student.AcademicInformation.Faculty.NameEn,
                                               FacultyNameTh = x.Student.AcademicInformation.Faculty.NameTh,
                                               FacultyCode = x.Student.AcademicInformation.Faculty.Code,
                                               DepartmentNameEn = x.Student.AcademicInformation.Department.NameEn,
                                               DepartmentNameTh = x.Student.AcademicInformation.Department.NameTh,
                                               DepartmentCode = x.Student.AcademicInformation.Department.Code,
                                               CurriculumVersionCode = x.Student.AcademicInformation.CurriculumVersion.Code,
                                               CurriculumVersionNameEn = x.Student.AcademicInformation.CurriculumVersion.NameEn,
                                               CurriculumVersionNameTh = x.Student.AcademicInformation.CurriculumVersion.NameTh,
                                               CreditComp = x.Student.AcademicInformation.CreditComp,
                                               CreditEarn = x.Student.AcademicInformation.CreditEarned,
                                               GPA = x.Student.AcademicInformation.GPA,
                                               StudentStatus = x.Student.StudentStatus,
                                               ResignReason = x.ResignReason.DescriptionEn,
                                               Remark = x.Remark,
                                               EffectiveTerm = x.EffectiveTerm == null ? "" : x.EffectiveTerm.AcademicTerm + "/" + x.EffectiveTerm.AcademicYear,
                                               ApprovedAtText = x.ApprovedAtText
                                           })
                                           .IgnoreQueryFilters()
                                           .ToList();

            return models;
        }
        public List<Student> GetStudentForLatedPayment(long termId)
        {
            var latedPaymentStudents = _db.LatePaymentTransactions.Where(x => x.TermId == termId)
                                                                  .Select(x => x.StudentId)
                                                                  .ToList();

            var students = _db.RegistrationCourses.Include(x => x.Student)
                                                  .Where(x => x.TermId == termId
                                                              && !latedPaymentStudents.Contains(x.StudentId))
                                                  .Select(x => x.Student)
                                                  .Distinct()
                                                  .OrderBy(x => x.Code)
                                                  .ToList();

            return students;
        }

        public GraduationInformation GetStudentGraduationInformation(long id)
        {
            var student = _db.GraduationInformations.IgnoreQueryFilters()
                                                    .Include(x => x.CurriculumInformation)
                                                        .ThenInclude(x => x.CurriculumVersion)
                                                        .ThenInclude(x => x.Curriculum)
                                                    .Include(x => x.Student)
                                                        .ThenInclude(x => x.AcademicInformation)
                                                    .Where(x => x.Id == id)
                                                    .FirstOrDefault();

            return student;
        }

        public int GetStudyYear(int admissionYear, int graduatedYear, int currentAcademicYear)
        {
            int studyYear = 1;
            if (admissionYear != 0)
            {
                studyYear = graduatedYear == 0 ? (currentAcademicYear - admissionYear + 1)
                                               : (graduatedYear - admissionYear + 1);
            }

            return studyYear > 4 ? 4 : studyYear;
        }

        public string GetTitleById(long id, string language = "en")
        {
            var title = _db.Titles.Where(x => x.Id == id)
                                  .Select(x => language == "th" ? x.NameTh : x.NameEn)
                                  .SingleOrDefault();

            return title;
        }

        public string GetPronoun(int gender, string language = "en")
        {
            if (gender == 0) // 0 = undefined, 1 = male, 2 = female
            {
                switch (language)
                {
                    case "en":
                        return "They";
                    case "th":
                        return "";
                }
            }
            else if (gender == 1)
            {
                switch (language)
                {
                    case "en":
                        return "He";
                    case "th":
                        return "";
                }
            }
            else if (gender == 2)
            {
                switch (language)
                {
                    case "en":
                        return "She";
                    case "th":
                        return "";
                }
            }
            return "";
        }

        public string GetPossessive(int gender, string language = "en")
        {
            if (gender == 0) // 0 = undefined, 1 = male, 2 = female
            {
                switch (language)
                {
                    case "en":
                        return "their";
                    case "th":
                        return "";
                }
            }
            else if (gender == 1)
            {
                switch (language)
                {
                    case "en":
                        return "his";
                    case "th":
                        return "";
                }
            }
            else if (gender == 2)
            {
                switch (language)
                {
                    case "en":
                        return "her";
                    case "th":
                        return "";
                }
            }
            return "";
        }

        public StudentRequiredDocument GetStudentRequiredDocument(Student student)
        {
            var requiredDocument = new StudentRequiredDocument
            {
                StudentId = student.Id
            };

            var submittedDocuments = _db.StudentDocuments.Include(x => x.Document)
                                                         .Where(x => x.StudentId == student.Id)
                                                         .IgnoreQueryFilters()
                                                         .ToList();

            requiredDocument.StudentDocuments = submittedDocuments;
            return requiredDocument;
        }

        public List<StudentDocument> GetWaitingDocumentByStudentId(string code)
        {
            var requiredDocuments = _db.StudentDocuments.Include(x => x.Document)
                                                        .Include(x => x.Student)
                                                        .Where(x => x.DocumentStatus != "c"
                                                                    && x.IsRequired
                                                                    && x.Student.Code == code)
                                                        .ToList();

            return requiredDocuments;
        }

        public List<StudentDocument> GetStudentDocument(Guid studentId)
        {
            var documents = _db.StudentDocuments.Include(x => x.RequiredDocument)
                                                .Where(x => x.StudentId == studentId)
                                                .ToList();

            return documents;
        }

        public AcademicInformation GetAcademicInformationByStudentId(Guid studentId)
        {
            var student = _db.AcademicInformations.Include(x => x.AcademicLevel)
                                                  .SingleOrDefault(x => x.StudentId == studentId);
            return student;
        }

        public List<StudentExemptedExamScore> GetStudentExemptedExamScore(Guid studentId)
        {
            var examScores = _db.StudentExemptedExamScores.Include(x => x.ExemptedAdmissionExamination)
                                                          .Where(x => x.StudentId == studentId)
                                                          .ToList();
            return examScores;
        }

        public bool SaveStudentDocumentForAdmission(RegistrationApplicationViewModel model)
        {
            try
            {
                var deletedStudentDocuments = _db.StudentDocuments.Where(x => x.StudentId == model.StudentId);
                _db.StudentDocuments.RemoveRange(deletedStudentDocuments);
                var studentDocuments = new List<StudentDocument>();
                foreach (var item in model.StudentDocuments)
                {
                    var document = new StudentDocument();
                    document.StudentId = model.StudentId;
                    document.RequiredDocumentId = item.RequiredDocumentId;
                    document.DocumentId = item.DocumentId;
                    document.ImageUrl = item.ImageUrl;
                    document.IsRequired = true;
                    document.Amount = 1;
                    if (item.ImageUrl == null)
                    {
                        document.SubmittedAmount = 0;
                    }
                    else
                    {
                        document.SubmittedAmount = 1;
                    }
                    studentDocuments.Add(document);
                }
                _db.StudentDocuments.AddRange(studentDocuments);
                _db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SaveStudentDocument(StudentRequiredDocument model)
        {
            try
            {
                var deletedStudentDocuments = _db.StudentDocuments.Where(x => x.StudentId == model.StudentId);
                _db.StudentDocuments.RemoveRange(deletedStudentDocuments);

                foreach (var item in model.StudentDocuments)
                {
                    item.StudentId = model.StudentId;
                }
                _db.StudentDocuments.AddRange(model.StudentDocuments);
                _db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public List<Student> GetStudentFromCodeRanges(int fromCode, int toCode, long academicLevelId, long admissionId)
        {
            var students = _db.Students.Include(x => x.AdmissionInformation)
                                       .Include(x => x.AcademicInformation)
                                           .ThenInclude(x => x.AcademicLevel)
                                       .Include(x => x.AcademicInformation)
                                           .ThenInclude(x => x.Faculty)
                                       .Include(x => x.AcademicInformation)
                                           .ThenInclude(x => x.Department)
                                       .Include(x => x.AdmissionInformation)
                                           .ThenInclude(x => x.AdmissionRound)
                                       .Include(x => x.Title)
                                       .Where(x => (fromCode == 0
                                                    || (toCode == 0 ? x.CodeInt == fromCode : x.CodeInt >= fromCode))
                                                    && (toCode == 0
                                                        || (fromCode == 0 ? x.CodeInt == toCode : x.CodeInt <= toCode))
                                                    && (academicLevelId == 0
                                                        || x.AcademicInformation.AcademicLevelId == academicLevelId)
                                                    && (admissionId == 0
                                                        || x.AdmissionInformation.AdmissionRoundId == admissionId))
                                       .ToList();
            return students;
        }

        public List<StudentRegistrationCourseViewModel> GetStudentRegistrationCourseViewModel(Guid? studentId = null, string code = null)
        {
            var id = string.IsNullOrEmpty(code) ? studentId
                                                : _db.Students.AsNoTracking()
                                                              .SingleOrDefault(x => x.Code == code).Id;

            var registrationCourse = _db.RegistrationCourses.AsNoTracking()
                                                            .Include(x => x.Course)
                                                            .Include(x => x.Section)
                                                            .Include(x => x.Grade)
                                                            .Where(x => x.StudentId == id
                                                                        && (x.Status != "d" || x.Grade.Name.ToUpper() == "W")
                                                                        && x.Grade.Name.ToUpper() != "T"
                                                                        && !x.IsTransferCourse)
                                                            .ToList();

            var termIds = registrationCourse.Select(x => x.TermId)
                                            .Distinct().ToList();

            var terms = _db.Terms.AsNoTracking()
                                 .Include(x => x.TermType)
                                 .Where(x => termIds.Contains(x.Id))
                                 .ToList();

            var grades = _db.Grades.AsNoTracking()
                                   .ToList();

            var registrationCourses = (from course in registrationCourse
                                       group course by course.TermId into groupByTerm
                                       orderby groupByTerm.Key
                                       let term = terms.Single(x => x.Id == groupByTerm.Key)
                                       let totalGPACourses = groupByTerm.Where(x => x.IsGradePublished
                                                                                    && !x.IsStarCourse
                                                                                    && (x.Grade?.IsCalculateGPA ?? false)).ToList()
                                       let courseCredits = (from regisCourse in groupByTerm
                                                            join grade in grades on regisCourse.GradeId equals grade.Id
                                                            where !regisCourse.IsStarCourse
                                                            select new
                                                            {
                                                                regisCourse.IsWithDrawn,
                                                                regisCourse.Course.Credit,
                                                                regisCourse.Course.RegistrationCredit,
                                                                regisCourse.IsTransferCourse,
                                                                grade.IsCalculateCredit,
                                                                grade.IsCalculateGPA,
                                                                grade.IsCalculateRegistrationCredit,
                                                                grade.Weight,
                                                            }).ToList()
                                       select new StudentRegistrationCourseViewModel
                                       {
                                           TermId = groupByTerm.Key,
                                           Term = term.TermText,
                                           AcademicYear = term.AcademicYear,
                                           AcademicTerm = term.AcademicTerm,
                                           IsSummer = term.TermType?.NameEn?.ToLower()?.Contains("summer") ?? false,
                                           TotalCredit = courseCredits.Where(x => x.IsCalculateCredit).Sum(x => x.Credit),
                                           TotalRegistrationCredit = courseCredits.Where(x => !x.IsTransferCourse && x.IsCalculateRegistrationCredit).Sum(x => x.RegistrationCredit),
                                           TotalGPTS = courseCredits.Where(x => !x.IsTransferCourse && x.IsCalculateGPA).Sum(x => x.Credit * x.Weight ?? 0),
                                           // TotalCredit = (from regisCourse in groupByTerm
                                           //                 join grade in grades on regisCourse.GradeId equals grade.Id
                                           //                 where grade.IsCalculateCredit && !regisCourse.IsStarCourse
                                           //                 select regisCourse.Course.Credit).Sum(),
                                           // TotalRegistrationCreditFromCredit = (from regisCourse in groupByTerm
                                           //                                     join grade in grades on regisCourse.GradeId equals grade.Id
                                           //                                     where grade.IsCalculateCredit && !regisCourse.IsTransferCourse && !regisCourse.IsStarCourse
                                           //                                     select regisCourse.Course.Credit)
                                           //                                     .Sum(),
                                           // TotalRegistrationCredit = (from regisCourse in groupByTerm
                                           //                             join grade in grades on regisCourse.GradeId equals grade.Id
                                           //                             where grade.IsCalculateCredit && !regisCourse.IsTransferCourse && !regisCourse.IsStarCourse
                                           //                             select regisCourse.Course.RegistrationCredit)
                                           //                             .Sum(),
                                           RegistrationCourses = groupByTerm.ToList(),
                                           TermGPA = !totalGPACourses.Any() ? 0
                                                                                  : totalGPACourses.Sum(x => (x.Grade.Weight ?? 0) * x.Course.Credit) / totalGPACourses.Sum(x => x.Course.Credit)
                                       }).ToList();


            // var registrationCourses = _db.RegistrationCourses.Include(x => x.Term)
            //                                                     .ThenInclude(x => x.TermType)
            //                                                  .Include(x => x.Course)
            //                                                  .Include(x => x.Section)
            //                                                  .Include(x => x.Grade)
            //                                                  .Where(x => x.StudentId == id
            //                                                              && (x.Status == "a" || x.Status == "r" || x.Grade.Name.ToUpper() == "W")
            //                                                              && x.Grade.Name.ToUpper() != "T"
            //                                                              && !x.IsTransferCourse)
            //                                                  .GroupBy(x => x.TermId)
            //                                                  .Select(x => new StudentRegistrationCourseViewModel
            //                                                               {
            //                                                                   Term = x.FirstOrDefault().Term.TermText,
            //                                                                   TermId = x.First().Term.Id,
            //                                                                   AcademicTerm = x.First().Term.AcademicTerm,
            //                                                                   AcademicYear = x.First().Term.AcademicYear,
            //                                                                   IsSummer = x.First().Term.TermType.NameEn.ToLower().Contains("summer"),
            //                                                                   RegistrationCourses = x.Select(y => y).ToList(),
            //                                                                   TotalCredit = x.Where(y => y.Grade.IsCalculateCredit).Sum(y => y.Course.Credit),
            //                                                                   TotalRegistrationCreditFromCredit = x.Where(y => !y.IsTransferCourse 
            //                                                                                                                    && !y.IsStarCourse
            //                                                                                                                    && y.Grade.IsCalculateCredit)
            //                                                                                                        .Sum(y => y.Course.Credit),
            //                                                                   TotalRegistrationCredit = x.Where(y => !y.IsTransferCourse 
            //                                                                                                          && !y.IsStarCourse)
            //                                                                                              .Sum(y => y.Course.RegistrationCredit),
            //                                                                   TermGPA = x.Where(y => y.IsGradePublished 
            //                                                                                          && !y.IsStarCourse 
            //                                                                                          && y.Grade.IsCalculateGPA)
            //                                                                              .Sum(y => y.Course.Credit) == 0 ? 0 
            //                                                                                                              : x.Where(y => y.IsGradePublished 
            //                                                                                                                             && !y.IsStarCourse 
            //                                                                                                                             && y.Grade.IsCalculateGPA)
            //                                                                                                                 .Sum(y => (y.Grade.Weight ?? 0) * y.Course.Credit) / x.Where(y => y.IsGradePublished 
            //                                                                                                                                                                                   && !y.IsStarCourse 
            //                                                                                                                                                                                   && y.Grade.IsCalculateGPA)
            //                                                                                                                                                                       .Sum(y => y.Course.Credit)
            //                                                               })
            //                                                  .ToList();

            var registrationLogs = _db.RegistrationLogs.AsNoTracking()
                                                       .Where(x => x.StudentId == id)
                                                       .ToList();

            decimal cummCredit = 0;
            decimal cummCreditWeight = 0;
            decimal cumCreditRegis = 0;
            foreach (var item in registrationCourses)
            {
                // CALCULATE GPA
                var calculatedCourses = item.RegistrationCourses.Where(x => (x.Course?.IsCalculateCredit ?? false)
                                                                            && !x.IsStarCourse
                                                                            && (x.Grade == null ? false : x.Grade?.IsCalculateCredit ?? false))
                                                                .ToList();

                decimal totalCredit = calculatedCourses.Where(x => !x.IsWithDrawn).Sum(x => x.Course?.Credit ?? 0);
                item.TotalCredit = totalCredit;
                cummCredit += totalCredit;

                cumCreditRegis += item.TotalRegistrationCredit;
                item.CumulativeCreditRegis = cumCreditRegis;

                cummCreditWeight += calculatedCourses.Sum(x => (x.Course?.Credit ?? 0) * (x.Grade?.Weight ?? 0));
                item.CumulativeGPA = GetGPA(id.Value, item.TermId).GPA; //cummCredit == 0 ? 0 : cummCreditWeight / cummCredit;
                item.CumulativeCreditComp = GetGPA(id.Value, item.TermId).CreditComp; //cummCredit == 0 ? 0 : cummCreditWeight / cummCredit;
                item.CumulativeGTPS = GetGPA(id.Value, item.TermId).CumulativeGPTS;

                // REGISTRATION LOGS
                item.RegistrationLogs = registrationLogs.Where(x => x.TermId == item.TermId)
                                                        .OrderBy(x => x.CreatedAt)
                                                        .ToList();

                foreach (var log in item.RegistrationLogs)
                {
                    log.CreatedAt = _dateTimeProvider.ConvertFromUtcToSE(log.CreatedAt) ?? DateTime.UtcNow;
                    var user = new UserTimeStamp
                    {
                        CreatedBy = log.CreatedBy
                    };
                    log.CreatedByFullNameEn = log.Channel.ToLower() == "r" ? _userProvider.FillUserTimeStampFullName(user)?.CreatedByFullNameEn ?? "" : log.CreatedBy;

                    if (log.RegistrationModification != null)
                    {
                        if (log.RegistrationModification.NewCourse != null && log.RegistrationModification.NewCourse.Any())
                        {
                            log.NewCourse = String.Join(", ", log.RegistrationModification.NewCourse.Select(x => x.CourseString).ToList());
                        }
                        else
                        {
                            log.NewCourse = "-";
                        }

                        if (log.RegistrationModification.RetainedCourse != null && log.RegistrationModification.RetainedCourse.Any())
                        {
                            log.RetainedCourse = String.Join(", ", log.RegistrationModification.RetainedCourse.Select(x => x.CourseString).ToList());
                        }
                        else
                        {
                            log.RetainedCourse = "-";
                        }

                        if (log.RegistrationModification.DiscardedCourse != null && log.RegistrationModification.DiscardedCourse.Any())
                        {
                            log.DiscardedCourse = String.Join(", ", log.RegistrationModification.DiscardedCourse.Select(x => x.CourseString).ToList());
                        }
                        else
                        {
                            log.DiscardedCourse = "-";
                        }

                    }

                    if (log.RegistrationSummary != null && log.RegistrationSummary.Any())
                    {
                        log.Summary = String.Join(", ", log.RegistrationSummary.Select(x => x.CourseString).ToList());
                    }
                    else
                    {
                        log.Summary = "-";
                    }
                }
            }

            return registrationCourses;
        }

        public List<StudentRegistrationCourseViewModel> GetStudentRegistrationCourseTranferWithGradeViewModel(Guid? studentId = null, string code = null)
        {
            var id = string.IsNullOrEmpty(code) ? studentId : GetStudentByCode(code)?.Id;
            var creditGradeNotcalc = _db.RegistrationCourses.Where(x => x.StudentId == id
                                                                        && x.Status != "d"
                                                                        && !x.Grade.IsCalculateGPA
                                                                        && x.IsTransferCourse)
                                                            .Sum(x => x.Course.Credit);
            var registrationCourses = _db.RegistrationCourses.Include(x => x.Term)
                                                                .ThenInclude(x => x.TermType)
                                                             .Include(x => x.Course)
                                                             .Include(x => x.Section)
                                                             .Include(x => x.Grade)
                                                             .Where(x => x.StudentId == id
                                                                         && x.Grade.Name.ToUpper() != "T"
                                                                         && x.Status != "d"
                                                                         && x.IsTransferCourse)
                                                             .GroupBy(x => x.StudentId)
                                                             .Select(x => new StudentRegistrationCourseViewModel
                                                             {
                                                                 Term = x.FirstOrDefault().Term.TermText,
                                                                 TermId = x.First().Term.Id,
                                                                 RegistrationCourses = x.Select(y => y).ToList(),
                                                                 TotalCredit = x.Where(y => y.Grade.IsCalculateCredit).Sum(y => y.Course.Credit),
                                                                 TotalGPTS = x.Where(y => y.Grade.IsCalculateGPA).Sum(y => y.Course.Credit * y.Grade.Weight ?? 0),
                                                                 TotalRegistrationCredit = 0,
                                                                 TotalRegistrationCreditFromCredit = 0,
                                                                 TermGPA = x.Where(y => y.IsGradePublished
                                                                                        && y.Grade.IsCalculateGPA)
                                                                                         .Sum(y => y.Course.Credit) == 0 ? 0
                                                                                                                         : x.Where(y => y.IsGradePublished
                                                                                                                                        && y.Grade.IsCalculateGPA)
                                                                                                                            .Sum(y => (y.Grade.Weight ?? 0) * y.Course.Credit) / x.Where(y => y.IsGradePublished
                                                                                                                                                                                              && y.Grade.IsCalculateGPA)
                                                                                                                                                                                  .Sum(y => y.Course.Credit)
                                                             })
                                                             .ToList();

            var registrationLogs = _db.RegistrationLogs.Where(x => x.StudentId == id)
                                                       .ToList();

            decimal cummCredit = 0;
            decimal cummCreditWeight = 0;
            foreach (var item in registrationCourses)
            {
                item.RegistrationLogs = registrationLogs.Where(x => x.TermId == item.TermId)
                                                        .OrderBy(x => x.Sequence)
                                                        .ToList();

                foreach (var log in item.RegistrationLogs)
                {
                    log.CreatedAt = _dateTimeProvider.ConvertFromUtcToSE(log.CreatedAt) ?? DateTime.UtcNow;

                    var user = new UserTimeStamp
                    {
                        CreatedBy = log.CreatedBy
                    };
                    log.CreatedByFullNameEn = log.Channel.ToLower() == "r" ? _userProvider.FillUserTimeStampFullName(user)?.CreatedByFullNameEn ?? "" : log.CreatedBy;

                    if (log.RegistrationModification != null)
                    {
                        if (log.RegistrationModification.NewCourse != null && log.RegistrationModification.NewCourse.Any())
                        {
                            log.NewCourse = String.Join(", ", log.RegistrationModification.NewCourse.Select(x => x.CourseString).ToList());
                        }
                        else
                        {
                            log.NewCourse = "-";
                        }

                        if (log.RegistrationModification.RetainedCourse != null && log.RegistrationModification.RetainedCourse.Any())
                        {
                            log.RetainedCourse = String.Join(", ", log.RegistrationModification.RetainedCourse.Select(x => x.CourseString).ToList());
                        }
                        else
                        {
                            log.RetainedCourse = "-";
                        }

                        if (log.RegistrationModification.DiscardedCourse != null && log.RegistrationModification.DiscardedCourse.Any())
                        {
                            log.DiscardedCourse = String.Join(", ", log.RegistrationModification.DiscardedCourse.Select(x => x.CourseString).ToList());
                        }
                        else
                        {
                            log.DiscardedCourse = "-";
                        }

                    }

                    if (log.RegistrationSummary != null && log.RegistrationSummary.Any())
                    {
                        log.Summary = String.Join(", ", log.RegistrationSummary.Select(x => x.CourseString).ToList());
                    }
                    else
                    {
                        log.Summary = "-";
                    }
                }

                var calculatedCourses = item.RegistrationCourses.Where(x => (x.Course?.IsCalculateCredit ?? false)
                                                                            && (x.Grade == null ? false : x.Grade?.IsCalculateCredit ?? false))
                                                                .ToList();

                decimal totalCredit = calculatedCourses.Sum(x => x.Course?.Credit ?? 0);
                item.TotalCredit = totalCredit;
                cummCredit += totalCredit;
                cummCreditWeight += calculatedCourses.Sum(x => (x.Course?.Credit ?? 0) * (x.Grade?.Weight ?? 0));
                item.CumulativeGPA = item.TermGPA;
                item.CumulativeCreditComp = totalCredit + creditGradeNotcalc;
            }

            return registrationCourses;
        }

        public List<StudentRegistrationCourseViewModel> GetStudentRegistrationCourseTranferViewModel(Guid? studentId = null, string code = null)
        {
            var id = string.IsNullOrEmpty(code) ? studentId : GetStudentByCode(code)?.Id;
            var registrationCourses = _db.RegistrationCourses.Include(x => x.Term)
                                                                .ThenInclude(x => x.TermType)
                                                             .Include(x => x.Course)
                                                             .Include(x => x.Section)
                                                             .Include(x => x.Grade)
                                                             .Where(x => x.StudentId == id
                                                                         && x.Status != "d"
                                                                         && x.Grade.Name.ToUpper() == "T"
                                                                         && x.IsTransferCourse)
                                                             .GroupBy(x => x.StudentId)
                                                             .Select(x => new StudentRegistrationCourseViewModel
                                                             {
                                                                 RegistrationCourses = x.Select(y => y).ToList(),
                                                                 TotalCredit = x.Sum(y => y.Course.Credit),
                                                                 TotalRegistrationCredit = 0,
                                                             })
                                                             .ToList();

            return registrationCourses;
        }

        #region GPA
        public void UpdateGradeComp()
        {
            var students = _db.Students.Include(x => x.AcademicInformation).OrderByDescending(x => x.Code);
            var allStudentRegistrationCourses = (from registrationCourse in _db.RegistrationCourses
                                                 join course in _db.Courses on registrationCourse.CourseId equals course.Id
                                                 join grade in _db.Grades on registrationCourse.GradeId equals grade.Id
                                                 join term in _db.Terms on registrationCourse.TermId equals term.Id
                                                 where students.Select(y => y.Id).Contains(registrationCourse.StudentId)
                                                 && grade.IsCalculateGPA
                                                 && course.IsCalculateCredit
                                                 && registrationCourse.IsGradePublished
                                                 && !registrationCourse.IsStarCourse
                                                 select new
                                                 {
                                                     RegistrationCourse = registrationCourse,
                                                     Course = course,
                                                     Grade = grade,
                                                     Term = term
                                                 }).ToList();
            var allCurriculumDependencies = _db.CurriculumDependencies.Where(x => x.DependencyType == "Equivalence").ToList();
            var allCourseEquivalents = (from courseEquivalent in _db.CourseEquivalents
                                        join course1 in _db.Courses on courseEquivalent.CourseId equals course1.Id
                                        join course2 in _db.Courses on courseEquivalent.EquilaventCourseId equals course2.Id
                                        select new
                                        {
                                            CourseEquivalent = courseEquivalent,
                                            Course = course1,
                                            EquilaventCourse = course2
                                        }).ToList();
            foreach (var student in students)
            {
                if (student.AcademicInformation == null)
                {
                    continue;
                }
                var studentRegistrationCourses = allStudentRegistrationCourses.Where(x => x.RegistrationCourse.StudentId == student.Id).ToList();
                foreach (var item in studentRegistrationCourses)
                {
                    item.Course.EquivalentCourseCode = item.Course.Code;
                }
                var curriculumDependencies = allCurriculumDependencies.Where(x => x.CurriculumVersionId == student.AcademicInformation.CurriculumVersionId).ToList();//  _db.CurriculumDependencies.Where(x => x.DependencyType == "Equivalence" && x.CurriculumVersionId == student.AcademicInformation.CurriculumVersionId).ToList();
                var courseEquivalents = (from courseEquivalent in allCourseEquivalents
                                         where curriculumDependencies.Select(x => x.DependencyId).Contains(courseEquivalent.CourseEquivalent.Id)
                                         && (studentRegistrationCourses.Select(x => x.RegistrationCourse.CourseId).Contains(courseEquivalent.CourseEquivalent.CourseId)
                                             || studentRegistrationCourses.Select(x => x.RegistrationCourse.CourseId).Contains(courseEquivalent.CourseEquivalent.EquilaventCourseId))
                                         select new
                                         {
                                             courseEquivalent.CourseEquivalent,
                                             courseEquivalent.Course,
                                             courseEquivalent.EquilaventCourse
                                         }).ToList();
                if (courseEquivalents.Any())
                {
                    foreach (var registrationCourse in studentRegistrationCourses)
                    {
                        var equivalentCourses = (from courseEquivalent in courseEquivalents
                                                 where courseEquivalent.CourseEquivalent.EffectivedAt <= registrationCourse.RegistrationCourse.GradePublishedAt
                                                 && (courseEquivalent.CourseEquivalent.EndedAt == null || courseEquivalent.CourseEquivalent.EndedAt >= registrationCourse.RegistrationCourse.GradePublishedAt)
                                                 && (courseEquivalent.CourseEquivalent.CourseId == registrationCourse.RegistrationCourse.CourseId
                                                     || courseEquivalent.CourseEquivalent.EquilaventCourseId == registrationCourse.RegistrationCourse.CourseId)
                                                 select String.Compare(courseEquivalent.Course.Code, courseEquivalent.EquilaventCourse.Code) <= 0 ? courseEquivalent.EquilaventCourse.Code : courseEquivalent.Course.Code)
                                                .ToList();
                        if (equivalentCourses.Any())
                        {
                            registrationCourse.Course.EquivalentCourseCode = equivalentCourses.Max();
                        }
                    }
                }

                var registrationCourses = (from studentRegistrationCourse in studentRegistrationCourses
                                           group new { studentRegistrationCourse.RegistrationCourse, studentRegistrationCourse.Course, studentRegistrationCourse.Grade, studentRegistrationCourse.Term }
                                               by new { studentRegistrationCourse.Course.EquivalentCourseCode } into grp
                                           select new
                                           {
                                               CourseCode = grp.Key,
                                               RecordToCalculateGrade = grp.OrderByDescending(x => x.Term.AcademicYear).ThenByDescending(x => x.Term.AcademicTerm).FirstOrDefault()
                                           }).ToList();
                if (registrationCourses.Any())
                {
                    var creditComp = registrationCourses.Sum(x => x.RecordToCalculateGrade.Course.RegistrationCredit);
                    student.AcademicInformation.CreditComp = creditComp;
                }
            }
            _db.SaveChanges();
        }

        public StudentGPAViewModel GetGPA(Guid studentId, long termId = 0)
        {
            var searchTerm = _db.Terms.AsNoTracking()
                                      .SingleOrDefault(x => x.Id == termId);

            var student = _db.Students.IgnoreQueryFilters()
                                      .AsNoTracking()
                                      .Include(x => x.AcademicInformation)
                                      .SingleOrDefault(x => x.Id == studentId);

            var studentRegistrationCourses = (from registrationCourse in _db.RegistrationCourses
                                              join course in _db.Courses on registrationCourse.CourseId equals course.Id
                                              join grade in _db.Grades on registrationCourse.GradeId equals grade.Id
                                              join term in _db.Terms on registrationCourse.TermId equals term.Id
                                              where registrationCourse.StudentId == studentId
                                                    && registrationCourse.Status != "d"
                                                    && grade.IsCalculateGPA
                                                    && course.IsCalculateCredit
                                                    && registrationCourse.IsGradePublished
                                                    && !registrationCourse.IsStarCourse
                                                    && (termId == 0 || term.AcademicYear < searchTerm.AcademicYear
                                                        || (term.AcademicYear == searchTerm.AcademicYear && term.AcademicTerm <= searchTerm.AcademicTerm)
                                                        || registrationCourse.IsTransferCourse)
                                              select new
                                              {
                                                  RegistrationCourse = registrationCourse,
                                                  Course = course,
                                                  Grade = grade,
                                                  Term = term
                                              })
                                                .AsNoTracking()
                                                .ToList();

            // foreach (var item in studentRegistrationCourses)
            // {
            //     item.Course.EquivalentCourseCode = item.Course.Code;
            // }

            // COURSE EQUIVALENCES
            var curriculumDependencieIds = _db.CurriculumDependencies.AsNoTracking()
                                                                     .Where(x => x.DependencyType == "Equivalence"
                                                                                 && x.CurriculumVersionId == student.AcademicInformation.CurriculumVersionId)
                                                                     .Select(x => x.Id)
                                                                     .ToList();

            var courseIds = studentRegistrationCourses.Select(x => x.RegistrationCourse.CourseId);
            var courseEquivalents = (from courseEquivalent in _db.CourseEquivalents
                                     join course1 in _db.Courses on courseEquivalent.CourseId equals course1.Id
                                     join course2 in _db.Courses on courseEquivalent.EquilaventCourseId equals course2.Id
                                     where curriculumDependencieIds.Contains(courseEquivalent.Id)
                                           && (courseIds.Contains(courseEquivalent.CourseId)
                                               || courseIds.Contains(courseEquivalent.EquilaventCourseId))
                                     select new
                                     {
                                         courseEquivalent,
                                         course1,
                                         course2
                                     }).ToList();

            foreach (var registrationCourse in studentRegistrationCourses)
            {
                registrationCourse.Course.EquivalentCourseCode = registrationCourse.Course.Code;

                var equivalentCourses = (from courseEquivalent in courseEquivalents
                                         where courseEquivalent.courseEquivalent.EffectivedAt <= registrationCourse.RegistrationCourse.GradePublishedAt
                                               && (courseEquivalent.courseEquivalent.EndedAt == null
                                                   || courseEquivalent.courseEquivalent.EndedAt >= registrationCourse.RegistrationCourse.GradePublishedAt)
                                               && (courseEquivalent.courseEquivalent.CourseId == registrationCourse.RegistrationCourse.CourseId
                                                   || courseEquivalent.courseEquivalent.EquilaventCourseId == registrationCourse.RegistrationCourse.CourseId)
                                         select String.Compare(courseEquivalent.course1.Code, courseEquivalent.course2.Code) <= 0 ? courseEquivalent.course2.Code : courseEquivalent.course1.Code)
                                        .ToList();
                if (equivalentCourses.Any())
                {
                    registrationCourse.Course.EquivalentCourseCode = equivalentCourses.Max();
                }
            }

            var registrationCourses = (from studentRegistrationCourse in studentRegistrationCourses
                                       group new { studentRegistrationCourse.RegistrationCourse, studentRegistrationCourse.Course, studentRegistrationCourse.Grade, studentRegistrationCourse.Term }
                                           by new { studentRegistrationCourse.Course.EquivalentCourseCode } into grp
                                       select new
                                       {
                                           CourseCode = grp.Key,
                                           RecordToCalculateGrade = grp.OrderByDescending(x => x.Term.AcademicYear).ThenByDescending(x => x.Term.AcademicTerm).FirstOrDefault()
                                       }).ToList();

            // CALCULATE ONLY CREDIT NOT GPA eg. GRADE S / O
            var creditGradeNotcalc = _db.RegistrationCourses.AsNoTracking()
                                                            .Where(x => x.StudentId == studentId
                                                                        && !x.IsTransferCourse
                                                                        && x.Grade.IsCalculateCredit
                                                                        && !x.Grade.IsCalculateGPA
                                                                        && x.Status != "d"
                                                                        && (termId == 0 || x.Term.AcademicYear < searchTerm.AcademicYear
                                                                            || (x.Term.AcademicYear == searchTerm.AcademicYear && x.Term.AcademicTerm <= searchTerm.AcademicTerm)))
                                                            .Sum(x => x.Course.Credit);
            var creditGradeNotcalcTranfer = _db.RegistrationCourses.Where(x => x.StudentId == studentId
                                                                               && x.Status != "d"
                                                                               && !x.Grade.IsCalculateGPA
                                                                               && x.IsTransferCourse)
                                                                   .Sum(x => x.Course.Credit);

            if (registrationCourses.Any())
            {
                var gpa = registrationCourses.Sum(x => x.RecordToCalculateGrade.Course.Credit * x.RecordToCalculateGrade.Grade.Weight ?? 0) / registrationCourses.Sum(x => x.RecordToCalculateGrade.Course.Credit);
                var creditComp = registrationCourses.Sum(x => x.RecordToCalculateGrade.Course.Credit);
                var cumGPTS = registrationCourses.Sum(x => x.RecordToCalculateGrade.Course.Credit * x.RecordToCalculateGrade.Grade.Weight ?? 0);
                return new StudentGPAViewModel
                {
                    GPA = gpa,
                    CreditComp = creditComp + creditGradeNotcalc + creditGradeNotcalcTranfer,
                    CumulativeGPTS = cumGPTS
                };
            }
            return new StudentGPAViewModel
            {
                GPA = 0,
                CreditComp = 0 + creditGradeNotcalc + creditGradeNotcalcTranfer
            };
        }

        public int GetRegistrationCreditbyStudentId(Guid studentId)
        {
            var registrationCredit = _db.RegistrationCourses.Where(x => x.StudentId == studentId
                                                                        && x.Status != "d"
                                                                        && x.IsPaid
                                                                        && !x.IsTransferCourse
                                                                        && !x.IsStarCourse
                                                                        && x.IsActive)
                                                            .Sum(x => x.Course.RegistrationCredit);

            return registrationCredit;
        }

        public int GetCreditTransferbyStudentId(Guid studentId)
        {
            var TranferCredit = _db.RegistrationCourses.Where(x => x.StudentId == studentId
                                                                        && x.Status != "d"
                                                                        && x.IsTransferCourse
                                                                        && x.IsGradePublished
                                                                        && x.IsActive)
                                                            .Sum(x => x.Course.Credit);

            return TranferCredit;
        }

        public void UpdateCGPA(Guid studentId)
        {
            var academicInfo = _db.AcademicInformations.SingleOrDefault(x => x.StudentId == studentId);
            var currentTerm = _academicProvider.GetCurrentTerm(academicInfo.AcademicLevelId);
            var gpa = GetGPA(studentId, currentTerm.Id);
            academicInfo.GPA = gpa.GPA;
            academicInfo.CreditComp = gpa.CreditComp;
            academicInfo.CreditEarned = GetRegistrationCreditbyStudentId(studentId);
            academicInfo.CreditTransfer = GetCreditTransferbyStudentId(studentId);
            academicInfo.IsHasGradeUpdate = false;
            _db.SaveChanges();
        }

        public void UpdateTermGrade(Guid studentId, long termId)
        {

            // NOT USE FOR NOW

            // var termGPA = GetGPA(studentId, termId);
            // var studentGrade = _db.StudentGrades.SingleOrDefault(x => x.StudentId == studentId && x.TermId == termId);
            // if (studentGrade == null)
            // {
            //     studentGrade = new StudentGrade() 
            //     {
            //         StudentId = studentId,
            //         TermId = termId,
            //         GPA = termGPA
            //     };
            //     _db.StudentGrades.Add(studentGrade);
            // }
            // else 
            // {
            //     studentGrade.GPA = termGPA;
            // }
            // _db.SaveChanges();
        }
        #endregion

        public bool IsStudentExtended(Guid? studentId = null, string code = null)
        {
            var id = string.IsNullOrEmpty(code) ? studentId : GetStudentByCode(code)?.Id;
            var isStudentExtended = _db.ExtendedStudents.Any(x => x.StudentId == id);
            return isStudentExtended;
        }

        #region POST
        public string SaveStudentContact(Student student)
        {
            var modelToUpdate = _db.Students.SingleOrDefault(x => x.Id == student.Id);
            try
            {
                modelToUpdate.Email = student.Email;
                modelToUpdate.Email2 = student.Email2;
                modelToUpdate.PersonalEmail = student.PersonalEmail;
                modelToUpdate.Facebook = student.Facebook;
                modelToUpdate.Line = student.Line;
                modelToUpdate.TelephoneNumber1 = student.TelephoneNumber1;
                modelToUpdate.TelephoneNumber2 = student.TelephoneNumber2;
                modelToUpdate.OtherContact = student.OtherContact;

                _db.SaveChanges();
                return "";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public string GetLastStudentStatus(Guid id, string status = null)
        {
            if (string.IsNullOrEmpty(status))
            {
                var lastStudentStatus = _db.StudentStatusLogs.Where(x => x.StudentId == id)
                                                             .OrderByDescending(x => x.UpdatedAt)
                                                             .Skip(1)
                                                             .FirstOrDefault();
                return lastStudentStatus?.StudentStatus ?? "s";
            }
            else
            {
                var studentStatusLogs = _db.StudentStatusLogs.Where(x => x.StudentId == id)
                                                             .OrderByDescending(x => x.UpdatedAt)
                                                             .ToList();

                var latestStatus = studentStatusLogs.FindIndex(x => x.StudentStatus == status);
                if (latestStatus == -1) // Not found
                {
                    return "s";
                }
                else
                {
                    return studentStatusLogs[latestStatus - 1]?.StudentStatus ?? "s";
                }
            }
        }

        public StudentProbation GetStudentProbationById(long id)
        {
            var model = _db.StudentProbations.Include(x => x.Student)
                                             .Include(x => x.Term)
                                             .Include(x => x.Probation)
                                             .Include(x => x.Retire)
                                             .IgnoreQueryFilters()
                                             .SingleOrDefault(x => x.Id == id);
            return model;
        }

        public StudentProbationViewModel GetStudentProbation(Criteria criteria)
        {
            var model = new StudentProbationViewModel();

            var students = (from student in _db.Students
                            join academic in _db.AcademicInformations on student.Id equals academic.StudentId
                            join department in _db.Departments on academic.DepartmentId equals department.Id
                            join version in _db.CurriculumVersions on academic.CurriculumVersionId equals version.Id into versions
                            from version in versions.DefaultIfEmpty()
                            join advisor in _db.Instructors on academic.AdvisorId equals advisor.Id into advisors
                            from advisor in advisors.DefaultIfEmpty()
                            where academic.AcademicLevelId == criteria.AcademicLevelId
                                  && (criteria.FacultyId == 0
                                      || academic.FacultyId == criteria.FacultyId)
                                  && (criteria.DepartmentId == 0
                                      || academic.DepartmentId == criteria.DepartmentId)
                                  && (string.IsNullOrEmpty(criteria.Status)
                                      || student.StudentStatus == criteria.Status)
                                  && ((criteria.GPAFrom ?? 0) == 0
                                      || academic.GPA >= criteria.GPAFrom)
                                  && ((criteria.GPATo ?? 0) == 0
                                      || academic.GPA <= criteria.GPATo)
                                  && ((criteria.StartStudentBatch ?? 0) == 0
                                      || academic.Batch >= criteria.StartStudentBatch)
                                  && ((criteria.EndStudentBatch ?? 0) == 0
                                      || academic.Batch <= criteria.EndStudentBatch)
                            select new StudentProbationDetail
                            {
                                StudentId = student.Id,
                                StudentCode = student.Code,
                                StudentTitle = student.Title.NameEn,
                                StudentFirstName = student.FirstNameEn,
                                StudentMidName = student.MidNameEn,
                                StudentLastName = student.LastNameEn,
                                DepartmentCode = department == null ? "N/A" : department.Code,
                                CurriculumVersionNameEn = version == null ? "N/A" : version.NameEn,
                                AdvisorTitle = advisor.Title.NameEn,
                                AdvisorFirstName = advisor.FirstNameEn,
                                AdvisorLastName = advisor.LastNameEn,
                                StudentGPA = academic.GPA,
                            }).AsNoTracking()
                                     .ToList();

            // GET TERMS
            var startTerm = _db.Terms.AsNoTracking()
                                     .SingleOrDefault(x => x.Id == criteria.StartTermId);
            var endTerm = _db.Terms.AsNoTracking()
                                   .SingleOrDefault(x => x.Id == criteria.EndTermId);
            var terms = _db.Terms.Where(x => x.AcademicLevelId == criteria.AcademicLevelId
                                             && (startTerm == null
                                                 || string.Compare(x.TermCompare, startTerm.TermCompare) >= 0)
                                             && (endTerm == null
                                                 || string.Compare(x.TermCompare, endTerm.TermCompare) <= 0))
                                 .AsNoTracking()
                                 .ToList();
            model.Terms = terms;

            var studentIds = students.Select(x => x.StudentId).ToList();
            var termIds = terms.Select(x => x.Id).ToList();
            var studentProbationDetails = new List<StudentProbationDetail>();
            // GET REGISTRATION COURSES
            var registrationCourses = _db.RegistrationCourses.Include(x => x.Grade)
                                                             .Include(x => x.Course)
                                                             .AsNoTracking()
                                                             .Where(x => studentIds.Contains(x.StudentId)
                                                                         && termIds.Contains(x.TermId)
                                                                         && (x.Course.Credit > 0 || x.Course.RegistrationCredit > 0)
                                                                         && x.Status != "d")
                                                             .ToList();

            // GET STUDENT PROBATIONS

            var studentProbations = _db.StudentProbations.Include(x => x.Probation)
                                                         .AsNoTracking()
                                                         .Where(x => studentIds.Contains(x.StudentId)
                                                                     && termIds.Contains(x.TermId))
                                                         .ToList();

            foreach (var student in students)
            {
                var courseByStudents = registrationCourses.Where(x => x.StudentId == student.StudentId)
                                                       .ToList();

                if (courseByStudents != null && courseByStudents.Any())
                {
                    var detail = new StudentProbationDetail
                    {
                        StudentId = student.StudentId,
                        StudentCode = student.StudentCode,
                        StudentTitle = student.StudentTitle,
                        StudentFirstName = student.StudentFirstName,
                        StudentMidName = student.StudentMidName,
                        StudentLastName = student.StudentLastName,
                        DepartmentCode = student.DepartmentCode,
                        CurriculumVersionNameEn = student.CurriculumVersionNameEn,
                        AdvisorTitle = student.AdvisorTitle,
                        AdvisorFirstName = student.AdvisorFirstName,
                        AdvisorLastName = student.AdvisorLastName,
                        StudentGPA = student.StudentGPA,
                    };

                    detail.TermGPAs = new List<StudentProbationTermGPA>();

                    foreach (var term in terms)
                    {
                        // PROBATION 
                        bool IsProbationTerm = false;
                        var probationTerm = studentProbations.Where(x => x.StudentId == student.StudentId
                                                                         && x.TermId == term.Id)
                                                             .FirstOrDefault();

                        string probationText = string.Empty;
                        if (probationTerm != null
                            && probationTerm.ProbationId.HasValue
                            && probationTerm.ProbationId > 0)
                        {
                            probationText = probationTerm.Probation.Name;
                            IsProbationTerm = true;
                        }
                        else if (probationTerm != null
                                 && probationTerm.RetireId.HasValue
                                 && probationTerm.RetireId > 0)
                        {
                            probationText = "Retire";
                            IsProbationTerm = true;
                        }

                        // CALCULATE GPA
                        var courseByTerms = courseByStudents.Where(x => x.TermId == term.Id);
                        string displayText = string.Empty;
                        decimal? gpa = null;

                        if (courseByTerms.All(x => x.IsPaid))
                        {
                            if (courseByTerms.Any(x => x.GradeId == null))
                            {
                                displayText = "X";
                            }
                            else
                            {
                                var calculatedCourses = courseByTerms.Where(x => x.Grade.IsCalculateGPA
                                                                                 && x.Course.Credit > 0)
                                                                     .ToList();

                                var haveCreditRegistration = courseByTerms.Any(x => x.Grade.IsCalculateRegistrationCredit);

                                if (calculatedCourses.Count() < 1 && haveCreditRegistration)
                                {
                                    //displayText = "0.00";

                                    var grades = courseByTerms.Select(x => x.GradeName)
                                                             .Distinct()
                                                             .ToList();

                                    decimal termCredit = calculatedCourses.Sum(y => y.Course.Credit);
                                    gpa = GetGPA(detail.StudentId, term.Id).GPA;
                                    displayText = gpa?.ToString(StringFormat.TwoDecimal);

                                    displayText += " | " + string.Join(", ", grades);
                                }
                                else if (calculatedCourses.Count() < 1)
                                {
                                    displayText = "";
                                }
                                else if (calculatedCourses.All(x => x.Grade.Weight == 0))
                                {
                                    var grades = courseByTerms.Select(x => x.GradeName)
                                                              .Distinct()
                                                              .ToList();

                                    decimal termCredit = calculatedCourses.Sum(y => y.Course.Credit);
                                    // gpa = termCredit == 0 ? 0 : calculatedCourses.Sum(y => y.Course.Credit * y.Grade.Weight) / termCredit;
                                    gpa = termCredit == 0 ? 0 : GetGPA(detail.StudentId, term.Id).GPA;
                                    displayText = gpa?.ToString(StringFormat.TwoDecimal);

                                    displayText += " | " + string.Join(", ", grades);
                                }
                                else
                                {
                                    decimal termCredit = calculatedCourses.Sum(y => y.Course.Credit);
                                    // gpa = termCredit == 0 ? 0 : calculatedCourses.Sum(y => y.Course.Credit * y.Grade.Weight) / termCredit;
                                    gpa = termCredit == 0 ? 0 : GetGPA(detail.StudentId, term.Id).GPA;
                                    displayText = gpa?.ToString(StringFormat.TwoDecimal);
                                }
                            }
                        }
                        else
                        {
                            displayText = "Unpaid";
                        }

                        if (IsProbationTerm)
                        {
                            displayText += $" ({probationText})";
                        }

                        var termGPA = new StudentProbationTermGPA
                        {
                            AcademicYear = term.AcademicYear,
                            AcademicTerm = term.AcademicTerm,
                            GPA = gpa,
                            DisplayText = displayText,
                            IsProbation = IsProbationTerm
                        };

                        detail.TermGPAs.Add(termGPA);
                    }

                    studentProbationDetails.Add(detail);
                }
            }

            switch (criteria.SortBy)
            {
                case "s":
                    model.Students = studentProbationDetails.OrderBy(x => x.StudentCode)
                                                            .ToList();
                    break;
                case "m":
                    model.Students = studentProbationDetails.OrderBy(x => x.CurriculumVersionNameEn)
                                                            .ToList();
                    break;
                case "g":
                    model.Students = studentProbationDetails.OrderBy(x => x.StudentGPA)
                                                            .ToList();
                    break;
                case "a":
                    model.Students = studentProbationDetails.OrderBy(x => x.AdvisorName)
                                                            .ToList();
                    break;
                default:
                    model.Students = studentProbationDetails.OrderBy(x => x.StudentCode)
                                                            .ToList();
                    break;
            }

            return model;
        }

        public List<StudentProbation> GetCurrentStudentProbation(Guid studentId, long currentTermId)
        {
            var studentProbations = _db.StudentProbations.Where(x => x.StudentId == studentId
                                                                     && x.TermId == currentTermId)
                                                         .ToList();

            return studentProbations;
        }

        public DismissStudentViewModel GetDismissTermAndGrade(string code)
        {
            var model = new DismissStudentViewModel();
            var studentInformation = GetStudentByCode(code);
            var dismissStudentGPAs = (from student in _db.Students
                                      join registrationCourse in _db.RegistrationCourses on student.Id equals registrationCourse.StudentId
                                      join term in _db.Terms on registrationCourse.TermId equals term.Id
                                      join course in _db.Courses on registrationCourse.CourseId equals course.Id
                                      join grade in _db.Grades on registrationCourse.GradeId equals grade.Id into gradeTmp
                                      from grade in gradeTmp.DefaultIfEmpty()
                                      where (grade == null || grade.IsCalculateGPA)
                                            && course.Credit > 0
                                            && student.Code == code
                                      group new { term, course, grade, student }
                                      by new { student.Id, TermId = term.Id } into grp
                                      select new
                                      {
                                          StudentId = grp.Key.Id,
                                          TermId = grp.Key.TermId,
                                          AcademicYear = grp.FirstOrDefault().term.AcademicYear,
                                          AcademicTerm = grp.FirstOrDefault().term.AcademicTerm,
                                          TermGPA = grp.All(x => x.grade == null) ? -1 : grp.Where(x => x.grade != null).Sum(x => x.course.Credit * x.grade.Weight) / grp.Sum(x => x.course.Credit)
                                      }).ToList();

            //Find year
            var terms = (from term in _db.Terms
                         where term.AcademicLevelId == studentInformation.AcademicInformation.AcademicLevelId
                         && dismissStudentGPAs.Select(x => x.AcademicYear).Contains(term.AcademicYear)
                         group term by term.AcademicYear into grp
                         select new DismissAcademicYearAndTerm
                         {
                             AcademicYear = grp.Key,
                             AcademicYearAndTermDetails = grp.Select(x => new DismissAcademicYearAndTermDetail {
                                 AcademicTerm = x.AcademicTerm,
                                 TermId = x.Id
                             }).OrderBy(x => x.AcademicTerm)
                                  .ToList()
                         }).OrderBy(x => x.AcademicYear)
                           .ToList();

            model.TermGPAs = new List<StudentDismissTermGPA>();
            foreach (var termYear in terms)
            {
                foreach (var term in termYear.AcademicYearAndTermDetails)
                {
                    var termGPA = new StudentDismissTermGPA();
                    termGPA.AcademicYear = termYear.AcademicYear;
                    termGPA.AcademicTerm = term.AcademicTerm;
                    var gpa = dismissStudentGPAs.SingleOrDefault(x => x.TermId == term.TermId);
                    if (gpa != null)
                    {
                        termGPA.GPA = gpa.TermGPA;
                    }

                    model.TermGPAs.Add(termGPA);
                }
            }

            model.Terms = terms;
            return model;
        }

        public DismissStudent FindDismissStudent(long Id)
        {
            var dismissStudent = _db.DismissStudents.Include(x => x.Student)
                                                    .Include(x => x.Probation)
                                                    .Include(x => x.Term)
                                                    .SingleOrDefault(x => x.Id == Id);
            return dismissStudent;
        }

        public bool UpdateDismissStudent(DismissStudentViewModel model, Student student)
        {
            try
            {
                var dismissStudent = new DismissStudent
                {
                    StudentId = model.StudentId,
                    TermId = model.TermId,
                    ProbationId = model.ProbationId,
                    Remark = model.Remark
                };

                var studentStatus = new StudentStatusLog
                {
                    StudentId = model.StudentId,
                    TermId = model.TermId,
                    Source = "Dismiss",
                    StudentStatus = "dm",
                    Remark = model.Remark
                };

                student.StudentStatus = "dm";
                student.IsActive = IsActiveFromStudentStatus(student.StudentStatus);
                _db.DismissStudents.Add(dismissStudent);
                _db.StudentStatusLogs.Add(studentStatus);
                _db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateImageUrl(Guid studentId, string imageUrl)
        {
            var student = _db.Students.SingleOrDefault(x => x.Id == studentId);
            if (student == null)
            {
                return false;
            }
            else
            {
                student.ProfileImageURL = imageUrl;
                _db.SaveChanges();
                return true;
            }
        }

        public CurriculumInformation GetCurrentCurriculum(Guid studentId)
        {
            var curriculum = _db.CurriculumInformations.Include(x => x.CurriculumVersion)
                                                           .ThenInclude(x => x.Curriculum)
                                                       .FirstOrDefault(x => x.StudentId == studentId && x.IsActive);
            return curriculum;
        }

        public static bool IsActiveFromStudentStatus(string studentStatus)
        {
            return studentStatus == "s"
                       || studentStatus == "ex"
                       || studentStatus == "la"
                       || studentStatus == "a";
        }

        public bool SaveStudentStatusLog(Guid studentId, long currentTermId, string source, string remark, string status, DateTime? effectiveDate = null)
        {
            try
            {
                _db.StudentStatusLogs.Add(new StudentStatusLog
                                          {
                                              StudentId = studentId,
                                              TermId = currentTermId,
                                              Source = source,
                                              EffectiveAt = effectiveDate,
                                              Remark = remark,
                                              StudentStatus = status
                                          });
                
                _db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        public bool SaveDocumentStudentByDocumentGroup(Guid studentId, long documentGroupId)
        {
            try
            {
                var deletedStudentDocuments = _db.StudentDocuments.Where(x => x.StudentId == studentId
                                                                              && x.RequiredDocumentId != null)
                                                                  .ToList();

                _db.StudentDocuments.RemoveRange(deletedStudentDocuments);

                var newDocuments = _db.RequiredDocuments.Where(x => x.AdmissionDocumentGroupId == documentGroupId)
                                                        .Select(x => new StudentDocument
                                                                     {
                                                                         StudentId = studentId,
                                                                         DocumentId = x.DocumentId,
                                                                         RequiredDocumentId = x.Id,
                                                                         Amount = x.Amount
                                                                     })
                                                        .ToList();

                _db.StudentDocuments.AddRange(newDocuments);
                _db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public bool UpdateStudentExemptedExamScore(List<StudentExemptedExamScore> scores, Guid studentId)
        {
            var previousScores = GetStudentExemptedExamScore(studentId);
            scores = scores.Where(x => x.ExemptedExaminationId != 0)
                           .ToList();
            try
            {
                _db.StudentExemptedExamScores.RemoveRange(previousScores);
                _db.StudentExemptedExamScores.AddRange(scores);
                _db.SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public Student SaveReenterStudent(Student student, Student reenterStudent, long termId, string newCode, string type, string reason, out string errorMessage)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    // update previous code
                    student.StudentStatus = type;
                    student.IsActive = IsActiveFromStudentStatus(student.StudentStatus);
                    _db.SaveChanges();
                    var source = type == "re" ? SaveStatusSouces.REENTER : SaveStatusSouces.READMISSION;
                    var saveStudentLog = SaveStudentStatusLog(student.Id, termId, source.GetDisplayName(), reason, type);
                    if (!saveStudentLog)
                    {
                        transaction.Rollback();
                        errorMessage = Message.UnableToSaveStudentStatusLog;
                        return null;
                    }

                    // add new reentered code
                    reenterStudent.Code = newCode;
                    reenterStudent.FirstNameTh = student.FirstNameTh;
                    reenterStudent.MidNameTh = student.MidNameTh;
                    reenterStudent.LastNameTh = student.LastNameTh;
                    reenterStudent.StudentStatus = "s";
                    reenterStudent.RaceId = student.RaceId;
                    reenterStudent.ReligionId = student.ReligionId;
                    reenterStudent.BirthProvinceId = student.BirthProvinceId;
                    reenterStudent.BirthStateId = student.BirthStateId;
                    reenterStudent.BirthCityId = student.BirthCityId;
                    reenterStudent.BirthCountryId = student.BirthCountryId;
                    reenterStudent.NationalityId = student.NationalityId;
                    reenterStudent.CitizenNumber = student.CitizenNumber;
                    reenterStudent.Passport = student.Passport;
                    reenterStudent.PassportIssuedAt = student.PassportIssuedAt;
                    reenterStudent.PassportExpiredAt = student.PassportExpiredAt;
                    reenterStudent.BankBranchId = student.BankBranchId;
                    reenterStudent.BankAccount = student.BankAccount;
                    reenterStudent.AccountUpdatedAt = student.AccountUpdatedAt;
                    reenterStudent.Facebook = student.Facebook;
                    reenterStudent.Line = student.Line;
                    reenterStudent.OtherContact = student.OtherContact;
                    reenterStudent.IdCardCreatedDate = student.IdCardCreatedDate;
                    reenterStudent.IdCardExpiredDate = student.IdCardExpiredDate;
                    reenterStudent.RegistrationStatusId = student.RegistrationStatusId;
                    reenterStudent.MaritalStatus = student.MaritalStatus;
                    reenterStudent.DeformationId = student.DeformationId;
                    reenterStudent.DisabledBookCode = student.DisabledBookCode;
                    reenterStudent.DisableBookIssuedAt = student.DisableBookIssuedAt;
                    reenterStudent.DisableBookExpiredAt = student.DisableBookExpiredAt;
                    reenterStudent.LivingStatus = student.LivingStatus;
                    reenterStudent.Talent = student.Talent;
                    reenterStudent.ProfileImageURL = student.ProfileImageURL;
                    reenterStudent.StudentFeeGroupId = student.StudentFeeGroupId;
                    reenterStudent.ResidentTypeId = student.ResidentTypeId;
                    reenterStudent.StudentFeeTypeId = student.StudentFeeTypeId;
                    reenterStudent.Email2 = student.Email2;
                    reenterStudent.NativeLanguage = student.NativeLanguage;
                    reenterStudent.Barcode = student.Barcode;
                    reenterStudent.AdmissionRemark = student.AdmissionRemark;

                    reenterStudent.AdmissionInformation.PreviousSchoolId = student.AdmissionInformation.PreviousSchoolId;
                    reenterStudent.AdmissionInformation.EducationBackgroundId = student.AdmissionInformation.EducationBackgroundId;
                    reenterStudent.AdmissionInformation.PreviousGraduatedYear = student.AdmissionInformation.PreviousGraduatedYear;
                    reenterStudent.AdmissionInformation.PreviousSchoolGPA = student.AdmissionInformation.PreviousSchoolGPA;
                    reenterStudent.AdmissionInformation.CheckDated = student.AdmissionInformation.CheckDated;
                    reenterStudent.AdmissionInformation.CheckReferenceNumber = student.AdmissionInformation.CheckReferenceNumber;
                    reenterStudent.AdmissionInformation.ReplyDate = student.AdmissionInformation.ReplyDate;
                    reenterStudent.AdmissionInformation.ReplyReferenceNumber = student.AdmissionInformation.ReplyReferenceNumber;
                    reenterStudent.AdmissionInformation.AdmissionDocumentGroupId = student.AdmissionInformation.AdmissionDocumentGroupId;
                    reenterStudent.AdmissionInformation.FacultyId = reenterStudent.AcademicInformation.FacultyId;
                    reenterStudent.AdmissionInformation.DepartmentId = reenterStudent.AcademicInformation.DepartmentId;
                    reenterStudent.AdmissionInformation.CurriculumVersionId = reenterStudent.AcademicInformation.CurriculumVersionId;
                    reenterStudent.AdmissionInformation.PreviousBachelorSchoolId = student.AdmissionInformation.PreviousBachelorSchoolId;
                    reenterStudent.AdmissionInformation.PreviousMasterSchoolId = student.AdmissionInformation.PreviousMasterSchoolId;
                    reenterStudent.AdmissionInformation.AgencyId = student.AdmissionInformation.AgencyId;
                    reenterStudent.AdmissionInformation.OfficerName = student.AdmissionInformation.OfficerName;
                    reenterStudent.AdmissionInformation.OfficerPhone = student.AdmissionInformation.OfficerPhone;
                    reenterStudent.AdmissionInformation.ChannelId = student.AdmissionInformation.ChannelId;
                    reenterStudent.AdmissionInformation.EntranceExamResult = student.AdmissionInformation.EntranceExamResult;
                    reenterStudent.AdmissionInformation.AppliedAt = student.AdmissionInformation.AppliedAt;
                    reenterStudent.AdmissionInformation.ApplicationNumber = student.AdmissionInformation.ApplicationNumber;
                    reenterStudent.AdmissionInformation.GraduatedCountryId = student.AdmissionInformation.GraduatedCountryId;
                    reenterStudent.AdmissionInformation.Password = student.AdmissionInformation.Password;
                    reenterStudent.AdmissionInformation.PreviousBachelorGraduatedYear = student.AdmissionInformation.PreviousBachelorGraduatedYear;
                    reenterStudent.AdmissionInformation.PreviousBachelorSchoolGPA = student.AdmissionInformation.PreviousBachelorSchoolGPA;
                    reenterStudent.AdmissionInformation.PreviousMasterGraduatedYear = student.AdmissionInformation.PreviousMasterGraduatedYear;
                    reenterStudent.AdmissionInformation.PreviousMasterSchoolGPA = student.AdmissionInformation.PreviousMasterSchoolGPA;

                    reenterStudent.AcademicInformation.AdvisorId = student.AcademicInformation.AdvisorId;
                    reenterStudent.AcademicInformation.OldStudentCode = student.Code;
                    reenterStudent.AcademicInformation.GPA = 0;
                    reenterStudent.AcademicInformation.CreditComp = 0;
                    reenterStudent.AcademicInformation.CreditExempted = 0;
                    reenterStudent.AcademicInformation.CreditEarned = 0;
                    reenterStudent.AcademicInformation.CreditTransfer = 0;
                    reenterStudent.AcademicInformation.MaximumCredit = 0;
                    reenterStudent.AcademicInformation.MinimumCredit = 0;
                    reenterStudent.AcademicInformation.DegreeName = student.AcademicInformation.DegreeName;
                    reenterStudent.AcademicInformation.StudyPlanId = student.AcademicInformation.StudyPlanId;
                    reenterStudent.AcademicInformation.IsAthlete = student.AcademicInformation.IsAthlete;
                    reenterStudent.AcademicInformation.IsGraduating = student.AcademicInformation.IsGraduating;

                    _db.Students.Add(reenterStudent);
                    _db.SaveChanges();

                    foreach (var item in student.StudentExemptedExamScores)
                    {
                        _db.StudentExemptedExamScores.Add(new StudentExemptedExamScore
                                                          {
                                                              StudentId = reenterStudent.Id,
                                                              ExemptedExaminationId = item.ExemptedExaminationId,
                                                              Score = item.Score,
                                                              StudentDocumentId = item.StudentDocumentId,
                                                              CreatedAt = item.CreatedAt,
                                                              CreatedBy = item.CreatedBy,
                                                              UpdatedAt = item.UpdatedAt,
                                                              UpdatedBy = item.UpdatedBy,
                                                              IsActive = item.IsActive
                                                          });
                    }

                    foreach (var item in student.StudentIncidents)
                    {
                        _db.StudentIncidents.Add(new StudentIncident
                                                 {
                                                     StudentId = reenterStudent.Id,
                                                     TermId = item.TermId,
                                                     IncidentId = item.IncidentId ,
                                                     LockedDocument = item.LockedDocument,
                                                     LockedRegistration = item.LockedRegistration,
                                                     LockedPayment = item.LockedPayment,
                                                     LockedVisa = item.LockedVisa,
                                                     LockedGraduation = item.LockedGraduation,
                                                     LockedChangeFaculty = item.LockedChangeFaculty,
                                                     ApprovedBy = item.ApprovedBy,
                                                     ApprovedAt = item.ApprovedAt,
                                                     CreatedAt = item.CreatedAt,
                                                     CreatedBy = item.CreatedBy,
                                                     UpdatedAt = item.UpdatedAt,
                                                     UpdatedBy = item.UpdatedBy,
                                                     IsActive = item.IsActive,
                                                     Remark = item.Remark
                                                 });
                    }

                    foreach (var item in student.StudentAddresses)
                    {
                        _db.StudentAddresses.Add(new StudentAddress
                                                 {
                                                     StudentId = reenterStudent.Id,
                                                     HouseNumber = item.HouseNumber,
                                                     AddressTh1 = item.AddressTh1,
                                                     AddressTh2 = item.AddressTh2,
                                                     AddressEn1 = item.AddressEn1,
                                                     AddressEn2 = item.AddressEn2,
                                                     Moo = item.Moo,
                                                     SoiTh = item.SoiTh,
                                                     SoiEn = item.SoiEn,
                                                     CountryId = item.CountryId,
                                                     ProvinceId = item.ProvinceId,
                                                     DistrictId = item.DistrictId,
                                                     SubdistrictId = item.SubdistrictId,
                                                     CityId = item.CityId,
                                                     StateId = item.StateId,
                                                     ZipCode = item.ZipCode,
                                                     TelephoneNumber = item.TelephoneNumber,
                                                     Type = item.Type,
                                                     RoadEn = item.RoadEn,
                                                     RoadTh = item.RoadTh,
                                                     CreatedAt = item.CreatedAt,
                                                     CreatedBy = item.CreatedBy,
                                                     UpdatedAt = item.UpdatedAt,
                                                     UpdatedBy = item.UpdatedBy,
                                                     IsActive = item.IsActive
                                                 });
                    }

                    foreach (var item in student.ParentInformations)
                    {
                        _db.ParentInformations.Add(new ParentInformation
                                                   {
                                                       StudentId = reenterStudent.Id,
                                                       FirstNameEn = item.FirstNameEn,
                                                       FirstNameTh = item.FirstNameTh,
                                                       LastNameEn = item.LastNameEn,
                                                       LastNameTh = item.LastNameTh,
                                                       MidNameEn = item.MidNameEn,
                                                       MidNameTh = item.MidNameTh,
                                                       RelationshipId = item.RelationshipId,
                                                       Email = item.Email,
                                                       MailToParent = item.MailToParent,
                                                       EmailToParent = item.EmailToParent,
                                                       SMSToParent = item.SMSToParent,
                                                       TelephoneNumber1 = item.TelephoneNumber1,
                                                       TelephoneNumber2 = item.TelephoneNumber2,
                                                       AddressTh = item.AddressTh,
                                                       AddressEn = item.AddressEn,
                                                       CountryId = item.CountryId,
                                                       ProvinceId = item.ProvinceId,
                                                       DistrictId = item.DistrictId,
                                                       SubdistrictId = item.SubdistrictId,
                                                       CityId = item.CityId,
                                                       StateId = item.StateId,
                                                       ZipCode = item.ZipCode,
                                                       CitizenNumber = item.CitizenNumber,
                                                       LivingStatus = item.LivingStatus,
                                                       OccupationId = item.OccupationId,
                                                       IsMainParent = item.IsMainParent,
                                                       IsEmergencyContact = item.IsEmergencyContact,
                                                       RevenueId = item.RevenueId,
                                                       CreatedAt = item.CreatedAt,
                                                       CreatedBy = item.CreatedBy,
                                                       UpdatedAt = item.UpdatedAt,
                                                       UpdatedBy = item.UpdatedBy,
                                                       IsActive = item.IsActive,
                                                       Passport = item.Passport
                                                   });
                    }

                    foreach (var item in student.MaintenanceStatuses)
                    {
                        _db.MaintenanceStatuses.Add(new MaintenanceStatus
                                                    {
                                                        StudentId = reenterStudent.Id,
                                                        TermId = item.TermId,
                                                        MaintenanceFeeId = item.MaintenanceFeeId,
                                                        PaidDate = item.PaidDate,
                                                        InvoiceNumber = item.InvoiceNumber,
                                                        ApprovedAt = item.ApprovedAt,
                                                        Remark = item.Remark,
                                                        CreatedAt = item.CreatedAt,
                                                        CreatedBy = item.CreatedBy,
                                                        UpdatedAt = item.UpdatedAt,
                                                        UpdatedBy = item.UpdatedBy,
                                                        IsActive = item.IsActive
                                                    });
                    }

                    foreach (var item in student.CheatingStatuses)
                    {
                        _db.CheatingStatuses.Add(new CheatingStatus
                                                 {
                                                     StudentId = reenterStudent.Id,
                                                     RegistrationCourseId = item.RegistrationCourseId,
                                                     TermId = item.TermId,
                                                     ExaminationTypeId = item.ExaminationTypeId,
                                                     IncidentId = item.IncidentId,
                                                     FromTermId = item.FromTermId,
                                                     ToTermId = item.ToTermId,
                                                     Detail = item.Detail,
                                                     CreatedAt = item.CreatedAt,
                                                     CreatedBy = item.CreatedBy,
                                                     UpdatedAt = item.UpdatedAt,
                                                     UpdatedBy = item.UpdatedBy,
                                                     IsActive = item.IsActive,
                                                     ApprovedBy = item.ApprovedBy
                                                 });
                    }

                    foreach (var item in student.ScholarshipStudents)
                    {
                        _db.ScholarshipStudents.Add(new ScholarshipStudent
                                                    {
                                                        StudentId = reenterStudent.Id,
                                                        ScholarshipId = item.ScholarshipId,
                                                        EffectivedTermId = item.EffectivedTermId,
                                                        ExpiredTermId = item.ExpiredTermId,
                                                        IsApproved = item.IsApproved,
                                                        ReferenceDate = item.ReferenceDate,
                                                        SendContract = item.SendContract,
                                                        LimitedAmount = item.LimitedAmount,
                                                        Remark = item.Remark,
                                                        AllowRepeatedRegistration = item.AllowRepeatedRegistration,
                                                        HaveScholarshipHour = item.HaveScholarshipHour,
                                                        ApprovedAt = item.ApprovedAt,
                                                        ApprovedBy = item.ApprovedBy,
                                                        CreatedAt = item.CreatedAt,
                                                        CreatedBy = item.CreatedBy,
                                                        UpdatedAt = item.UpdatedAt,
                                                        UpdatedBy = item.UpdatedBy,
                                                        IsActive = item.IsActive
                                                    });
                    }

                    foreach (var item in student.StudentDocuments)
                    {
                        _db.StudentDocuments.Add(new StudentDocument
                                                     {
                                                         StudentId = reenterStudent.Id,
                                                         RequiredDocumentId = item.RequiredDocumentId,
                                                         ImageUrl = item.ImageUrl,
                                                         Amount = item.Amount,
                                                         Remark = item.Remark,
                                                         DocumentId = item.DocumentId,
                                                         IsRequired = item.IsRequired,
                                                         SubmittedAmount = item.SubmittedAmount,
                                                         CreatedAt = item.CreatedAt,
                                                         CreatedBy = item.CreatedBy,
                                                         UpdatedAt = item.UpdatedAt,
                                                         UpdatedBy = item.UpdatedBy,
                                                         IsActive = item.IsActive
                                                     });
                    }

                    _db.SaveChanges();

                    var saveReenterLog = SaveStudentStatusLog(reenterStudent.Id, termId, source.GetDisplayName(), reason, type);
                    if (!saveStudentLog)
                    {
                        transaction.Rollback();
                        errorMessage = Message.UnableToSaveStudentStatusLog;
                        return null;
                    }

                    transaction.Commit();
                    errorMessage = "";
                    return reenterStudent;
                }
                catch
                {
                    transaction.Rollback();
                    errorMessage = Message.UnableToSaveReenterStudent;
                    return null;
                }
            }
        }
        
        public Document GetDocument(long documentId) 
        {
            return _db.Documents.SingleOrDefault(x => x.Id == documentId);
        }
        
        public bool SaveStudentProfileImage(string studentCode, string imageUrl)
        {
            try 
            {
                var student = GetStudentByCode(studentCode);
                student.ProfileImageURL = String.IsNullOrEmpty(imageUrl) ? null : imageUrl;
                _db.SaveChanges();
                return true;
            }
            catch {
                return false;
            }
        }

        public CheatingStatus GetCheatingStatus(long id)
        {
            var cheatingStatus = _db.CheatingStatuses.Include(x => x.RegistrationCourse)
                                                         .ThenInclude(x => x.Section)
                                                             .ThenInclude(x => x.Course)
                                                     .Include(x => x.Term)
                                                     .Include(x => x.ExaminationType)
                                                     .Include(x => x.Incident)
                                                     .Include(x => x.FromTerm)
                                                     .Include(x => x.ToTerm)
                                                     .Include(x => x.Student)
                                                         .ThenInclude(x => x.AcademicInformation)
                                                             .ThenInclude(x => x.AcademicLevel)
                                                     .OrderBy(x => x.Term.AcademicTerm)
                                                     .ThenBy(x => x.Term.AcademicYear)
                                                     .SingleOrDefault(x => x.Id == id);
            return cheatingStatus;
        }

        public bool SaveStudentIncident(CheatingStatus status)
        {
            try
            {
                var incident = _db.Incidents.SingleOrDefault(x => x.Id == status.IncidentId);
                if (incident != null)
                {
                    _db.StudentIncidents.Add(new StudentIncident
                                             {
                                                 StudentId = status.StudentId,
                                                 IncidentId = status.IncidentId ?? 0,
                                                 TermId = status.TermId,
                                                 LockedDocument = incident.LockedDocument,
                                                 LockedRegistration = incident.LockedRegistration,
                                                 LockedPayment = incident.LockedPayment,
                                                 LockedVisa = incident.LockedVisa,
                                                 LockedGraduation = incident.LockedGraduation,
                                                 LockedChangeFaculty = incident.LockedChangeFaculty,
                                                 ApprovedAt = DateTime.Now,
                                             });

                    _db.SaveChanges();
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateStudentIncident(CheatingStatus previousModel, CheatingStatus model)
        {
            try
            {
                if (model.IncidentId == previousModel.IncidentId)
                {
                    return true;
                }

                var studentIncident = _db.StudentIncidents.FirstOrDefault(x => x.StudentId == model.StudentId
                                                                               && x.IncidentId == previousModel.IncidentId
                                                                               && x.TermId == model.TermId
                                                                               && x.IsActive);
                if (studentIncident != null)
                {
                    studentIncident.IsActive = false;
                }

                SaveStudentIncident(model);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public long GetStudentAcademicLevelIdByCode(string code)
        {
            var academicLevelId = _db.Students.Include(x => x.AcademicInformation)
                                              .SingleOrDefault(x => x.Code == code)
                                              ?.AcademicInformation
                                              ?.AcademicLevelId ?? 0;
            return academicLevelId;
        }

        public bool SaveStudentProfileImageByApplicationNumber(string applicationNumber, string imageUrl)
        {
            try 
            {
                var student = GetStudentByApplicationNumber(applicationNumber);
                student.ProfileImageURL = String.IsNullOrEmpty(imageUrl) ? null : imageUrl;
                _db.SaveChanges();
                return true;
            }
            catch {
                return false;
            }
        }

        public Student GetStudentByApplicationNumber(string applicationNumber)
        {
            var admissionInformation = _db.AdmissionInformations.Include(x => x.Student)
                                                                .SingleOrDefault(x => x.ApplicationNumber == applicationNumber);
            return admissionInformation.Student;
        }

        public List<TransferUniversity> GetTransferUniversityByStudentCode(string studentCode)
        {
            var university = _db.StudentTransferLogs.Where(x => x.Student.Code == studentCode)
                                                    .OrderByDescending(x => x.UpdatedAt)
                                                    .Select(x => x.TransferUniversity)
                                                    .Distinct()
                                                    .ToList();
            return university;
        }

        public StudentResultApiViewModel AddStudents(long academicLevelId, long termId, long admissionRoundId, SaveStudentsViewModel model)
        {
            var result = new StudentResultApiViewModel
                         {
                             Success = new List<StudentSuccessViewModel>(),
                             Duplicate = new List<StudentAPIViewModel>(),
                             Fail = new List<StudentFailViewModel>()
                         };

            foreach(var item in model.Students)
            {
                if(string.IsNullOrEmpty(item.AlreadyExist))
                {
                    var student = CheckStudentInformationAndCreateStudent(item, academicLevelId, admissionRoundId);
                    if(student is StudentSuccessViewModel)
                    {
                        result.Success.Add(student);
                    }
                    else
                    {
                        result.Fail.Add(student);
                    }
                }
                else
                {
                    result.Duplicate.Add(item);
                }
            }

            return result;
        }

        private dynamic CheckStudentInformationAndCreateStudent(StudentAPIViewModel model, long academicLevelId, long admissionRoundId)
        {
            var fail = new StudentFailViewModel();
            var success = new StudentSuccessViewModel();
            fail.Student = model;

            var title = _db.Titles.SingleOrDefault(x => x.NameEn.StartsWith(model.Title)
                                                        || x.NameTh.StartsWith(model.Title));
            if(title == null)
            {
                fail.Comment = "Title not found";
                return fail;
            }

            if(string.IsNullOrEmpty(model.FirstNameEn) || string.IsNullOrEmpty(model.LastNameEn))
            {
                fail.Comment = "First Name and Last Name not null";
                return fail;
            }

            if(!(model.Gender == 0 || model.Gender == 1 || model.Gender == 2))
            {
                fail.Comment = "Gender invalid, Example : 0 = undefind, 1 = male, 2 = female";
                return fail;
            }

            var nationality = _db.Nationalities.SingleOrDefault(x => x.NameEn.StartsWith(model.Nationality)
                                                                     || x.NameTh.StartsWith(model.Nationality));
            if(nationality == null)
            {
                fail.Comment = "Nationality not found";
                return fail;
            }

            var religion = _db.Religions.SingleOrDefault(x => x.NameEn.StartsWith(model.Religion)
                                                              || x.NameTh.StartsWith(model.Religion));
            if(religion == null)
            {
                fail.Comment = "Religion not found";
                return fail;
            }

            DateTime? birthDate = _dateTimeProvider.ConvertStringToDateTime(model.BirthDate);
            if(birthDate == null || birthDate == new DateTime())
            {
                fail.Comment = "BirthDate invalid, format: dd/MM/yyyy";
                return fail;
            }

            var birthCountry = _db.Countries.SingleOrDefault(x => x.NameEn.StartsWith(model.BirthCountry)
                                                                  || x.NameTh.StartsWith(model.BirthCountry));
            if(birthCountry == null)
            {
                fail.Comment = "Birth Country not found";
                return fail;
            }

            var residentType = _db.ResidentTypes.SingleOrDefault(x => x.NameEn.StartsWith(model.ResidentType) 
                                                                      || x.NameTh.StartsWith(model.ResidentType));
            if(residentType == null)
            {
                fail.Comment = "Resident type not found, Example : resident, non-resident, visiting";
                return fail;
            }

            var studentFeeType = _db.StudentFeeTypes.SingleOrDefault(x => x.NameEn.StartsWith(model.StudentFeeType)
                                                                          || x.NameTh.StartsWith(model.StudentFeeType));
            if(studentFeeType == null)
            {
                fail.Comment = "Student fee type not found, Example : resident, non-resident, inbound";
                return fail;
            }

            if(model.Batch <= 0 )
            {
                fail.Comment = "Batch invalid";
                return fail;
            }

            var faculty = _db.Faculties.SingleOrDefault(x => x.NameEn == model.Faculty);
            if(faculty == null)
            
            {
                fail.Comment = "Faculty not found";
                return fail;
            }

            var department = _db.Departments.SingleOrDefault(x => x.NameEn == model.Department);
            if(department == null)
            {
                fail.Comment = "Department not found";
                return fail;
            }

            var academicProgram = _db.AcademicPrograms.SingleOrDefault(x => x.NameEn == model.AcademicProgram);
            if(department == null)
            {
                fail.Comment = "Academic Program not found";
                return fail;
            }

            var term = _db.Terms.SingleOrDefault(x => x.AcademicLevelId == academicLevelId
                                                      && x.AcademicTerm == model.AdmissionTerm
                                                      && x.AcademicYear == model.AdmissionYear);

            if(term == null)
            {
                fail.Comment = "Admission Term or Admisstion Year invalid";
                return fail;
            }
            DateTime? admissionDate = _dateTimeProvider.ConvertStringToDateTime(model.AdmissionDate);
            if(admissionDate == null || admissionDate == new DateTime())
            {
                    fail.Comment = "Admission Date invalid, format: dd/MM/yyyy";
                    return fail;
            }

            DateTime? passportExpiredAt = null;
            if(!string.IsNullOrEmpty(model.PassportExpiredAt))
            {
                passportExpiredAt = _dateTimeProvider.ConvertStringToDateTime(model.PassportExpiredAt);
                if(passportExpiredAt == null || passportExpiredAt == new DateTime())
                {
                    fail.Comment = "Passpost expried invalid, format: dd/MM/yyyy";
                    return fail;
                }
            }
            long? raceId = null;
            if(!string.IsNullOrEmpty(model.Race))
            {
                var race = _db.Races.SingleOrDefault(x => x.NameEn == model.Race  
                                                      || x.NameTh == model.Race);
                if(race == null)
                {
                    fail.Comment = "Race not found";
                    return fail;
                }

                raceId = race.Id;
            }
            DateTime? passportIssueAt = null;
            if(!string.IsNullOrEmpty(model.PassportIssueAt))
            {
                passportIssueAt = _dateTimeProvider.ConvertStringToDateTime(model.PassportIssueAt);
                if(passportIssueAt == null || passportIssueAt == new DateTime())
                {
                    fail.Comment = "Passpost issue invalid, format: dd/MM/yyyy";
                    return fail;
                }
            }

            var studentGroup = _db.StudentGroups.SingleOrDefault(x => x.Name == model.StudentGroup);
            if(studentGroup == null)
            {
                fail.Comment = "Student Group not found";
                return fail;
            }

            long? advisorId = null; 
            if(!string.IsNullOrEmpty(model.AdvisorCode))
            {
                var advisor = _db.Instructors.SingleOrDefault(x => x.Code == model.AdvisorCode);
                if(advisor == null)
                {
                    fail.Comment = "Advisor not found";
                    return fail;
                }

                advisorId = advisor.Id;
            }
            var code = _admissionProvider.GenerateStudentCode(academicLevelId, admissionRoundId);
            if(string.IsNullOrEmpty(code))
            {
                fail.Comment = "Student code ranges not found";
                return fail;
            }
            var student = new Student();
            using(var transaction = _db.Database.BeginTransaction())
            {
                try
                {                    
                    student = new Student
                              {
                                  Code = code,
                                  TitleId = title.Id,
                                  FirstNameEn = model.FirstNameEn,
                                  FirstNameTh = model.FirstNameTh,
                                  LastNameEn = model.LastNameEn,
                                  LastNameTh = model.LastNameTh,
                                  MidNameEn = model.MidNameEn,
                                  Gender = model.Gender,
                                  RaceId = raceId,
                                  NationalityId = nationality.Id,
                                  ReligionId = religion.Id,
                                  BirthDate = birthDate??new DateTime(),
                                  BirthCountryId = birthCountry.Id,
                                  CitizenNumber = model.CitizenNumber,
                                  Passport = model.Passport,
                                  PassportExpiredAt = passportExpiredAt,
                                  PassportIssuedAt = passportIssueAt,
                                  Email = model.Email,
                                  PersonalEmail = model.PersonalEmail,
                                  TelephoneNumber1 = model.TelephoneNumber1,
                                  TelephoneNumber2 = model.TelephoneNumber2,
                                  ResidentTypeId = residentType.Id,
                                  StudentFeeTypeId  = studentFeeType.Id,
                                  RegistrationStatusId = 1,
                                  StudentStatus = "a"
                              };

                    _db.Students.Add(student);
                    _db.SaveChanges();
                    var admissionCurriculum = _db.AdmissionCurriculums.SingleOrDefault(x => x.AdmissionRoundId == admissionRoundId
                                                                                            && x.AcademicLevelId == academicLevelId
                                                                                            && x.DepartmentId == department.Id);
                    var academicInfo = new AcademicInformation  
                                       {
                                           AcademicLevelId = academicLevelId,
                                           StudentId = student.Id,
                                           Batch = model.Batch,
                                           CreditComp = 0,
                                           MinimumCredit = 12,
                                           MaximumCredit = 22,
                                           StudentGroupId = studentGroup.Id,
                                           FacultyId = faculty.Id,
                                           DepartmentId = department.Id,
                                           GPA = 0,
                                           AcademicProgramId = academicProgram.Id,
                                           IsAthlete = false,
                                           IsGraduating = false,
                                           CurriculumVersionId = admissionCurriculum.CurriculumVersionId,
                                       };

                    _db.AcademicInformations.Add(academicInfo);
                    _db.SaveChanges();

                    var curriculumInformation = new CurriculumInformation
                                                {
                                                    StudentId = student.Id,
                                                    FacultyId = faculty.Id,
                                                    DepartmentId = department.Id,
                                                    CurriculumVersionId = admissionCurriculum.CurriculumVersionId
                                                };

                    _db.CurriculumInformations.Add(curriculumInformation);
                    _db.SaveChanges();
                    transaction.Commit();
                    
                }
                catch
                {
                    fail.Comment = "Unable to Save student";
                    transaction.Rollback();
                    return fail;
                }
            
                success = new StudentSuccessViewModel(student.Code, model);
            }
            if(model.StudentAddresses != null && model.StudentAddresses?.Count != 0)
            {
                var studentAddresses = new StudentAddressResuleViewModel
                                       {
                                           Success = new List<StudentAddressViewModel>(),
                                           Fail = new List<StudentAddressViewModel>()
                                       };

                foreach(var item in model.StudentAddresses)
                {
                    var studentAddress = SaveStudentAddress(student.Id, item);
                    if(string.IsNullOrEmpty(studentAddress.Comment))
                    {
                        studentAddresses.Success.Add(studentAddress);
                    }
                    else
                    {
                        studentAddresses.Fail.Add(studentAddress);
                    }
                }

                success.StudentAddress = studentAddresses;
            }

            if(model.ParentInformations != null && model.ParentInformations?.Count != 0)
            {
                var studentParents = new StudentParentResultViewModel
                                       {
                                           Success = new List<ParentInformationViewModel>(),
                                           Fail = new List<ParentInformationViewModel>()
                                       };

                foreach(var item in model.ParentInformations)
                {
                    var studentParent = SaveStudentParentInformation(student.Id, item);
                    if(string.IsNullOrEmpty(studentParent.Comment))
                    {
                        studentParents.Success.Add(studentParent);
                    }
                    else
                    {
                        studentParents.Fail.Add(studentParent);
                    }
                }

                success.ParentInformation = studentParents;
            }
            

            return success;
        }

        public StudentAddressViewModel SaveStudentAddress(Guid studentId, StudentAddressViewModel address)
        {
            var country = _db.Countries.SingleOrDefault(x => x.NameEn == address.Country
                                                             || x.NameTh == address.Country);
            if(country == null)
            {
                address.Comment += "Country not found";
            }

            if(address.Type != "Permanent" && address.Type != "Current")
            {
                address.Comment += string.IsNullOrEmpty(address.Comment) ? "Type invalid(example Permanent, Current)" : ", Type invalid(example Permanent, Current)";
            }

            if(string.IsNullOrEmpty(address.ZipCode))
            {
                address.Comment += string.IsNullOrEmpty(address.Comment) ? "ZipCode not null " : ", ZipCode not null ";
            }

            long? provinceId = null;
            if(!string.IsNullOrEmpty(address.Province))
            {
                var province = _db.Provinces.SingleOrDefault(x => x.NameTh == address.Province 
                                                                  || x.NameEn == address.Province);

                if(province == null)
                {
                    address.Comment += string.IsNullOrEmpty(address.Comment) ? "Province not found" : ", Province not found";
                }
                else
                {
                    provinceId = province.Id;
                }
            }        
            
            long? districtId = null;
            if(!string.IsNullOrEmpty(address.District))
            {
                var district = _db.Districts.SingleOrDefault(x => x.NameTh == address.District 
                                                                  || x.NameEn == address.District);

                if(district == null)
                {
                    address.Comment += string.IsNullOrEmpty(address.Comment) ? "District not found" : ", District not found";
                }
                else
                {
                    districtId = district.Id;
                }
            }

            long? subdistrictId = null;
            if(!string.IsNullOrEmpty(address.Subdistrict))
            {
                var subdistrict = _db.Subdistricts.SingleOrDefault(x => x.NameTh == address.Subdistrict 
                                                                  || x.NameEn == address.Subdistrict);

                if(subdistrict == null)
                {
                    address.Comment += string.IsNullOrEmpty(address.Comment) ? "Subdistrict not found" : ", Subdistrict not found";
                }
                else
                {
                    subdistrictId = subdistrict.Id;
                }
            }

            if(string.IsNullOrEmpty(address.Comment))
            {
                using(var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        var studentAddress = new StudentAddress 
                                            {
                                                StudentId = studentId,
                                                HouseNumber = address.HouseNumber,
                                                AddressTh1 = address.AddressTh1,
                                                AddressTh2 = address.AddressTh2,
                                                Moo = address.Moo,
                                                SoiTh = address.SoiTh,
                                                SoiEn = address.SoiEn,
                                                CountryId = country.Id,
                                                ProvinceId = provinceId,
                                                DistrictId = districtId,
                                                SubdistrictId = subdistrictId,
                                                ZipCode = address.ZipCode,
                                                TelephoneNumber = address.TelephoneNumber,
                                                Type = address.Type,
                                                AddressEn1 = address.AddressEn1,
                                                AddressEn2 = address.AddressEn2,
                                                RoadEn = address.RoadEn,
                                                RoadTh = address.RoadTh
                                            };
                        _db.StudentAddresses.Add(studentAddress);
                        _db.SaveChanges();
                        transaction.Commit();
                        return address;
                    }
                    catch
                    {
                        address.Comment += string.IsNullOrEmpty(address.Comment) ? "Unable to save address" : ", Unable to save address";
                        transaction.Rollback();
                        return address;
                    }
                }
            }

            return address;
        }

        public ParentInformationViewModel SaveStudentParentInformation(Guid studentId, ParentInformationViewModel parent)
        {
            if(string.IsNullOrEmpty(parent.FirstNameEn) || string.IsNullOrEmpty(parent.LastNameEn))
            {
                parent.Comment += string.IsNullOrEmpty(parent.Comment) ? "FirstNameEn or LastNameEn not null" : ", FirstNameEn or LastNameEn not null";
            }

            if(parent.IsMainParent?.ToUpper() != "YES" && parent.IsMainParent?.ToUpper() != "NO")
            {
                parent.Comment += string.IsNullOrEmpty(parent.Comment) ? "IsMainParent invalid(example yes, no)" : ", IsMainParent invalid(example yes, no)";
            }

            if(parent.IsEmergencyContact?.ToUpper() != "YES" && parent.IsEmergencyContact?.ToUpper() != "NO")
            {
                parent.Comment += string.IsNullOrEmpty(parent.Comment) ? "IsEmergencyContact invalid(example yes, no)" : ", IsEmergencyContact invalid(example yes, no)";
            }

            var relationship = _db.Relationships.SingleOrDefault(x => x.NameEn == parent.Relationship);
            if(relationship == null)
            {
                parent.Comment += string.IsNullOrEmpty(parent.Comment) ? "Relationship invalid(example Father, Mother, Agency, Guardian)" : ", Type invalid(example Father, Mother, Agency, Guardian)";
            }

            if(string.IsNullOrEmpty(parent.TelephoneNumber1))
            {
                parent.Comment += string.IsNullOrEmpty(parent.Comment) ? "TelephoneNumber1 not found" : ", TelephoneNumber1 not found";
            }

            long? provinceId = null;
            if(!string.IsNullOrEmpty(parent.Province))
            {
                var province = _db.Provinces.SingleOrDefault(x => x.NameTh == parent.Province 
                                                                  || x.NameEn == parent.Province);

                if(province == null)
                {
                    parent.Comment += string.IsNullOrEmpty(parent.Comment)? "Province not found" : ", Province not found";
                }
                else
                {
                    provinceId = province.Id;
                }
            }
            
            long? districtId = null;
            if(!string.IsNullOrEmpty(parent.District))
            {
                var district = _db.Districts.SingleOrDefault(x => x.NameTh == parent.District 
                                                                  || x.NameEn == parent.District);

                if(district == null)
                {
                    parent.Comment += string.IsNullOrEmpty(parent.Comment) ? "District not found" : ", District not found";
                }
                else
                {
                    districtId = district.Id;
                }
            }

            long? countryId = null;
            if(!string.IsNullOrEmpty(parent.Country))
            {
                var country = _db.Countries.SingleOrDefault(x => x.NameEn == parent.Country
                                                                 || x.NameTh == parent.Country);
                if(country == null)
                {
                    parent.Comment += "Country not found";
                }
                else
                {
                    countryId = country.Id;
                }
            }

            long? subdistrictId = null;
            if(!string.IsNullOrEmpty(parent.Subdistrict))
            {
                var subdistrict = _db.Subdistricts.SingleOrDefault(x => x.NameTh == parent.Subdistrict 
                                                                        || x.NameEn == parent.Subdistrict);

                if(subdistrict == null)
                {
                    parent.Comment += string.IsNullOrEmpty(parent.Comment) ? "Subdistrict not found" : ", Subdistrict not found";
                }
                else
                {
                    subdistrictId = subdistrict.Id;
                }
            }

            if(string.IsNullOrEmpty(parent.Comment))
            {
                using(var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        var parentInfo = new ParentInformation 
                                         {
                                             StudentId = studentId,
                                             RelationshipId = relationship.Id,
                                             Email = parent.Email,
                                             TelephoneNumber1 = parent.TelephoneNumber1,
                                             TelephoneNumber2 = parent.TelephoneNumber2,
                                             AddressTh = parent.AddressTh,
                                             AddressEn = parent.AddressEn,
                                             CountryId = countryId,
                                             ProvinceId = provinceId,
                                             DistrictId = districtId,
                                             SubdistrictId = subdistrictId,
                                             ZipCode = parent.ZipCode,
                                             CitizenNumber = parent.CitizenNumber,
                                             FirstNameEn = parent.FirstNameEn,
                                             LastNameEn = parent.LastNameEn,
                                             MidNameEn = parent.MidNameEn,
                                             IsEmergencyContact = parent.IsEmergencyContact.ToUpper() == "YES" ? true : false,
                                             IsMainParent = parent.IsMainParent.ToUpper() == "YES" ? true : false,
                                             FirstNameTh = parent.FirstNameTh,
                                             LastNameTh = parent.FirstNameTh,
                                             MidNameTh = parent.MidNameTh,
                                             Passport = parent.Passport                      
                                         };
                        _db.ParentInformations.Add(parentInfo);
                        _db.SaveChanges();
                        transaction.Commit();
                        return parent;
                    }
                    catch
                    {
                        parent.Comment += string.IsNullOrEmpty(parent.Comment) ? "Unable to save parent" : ", Unable to save parent";
                        transaction.Rollback();
                        return parent;
                    }
                }
            }
            
            return parent;
        }

        public List<Student> GetStudentForAssignAdvisee(Criteria criteria)
        {
            var students = _db.Students.Include(x => x.Nationality)
                                       .Include(x => x.AdmissionInformation)
                                           .ThenInclude(x => x.Faculty)
                                       .Include(x => x.AcademicInformation)
                                           .ThenInclude(x => x.Faculty)
                                       .Include(x => x.AcademicInformation)
                                           .ThenInclude(x => x.Department)
                                       .Include(x => x.AcademicInformation)
                                           .ThenInclude(x => x.CurriculumVersion)
                                        .Include(x => x.AcademicInformation)
                                           .ThenInclude(x => x.Advisor)
                                                .ThenInclude(x => x.Title)
                                       .Include(x => x.Title)
                                       .Where(x => x.StudentStatus != "a"
                                                   && (criteria.StudentCodeFrom == null
                                                       || (criteria.StudentCodeTo == null || criteria.StudentCodeFrom == criteria.StudentCodeTo 
                                                            ? x.Code.StartsWith(criteria.StudentCodeFrom.ToString())
                                                            : string.Compare(x.Code, criteria.StudentCodeFrom.ToString()) >= 0))
                                                   && (criteria.StudentCodeTo == null
                                                       || (criteria.StudentCodeFrom == null || criteria.StudentCodeFrom == criteria.StudentCodeTo
                                                            ? x.Code.StartsWith(criteria.StudentCodeTo.ToString())
                                                            : string.Compare(x.Code, criteria.StudentCodeTo.ToString()) <= 0))                                                   
                                                   && (string.IsNullOrEmpty(criteria.PreviousCode)
                                                       || x.AcademicInformation.OldStudentCode.StartsWith(criteria.PreviousCode))
                                                   && (string.IsNullOrEmpty(criteria.FirstName)
                                                       || x.FirstNameEn.StartsWith(criteria.FirstName)
                                                       || x.FirstNameTh.StartsWith(criteria.FirstName))
                                                   && (string.IsNullOrEmpty(criteria.LastName)
                                                       || x.LastNameEn.StartsWith(criteria.LastName)
                                                       || x.LastNameTh.StartsWith(criteria.LastName))
                                                   && (criteria.AdmissionTypeId == 0
                                                       || x.AdmissionInformation.AdmissionTypeId == criteria.AdmissionTypeId)
                                                   && (criteria.AcademicLevelId == 0
                                                       || x.AcademicInformation.AcademicLevelId == criteria.AcademicLevelId)
                                                   && (criteria.FacultyId == 0
                                                       || x.AcademicInformation.FacultyId == criteria.FacultyId)
                                                   && (criteria.DepartmentId == 0
                                                       || x.AcademicInformation.DepartmentId == criteria.DepartmentId)
                                                   && (criteria.CurriculumId == 0
                                                       || x.AcademicInformation.CurriculumVersion.CurriculumId == criteria.CurriculumId)
                                                   && (criteria.CurriculumVersionId == 0
                                                       || x.AcademicInformation.CurriculumVersionId == criteria.CurriculumVersionId)
                                                   && (criteria.Gender == null
                                                       || x.Gender == criteria.Gender)
                                                   && (criteria.NationalityId == 0
                                                       || x.NationalityId == criteria.NationalityId)
                                                   && (criteria.CreditFrom == null
                                                       || x.AcademicInformation.CreditComp >= criteria.CreditFrom)
                                                   && (criteria.CreditTo == null
                                                       || x.AcademicInformation.CreditComp <= criteria.CreditTo)
                                                   && (string.IsNullOrEmpty(criteria.StudentStatus)
                                                       || x.StudentStatus == criteria.StudentStatus)
                                                   && (string.IsNullOrEmpty(criteria.Status)
                                                       || x.IsActive == Convert.ToBoolean(criteria.Status)))
                                       //.Select(x => _mapper.Map<Student, StudentSearchViewModel>(x))
                                       .IgnoreQueryFilters()
                                       ;

            return students.OrderBy(x => x.Code).ToList();
        }
        
        public List<StudentIncidentLog> GetStudentIncidentLogsByStudentId(Guid id)
        {
            var studentIncidentLogs = _db.StudentIncidentLogs.Include(x => x.Incident)
                                                             .Include(x => x.Term)
                                                             .Where(x => x.StudentId == id)
                                                             .AsNoTracking()
                                                             .OrderBy(x => x.LockedByAt)
                                                             .ToList();
            var userIds = studentIncidentLogs.SelectMany(x => new [] { x.LockedBy, x.UnlockedBy}).ToList();
            var users = _userProvider.GetCreatedFullNameByIds(userIds);
            foreach(var item in studentIncidentLogs)
            {
                item.LockedBy = users.Any(x => x.CreatedBy == item.LockedBy) ? users.Where(x => x.CreatedBy == item.LockedBy).FirstOrDefault().CreatedByFullNameEn : item.LockedBy;
                item.UnlockedBy = users.Any(x => x.CreatedBy == item.UnlockedBy) ? users.Where(x => x.CreatedBy == item.UnlockedBy).FirstOrDefault().CreatedByFullNameEn : item.UnlockedBy;
            }
            return studentIncidentLogs;
        }

        public List<StudentIncident> GetStudentIncidentsByStudentId(Guid id)
        {
            var studentIncidents = _db.StudentIncidents.Include(x => x.Incident)
                                                       .Include(x => x.Term)
                                                       .Where(x => x.StudentId == id)
                                                       .AsNoTracking()
                                                       .ToList();

            return studentIncidents;
        }
    }
}