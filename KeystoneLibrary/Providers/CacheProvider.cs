using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.Extensions.Caching.Memory;

namespace KeystoneLibrary.Providers
{
    public class CacheProvider : ICacheProvider
    {
        protected readonly ApplicationDbContext _db;
        protected readonly IMemoryCache _memoryCache;
        protected readonly IInstructorProvider _instructorProvider;
        protected readonly IRegistrationProvider _registrationProvider;
        protected readonly IAcademicProvider _academicProvider;
        
        public CacheProvider(ApplicationDbContext db,
                             IMemoryCache memoryCache,
                             IInstructorProvider instructorProvider,
                             IRegistrationProvider registrationProvider,
                             IAcademicProvider academicProvider)
        {
            _db = db;
            _memoryCache = memoryCache;
            _instructorProvider = instructorProvider;
            _registrationProvider = registrationProvider;
            _academicProvider = academicProvider;
        }

        public List<Instructor> GetInstructors()
        {
            List<Instructor> cacheInstructors = new List<Instructor>();
            if (!_memoryCache.TryGetValue(CacheKey.Instructors, out cacheInstructors))
            {
                cacheInstructors = _instructorProvider.GetInstructors();
                var cacheOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(1));
                _memoryCache.Set(CacheKey.Instructors, cacheInstructors, cacheOptions);
            }

            cacheInstructors = _memoryCache.Get<List<Instructor>>(CacheKey.Instructors);
            return cacheInstructors;
        }

        public Term GetRegistrationTerm(long academiclevelId)
        {
            Term cacheTerm = new Term();
            if (!_memoryCache.TryGetValue(CacheKey.RegistrationTerm, out cacheTerm))
            {
                cacheTerm = _registrationProvider.GetRegistrationTerm(academiclevelId);
                var cacheOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(1));
                _memoryCache.Set(CacheKey.RegistrationTerm, cacheTerm, cacheOptions);
            }

            cacheTerm = _memoryCache.Get<Term>(CacheKey.RegistrationTerm);
            return cacheTerm;
        }

        public Term GetCurrentTerm(long academiclevelId)
        {
            Term cacheTerm = new Term();
            if (!_memoryCache.TryGetValue(CacheKey.CurrentTerm, out cacheTerm))
            {
                cacheTerm = _academicProvider.GetCurrentTerm(academiclevelId);
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
                cacheTerm = _academicProvider.GetQuestionnaireTerm(academiclevelId);
                var cacheOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(1));
                _memoryCache.Set(CacheKey.QuestionnaireTerm, cacheTerm, cacheOptions);
            }

            cacheTerm = _memoryCache.Get<Term>(CacheKey.QuestionnaireTerm);
            return cacheTerm;
        }

        public List<Faculty> GetFaculties()
        {
            List<Faculty> faculties = new List<Faculty>();

            if (!_memoryCache.TryGetValue(CacheKey.Faculty, out faculties))
            {
                faculties = _db.Faculties.ToList();
                var cacheOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(1));
                _memoryCache.Set(CacheKey.Faculty, faculties, cacheOptions);
            }

            faculties = _memoryCache.Get<List<Faculty>>(CacheKey.Faculty);
            return faculties;
        }

        public List<Department> GetDepartments()
        {
            List<Department> departments = new List<Department>();

            if (!_memoryCache.TryGetValue(CacheKey.Department, out departments))
            {
                departments = _db.Departments.ToList();
                var cacheOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(1));
                _memoryCache.Set(CacheKey.Department, departments, cacheOptions);
            }

            departments = _memoryCache.Get<List<Department>>(CacheKey.Department);
            return departments;
        }

        public List<Course> GetCourses()
        {
            List<Course> courses = new List<Course>();

            if (!_memoryCache.TryGetValue(CacheKey.Course, out courses))
            {
                courses = _db.Courses.Where(x => x.TransferUniversityId == null)
                                     .ToList();
                var cacheOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(1));
                _memoryCache.Set(CacheKey.Course, courses, cacheOptions);
            }

            courses = _memoryCache.Get<List<Course>>(CacheKey.Course);
            return courses;
        }

        public List<Course> GetCourseAndTransferCourse()
        {
            List<Course> courses = new List<Course>();

            if (!_memoryCache.TryGetValue(CacheKey.Course, out courses))
            {
                courses = _db.Courses.ToList();
                var cacheOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(1));
                _memoryCache.Set(CacheKey.Course, courses, cacheOptions);
            }

            courses = _memoryCache.Get<List<Course>>(CacheKey.Course);
            return courses;
        }

        public List<Course> GetExternalCourses()
        {
            List<Course> courses = new List<Course>();

            if (!_memoryCache.TryGetValue(CacheKey.Course, out courses))
            {
                courses = _db.Courses.Where(x => x.TransferUniversityId != null)
                                     .ToList();
                var cacheOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(1));
                _memoryCache.Set(CacheKey.Course, courses, cacheOptions);
            }

            courses = _memoryCache.Get<List<Course>>(CacheKey.Course);
            return courses;
        }
    }
}