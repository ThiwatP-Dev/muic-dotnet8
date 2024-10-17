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
    public class TranscriptController : BaseController
    {
        protected readonly IReportProvider _reportProvider;
        protected readonly IInstructorProvider _instructorProvider;
        protected readonly IStudentProvider _studentProvider;
        protected readonly IAcademicProvider _academicProvider;

        private UserManager<ApplicationUser> _userManager { get; }
        
        public TranscriptController(ApplicationDbContext db,
                                    IFlashMessage flashMessage,
                                    ISelectListProvider selectListProvider,
                                    UserManager<ApplicationUser> user,
                                    IReportProvider reportProvider,
                                    IInstructorProvider instructorProvider,
                                    IStudentProvider studentProvider,
                                    IAcademicProvider academicProvider) : base(db, flashMessage, selectListProvider)
        {
            _reportProvider = reportProvider;
            _instructorProvider = instructorProvider;
            _userManager = user;
            _studentProvider = studentProvider;
            _academicProvider = academicProvider;
        }

        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.Code, criteria.Language, criteria.FacultyId);
            var model = new TranscriptViewModel();
            model.Criteria = criteria;
            if ((string.IsNullOrEmpty(criteria.Code) && criteria.StudentCodeFrom == null && criteria.StudentCodeTo == null) ||
                (string.IsNullOrEmpty(criteria.Type) && string.IsNullOrEmpty(criteria.Language)
                 && criteria.ApprovedById == 0))
            {
                _flashMessage.Warning(Message.RequiredData);
                return View(model);
            }
            
            var transcripts = new List<TranscriptInformation>();
            if (!string.IsNullOrEmpty(criteria.Code))
            {
                if (_studentProvider.IsExistStudent(criteria.Code))
                {
                    var student = _reportProvider.GetStudentInformationForTranscript(criteria.Code, criteria);
                    if (student != null)
                    {
                        var transcript = _reportProvider.GetTranscript(student, criteria.Language, false);
                        transcripts.Add(transcript);
                    }
                }
            }
            else if (criteria.StudentCodeFrom != null && criteria.StudentCodeTo != null)
            {
                transcripts = GetMultipleStudents(criteria, null);
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
        public async Task<ActionResult> Preview(Criteria criteria, TranscriptViewModel model)
        {
            if (!string.IsNullOrEmpty(criteria.Code)
                || (criteria.StudentCodeFrom != null && criteria.StudentCodeTo != null))
            {
                var transcripts = new List<TranscriptInformation>();
                if (criteria.ApprovedById != 0)
                {
                    var signatory = _reportProvider.GetSignatoryNameById(criteria.ApprovedById, criteria.Language);

                    if (!string.IsNullOrEmpty(criteria.Code))
                    {
                        if (_studentProvider.IsExistStudent(criteria.Code))
                        {
                            var student = _reportProvider.GetStudentInformationForTranscript(criteria.Code, criteria);
                            if (student != null)
                            {
                                var transcript = _reportProvider.GetTranscript(student, criteria.Language, true);
                                var mapTranscript = _reportProvider.MapTranscriptPreview(transcript, model);
                                transcript.ApprovedBy = signatory;
                                transcript = mapTranscript;
                                transcripts.Add(transcript);
                            }
                        }
                    }
                    else if (criteria.StudentCodeFrom != null && criteria.StudentCodeTo != null)
                    {
                        transcripts = GetMultipleStudents(criteria, signatory);
                        foreach (var item in model.Transcripts)
                        {
                            foreach (var project in item.ProjectNames)
                            {
                                if (project != null)
                                {
                                    transcripts.Where(x => x.StudentCode == item.StudentCode)
                                               .Select(x => {
                                                                x.ProjectNames = item.ProjectNames;
                                                                return x;
                                                            })
                                               .ToList();
                                }
                            }
                        }
                    }
                }
                else
                { 
                    _flashMessage.Danger(Message.UnableToCreate);
                    return RedirectToAction(nameof(Index));
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
                
                if (transcripts.Count > 1)
                {
                    report.Body = transcripts;
                }
                else if (transcripts.Count == 1)
                {
                    report.Body = transcripts[0];
                }

                return View(report);
            }
            else
            { 
                _flashMessage.Danger(Message.UnableToCreate);
                return RedirectToAction(nameof(Index), criteria);
            }
        }

        private void CreateSelectList(string code = null, string language = null, long facultyId = 0)
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

        private List<TranscriptInformation> GetMultipleStudents(Criteria criteria, string signatory)
        {
            var transcripts = new List<TranscriptInformation>();
            if (criteria.StudentCodeFrom < criteria.StudentCodeTo)
            {
                for (var studentCode = criteria.StudentCodeFrom; studentCode <= criteria.StudentCodeTo; studentCode++)
                {
                    if (_studentProvider.IsExistStudent(studentCode.ToString()))
                    {
                        var student = _reportProvider.GetStudentInformationForTranscript(studentCode.ToString(), criteria);
                        if (student != null)
                        {
                            var transcript = _reportProvider.GetTranscript(student, criteria.Language, true);
                            transcript.ApprovedBy = signatory;
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