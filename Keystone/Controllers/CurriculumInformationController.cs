using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Profile;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class CurriculumInformationController : BaseController
    {
        protected readonly ICurriculumProvider _curriculumProvider;
        protected readonly IStudentProvider _studentProvider;
        
        public CurriculumInformationController(ApplicationDbContext db,
                                               IFlashMessage flashMessage,
                                               ICurriculumProvider curriculumProvider,
                                               IStudentProvider studentProvider,
                                               ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) 
        { 
            _curriculumProvider = curriculumProvider;
            _studentProvider = studentProvider;
        }

        public ActionResult Create(long academiclevelId, Guid studentId, long facultyId, long departmentId)
        {
            //TODO: Permission to Create , probably check from tab permission of student
            CreateSelectList(studentId, 0, academiclevelId, 0);
            var student = _studentProvider.GetStudentInformationById(studentId);
            var model = new CurriculumInformation
                        {
                            StudentId = studentId,
                            FacultyId = facultyId,
                            DepartmentId = departmentId,
                            AcademiclevelId = academiclevelId,
                            StudentCode = student.Code,
                            StudentName = student.FullNameEn,
                            Term = student.AdmissionInformation.AdmissionTerm.TermText,
                            Division = student.AcademicInformation.Faculty.NameEn,
                            Major = student.AcademicInformation.Department.NameEn
                        };
            return View("~/Views/Student/CurriculumInformation/Create.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CurriculumInformation model)
        {
            //TODO: Permission to Create , probably check from tab permission of student
            var isActive = _curriculumProvider.IsActiveCurriculumInformation(model.StudentId, 0);
            var student = _studentProvider.GetStudentInformationById(model.StudentId);
            model.StudentCode = student.Code;
            model.StudentName = student.FullNameEn;
            model.Term = student.AdmissionInformation.AdmissionTerm.TermText;
            model.Division = student.AcademicInformation.Faculty.NameEn;
            model.Major = student.AcademicInformation.Department.NameEn;
            model.AcademiclevelId = student.AcademicInformation.AcademicLevelId;

            if (model.CurriculumVersionId == null)
            {
                _flashMessage.Danger(Message.UnableToCreate);
                CreateSelectList(model.StudentId, model.CurriculumId ?? 0, model.AcademiclevelId, model.CurriculumVersionId ?? 0);
                return View("~/Views/Student/CurriculumInformation/Create.cshtml", model);
            }

            if (model.IsActive == true && isActive == true)
            {
               _flashMessage.Danger(Message.CurriculumInformationActive);
               CreateSelectList(model.StudentId, model.CurriculumId ?? 0, model.AcademiclevelId, model.CurriculumVersionId ?? 0);
               return View("~/Views/Student/CurriculumInformation/Create.cshtml", model);
            }

            var specializationGroups = new List<SpecializationGroupInformation>();
            if (model.SpecializationGroupInformations != null && model.SpecializationGroupInformations.Any())
            {
                specializationGroups = model.SpecializationGroupInformations.GroupBy(x => x.SpecializationGroupId)
                                                                            .Select(x => x.First())
                                                                            .ToList();
            }

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var curriculum = _curriculumProvider.GetCurriculum(model.CurriculumId ?? 0);
                    var curriculumInformation = new CurriculumInformation
                                                {
                                                    StudentId = model.StudentId,
                                                    FacultyId = curriculum.FacultyId,
                                                    DepartmentId = curriculum.DepartmentId,
                                                    CurriculumVersionId = model.CurriculumVersionId,
                                                    StudyPlanId = model.StudyPlanId,
                                                    SpecializationGroupInformations = specializationGroups,
                                                    IsActive = model.IsActive,
                                                    CreatedAt = DateTime.UtcNow,
                                                    UpdatedAt = DateTime.UtcNow
                                                };

                    if (!_curriculumProvider.IsExistCurriculumInformation(curriculumInformation))
                    {
                        _db.CurriculumInformations.Add(curriculumInformation);
                        _db.SaveChanges();

                        var activeCurriculumInformation = _db.CurriculumInformations.FirstOrDefault(x => x.StudentId == model.StudentId
                                                                                                         && x.IsActive);

                        if (activeCurriculumInformation != null && (student.AcademicInformation.CurriculumVersionId != activeCurriculumInformation.CurriculumVersionId))
                        {
                            student.AcademicInformation.CurriculumVersionId = activeCurriculumInformation.CurriculumVersionId;
                            student.AcademicInformation.FacultyId = curriculum.FacultyId;
                            student.AcademicInformation.DepartmentId = curriculum.DepartmentId;
                            _db.SaveChanges();
                        }

                        transaction.Commit();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return RedirectToAction("Details", "Student", new { id = model.StudentId, tabIndex = "2" });
                    }
                    else
                    {
                        transaction.Rollback();
                        _flashMessage.Danger(Message.UnableToCreate);
                        
                        CreateSelectList(model.StudentId, model.CurriculumId ?? 0, model.AcademiclevelId, model.CurriculumVersionId ?? 0);
                        return View("~/Views/Student/CurriculumInformation/Create.cshtml", model);
                    }
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToCreate);

                    CreateSelectList(model.StudentId, model.CurriculumId ?? 0, model.AcademiclevelId, model.CurriculumVersionId ?? 0);
                    return View("~/Views/Student/CurriculumInformation/Create.cshtml", model);
                }
            }
        }

        public IActionResult Edit(long id)
        {
            //TODO: Permission to Create , probably check from tab permission of student
            var model = _curriculumProvider.GetCurriculumInformation(id);
            var student = _studentProvider.GetStudentInformationById(model.StudentId);
            CreateSelectList(model.StudentId, model.CurriculumVersion.CurriculumId, model.Student.AcademicInformation.AcademicLevelId, model.CurriculumVersionId ?? 0);
            model.StudentCode = student.Code;
            model.StudentName = student.FullNameEn;
            model.Term = student.AdmissionInformation.AdmissionTerm.TermText;
            model.Division = student.AcademicInformation.Faculty.NameEn;
            model.Major = student.AcademicInformation.Department.NameEn;
            model.CurriculumId = model.CurriculumVersion.CurriculumId;
            return View("~/Views/Student/CurriculumInformation/Edit.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CurriculumInformation model)
        {
            //TODO: Permission to Create , probably check from tab permission of student
            var isActive = _curriculumProvider.IsActiveCurriculumInformation(model.StudentId, model.Id);
            var student = _studentProvider.GetStudentInformationById(model.StudentId);
           
            model.StudentCode = student.Code;
            model.StudentName = student.FullNameEn;
            model.Term = student.AdmissionInformation.AdmissionTerm.TermText;
            model.Division = student.AcademicInformation.Faculty.NameEn;
            model.Major = student.AcademicInformation.Department.NameEn;
            model.AcademiclevelId = student.AcademicInformation.AcademicLevelId;
            
            if (model.CurriculumVersionId == null)
            {
                _flashMessage.Danger(Message.UnableToEdit);
                CreateSelectList(model.StudentId, model.CurriculumId ?? 0, model.AcademiclevelId, model.CurriculumVersionId ?? 0);
                return View("~/Views/Student/CurriculumInformation/Edit.cshtml", model);
            }

            if (model.IsActive && isActive)
            {
               _flashMessage.Danger(Message.CurriculumInformationActive);
               CreateSelectList(model.StudentId, model.CurriculumId ?? 0, model.AcademiclevelId, model.CurriculumVersionId ?? 0);
               return View("~/Views/Student/CurriculumInformation/Edit.cshtml", model);
            }

            var specializationGroups = new List<SpecializationGroupInformation>();
            if (model.SpecializationGroupInformations != null && model.SpecializationGroupInformations.Any())
            {
                specializationGroups = model.SpecializationGroupInformations.GroupBy(x => x.SpecializationGroupId)
                                                                            .Select(x => x.First())
                                                                            .ToList();
            };

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    // CURRICULUM INFORMATION
                    var curriculumInformation = _curriculumProvider.GetCurriculumInformation(model.Id);
                    _db.SpecializationGroupInformations.RemoveRange(curriculumInformation.SpecializationGroupInformations);
                    var curriculum = _curriculumProvider.GetCurriculum(model.CurriculumId ?? 0);
                    curriculumInformation.CurriculumVersionId = model.CurriculumVersionId;
                    curriculumInformation.SpecializationGroupInformations = specializationGroups;
                    curriculumInformation.StudyPlanId = model.StudyPlanId;
                    curriculumInformation.IsActive = model.IsActive;
                    curriculumInformation.FacultyId = curriculum.FacultyId;
                    curriculumInformation.DepartmentId = curriculum.DepartmentId;
                    _db.SaveChanges();
                    

                    // ACADEMIC INFORMATION
                    var academicInfo = _db.AcademicInformations.FirstOrDefault(x => x.StudentId == model.StudentId);                                                                   
                    if (curriculumInformation != null && academicInfo != null)
                    {
                        academicInfo.FacultyId = curriculumInformation.FacultyId;
                        academicInfo.DepartmentId = curriculumInformation.DepartmentId;
                        academicInfo.CurriculumVersionId = curriculumInformation.CurriculumVersionId;
                        _db.SaveChanges();
                    }

                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);

                    return RedirectToAction("Details", "Student", new { id = model.StudentId, tabIndex = "2" });
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToEdit);

                    CreateSelectList(model.StudentId, model.CurriculumId ?? 0, model.AcademiclevelId, model.CurriculumVersionId ?? 0);
                    return View("~/Views/Student/CurriculumInformation/Edit.cshtml", model);
                }
            }
        }

        public IActionResult Delete(long id)
        {
            //TODO: Permission to Create , probably check from tab permission of student
            var model = _curriculumProvider.GetCurriculumInformation(id);
            try
            {
                _db.CurriculumInformations.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction("Details", "Student", new { id = model.StudentId, tabIndex = "2" });
        }

        private void CreateSelectList(Guid studentId, long curriculumId = 0, long academicLevelId = 0, long? curriculumVersionId = 0) 
        {
            ViewBag.Curriculums = _selectListProvider.GetCurriculumByAcademicLevelId(academicLevelId);
            if (curriculumId != 0)
            {
                ViewBag.CurriculumVersions = _selectListProvider.GetCurriculumVersionsByCurriculumId(curriculumId);
            }

            if (curriculumVersionId != 0)
            {
                ViewBag.StudyPlans = _selectListProvider.GetStudyPlanByCurriculumVersion(curriculumVersionId ?? 0);
                ViewBag.SpecializationGroups = _selectListProvider.GetSpecializationGroupByCurriculumVersionId(curriculumVersionId ?? 0);
            }
        }
    }
}