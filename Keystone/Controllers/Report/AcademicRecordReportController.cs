using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers.Report
{
    [PermissionAuthorize("AcademicRecordReport", "")]
    public class AcademicRecordReportController : BaseController
    {
        protected readonly IReportProvider _reportProvider;
        protected readonly IInstructorProvider _instructorProvider;
        protected readonly IStudentProvider _studentProvider;
        protected readonly IStudentPhotoProvider _studentPhotoProvider;

        private UserManager<ApplicationUser> _userManager { get; }

        public AcademicRecordReportController(ApplicationDbContext db,
                                              IFlashMessage flashMessage,
                                              ISelectListProvider selectListProvider,
                                              UserManager<ApplicationUser> user,
                                              IReportProvider reportProvider,
                                              IInstructorProvider instructorProvider,
                                              IStudentProvider studentProvider,
                                              IStudentPhotoProvider studentPhotoProvider) : base(db, flashMessage, selectListProvider)
        {
            _reportProvider = reportProvider;
            _instructorProvider = instructorProvider;
            _userManager = user;
            _studentProvider = studentProvider;
            _studentPhotoProvider = studentPhotoProvider;
        }

        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.FacultyId, criteria.Code, criteria.Language);
            var model = new TranscriptViewModel();
            model.Criteria = criteria;
            if ((string.IsNullOrEmpty(criteria.Code) && criteria.StudentCodeFrom == null && criteria.StudentCodeTo == null) || string.IsNullOrEmpty(criteria.Language))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }
            
            var transcripts = new List<TranscriptInformation>();
            if (!string.IsNullOrEmpty(criteria.Code))
            {
                var student = _reportProvider.GetStudentInformationForTranscript(criteria.Code, criteria);
                if (student != null)
                {
                    var transcript = _reportProvider.GetTranscript(student, criteria.Language, false);
                    transcripts.Add(transcript);
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
        public async Task<ActionResult> Preview(Criteria criteria)
        {
            if (!string.IsNullOrEmpty(criteria.Code)
                || (criteria.StudentCodeFrom != null && criteria.StudentCodeTo != null))
            {
                var transcripts = new List<TranscriptInformation>();
                string profileImageURL = "";
                if (!string.IsNullOrEmpty(criteria.Code))
                {
                    var student = _reportProvider.GetStudentInformationForTranscript(criteria.Code, criteria);
                    if (student != null)
                    {
                        var transcript = _reportProvider.GetTranscript(student, criteria.Language, true);
                        profileImageURL = student.ProfileImageURL;
                        try
                        {
                            profileImageURL = await _studentPhotoProvider.GetStudentImg(transcript.StudentCode);
                        }
                        catch (Exception) { }

                        transcripts.Add(transcript);
                    }
                }
                else if (criteria.StudentCodeFrom != null && criteria.StudentCodeTo != null)
                {
                    transcripts = GetMultipleStudents(criteria);

                    foreach (var student in transcripts)
                    {
                        try
                        {
                            student.ProfileImageURL = await _studentPhotoProvider.GetStudentImg(student.StudentCode);
                        }
                        catch (Exception) { }
                    }
                }
                
                var user = await _userManager.GetUserAsync(User);
                var report = new ReportViewModel
                             {
                                 Title = Certificate.Transcript,
                                 ProfileImageURL = profileImageURL,
                                 Subject = Certificate.Transcript,
                                 Creator = user == null ? "" : user.NormalizedUserName,
                                 Author = user == null ? "" : user.NormalizedUserName,
                                 Criteria = criteria,
                                 Body = transcripts
                              };

                return View(report);
            }
            else
            { 
                _flashMessage.Danger(Message.UnableToCreate);
                return RedirectToAction(nameof(Index), criteria);
            }
        }

        private void CreateSelectList(long facultyId = 0, string code = null, string language = null)
        {
            ViewBag.Languages = _selectListProvider.GetLanguages();
            ViewBag.Purposes = _selectListProvider.GetCertificatePurposes();
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
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
                    var student = _reportProvider.GetStudentInformationForTranscript(studentCode.ToString(), criteria);
                    if (student != null)
                    {
                        var transcript = _reportProvider.GetTranscript(student, criteria.Language, true);
                        transcripts.Add(transcript);
                    }
                }
            }

            return transcripts;
        }
    }
}