using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Fee;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class TuitionFeeController : BaseController
    {
        protected readonly IExceptionManager _exceptionManager;
        protected readonly IAcademicProvider _academicProvider;

        public TuitionFeeController(ApplicationDbContext db, 
                                    IFlashMessage flashMessage, 
                                    IMapper mapper, 
                                    IExceptionManager exceptionManager,
                                    ISelectListProvider selectListProvider,
                                    IAcademicProvider academicProvider) : base(db, flashMessage, mapper, selectListProvider)
        {
            _exceptionManager = exceptionManager;
            _academicProvider = academicProvider;
        }

        public IActionResult Index(int page)
        {
            var models = (from tuitionFee in _db.TuitionFees
                          join course in _db.Courses on tuitionFee.CourseId equals course.Id into mappedCourse
                          from courseResult in mappedCourse.DefaultIfEmpty()
                          join courseRate in _db.CourseRates on tuitionFee.CourseRateId equals courseRate.Id into mappedCourseRate
                          from courseRateResult in mappedCourseRate.DefaultIfEmpty()
                          join feeItem in _db.FeeItems on tuitionFee.FeeItemId equals feeItem.Id into result
                          from resultFee in result.DefaultIfEmpty()
                          join startedTerm in _db.Terms on tuitionFee.StartedTermId equals startedTerm.Id into resultStart
                          from resultStartedTerm in resultStart.DefaultIfEmpty()
                          join endedTerm in _db.Terms on tuitionFee.EndedTermId equals endedTerm.Id into resultEnd
                          from resultEndedTerm in resultEnd.DefaultIfEmpty()
                          select new TuitionFee
                          {
                              Id = tuitionFee.Id,
                              Code = tuitionFee.Code,
                              StartedBatch = tuitionFee.StartedBatch,
                              EndedBatch = tuitionFee.EndedBatch,
                              StudentGroupIds = tuitionFee.StudentGroupIds,
                              CourseId = tuitionFee.CourseId,
                              SectionNumber = tuitionFee.SectionNumber,
                              FeeItemId = tuitionFee.FeeItemId,
                              CourseRateId = tuitionFee.CourseRateId,
                            //   TuitionFeeFormulaId = tuitionFee.TuitionFeeFormulaId,
                              StartedTermId = tuitionFee.StartedTermId,
                              EndedTermId = tuitionFee.EndedTermId,
                              StartedTerm = resultStartedTerm,
                              EndedTerm = resultEndedTerm,
                              Amount = tuitionFee.Amount,
                              Course = courseResult,
                              CourseRate = courseRateResult,
                              FeeItem = resultFee,
                              IsActive = tuitionFee.IsActive
                          }).IgnoreQueryFilters()
                          .GetPaged(page);
             
            return View(models);
        }

        public ActionResult Create()
        {
            CreateSelectList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TuitionFee model)
        {
            var academicLevelId = GetAcademicLevelId(model);
            CreateSelectList(academicLevelId);
            if (!IsTuitionFeeDuplicate(model))
            {
                if (ModelState.IsValid)
                {
                    _db.TuitionFees.Add(model);

                    try
                    {
                        _db.SaveChanges();

                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception e)
                    {
                        if (_exceptionManager.IsDuplicatedEntityCode(e))
                        {
                            _flashMessage.Danger(Message.CodeUniqueConstraintError);
                        }
                        else
                        {
                            _flashMessage.Danger(Message.UnableToCreate);
                        }

                        return View(model);
                    }
                }

                _flashMessage.Danger(Message.UnableToCreate);
                return View(model);
            }

            _flashMessage.Danger(Message.ExistedTuitionFee);
            return View(model);
        }

        public ActionResult Edit(long id) 
        {
            var model = Find(id);
            var academicLevelId = GetAcademicLevelId(model);
            CreateSelectList(academicLevelId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(TuitionFee model)
        {
            var academicLevelId = GetAcademicLevelId(model);
            CreateSelectList(academicLevelId);
            if (!IsTuitionFeeDuplicate(model))
            {
                var newModel = Find(model.Id);
                if (ModelState.IsValid && await TryUpdateModelAsync(newModel))
                {
                    try
                    {
                        await _db.SaveChangesAsync();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return RedirectToAction(nameof(Index));
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
                        
                        return View(model);
                    }
                }
                
                _flashMessage.Danger(Message.UnableToEdit);
                return View(model);
            }
            
            _flashMessage.Danger(Message.ExistedTuitionFee);
            return View(model);
        }

        public ActionResult Delete(long id)
        {
            var model = Find(id);
            try
            {
                _db.TuitionFees.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Index));
        }

        private List<FeeItem> GetFeeItems()
        {
            var models = _db.FeeItems.ToList();
            return models;
        }

        private long GetAcademicLevelId(TuitionFee model)
        {
            var startedTerm = _academicProvider.GetTerm(model.StartedTermId ?? 0);
            var endedTerm = _academicProvider.GetTerm(model.EndedTermId ?? 0);
            var academicLevelId = startedTerm == null ? endedTerm == null ? 0 : endedTerm.AcademicLevelId : startedTerm.AcademicLevelId;
            return academicLevelId;
        }

        private void CreateSelectList(long academicLevelId = 0)
        {
            ViewBag.FeeItems = _selectListProvider.GetFeeItems();
            ViewBag.Courses = _selectListProvider.GetCourses();
            ViewBag.CourseRates = _selectListProvider.GetCourseRates();
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (academicLevelId > 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
        }

        private TuitionFee Find(long? id)
        {
            var model = _db.TuitionFees.IgnoreQueryFilters()
                                       .SingleOrDefault(x => x.Id == id);
            return model;
        }

        private Boolean IsTuitionFeeDuplicate(TuitionFee model)
        {
            var tuitionFees = _db.TuitionFees.IgnoreQueryFilters()
                                             .Any(x => x.Code == model.Code
                                                       && x.StartedBatch == model.StartedBatch
                                                       && x.EndedBatch == model.EndedBatch
                                                       && x.StudentGroupIds == model.StudentGroupIds
                                                       && x.CourseId == model.CourseId
                                                       && x.FeeItemId == model.FeeItemId
                                                       && x.CourseRateId == model.CourseRateId
                                                    //    && x.TuitionFeeFormulaId == model.TuitionFeeFormulaId
                                                       && x.StartedTermId == model.StartedTermId
                                                       && x.EndedTermId == model.EndedTermId
                                                       && x.SectionNumber == model.SectionNumber
                                                       && x.Amount == model.Amount
                                                       && x.IsActive == model.IsActive);
            return tuitionFees;
        }
    }
}