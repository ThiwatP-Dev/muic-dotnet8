using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers.Report
{
    [PermissionAuthorize("MUICGradeReport", "")]
    public class MUICGradeReportController : BaseController
    {
        protected readonly IReportProvider _reportProvider;
        protected readonly IInstructorProvider _instructorProvider;
        protected readonly IStudentProvider _studentProvider;
        protected readonly IAcademicProvider _academicProvider;
        protected readonly IStudentPhotoProvider _studentPhotoProvider;


        private UserManager<ApplicationUser> _userManager { get; }
        
        public MUICGradeReportController(ApplicationDbContext db,
                                         IFlashMessage flashMessage,
                                         ISelectListProvider selectListProvider,
                                         UserManager<ApplicationUser> user,
                                         IReportProvider reportProvider,
                                         IInstructorProvider instructorProvider,
                                         IStudentProvider studentProvider,
                                         IStudentPhotoProvider studentPhotoProvider,
                                         IAcademicProvider academicProvider) : base(db, flashMessage, selectListProvider)
        {
            _reportProvider = reportProvider;
            _instructorProvider = instructorProvider;
            _userManager = user;
            _studentProvider = studentProvider;
            _academicProvider = academicProvider;
            _studentPhotoProvider = studentPhotoProvider;
        }

        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.Code, criteria.Language, criteria.FacultyId, criteria.AcademicLevelId, criteria.DepartmentId);
            var model = new TranscriptViewModel();
            model.Criteria = criteria;
            if ((string.IsNullOrEmpty(criteria.Code) 
                && criteria.StudentCodeFrom == null 
                && criteria.StudentCodeTo == null) 
                || string.IsNullOrEmpty(criteria.Language))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }
            
            var transcripts = new List<TranscriptInformation>();
            if (!string.IsNullOrEmpty(criteria.Code))
            {
                if (_studentProvider.IsExistStudentCodeAndStatus(criteria.Code, criteria.Status))
                {
                    var student = _reportProvider.GetStudentInformationForTranscript(criteria.Code, criteria);
                    if (student != null)
                    {
                        _studentProvider.UpdateCGPA(student.Id);
                        var transcript = _reportProvider.GetTranscript(student, criteria.Language, false);
                        transcripts.Add(transcript);
                    }
                }
            }
            else if (criteria.StudentCodeFrom != null && criteria.StudentCodeTo != null)
            {
                transcripts = GetMultipleStudents(criteria);
            }
            
            if (transcripts.Any())
            {
                model.Transcripts = transcripts;
            }
            else
            {
                _flashMessage.Danger(Message.StudentNotFound);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableCors("CorsPolicy")]
        public async Task<ActionResult> Preview(Criteria criteria, TranscriptViewModel model)
        {
            if (!string.IsNullOrEmpty(criteria.Code)
                || (criteria.StudentCodeFrom != null && criteria.StudentCodeTo != null))
            {
                var transcripts = new List<TranscriptInformation>();
                if (!string.IsNullOrEmpty(criteria.Code))
                {
                    if (_studentProvider.IsExistStudentCodeAndStatus(criteria.Code, criteria.Status))
                    {
                        var student = _reportProvider.GetStudentInformationForTranscript(criteria.Code, criteria);
                        if (student != null)
                        {
                            _studentProvider.UpdateCGPA(student.Id);
                            var transcript = _reportProvider.GetTranscript(student, criteria.Language, true);
                            transcripts.Add(transcript);
                        }
                    }
                }
                else if (criteria.StudentCodeFrom != null && criteria.StudentCodeTo != null)
                {
                    transcripts = GetMultipleStudentsPreview(criteria, model);
                }

                foreach (var student in transcripts)
                {
                    try
                    {
                        student.ProfileImageURL = await _studentPhotoProvider.GetStudentImg(student.StudentCode);
                    }
                    catch (Exception) { }
                }

                var user = await _userManager.GetUserAsync(User);
                var report = new ReportViewModel
                             {
                                 Title = Certificate.Transcript,
                                 Subject = Certificate.Transcript,
                                 Creator = user == null ? "" : user.NormalizedUserName,
                                 Author = user == null ? "" : user.NormalizedUserName,
                                 Criteria = criteria
                              };
                
                report.Body = transcripts;
                CreateSelectList();
                return View(report);
            }
            else
            { 
                _flashMessage.Danger(Message.UnableToCreate);
                return RedirectToAction(nameof(Index), criteria);
            }
        }

        private void CreateSelectList(string code = null, string language = null, long facultyId = 0, long academicLevelId = 0, long termId = 0)
        {
            ViewBag.Languages = _selectListProvider.GetLanguages();
            ViewBag.Purposes = _selectListProvider.GetCertificatePurposes();
            ViewBag.StudentStatuses = _selectListProvider.GetStudentStatuses();
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.StudentProfileStatuses = _selectListProvider.GetStudentProfileStatuses();
            if(academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
            ViewBag.Faculties = _selectListProvider.GetFaculties();
            ViewBag.Signatories = _selectListProvider.GetSignatories();
            ViewBag.CurriculumVersions = _selectListProvider.GetCurriculumVersionsByCurriculumInformation(code, language);
            
            if (facultyId != 0)
            {
                ViewBag.Departments = _selectListProvider.GetDepartments(facultyId);
            }
        }

        private List<TranscriptInformation> GetMultipleStudents(Criteria criteria)
        {
            var transcripts = new List<TranscriptInformation>();
            if (criteria.StudentCodeFrom < criteria.StudentCodeTo)
            {
                for (var studentCode = criteria.StudentCodeFrom; studentCode <= criteria.StudentCodeTo; studentCode++)
                {
                    if (_studentProvider.IsExistStudentCodeAndStatus(studentCode.ToString(), criteria.Status))
                    {
                        var student = _reportProvider.GetStudentInformationForTranscript(studentCode.ToString(), criteria);
                        if (student != null)
                        {
                            _studentProvider.UpdateCGPA(student.Id);
                            var transcript = _reportProvider.GetTranscript(student, criteria.Language, true);
                            transcripts.Add(transcript);
                        }
                    }
                }
            }

            return transcripts;
        } 

        private List<TranscriptInformation> GetMultipleStudentsPreview(Criteria criteria, TranscriptViewModel model)
        {
            var transcripts = new List<TranscriptInformation>();
            if (criteria.StudentCodeFrom < criteria.StudentCodeTo)
            {
                for (var studentCode = criteria.StudentCodeFrom; studentCode <= criteria.StudentCodeTo; studentCode++)
                {
                    if (_studentProvider.IsExistStudentCodeAndStatus(studentCode.ToString(), criteria.Status))
                    {
                        var student = _reportProvider.GetStudentInformationForTranscript(studentCode.ToString(), criteria);
                        if (student != null)
                        {
                            var transcript = _reportProvider.GetTranscript(student, criteria.Language, true);
                            // var mapTranscript = _reportProvider.MapTranscriptPreview(transcript, model);
                            // transcript = mapTranscript;
                            transcripts.Add(transcript);
                        }
                    }
                }
            }

            return transcripts;
        } 

        public TranscriptInformation TranscriptGetFacutyAndDepartment(long curriculumVersionId, string language)
        {
            var curriculumVersion = _academicProvider.GetFacultyAndDepartmentByCurriculumVersionId(curriculumVersionId, language);
            return curriculumVersion;
        } 
    }
}