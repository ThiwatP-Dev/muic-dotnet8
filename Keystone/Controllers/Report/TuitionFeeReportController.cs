using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;
using Keystone.Permission;

namespace Keystone.Controllers
{
    [PermissionAuthorize("TuitionFeeReport", "")]
    public class TuitionFeeReportController : BaseController
    {
        public TuitionFeeReportController(ApplicationDbContext db,
                                          ISelectListProvider selectListProvider,
                                          IFlashMessage flashMessage) : base(db, flashMessage, selectListProvider) { }

        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList();
            if (criteria.AcademicLevelId == 0
                || (criteria.StartStudentBatch ?? 0) == 0
                || (criteria.EndStudentBatch ?? 0) == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            TuitionFeeReportViewModel model = new TuitionFeeReportViewModel
                                              {
                                                  Criteria = criteria,
                                                  Results = new List<TuitionFeeReportResultViewModel>()
                                              };

            var result = (from tuitionFee in _db.TuitionFees
                          join course in _db.Courses on tuitionFee.CourseId equals course.Id
                          where course.AcademicLevelId == criteria.AcademicLevelId
                                && (string.IsNullOrEmpty(criteria.CourseCode)
                                    || course.Code == criteria.CourseCode)
                                && (string.IsNullOrEmpty(criteria.CourseName)
                                    || course.NameEn.Contains(criteria.CourseName))
                          select new 
                                 {
                                     tuitionFee.CourseId,
                                     course.Code,
                                     course.NameEn,
                                     course.CreditText,
                                     tuitionFee.StartedBatch,
                                     tuitionFee.EndedBatch,
                                     tuitionFee.TuitionFeeFormulaId
                                 }).ToList();
                          
            foreach (var item in result)
            {
                var formular = _db.TuitionFeeFormulas.SingleOrDefault(x => x.Id == item.TuitionFeeFormulaId);
                if (formular != null)
                {
                    //batch loop
                    for(int batch = criteria.StartStudentBatch ?? 0; batch <= (criteria.EndStudentBatch ?? 0); batch++)
                    {
                        if ((item.StartedBatch == null || batch >= item.StartedBatch)
                            && (item.EndedBatch == null || batch <= item.EndedBatch))
                        {
                            var firstRate = _db.TuitionFeeRates.FirstOrDefault(x => x.TuitionFeeTypeId == formular.FirstTuitionFeeTypeId
                                                                                    && (x.StartedBatch == null || batch >= x.StartedBatch)
                                                                                    && (x.EndedBatch == null || batch <= x.EndedBatch))
                                                               ?.Amount ?? 0;
                            var secondRate = _db.TuitionFeeRates.FirstOrDefault(x => x.TuitionFeeTypeId == formular.SecondTuitionFeeTypeId
                                                                                    && (x.StartedBatch == null || batch >= x.StartedBatch)
                                                                                    && (x.EndedBatch == null || batch <= x.EndedBatch))
                                                                ?.Amount ?? 0;
                            model.Results.Add(new TuitionFeeReportResultViewModel
                                              {
                                                  CourseId = item.CourseId ?? 0,
                                                  CourseCode = item.Code,
                                                  CourseName = item.NameEn,
                                                  CreditText = item.CreditText,
                                                  Batch = batch,
                                                  FormularName = formular.Name,
                                                  FirstAmount = firstRate,
                                                  SecondAmount = secondRate
                                              });
                        }
                    }
                }
            }

            model.Results = model.Results.OrderBy(x => x.CourseCode).ToList();
            return View(model);
        }

        public void CreateSelectList()
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
        }
    }
}