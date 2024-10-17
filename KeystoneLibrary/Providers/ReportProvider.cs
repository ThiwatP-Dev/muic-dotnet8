using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Curriculums;
using KeystoneLibrary.Models.DataModels.MasterTables;
using KeystoneLibrary.Models.DataModels.Profile;
using KeystoneLibrary.Models.Report;
using Microsoft.EntityFrameworkCore;

namespace KeystoneLibrary.Providers
{
    public class ReportProvider : BaseProvider, IReportProvider
    {
        protected IStudentProvider _studentProvider;
        protected ICacheProvider _cacheProvider;
        protected IGradeProvider _gradeProvider;
        protected IAcademicProvider _academicProvider;
        protected ICurriculumProvider _curriculumProvider;
        protected IDateTimeProvider _dateTimeProvider;

        public ReportProvider(ApplicationDbContext db,
                              IMapper mapper,
                              IStudentProvider studentProvider,
                              ICacheProvider cacheProvider,
                              IGradeProvider gradeProvider,
                              IAcademicProvider academicProvider,
                              ICurriculumProvider curriculumProvider,
                              IDateTimeProvider dateTimeProvider) : base(db, mapper)
        {
            _studentProvider = studentProvider;
            _cacheProvider = cacheProvider;
            _gradeProvider = gradeProvider;
            _academicProvider = academicProvider;
            _curriculumProvider = curriculumProvider;
            _dateTimeProvider = dateTimeProvider;
        }

