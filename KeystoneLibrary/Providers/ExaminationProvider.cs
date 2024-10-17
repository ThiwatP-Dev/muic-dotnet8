using Microsoft.EntityFrameworkCore;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Providers
{
    public class ExaminationProvider : BaseProvider, IExaminationProvider
    {
        public ExaminationProvider(ApplicationDbContext db) : base(db) { }
        
        public ExaminationPeriod GetExaminationPeriod(long id)
        {
            return _db.ExaminationPeriods.SingleOrDefault(x => x.Id == id);
        }

        public ExaminationCoursePeriod GetExaminationCoursePeriod(long id)
        {
            return _db.ExaminationCoursePeriods.SingleOrDefault(x => x.Id == id);
        }

        public ExaminationCourseCondition GetExaminationCourseCondition(long id)
        {
            return _db.ExaminationCourseConditions.SingleOrDefault(x => x.Id == id);
        }

        public bool IsPeriodExists(long id, long termId, int period)
        {
            return _db.ExaminationPeriods.Any(x => x.Id != id
                                                   && x.TermId == termId
                                                   && x.Period == period);
        }

        public bool IsCoursePeriodExists(long id, long courseId, bool isEvening)
        {
            return _db.ExaminationCoursePeriods.Any(x => x.Id != id
                                                         && x.CourseId == courseId
                                                         && x.IsEvening == isEvening);
        }

        public string GetExaminationCoursePeriodCondition(long courseId)
        {
            string message = "", courseMessage = "", conditionMessage = "";
            var courseMessages = new List<string>();
            var result = new List<string>();
            var data = _db.ExaminationCourseConditions.Where(x => x.CourseIds.Contains(courseId.ToString()))
                                                      .GroupBy(x => x.ConditionText)
                                                      .ToList();
            
            if (!data.Any())
                return conditionMessage;

            foreach (var item in data)
            {                
                List<long> courseIds = JsonConvert.DeserializeObject<List<long>>(item.First().CourseIds);
                foreach (var course in courseIds)
                {
                    var coursePeriods = _db.ExaminationCoursePeriods.Include(x => x.Course)
                                                                    .Where(x => x.CourseId == course)
                                                                    .OrderBy(x => x.CourseId)
                                                                        .ThenBy(x => x.Period)
                                                                    .ToList();
                    
                    if (coursePeriods != null && coursePeriods.Any())
                    {
                        var courseDetail = new
                                           {
                                               CourseId = coursePeriods.First().CourseId,
                                               CourseCode = coursePeriods.First().Course.Code,
                                               CoursePeriods = coursePeriods.Select(x => x.Period)
                                                                            .ToList()
                                           };
                        
                        var periodString = new List<string>();
                        foreach(var couresPeriod in courseDetail.CoursePeriods)
                        {
                            var period = "Period " + couresPeriod;
                            periodString.Add(period);
                        }
                        
                        courseMessage = $"{ courseDetail.CourseCode } ( { string.Join(", ", periodString) } )";
                        courseMessages.Add(courseMessage);
                    }
                    else
                    {
                        var noPeriod = _db.Courses.Where(x => x.Id == course)
                                                  .FirstOrDefault();
                        courseMessage = $"{ noPeriod.Code }";
                        courseMessages.Add(courseMessage);
                    }
                }
                
                message = $"{ item.Key } : { string.Join(", ", courseMessages) }";
                result.Add(message);
                courseMessages.Clear();
            }

            conditionMessage = $"{ string.Join("/", result) }";
            return conditionMessage;
        }
    }
}