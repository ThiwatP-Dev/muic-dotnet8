using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class VerificationLetterReportController : BaseController
    {
        public VerificationLetterReportController(ApplicationDbContext db,
                                                  ISelectListProvider selectListProvider,
                                                  IFlashMessage flashMessage) : base(db, flashMessage, selectListProvider) { }

        public IActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.TermId);
            if (criteria.AcademicLevelId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }
            
            var models = _db.Students.Include(x => x.AdmissionInformation)
                                         .ThenInclude(x => x.AdmissionTerm)
                                     .Include(x => x.AdmissionInformation)
                                         .ThenInclude(x => x.AdmissionRound)
                                     .Include(x => x.AdmissionInformation)
                                         .ThenInclude(x => x.PreviousSchool)
                                             .ThenInclude(x => x.SchoolGroup)
                                     .Include(x => x.AcademicInformation)
                                         .ThenInclude(x => x.AcademicLevel)
                                     .Where(x => (criteria.StudentCodeFrom == null
                                                  || (criteria.StudentCodeTo == null ? x.CodeInt == criteria.StudentCodeFrom : x.CodeInt >= criteria.StudentCodeFrom))
                                                  && (criteria.StudentCodeTo == null
                                                      || (criteria.StudentCodeFrom == null ? x.CodeInt == criteria.StudentCodeTo : x.CodeInt <= criteria.StudentCodeTo))
                                                  && (criteria.AcademicLevelId == 0
                                                      || x.AcademicInformation.AcademicLevelId == criteria.AcademicLevelId)
                                                  && (criteria.TermId == 0
                                                      || x.AdmissionInformation.AdmissionTermId == criteria.TermId)
                                                  && (criteria.AdmissionRoundId == 0
                                                      || x.AdmissionInformation.AdmissionRoundId == criteria.AdmissionRoundId)
                                                  && (criteria.SchoolGroupId == 0
                                                      || x.AdmissionInformation.PreviousSchool.SchoolGroupId == criteria.SchoolGroupId)
                                                  && (criteria.PreviousSchoolId == 0
                                                      || x.AdmissionInformation.PreviousSchoolId == criteria.PreviousSchoolId)
                                                  && (criteria.StartStudentBatch == null
                                                      || criteria.StartStudentBatch == 0
                                                      || criteria.StartStudentBatch <= x.AcademicInformation.Batch)
                                                  && (criteria.EndStudentBatch == null
                                                      || criteria.EndStudentBatch == 0
                                                      || criteria.EndStudentBatch >= x.AcademicInformation.Batch)
                                                  && (criteria.IsSubmitted == "all"
                                                      || (Convert.ToBoolean(criteria.IsSubmitted) ? x.AdmissionInformation.CheckDated != null
                                                                                                  : x.AdmissionInformation.CheckDated == null))
                                                  && (criteria.IsReceived == "all"
                                                      || (Convert.ToBoolean(criteria.IsReceived) ? x.AdmissionInformation.ReplyDate != null
                                                                                                 : x.AdmissionInformation.ReplyDate == null)))
                                     .GetPaged(criteria, page);

            return View(models);
        }

        public void CreateSelectList(long academicLevelId = 0, long admissionTermId = 0, long schoolGroupId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.SubmittedStatus = _selectListProvider.GetSubmittedStatus();
            ViewBag.ReceivedStatus = _selectListProvider.GetReceivedStatus();
            ViewBag.SchoolGroups = _selectListProvider.GetSchoolGroup();
            ViewBag.PreviousSchools = _selectListProvider.GetPreviousSchools(schoolGroupId);
            
            if (academicLevelId > 0)
            {
                ViewBag.AdmissionTerms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }

            if (admissionTermId > 0)
            {
                ViewBag.AdmissionRounds = _selectListProvider.GetAdmissionRoundByTermId(admissionTermId);
            }
        }
    }
}