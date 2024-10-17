using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class ChangeCurriculumPreviewController : BaseController
    {
        protected readonly ICacheProvider _cacheProvider;
        protected readonly IRegistrationProvider _registrationProvider;
        protected readonly IStudentProvider _studentProvider;
        protected readonly ICurriculumProvider _curriculumProvider;
        protected readonly IGradeProvider _gradeProvider;
        
        public ChangeCurriculumPreviewController(ApplicationDbContext db, 
                                                 IFlashMessage flashMessage, 
                                                 IMapper mapper, 
                                                 ISelectListProvider selectListProvider,
                                                 ICacheProvider cacheProvider,
                                                 IStudentProvider studentProvider,
                                                 IRegistrationProvider registrationProvider,
                                                 ICurriculumProvider curriculumProvider,
                                                 IGradeProvider gradeProvider) : base(db, flashMessage, mapper, selectListProvider)
        {
            _cacheProvider = cacheProvider;
            _registrationProvider = registrationProvider;
            _studentProvider = studentProvider;
            _curriculumProvider = curriculumProvider;
            _gradeProvider = gradeProvider;
        }
        
        public IActionResult Index(string studentCode, long curriculumId, long curriculumVersionId)
        {
            if (curriculumVersionId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return RedirectToAction(nameof(Index), new { studentCode = studentCode });
            }

            var student = _studentProvider.GetStudentInformationByCode(studentCode);
            var model = new StudentTransferViewModel();
            model.StudentId = student.Id;
            model.StudentCode = student.Code;
            model.CurriculumId = curriculumId;
            model.CurriculumVersionId = curriculumVersionId;
            model.StudentFirstName = student.FirstNameEn;
            model.StudentLastName = student.LastNameEn;
            model.AcademicLevel = student.AcademicInformation.AcademicLevel.NameEn;
            model.FacultyName = student.AcademicInformation.Faculty.NameEn;
            model.DepartmentName = student.AcademicInformation.Department?.NameEn ?? "N/A";
            model.Credit = student.AcademicInformation.CreditComp;
            model.GPA = student.AcademicInformation.GPA;

            if (model.CurriculumVersionId != 0) 
            {
                model.CurriculumVersionName =  _curriculumProvider.GetCurriculumVersionName(student.AcademicInformation.CurriculumVersionId ?? 0);
                model.NewCurriculumVersionName =  _curriculumProvider.GetCurriculumVersionName(curriculumVersionId);
            }

            int currentTotalCourseGroup = 0;
            model.CurrentCurriculumCourseGroupViewModels = _curriculumProvider.GetCourseGroupWithRegistrationCourses(student.Id, student.AcademicInformation.CurriculumVersionId ?? 0, out currentTotalCourseGroup);
            
            int newTotalCourseGroup = 0;
            model.NewCurriculumCourseGroupViewModels = _curriculumProvider.GetCourseGroupWithRegistrationCourses(student.Id, curriculumVersionId, out newTotalCourseGroup);
            
            
            
            // model.StudentCourseEquivalents = _registrationProvider.GetRegistrationEquivalentCourses(model.StudentCourses, curriculumVersionId);
            // model.StudentId = model.StudentId;
            // model.StudentCourses = model.StudentCourses;

            // model.SummaryOldCurriculumCourseGroups = new List<StudentCourseCategorizationViewModel>();
            // var courseGroups = (from courseGroup in model.StudentCourses
            //                     group courseGroup by courseGroup.CourseGroupId into grp
            //                     select new
            //                            {
            //                                CourseGroupId = grp.Key,
            //                                CourseGroupName = grp.FirstOrDefault().CourseGroupName,
            //                                Courses = grp.OrderBy(x => x.CourseCode)
            //                                             .ToList()
            //                            }).ToList();

            // foreach (var courseGroup in courseGroups)
            // {
            //     var group = new StudentCourseCategorizationViewModel();
            //     group.CourseGroupId = courseGroup.CourseGroupId;
            //     group.CourseGroupName = courseGroup.CourseGroupName;
            //     group.courseList = new List<StudentCourseCategorizationDetail>();
            //     foreach (var course in courseGroup.Courses)
            //     {
            //         var groupDetail = new StudentCourseCategorizationDetail();
            //         groupDetail.CourseId = course.CourseId;
            //         groupDetail.CourseCode = course.CourseCode;
            //         groupDetail.CourseName = course.CourseName;
            //         groupDetail.GradeId = course.GradeId;
            //         groupDetail.GradeName = course.GradeName;
            //         group.courseList.Add(groupDetail);
            //     }

            //     model.SummaryOldCurriculumCourseGroups.Add(group);
            // }

            // var studentCourseCategorizations = new List<StudentCourseCategorizationViewModel>();
            // var groups = _curriculumProvider.GetCourseGroupsForChangeCurriculum(model.CurriculumVersionId, 0, 0);
            // foreach (var group in groups) 
            // {
            //     var row = new StudentCourseCategorizationViewModel();
            //     row.CourseGroupId = group.Id;
            //     var groupInfo = _db.CourseGroups.SingleOrDefault(x => x.Id == group.Id);
            //     row.CourseGroupName = groupInfo.NameEn;
            //     row.courseList = new List<StudentCourseCategorizationDetail>();
            //     studentCourseCategorizations.Add(row);
            // }

            // var newCourseIds = model.StudentCourseEquivalents.Select(y => y.NewCourseId).ToList();
            // var currentCourseIds = model.StudentCourseEquivalents.Select(y => y.CurrentCourseId).ToList();

            // var courses = _db.Courses.Where(x => newCourseIds.Contains(x.Id)
            //                                      || currentCourseIds.Contains(x.Id))
            //                          .ToList();
            // var grades = _gradeProvider.GetGrades();

            // foreach (var item in model.StudentCourseEquivalents) 
            // {
            //     if (item.CurriculumCourseGroupId != 0)
            //     {
            //         var courseCategorize = studentCourseCategorizations.SingleOrDefault(x => x.CourseGroupId == item.CurriculumCourseGroupId);
            //         if (item.NewCourseId != 0) 
            //         {
            //             if (courseCategorize != null) 
            //             {
            //                 var detail = new StudentCourseCategorizationDetail();
            //                 var course = courses.SingleOrDefault(x => x.Id == item.NewCourseId);
            //                 detail.CourseId = course.Id;
            //                 detail.CourseCode = course.Code;
            //                 detail.CourseName = course.NameEn;
            //                 detail.CourseFullName = course.NameEnAndCredit;
            //                 var grade = grades.SingleOrDefault(x => x.Id == item.GradeId);
            //                 if (grade != null) 
            //                 {
            //                     detail.GradeId = item.GradeId;
            //                     detail.GradeName = grades.SingleOrDefault(x => x.Id == item.GradeId).Name;
            //                 }

            //                 detail.TermId = item.TermId;
            //                 detail.CourseGroupId = item.CurriculumCourseGroupId;
            //                 detail.RegistrationCourseId = item.RegistrationCourseId;
            //                 courseCategorize.courseList.Add(detail);
            //             }
            //         }
            //         else 
            //         {
            //             var detail = new StudentCourseCategorizationDetail();
            //             var course = courses.SingleOrDefault(x => x.Id == item.CurrentCourseId);
            //             detail.CourseId = course.Id;
            //             detail.CourseCode = course.Code;
            //             detail.CourseName = course.NameEn;
            //             detail.CourseFullName = course.NameEnAndCredit;
            //             var grade = grades.SingleOrDefault(x => x.Id == item.GradeId);
            //             if (grade != null) 
            //             {
            //                 detail.GradeId = item.GradeId;
            //                 detail.GradeName = grades.SingleOrDefault(x => x.Id == item.GradeId).Name;
            //             }

            //             detail.TermId = item.TermId;
            //             detail.CourseGroupId = item.CurriculumCourseGroupId;
            //             detail.RegistrationCourseId = item.RegistrationCourseId;
            //             courseCategorize.courseList.Add(detail);
            //         }
            //     }
            //     else 
            //     {
            //         var courseCategorize = studentCourseCategorizations.SingleOrDefault(x => x.CourseGroupId == 0);
            //         var detail = new StudentCourseCategorizationDetail();
            //         var course = courses.SingleOrDefault(x => x.Id == item.CurrentCourseId);
            //         detail.CourseId = course.Id;
            //         detail.CourseCode = course.Code;
            //         detail.CourseName = course.NameEn;
            //         detail.CourseFullName = course.NameEnAndCredit;
            //         var grade = grades.SingleOrDefault(x => x.Id == item.GradeId);
            //         if (grade != null) 
            //         {
            //             detail.GradeId = item.GradeId;
            //             detail.GradeName = grades.SingleOrDefault(x => x.Id == item.GradeId).Name;
            //         }

            //         detail.TermId = item.TermId;
            //         detail.CourseGroupId = item.CurriculumCourseGroupId;
            //         detail.RegistrationCourseId = item.RegistrationCourseId;
            //         courseCategorize.courseList.Add(detail);    
            //     }
            // }
            // model.SummaryNewCurriculumCourseGroups = studentCourseCategorizations.Where(x => x.courseList.Any()).ToList();

            return View("~/Views/ChangeCurriculum/PreviewMatchCourses.cshtml", model);
        }
    }
}