        #region Transcript
        public TranscriptInformation GetTranscript(Student student, string language, bool isPreview)
        {
            TranscriptInformation model = new TranscriptInformation();   
            if (isPreview)
            {
                var result = new List<TranscriptTerm>();
                var registrationCourses = _studentProvider.GetStudentRegistrationCourseViewModel(student.Id);
                var registrationCoursesTranfer = _studentProvider.GetStudentRegistrationCourseTranferViewModel(student.Id);
                var registrationCoursesTranferWithGrade = _studentProvider.GetStudentRegistrationCourseTranferWithGradeViewModel(student.Id);
                decimal cumulativeCreditRegis = 0;
                decimal cumulativeCreditRegisFromCredit = 0;
                foreach(var item in registrationCourses)
                {
                    cumulativeCreditRegis += item.TotalRegistrationCredit;
                    cumulativeCreditRegisFromCredit += item.TotalRegistrationCreditFromCredit;

                    var transaction = new TranscriptTerm
                                      {
                                          TermId = item.TermId,
                                          AcademicYear = language == "en" ? item.AcademicYear
                                                                          : item.AcademicYear + 543,
                                          AcademicTerm = item.AcademicTerm,
                                          IsSummer = item.IsSummer,
                                          CumulativeGPA = item.CumulativeGPA,
                                          TotalCredit = item.TotalCredit,
                                          TotalRegistrationCredit = item.TotalRegistrationCredit,
                                          TotalRegistrationCreditFromCredit = item.TotalRegistrationCreditFromCredit,
                                          TermGPATranscript = item.TermGPA,
                                          CumulativeCreditRegis = cumulativeCreditRegis,
                                          CumulativeCreditRegisFromCredit = cumulativeCreditRegisFromCredit,
                                          CumulativeCreditComp = item.CumulativeCreditComp,
                                          CumulativeGPTS = item.CumulativeGTPS,
                                          GPTS = item.TotalGPTS,
                                          TranscriptCourses = item.RegistrationCourses != null ? item.RegistrationCourses.Where(x => x.IsPaid)
                                                                                                                         .Select(x => new TranscriptCourse
                                                                                                                                    {
                                                                                                                                         CourseCode = x.Course.Code,
                                                                                                                                         CourseName1 = language == "en" ? x.Course.TranscriptNameEn1
                                                                                                                                                                        : x.Course.TranscriptNameTh1,
                                                                                                                                         CourseName2 = language == "en" ? x.Course.TranscriptNameEn2
                                                                                                                                                                        : x.Course.TranscriptNameTh2,
                                                                                                                                         CourseName3 = language == "en" ? x.Course.TranscriptNameEn3
                                                                                                                                                                        : x.Course.TranscriptNameTh3,
                                                                                                                                         CreditText = x.Course.CreditText,
                                                                                                                                         Credit = x.Course.Credit,
                                                                                                                                         RegistrationCredit = x.Course.RegistrationCredit,
                                                                                                                                         Grade = string.IsNullOrEmpty(x.GradeName) ? "W" : x.GradeName.ToUpper() + (x.IsStarCourse ? "*" : ""),
                                                                                                                                         GradeWeight = x.Grade?.Weight ?? 0,
                                                                                                                                         Section = string.IsNullOrEmpty(x.Section?.Number) ? "" : x.Section.Number,
                                                                                                                                         TrasferUniversityId = x.TransferUniversityId
                                                                                                                                    })
                                                                                                                         .ToList()
                                                                                              : new List<TranscriptCourse>()
                                      };
                    result.Add(transaction);
                }
                model.TranscriptTerms = result;
                result = new List<TranscriptTerm>();

                foreach(var item in registrationCoursesTranfer)
                {
                    var transaction = new TranscriptTerm
                                      {
                                          AcademicTerm = item.AcademicTerm,
                                          TotalCredit = item.TotalCredit,
                                          TotalRegistrationCredit = 0,
                                          TranscriptCourses = item.RegistrationCourses != null ? item.RegistrationCourses.Select(x => new TranscriptCourse
                                                                                                                                    {
                                                                                                                                         CourseCode = x.Course.Code,
                                                                                                                                         CourseName1 = language == "en" ? x.Course.TranscriptNameEn1
                                                                                                                                                                        : x.Course.TranscriptNameTh1,
                                                                                                                                         CourseName2 = language == "en" ? x.Course.TranscriptNameEn2
                                                                                                                                                                        : x.Course.TranscriptNameTh2,
                                                                                                                                         CourseName3 = language == "en" ? x.Course.TranscriptNameEn3
                                                                                                                                                                        : x.Course.TranscriptNameTh3,
                                                                                                                                         CreditText = x.Course.CreditText,
                                                                                                                                         Credit = x.Course.Credit,
                                                                                                                                         RegistrationCredit = x.Course.RegistrationCredit,
                                                                                                                                         Grade = string.IsNullOrEmpty(x.GradeName) ? "W" : x.GradeName.ToUpper() + (x.IsStarCourse ? "*" : ""),
                                                                                                                                         GradeWeight = x.Grade?.Weight ?? 0,
                                                                                                                                         TrasferUniversityId = x.TransferUniversityId
                                                                                                                                    })
                                                                                                                         .ToList()
                                                                                              : new List<TranscriptCourse>()
                                      };
                    result.Add(transaction);
                }

                model.Transfer = result;
                result = new List<TranscriptTerm>();

                foreach(var item in registrationCoursesTranferWithGrade)
                {
                    var transaction = new TranscriptTerm
                                      {
                                          TermId = item.TermId,
                                          AcademicYear = language == "en" ? item.AcademicYear
                                                                          : item.AcademicYear + 543,
                                          AcademicTerm = item.AcademicTerm,
                                          IsSummer = item.IsSummer,
                                          CumulativeGPA = item.CumulativeGPA,
                                          TotalCredit = item.TotalCredit,
                                          TotalRegistrationCredit = item.TotalRegistrationCredit,
                                          TermGPATranscript = item.TermGPA,
                                          CumulativeCreditRegis = cumulativeCreditRegis,
                                          CumulativeCreditComp = item.CumulativeCreditComp,
                                          GPTS = item.TotalGPTS,
                                          TranscriptCourses = item.RegistrationCourses != null ? item.RegistrationCourses.Select(x => new TranscriptCourse
                                                                                                                                    {
                                                                                                                                         CourseCode = x.Course.Code,
                                                                                                                                         CourseName1 = language == "en" ? x.Course.TranscriptNameEn1
                                                                                                                                                                        : x.Course.TranscriptNameTh1,
                                                                                                                                         CourseName2 = language == "en" ? x.Course.TranscriptNameEn2
                                                                                                                                                                        : x.Course.TranscriptNameTh2,
                                                                                                                                         CourseName3 = language == "en" ? x.Course.TranscriptNameEn3
                                                                                                                                                                        : x.Course.TranscriptNameTh3,
                                                                                                                                         CreditText = x.Course.CreditText,
                                                                                                                                         Credit = x.Course.Credit,
                                                                                                                                         RegistrationCredit = x.Course.RegistrationCredit,
                                                                                                                                         Grade = string.IsNullOrEmpty(x.GradeName) ? "W" : x.GradeName.ToUpper() + (x.IsStarCourse ? "*" : ""),
                                                                                                                                         GradeWeight = x.Grade?.Weight ?? 0,
                                                                                                                                         Section = string.IsNullOrEmpty(x.Section?.Number) ? "" : x.Section.Number,
                                                                                                                                         TrasferUniversityId = x.TransferUniversityId
                                                                                                                                    })
                                                                                                                         .ToList()
                                                                                              : new List<TranscriptCourse>()
                                      };
                    result.Add(transaction);
                }
                model.TransferWithCourse = result;
            }
            
            
            model.Language = language;
            model.StudentId = student.Id;
            model.StudentCode = student.Code;
            model.StudentStatus = student.StudentStatus;
            model.IsGraduated = student.GraduationInformation?.IsActive ?? false;
            model.GraduateClass = student.GraduationInformation?.Class;
            model.TotalCreditCompleted = student.AcademicInformation?.CreditComp ?? 0;
            model.TotalCreditAudited = student.RegistrationCourses.Where(x => x.GradeId != null && x.Grade.Name.ToLower().Contains("au")).Sum(x => x.Course.Credit);
            model.TotalCreditExempted = student.AcademicInformation?.CreditExempted ?? 0;
            model.TotalCreditTransferred = student.RegistrationCourses.Where(x => x.Status != "d"                                                                                                                      
                                                                                  && x.IsTransferCourse
                                                                                  && x.IsGradePublished
                                                                                  && x.IsActive)
                                                                      .Sum(x => x.Course.Credit);
            model.TotalFoundationCourseCredit = student.RegistrationCourses.Where(x => x.Course.IsIntensiveCourse).Sum(x => x.Course.Credit);
            model.TotalCreditEarnd = student.AcademicInformation?.CreditEarned ?? 0;
            model.CurriculumId = student.AcademicInformation.CurriculumVersion.CurriculumId;
            model.CreatedAt = DateTime.UtcNow.AddHours(7);
            model.ProfileImageURL = student.ProfileImageURL;
            model.TransferedUniversity = _studentProvider.GetTransferUniversityByStudentCode(student.Code);
            model.BirthDateRender = student.BirthDate;

            var currriculum = student.CurriculumInformations.Any()
                              ? student.CurriculumInformations.Where(x => x.IsActive)
                                                              .FirstOrDefault()
                              : null;

            model.CurriculumVersionId = currriculum == null ? 0 : currriculum.CurriculumVersionId ?? 0;

            var address = student.StudentAddresses == null ? null : student.StudentAddresses.FirstOrDefault(x => x.Type == "c");
            var curriculumInformationDetail = _curriculumProvider.GetMinorAndConcentration(student.Id, language);

            var resignInfo = _db.ResignStudents.Where(x => x.StudentId == student.Id).OrderByDescending(x => x.UpdatedAt).FirstOrDefault();

            if (student.StudentStatus.StartsWith("g"))
            {
                model.StatusAt = student.GraduationInformations?.Where(x => x.IsActive)?.FirstOrDefault()?.GraduatedAt;
            }
            else if (student.StudentStatus == "rs" && resignInfo != null)
            {
                model.StatusAt = resignInfo.ApprovedAt;
            }
            else if(student.StudentStatusLogs != null && student.StudentStatusLogs.Count > 0 && student.StudentStatusLogs.Any(x => x.StudentStatus == student.StudentStatus))
            {
                model.StatusAt = student.StudentStatusLogs.Where(x => x.StudentStatus == student.StudentStatus)?.OrderByDescending(x => x.Id)?.FirstOrDefault()?.EffectiveAt;
            }

            if (language == "en")
            {
                model.Gender = student.GenderText;
                model.FirstName = $"{ student.Title.NameEn } { student.FirstNameEn }";
                model.MidName = student.MidNameEn ?? "";
                model.LastName = student.LastNameEn ?? "";
                model.PreviousGraduatedYear = student.AdmissionInformation?.PreviousGraduatedYear?.ToString() ?? "---";
                model.AdmissionAt = student.AdmissionInformation?.AdmissionDateText ?? "---";
                model.GraduatedAt = student.GraduationInformation?.GraduatedAtFullText;
                model.GraduateTerm = student.GraduationInformation?.Term?.TermText ?? "---";
                model.BirthPlace = student.BirthProvince == null ? student.BirthState?.NameEn ?? "-"
                                                                 : student.BirthProvince?.NameEn ?? "-";
                model.Address1 = address == null ? "---" : $"{ address.HouseNumber } { address.RoadEn }";
                model.Address2 = address == null ? "---"
                                                 : address.Subdistrict == null ? $"{ address.City?.NameEn ?? "" } { address.State?.NameEn ?? "" }"
                                                                               : $"{ address.Subdistrict?.NameEn ?? "" } { address.District?.NameEn ?? ""}";
                model.Address3 = address == null ? "---"
                                                 : address.Province == null ? $"{ address.Country?.NameEn ?? "" } { address.ZipCode }"
                                                                            : $"{ address.Province?.NameEn ?? "" } { address.ZipCode }";

                model.PhoneNumber = student.TelephoneNumber1;
                model.Nationality = student.Nationality?.NameEn ?? "---";
                model.Religion = student.Religion?.NameEn ?? "---";
                model.AcademicLevel = student.AcademicInformation?.AcademicLevel?.NameEn ?? "---";
                model.EducationBackground = student.AdmissionInformation?.EducationBackground?.NameEn ?? "---";
                model.PreviousSchool = student.AdmissionInformation?.PreviousSchool?.NameEn ?? "---";
                model.PreviousSchoolCountry = student.AdmissionInformation?.PreviousSchool?.Country?.NameEn ?? "---";
                model.CurriculumVersion = curriculumInformationDetail.CurriculumVersion ?? "---";
                model.Minor = curriculumInformationDetail.Minor ?? "---";
                model.Concentration = curriculumInformationDetail.Concentration ?? "---";
                model.Award = student.GraduationInformation?.AcademicHonor?.NameEn ?? "";
                model.Faculty = currriculum == null ? "---" : currriculum.Faculty?.NameEn;
                model.Department = currriculum == null ? "---" : currriculum.Department?.NameEn;
                model.Degree = currriculum == null ? "---" : currriculum.CurriculumVersion?.DegreeNameEn;
                model.Curriculum = currriculum == null ? "---" : currriculum.CurriculumVersion?.Curriculum?.NameEn;
            }
            else //th
            {
                model.FirstName = $"{ student.Title.NameTh } { student.FirstNameTh }";
                model.MidName = student.MidNameTh;
                model.LastName = student.LastNameTh;
                model.PreviousGraduatedYear = (student.AdmissionInformation?.PreviousGraduatedYear + 543)?.ToString() ?? "---";
                model.AdmissionAt = student.AdmissionInformation?.AdmissionDateThText ?? "---";
                model.GraduatedAt = student.GraduationInformation?.GraduatedAtThFullText;
                model.GraduateTerm = student.GraduationInformation?.Term?.TermThText ?? "---";
                model.BirthPlace = student.BirthProvince == null ? student.BirthState?.NameTh ?? "---"
                                                                 : student.BirthProvince?.NameTh ?? "---";
                model.Address1 = address == null ? "---" : $"{ address.HouseNumber } { address.RoadTh }";
                model.Address2 = address == null ? "---"
                                                 : address.Subdistrict == null ? $"{ address.City?.NameTh ?? "" } { address.State?.NameTh ?? "" }"
                                                                               : $"{ address.Subdistrict?.NameTh ?? "" } { address.District?.NameTh ?? "" }";
                model.Address3 = address == null ? "---"
                                                 : address.Province == null ? $"{ address.Country?.NameTh ?? "" } { address.ZipCode }"
                                                                            : $"{ address.Province?.NameTh ?? "" } { address.ZipCode }";
                model.Nationality = student.Nationality?.NameTh ?? "---";
                model.Religion = student.Religion?.NameTh ?? "---";
                model.AcademicLevel = student.AcademicInformation?.AcademicLevel?.NameTh ?? "---";
                model.EducationBackground = student.AdmissionInformation?.EducationBackground?.NameTh ?? "---";
                model.PreviousSchool = student.AdmissionInformation?.PreviousSchool?.NameTh ?? "---";
                model.PreviousSchoolCountry = student.AdmissionInformation?.PreviousSchool?.Country?.NameTh ?? "---";
                model.CurriculumVersion = curriculumInformationDetail.CurriculumVersion ?? "---";
                model.Minor = curriculumInformationDetail.Minor ?? "---";
                model.Concentration = curriculumInformationDetail.Concentration ?? "---";
                model.Award = student.GraduationInformation?.AcademicHonor?.NameTh ?? "";
                model.Faculty = currriculum == null ? "---" : currriculum.Faculty?.NameTh;
                model.Department = currriculum == null ? "---" : currriculum.Department?.NameTh;
                model.Degree = currriculum == null ? "---" : currriculum.CurriculumVersion?.DegreeNameTh;
                model.Curriculum = currriculum == null ? "---" : currriculum.CurriculumVersion?.Curriculum?.NameTh;
            }

            return model;
        }

