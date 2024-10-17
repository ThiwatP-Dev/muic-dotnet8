using Keystone.BackgroundTask;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Questionnaire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    public class QuestionnaireApprovalController : BaseController
    {
        protected readonly IDateTimeProvider _dateTimeProvider;
        protected readonly IUserProvider _userProvider;
        protected readonly IAcademicProvider _academicProvider;
        protected readonly IQuestionnaireProvider _questionnaireProvider;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly BackgroundWorkerQueue _backgroundWorkerQueue;

        public QuestionnaireApprovalController(ApplicationDbContext db,
                                               IFlashMessage flashMessage,
                                               IDateTimeProvider dateTimeProvider,
                                               ISelectListProvider selectListProvider,
                                               IUserProvider userProvider,
                                               IAcademicProvider academicProvider,
                                               IQuestionnaireProvider questionnaireProvider,
                                               IServiceScopeFactory serviceScopeFactory,
                                               BackgroundWorkerQueue backgroundWorkerQueue) : base(db, flashMessage, selectListProvider) 
        { 
            _dateTimeProvider = dateTimeProvider;
            _userProvider = userProvider;
            _academicProvider = academicProvider;
            _questionnaireProvider = questionnaireProvider;
            _backgroundWorkerQueue = backgroundWorkerQueue;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public ActionResult Index(Criteria criteria, int page)
        {
            
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            if (criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            if(!string.IsNullOrEmpty(criteria.IsRecalcScore) && Convert.ToBoolean(criteria.IsRecalcScore))
            {
                //THIS IS RISKY HACKED PLEASE USE RESPONSIBILY OR MAYBE Move it to have a proper feature later
                _backgroundWorkerQueue.QueueBackgroundWorkItem(async token =>
                {
                    try
                    {
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            var questionaireProvider = scope.ServiceProvider.GetRequiredService<IQuestionnaireProvider>();

                            questionaireProvider.BuildScore(criteria.TermId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _flashMessage.Warning(ex.Message + "\n" + ex.StackTrace);
                    }

                });
                _flashMessage.Confirmation("Create job to recal the score. Just wait.");
            }

            var userInstructorId = GetInstructorId();
            var model = new QuestionnaireApprovalViewModel();

            // QUESTIONNAIRE MEMBER
            var IsQuestionnaireMember = _db.QuestionnaireMembers.Any(x => x.InstructorId == userInstructorId);

            // PROGRAM DIRECTOR
            var facultyMember = _db.FacultyMembers.AsNoTracking()
                                                  .Where(x => x.InstructorId == userInstructorId && x.Type == "pd")
                                                  .ToList();

            var sectionViewModel = new List<QuestionnaireApprovalSection>();
            var courseIds = _db.FilterCourseGroupDetails.Where(x => x.FilterCourseGroupId == criteria.FilterCourseGroupId)
                                                        .Select(x => x.CourseId)
                                                        .ToList();

            if (IsQuestionnaireMember || (facultyMember != null && facultyMember.Any()))
            {
                var questionnaires = _db.QuestionnaireApprovals.AsNoTracking()
                                                               .IgnoreQueryFilters()
                                                               .Include(x => x.Course)
                                                                   .ThenInclude(x => x.Faculty)
                                                               .Include(x => x.Section)
                                                               .Include(x => x.Instructor)
                                                                  .ThenInclude(x => x.Title)
                                                               .Where(x => x.IsActive 
                                                                            && x.TermId == criteria.TermId
                                                                            && (criteria.FacultyId == 0 
                                                                                || x.Course.FacultyId == criteria.FacultyId)
                                                                            && (criteria.FilterCourseGroupId == 0 
                                                                                || courseIds.Contains(x.CourseId))
                                                                            && (string.IsNullOrEmpty(criteria.CourseCode) 
                                                                                || x.Course.Code.Contains(criteria.CourseCode)))
                                                               .ToList();

                var allSectionId = questionnaires.Select(x => x.SectionId).Distinct().ToList();
                var allRelatedResponse = (from response in _db.Responses.AsNoTracking()
                                          where response.IsActive
                                                     && allSectionId.Contains(response.SectionId.Value)
                                          group response.StudentId by new
                                          {
                                              SectionId = response.SectionId.Value,
                                              InstructorId = response.Section.MainInstructorId,
                                          } into responseGroup
                                          select new
                                          {
                                              SectionId = responseGroup.Key.SectionId,
                                              InstructorId = responseGroup.Key.InstructorId,
                                              Group = responseGroup
                                          }).ToList();


                foreach (var questionnaire in questionnaires)
                {                        
                    var responseCount = allRelatedResponse.FirstOrDefault(x => x.SectionId == questionnaire.SectionId 
                                                                                    && x.InstructorId == questionnaire.InstructorId)
                                                          ?.Group.Distinct().Count() ?? questionnaire.TotalSurvey;
                    var questionnaireApprovalSection = new QuestionnaireApprovalSection()
                                                        {
                                                            FacultyId = questionnaire.Course.FacultyId,
                                                            DepartmentId = questionnaire.Course.DepartmentId,
                                                            CourseId = questionnaire.CourseId,
                                                            SectionId = questionnaire.SectionId,
                                                            InstructorId = questionnaire.InstructorId ?? 0,
                                                            CourseCode = questionnaire.Course.Code,
                                                            CourseName = questionnaire.Course.NameEn,
                                                            Faculty = questionnaire.Course.Faculty.Abbreviation,
                                                            SectionType = questionnaire.Section.ParentSectionId == null ? "Master" : "Joint",
                                                            CourseLab = questionnaire.Course.Lab,
                                                            CourseOther = questionnaire.Course.Other,
                                                            CourseLecture = questionnaire.Course.Lecture,
                                                            CourseCredit = questionnaire.Course.Credit,
                                                            Section = questionnaire.Section.Number,
                                                            InstructorTitle = questionnaire.Instructor.Title.NameEn,
                                                            InstructorName = $"{ questionnaire.Instructor.FirstNameEn } { questionnaire.Instructor.LastNameEn }",
                                                            RegisteredStudents = questionnaire.TotalEnrolled,
                                                            StudentServey = responseCount,
                                                            StudentServeyThatCount = questionnaire.TotalSurvey,
                                                            Status = questionnaire.Status,
                                                            QuestionnaireApprovalId = questionnaire.Id,
                                                            Total = questionnaire.TotalSectionScore,
                                                            TotalRelatedSection = questionnaire.TotalSectionMJScore,
                                                            TotalSD = questionnaire.TotalSectionSD,
                                                            TotalRelatedSectionSD = questionnaire.TotalSectionMJSD
                                                        };
                    sectionViewModel.Add(questionnaireApprovalSection);
                }

                //// QUESTIONNAIRE MEMBER
                //if (IsQuestionnaireMember)
                //{
                //    sectionViewModel = sectionViewModel.Where(x => x.Status == "w" || x.Status == "s").ToList();
                //} 
                // PROGRAM DIRECTOR
                //else 
                if (!IsQuestionnaireMember 
                    && facultyMember != null 
                    && facultyMember.Any())
                {
                    var filterCourseGroupIds = facultyMember.Select(x => x.FilterCourseGroupId);
                    var filterCourseIds = _db.FilterCourseGroupDetails.AsNoTracking()
                                                                      .Where(x => filterCourseGroupIds.Contains(x.FilterCourseGroupId))
                                                                      .Select(x => x.CourseId)
                                                                      .ToList();
                    sectionViewModel = sectionViewModel.Where(x => filterCourseIds.Contains(x.CourseId)
                                                                   && (x.Status == "s" || x.Status == "p")).ToList();
                }
            }

            model.Sections = sectionViewModel.Where(x => (x.Status == criteria.Status 
                                                          || string.IsNullOrEmpty(criteria.Status)))
                                             .OrderBy(x => x.Course)
                                                .ThenBy(x => x.Section)
                                             .ToList();

            var term = _academicProvider.GetTerm(criteria.TermId);
            model.AcademicLevelId = term.AcademicLevelId;
            model.TermId = criteria.TermId;
            return View(model);
        }

        public ActionResult IndexOld(Criteria criteria, int page)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            if (criteria.TermId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var userInstructorId = GetInstructorId();
            var model = new QuestionnaireApprovalViewModel();

            // QUESTIONNAIRE MEMBER
            var IsQuestionnaireMember = _db.QuestionnaireMembers.Any(x => x.InstructorId == userInstructorId);

            // PROGRAM DIRECTOR
            var facultyMember = _db.FacultyMembers.AsNoTracking()
                                                  .Where(x => x.InstructorId == userInstructorId && x.Type == "pd")
                                                  .ToList();

            var sectionViewModel = new List<QuestionnaireApprovalSection>();
            
            if (IsQuestionnaireMember || (facultyMember != null && facultyMember.Any()))
            {
                var sections = _db.Sections.IgnoreQueryFilters()
                                           .Include(x => x.Course)
                                           .Include(x => x.MainInstructor)
                                           .Include(x => x.SectionSlots)
                                                .ThenInclude(x => x.Instructor)
                                                .ThenInclude(x => x.Title)
                                           .AsNoTracking()
                                           .Where(x => x.TermId == criteria.TermId
                                                       && !x.IsClosed
                                                       && x.Status == "a")
                                           .ToList();

                // var sectionIds = sections.Select(x => x.Id).ToList();
                var sectionIdWithParentSectionIds = sections.Select(x => new { SectionId = x.Id, x.ParentSectionId }).ToList();

                var registrationCourses = _db.RegistrationCourses.AsNoTracking()
                                                                 .Where(x => x.SectionId != null
                                                                             && x.TermId == criteria.TermId
                                                                            //  && sectionIds.Contains(x.SectionId.Value)
                                                                             && x.Status != "d")
                                                                 .ToList();

                var responses = _db.Responses.Include(x => x.Answer)
                                             .AsNoTracking()
                                             .Where(x => x.TermId == criteria.TermId
                                                        //  && sectionIds.Contains(x.SectionId.Value)
                                                         && x.Answer.Question.IsCalculate
                                                         && x.Answer.Value != "0")
                                             .ToList();

                var questionnaireApprovals = _db.QuestionnaireApprovals.AsNoTracking()
                                                                       .Where(x => x.TermId == criteria.TermId
                                                                                //    sectionIds.Contains(x.SectionId)
                                                                                   )
                                                                       .ToList();

                var filterSections = sections.Where(x => (criteria.FacultyId == 0 
                                                             || x.Course.FacultyId == criteria.FacultyId)
                                                         && (criteria.DepartmentId == 0 
                                                           || x.Course.DepartmentId == criteria.DepartmentId)
                                                         && (string.IsNullOrEmpty(criteria.CourseCode) 
                                                           || x.CourseCode.Contains(criteria.CourseCode)))
                                             .ToList();
                                             
                foreach (var section in filterSections)
                {
                    var regisStudents = registrationCourses.Where(x => x.SectionId == section.Id).Count();

                    var instructors = section.SectionSlots.Where(x => x.InstructorId.HasValue)
                                                          .Select(x => new { x.InstructorId.Value, x.Instructor.FullNameEn })
                                                          .Distinct()
                                                          .ToList();

                    instructors = instructors.Distinct().ToList();

                    // JOINT SECTION
                    if (section.ParentSectionId.HasValue)
                    {
                        var masterSection = sections.FirstOrDefault(x => x.Id == section.ParentSectionId.Value);
                        if (masterSection != null)
                        {
                            var jointInstructors = masterSection.SectionSlots.Where(x => x.InstructorId.HasValue)
                                                                             .Select(x => new { x.InstructorId.Value, x.Instructor.FullNameEn })
                                                                             .Distinct()
                                                                             .ToList();
                            instructors = jointInstructors.Distinct().ToList(); 
                        }                                           
                    }
                    
                    if (section.MainInstructorId.HasValue)
                    {
                        if (!instructors.Any(x => x.Value == section.MainInstructorId.Value))
                        {
                            instructors.Add(new { section.MainInstructorId.Value, section.MainInstructor.FullNameEn });
                        }
                    }           
                                            
                    foreach (var instructor in instructors)
                    {
                        var approval = questionnaireApprovals.FirstOrDefault(x => x.SectionId == section.Id
                                                                                  && x.InstructorId == instructor.Value);
                        if (approval == null)
                        {
                            var newApproval = new QuestionnaireApproval()
                                              {
                                                CourseId = section.CourseId,
                                                SectionId = section.Id,
                                                InstructorId = instructor.Value,
                                                TermId = criteria.TermId,
                                                Status = "w"
                                              };
                                              
                            _db.QuestionnaireApprovals.Add(newApproval);
                            _db.SaveChanges();
                            approval = newApproval;
                        }

                        // RESPONSE RELATED SECTION
                        long parentSectionId = section.ParentSectionId.HasValue ? section.ParentSectionId.Value : section.Id;
                        var relatedSectionIds = sectionIdWithParentSectionIds.Where(x => x.SectionId == section.Id
                                                                                         || x.SectionId == parentSectionId
                                                                                         || x.ParentSectionId == parentSectionId)
                                                                             .Select(x => x.SectionId)
                                                                             .ToList();

                        var responseRelatedSection = responses.Any(x => relatedSectionIds.Contains(x.SectionId.Value)
                                                                        && x.InstructorId == instructor.Value) 
                                                        ? Math.Round(responses.Where(x => relatedSectionIds.Contains(x.SectionId.Value)
                                                                                          && x.InstructorId == instructor.Value)
                                                                   .Select(x => Convert.ToDecimal(x.Answer.Value)).Average(), 2) : 0;


                        // RESPONSE EACH SECTION
                        var response = responses.Any(x => x.SectionId == section.Id
                                                          && x.InstructorId == instructor.Value) 
                                            ? Math.Round(responses.Where(x => x.SectionId == section.Id
                                                                              && x.InstructorId == instructor.Value)
                                                       .Select(x => Convert.ToDecimal(x.Answer.Value)).Average(), 2)
                                            : 0;
                        
                        var totalSurveys = responses.Any(x => x.SectionId == section.Id
                                                              && x.InstructorId == instructor.Value) 
                                            ? responses.Where(x => x.SectionId == section.Id
                                                                              && x.InstructorId == instructor.Value)
                                                       .Select(x => x.StudentId).Distinct().Count()
                                            : 0;

                        var questionnaireApprovalSection = new QuestionnaireApprovalSection()
                                                           {
                                                                FacultyId = section.Course.FacultyId,
                                                                DepartmentId = section.Course.DepartmentId,
                                                                CourseId = section.CourseId,
                                                                SectionId = section.Id,
                                                                InstructorId = instructor.Value,
                                                                CourseCode = section.Course.Code,
                                                                CourseName = section.Course.NameEn,
                                                                SectionType = section.ParentSectionId == null ? "Master" : "Joint",
                                                                CourseLab = section.Course.Lab,
                                                                CourseOther = section.Course.Other,
                                                                CourseLecture = section.Course.Lecture,
                                                                CourseCredit = section.Course.Credit,
                                                                Section = section.Number,
                                                                InstructorName = instructor.FullNameEn,
                                                                RegisteredStudents = regisStudents,
                                                                StudentServey = totalSurveys,
                                                                Status = approval.Status,
                                                                QuestionnaireApprovalId = approval.Id,
                                                                Total = response,
                                                                TotalRelatedSection = responseRelatedSection
                                                           };
                        sectionViewModel.Add(questionnaireApprovalSection);
                    }
                }

                // QUESTIONNAIRE MEMBER
                if (IsQuestionnaireMember)
                {
                    sectionViewModel = sectionViewModel.Where(x => x.Status == "w" || x.Status == "s").ToList();
                } 
                // PROGRAM DIRECTOR
                else if (facultyMember != null && facultyMember.Any())
                {
                    var filterCourseGroupIds = facultyMember.Select(x => x.FilterCourseGroupId);
                    var filterCourseIds = _db.FilterCourseGroupDetails.AsNoTracking()
                                                                      .Where(x => filterCourseGroupIds.Contains(x.FilterCourseGroupId))
                                                                      .Select(x => x.CourseId)
                                                                      .ToList();
                    sectionViewModel = sectionViewModel.Where(x => filterCourseIds.Contains(x.CourseId)
                                                                   && (x.Status == "s" || x.Status == "p")).ToList();
                }
            }

            model.Sections = sectionViewModel.Where(x => (x.Status == criteria.Status 
                                                          || string.IsNullOrEmpty(criteria.Status)))
                                             .OrderBy(x => x.Course)
                                                .ThenBy(x => x.Section)
                                             .ToList();

            var term = _academicProvider.GetTerm(criteria.TermId);
            model.AcademicLevelId = term.AcademicLevelId;
            model.TermId = criteria.TermId;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(ValueCountLimit = Int32.MaxValue)]
        public ActionResult ChangeStatus(QuestionnaireApprovalViewModel model, string returnUrl)
        {
            var userInstructorId = GetInstructorId();

            // QUESTIONNAIRE MEMBER
            var IsQuestionnaireMember = _db.QuestionnaireMembers.Any(x => x.InstructorId == userInstructorId);

            // PROGRAM DIRECTOR
            var facultyMember = _db.FacultyMembers.AsNoTracking()
                                                  .Where(x => x.InstructorId == userInstructorId && x.Type == "pd")
                                                  .ToList();

            if (!facultyMember.Any() && !IsQuestionnaireMember)
            {
                _flashMessage.Danger(Message.UnableToEdit);
                return Redirect(returnUrl);
            }

            model.Sections = model.Sections.Where(x => x.IsChecked).ToList();
            var questionnaireApprovalIds = model.Sections.Select(x => x.QuestionnaireApprovalId).ToList();
            var questionnaireApprovalLogs = new List<QuestionnaireApprovalLog>();
            
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    if (IsQuestionnaireMember && model.Sections.Any(x => x.Status == "s"))
                    {
                        var questionnaireApprovals = _db.QuestionnaireApprovals.Where(x => x.Status == "s"
                                                                                           && questionnaireApprovalIds.Contains(x.Id))
                                                                               .ToList();

                        var questionnaireApprovalIdsStatusW = questionnaireApprovals.Select(x => x.Id).ToList();
                        questionnaireApprovalIds = questionnaireApprovalIds.Where(x => !questionnaireApprovalIdsStatusW.Contains(x)).ToList();
                        foreach (var item in questionnaireApprovals)
                        {
                            item.Status = "p";
                            var approvalLog = new QuestionnaireApprovalLog
                            {
                                QuestionnaireApprovalId = item.Id,
                                Status = "p"
                            };

                            questionnaireApprovalLogs.Add(approvalLog);
                        }
                        _db.QuestionnaireApprovalLogs.AddRange(questionnaireApprovalLogs);
                        _db.SaveChanges();
                    }

                    if (IsQuestionnaireMember && model.Sections.Any(x => x.Status == "w"))
                    {
                        var questionnaireApprovals = _db.QuestionnaireApprovals.Where(x => x.Status == "w" 
                                                                                           && questionnaireApprovalIds.Contains(x.Id))
                                                                               .ToList();

                        var questionnaireApprovalIdsStatusW =  questionnaireApprovals.Select(x => x.Id).ToList();
                        questionnaireApprovalIds = questionnaireApprovalIds.Where(x => !questionnaireApprovalIdsStatusW.Contains(x)).ToList();
                        foreach (var item in questionnaireApprovals)
                        {
                            item.Status = "s";
                            var approvalLog = new QuestionnaireApprovalLog
                                              {
                                                  QuestionnaireApprovalId = item.Id,
                                                  Status = "s"
                                              };

                            questionnaireApprovalLogs.Add(approvalLog);
                        }
                        _db.QuestionnaireApprovalLogs.AddRange(questionnaireApprovalLogs);
                        _db.SaveChanges();
                    }

                    if (facultyMember != null)
                    {
                        questionnaireApprovalLogs = new List<QuestionnaireApprovalLog>();
                        var filterCourseIds = new List<long>();
                        var filterCourseGroupIds = facultyMember.Select(x => x.FilterCourseGroupId).ToList();
                        filterCourseIds = _db.FilterCourseGroupDetails.AsNoTracking()
                                                                      .Where(x => filterCourseGroupIds.Contains(x.FilterCourseGroupId))
                                                                      .Select(x => x.CourseId)
                                                                      .ToList();

                        var questionnaireApprovals = _db.QuestionnaireApprovals.Where(x => x.Status == "s" 
                                                                                           && filterCourseIds.Contains(x.CourseId)
                                                                                           && questionnaireApprovalIds.Contains(x.Id))
                                                                               .ToList();
                        foreach (var item in questionnaireApprovals)
                        {
                            item.Status = "p";
                            var approvalLog = new QuestionnaireApprovalLog
                                              {
                                                QuestionnaireApprovalId = item.Id,
                                                Status = "p"
                                              };

                            questionnaireApprovalLogs.Add(approvalLog);
                        }
                        _db.QuestionnaireApprovalLogs.AddRange(questionnaireApprovalLogs);
                        _db.SaveChanges();
                    }
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return Redirect(returnUrl);
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToEdit);
                    transaction.Rollback();
                    return Redirect(returnUrl);
                }
            }
        }

        public ActionResult Details(long id)
        {
            var questionnaireApprovalLogs = _db.QuestionnaireApprovalLogs.AsNoTracking()
                                                                         .Where(x => x.QuestionnaireApprovalId == id)
                                                                         .Select(x => new QuestionnaireApprovalLogDetail
                                                                                      {
                                                                                          UpdateDate = x.UpdatedAt,
                                                                                          Status = x.Status,
                                                                                          UserId = x.UpdatedBy
                                                                                      })
                                                                         .ToList();

            var userIds = questionnaireApprovalLogs.Select(x => x.UserId).ToList();
            var users = _userProvider.GetCreatedFullNameByIds(userIds);
            foreach (var item in questionnaireApprovalLogs)
            {
                item.UpdateBy = users.Where(x => x.CreatedBy == item.UserId).FirstOrDefault().CreatedByFullNameEn;
                item.UpdateDate = (DateTime)_dateTimeProvider.ConvertFromUtcToSE(item.UpdateDate);
            }

            return PartialView("_Details", questionnaireApprovalLogs);
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            ViewBag.Courses = _selectListProvider.GetCourses();
            ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            ViewBag.FilterCourseGroups = _selectListProvider.GetFilterCourseGroupsByFacultyId(facultyId);
            ViewBag.Statuses = _selectListProvider.GetQuestionnaireApprovalStatuses();
            ViewBag.YesNoAnswers = _selectListProvider.GetNoYesAnswer();
        }
    }
}