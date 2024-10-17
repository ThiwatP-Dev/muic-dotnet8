using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class ExportProfileController : BaseController
    {
        protected readonly IFileProvider _fileProvider;

        public ExportProfileController(ApplicationDbContext db,
                                       IFlashMessage flashMessage,
                                       ISelectListProvider selectListProvider,
                                       IFileProvider fileProvider) : base(db, flashMessage, selectListProvider)
        {
            _fileProvider = fileProvider;
        }

        public IActionResult Index(Criteria criteria)
        {
            CreateSelectList(criteria.AcademicLevelId);
            if (criteria.AcademicLevelId == 0)
            {
                _flashMessage.Warning(Message.RequiredData);
                return View();
            }

            var results = _db.Students.Include(x => x.AdmissionInformation)  
                                      .Include(x => x.AdmissionInformation)
                                      .Where(x => x.AcademicInformation.AcademicLevelId == criteria.AcademicLevelId
                                                  && (criteria.TermId == 0 || x.AdmissionInformation.AdmissionTermId == criteria.TermId) 
                                                  && (criteria.StudentCodeFrom == null
                                                      || criteria.StudentCodeFrom == 0
                                                      || x.CodeInt >= criteria.StudentCodeFrom)
                                                  && (criteria.StudentCodeTo == null
                                                      || criteria.StudentCodeTo == 0
                                                      || x.CodeInt <= criteria.StudentCodeTo)
                                                  && x.StudentStatus != "d"
                                                  && !string.IsNullOrEmpty(x.ProfileImageURL))
                                      .Select(x => new ExportProfileStudent
                                                   {
                                                       StudentCode = x.Code,
                                                       StudentFullName = x.FullNameEn,
                                                       ProfileImageURL = x.ProfileImageURL
                                                   })
                                      .ToList();

            ExportProfileViewModel model = new ExportProfileViewModel
            {
                Criteria = criteria,
                Results = results.OrderBy(x => x.StudentCode)
                                 .ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Download(List<ExportProfileStudent> results, string returnUrl)
        {
            var downLoadFiles = results.Where(x => x.IsChecked == "on")
                                       .ToList();   
            if (downLoadFiles.Any())
            {
                try
                {
                    _fileProvider.DownloadFiles(downLoadFiles.Select(x => x.ProfileImageURL)
                                                             .ToList());

                    _flashMessage.Confirmation(Message.DownloadSucceed);
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToDownload);
                }
            }
            else
            {
                _flashMessage.Warning(Message.RequiredData);
            }
            
            return Redirect(returnUrl);
        }

        private void CreateSelectList(long academicLevelId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
        }
    }
}