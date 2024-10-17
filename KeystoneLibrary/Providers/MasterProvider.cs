using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Fee;
using KeystoneLibrary.Models.DataModels.MasterTables;
using KeystoneLibrary.Models.DataModels.Profile;
using KeystoneLibrary.Models.DataModels.Advising;
using Microsoft.EntityFrameworkCore;

namespace KeystoneLibrary.Providers
{
    public class MasterProvider : BaseProvider, IMasterProvider
    {
        public MasterProvider(ApplicationDbContext db) : base(db) { }

        public ResidentType FindResidentType(long id)
        {
            var model = _db.ResidentTypes.IgnoreQueryFilters()
                                         .SingleOrDefault(x => x.Id == id);
            return model;
        }

        public StudentFeeType GetStudentFeeType(long id)
        {
            var model = _db.StudentFeeTypes.IgnoreQueryFilters()
                                           .SingleOrDefault(x => x.Id == id);
            return model;
        }
        
        public Probation FindProbation(long id)
        {
            var model = _db.Probations.IgnoreQueryFilters()
                                      .SingleOrDefault(x => x.Id == id);
            return model;
        }

        public ExtendedYear FindExtendedYear(long id)
        {
            var model = _db.ExtendedYears.IgnoreQueryFilters()
                                         .SingleOrDefault(x => x.Id == id);
            return model;
        }

        public InstructorType FindInstructorType(long id)
        {
            var model = _db.InstructorTypes.IgnoreQueryFilters()
                                           .SingleOrDefault(x => x.Id == id);
            return model;
        }

        public bool IsExistExtendedYear(ExtendedYear model)
        {
            var today = DateTime.Today.Date;
            var isExist = _db.ExtendedYears.Any(x => x.Id != model.Id
                                                     && x.AcademicLevelId == model.AcademicLevelId
                                                     && (x.ExpiredAt == null
                                                         || x.ExpiredAt.Value.Date >= today));
            return isExist;
        }

        public ExtendedStudent FindExtendedStudent(long id)
        {
            var model = _db.ExtendedStudents.IgnoreQueryFilters()
                                            .SingleOrDefault(x => x.Id == id);
            return model;
        }

        public SpecializationGroup FindSpecializationGroup(long id) 
        {
            var model = _db.SpecializationGroups.IgnoreQueryFilters()
                                                .SingleOrDefault(x => x.Id == id);
            return model;
        }

        public Petition FindPetition(long id) 
        {
            var model = _db.Petitions.IgnoreQueryFilters()
                                     .SingleOrDefault(x => x.Id == id);
            return model;
        }

        public List<ExemptedAdmissionExamination> GetExemptedAdmissionExaminations() 
        {
            var exams = _db.ExemptedAdmissionExaminations.OrderBy(x => x.NameEn)
                                                         .ToList();
            return exams;
        }

        public Nationality GetNationality(long id)
        {
            var nationality = _db.Nationalities.SingleOrDefault(x => x.Id == id);
            return nationality;
        }

        public TransferUniversity FindTrasferUniversity(long id)
        {
            var transferUniversity = _db.TransferUniversities.Include(x => x.Courses)
                                                                 .ThenInclude(x => x.AcademicLevel)
                                                             .Include(x => x.Country)
                                                             .IgnoreQueryFilters()
                                                             .SingleOrDefault(x => x.Id == id);

            transferUniversity.AcademicLevelId = transferUniversity.Courses?.FirstOrDefault()?.AcademicLevelId ?? 0;
            return transferUniversity;
        }

        public Course GetExternalCourse(long id)
        {
            var externalCourse = _db.Courses.IgnoreQueryFilters()
                                            .SingleOrDefault(x => x.Id == id);
            return externalCourse;
        }
        
        public CourseExclusion FindCourseExclusion(long id)
        {
            var model = _db.CourseExclusions.Include(x => x.Course)
                                            .Include(x => x.ExcludingCourse)
                                            .Include(x => x.CurriculumVersion)
                                                .ThenInclude(x => x.Curriculum).IgnoreQueryFilters()
                                            .SingleOrDefault(x => x.Id == id);
            if (model != null)
            {
                model.AcademicLevelId = model.CurriculumVersion.Curriculum.AcademicLevelId;
                model.CurriculumId = model.CurriculumVersion.CurriculumId;
            }                              

            return model;
        }

        public List<Language> GetLanguages(List<long> ids)
        {
            var languages = _db.Languages.Where(x => ids.Contains(x.Id))
                                         .IgnoreQueryFilters()
                                         .ToList();
            return languages;
        }

        public ExaminationType FindExaminationType(long id)
        {
            var type = _db.ExaminationTypes.IgnoreQueryFilters()
                                           .SingleOrDefault(x => x.Id == id);
            return type;
        }

        public AdvisingLog GetAdvisingLog(long id)
        {
            var advisingLog = _db.AdvisingLogs.IgnoreQueryFilters()
                                              .SingleOrDefault(x => x.Id == id);
            return advisingLog;
        }

        public AdvisingStatus GetAdvisingStatus(long id)
        {
            var advisingStatus = _db.AdvisingStatuses.IgnoreQueryFilters()
                                                     .SingleOrDefault(x => x.Id == id);
            return advisingStatus;
        }
        
        public ReEnterReason GetReEnterReason(long id)
        {
            var model = _db.ReEnterReasons.IgnoreQueryFilters()
                                          .SingleOrDefault(x => x.Id == id);
            return model;
        }

        public AdmissionPlace GetAdmissionPlace(long id)
        {
            var model = _db.AdmissionPlaces.IgnoreQueryFilters()
                                           .SingleOrDefault(x => x.Id == id);
            return model;
        }    

        public Invoice GetInvoice(long id)
        {
            var model = _db.Invoices.Include(x => x.InvoiceItems)
                                    .IgnoreQueryFilters()
                                    .SingleOrDefault(x => x.Id == id);
            return model;
        }
        
        public FeeItem GetFeeItem(long id)
        {
            var model = _db.FeeItems.IgnoreQueryFilters()
                                    .SingleOrDefault(x => x.Id == id);
            return model;
        }

        public PaymentMethod GetPaymentMethod(long id)
        {
            var model = _db.PaymentMethods.IgnoreQueryFilters()
                                          .SingleOrDefault(x => x.Id == id);
            return model;
        }

        public DistributionMethod GetDistributionMethod(long id)
        {
            var model = _db.DistributionMethods.IgnoreQueryFilters()
                                               .SingleOrDefault(x => x.Id == id);
            return model;
        }

        public StandardGradingGroup GetStandardGradingGroup(long id)
        {
            var model = _db.StandardGradingGroups.Include(x => x.StandardGradingScores)
                                                 .Include(x => x.GradeTemplate)
                                                 .IgnoreQueryFilters()
                                                 .SingleOrDefault(x => x.Id == id);
            return model;
        }

        public CustomCourseGroup GetCustomCourseGroup(long id)
        {
            var model = _db.CustomCourseGroups.Include(x => x.TuitionFeeRates)
                                              .IgnoreQueryFilters()
                                              .SingleOrDefault(x => x.Id == id);
            return model;
        }

        public QuestionnaireCourseGroup GetQuestionnaireCourseGroup(long id)
        {
            var model = _db.QuestionnaireCourseGroups.IgnoreQueryFilters()
                                                     .SingleOrDefault(x => x.Id == id);
            return model;
        }
    }
}