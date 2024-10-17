using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers.Report
{
    public class EntranceExaminationReportController : BaseController
    {
        public EntranceExaminationReportController(ApplicationDbContext db,
                                                   IFlashMessage flashMessage,
                                                   IMapper mapper,
                                                   ISelectListProvider selectListProvider) : base(db, flashMessage, mapper, selectListProvider) { }

        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId);
            if (criteria.AcademicLevelId == 0 || criteria.FacultyId == 0 || criteria.AdmissionRoundId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }
            
            var admissionExamination = _db.AdmissionExaminations.Include(x => x.AdmissionExaminationSchedules)
                                                                    .ThenInclude(x => x.AdmissionExaminationType)
                                                                .Include(x => x.AdmissionExaminationSchedules)
                                                                    .ThenInclude(x => x.Room)
                                                                .Include(x => x.FacultyId)
                                                                .Include(x => x.Department)
                                                                .Include(x => x.AdmissionRound)
                                                                    .ThenInclude(x => x.AdmissionTerm)
                                                                .Where(x => x.AcademicLevelId == criteria.AcademicLevelId
                                                                             && x.FacultyId == criteria.FacultyId
                                                                             && x.AdmissionRoundId == criteria.AdmissionRoundId
                                                                             && (criteria.DepartmentId == 0
                                                                                 || x.DepartmentId == criteria.DepartmentId))
                                                                .Select(x => new EntranceExaminationReportViewModel
                                                                             {
                                                                                 Criteria = criteria,
                                                                                 AdmissionRoundId = x.AdmissionRoundId,
                                                                                 Term = x.AdmissionRound.AdmissionTerm.TermText,
                                                                                 Faculty = x.Faculty.NameEn,
                                                                                 Department = x.Department.NameEn ?? "N/A",
                                                                                 EntranceExaminationSchedules = x.AdmissionExaminationSchedules.Select(y => new EntranceExaminationSchedule
                                                                                                                                                            {
                                                                                                                                                                Id = y.Id,
                                                                                                                                                                Room = y.Room.NameEn,
                                                                                                                                                                TestDate = y.TestedAtText,
                                                                                                                                                                TestTime = y.StartEndTimeText,
                                                                                                                                                                AdmissionExaminationType = y.AdmissionExaminationType.NameEn
                                                                                                                                                            })
                                                                                                                                               .ToList()
                                                                             })
                                                                .FirstOrDefault();
            
            if (admissionExamination != null && admissionExamination.EntranceExaminationSchedules != null)
            {
                admissionExamination.Rooms = string.Join(" / ", admissionExamination.EntranceExaminationSchedules.Select(x => x.Room));
                admissionExamination.TestDates = string.Join(" / ", admissionExamination.EntranceExaminationSchedules.Select(x => x.TestDate));
                admissionExamination.TestTimes = string.Join(" / ", admissionExamination.EntranceExaminationSchedules.Select(x => x.TestTime));

                admissionExamination.Students = _db.Students.Include(x => x.AcademicInformation)
                                                            .Where(x => x.StudentStatus == "a"
                                                                        && x.AcademicInformation.AcademicLevelId == criteria.AcademicLevelId
                                                                        && x.AcademicInformation.FacultyId == criteria.FacultyId
                                                                        && (criteria.DepartmentId == 0
                                                                            || x.AcademicInformation.DepartmentId == criteria.DepartmentId)
                                                                        && x.AdmissionInformation.AdmissionRoundId == criteria.AdmissionRoundId)
                                                            .ToList();
            }
            else
            {
                _flashMessage.Danger(Message.DataNotFound);
                return View();
            }
            
            return View(admissionExamination);
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
            ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
            ViewBag.AdmissionRounds = _selectListProvider.GetAdmissionRoundByAcademicLevelId(academicLevelId);
        }
    }
}