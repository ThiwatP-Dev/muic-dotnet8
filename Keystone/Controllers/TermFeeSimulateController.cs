using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class TermFeeSimulateController : BaseController
    {
        protected readonly IStudentProvider _studentProvider;
        protected readonly IFeeProvider _feeProvider;

        public TermFeeSimulateController(ApplicationDbContext db, 
                                         IFlashMessage flashMessage, 
                                         IStudentProvider studentProvider,
                                         IFeeProvider feeProvider,
                                         ISelectListProvider selectListProvider) : base(db, flashMessage, selectListProvider) 
        {
            _studentProvider = studentProvider;
            _feeProvider = feeProvider;
        }   
        
        public IActionResult Index(Criteria criteria)
        {
            ViewBag.TermTypes = _selectListProvider.GetTermTypes();
            var student = _studentProvider.GetStudentInformationById(criteria.StudentId);
            TermFeeSimulateViewModel model = new TermFeeSimulateViewModel
                                             {
                                                 Criteria = criteria,
                                                 StudentFeeGroupName = student.StudentFeeGroup?.Name,
                                                 StudentCode = student.Code,
                                                 StudentFullName = student.FullNameEn,
                                                 TermText = student.AdmissionInformation.AdmissionTerm?.TermText
                                             };

            if (student.AcademicInformation != null && student.AdmissionInformation != null)
            {
                model.Results = _feeProvider.GetStudentTermFees(student.StudentFeeGroupId ?? 0,
                                                                student.AcademicInformation.Batch,
                                                                student.AcademicInformation.AcademicLevelId,
                                                                student.AdmissionInformation.AdmissionTermId ?? 0,
                                                                criteria.NumberOfTerms,
                                                                criteria.TermTypeId);
            }
            
            return View(model);
        }
    }
}