        public TranscriptInformation MapTranscriptPreview(TranscriptInformation transcript, TranscriptViewModel model)
        {
            var transcriptUpdate = model.Transcripts.SingleOrDefault(x => x.StudentCode == transcript.StudentCode);
            if(transcriptUpdate != null)
            {
                transcript.BirthDateRender = transcriptUpdate.BirthDateRender;
                transcript.StatusAt = transcriptUpdate.StatusAt;
                transcript.Nationality = transcriptUpdate.Nationality;
                transcript.Faculty = transcriptUpdate.Faculty;
                transcript.Department = transcriptUpdate.Department;
                transcript.Curriculum = transcriptUpdate.Curriculum;
                transcript.Minor = transcriptUpdate.Minor ?? "";
                transcript.SecondMinor = transcriptUpdate.SecondMinor ?? "";
                transcript.Concentration = transcriptUpdate.Concentration ?? "";
                transcript.SecondConcentration = transcriptUpdate.SecondConcentration ?? "";
                transcript.Degree = transcriptUpdate.Degree;
                transcript.Award = transcriptUpdate.Award ?? "";
                transcript.TotalCreditCompleted = transcriptUpdate.TotalCreditCompleted;
                transcript.TotalCreditTransferred = transcriptUpdate.TotalCreditTransferred;
                transcript.TotalCreditEarnd = transcriptUpdate.TotalCreditEarnd;
                transcript.StudentStatus = transcriptUpdate.StudentStatus;
                transcript.Award = transcriptUpdate.Award ?? "";                
            }
            // foreach (var item in model.Transcripts)
            // {
            //     transcript.StudentCode = item.StudentCode;
            //     transcript.FirstName = item.FirstName;
            //     transcript.MidName = item.MidName;
            //     transcript.LastName = item.LastName;
            //     transcript.Address1 = item.Address1;
            //     transcript.Address2 = item.Address2;
            //     transcript.Address3 = item.Address3;
            //     transcript.BirthDate = item.BirthDate;
            //     transcript.BirthDateRender = _dateTimeProvider.ConvertStringToDateTime(item.BirthDate);
            //     transcript.BirthPlace = item.BirthPlace;
            //     transcript.Nationality = item.Nationality;
            //     transcript.Religion = item.Religion;
            //     transcript.AdmissionAt = item.AdmissionAt;
            //     transcript.AdmissionAtRender = _dateTimeProvider.ConvertStringToDateTime(item.AdmissionAt);
            //     transcript.AcademicLevel = item.AcademicLevel;
            //     transcript.EducationBackground = item.EducationBackground;
            //     transcript.PreviousSchool = item.PreviousSchool;
            //     transcript.PreviousSchoolCountry = item.PreviousSchoolCountry;
            //     transcript.PreviousGraduatedYear = item.PreviousGraduatedYear;
            //     transcript.Faculty = item.Faculty;
            //     transcript.Department = item.Department;
            //     transcript.Curriculum = item.Curriculum;
            //     transcript.Minor = item.Minor ?? "";
            //     transcript.SecondMinor = item.SecondMinor ?? "";
            //     transcript.Concentration = item.Concentration ?? "";
            //     transcript.SecondConcentration = item.SecondConcentration ?? "";
            //     transcript.GraduatedAt = item.GraduatedAt;
            //     transcript.GraduatedAtRender = _dateTimeProvider.ConvertStringToDateTime(item.GraduatedAt);
            //     transcript.Degree = item.Degree;
            //     transcript.Award = item.Award ?? "";
            //     transcript.TotalCreditCompleted = item.TotalCreditCompleted;
            //     transcript.TotalCreditTransferred = item.TotalCreditTransferred;
            //     transcript.TotalCreditEarnd = item.TotalCreditEarnd;
            // }

            return transcript;
        }

