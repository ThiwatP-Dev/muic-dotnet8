using KeystoneLibrary.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using KeystoneLibrary.Models.USpark;
using KeystoneLibrary.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Keystone.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("[controller]")]
    public class USparkConfigController : BaseController
    {
        public USparkConfigController(ApplicationDbContext db) : base(db) { }

        [HttpGet("Term/{ksTermId}")]
        public IActionResult GetTermInfo(long ksTermId)
        {
            var term = _db.Terms.AsNoTracking()
                                .Include(x => x.TermType)
                                .FirstOrDefault(x => x.Id == ksTermId);
            if (term == null)
            {
                return Error(StudentAPIException.TermNotFound());
            }

            var retObj = new USparkTermViewModel
            {
                AcademicTerm = term.AcademicTerm,
                AcademicYear = term.AcademicYear,
                AddDropPaymentEndedAt = term.AddDropPaymentEndedAt,
                EndedAt = term.EndedAt,
                FirstRegistrationEndedAt = term.FirstRegistrationEndedAt,
                FirstRegistrationPaymentEndedAt = term.FirstRegistrationPaymentEndedAt,
                //IsAdmission = term.IsAdmission,
                //IsAdvising = term.IsAdvising,
                //IsQuestionnaire = term.IsQuestionnaire,
                //IsRegistration = term.IsRegistration,
                KsTermId = term.Id,
                LastPaymentEndedAt = term.LastPaymentEndedAt,
                StartedAt = term.StartedAt,
                TermPeriodText = term.TermPeriodText,
                TermText = term.TermText,
                TermThText = term.TermThText,
                TermTypeId = term.TermTypeId,
                TermTypeText = term.TermType.NameEn
            };


            return Success(retObj);
        }

    }
}