using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace KeystoneLibrary.Providers
{
    public class EquivalenceProvider : BaseProvider, IEquivalenceProvider
    {

        public EquivalenceProvider(ApplicationDbContext db,
                                   IMapper mapper) : base(db, mapper)
        {

        }

         public List<StudentCourseEquivalentViewModel> GetExternalEquivalentCourses(List<StudentTransferCourseViewModel> courseList, long universityId) 
        {
            var now = DateTime.UtcNow;

            var studentCourseEquivalents = new List<StudentCourseEquivalentViewModel>();
            
            var allCourses = _db.Courses.Where(x => courseList.Select(y => y.CourseId).Contains(x.Id)).ToList();
            var grades = _db.Grades.Where(x => courseList.Select(y => y.GradeId).Contains(x.Id)).ToList();

            foreach (var course in courseList) 
            {
                if (course.CourseId != 0) 
                {
                    var courseEquivalent = _db.CourseEquivalents.Include(x => x.Course)
                                                                .Include(x => x.EquilaventCourse)
                                                                .Where(x => x.CourseId == course.CourseId 
                                                                            && (x.EffectivedAt == null || x.EffectivedAt.Date <= now) 
                                                                            && (x.EndedAt == null || x.EndedAt >= now)
                                                                            && x.Course.TransferUniversityId == universityId)
                                                                .FirstOrDefault(); 

                    var equivalent = new StudentCourseEquivalentViewModel();
                    equivalent.RegistrationCourseId = course.RegistrationCourseId;
                    equivalent.CurrentCourseId = course.CourseId;
                    if (course.CourseName == null)
                    {
                        equivalent.CurrentCourseName = allCourses.SingleOrDefault(x => x.Id == course.CourseId)?.CourseAndCredit ?? "N/A";
                        equivalent.CurrentCourseGrade = grades.SingleOrDefault(x => x.Id == course.GradeId).Name ?? "N/A";
                    }
                    else 
                    {
                        equivalent.CurrentCourseName = $"{ course.CourseCode } { course.CourseName }";
                        equivalent.CurrentCourseGrade = course.GradeName;
                    }

                    equivalent.TermId = course.TermId;
                    equivalent.SectionId = course.SectionId;
                    equivalent.GradeId = course.GradeId;
                    equivalent.GradeName = course.GradeName;
                    equivalent.NewCourseId =  courseEquivalent == null ? 0 : courseEquivalent.EquilaventCourseId;
                    equivalent.NewGradeId = course.GradeId;
                    studentCourseEquivalents.Add(equivalent);
                }
            }

            return studentCourseEquivalents;
        }
    }
}