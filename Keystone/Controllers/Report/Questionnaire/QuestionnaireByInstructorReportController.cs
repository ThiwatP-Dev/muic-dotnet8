using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class QuestionnaireByInstructorReportController : BaseController
    {
        protected readonly IQuestionnaireProvider _questionnaireProvider;

        public QuestionnaireByInstructorReportController(ApplicationDbContext db,
                                                         ISelectListProvider selectListProvider,
                                                         IFlashMessage flashMessage,
                                                         IQuestionnaireProvider questionnaireProvider) : base(db, flashMessage, selectListProvider)
        {
            _questionnaireProvider = questionnaireProvider;
        }

        public IActionResult Index(Criteria criteria, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            var userInstructorId = GetInstructorId();

            bool IsAllow = false;
            bool IsInstructor = false;

            // QUESTIONNAIRE MEMBER
            var IsQuestionnaireMember = _db.QuestionnaireMembers.Any(x => x.InstructorId == userInstructorId);
            IsAllow = IsQuestionnaireMember;

            // PROGRAM DIRECTOR
            if (!IsAllow)
            {
                var questionnaireMemberApproved = _db.QuestionnaireApprovals.Where(x => x.InstructorId == criteria.InstructorId
                                                                                        && x.TermId == criteria.TermId)
                                                                            .All(x => x.Status == "s");

                var facultyMember = _db.FacultyMembers.AsNoTracking()
                                                      .Where(x => x.InstructorId == userInstructorId && x.Type == "pd")
                                                      .ToList();
 
                if (facultyMember != null && facultyMember.Any())
                {
                    IsAllow = questionnaireMemberApproved;
                }
            }

            // INSTRUCTOR
            if (!IsAllow)
            {
                var pdApproved = _db.QuestionnaireApprovals.Where(x => x.InstructorId == criteria.InstructorId
                                                                     && x.TermId == criteria.TermId)
                                                           .All(x => x.Status == "p");

                IsAllow = (criteria.InstructorId == userInstructorId && pdApproved);
                IsInstructor = true;
            } 

            if (IsInstructor)
            {
                criteria.InstructorId = userInstructorId;
            }
            
            if (criteria.AcademicLevelId == 0)
            {
                criteria.AcademicLevelId = _db.AcademicLevels.AsNoTracking()
                                                             .SingleOrDefault(x => x.NameEn.ToLower().Contains("bachelor")).Id;
                criteria.TermId = _questionnaireProvider.GetQuestionnaireTermByAcademicLevelId(criteria.AcademicLevelId).Id;
                
                CreateSelectList(criteria.AcademicLevelId, criteria.InstructorId);
                _flashMessage.Warning(Message.RequiredData);
                return View(new QuestionnaireByInstructorAndSectionViewModel
                            {
                                Criteria = criteria
                            });
            }

            CreateSelectList(criteria.AcademicLevelId, criteria.InstructorId);

            if (!IsAllow)
            {
                return View(null);
            }

            var students = _db.Responses.AsNoTracking()
                                        .Where(x => x.TermId == criteria.TermId
                                                    && x.InstructorId == criteria.InstructorId)
                                        .Select(x => new QuestionnaireByInstructorAndSectionStudent
                                                     {
                                                         StudentId = x.StudentId,
                                                         TermId = x.TermId,
                                                         SectionId = x.SectionId.Value,
                                                         InstructorId = x.InstructorId.Value,
                                                         QuestionGroupId = x.QuestionGroupId,
                                                         QuestionId = x.Answer.QuestionId,
                                                         StudentCode = x.Student.Code,
                                                         Title = x.Student.Title.NameEn,
                                                         FirstName = x.Student.FirstNameEn,
                                                         MidName = x.Student.MidNameEn,
                                                         LastName = x.Student.LastNameEn,
                                                         Value = x.Answer.ValueDecimal,
                                                         ValueText = x.AnswerRemark,
                                                         Type = x.Answer.Question.QuestionType,
                                                         IsCalulate = x.Answer.Question.IsCalculate
                                                     })
                                        .ToList();

            var model = _questionnaireProvider.GetQuestionnaireByInstructorAndSectionReport(students);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(ValueCountLimit = Int32.MaxValue)]
        public IActionResult ExportExcel(QuestionnaireByInstructorAndSectionViewModel model, string returnUrl)
        {
            if (model.Students != null && model.Students.Any())
            {
                using (var wb = GenerateWorkBook(model))
                {
                    return wb.Deliver($"Questionnaire By Instructor Report.xlsx");
                }
            }

            return Redirect(returnUrl);
        }

        private XLWorkbook GenerateWorkBook(QuestionnaireByInstructorAndSectionViewModel model)
        {
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet();
            int row = 1;
            var column = 1;
            var questionColumn = 1;

            ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range(ws.Cell(row, column), ws.Cell(2, column)).Merge();
            ws.Cell(row, column++).Value = "No.";
            var _row = row;
            foreach (var group in model.Header)
            {
                var mergeCells = column + group.Questions.Count - 1;
                ws.Range(ws.Cell(_row, column), ws.Cell(_row, mergeCells)).Merge();
                if (group.Questions.All(x => x.Type == "s"))
                {
                    ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Range(ws.Cell(row, column), ws.Cell(2, column)).Merge();
                }

                ws.Cell(_row++, column).Value = group.GroupName;
                foreach (var question in group.Questions)
                {
                    if (question.Type != "s")
                    {
                        ws.Cell(_row, column++).Value = question.Order + (question.IsCalculate ? "*" : "");
                    }
                    else
                    {
                        column++;
                    }
                    questionColumn++;
                }
                _row--;
            }

            ws.Cell(row, column).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range(ws.Cell(row, column), ws.Cell(2, column)).Merge();
            ws.Cell(row++, column++).Value = "Total";
            row++;

            int index = 1;
            foreach (var student in model.Students)
            {
                column = 1;
                // var fullName = string.IsNullOrEmpty(student.MidName) ? $"{ student.Title } { student.FirstName } { student.LastName }"
                //                                                      : $"{ student.Title } { student.FirstName } { student.MidName } { student.LastName }";
                ws.Cell(row, column).Value = index++;
                column++;

                foreach (var question in student.Questions)
                {
                    if (question.Type == "s")
                    {
                        ws.Cell(row, column).Value = question.AnswerText;
                        column++;
                    }
                    else
                    {
                        ws.Cell(row, column).Value = question.Answer;
                        column++;
                    }
                }

                ws.Cell(row, column).Value = student.Average.ToString(StringFormat.TwoDecimal);

                row += 1;
            }

            column = 1;
            ws.Range(ws.Cell(row, column), ws.Cell(row, questionColumn)).Merge();
            ws.Cell(row, column).Value = "Total";
            column++;

            ws.Cell(row, questionColumn + 1).Value = model.TotalAverage.ToString(StringFormat.TwoDecimal);
            return wb;
        }

        private void CreateSelectList(long academicLevelId = 0, long instructorId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            ViewBag.Instructors = _selectListProvider.GetInstructorById(instructorId);
        }
    }
}