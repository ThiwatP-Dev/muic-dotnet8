using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Admission;
using KeystoneLibrary.Models.Report;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class VerificationLetterController : BaseController
    {
        protected readonly IAdmissionProvider _admissionProvider;

        public VerificationLetterController(ApplicationDbContext db, 
                                            IFlashMessage flashMessage, 
                                            IMapper mapper, 
                                            ISelectListProvider selectListProvider,
                                            IAdmissionProvider admissionProvider) : base(db, flashMessage, mapper, selectListProvider)
        {
            _admissionProvider = admissionProvider;
        }

        public IActionResult Index(Criteria criteria, int page = 1)
        {
            CreateSelectList();
            var model = _db.VerificationLetters.Include(x => x.SchoolGroup)
                                               .Include(x => x.PreviousSchool)
                                               .Include(x => x.VerificationStudents)
                                               .Where(x => (criteria.RunningNumber == 0
                                                            || x.RunningNumber == criteria.RunningNumber)
                                                           && (criteria.DocumentYear == 0
                                                               || x.Year == criteria.DocumentYear)
                                                           && (criteria.TermId == 0
                                                               || x.AdmissionTermId == criteria.TermId)
                                                           && (criteria.AdmissionRoundId == 0
                                                               || x.AdmissionTermId == criteria.AdmissionRoundId)
                                                           && (criteria.StudentCodeFrom == null
                                                               || x.StudentCodeFromInt >= criteria.StudentCodeFrom)
                                                           && (criteria.StudentCodeTo == null
                                                               || x.StudentCodeToInt <= criteria.StudentCodeTo)
                                                           && (criteria.SchoolGroupId == 0
                                                               || x.SchoolGroupId == criteria.SchoolGroupId)
                                                           && (criteria.PreviousSchoolId == 0
                                                               || x.PreviousSchoolId == criteria.PreviousSchoolId)
                                                           && (criteria.StartStudentBatch == null
                                                               || x.BatchFrom >= criteria.StartStudentBatch)
                                                           && (criteria.EndStudentBatch == null
                                                               || x.BatchTo <= criteria.EndStudentBatch))
                                               .GetPaged(criteria, page);
            return View(model);
        }

        public IActionResult Create()
        {
            CreateSelectList();
            var year = DateTime.Now.Year;
            var model = new VerificationLetter
                        {
                            RunningNumber = _admissionProvider.GetVerificationLetterRunningNumber(year),
                            Year = year,
                            SentAt = DateTime.Now
                        };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(VerificationLetter model)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    model.VerificationStudents.RemoveAll(x => x.IsChecked != "on");
                    model.VerificationStudents.Select(x => {
                                                               x.Student = null;
                                                               return x;
                                                           })
                                              .ToList();

                    _db.VerificationLetters.Add(model);
                    _db.SaveChanges();

                    var studentIds = model.VerificationStudents.Select(x => x.StudentId)
                                                               .ToList();
                    _admissionProvider.SetStudentVerificationLetter(studentIds, model.RunningNumberString, model.SentAt);
                    _db.SaveChanges();

                    transaction.Commit();
                    _flashMessage.Confirmation(Message.SaveSucceed);
                    return RedirectToAction(nameof(Preview), new { id = model.Id });
                }
                catch
                {
                    transaction.Rollback();
                    _flashMessage.Danger(Message.UnableToCreate);
                    CreateSelectList(model.AcademicLevelId, model.AdmissionTermId ?? 0, model.SchoolGroupId ?? 0);
                    return View(model);
                }
            }
        }

        public IActionResult Edit(long id)
        {
            var model = _admissionProvider.GetVerificationLetter(id);
            CreateSelectList(model.AcademicLevelId, model.AdmissionTermId ?? 0, model.SchoolGroupId ?? 0);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VerificationLetter model)
        {   
            var verificationLetter = _admissionProvider.GetVerificationLetter(model.Id);
            if (ModelState.IsValid)
            {
                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        if (model.VerificationStudents != null && model.VerificationStudents.Any())
                        {
                            // remove unchecked students
                            var verificationStudents = _db.VerificationStudents.Where(x => x.VerificationLetterId == model.Id);
                            _db.VerificationStudents.RemoveRange(verificationStudents);

                            var uncheckedStudentIds = model.VerificationStudents.Where(x => x.IsChecked != "on")
                                                                                .Select(x => x.StudentId)
                                                                                .ToList();
                            _admissionProvider.SetStudentVerificationLetter(uncheckedStudentIds, null, null);

                            // update checked students
                            var verificationstudentToUpdate = model.VerificationStudents.Where(x => x.IsChecked == "on")
                                                                                        .Select(x => new VerificationStudent
                                                                                                     {
                                                                                                         StudentId = x.StudentId,
                                                                                                         VerificationLetterId = model.Id
                                                                                                     })
                                                                                        .ToList();

                            var studentIdsToUpdate = verificationstudentToUpdate.Select(x => x.StudentId)
                                                                                .ToList();
                            _admissionProvider.SetStudentVerificationLetter(studentIdsToUpdate, model.RunningNumberString, model.SentAt);
                            _db.VerificationStudents.AddRange(verificationstudentToUpdate);
                        }

                        await _db.SaveChangesAsync();
                        transaction.Commit();
                        _flashMessage.Confirmation(Message.SaveSucceed);
                        return RedirectToAction(nameof(Preview), new { id = model.Id });
                    }
                    catch
                    {
                        transaction.Rollback();
                        CreateSelectList(model.AcademicLevelId, model.AdmissionTermId ?? 0, model.SchoolGroupId ?? 0);
                        _flashMessage.Danger(Message.UnableToEdit);
                        return View(model);
                    }
                }
            }
            else
            {
                CreateSelectList(model.AcademicLevelId, model.AdmissionTermId ?? 0, model.SchoolGroupId ?? 0);
                _flashMessage.Danger(Message.UnableToEdit);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(long id, string receivedNumber, DateTime? receivedAt)
        {
            var verificationLetter = _admissionProvider.GetVerificationLetter(id);
            try
            {
                verificationLetter.ReceivedNumber = receivedNumber;
                verificationLetter.ReceivedAt = receivedAt;
                var studentIds = verificationLetter.VerificationStudents.Select(x => x.StudentId)
                                                                        .ToList();
                _admissionProvider.SetStudentReplyVerificationLetter(studentIds, receivedNumber, receivedAt);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToEdit);
            }
            
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(long id)
        {
            var model = _admissionProvider.GetVerificationLetter(id);
            try
            {
                _db.VerificationLetters.Remove(model);
                _db.SaveChanges();
                _flashMessage.Confirmation(Message.SaveSucceed);
            }
            catch
            {
                _flashMessage.Danger(Message.UnableToDelete);
            }

            return RedirectToAction(nameof(Index), new
                                                   {
                                                       AcademicLevelId = model.AcademicLevelId,
                                                       SchoolGroupId = model.SchoolGroupId,
                                                       PreviousSchoolId = model.PreviousSchoolId 
                                                   });
        }

        public IActionResult Preview(long id)
        {
            var verificationLetter = _db.VerificationLetters.Include(x => x.Signatory)
                                                            .Include(x => x.PreviousSchool)
                                                            .Include(x => x.SchoolGroup)
                                                            .SingleOrDefault(x => x.Id == id);
            var model = _mapper.Map<VerificationLetter, VerificationLetterViewModel>(verificationLetter);

            var students = _db.VerificationStudents.Include(x => x.Student)
                                                       .ThenInclude(x => x.AdmissionInformation)
                                                       .ThenInclude(x => x.PreviousSchool)
                                                   .Where(x => x.VerificationLetterId == id)
                                                   .Select(x => _mapper.Map<VerificationStudent, VerificationStudentViewModel>(x))
                                                   .ToList();
            model.Students = students;
            
            return View(model);
        }

        private void CreateSelectList(long academicLevelId = 0, long termId = 0, long schoolGroupId = 0)
        {
            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.SchoolGroups = _selectListProvider.GetSchoolGroup();
            ViewBag.Signatories = _selectListProvider.GetSignatories();
            ViewBag.PreviousSchools = _selectListProvider.GetPreviousSchools(schoolGroupId);

            if (academicLevelId > 0)
            {
                ViewBag.AdmissionTerms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.AdmissionRounds = _selectListProvider.GetAdmissionRounds(academicLevelId, termId);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public PartialViewResult GetStudent([FromBody]VerificationLetter verificationLetter)
        {
            verificationLetter.VerificationStudents = _admissionProvider.GetverificationLetterByVerificationLetterId(verificationLetter.Id);
            var students = _admissionProvider.GetVerificationStudents(verificationLetter);
            verificationLetter.VerificationStudents.AddRange(students);
            return PartialView("~/Views/VerificationLetter/_StudentList.cshtml", verificationLetter.VerificationStudents);
        }
    }
}