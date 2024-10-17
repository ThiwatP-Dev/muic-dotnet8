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
    public class USparkStudentController : BaseController
    {
        public USparkStudentController(ApplicationDbContext db) : base(db) { }

        [HttpGet("Permission")]
        public IActionResult GetStudentPermission(string studentCode, long termId)
        {
            studentCode = studentCode?.Trim() ?? "";
            if (string.IsNullOrEmpty(studentCode) 
                    //|| termId <= 0
               )
            {
                return Error(ApiException.InvalidParameter());
            }

            var student = _db.Students.AsNoTracking()
                                      .Include(x => x.AdmissionInformation)
                                          .ThenInclude(x => x.AdmissionType)
                                      .FirstOrDefault(x => studentCode.Equals(x.Code));
            if (student == null)
            {
                return Error(StudentAPIException.StudentsNotFound());
            }

            //var term = _db.Terms.AsNoTracking()
            //                    .FirstOrDefault(x => x.Id == termId);
            //if (term == null)
            //{
            //    return Error(StudentAPIException.TermNotFound());
            //}

            USparkStudentPermissionResponse studentPermission = new USparkStudentPermissionResponse()
            {
                StudentCode = studentCode,
                IsAllowPaymentVisible = true,
                IsAllowPayment = false,
                IsAllowRegistration = false,
                IsAllowSignIn = true,

                IsAllowPaymentRemark = "Not valid status",
                IsAllowPaymentVisibleRemark = "Not valid status",
                IsAllowRegistrationRemark = "Not valid status"
        };

            //Hard Coded Check follow requirement TFS #10836
            if (student.AdmissionInformation != null && student.AdmissionInformation.AdmissionType != null)
            {
                var admissionTypeName = student.AdmissionInformation.AdmissionType.NameEn;
                if (student.StudentStatus == "s") // studying
                {
                    switch (admissionTypeName)
                    {
                        case "Exchange inbound":
                            studentPermission.IsAllowPayment = false;
                            studentPermission.IsAllowPaymentVisible = false;
                            studentPermission.IsAllowRegistration = false;
                            studentPermission.IsAllowPaymentRemark = "Exchange inbound";
                            studentPermission.IsAllowPaymentVisibleRemark = "Exchange inbound";
                            studentPermission.IsAllowRegistrationRemark = "Exchange inbound";
                            break;
                        case "Visiting direct application":
                            studentPermission.IsAllowPayment = true;
                            studentPermission.IsAllowPaymentVisible = true;
                            studentPermission.IsAllowRegistration = true;
                            studentPermission.IsAllowPaymentRemark = "Visiting direct application";
                            studentPermission.IsAllowPaymentVisibleRemark = "Visiting direct application";
                            studentPermission.IsAllowRegistrationRemark = "Visiting direct application";
                            break;
                        case "Visiting agency":
                            studentPermission.IsAllowPayment = false;
                            studentPermission.IsAllowPaymentVisible = false;
                            studentPermission.IsAllowRegistration = false;
                            studentPermission.IsAllowPaymentRemark = "Visiting agency";
                            studentPermission.IsAllowPaymentVisibleRemark = "Visiting agency";
                            studentPermission.IsAllowRegistrationRemark = "Visiting agency";
                            break;
                        case "PDU Summer":
                            studentPermission.IsAllowPayment = false;
                            studentPermission.IsAllowPaymentVisible = false;
                            studentPermission.IsAllowRegistration = false;
                            studentPermission.IsAllowPaymentRemark = "PDU Summer";
                            studentPermission.IsAllowPaymentVisibleRemark = "PDU Summer";
                            studentPermission.IsAllowRegistrationRemark = "PDU Summer";
                            break;
                        case "External":
                            studentPermission.IsAllowPayment = false;
                            studentPermission.IsAllowPaymentVisible = false;
                            studentPermission.IsAllowRegistration = false;
                            studentPermission.IsAllowPaymentRemark = "External";
                            studentPermission.IsAllowPaymentVisibleRemark = "External";
                            studentPermission.IsAllowRegistrationRemark = "External";
                            break;
                        default:
                            studentPermission.IsAllowPayment = true;
                            studentPermission.IsAllowPaymentVisible = true;
                            studentPermission.IsAllowRegistration = true;
                            studentPermission.IsAllowPaymentRemark = "Normal Studying";
                            studentPermission.IsAllowPaymentVisibleRemark = "Normal Studying";
                            studentPermission.IsAllowRegistrationRemark = "Normal Studying";
                            break;
                    }
                }
                else if (student.StudentStatus == "ex") //exchange
                {
                    studentPermission.IsAllowPayment = true;
                    studentPermission.IsAllowPaymentVisible = true;
                    studentPermission.IsAllowRegistration = false;
                    studentPermission.IsAllowPaymentRemark = "Exchange";
                    studentPermission.IsAllowPaymentVisibleRemark = "Exchange";
                    studentPermission.IsAllowRegistrationRemark = "Exchange";
                }
                //else with default as not allow regis, or payment
            }

            // Incident take priority overwrite above setting
            var incidents = _db.StudentIncidents.AsNoTracking()
                                                .Include(x => x.Incident)
                                                .Where(x => x.IsActive
                                                                && x.StudentId == student.Id)
                                                .OrderByDescending(x => x.UpdatedAt)
                                                .ToList();

            if (incidents != null && incidents.Any())
            {
                var lockedPayment = incidents.FirstOrDefault(x => x.LockedPayment);
                if (lockedPayment != null)
                {
                    studentPermission.IsAllowPayment = false;
                    studentPermission.IsAllowPaymentRemark = $"{lockedPayment.Incident.NameEn} by {lockedPayment.ApprovedBy}";
                }

                var lockedRegistration = incidents.FirstOrDefault(x => x.LockedRegistration);
                if (lockedRegistration != null)
                {
                    studentPermission.IsAllowRegistration = false;
                    studentPermission.IsAllowRegistrationRemark = $"{lockedRegistration.Incident.NameEn} by {lockedRegistration.ApprovedBy}";
                }

                var lockedSignIn = incidents.FirstOrDefault(x => x.LockedSignIn);
                if (lockedSignIn != null)
                {
                    studentPermission.IsAllowSignIn = false;
                    studentPermission.IsAllowSignInRemark = $"{lockedSignIn.Incident.NameEn} by {lockedSignIn.ApprovedBy}";
                }
            }

            return Success(studentPermission);
        }

    }
}