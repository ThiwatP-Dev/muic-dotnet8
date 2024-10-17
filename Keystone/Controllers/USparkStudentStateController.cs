using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;
using Microsoft.AspNetCore.Authorization;
using KeystoneLibrary.Models.USpark;

namespace Keystone.Controllers
{
    [AllowAnonymous]
    public class USparkStudentStateController : BaseController
    {
        protected readonly IRegistrationProvider _registrationProvider;

        public USparkStudentStateController(ApplicationDbContext db, 
                                          IFlashMessage flashMessage, 
                                          IMapper mapper,
                                          IRegistrationProvider registrationProvider) : base(db, flashMessage, mapper)
        {
            _registrationProvider = registrationProvider;
        }

        public IActionResult Update(string studentCode, long ksTermId, string state)
        {
            var result = new USparkAPIResponse();
            var student = _db.Students.SingleOrDefault(x => x.Code == studentCode);
            var term = _db.Terms.SingleOrDefault(x => x.Id == ksTermId);
            if (student == null)
            {
                result.Message = "Student not found";
                return StatusCode(400, result);
            }
            else if (term == null)
            {
                result.Message = "Term not found";
                return StatusCode(400, result);
            }
            else
            {
                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        if (!_registrationProvider.UpdateStudentState(student.Id, studentCode, ksTermId, state, "US", out string errorMsg))
                        {
                            transaction.Rollback();
                            result.Message = errorMsg;
                            return StatusCode(500, result);
                        }

                        transaction.Commit();
                        return StatusCode(200, result);
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        return StatusCode(500, e);
                    }
                }
            }
        }
    }
}