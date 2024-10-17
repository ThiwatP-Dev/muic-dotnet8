using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Profile;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("Certificate", "")]
    public class CertificateController : BaseController
    {
        private UserManager<ApplicationUser> _userManager { get; }
        protected readonly IReportProvider _reportProvider;
        protected readonly IStudentProvider _studentProvider;
        protected readonly ICacheProvider _cacheProvider;
        protected readonly IAcademicProvider _academicProvider;
        protected readonly IAdmissionProvider _admissionProvider;
        protected readonly IRegistrationProvider _registrationProvider;
        protected readonly IReceiptProvider _receiptProvider;
        protected readonly IDateTimeProvider _dateTimeProvider;
        private static int currentYear = 0;

        public CertificateController(ApplicationDbContext db,
                                     IFlashMessage flashMessage,
                                     IMapper mapper,
                                     UserManager<ApplicationUser> user,
                                     IReportProvider reportProvider,
                                     ICacheProvider cacheProvider,
                                     IStudentProvider studentProvider,
                                     IAcademicProvider academicProvider,
                                     IAdmissionProvider admissionProvider,
                                     IRegistrationProvider registrationProvider,
                                     IReceiptProvider receiptProvider,
                                     IDateTimeProvider dateTimeProvider,
                                     ISelectListProvider selectListProvider) : base(db, flashMessage, mapper, selectListProvider) 
        { 
            _userManager = user;
            _reportProvider = reportProvider;
            _studentProvider = studentProvider;
            _cacheProvider = cacheProvider;
            _academicProvider = academicProvider;
            _admissionProvider = admissionProvider;
            _registrationProvider = registrationProvider;
            _receiptProvider = receiptProvider;
            _dateTimeProvider = dateTimeProvider;
            currentYear = DateTime.Now.Year;
        }

        public IActionResult Index(CertificationViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList();

            if (model.CertificationType == "ExpensesOutlineCertificate")
            {
                CreateReceiptNumberSelectList(model);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Template(CertificationViewModel model)
        {
            return PartialView("~/Views/Certificate/_FormTemplate.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AcademicYearWithGPA(CertificationViewModel model)
        {
            var student = _studentProvider.GetStudentInformationByCode(model.StudentCode);

            if (student != null && student.AcademicInformation != null)
            {
                model = _mapper.Map<Student, CertificationViewModel>(student, model);
                MapStudent(student, model, model.Language);

                CreateSelectList(model.Language, model.FacultyId);
                return PartialView("~/Views/Certificate/_AcademicYearWithGPATemplate.cshtml", model);
            }
            
            CreateSelectList(model.Language);
            return PartialView("~/Views/Certificate/_AcademicYearWithGPATemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.StudentNotFound });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GraduatedEnglishInstruction(CertificationViewModel model)
        {
            if (model.Language == "th")
            {
                CreateSelectList(model.Language);
                return PartialView("~/Views/Certificate/_GraduatedEnglishInstructionTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.CertificateUnavailableInSelectedLanguage });
            }

            var student = _studentProvider.GetStudentInformationByCode(model.StudentCode);

            if (student != null)
            {
                model = _mapper.Map<Student, CertificationViewModel>(student, model);
                MapStudent(student, model, model.Language);
                if (model.GraduatedAt == null)
                {
                    return PartialView("~/Views/Certificate/_GraduatedEnglishInstructionTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.CertificateForGraduatedStudent });
                }

                CreateSelectList(model.Language, model.FacultyId);
                return PartialView("~/Views/Certificate/_GraduatedEnglishInstructionTemplate.cshtml", model);
            }

            CreateSelectList(model.Language);
            return PartialView("~/Views/Certificate/_GraduatedEnglishInstructionTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.StudentNotFound });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GraduatingWaitingGrade(CertificationViewModel model) 
        {
            var student = _studentProvider.GetStudentInformationByCode(model.StudentCode);  

            if (student != null)
            {
                var currentTerm = _cacheProvider.GetCurrentTerm(student.AcademicInformation.AcademicLevelId);

                CreateSelectListCourse(student.Id, currentTerm.Id);
                model = _mapper.Map<Student, CertificationViewModel>(student, model);

                MapStudent(student, model, model.Language);

                CreateSelectList(model.Language, model.FacultyId);
                return PartialView("~/Views/Certificate/_GraduatingWaitingGradeTemplate.cshtml", model);
            }

            CreateSelectList(model.Language);
            return PartialView("~/Views/Certificate/_GraduatingWaitingGradeTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.StudentNotFound });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GraduatedWithAdmissionDateAndGraduatedDate(CertificationViewModel model)
        {
            if (model.Language == "th")
            {
                CreateSelectList(model.Language);
                return PartialView("~/Views/Certificate/_GraduatedWithAdmissionDateAndGraduatedDateTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.CertificateUnavailableInSelectedLanguage });
            }

            var student = _studentProvider.GetStudentInformationByCode(model.StudentCode);

            if (student != null && student.AcademicInformation != null)
            {
                model = _mapper.Map<Student, CertificationViewModel>(student, model);
                MapStudent(student, model, model.Language);
                if (model.GraduatedAt == null)
                {
                    return PartialView("~/Views/Certificate/_GraduatedWithAdmissionDateAndGraduatedDateTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.CertificateForGraduatedStudent });
                }

                CreateSelectList(model.Language, model.FacultyId);
                return PartialView("~/Views/Certificate/_GraduatedWithAdmissionDateAndGraduatedDateTemplate.cshtml", model);
            }

            CreateSelectList(model.Language);
            return PartialView("~/Views/Certificate/_GraduatedWithAdmissionDateAndGraduatedDateTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.StudentNotFound });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StudentWithAdmissionFromTo(CertificationViewModel model)
        {
            var student = _studentProvider.GetStudentInformationByCode(model.StudentCode);

            if (student != null && student.AcademicInformation != null)
            {
                model = _mapper.Map<Student, CertificationViewModel>(student, model);
                MapStudent(student, model, model.Language);
                if (model.GraduatedAt == null)
                {
                    return PartialView("~/Views/Certificate/_StudentWithAdmissionFromToTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.CertificateForGraduatedStudent });
                }
                
                CreateSelectList(model.Language, model.FacultyId);
                return PartialView("~/Views/Certificate/_StudentWithAdmissionFromToTemplate.cshtml", model);
            }

            CreateSelectList(model.Language);
            return PartialView("~/Views/Certificate/_StudentWithAdmissionFromToTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.StudentNotFound });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GraduatedNormalForm(CertificationViewModel model)
        {
            var student = _studentProvider.GetStudentInformationByCode(model.StudentCode);

            if (student != null)
            {
                model = _mapper.Map<Student, CertificationViewModel>(student, model);
                MapStudent(student, model, model.Language);
                if (model.GraduatedAt == null)
                {
                    return PartialView("~/Views/Certificate/_GraduatedNormalFormTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.CertificateForGraduatedStudent });
                }

                CreateSelectList(model.Language, model.FacultyId);
                return PartialView("~/Views/Certificate/_GraduatedNormalFormTemplate.cshtml", model);
            }

            CreateSelectList(model.Language);
            return PartialView("~/Views/Certificate/_GraduatedNormalFormTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.StudentNotFound });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StudentStatus(CertificationViewModel model)
        {
            var student = _studentProvider.GetStudentInformationByCode(model.StudentCode);

            if (student != null && student.AcademicInformation != null)
            {
                model = _mapper.Map<Student, CertificationViewModel>(student, model);
                MapStudent(student, model, model.Language);

                CreateSelectList(model.Language, model.FacultyId);
                return PartialView("~/Views/Certificate/_StudentStatusTemplate.cshtml", model);
            }

            CreateSelectList(model.Language);
            return PartialView("~/Views/Certificate/_StudentStatusTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.StudentNotFound });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmAcademicYear(CertificationViewModel model)
        {
            var student = _studentProvider.GetStudentInformationByCode(model.StudentCode);

            if (student != null && student.AcademicInformation != null)
            {
                model = _mapper.Map<Student, CertificationViewModel>(student, model);
                MapStudent(student, model, model.Language);

                CreateSelectList(model.Language, model.FacultyId);
                return PartialView("~/Views/Certificate/_ConfirmAcademicYearTemplate.cshtml", model);
            }

            CreateSelectList(model.Language);
            return PartialView("~/Views/Certificate/_ConfirmAcademicYearTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.StudentNotFound });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GraduatedWithCeremonyAt(CertificationViewModel model)
        {
            if (model.Language == "th")
            {
                CreateSelectList(model.Language);
                return PartialView("~/Views/Certificate/_GraduatedWithCeremonyAtTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.CertificateUnavailableInSelectedLanguage });
            }
            
            var student = _studentProvider.GetStudentInformationByCode(model.StudentCode);

            if (student != null)
            {
                model = _mapper.Map<Student, CertificationViewModel>(student, model);
                MapStudent(student, model, model.Language);
                if (model.GraduatedAt == null)
                {
                    return PartialView("~/Views/Certificate/_GraduatedWithCeremonyAtTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.CertificateForGraduatedStudent });
                }

                CreateSelectList(model.Language, model.FacultyId);
                return PartialView("~/Views/Certificate/_GraduatedWithCeremonyAtTemplate.cshtml", model);
            }

            CreateSelectList(model.Language);
            return PartialView("~/Views/Certificate/_GraduatedWithCeremonyAtTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.StudentNotFound });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StudentDraftDeferment(CertificationViewModel model)
        {
            if (model.Language == "en")
            {
                CreateSelectList(model.Language);
                return PartialView("~/Views/Certificate/_StudentDraftDefermentTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.CertificateUnavailableInSelectedLanguage });
            }

            var student = _studentProvider.GetStudentInformationByCode(model.StudentCode);

            if (student != null && student.AcademicInformation != null)
            {
                model = _mapper.Map<Student, CertificationViewModel>(student, model);
                MapStudent(student, model, model.Language);

                CreateSelectList(model.Language, model.FacultyId);
                return PartialView("~/Views/Certificate/_StudentDraftDefermentTemplate.cshtml", model);
            }

            CreateSelectList(model.Language);
            return PartialView("~/Views/Certificate/_StudentDraftDefermentTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.StudentNotFound });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GraduatingWaitingFinalResult(CertificationViewModel model)
        {
            var student = _studentProvider.GetStudentInformationByCode(model.StudentCode);

            if (student != null && student.AcademicInformation != null)
            {
                model = _mapper.Map<Student, CertificationViewModel>(student, model);
                MapStudent(student, model, model.Language);
                if (model.GraduatedYear == null)
                {
                    model.GraduatedYear = _reportProvider.GetYear(DateTime.Now.Year, model.Language);
                }
                
                var currentTerm = _cacheProvider.GetCurrentTerm(student.AcademicInformation.AcademicLevelId);

                model.RegistringCredit = _registrationProvider.GetRegisteringCredit(currentTerm.Id, student.Id);

                CreateSelectList(model.Language, model.FacultyId);
                return PartialView("~/Views/Certificate/_GraduatingWaitingFinalResultTemplate.cshtml", model);
            }

            return PartialView("~/Views/Certificate/_GraduatingWaitingFinalResultTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.StudentNotFound });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GraduatedWithEnglishAssessment(CertificationViewModel model)
        {
            if (model.Language == "th")
            {
                CreateSelectList(model.Language);
                return PartialView("~/Views/Certificate/_GraduatedWithEnglishAssessmentTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.CertificateUnavailableInSelectedLanguage });
            }
            
            var student = _studentProvider.GetStudentInformationByCode(model.StudentCode);

            if (student != null)
            {
                model = _mapper.Map<Student, CertificationViewModel>(student, model);
                MapStudent(student, model, model.Language);
                if (model.GraduatedAt == null)
                {
                    return PartialView("~/Views/Certificate/_GraduatedWithEnglishAssessmentTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.CertificateForGraduatedStudent });
                }

                CreateSelectList(model.Language, model.FacultyId);
                return PartialView("~/Views/Certificate/_GraduatedWithEnglishAssessmentTemplate.cshtml", model);
            }

            CreateSelectList(model.Language);
            return PartialView("~/Views/Certificate/_GraduatedWithEnglishAssessmentTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.StudentNotFound });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StatusCertificateWithAssessment(CertificationViewModel model)
        {
            if (model.Language == "th")
            {
                CreateSelectList(model.Language);
                return PartialView("~/Views/Certificate/_StatusCertificateWithAssessmentTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.CertificateUnavailableInSelectedLanguage });
            }
            
            var student = _studentProvider.GetStudentInformationByCode(model.StudentCode);

            if (student != null)
            {
                model = _mapper.Map<Student, CertificationViewModel>(student, model);
                MapStudent(student, model, model.Language);

                CreateSelectList(model.Language, model.FacultyId);
                return PartialView("~/Views/Certificate/_StatusCertificateWithAssessmentTemplate.cshtml", model);
            }

            CreateSelectList(model.Language);
            return PartialView("~/Views/Certificate/_StatusCertificateWithAssessmentTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.StudentNotFound });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GraduatingStatusCertification(CertificationViewModel model)
        {
            var student = _studentProvider.GetStudentInformationByCode(model.StudentCode);

            if (student != null)
            {
                model = _mapper.Map<Student, CertificationViewModel>(student, model);
                MapStudent(student, model, model.Language);

                CreateSelectList(model.Language, model.FacultyId);
                return PartialView("~/Views/Certificate/_GraduatingStatusCertificationTemplate.cshtml", model);
            }

            CreateSelectList(model.Language);
            return PartialView("~/Views/Certificate/_GraduatingStatusCertificationTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.StudentNotFound });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GraduationRankingCertification(CertificationViewModel model)
        {
            if (model.Language == "th")
            {
                CreateSelectList(model.Language);
                return PartialView("~/Views/Certificate/_GraduationRankingCertificationTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.CertificateUnavailableInSelectedLanguage });
            }

            var student = _studentProvider.GetStudentInformationByCode(model.StudentCode);

            if (student != null)
            {
                model = _mapper.Map<Student, CertificationViewModel>(student, model);
                MapStudent(student, model, model.Language);
                if (model.GraduatedAt == null)
                {
                    return PartialView("~/Views/Certificate/_GraduationRankingCertificationTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.CertificateForGraduatedStudent });
                }

                CreateSelectList(model.Language, model.FacultyId);
                return PartialView("~/Views/Certificate/_GraduationRankingCertificationTemplate.cshtml", model);
            }

            CreateSelectList(model.Language);
            return PartialView("~/Views/Certificate/_GraduationRankingCertificationTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.StudentNotFound });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThaiAirwayCertification(CertificationViewModel model)
        {
            if (model.Language == "th")
            {
                CreateSelectList(model.Language);
                return PartialView("~/Views/Certificate/_ThaiAirwayCertificationTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.CertificateUnavailableInSelectedLanguage });
            }
            
            var student = _studentProvider.GetStudentInformationByCode(model.StudentCode);

            if (student != null)
            {
                model = _mapper.Map<Student, CertificationViewModel>(student, model);
                MapStudent(student, model, model.Language);

                CreateSelectList(model.Language, model.FacultyId);
                return PartialView("~/Views/Certificate/_ThaiAirwayCertificationTemplate.cshtml", model);
            }

            CreateSelectList(model.Language);
            return PartialView("~/Views/Certificate/_ThaiAirwayCertificationTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.StudentNotFound });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TranslationGraduatedCertificate(CertificationViewModel model)
        {
            if (model.Language == "en")
            {
                CreateSelectList(model.Language);
                return PartialView("~/Views/Certificate/_TranslationGraduatedCertificateTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.CertificateUnavailableInSelectedLanguage });
            }

            var student = _studentProvider.GetStudentInformationByCode(model.StudentCode);

            if (student != null)
            {
                model = _mapper.Map<Student, CertificationViewModel>(student, model);
                MapStudent(student, model, model.Language);
                if (model.GraduatedAt == null)
                {
                    return PartialView("~/Views/Certificate/_TranslationGraduatedCertificateTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.CertificateForGraduatedStudent });
                }

                CreateSelectList(model.Language, model.FacultyId);
                return PartialView("~/Views/Certificate/_TranslationGraduatedCertificateTemplate.cshtml", model);
            }

            CreateSelectList(model.Language);
            return PartialView("~/Views/Certificate/_TranslationGraduatedCertificateTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.StudentNotFound });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangingStudentName(CertificationViewModel model)
        {
            var student = _studentProvider.GetStudentInformationByCode(model.StudentCode);

            if (student != null)
            {
                model = _mapper.Map<Student, CertificationViewModel>(student, model);
                MapStudent(student, model, model.Language);
                if (model.GraduatedAt == null)
                {
                    return PartialView("~/Views/Certificate/_ChangingStudentNameTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.CertificateForGraduatedStudent });
                }

                CreateSelectList(model.Language, model.FacultyId);
                return PartialView("~/Views/Certificate/_ChangingStudentNameTemplate.cshtml", model);
            }

            CreateSelectList(model.Language);
            return PartialView("~/Views/Certificate/_ChangingStudentNameTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.StudentNotFound });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExpensesOutlineCertificate(CertificationViewModel model)
        {
            //var id = Int64.Parse(model.ReceiptId);
            var student = _studentProvider.GetStudentInformationByCode(model.StudentCode);
            if (student != null || model.ReceiptId == 0)
            {
                if (model.AcademicYear == 0 || model.AcademicTerm == 0 || model.ReceiptId == 0)
                {
                    return PartialView("~/Views/Certificate/_ExpensesOutlineCertificateTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.RequiredData });
                }
                
                model = _mapper.Map<Student, CertificationViewModel>(student, model);
                MapStudent(student, model, model.Language);
                model.Receipt = _receiptProvider.GetReceiptCertificateById(model.ReceiptId, model.Language);
                model.ReceiptNumber = model.Receipt?.Number ?? "";
                model.PaidAt = model.Receipt?.CreatedAt ?? DateTime.Now;
                model.TermId = _academicProvider.GetTermByTermAndYear(student?.AcademicInformation?.AcademicLevelId ?? 0,
                                                                      model.AcademicTerm, model.AcademicYear)?.Id ?? 0;

                CreateSelectList(model.Language, model.FacultyId, model.AcademicLevelId);
                return PartialView("~/Views/Certificate/_ExpensesOutlineCertificateTemplate.cshtml", model);
            }
            
            CreateSelectList(model.Language);
            return PartialView("~/Views/Certificate/_ExpensesOutlineCertificateTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.StudentNotFound });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GraduationWithIELTS(CertificationViewModel model)
        {
            if (model.Language == "th")
            {
                CreateSelectList(model.Language);
                return PartialView("~/Views/Certificate/_GraduationWithIELTSTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.CertificateUnavailableInSelectedLanguage });
            }

            var student = _studentProvider.GetStudentInformationByCode(model.StudentCode);

            if (student != null)
            {
                model = _mapper.Map<Student, CertificationViewModel>(student, model);
                MapStudent(student, model, model.Language);
                if (model.GraduatedAt == null)
                {
                    return PartialView("~/Views/Certificate/_GraduationWithIELTSTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.CertificateForGraduatedStudent });
                }

                CreateSelectList(model.Language, model.FacultyId);
                return PartialView("~/Views/Certificate/_GraduationWithIELTSTemplate.cshtml", model);
            }

            CreateSelectList(model.Language);
            return PartialView("~/Views/Certificate/_GraduationWithIELTSTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.StudentNotFound });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CertifyNewStudent(CertificationViewModel model)
        {
            if (model.Language == "th")
            {
                CreateSelectList(model.Language);
                return PartialView("~/Views/Certificate/_CertifyNewStudentTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.CertificateUnavailableInSelectedLanguage });
            }

            var student = _studentProvider.GetStudentInformationByCode(model.StudentCode);

            if (student != null)
            {
                model = _mapper.Map<Student, CertificationViewModel>(student, model);
                MapStudent(student, model, model.Language);
                if (!model.IsAdmissionStudent)
                {
                    return PartialView("~/Views/Certificate/_CertifyNewStudentTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.CertificateForNewStudent });
                }

                CreateSelectList(model.Language, model.FacultyId);
                return PartialView("~/Views/Certificate/_CertifyNewStudentTemplate.cshtml", model);
            }

            CreateSelectList(model.Language);
            return PartialView("~/Views/Certificate/_CertifyNewStudentTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.StudentNotFound });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GoingAbroadCertification(CertificationViewModel model)
        {
            if (model.Language == "th")
            {
                CreateSelectList(model.Language);
                return PartialView("~/Views/Certificate/_GoingAbroadCertificationTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.CertificateUnavailableInSelectedLanguage });
            }

            var student = _studentProvider.GetStudentInformationByCode(model.StudentCode);

            if (student != null)
            {
                model = _mapper.Map<Student, CertificationViewModel>(student, model);
                MapStudent(student, model, model.Language);

                CreateSelectList(model.Language, model.FacultyId);
                return PartialView("~/Views/Certificate/_GoingAbroadCertificationTemplate.cshtml", model);
            }

            CreateSelectList(model.Language);
            return PartialView("~/Views/Certificate/_GoingAbroadCertificationTemplate.cshtml", new CertificationViewModel { ErrorMessage = Message.StudentNotFound });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Preview(CertificationViewModel model)
        {
            if (string.IsNullOrEmpty(model.CertificationType))
            {
                _flashMessage.Danger(Message.EmptyCertificateType);
                return RedirectToAction(nameof(Index));
            }

            var redirectValue = RedirectToAction(nameof(Index), new
                                                                {
                                                                    CertificationType = model.CertificationType,
                                                                    Purpose = model.Purpose,
                                                                    StudentCode = model.StudentCode,
                                                                    Language = model.Language,
                                                                    IsUrgent = model.IsUrgent
                                                                });

            string pdfView = "";
            TextInfo textInfo = new CultureInfo("en-US",false).TextInfo;
            model.StudentFirstName = textInfo.ToTitleCase((model.StudentFirstName ?? "").ToLower());
            model.StudentLastName = textInfo.ToTitleCase((model.StudentLastName ?? "").ToLower());
            model.ChangedName = textInfo.ToTitleCase((model.ChangedName ?? "").ToLower());
            model.ChangedSurname = textInfo.ToTitleCase((model.ChangedSurname ?? "").ToLower());
            model.DegreeName = textInfo.ToTitleCase((model.DegreeName ?? "").ToLower());
            model.Title = textInfo.ToTitleCase((_studentProvider.GetTitleById(model.TitleId, model.Language) ?? "").ToLower());
            model.FacultyName = textInfo.ToTitleCase((_academicProvider.GetFacultyShortNameById(model.FacultyId, model.Language) ?? "").ToLower());
            model.DepartmentName = textInfo.ToTitleCase((_academicProvider.GetDepartmentNameById(model.DepartmentId, model.Language) ?? "").ToLower());
            model.AcademicLevelName = textInfo.ToTitleCase((_academicProvider.GetAcademicLevelNameById(model.AcademicLevelId, model.Language) ?? "").ToLower());
            model.Position = textInfo.ToTitleCase((_reportProvider.GetSignatoryPositionById(model.ApprovedBy, model.Language) ?? "").ToLower());
            model.ApprovedByName = textInfo.ToTitleCase((_reportProvider.GetSignatoryNameById(model.ApprovedBy, model.Language) ?? "").ToLower());
            model.CreatedAt = _dateTimeProvider.ConvertStringToDateTime(model.CreatedAtString);

            if (model.TermId != 0)
            {
                var term = _academicProvider.GetTerm(model.TermId);
                model.TermText = $"{ term.AcademicTerm }/{ term.AcademicYear }";
            }

            if (model.ApprovedBy == 0)
            {
                _flashMessage.Danger(Message.RequiredData);
                return redirectValue;
            }
                
            if (model.CertificationType == Certificate.AcademicYearWithGPA)
            {
                pdfView = "PdfPreview/AcademicYearWithGPA";
            }
            else if (model.CertificationType == Certificate.StudentWithAdmissionFromTo)
            {
                pdfView = "PdfPreview/StudentWithAdmissionFromTo";
            }
            else if (model.CertificationType == Certificate.GraduatedEnglishInstruction)
            {
                pdfView = "PdfPreview/GraduatedEnglishInstruction";
            }
            else if (model.CertificationType == Certificate.GraduatedWithAdmissionDateAndGraduatedDate)
            {
                pdfView = "PdfPreview/GraduatedWithAdmissionDateAndGraduatedDate";
            }
            else if (model.CertificationType == Certificate.StudentStatus)
            {
                pdfView = "PdfPreview/StudentStatus";
            }
            else if (model.CertificationType == Certificate.TranslationGraduatedCertificate)
            {
                pdfView = "PdfPreview/TranslationGraduatedCertificate";
                
                if (model.SignIds != null && model.SignIds.Count() >= 2)
                {
                    model.Signs = _reportProvider.GetSignatoriesByIds(model.SignIds, model.Language);
                    model.Positions = _reportProvider.GetPositionsBySignatoryIds(model.SignIds, model.Language);
                }
                else
                {
                    _flashMessage.Danger(Message.RequiredData);
                    return redirectValue;
                }
            }
            else if (model.CertificationType == Certificate.StudentDraftDeferment)
            {
                pdfView = "PdfPreview/StudentDraftDeferment";
            }
            else if (model.CertificationType == Certificate.ConfirmAcademicYear)
            {
                pdfView = "PdfPreview/ConfirmAcademicYear";
            }
            else if (model.CertificationType == Certificate.GraduatingStatusCertification)
            {
                pdfView = "PdfPreview/GraduatingStatusCertification";
            }
            else if (model.CertificationType == Certificate.GraduatingWaitingGrade)
            {
                pdfView = "PdfPreview/GraduatingWaitingGrade";
                
                if (model.CourseIds != null && model.CourseIds.Any())
                {
                    var courses = _registrationProvider.GetCourseByIds(model.CourseIds);

                    courses.Select(x => {
                                            x.NameEn = textInfo.ToTitleCase((x.NameEn ?? "").ToLower()); 
                                            return x; 
                                        })
                           .ToList();
                    model.CourseCodeAndNames = model.Language == "th" ? courses.Select(x => x.CodeAndNameTh).ToList()
                                                                    : courses.Select(x => x.CodeAndName).ToList();
                }
                else 
                {
                    _flashMessage.Danger(Message.RequiredData);
                    return redirectValue;
                }
            }
            else if (model.CertificationType == Certificate.GraduatedNormalForm)
            {
                pdfView = "PdfPreview/GraduatedNormalForm";
            }
            else if (model.CertificationType == Certificate.GraduatedWithCeremonyAt)
            {
                pdfView = "PdfPreview/GraduatedWithCeremonyAt";
            }
            else if (model.CertificationType == Certificate.GraduatingWaitingFinalResult)
            {
                if (model.GraduatedYear == null)
                {
                    model.GraduatedYear = _reportProvider.GetYear(DateTime.Now.Year, model.Language);

                }
                
                pdfView = "PdfPreview/GraduatingWaitingFinalResult";
            }
            else if (model.CertificationType == Certificate.GraduatedWithEnglishAssessment)
            {
                pdfView = "PdfPreview/GraduatedWithEnglishAssessment";
            }
            else if (model.CertificationType == Certificate.StatusCertificateWithAssessment)
            {
                pdfView = "PdfPreview/StatusCertificateWithAssessment";
            }
            else if (model.CertificationType == Certificate.ThaiAirwayCertification)
            {
                pdfView = "PdfPreview/ThaiAirwayCertification";
            }
            else if (model.CertificationType == Certificate.ExpensesOutlineCertificate)
            {
                if (model.Receipt != null && model.Receipt.ReceiptItems != null)
                {
                    model.Receipt.TotalAmount = model.Receipt.ReceiptItems.Sum(x => x.TotalAmount);
                    model.Receipt.ReceiptItems = model.Receipt.ReceiptItems.Where(x => !string.IsNullOrWhiteSpace(x.FeeItemName))
                                                                           .ToList();
                }

                if (!model.Receipt.ReceiptItems.Any())
                {
                    _flashMessage.Danger(Message.CertificateEmptyFeeItem);
                    return RedirectToAction(nameof(Index), new {
                                                                    CertificationType = model.CertificationType,
                                                                    Purpose = model.Purpose,
                                                                    StudentCode = model.StudentCode,
                                                                    Language = model.Language,
                                                                    IsUrgent = model.IsUrgent,
                                                                    AcademicYear = model.AcademicYear,
                                                                    AcademicTerm = model.AcademicTerm,
                                                                    ReceiptId = model.ReceiptId
                                                               });
                }

                pdfView = "PdfPreview/ExpensesOutlineCertificate";
            }
            else if (model.CertificationType == Certificate.GraduationWithIELTS)
            {
                pdfView = "PdfPreview/GraduationWithIELTS";
            }
            else if (model.CertificationType == Certificate.GoingAbroadCertification)
            {
                pdfView = "PdfPreview/GoingAbroadCertification";

                if (model.AbroadCountryId != 0)
                {
                    model.AbroadCountry = _reportProvider.GetCountryById(model.AbroadCountryId, model.Language);
                }
                else
                {
                    _flashMessage.Danger(Message.RequiredData);
                    return redirectValue;
                }
            }
            else if (model.CertificationType == Certificate.CertifyNewStudent)
            {
                pdfView = "PdfPreview/CertifyNewStudent";
            }
            else if (model.CertificationType == Certificate.GraduationRankingCertification)
            {
                pdfView = "PdfPreview/GraduationRankingCertification";
            }
            else if (model.CertificationType == Certificate.ChangingStudentName)
            {
                pdfView = "PdfPreview/ChangingStudentName";

                if (model.ChangeNameType != null)
                {
                    model.ChangeNameTypeText = _reportProvider.GetChangeNameType(model.ChangeNameType, model.Language);
                }
                else
                {
                    _flashMessage.Danger(Message.RequiredData);
                    return redirectValue;
                }
            }
            else if (model.CertificationType == Certificate.CertifyNewStudent)
            {
                pdfView = "PdfPreview/CertifyNewStudent";
            }

            var user = await _userManager.GetUserAsync(User);
            var report = new ReportViewModel
                            {
                                Title = model.CertificationType,
                                Subject = model.CertificationType,
                                Creator = user == null ? "" : user.NormalizedUserName,
                                Author = user == null ? "" : user.NormalizedUserName,
                                Language = model.Language,
                                Body = model
                            };

            if (string.IsNullOrEmpty(pdfView))
            {
                _flashMessage.Danger(Message.EmptyCertificateType);
                return View();
            }

            return View(pdfView, report);
        }

        public ActionResult RecordPrintingLog(CertificationViewModel model)
        {
            using(var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var certificateType = _selectListProvider.GetCertificateTypes()
                                                        .SingleOrDefault(x => x.Value == model.CertificationType);
                    var printingLog = _mapper.Map<CertificationViewModel, PrintingLog>(model);
                    printingLog.Document = certificateType?.Text ?? "";

                    _db.PrintingLogs.Add(printingLog);
                    _db.SaveChanges();

                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToSave);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private void CreateSelectList(string language = "en", long facultyId = 0, long academicLevelId = 0)
        {
            ViewBag.CertificateTypes = _selectListProvider.GetCertificateTypes();
            ViewBag.Languages = _selectListProvider.GetLanguages();
            ViewBag.Titles = language == "th" ? _selectListProvider.GetTitlesTh() : _selectListProvider.GetTitlesEn();
            ViewBag.Purposes = _selectListProvider.GetCertificatePurposes();
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels(language);
            ViewBag.Faculties = _selectListProvider.GetFaculties(language);
            ViewBag.Signatories = _selectListProvider.GetSignatories(language);
            ViewBag.Countries = _selectListProvider.GetCountries();
            ViewBag.ChangeNameTypes = _selectListProvider.GetCertificateChangeNameTypes(language);
            
            if (facultyId != 0)
            {
                ViewBag.Departments = _selectListProvider.GetDepartments(facultyId, language);
            }
        }

        private void CreateReceiptNumberSelectList(CertificationViewModel model)
        {
            ViewBag.ReceiptNumbers = _selectListProvider.GetReceiptNumberByStudentCodeAndTerm(model.StudentCode, model.AcademicYear, model.AcademicTerm);
        }

        private void CreateSelectListCourse(Guid studentId, long termId)
        {
            ViewBag.Courses = _selectListProvider.GetCoursesByStudentAndTerm(studentId, termId);
        }

        private void MapStudent(Student student, CertificationViewModel model, string language)
        {
            var currentAcademicYear = _cacheProvider.GetCurrentTerm(model.AcademicLevelId).AcademicYear;
            model.ReferenceNumber = _reportProvider.GetNewReferenceNumber(currentYear, language);
            model.Year = _reportProvider.GetYear(currentAcademicYear, language);
            model.RunningNumber = Convert.ToInt32(model.ReferenceNumber.Split("/")[0]);
            model.DocumentYear = Convert.ToInt32(model.ReferenceNumber.Split("/")[1]);
            model.StudyYear = _studentProvider.GetStudyYear(model.AdmissionYear ?? 0, model.GraduatedYear ?? 0, model.Year);
            model.Pronoun = _studentProvider.GetPronoun(student.Gender, model.Language);
            model.Possessive = _studentProvider.GetPossessive(student.Gender, model.Language);
            model.IELTSScore = _admissionProvider.GetIELTSScore(student.Id);
            if (model.Language == "en")
            {
                model.StudentFirstName = student.FirstNameEn;
                model.StudentLastName = student.LastNameEn;
                model.DegreeName = student.AcademicInformation.DegreeName;
                model.DepartmentName = student.AcademicInformation.Department.NameEn;
                model.ChangedName = student.FirstNameEn;
                model.ChangedSurname = student.LastNameEn;
            }
            else
            {
                model.StudentFirstName = student.FirstNameTh;
                model.StudentLastName = student.LastNameTh;
                model.AcademicHonor = student.GraduationInformation?.AcademicHonor?.NameTh;
                model.DegreeName = student.AcademicInformation.CurriculumVersion?.DegreeNameTh ?? "-";
                model.GraduatedYear = model.GraduatedYear + 543;
                model.DepartmentName = student.AcademicInformation.Department.NameTh;
                model.ChangedName = student.FirstNameTh;
                model.ChangedSurname = student.LastNameTh;
            }
        }
    }
}