        private decimal CalculateGPTS(List<TranscriptCourse> transcriptCourses, List<Grade> grades)
        {
            decimal gpts = 0;
            foreach(var transcripCourse in transcriptCourses)
            {
                var grade = grades.SingleOrDefault(x => x.Name == transcripCourse.Grade);
                if (grade != null && grade.IsCalculateGPA)
                {
                    gpts += (grade.Weight ?? 0) * transcripCourse.Credit;
                }
            }

            return gpts;
        }

        private decimal CalculateCumulativeGPA(decimal totalGradeWeight, decimal totalCredit)
        {
            return totalCredit == 0 ? 0 : Decimal.Round(totalGradeWeight/totalCredit, 2);
        }
        #endregion

        #region Reference Number
        public List<PrintingLog> GetPrintingLogs(int year)
        {
            var printingLogs = _db.PrintingLogs.Where(x => x.Year == year)
                                               .OrderByDescending(x => x.RunningNumber)
                                               .ToList();
            return printingLogs;
        }

        public int GetNewRunningNumber(int year)
        {
            var printingLog = GetPrintingLogs(year).FirstOrDefault();
            if (printingLog == null)
            {
                return 1;
            }
            else
            {
                var runningNumber = printingLog.Year < year ? 1 : printingLog.RunningNumber + 1;
                return runningNumber;
            }
        }
        
