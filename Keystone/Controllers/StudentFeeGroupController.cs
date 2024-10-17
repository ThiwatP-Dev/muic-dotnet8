using Keystone.Permission;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Fee;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    [PermissionAuthorize("StudentFeeGroup", "")]
    public class StudentFeeGroupController : BaseController
    {
        protected readonly IExceptionManager _exceptionManager;
        protected readonly IFeeProvider _feeProvider;

        public StudentFeeGroupController(ApplicationDbContext db,
                                         IFlashMessage flashMessage,
                                         IFeeProvider feeProvider,
                                         IExceptionManager exceptionManager,
                                         ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider)
        {
            _exceptionManager = exceptionManager;
            _feeProvider = feeProvider;
        }

        public ActionResult Index(int page, Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId, criteria.FacultyId, criteria.DepartmentId, criteria.CurriculumId);
            if (criteria.AcademicLevelId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }
            
            var model = (from feeGroup in _db.StudentFeeGroups
                         join academicLevel in _db.AcademicLevels on feeGroup.AcademicLevelId equals academicLevel.Id into academicLevels
                         from academicLevel in academicLevels.DefaultIfEmpty()
                         join studentFeeType in _db.StudentFeeTypes on feeGroup.StudentFeeTypeId equals studentFeeType.Id into studentFeeTypes
                         from studentFeeType in studentFeeTypes.DefaultIfEmpty()
                         join startedTerm in _db.Terms on feeGroup.StartedTermId equals startedTerm.Id into startedTerms
                         from startedTerm in startedTerms.DefaultIfEmpty()
                         join endedTerm in _db.Terms on feeGroup.EndedTermId equals endedTerm.Id into endedTerms
                         from endedTerm in endedTerms.DefaultIfEmpty()
                         where feeGroup.AcademicLevelId == criteria.AcademicLevelId
                               && (string.IsNullOrEmpty(criteria.CodeAndName)
                                   || feeGroup.Code.StartsWith(criteria.CodeAndName)
                                   || feeGroup.Name.StartsWith(criteria.CodeAndName))
                               && (criteria.IsThai == null || feeGroup.IsThai == criteria.IsThai)
                               && (criteria.StudentFeeTypeId == 0
                                   || feeGroup.StudentFeeTypeId == criteria.StudentFeeTypeId)
                         select new StudentFeeGroup 
                                {
                                    Id = feeGroup.Id,
                                    Code = feeGroup.Code,
                                    Name = feeGroup.Name,
                                    AcademicLevel = academicLevel.NameEn,
                                    StartedBatch = feeGroup.StartedBatch,
                                    EndedBatch = feeGroup.EndedBatch,
                                    StartedTerm = startedTerm.TermText,
                                    EndedTerm = endedTerm.TermText,
                                    IsThai = feeGroup.IsThai,
                                    IsLumpsumPayment = feeGroup.IsLumpsumPayment,
                                    StudentFeeTypeId = feeGroup.StudentFeeTypeId,
                                    Remark = feeGroup.Remark,
                                    StudentFeeType = studentFeeType.NameEn
                                }).OrderByDescending(x => x.StartedTerm)
                                      .ThenByDescending(x => x.Code)
                                  .IgnoreQueryFilters()
                                  .GetPaged(criteria, page, true);
                                  
            return View(model);
        }

        [PermissionAuthorize("StudentFeeGroup", PolicyGenerator.Write)]
        public ActionResult Create(string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            return View(new StudentFeeGroup());
        }

        [PermissionAuthorize("StudentFeeGroup", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(StudentFeeGroup model, string returnUrl)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    if (model.TermFees != null)
                    {
                        model.TermFees = model.TermFees.Where(x => x.FeeItemIdAllowNull != 0).ToList();
                        foreach (var item in model.TermFees)
                        {
                            item.IsOneTime = item.CalculateType == "o" ? true : false;
                            item.IsPerTerm = item.CalculateType == "t" ? true : false;
                            item.IsPerYear = item.CalculateType == "y" ? true : false;
                            item.FeeItemId = item.FeeItemIdAllowNull ?? 0;
                            item.Amount = item.AmountAllowNull ?? 0;
                        }
                    }

                    _db.StudentFeeGroups.Add(model);
                    _db.SaveChanges();
                    
                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Index), new { AcademicLevelId = model.AcademicLevelId });
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    if (_exceptionManager.IsDuplicatedEntityCode(e))
                    {
                        _flashMessage.Danger(Message.CodeUniqueConstraintError);
                    }
                    else
                    {
                        _flashMessage.Danger(Message.UnableToCreate);
                    }

                    ViewBag.ReturnUrl = returnUrl;
                    CreateSelectList(model.AcademicLevelId ?? 0, model.FacultyId ?? 0, model.DepartmentId ?? 0, model.CurriculumId ?? 0);
                    return View(model);
                }
            }
        }
        
        public ActionResult Edit(long id, string returnUrl)
        {
            var model = Find(id);
            foreach (var item in model.TermFees)
            {
                item.FeeItemIdAllowNull = item.FeeItemId;
                item.AmountAllowNull = item.Amount;
                item.CalculateType = item.IsOneTime ? "o" : item.IsPerTerm ? "t" : item.IsPerYear ? "y" : string.Empty;
            }

            ViewBag.ReturnUrl = returnUrl;
            CreateSelectList(model.AcademicLevelId ?? 0, model.FacultyId ?? 0, model.DepartmentId ?? 0, model.CurriculumId ?? 0);
            return View(model);
        }

        [PermissionAuthorize("StudentFeeGroup", PolicyGenerator.Write)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(StudentFeeGroup model, string returnUrl)
        {
            var updateStudentFeeGroup = Find(model.Id);
            try
            {
                updateStudentFeeGroup.Code = model.Code;
                updateStudentFeeGroup.Name = model.Name;
                updateStudentFeeGroup.Remark = model.Remark;
                updateStudentFeeGroup.StartedBatch = model.StartedBatch;
                updateStudentFeeGroup.EndedBatch = model.EndedBatch;
                updateStudentFeeGroup.StudentGroupIds = model.StudentGroupIds;
                updateStudentFeeGroup.AcademicLevelId = model.AcademicLevelId;
                updateStudentFeeGroup.FacultyId = model.FacultyId;
                updateStudentFeeGroup.DepartmentId = model.DepartmentId;
                updateStudentFeeGroup.CurriculumId = model.CurriculumId;
                updateStudentFeeGroup.CurriculumVersionId = model.CurriculumVersionId;
                updateStudentFeeGroup.NationalityId = model.NationalityId;
                updateStudentFeeGroup.IsThai = model.IsThai;
                updateStudentFeeGroup.IsForIntensive = model.IsForIntensive;
                updateStudentFeeGroup.StartedTermId = model.StartedTermId;
                updateStudentFeeGroup.EndedTermId = model.EndedTermId;
                updateStudentFeeGroup.IsLumpsumPayment = model.IsLumpsumPayment;
                updateStudentFeeGroup.StudentFeeTypeId = model.StudentFeeTypeId;

                var termFeeIds = model.TermFees.Select(x => x.Id).ToList();
                var oldTermFees = _db.TermFees.Where(x => x.StudentFeeGroupId == model.Id)
                                                .ToList();

                var removeTermFees = _db.TermFees.Where(x => x.StudentFeeGroupId == model.Id
                                                                && !termFeeIds.Contains(x.Id))
                                                    .ToList();
                _db.TermFees.RemoveRange(removeTermFees);
                if (model.TermFees != null)
                {
                    var termFees = new List<TermFee>();
                    for (var i = 0; i < model.TermFees.Count(); i++)
                    {
                        model.TermFees = model.TermFees.Where(x => x.FeeItemIdAllowNull != 0).ToList();
                        if (oldTermFees.Any(x => x.Id == model.TermFees[i].Id))
                        {
                            oldTermFees.Where(x => x.Id == model.TermFees[i].Id)
                                        .Select(x => {
                                                        x.FeeItemId = model.TermFees[i].FeeItemIdAllowNull ?? 0;
                                                        x.StartedBatch = model.TermFees[i].StartedBatch;
                                                        x.EndedBatch = model.TermFees[i].EndedBatch;
                                                        x.StartedTermId = model.TermFees[i].StartedTermId;
                                                        x.EndedTermId = model.TermFees[i].EndedTermId;
                                                        x.TermTypeId = model.TermFees[i].TermTypeId;
                                                        x.Term = model.TermFees[i].Term;
                                                        x.Amount = model.TermFees[i].AmountAllowNull ?? 0;
                                                        x.IsOneTime = model.TermFees[i].CalculateType == "o" ? true : false;
                                                        x.IsPerTerm = model.TermFees[i].CalculateType == "t" ? true : false;
                                                        x.IsPerYear = model.TermFees[i].CalculateType == "y" ? true : false;
                                                        return x;
                                                    })
                                        .ToList();
                        }
                        else
                        {
                            var termFee = new TermFee
                                          {
                                              StudentFeeGroupId = model.Id,
                                              FeeItemId = model.TermFees[i].FeeItemIdAllowNull ?? 0,
                                              StartedBatch = model.TermFees[i].StartedBatch,
                                              EndedBatch = model.TermFees[i].EndedBatch,
                                              StartedTermId = model.TermFees[i].StartedTermId,
                                              EndedTermId = model.TermFees[i].EndedTermId,
                                              TermTypeId = model.TermFees[i].TermTypeId,
                                              Term = model.TermFees[i].Term,
                                              Amount = model.TermFees[i].AmountAllowNull ?? 0,
                                              IsOneTime = model.TermFees[i].CalculateType == "o" ? true : false,
                                              IsPerTerm = model.TermFees[i].CalculateType == "t" ? true : false,
                                              IsPerYear = model.TermFees[i].CalculateType == "y" ? true : false,
                                          };

                            termFees.Add(termFee);
                        }

                        _db.TermFees.AddRange(termFees);
                    }
                }
                
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return Redirect(returnUrl);
            }
            catch (Exception e)
            {
                if (_exceptionManager.IsDuplicatedEntityCode(e))
                {
                    _flashMessage.Danger(Message.CodeUniqueConstraintError);
                }
                else
                {
                    _flashMessage.Danger(Message.UnableToEdit);
                }
                
                ViewBag.ReturnUrl = returnUrl;
                CreateSelectList(model.AcademicLevelId ?? 0, model.FacultyId ?? 0, model.DepartmentId ?? 0, model.CurriculumId ?? 0);
                return View(model);
            }
        }

        [PermissionAuthorize("StudentFeeGroup", PolicyGenerator.Write)]
        public ActionResult Delete(long id)
        {
            var studentFeeGroup = Find(id);
            try
            {
                _db.TermFees.RemoveRange(studentFeeGroup.TermFees);
                _db.StudentFeeGroups.Remove(studentFeeGroup);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }
            
            return RedirectToAction(nameof(Index));
        }
        
        private StudentFeeGroup Find(long? id)
        {
            var model = _db.StudentFeeGroups.Include(x => x.TermFees)
                                            .IgnoreQueryFilters()
                                            .SingleOrDefault(x => x.Id == id);
            return model;
        }

        private void CreateSelectList(long academicLevelId = 0, long facultyId = 0, long departmentId = 0, long curriculumId = 0)
        {
            ViewBag.TermFees = _feeProvider.GetTermFees();
            ViewBag.CalculateTypes = _selectListProvider.GetTermFeeCalculateTypes();
            ViewBag.ThaiStatuses = _selectListProvider.GetThaiStatuses();
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Nationalities = _selectListProvider.GetNationalities();
            ViewBag.TermTypes = _selectListProvider.GetTermTypes();
            ViewBag.FeeItems = _selectListProvider.GetFeeItems();
            ViewBag.StudentFeeTypes = _selectListProvider.GetStudentFeeTypes();

            if (academicLevelId != 0)
            {
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }

            if (facultyId != 0)
            {
                ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
            }

            if (departmentId != 0)
            {
                ViewBag.Curriculums = _selectListProvider.GetCurriculumByDepartment(academicLevelId, facultyId, departmentId);
            }

            if (curriculumId != 0)
            {
                ViewBag.CurriculumVersions = _selectListProvider.GetCurriculumVersion(curriculumId);
            }
        }
    }
}