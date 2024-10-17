using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class CertificateAddressController : BaseController
    {
        protected readonly IDateTimeProvider _dateTimeProvider;

        public CertificateAddressController(ApplicationDbContext db,
                                            IFlashMessage flashMessage,
                                            ISelectListProvider selectListProvider,
                                            IDateTimeProvider dateTimeProvider) : base(db, flashMessage, selectListProvider) 
        {
            _dateTimeProvider = dateTimeProvider;
        }

        public ActionResult Index(Criteria criteria, int page)
        {
            CreateSelectList();
            DateTime? start = _dateTimeProvider.ConvertStringToDateTime(criteria.StartedAt);
            DateTime? end = _dateTimeProvider.ConvertStringToDateTime(criteria.EndedAt);
            if (string.IsNullOrEmpty(criteria.StartedAt) && string.IsNullOrEmpty(criteria.EndedAt)
                && criteria.DistributionMethodId == 0 && string.IsNullOrEmpty(criteria.Status))
            {
                start = DateTime.Today;
                end = DateTime.Today;
                criteria.Status = "a";
            }
            
            var certificate = _db.StudentCertificates.Where(x => (start == null
                                                                  || x.CreatedAt >= start.Value.Date)
                                                                  && (end == null
                                                                      || x.CreatedAt <= end.Value.Date)
                                                                  && (string.IsNullOrEmpty(criteria.Status)
                                                                      || x.Status == criteria.Status)
                                                                  && (criteria.DistributionMethodId == 0 
                                                                      || x.DistributionMethodId == criteria.DistributionMethodId))
                                                     .Select(x => new CertificateAddress
                                                                  {
                                                                      Certificate = x.CertificateText,
                                                                      StudentCode = x.StudentCode,
                                                                      StudentName = x.FullName,
                                                                      CreatedAt = x.CreatedAtText,
                                                                      Email = x.Email,
                                                                      Address = x.Address,
                                                                      TelephoneNumber = x.TelephoneNumber
                                                                  })
                                                     .ToList();

            CertificateAddressViewModel model = new CertificateAddressViewModel
                                                {
                                                    Criteria = criteria,
                                                    Results = certificate.OrderBy(x => x.StudentCode)
                                                                         .ToList()
                                                };

            return View(model);
        }

        private void CreateSelectList()
        {
            ViewBag.DistributionMethods = _selectListProvider.GetDistributionMethods();
            ViewBag.Statuses = _selectListProvider.GetPetitionStatuses();
        }
    }
}