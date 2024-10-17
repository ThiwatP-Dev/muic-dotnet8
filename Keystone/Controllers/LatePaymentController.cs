using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class LatePaymentController : BaseController
    {
        public LatePaymentController(ApplicationDbContext db,
                                     IFlashMessage flashMessage,
                                     ISelectListProvider selectListProvider,
                                     IMapper mapper) : base(db, flashMessage, mapper, selectListProvider) { }
        
        public IActionResult Index(long academicLevelId, long termId, string returnUrl, int page)
        {
            LatePaymentViewModel model = new LatePaymentViewModel();
            CreateSelectList(academicLevelId);
            if (termId != 0)
            {
                ViewBag.Students = _selectListProvider.GetStudentForLatedPayment(termId);                 
                ViewBag.Signatories = _selectListProvider.GetSignatories();
                model.AcademicLevelId = academicLevelId;
                model.TermId = termId;
                model.LatePayments = FindLatePaymentTransactions(termId);
            }
            else
            {
                _flashMessage.Danger(Message.RequiredData);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        public ActionResult IsModelValid(LatePaymentViewModel model)
        {
            string statusCode =  "0";
            if (ModelState.IsValid)
            {
                if (model.Status == "add" && model.StudentId != Guid.Empty)
                {
                    bool isDuplicatedStudent = _db.LatePaymentTransactions.Any(x => x.StudentId == model.StudentId
                                                                                    && x.TermId == model.TermId);
                    statusCode = isDuplicatedStudent ? "1" : "2";
                    
                } 
                else if (model.Status == "edit")
                {
                    statusCode = "3";
                }
            }
            
            // 0 = "invalid", 1 = "duplicated student", 2 = "valid add", 3 = "valid edit"
            return Json(statusCode);
        }

        public ActionResult Create(LatePaymentTransaction model)
        {
            try
            {
                _db.LatePaymentTransactions.Add(model);
                _db.SaveChanges();
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToCreate);
            }
            
            var latePaymentTransactions = FindLatePaymentTransactions(model.TermId);
            return PartialView("~/Views/LatePayment/_LatePaymentDetails.cshtml", latePaymentTransactions);
        }

        public ActionResult Edit(LatePaymentTransaction model)
        {
            try
            {
                _db.Entry(model).State = EntityState.Modified;
                _db.SaveChanges();
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToEdit);
            }
            
            var latePaymentTransactions = FindLatePaymentTransactions(model.TermId);
            return PartialView("~/Views/LatePayment/_LatePaymentDetails.cshtml", latePaymentTransactions);
        }

        public LatePaymentViewModel FindLatePaymentTransaction(string studentCode, long termId)
        {
            LatePaymentTransaction transaction = _db.LatePaymentTransactions.Include(x => x.Student)
                                                                                .ThenInclude(x => x.RegistrationCourses)
                                                                            .SingleOrDefault(x => x.TermId == termId
                                                                                                  && x.Student.Code == studentCode);

            LatePaymentViewModel model = _mapper.Map<LatePaymentTransaction, LatePaymentViewModel>(transaction);
            return model;
        }

        public ActionResult Delete(long id)
        {
            LatePaymentTransaction model = _db.LatePaymentTransactions.Include(x => x.Term)
                                                                      .SingleOrDefault(x => x.Id == id);
            try
            {
                _db.LatePaymentTransactions.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Index), new 
                                                   {
                                                       academicLevelId = model.Term.AcademicLevelId,
                                                       termId = model.TermId
                                                   });
        }

        private List<LatePaymentViewModel> FindLatePaymentTransactions(long termId)
        {
            var latePaymentTransactions = _db.LatePaymentTransactions.Include(x => x.Student)
                                                                         .ThenInclude(x => x.RegistrationCourses)
                                                                     .Where(x => x.TermId == termId)
                                                                     .ToList()
                                                                     .Select(x => _mapper.Map<LatePaymentTransaction, LatePaymentViewModel>(x))
                                                                     .OrderBy(x => x.StudentCode)
                                                                     .ToList();
            return latePaymentTransactions;
        }

        private void CreateSelectList(long academicLevelId = 0) 
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
            }
           
            ViewBag.Types = _selectListProvider.GetLatePaymentPermissionTypes();
        }
    }
}