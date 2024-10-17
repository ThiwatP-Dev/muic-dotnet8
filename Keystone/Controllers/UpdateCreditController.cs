using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("UpdateCredit", "")]
    public class UpdateCreditController : BaseController
    {
        private readonly IConfiguration _config;
        private readonly IRegistrationProvider _registrationProvider;
        public UpdateCreditController(ApplicationDbContext db,
                                      IFlashMessage flashMessage,
                                      ISelectListProvider selectListProvider,
                                      IRegistrationProvider registrationProvider,
                                      IConfiguration config) : base(db, flashMessage, selectListProvider) 
        { 
            _config = config;
            _registrationProvider = registrationProvider;
        }

        public IActionResult Index(Criteria criteria, string returnUrl)
        {   
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId, criteria.DepartmentId, criteria.CurriculumId);
            ViewBag.ReturnUrl = returnUrl;
            if (criteria.AcademicLevelId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                criteria.StudentStatus = "s";
                return View(new UpdateCreditViewModel
                            { 
                                Criteria = criteria
                            });
            }

            var studentUpdateCredits = _db.Students.Where(x => (x.AcademicInformation.AcademicLevelId == criteria.AcademicLevelId
                                                                || criteria.AcademicLevelId == 0)
                                                                && (x.AcademicInformation.FacultyId == criteria.FacultyId
                                                                    || criteria.FacultyId == 0)
                                                                && (x.AcademicInformation.DepartmentId == criteria.DepartmentId
                                                                    || criteria.DepartmentId == 0)
                                                                && (x.AcademicInformation.CurriculumVersion.CurriculumId == criteria.CurriculumId
                                                                    || criteria.CurriculumId == 0)
                                                                && (x.AcademicInformation.CurriculumVersionId == criteria.CurriculumVersionId
                                                                    || criteria.CurriculumVersionId == 0)
                                                                && ((criteria.MinimumCreditFrom == null
                                                                    || x.AcademicInformation.MinimumCredit >= criteria.MinimumCreditFrom)
                                                                    &&(criteria.MinimumCreditTo == null
                                                                    || x.AcademicInformation.MinimumCredit <= criteria.MinimumCreditTo))
                                                                && ((criteria.MaximumCreditFrom == null
                                                                    || x.AcademicInformation.MaximumCredit >= criteria.MaximumCreditFrom)
                                                                    &&(criteria.MaximumCreditTo == null
                                                                    || x.AcademicInformation.MaximumCredit <= criteria.MaximumCreditTo))
                                                                && (criteria.StartStudentBatch == null
                                                                    || x.AcademicInformation.Batch >= criteria.StartStudentBatch)
                                                                && (criteria.EndStudentBatch == null
                                                                    || x.AcademicInformation.Batch <= criteria.EndStudentBatch)
                                                                && (string.IsNullOrEmpty(criteria.StudentStatus) 
                                                                    || x.StudentStatus == criteria.StudentStatus) 
                                                                && (x.Code == criteria.Code
                                                                    || string.IsNullOrEmpty(criteria.Code)))
                                                   .Select(x => new StudentUpdateCredit
                                                                {
                                                                    AcademicInfomationId = x.AcademicInformation.Id,
                                                                    StudentCode = x.Code,
                                                                    TitleNameEn = x.Title.NameEn,
                                                                    FirstNameEn = x.FirstNameEn,
                                                                    MidNameEn = x.MidNameEn,
                                                                    LastNameEn = x.LastNameEn,
                                                                    Major = x.AcademicInformation.Department.ShortNameEn,
                                                                    CurriculumVersion = x.AcademicInformation.CurriculumVersion.NameEn,
                                                                    MinimumCredit = x.AcademicInformation.MinimumCredit,
                                                                    MaximumCredit = x.AcademicInformation.MaximumCredit
                                                                })
                                                   .ToList();
            if(studentUpdateCredits.Any())
            {
                if(criteria.OrderBy == "c")
                {
                    studentUpdateCredits = studentUpdateCredits.OrderBy(x => x.StudentCode).ToList();
                }
                else
                {
                    studentUpdateCredits = studentUpdateCredits.OrderBy(x => x.FirstNameEn)
                                                                  .ThenBy(x => x.LastNameEn)
                                                               .ToList();
                }
            }
            var result = new UpdateCreditViewModel
                         {
                             Criteria = criteria,
                             StudentUpdateCredits = studentUpdateCredits
                         };

            return View(result);
        }

        [PermissionAuthorize("UpdateCredit", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(ValueCountLimit = Int32.MaxValue)]
        public async Task<IActionResult> Update(UpdateCreditViewModel model, string returnUrl)
        {
            
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    foreach(var item in model.StudentUpdateCredits)
                    {
                        if(item.IsChecked == "on")
                        {  
                            var academicInfo = _db.AcademicInformations.Include(x => x.Student)
                                                                       .SingleOrDefault(x => x.Id == item.AcademicInfomationId);

                            if(model.MaximumCredit == 0 && model.MinimumCredit == 0 )
                            {
                                academicInfo.MaximumCredit  = null;
                                academicInfo.MinimumCredit  = null;
                            } 
                            else 
                            {
                                academicInfo.MaximumCredit = model.MaximumCredit;
                                academicInfo.MinimumCredit = model.MinimumCredit;
                            }

                            _db.SaveChanges();

                            if(await _registrationProvider.UpdateCreditUspark(academicInfo.Student.Code, model.MaximumCredit, model.MinimumCredit))
                            {
                                transaction.Rollback();
                                _flashMessage.Danger(Message.UnableToCreate);

                                CreateSelectList();
                                ViewBag.ReturnUrl = returnUrl;
                                return RedirectToAction(nameof(Index), model.Criteria);
                            }
                        }
                    }

                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);

                    CreateSelectList();
                    return RedirectToAction(nameof(Index), model.Criteria);
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToCreate);

                    CreateSelectList();
                    ViewBag.ReturnUrl = returnUrl;
                    return RedirectToAction(nameof(Index), model.Criteria);
                }
            }

        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0, long department = 0, long curriculumId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Statuses = _selectListProvider.GetStudentStatuses();

            if (academicLevelId != 0)
            {
                ViewBag.AdmissionRounds = _selectListProvider.GetAdmissionRoundByAcademicLevelId(academicLevelId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);

                if (facultyId != 0)
                {
                    ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);

                    if (department != 0)
                    {
                        ViewBag.Curriculums = _selectListProvider.GetCurriculumByDepartment(academicLevelId, facultyId, department);

                        if (curriculumId != 0)
                        {
                            ViewBag.CurriculumVersions = _selectListProvider.GetCurriculumVersion(curriculumId);
                        }
                    }
                }
            }
        }
    }
}