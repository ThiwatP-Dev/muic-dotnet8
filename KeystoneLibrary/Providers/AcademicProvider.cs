using KeystoneLibrary.Data;
using Microsoft.EntityFrameworkCore;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.Extensions.Caching.Memory;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Report;
using KeystoneLibrary.Models.DataModels.Curriculums;

namespace KeystoneLibrary.Providers
{
    public class AcademicProvider : BaseProvider, IAcademicProvider
    {
        protected readonly IMasterProvider _masterProvider;
        protected readonly IMemoryCache _memoryCache;
        
        public AcademicProvider(ApplicationDbContext db,
                                IMasterProvider masterProvider,
                                IMemoryCache memoryCache) : base(db)
        {
            _masterProvider = masterProvider;
            _memoryCache = memoryCache;
        }

        public List<Faculty> GetFacultiesByAcademicLevelIdForAdmission(long id)
        {
            var faculties = _db.Curriculums.Include(x => x.Faculty)
                                           .Where(x => x.AcademicLevelId == id)
                                           .Select(x => x.Faculty)
                                           .Distinct()
                                           .OrderBy(x => x.NameEn)
                                           .ToList();
            return faculties;
        }

        public List<Department> GetDepartmentsByFacultyIds(List<long> ids)
        {
            var departments = _db.Departments.Where(x => ids.Contains(x.FacultyId))
                                             .ToList();
            return departments;
        }

        public List<Department> GetDepartmentsByAcademicLevelIdAndFacultyIdForAdmission(long academicLevelId, long facultyId)
        {
            var departments = _db.Curriculums.Where(x => x.AcademicLevelId == academicLevelId
                                                         && x.FacultyId == facultyId)
                                             .Select(x => x.Department)
                                             .Distinct()
                                             .OrderBy(x => x.NameEn)
                                             .ToList();
            return departments;
        }

        public Term GetTerm(long id)
        {
            var term = _db.Terms.Find(id);
            return term;
        }

        public Term GetTermByTermAndYear(long academicLevelId, int academicTerm, int academicYear)
        {
            var term = _db.Terms.SingleOrDefault(x => x.AcademicLevelId == academicLevelId
                                                      && x.AcademicTerm == academicTerm
                                                      && x.AcademicYear == academicYear);
            return term;
        }

        public List<Term> GetTermsByAcademicLevelId(long id)
        {
            var terms = _db.Terms.Include(x => x.AcademicLevel)
                                 .Where(x => x.AcademicLevelId == id)
                                 .OrderByDescending(x => x.AcademicYear)
                                 .ThenByDescending(x => x.AcademicTerm)
                                 .ToList();
            return terms;
        }

        public List<Term> GetTermByAcademicYear(int year)
        {
            var terms = _db.Terms.Include(x => x.AcademicLevel)
                                 .Where(x => x.AcademicYear == year)
                                 .OrderByDescending(x => x.AcademicTerm)
                                 .ToList();
            return terms;
        }

        public Term GetCurrentTermByAcademicLevelId(long academicLevelId)
        {
            var term = _db.Terms.Include(x => x.AcademicLevel)
                                 .SingleOrDefault(x => x.AcademicLevelId == academicLevelId && x.IsCurrent);
            return term;
        }

        public List<Department> GetExceptionWithdrawalDepartments(long facultyId)
        {
            var departments = _db.Departments.Include(x => x.ExceptionalDepartment)
                                             .Where(x => (facultyId == 0 
                                                          || x.FacultyId == facultyId)
                                                         && x.ExceptionalDepartment == null)
                                             .OrderBy(x => x.NameEn)
                                             .ToList();
            return departments;
        }

        public List<Department> GetDepartmentsByFacultyIds(long academicLevelId, List<long> ids)
        {
            var departments = _db.Curriculums.Where(x => x.AcademicLevelId == academicLevelId
                                                         && ids.Contains(x.FacultyId))
                                             .Select(x => x.Department)
                                             .Distinct()
                                             .ToList();
            return departments;
        }

        public List<Faculty> GetFacultiesByAcademicLevelId(long id)
        {
            var faculties = _db.Curriculums.Include(x => x.Faculty)
                                           .Where(x => x.AcademicLevelId == id)
                                           .Select(x => x.Faculty)
                                           .Distinct()
                                           .OrderBy(x => x.Code)
                                           .ToList();
            return faculties;
        }
        
