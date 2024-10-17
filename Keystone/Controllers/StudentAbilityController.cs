using Microsoft.AspNetCore.Mvc;
using KeystoneLibrary.Models;
using KeystoneLibrary.Data;
using KeystoneLibrary.Models.DataModels.Profile;
using Vereyon.Web;
using KeystoneLibrary.Interfaces;

namespace Keystone.Controllers
{
    public class StudentAbilityController : BaseController
    {
        protected readonly IStudentProvider _studentProvider;
        
        public StudentAbilityController(ApplicationDbContext db,
                                        IStudentProvider studentProvider,
                                        IFlashMessage flashMessage,
                                        ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) 
        {
            _studentProvider = studentProvider;
        }

        public IActionResult CreateAbility(Guid id, string returnUrl)
        {
            CreateSelectList();
            SpecializationGroupInformation abilityStudent = new SpecializationGroupInformation();
            // abilityStudent.Student = _studentProvider.GetStudentById(id);
            // abilityStudent.StudentId = id;
            ViewBag.ReturnUrl = returnUrl;
            return View(abilityStudent);            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAbility(Guid id, long abilityId, string returnUrl)
        {
            CreateSelectList();
            ViewBag.ReturnUrl = returnUrl;
            var isExist = _db.SpecializationGroupInformations.Any(x => id == x.CurriculumInformation.StudentId
                                                                       && abilityId == x.SpecializationGroupId);
            
            if (isExist)
            {
                _flashMessage.Danger(Message.ExistedAbility);
                return RedirectToAction("Details", nameof(Student), new { id = id, tabIndex = "1", returnUrl = returnUrl });
            }

            try
            {
                _db.SpecializationGroupInformations.Add(new SpecializationGroupInformation()
                                                        {
                                                            // StudentId = id,
                                                            SpecializationGroupId = abilityId
                                                        });
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction("Details", nameof(Student), new { id = id, tabIndex = "1", returnUrl = returnUrl });
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToSave);
                return RedirectToAction("Details", nameof(Student), new { id = id , tabIndex = "1", returnUrl = returnUrl });
            }
        }

        public ActionResult DeleteAbility(long Id)
        {
            var model = _db.SpecializationGroupInformations.SingleOrDefault(x => Id == x.Id);
            var code = _studentProvider.GetStudentCodeById(model.CurriculumInformation?.StudentId ?? Guid.Empty);

            try
            {
                _db.SpecializationGroupInformations.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
                return RedirectToAction("Details", nameof(Student), new { code = code, tabIndex = "1"});
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
                return RedirectToAction("Details", nameof(Student), new { code = code, tabIndex = "1"});
            }
        }

        private void CreateSelectList() 
        {
            ViewBag.StudentAbilities = _selectListProvider.GetAbilities();
        }
    }
}