        public string GetNewReferenceNumber(int year, string language = "en")
        {
            var runningCode = GetNewRunningNumber(year).ToString();
            var referenceNumber =  $"{ runningCode.PadLeft(5, '0') }/{ GetYear(year, language) }";
            return referenceNumber;
        }

        public int GetYear(int year, string language = "en")
        {
            if (language == "th")
            {
                return year + 543;
            }
            else
            {
                return year;
            }
        }
        #endregion

        public string GetSignatoryPositionById(long id, string language = "en")
        {
            var position = _db.Signatories.Where(x => x.Id == id)
                                          .Select(x => language == "th" ? x.PositionTh : x.PositionEn)
                                          .IgnoreQueryFilters()
                                          .SingleOrDefault();
            return position;
        }

        public string GetSignatoryNameById(long id, string language = "en")
        {
            var name = _db.Signatories.Where(x => x.Id == id)
                                      .Select(x => language == "th" ? x.FullNameTh : x.FullNameEn)
                                      .IgnoreQueryFilters()
                                      .SingleOrDefault();
            return name;
        }

        public List<string> GetSignatoriesByIds(List<long> ids, string language = "en")
        {
            var signatories = _db.Signatories.Where(x => ids.Contains(x.Id))
                                             .Select(x => language == "th" ? x.FullNameTh : x.FullNameEn)
                                             .ToList();
            return signatories;
        }

        public List<string> GetPositionsBySignatoryIds(List<long> ids, string language = "en")
        {
            var positions = _db.Signatories.Where(x => ids.Contains(x.Id))
                                           .Select(x => language == "th" ? x.PositionTh : x.PositionEn)
                                           .ToList();
            return positions;
        }

        public string GetCountryById(long id, string language = "en")
        {
            var country = _db.Countries.Where(x => x.Id == id)
                                       .Select(x => language == "th" ? x.NameTh : x.NameEn)
                                       .IgnoreQueryFilters()
                                       .SingleOrDefault();
            return country;
        }

        public Student GetStudentInformationForTranscript(string studentCode, Criteria criteria)
        {
            var student = _db.Students.Include(x => x.AdmissionInformation)
                                          .ThenInclude(x => x.PreviousSchool)
                                          .ThenInclude(x => x.Country)
                                      .Include(x => x.AdmissionInformation)
                                          .ThenInclude(x => x.EducationBackground)
                                      .Include(x => x.AcademicInformation)
                                          .ThenInclude(x => x.AcademicLevel)
                                      .Include(x => x.AcademicInformation)
                                          .ThenInclude(x => x.Department)
                                          .ThenInclude(x => x.Faculty)
                                      .Include(x => x.AcademicInformation)
                                          .ThenInclude(x => x.CurriculumVersion)
                                          .ThenInclude(x => x.Curriculum)
                                      .Include(x => x.StudentAddresses)
                                          .ThenInclude(x => x.Subdistrict)
                                          .ThenInclude(x => x.District)
                                          .ThenInclude(x => x.Province)
                                          .ThenInclude(x => x.Country)
                                      .Include(x => x.StudentAddresses)
                                          .ThenInclude(x => x.City)
                                          .ThenInclude(x => x.State)
                                          .ThenInclude(x => x.Country)
                                      .Include(x => x.GraduationInformations)
                                          .ThenInclude(x => x.AcademicHonor)
                                      .Include(x => x.GraduationInformations)
                                          .ThenInclude(x => x.Term)
                                      .Include(x => x.CurriculumInformations)
                                          .ThenInclude(x => x.SpecializationGroupInformations)
                                          .ThenInclude(x => x.SpecializationGroup)
                                      .Include(x => x.Nationality)
                                      .Include(x => x.Religion)
                                      .Include(x => x.Title)
                                      .Include(x => x.BirthProvince)
                                      .Include(x => x.BirthState)
                                      .Include(x => x.StudentStatusLogs)
                                      .Include(x => x.RegistrationCourses)
                                          .ThenInclude(x => x.Course)
                                      .Include(x => x.RegistrationCourses)
                                          .ThenInclude(x => x.Grade)
                                      .IgnoreQueryFilters()
                                      .SingleOrDefault(x => x.Code == studentCode
                                                            && (criteria.AcademicLevelId == 0
                                                                || criteria.AcademicLevelId == x.AcademicInformation.AcademicLevelId)
                                                            && (criteria.GraduatedClass == 0
                                                                || x.GraduationInformations == null
                                                                || x.GraduationInformations.Any(y => y.ClassInt == criteria.GraduatedClass))
                                                            && (criteria.FacultyId == 0
                                                                || criteria.FacultyId == x.AcademicInformation.FacultyId)
                                                            && (criteria.DepartmentId == 0
                                                                || criteria.DepartmentId == x.AcademicInformation.DepartmentId));
            return student;
        }