        public List<Curriculum> GetCurriculumsByDepartmentId(long academicLevelId, long facultyId, long departmentId)
        {
            var curriculums = _db.Curriculums.Where(x => x.AcademicLevelId == academicLevelId
                                                         && x.FacultyId == facultyId
                                                         && x.DepartmentId == departmentId)
                                             .ToList();
            return curriculums;
        }

        public List<Curriculum> GetCurriculumsByDepartmentIds(long academicLevelId, List<long> facultyIds, List<long> departmentIds)
        {
            var curriculums = _db.Curriculums.Where(x => x.AcademicLevelId == academicLevelId
                                                         && facultyIds.Contains(x.FacultyId)
                                                         && departmentIds.Contains(x.DepartmentId ?? 0))
                                             .ToList();
            return curriculums;
        }

        public List<CurriculumVersion> GetCurriculumVersionsByCurriculumIds(long academicLevelId, List<long> curriculumIds)
        {
            var currentTerm = GetCurrentTerm(academicLevelId);
            if (currentTerm != null)
            {
                var curriculumVersions = _db.CurriculumVersions.Where(x => x.Curriculum.AcademicLevelId == academicLevelId
                                                                           && curriculumIds.Contains(x.CurriculumId))
                                                                        //    && (x.OpenedTerm == null
                                                                        //        || x.OpenedTerm.AcademicYear > currentTerm.AcademicYear
                                                                        //        || (x.OpenedTerm.AcademicYear == currentTerm.AcademicYear
                                                                        //            && x.OpenedTerm.AcademicTerm <= currentTerm.AcademicTerm))
                                                                        //    && (x.ClosedTerm == null
                                                                        //        || x.ClosedTerm.AcademicYear < currentTerm.AcademicYear
                                                                        //        || (x.ClosedTerm.AcademicYear == currentTerm.AcademicYear
                                                                        //            && x.ClosedTerm.AcademicTerm >= currentTerm.AcademicTerm)))
                                                               .ToList();
                return curriculumVersions;
            }

            return new List<CurriculumVersion>();
        }

        public List<Department> GetDepartmentsByAcademicLevelIdAndFacultyId(long academicLevelId, long facultyId)
        {
            var departments = _db.Curriculums.Where(x => x.AcademicLevelId == academicLevelId
                                                         && x.FacultyId == facultyId)
                                             .Select(x => x.Department)
                                             .Distinct()
                                             .ToList();
            return departments;
        }

        public List<Department> GetDepartmentsByAcademicLevelIdAndFacultyId(long academicLevelId)
        {
            var departments = _db.Curriculums.Where(x => x.AcademicLevelId == academicLevelId)
                                             .Select(x => x.Department)
                                             .Distinct()
                                             .ToList();
            return departments;
        }

        public AcademicLevel GetAcademicLevel(long id)
        {
            var level = _db.AcademicLevels.SingleOrDefault(x => x.Id == id);
            return level;
        }

        public string GetFacultyNameById(long id)
        {
            var facultyName = _db.Faculties.SingleOrDefault(x => x.Id == id).NameEn;
            
            return facultyName;
        }

        public string GetFacultyNameByIds(List<long> ids)
        {
            var faculties = _db.Faculties.Where(x => ids.Contains(x.Id))
                                         .Select(x => x.ShortNameEn)
                                         .ToList();
            
            return String.Join(", ", faculties);
        }

        public string GetDepartmentNameByIds(List<long> ids)
        {
            var departments = _db.Departments.Where(x => ids.Contains(x.Id))
                                             .Select(x => x.ShortNameEn)
                                             .ToList();
            
            return String.Join(", ", departments);
        }

        public string GetFacultyShortNameById(long id, string language = "en")
        {
            var name = _db.Faculties.Where(x => x.Id == id)
                                    .Select(x => language == "th" ? x.ShortNameTh : x.ShortNameEn)
                                    .IgnoreQueryFilters()
                                    .SingleOrDefault();
            
            return name;
        }

        public string GetDepartmentShortNameById(long id, string language = "en")
        {
            var name = _db.Departments.Where(x => x.Id == id)
                                      .Select(x => language == "th" ? x.ShortNameTh : x.ShortNameEn)
                                      .IgnoreQueryFilters()
                                      .SingleOrDefault();
            
            return name;
        }

