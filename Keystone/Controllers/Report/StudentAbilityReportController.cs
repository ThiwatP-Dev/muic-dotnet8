using KeystoneLibrary.Data;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;
using KeystoneLibrary.Interfaces;
using Keystone.Permission;

namespace Keystone.Controllers
{
    [PermissionAuthorize("StudentAbilityReport", "")]
    public class StudentAbilityReportController : BaseController
    {
        public StudentAbilityReportController(ApplicationDbContext db,
                                              IFlashMessage flashMessage,
                                              ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) { }

        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId, criteria.CurriculumId);
            if (criteria.StartStudentBatch == null && criteria.EndStudentBatch == null)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }
            // var students = from student in _db.Students.AsNoTracking()
            //                join title in _db.Titles.AsNoTracking() on student.TitleId equals title.Id
            //                join academicInfo in _db.AcademicInformations.AsNoTracking() on student.Id equals academicInfo.StudentId
            //                join department in _db.Departments.AsNoTracking() on academicInfo.DepartmentId equals department.Id
            //                let abilities = from curriculumInfo in _db.CurriculumInformations.AsNoTracking()
            //                                join specialGroupInfo in _db.SpecializationGroupInformations.AsNoTracking() on curriculumInfo.Id equals specialGroupInfo.CurriculumInformationId into specialGroupInfos
            //                                from specialGroupInfo in specialGroupInfos.DefaultIfEmpty()
            //                                join specialGroup in _db.SpecializationGroups.AsNoTracking() on specialGroupInfo.SpecializationGroupId equals specialGroup.Id into specialGroups
            //                                from specialGroup in specialGroups.DefaultIfEmpty()
            //                                where curriculumInfo.StudentId == student.Id
            //                                select new 
            //                                       {
            //                                           curriculumInfo.CurriculumId,
            //                                           curriculumInfo.CurriculumVersionId,
            //                                           SpecializationGroupId = specialGroupInfo == null ? 0 : specialGroupInfo.SpecializationGroupId,
            //                                           specialGroup.NameEn
            //                                       }
            //                let predefineCourses = from predefind in _db.StudentPredefinedCourses.AsNoTracking()
            //                                       join course in _db.Courses on predefind.CourseId equals course.Id
            //                                       where predefind.StudentId == student.Id
            //                                       select new 
            //                                              {
            //                                                  course.Code,
            //                                                  course.NameEn,
            //                                                  course.Credit,
            //                                                  course.Lecture,
            //                                                  course.Lab,
            //                                                  course.Other,
            //                                                  predefind.Type
            //                                              }
                        //    where (string.IsNullOrEmpty(criteria.Code) || student.Code.Contains(criteria.Code))
                        //           && (string.IsNullOrEmpty(criteria.FirstName)
                        //               || (!string.IsNullOrEmpty(student.FirstNameEn) && student.FirstNameEn.StartsWith(criteria.FirstName))
                        //               || (!string.IsNullOrEmpty(student.FirstNameTh) && student.FirstNameTh.StartsWith(criteria.FirstName)))
                        //           && (string.IsNullOrEmpty(criteria.LastName)
                        //               || (!string.IsNullOrEmpty(student.LastNameEn) && student.LastNameEn.StartsWith(criteria.LastName))
                        //               || (!string.IsNullOrEmpty(student.LastNameTh) && student.LastNameTh.StartsWith(criteria.LastName)))
                        //           && (criteria.NationalityId == 0 || student.NationalityId == criteria.NationalityId)
                        //           && (criteria.AcademicLevelId == 0 || academicInfo.AcademicLevelId == criteria.AcademicLevelId)
                        //           && (criteria.FacultyId == 0 || academicInfo.FacultyId == criteria.FacultyId)
                        //           && (criteria.DepartmentId == 0 || academicInfo.DepartmentId == criteria.DepartmentId)
                        //           && (criteria.CurriculumId == 0 || (abilities != null && abilities.Any(x => x.CurriculumId == criteria.CurriculumId)))
                        //           && (criteria.CurriculumVersionId == 0 || (abilities != null && abilities.Any(x => x.CurriculumVersionId == criteria.CurriculumVersionId)))
                        //           && (criteria.AbilityId == 0 || (abilities != null && abilities.Any(x => x.SpecializationGroupId == criteria.AbilityId)))
                        //           && (string.IsNullOrEmpty(criteria.Status) || student.StudentStatus == criteria.Status)
                        //           && (criteria.StartStudentBatch == null || academicInfo.Batch >= criteria.StartStudentBatch)
                        //           && (criteria.EndStudentBatch == null || academicInfo.Batch <= criteria.EndStudentBatch)
            //                orderby student.Code
            //                select new 
            //                       {
            //                           student.Code,
            //                           TitleNameEn = title.NameEn,
            //                           student.FirstNameEn,
            //                           student.MidNameEn,
            //                           student.LastNameEn,
            //                           DepartmentCode = department.Code,
            //                           Abilities = abilities.AsNoTracking()
            //                                                .AsEnumerable(),
            //                           PredefindCourses = predefineCourses.AsNoTracking()
            //                                                              .ToList()
          
            //                       };

            var students = _db.Students.AsNoTracking()
                                       .Where(x => (string.IsNullOrEmpty(criteria.Code) 
                                                    || x.Code.Contains(criteria.Code))
                                                    && (string.IsNullOrEmpty(criteria.FirstName)
                                                        || x.FirstNameEn.StartsWith(criteria.FirstName)
                                                        || x.FirstNameTh.StartsWith(criteria.FirstName))
                                                    && (string.IsNullOrEmpty(criteria.LastName)
                                                        || x.LastNameEn.StartsWith(criteria.LastName)
                                                        || x.LastNameTh.StartsWith(criteria.LastName))
                                                    && (criteria.NationalityId == 0 
                                                        || x.NationalityId == criteria.NationalityId)
                                                    && (criteria.AcademicLevelId == 0 
                                                        || x.AcademicInformation.AcademicLevelId == criteria.AcademicLevelId)
                                                    && (criteria.TermIds == null
                                                        || !criteria.TermIds.Any()
                                                        || criteria.TermIds.Any(y => y == x.AdmissionInformation.AdmissionTermId))
                                                    && (criteria.FacultyId == 0 
                                                        || x.AcademicInformation.FacultyId == criteria.FacultyId)
                                                    && (criteria.DepartmentId == 0 
                                                        || x.AcademicInformation.DepartmentId == criteria.DepartmentId)
                                                    && (string.IsNullOrEmpty(criteria.Status) 
                                                        || x.StudentStatus == criteria.Status)
                                                    && (criteria.StartStudentBatch == null 
                                                        || x.AcademicInformation.Batch >= criteria.StartStudentBatch)
                                                    && (criteria.EndStudentBatch == null 
                                                        || x.AcademicInformation.Batch <= criteria.EndStudentBatch))
                                       .Select(x => new StudentAbilityReprotViewModel
                                                    {
                                                        Id = x.Id,
                                                        Code = x.Code,
                                                        Title = x.Title.NameEn,
                                                        Major = x.AcademicInformation.Department.Code,
                                                        StudentFirstName = x.FirstNameEn,
                                                        StudentLastName = x.LastNameEn,
                                                        StudentMidName = x.MidNameEn,
                                                        IntakeTerm = x.AdmissionInformation.AdmissionTerm == null ? "" : x.AdmissionInformation.AdmissionTerm.AcademicTerm + "/" + x.AdmissionInformation.AdmissionTerm.AcademicYear
                                                    })
                                        .OrderBy(x => x.Code)
                                        .ToList();

            var studentIds = students.Select(x => x.Id).ToList();
            var predefineCourses =  _db.StudentPredefinedCourses.AsNoTracking()
                                                                .Where(x => studentIds.Contains(x.StudentId))
                                                                .Select(x => new PredefindCoursesViewModel
                                                                             {
                                                                                 StudentId = x.StudentId,
                                                                                 CourseRateId = x.Course.CourseRateId,
                                                                                 CourseCode = x.Course.Code,
                                                                                 CourseName = x.Course.NameEn,
                                                                                 Credit = x.Course.Credit,
                                                                                 Lab = x.Course.Lab,
                                                                                 Lecture = x.Course.Lecture,
                                                                                 Other = x.Course.Other,
                                                                                 Type = x.Type
                                                                             })
                                                                .ToList();

            var specializationGroupInformations = _db.SpecializationGroupInformations.Where(x => studentIds.Contains(x.CurriculumInformation.StudentId)
                                                                                                 && (criteria.CurriculumId == 0
                                                                                                 || x.CurriculumInformation.CurriculumVersion.CurriculumId == criteria.CurriculumId)
                                                                                                 && (criteria.CurriculumVersionId == 0
                                                                                                     || x.CurriculumInformation.CurriculumVersionId == criteria.CurriculumVersionId)
                                                                                                 && (criteria.AbilityId == 0
                                                                                                     || x.SpecializationGroupId == criteria.AbilityId))
                                                                                     .AsNoTracking()
                                                                                     .Select(x => new
                                                                                                  {
                                                                                                      SpecializationGroupName = x.SpecializationGroup.NameEn,
                                                                                                      StudentId = x.CurriculumInformation.StudentId
                                                                                                  })
                                                                                     .ToList();

            if (criteria.CurriculumId != 0 || criteria.CurriculumVersionId != 0 || criteria.AbilityId != 0)
            {
                var specializationStudentIds = specializationGroupInformations.Select(x => x.StudentId).ToList();
                students = students.Where(x => specializationStudentIds.Contains(x.Id))
                                   .ToList();

            }                                                  
            
            var results = new List<StudentAbilityReprotViewModel>();
            foreach (var item in students)
            {
                item.PredefindCourses = predefineCourses.Where(x => x.StudentId == item.Id)
                                                        .Select(x => x.CourseAndCreditAndType)
                                                        .ToList();

                item.Abilities = specializationGroupInformations.Where(x => x.StudentId == item.Id)
                                                                .Select(x => x.SpecializationGroupName)
                                                                .ToList();
                // results.Add(new StudentAbilityReprotViewModel
                //             {
                //                 Abilities = item.Abilities.Where(x => !string.IsNullOrEmpty(x.NameEn))
                //                                           .Select(x => x.NameEn)
                //                                           .ToList()
                //             });
            }

            return View(students.GetAllPaged(criteria));
        }

        [HttpPost]
        [RequestFormLimits(ValueCountLimit = Int32.MaxValue)]
        public IActionResult ExportExcel(List<StudentAbilityReprotViewModel> results, string returnUrl)
        {
            if (results != null && results.Any())
            {
                using (var wb = GenerateWorkBook(results))
                {
                    return wb.Deliver($"Student Ability Report.xlsx");
                }
            }

            return Redirect(returnUrl);
        }

        [HttpPost]
        [RequestFormLimits(ValueCountLimit = Int32.MaxValue)]
        public IActionResult ExportExcelPivot(List<StudentAbilityReprotViewModel> results, string returnUrl)
        {
            if (results != null && results.Any())
            {
                using (var wb = GenerateWorkBookPivot(results))
                {
                    return wb.Deliver($"Student Ability Pivot Report.xlsx");
                }
            }

            return Redirect(returnUrl);
        }

        private XLWorkbook GenerateWorkBook(List<StudentAbilityReprotViewModel> results)
        {
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet();
            int row = 1;
            var column = 1;

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row, column++).Value = "Code".ToUpper();

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row, column++).Value = "Title".ToUpper();

            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row, column++).Value = "Name".ToUpper();

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row, column++).Value = "Major".ToUpper();

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row, column++).Value = "Intake Term".ToUpper();

            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row, column++).Value = "English Ability".ToUpper();

            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row, column++).Value = "Math Ability".ToUpper();

            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row, column++).Value = "Other Ability".ToUpper();

            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row++, column++).Value = "Predefind Course".ToUpper();

            foreach (var item in results)
            {
                var predefind = string.Empty;
                if (item.PredefindCourses != null && item.PredefindCourses.Any())
                {
                    predefind = string.Join(", ", item.PredefindCourses);
                }

                if (item.Abilities != null && item.Abilities.Any())
                {
                    column = 1;
                    var englishTrack = string.Join(Environment.NewLine, item.Abilities.Where(x => x.Contains("EnglishTrack")).ToList());
                    var mathTrack = string.Join(Environment.NewLine, item.Abilities.Where(x => x.Contains("MathTrack")).ToList());
                    var otherTrack = string.Join(Environment.NewLine, item.Abilities.Where(x => !x.Contains("EnglishTrack") && !x.Contains("MathTrack")));
                    
                    ws.Cell(row, column).SetValue<string>(item.Code);
                    ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Cell(row, column++).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;


                    ws.Cell(row, column).Value = item.Title;
                    ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Cell(row, column++).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(row, column).Value = item.FullName;
                    ws.Cell(row, column++).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;


                    ws.Cell(row, column).Value = item.Major;
                    ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Cell(row, column++).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(row, column).SetValue<string>(item.IntakeTerm);
                    ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Cell(row, column++).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(row, column).Value = englishTrack;
                    ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    ws.Cell(row, column++).Style.Alignment.WrapText = true;


                    ws.Cell(row, column).Value = mathTrack;
                    ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    ws.Cell(row, column++).Style.Alignment.WrapText = true;

                    ws.Cell(row, column).Value = otherTrack;
                    ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    ws.Cell(row, column++).Style.Alignment.WrapText = true;
                    
                    ws.Cell(row, column).Value = predefind;
                    ws.Cell(row++, column++).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }
                else
                {
                    column = 1;
                    ws.Cell(row, column).SetValue<string>(item.Code);
                    ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Cell(row, column++).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(row, column).Value = item.Title;
                    ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Cell(row, column++).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(row, column).Value = item.FullName;
                    ws.Cell(row, column++).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    ws.Cell(row, column).Value = item.Major;
                    ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Cell(row, column++).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(row, column).SetValue<string>(item.IntakeTerm);
                    ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Cell(row, column++).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(row, column++).Value = string.Empty;
                    ws.Cell(row, column++).Value = string.Empty;
                    ws.Cell(row, column++).Value = string.Empty;
                    ws.Cell(row++, column).Value = predefind;
                }
            }
            ws.Columns().AdjustToContents();            
            ws.Rows().AdjustToContents();
            return wb;
            // var wb = new XLWorkbook();
            // var ws = wb.AddWorksheet();
            // int row = 1;
            // var column = 1;
            // ws.Cell(row, column++).Value = "Code";
            // ws.Cell(row, column++).Value = "Name";
            // ws.Cell(row, column++).Value = "Major";
            // ws.Cell(row, column++).Value = "Ability";
            // ws.Cell(row++, column++).Value = "Predefind Course";

            // foreach (var item in results)
            // {
            //     var predefind = string.Empty;
            //     if (item.PredefindCourses != null && item.PredefindCourses.Any())
            //     {
            //         predefind = string.Join(", ", item.PredefindCourses);
            //     }

            //     column = 1;
            //     if (item.Abilities != null && item.Abilities.Any())
            //     {
            //         var rowCount = item.Abilities.Count();
            //         var mergeRows = row + rowCount - 1;
            //         ws.Cell(row, column).SetValue<string>(item.Code);
            //         ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            //         ws.Range(ws.Cell(row, column), ws.Cell(mergeRows, column)).Merge();
            //         column++;

            //         ws.Cell(row, column).Value = item.FullName;
            //         ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            //         ws.Range(ws.Cell(row, column), ws.Cell(mergeRows, column)).Merge();
            //         column++;

            //         ws.Cell(row, column).Value = item.Major;
            //         ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            //         ws.Range(ws.Cell(row, column), ws.Cell(mergeRows, column)).Merge();
            //         column++;
                    
            //         var _row = row;
            //         foreach (var ability in item.Abilities)
            //         {
            //             ws.Cell(_row++, column).Value = ability;
            //         }

            //         column++;
            //         ws.Cell(row, column).Value = predefind;
            //         ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            //         ws.Range(ws.Cell(row, column), ws.Cell(mergeRows, column)).Merge();
            //         row += rowCount;
            //     }
            //     else
            //     {
            //         ws.Cell(row, column++).SetValue<string>(item.Code);
            //         ws.Cell(row, column++).Value = item.FullName;
            //         ws.Cell(row, column++).Value = item.Major;
            //         ws.Cell(row, column++).Value = string.Empty;
            //         ws.Cell(row++, column).Value = predefind;
            //     }
            // }

            // return wb;
        }

        private XLWorkbook GenerateWorkBookPivot(List<StudentAbilityReprotViewModel> results)
        {
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet();
            int row = 1;
            var column = 1;
            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row, column++).Value = "Code";

            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row, column++).Value = "Title";

            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row, column++).Value = "Name";

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row, column++).Value = "Major";

            ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row, column++).Value = "Intake Term";

            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row, column++).Value = "Ability";

            ws.Cell(row, column).Style.Font.Bold = true;
            ws.Cell(row, column).Style.Fill.BackgroundColor = XLColor.FromArgb(184, 204, 228);
            ws.Cell(row++, column++).Value = "Predefind Course";

            foreach (var item in results)
            {
                var predefind = string.Empty;
                if (item.PredefindCourses != null && item.PredefindCourses.Any())
                {
                    predefind = string.Join(", ", item.PredefindCourses);
                }

                if (item.Abilities != null && item.Abilities.Any())
                {
                    foreach(var ability in item.Abilities)
                    {
                        column = 1;
                        ws.Cell(row, column).SetValue<string>(item.Code);
                        ws.Cell(row, column++).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        ws.Cell(row, column).Value = item.Title;
                        ws.Cell(row, column++).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                        ws.Cell(row, column).Value = item.FullName;
                        ws.Cell(row, column++).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                        ws.Cell(row, column).Value = item.Major;
                        ws.Cell(row, column++).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        ws.Cell(row, column).SetValue<string>(item.IntakeTerm);
                        ws.Cell(row, column++).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        ws.Cell(row, column).Value = ability;
                        ws.Cell(row, column++).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                        ws.Cell(row, column).Value = predefind;
                        ws.Cell(row++, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    }
                }
                else
                {
                    column = 1;
                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, column++).SetValue<string>(item.Code);

                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, column++).Value = item.Title;

                    ws.Cell(row, column++).Value = item.FullName;

                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, column++).Value = item.Major;

                    ws.Cell(row, column).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, column++).SetValue<string>(item.IntakeTerm);

                    ws.Cell(row, column++).Value = string.Empty;
                    ws.Cell(row++, column).Value = predefind;
                }
            }

            ws.Columns().AdjustToContents();
            ws.Rows().AdjustToContents();
            return wb;
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0, long curriculumnId = 0)
        {
            ViewBag.Nationalities = _selectListProvider.GetNationalities();
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Abilities = _selectListProvider.GetAbilities();
            ViewBag.Statuses = _selectListProvider.GetStudentStatuses();
            if (academicLevelId > 0)
            {
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
                ViewBag.Curriculums = _selectListProvider.GetCurriculumByAcademicLevelId(academicLevelId);
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                if (curriculumnId > 0)
                {
                    ViewBag.CurriculumVersions = _selectListProvider.GetCurriculumVersion(curriculumnId);
                }
                
                if (facultyId > 0)
                {
                    ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
                }
            }
        }
    }
}