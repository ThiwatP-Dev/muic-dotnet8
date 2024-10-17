using AutoMapper;
using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers.MasterTables
{
    [PermissionAuthorize("StudentCodeStatus", "")]
    public class StudentCodeStatusController : BaseController
    {
        protected readonly IAdmissionProvider _admissionProvider;

        public StudentCodeStatusController(ApplicationDbContext db,
                                           IFlashMessage flashMessage,
                                           ISelectListProvider selectListProvider,
                                           IMapper mapper,
                                           IAdmissionProvider admissionProvider) : base(db, flashMessage, mapper, selectListProvider)
        {
            _admissionProvider = admissionProvider;
        }

        public ActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList(criteria.AcademicLevelId);
            if (criteria.AcademicLevelId == 0 || criteria.StudentCodeFrom == null || criteria.StudentCodeTo == null)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }
            
            var model = _db.Students.Where(x => x.CodeInt >= criteria.StudentCodeFrom
                                                && x.CodeInt <= criteria.StudentCodeTo
                                                && x.StudentStatus != "d")
                                    .ToList();
            
            var students = new List<StudentCodeStatusViewModel>();
            for (int i = criteria.StudentCodeFrom ?? 0; i <= criteria.StudentCodeTo; i++)
            {
                if (model.Any(x => x.Code.Trim() == i.ToString()))
                {
                    students.Add(new StudentCodeStatusViewModel
                                 {
                                     Code = i, 
                                     IsUsed = true
                                 });
                }
                else 
                {
                    students.Add(new StudentCodeStatusViewModel
                                 {
                                     Code = i, 
                                     IsUsed = false
                                 });
                }
            }

            var modelPageResult = students.Where(x => criteria.Status == "all"
                                                      || x.IsUsed == Convert.ToBoolean(criteria.Status))
                                          .OrderBy(x => x.Code)
                                          .AsQueryable()
                                          .GetPaged(criteria, page);            
            return View(modelPageResult);
        }

        private void CreateSelectList(long academicLevelId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.StudentCodeStatuses = _selectListProvider.GetStudentCodeStatuses();
            if (academicLevelId > 0)
            {
                ViewBag.AdmissionRounds = _selectListProvider.GetAdmissionRoundByAcademicLevelId(academicLevelId);
            }
        }

        public StudentCodeStatusRange GetStudentCodeRange(long admissionRoundId)
        {
            var code = _admissionProvider.GetStudentCodeRange(admissionRoundId);
            return code;
        }
    }
}