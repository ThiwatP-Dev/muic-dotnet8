using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class WaitingDocumentReportController : BaseController
    {
        public WaitingDocumentReportController(ApplicationDbContext db,
                                               ISelectListProvider selectListProvider,
                                               IFlashMessage flashMessage) : base(db, flashMessage, selectListProvider) { }

        public ActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);

            if (criteria.AcademicLevelId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }
            
            var studentDocuments = (from studentDocument in _db.StudentDocuments
                                    join document in _db.Documents on studentDocument.DocumentId equals document.Id
                                    join student in _db.Students on studentDocument.StudentId equals student.Id
                                    join admissionInformation in _db.AdmissionInformations on student.Id equals admissionInformation.StudentId into admissionInformations
                                    from admissionInformation in admissionInformations.DefaultIfEmpty()
                                    join previousSchool in _db.PreviousSchools on admissionInformation.PreviousSchoolId equals previousSchool.Id into previousSchools
                                    from previousSchool in previousSchools.DefaultIfEmpty()
                                    join academicInformation in _db.AcademicInformations on student.Id equals academicInformation.StudentId into academicInformations
                                    from academicInformation in academicInformations.DefaultIfEmpty()
                                    join faculty in _db.Faculties on academicInformation.FacultyId equals faculty.Id
                                    join department in _db.Departments on academicInformation.DepartmentId equals department.Id into departments
                                    from department in departments.DefaultIfEmpty()
                                    where studentDocument.IsRequired
                                          && studentDocument.DocumentStatus == "w"
                                          && (academicInformation.AcademicLevelId == criteria.AcademicLevelId)
                                          && (criteria.AdmissionRoundId == 0
                                              || admissionInformation.AdmissionRoundId == criteria.AdmissionRoundId)
                                          && (criteria.FacultyId == 0
                                              || faculty.Id == criteria.FacultyId)
                                          && (criteria.DepartmentId == 0
                                              || department.Id == criteria.DepartmentId)
                                          && (criteria.StartStudentBatch == null
                                              || academicInformation.Batch == criteria.StartStudentBatch)
                                          && (string.IsNullOrEmpty(criteria.Code)
                                              || student.Code.StartsWith(criteria.Code))
                                          && (string.IsNullOrEmpty(criteria.Status)
                                              || student.StudentStatus == criteria.Status)
                                    group new { studentDocument, student, faculty, department, previousSchool, document }
                                    by studentDocument.StudentId
                                    into studentGroup
                                    select new WaitingDocumentReportViewModel
                                           {
                                               Code = studentGroup.FirstOrDefault().student.Code,
                                               FullName = studentGroup.FirstOrDefault().student.FullNameEn,
                                               Gender = studentGroup.FirstOrDefault().student.GenderText,
                                               Email = studentGroup.FirstOrDefault().student.Email,
                                               PersonalEmail = studentGroup.FirstOrDefault().student.PersonalEmail,
                                               Phone = studentGroup.FirstOrDefault().student.TelephoneNumber1,
                                               StudentStatus = studentGroup.FirstOrDefault().student.StudentStatusText,
                                               Faculty = studentGroup.FirstOrDefault().faculty == null
                                                         ? "" : studentGroup.FirstOrDefault().faculty.NameEn,
                                               Department = studentGroup.FirstOrDefault().department == null
                                                            ? "" : studentGroup.FirstOrDefault().department.NameEn,
                                               PreviousSchool = studentGroup.FirstOrDefault().previousSchool == null
                                                                ? "" : studentGroup.FirstOrDefault().previousSchool.NameEn,
                                               WaitingDocuments = studentGroup.Select(x => new WaitingDocument
                                                                                           {
                                                                                               DocumentName = x.document.NameEn,
                                                                                               Remark = x.document.Remark
                                                                                           })
                                                                              .OrderBy(x => x.DocumentName)
                                                                              .ToList()
                                           }).GetPaged(criteria, page);

            return View(studentDocuments);
        }

        public void CreateSelectList(long academicLevelId, long facultyId)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.StudentStatuses = _selectListProvider.GetStudentStatuses();
            ViewBag.AdmissionRounds = _selectListProvider.GetAdmissionRoundByAcademicLevelId(academicLevelId);
            ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
        }
    }
}