        public string GetChangeNameType(string type, string language = "en")
        {
            if (type == "name")
            {
                return language == "th" ? "เปลี่ยนชื่อเป็น" : "name has been changed to";
            }
            else if (type == "surname")
            {
                return language == "th" ? "เปลี่ยนนามสกุลเป็น" : "surname has been changed to";
            }
            else if (type == "name and surname")
            {
                return language == "th" ? "เปลี่ยนชื่อและสกุลเป็น" : "name and surname has been changed to";
            }
            else
            {
                return language == "th" ? "เปลี่ยนตัวสะกดเป็น" : "name is now spelling as";
            }
        }

        #region Students
        public ApplicantsByAdmissionRoundViewModel GetApplicantsByAdmissionRounds(Criteria criteria) 
        {
            var result = new ApplicantsByAdmissionRoundViewModel();
            result.Criteria = criteria;
            var startTerm = _academicProvider.GetTerm(criteria.StartTermId);
            var endTerm = _academicProvider.GetTerm(criteria.EndTermId);
            var studentsByDepartmentAndStudentId = (from student in _db.Students
                                                    join admissionInfo in _db.AdmissionInformations on student.Id equals admissionInfo.StudentId
                                                    join academicInfo in _db.AcademicInformations on student.Id equals academicInfo.StudentId
                                                    join admissionRound in _db.AdmissionRounds on admissionInfo.AdmissionRoundId equals admissionRound.Id
                                                    join admissionTerm in _db.Terms on admissionRound.AdmissionTermId equals admissionTerm.Id
                                                    join department in _db.Departments on academicInfo.DepartmentId equals department.Id
                                                    join faculty in _db.Faculties on academicInfo.FacultyId equals faculty.Id
                                                    join regisCourse in _db.RegistrationCourses on student.Id equals regisCourse.StudentId into regisCourseTmp
                                                    from regisCourse in regisCourseTmp.DefaultIfEmpty()
                                                    join course in _db.Courses on regisCourse.CourseId equals course.Id into courseTmp
                                                    from course in courseTmp.DefaultIfEmpty()
                                                    where (admissionTerm.AcademicYear > startTerm.AcademicYear
                                                           || (admissionTerm.AcademicYear == startTerm.AcademicYear && admissionTerm.AcademicTerm >= startTerm.AcademicTerm))
                                                          && (admissionTerm.AcademicYear < endTerm.AcademicYear
                                                              || (admissionTerm.AcademicYear == endTerm.AcademicYear && admissionTerm.AcademicTerm <= endTerm.AcademicTerm))
                                                          && (criteria.DepartmentId == 0 || academicInfo.DepartmentId == criteria.DepartmentId)
                                                          && (criteria.FacultyId == 0 || academicInfo.FacultyId == criteria.FacultyId)
                                                          && academicInfo.AcademicLevelId == criteria.AcademicLevelId
                                                          && admissionInfo.AdmissionTermId != 0
                                                    group new { student, department, faculty, admissionInfo, admissionRound, admissionTerm, regisCourse, course } 
                                                    by new { StudentId = student.Id } into grp
                                                    select new
                                                           {
                                                               Faculty = grp.FirstOrDefault().faculty,
                                                               Department = grp.FirstOrDefault().department,
                                                               AdmissionInformation = grp.FirstOrDefault().admissionInfo,
                                                               AdmissionRound = grp.FirstOrDefault().admissionRound,
                                                               AdmissionTerm = grp.FirstOrDefault().admissionTerm,
                                                               Student = grp.FirstOrDefault().student,
                                                               Courses = grp.Select(x => x.course).ToList()
                                                           }).OrderBy(x => x.Faculty.NameEn)
                                                             .ThenBy(x => x.Department.NameEn)
                                                             .ToList();

            var studentsByDepartment = (from data in studentsByDepartmentAndStudentId
                                        group data by data.Department.Id into grp
                                        select new
                                               {
                                                   Faculty = grp.FirstOrDefault().Faculty,
                                                   Department = grp.FirstOrDefault().Department,
                                                   GroupDataList = grp.ToList()
                                               }).ToList();

            result.TermDetails = (from term in _db.Terms
                                 join round in _db.AdmissionRounds on term.Id equals round.AdmissionTermId
                                 where term.AcademicLevelId == criteria.AcademicLevelId
                                       && (term.AcademicYear > startTerm.AcademicYear
                                           || (term.AcademicYear == startTerm.AcademicYear && term.AcademicTerm >= startTerm.AcademicTerm))
                                       && (term.AcademicYear < endTerm.AcademicYear
                                           || (term.AcademicYear == endTerm.AcademicYear && term.AcademicTerm <= endTerm.AcademicTerm))
                                 group new { term, round }
                                 by term.Id into termGrp
                                 select new TermDetail 
                                        {
                                            TermId = termGrp.FirstOrDefault().term.Id,
                                            TermName = termGrp.FirstOrDefault().term.TermText,
                                            TermNameForSort = termGrp.FirstOrDefault().term.AcademicYear + "/" + termGrp.FirstOrDefault().term.AcademicTerm,
                                            Rounds = termGrp.Select(x => x.round).OrderBy(x => x.Round).ToList(),
                                            RoundColspan = (termGrp.Select(x => x.round).Count() + 1) * 3 + 3,
                                            RoundTotalColName = "Round Total",
                                            ReEnterColName = "RE-ENTER",
                                            RoundTotalAndReEnterColName = "Round Total + RE-ENTER"
                                        }).OrderByDescending(x => x.TermNameForSort)
                                          .ToList();

            var currentFacultyName = "";
            foreach (var item in studentsByDepartment) 
            {
                if (currentFacultyName == "" || (currentFacultyName != "" && currentFacultyName != item.Faculty.NameEn))
                {
                    var facultyRow = new ApplicantsByAdmissionRoundDetail();
                    facultyRow.FacultyId = item.Faculty.Id;
                    facultyRow.FacultyName = item.Faculty.NameEn;
                    facultyRow.IsFaculty = true;
                    result.ApplicantsByAdmissionRoundDetails.Add(facultyRow);
                    currentFacultyName = item.Faculty.NameEn;
                }

                var row = new ApplicantsByAdmissionRoundDetail();
                row.DepartmentId = item.Department.Id;
                row.DepartmentName = item.Department.NameEn;
                row.FacultyId = item.Faculty.Id;
                row.FacultyName = item.Faculty.NameEn;
                row.IsFaculty = false;
                foreach (var term in result.TermDetails)
                {
                    var applyStudentSum = 0;
                    var intensiveStudentSum = 0;
                    var registrationSum = 0;
                    foreach (var round in term.Rounds) 
                    {
                        var roundDetail = new StudentInRound();
                        roundDetail.ApplyStudent = item.GroupDataList.Where(x => x.AdmissionTerm.Id == term.TermId && x.AdmissionRound.Id == round.Id).Select(x => x.Student.Id).Count();
                        roundDetail.IntensiveStudent = item.GroupDataList.Where(x => x.AdmissionTerm.Id == term.TermId && x.AdmissionRound.Id == round.Id && x.Courses.Any(y => y != null && y.IsIntensiveCourse)).Count();
                        roundDetail.Registration = item.GroupDataList.Where(x => x.AdmissionTerm.Id == term.TermId && x.AdmissionRound.Id == round.Id && x.Courses.Any(y => y != null && !y.IsIntensiveCourse)).Count();

                        applyStudentSum += roundDetail.ApplyStudent;
                        intensiveStudentSum += roundDetail.IntensiveStudent;
                        registrationSum += roundDetail.Registration;

                        row.StudentInRounds.Add(roundDetail);
                    }

                    var roundDetailSum = new StudentInRound();
                    roundDetailSum.IsSummary = true;
                    roundDetailSum.ApplyStudent = applyStudentSum;
                    roundDetailSum.IntensiveStudent = intensiveStudentSum;
                    roundDetailSum.Registration = registrationSum;
                    roundDetailSum.ReEnterApplyStudent = 0;
                    roundDetailSum.ReEnterRegistration = 0;
                    roundDetailSum.TotalRegistration = roundDetailSum.ReEnterRegistration + roundDetailSum.Registration;
                    row.StudentInRounds.Add(roundDetailSum);
                }

                result.ApplicantsByAdmissionRoundDetails.Add(row);
            }

            // Sum each faculty
            foreach (var facultyRow in result.ApplicantsByAdmissionRoundDetails.Where(x => x.IsFaculty)) 
            {
                var departmentRows = result.ApplicantsByAdmissionRoundDetails.Where(x => !x.IsFaculty && x.FacultyId == facultyRow.FacultyId).ToList();
                var roundCount = departmentRows.FirstOrDefault().StudentInRounds.Count;
                for (int i = 0; i < roundCount; i++)
                {
                    var roundDetail = new StudentInRound();
                    roundDetail.ApplyStudent = departmentRows.Select(x => x.StudentInRounds[i]).Sum(x => x.ApplyStudent);
                    roundDetail.IntensiveStudent = departmentRows.Select(x => x.StudentInRounds[i]).Sum(x => x.IntensiveStudent);
                    roundDetail.Registration = departmentRows.Select(x => x.StudentInRounds[i]).Sum(x => x.Registration);
                    if (departmentRows.Select(x => x.StudentInRounds[i]).FirstOrDefault().IsSummary) 
                    {
                        roundDetail.IsSummary = true;
                        roundDetail.ReEnterApplyStudent = departmentRows.Select(x => x.StudentInRounds[i]).Sum(x => x.ReEnterApplyStudent);
                        roundDetail.ReEnterRegistration = departmentRows.Select(x => x.StudentInRounds[i]).Sum(x => x.ReEnterRegistration);
                        roundDetail.TotalRegistration = departmentRows.Select(x => x.StudentInRounds[i]).Sum(x => x.TotalRegistration);
                    }
                    
                    facultyRow.StudentInRounds.Add(roundDetail);
                }
            }
            
            return result;
        }