        public string GetDepartmentNameById(long id, string language = "en")
        {
            var name = _db.Departments.Where(x => x.Id == id)
                                      .Select(x => language == "th" ? x.NameTh : x.NameEn)
                                      .IgnoreQueryFilters()
                                      .SingleOrDefault();
            
            return name;
        }

        public string GetAcademicLevelNameById(long id, string language = "en")
        {
            var name = _db.AcademicLevels.Where(x => x.Id == id)
                                         .Select(x => language == "th" ? x.NameTh : x.NameEn)
                                         .IgnoreQueryFilters()
                                         .SingleOrDefault();
            
            return name;
        }

        public Course GetCourseDetail(long id)
        {
            var course = _db.Courses.Include(x => x.Faculty)
                                    .Include(x => x.Department)
                                    .Include(x => x.AcademicLevel)
                                    .Include(x => x.TeachingType)
                                    .Include(x => x.CourseRate)
                                    .Include(x => x.GradeTemplate)
                                    .IgnoreQueryFilters()
                                    .SingleOrDefault(x => x.Id == id);

            var excludedNativeLanguageIds = string.IsNullOrEmpty(course.ExcludingNativeLanguageId) ? new List<long>() : JsonConvert.DeserializeObject<List<long>>(course.ExcludingNativeLanguageId ?? "");
            var excludedNativeLanguage = _masterProvider.GetLanguages(excludedNativeLanguageIds);
            course.ExcludingNativeLanguageText = excludedNativeLanguage == null ? "" : string.Join(", ", excludedNativeLanguage.Select(x => x.NameEn));
            return course;
        }

        public TranscriptInformation GetFacultyAndDepartmentByCurriculumVersionId(long curriculumVersionId, string language)
        {
            var curriculums = new TranscriptInformation();
            if (language == "en")
            {
                curriculums = _db.CurriculumInformations.Where(x => x.CurriculumVersionId == curriculumVersionId)
                                                        .Select(x => new TranscriptInformation
                                                                     {
                                                                         Curriculum = x.CurriculumVersion.Curriculum.NameEn,
                                                                         Faculty = x.Faculty.NameEn,
                                                                         Department = x.Department.NameEn ?? "",
                                                                         Degree = x.CurriculumVersion.DegreeNameEn
                                                                     })
                                                        .SingleOrDefault();
            }
            else
            {
                curriculums = _db.CurriculumInformations.Where(x => x.CurriculumVersionId == curriculumVersionId)
                                                        .Select(x => new TranscriptInformation
                                                                     {
                                                                         Curriculum = x.CurriculumVersion.Curriculum.NameTh,
                                                                         Faculty = x.CurriculumVersion.Curriculum.Faculty.NameTh,
                                                                         Department = x.CurriculumVersion.Curriculum.Department.NameTh,
                                                                         Degree = x.CurriculumVersion.DegreeNameTh
                                                                     })
                                                        .SingleOrDefault();
            }

            return curriculums;
        }

        #region News
        public List<Topic> GetTopicByChannelId(long id)
        {
            var topics = _db.Topics.Where(x => x.ChannelId == id)
                                   .ToList();
            return topics;
        }

        public Term GetCurrentTerm(long academiclevelId)
        {
            Term cacheTerm = new Term();
            if (!_memoryCache.TryGetValue(CacheKey.CurrentTerm, out cacheTerm))
            {
                cacheTerm = GetTermsByAcademicLevelId(academiclevelId).SingleOrDefault(x => x.IsCurrent);
                var cacheOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(1));
                _memoryCache.Set(CacheKey.CurrentTerm, cacheTerm, cacheOptions);
            }

            cacheTerm = _memoryCache.Get<Term>(CacheKey.CurrentTerm);
            return cacheTerm;
        }

        public Term GetQuestionnaireTerm(long academiclevelId)
        {
            Term cacheTerm = new Term();
            if (!_memoryCache.TryGetValue(CacheKey.QuestionnaireTerm, out cacheTerm))
            {
                cacheTerm = GetTermsByAcademicLevelId(academiclevelId).SingleOrDefault(x => x.IsQuestionnaire);
                var cacheOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(1));
                _memoryCache.Set(CacheKey.QuestionnaireTerm, cacheTerm, cacheOptions);
            }

            cacheTerm = _memoryCache.Get<Term>(CacheKey.QuestionnaireTerm);
            return cacheTerm;
        }
        #endregion
    }
}