        public CurriculumVersionReportViewModel GetCurriculumVersionReport(long curriculumVersionId) 
        {
            var result = new CurriculumVersionReportViewModel();
            var curriculumInfo = (from version in _db.CurriculumVersions
                                  join curriculum in _db.Curriculums on version.CurriculumId equals curriculum.Id
                                  join level in _db.AcademicLevels on curriculum.AcademicLevelId equals level.Id
                                  join faculty in _db.Faculties on curriculum.FacultyId equals faculty.Id
                                  join department in _db.Departments on curriculum.DepartmentId equals department.Id
                                  join program in _db.AcademicPrograms on version.AcademicProgramId equals program.Id into programTmp
                                  from program in programTmp.DefaultIfEmpty()
                                  join implementedTerm in _db.Terms on version.ImplementedTermId equals implementedTerm.Id into implementedTermTmp
                                  from implementedTerm in implementedTermTmp.DefaultIfEmpty()
                                  join openedTerm in _db.Terms on version.OpenedTermId equals openedTerm.Id into openedTermTmp
                                  from openedTerm in openedTermTmp.DefaultIfEmpty()
                                  join closedTerm in _db.Terms on version.ClosedTermId equals closedTerm.Id into closedTermTmp
                                  from closedTerm in closedTermTmp.DefaultIfEmpty()
                                  where version.Id == curriculumVersionId
                                  select new 
                                         {
                                             CurriculumVersion = version,
                                             Curriculum = curriculum,
                                             AcademicLevel = level,
                                             Faculty = faculty,
                                             Department = department,
                                             ImplementedTerm = implementedTerm,
                                             OpenedTerm = openedTerm,
                                             ClosedTerm = closedTerm,
                                             AcademicProgram = program
                                         }).FirstOrDefault();

            if (curriculumInfo != null)
            {
                result.CurriculumId = curriculumInfo.Curriculum.Id;
                result.CurriculumName = curriculumInfo.Curriculum.NameEn;
                result.ReferenceCode = curriculumInfo.Curriculum.ReferenceCode;
                result.AcademicLevelId = curriculumInfo.AcademicLevel.Id;
                result.AcademicLevel = curriculumInfo.AcademicLevel.NameEn;
                result.AbbreviationEn = curriculumInfo.Curriculum.AbbreviationEn;
                result.AbbreviationTh = curriculumInfo.Curriculum.AbbreviationTh;
                result.FacultyName = curriculumInfo.Faculty.NameEn;
                result.DepartmentName = curriculumInfo.Department.NameEn;
                result.TermTypeText = curriculumInfo.Curriculum.TermTypeText;
                result.MinimumGPAText = curriculumInfo.Curriculum.MinimumGPAText;
                
                result.CurriculumVersionId = curriculumInfo.CurriculumVersion.Id;
                result.CurriculumVersionCode = curriculumInfo.CurriculumVersion.Code;
                result.CurriculumVersionNameEn = curriculumInfo.CurriculumVersion.NameEn;
                result.CurriculumVersionNameTh = curriculumInfo.CurriculumVersion.NameTh;
                result.DegreeNameEn = curriculumInfo.CurriculumVersion.DegreeNameEn;
                result.DegreeNameTh = curriculumInfo.CurriculumVersion.DegreeNameTh;
                result.DegreeAbbreviationEn = curriculumInfo.CurriculumVersion.DegreeAbbreviationEn;
                result.DegreeAbbreviationTh = curriculumInfo.CurriculumVersion.DegreeAbbreviationTh;
                result.ImplementedTerm = curriculumInfo.ImplementedTerm?.TermText ?? "N/A";
                result.OpenedTerm = curriculumInfo.OpenedTerm?.TermText ?? "N/A";
                result.ClosedTerm = curriculumInfo.ClosedTerm?.TermText ?? "N/A";
                result.MinimumTerm = curriculumInfo.CurriculumVersion.MinimumTerm;
                result.MaximumTerm = curriculumInfo.CurriculumVersion.MaximumTerm;
                result.AcademicProgramName = curriculumInfo.AcademicProgram?.NameEn ?? "N/A";
                result.ApprovedDateText = curriculumInfo.CurriculumVersion.ApprovedDateText;
                result.Remark = curriculumInfo.CurriculumVersion.Remark;
                result.TotalCredit = curriculumInfo.CurriculumVersion.TotalCredit;

                result.CourseGroups = new List<CourseGroup>();
                result.CourseGroups = _curriculumProvider.GetCourseGroupRecursiveByVersionId(curriculumVersionId);

                if (result.CourseGroups != null)
                {
                    result.CourseGroups = result.CourseGroups.OrderBy(x => x.Sequence).ToList();
                }
            }
            
            return result;
        }

        #endregion
    }
}