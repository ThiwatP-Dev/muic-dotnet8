using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Profile;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Keystone.Controllers
{
    public class CitizenCardReaderController : BaseController
    {
        protected readonly IStudentProvider _studentProvider;
        protected readonly IFileProvider _fileProvider;

        public CitizenCardReaderController(ApplicationDbContext db, 
                                           IFlashMessage flashMessage,
                                           ISelectListProvider selectListProvider,
                                           IStudentProvider studentProvider,
                                           IFileProvider fileProvider) : base(db, flashMessage, selectListProvider)
        {
            _studentProvider = studentProvider;
            _fileProvider = fileProvider;
        }

        public IActionResult Index(string load, string returnUrl)
        {
            CitizenCardReaderViewModel model = new CitizenCardReaderViewModel();
            model.StudentStatus = "a";
            if (!string.IsNullOrEmpty(load))
            {
                try
                {
                    var student = _studentProvider.GetStudentInformationByCitizenCard();
                    model.TitleId = student.TitleId;
                    model.FirstNameEn = student.FirstNameEn;
                    model.MidNameEn = student.MidNameEn;
                    model.LastNameEn = student.LastNameEn;
                    model.FirstNameTh = student.FirstNameTh;
                    model.MidNameTh = student.MidNameTh;
                    model.LastNameTh = student.LastNameTh;
                    model.Gender = student.Gender;
                    model.BirthDate = student.BirthDate;
                    model.NationalityId = student.NationalityId ?? 0;
                    model.CitizenNumber = student.CitizenNumber;
                    model.ProfileImage64 = student.ProfileImage64;
                    var address = student.StudentAddresses[0];
                    model.HouseNumber = address.HouseNumber;
                    model.Moo = address.Moo;
                    model.SoiTh = address.SoiTh;
                    model.CountryId = address.CountryId;
                    model.ProvinceId = address.ProvinceId;
                    model.DistrictId = address.DistrictId;
                    model.SubdistrictId = address.SubdistrictId;
                    model.StudentFeeTypeId = student.StudentFeeGroup.StudentFeeTypeId;

                    model.AppliedAt = DateTime.Today;
                }
                catch
                {
                    _flashMessage.Danger(Message.UnableToRead);
                }
            }
            
            CreateModelSelectList(model);

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(CitizenCardReaderViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var student = new Student();
                try
                {
                    string imageUrl = string.Empty;
                    if (!String.IsNullOrEmpty(model.ProfileImage64)) 
                    {
                        imageUrl = _fileProvider.UploadFile(UploadSubDirectory.STUDENT_PROFILE_IMAGE, model.ProfileImage64, "png", model.Code);
                    }

                    student = new Student
                    {
                        Code = model.Code,
                        TitleId = model.TitleId,
                        FirstNameTh = model.FirstNameTh,
                        FirstNameEn = model.FirstNameEn,
                        LastNameTh = model.LastNameTh,
                        LastNameEn = model.LastNameEn,
                        Gender = model.Gender,
                        RaceId = model.RaceId,
                        NationalityId = model.NationalityId,
                        ReligionId = model.ReligionId,
                        BirthDate = model.BirthDate,
                        BirthProvinceId = model.BirthProvinceId,
                        BirthStateId = model.BirthStateId,
                        BirthCountryId = model.BirthCountryId,
                        CitizenNumber = model.CitizenNumber,
                        Passport = model.Passport,
                        Email = model.Email,
                        PersonalEmail = model.PersonalEmail,
                        TelephoneNumber1 = model.TelephoneNumber1,
                        TelephoneNumber2 = model.TelephoneNumber2,
                        RegistrationStatusId = 1,
                        DeformationId = model.DeformationId,
                        DisableBookExpiredAt = model.DisableBookExpiredAt,
                        DisableBookIssuedAt = model.DisableBookIssuedAt,
                        DisabledBookCode = model.DisabledBookCode,
                        LivingStatus = model.LivingStatus,
                        MaritalStatus = model.MaritalStatus,
                        MidNameEn = model.MidNameEn,
                        MidNameTh = model.MidNameTh,
                        Talent = model.Talent,
                        BirthCityId = model.BirthCityId,
                        Facebook = model.Facebook,
                        Line = model.Line,
                        OtherContact = model.OtherContact,
                        StudentStatus = model.StudentStatus,
                        ProfileImageURL = imageUrl
                    };

                    _db.Students.Add(student);
                    _db.SaveChanges();
                }
                catch
                {
                    ViewBag.ReturnUrl = returnUrl;
                    _flashMessage.Danger(Message.UnableToCreate);
                    return View(model);
                }

                bool isError = false;
                List<string> errorMessage = new List<string>();
                try
                {
                    var address = new StudentAddress
                    {
                        StudentId = student.Id,
                        HouseNumber = model.HouseNumber,
                        AddressTh1 = model.AddressTh1,
                        AddressTh2 = model.AddressTh2,
                        AddressEn1 = model.AddressEn1,
                        AddressEn2 = model.AddressEn2,
                        Moo = model.Moo,
                        SoiTh = model.SoiTh,
                        RoadTh = model.RoadTh,
                        SoiEn = model.SoiEn,
                        RoadEn = model.RoadEn,
                        CountryId = model.CountryId,
                        ProvinceId = model.ProvinceId,
                        DistrictId = model.DistrictId,
                        SubdistrictId = model.SubdistrictId,
                        CityId = model.CityId,
                        StateId = model.StateId,
                        ZipCode = model.ZipCode,
                        TelephoneNumber = model.TelephoneNumber,
                        Type = "p"
                    };

                    _db.StudentAddresses.Add(address);
                    _db.SaveChanges();
                }
                catch
                {
                    isError = true;
                    errorMessage.Add("Unable to save student address, invalid input in some fields.");
                }

                try
                {
                    var admission = new AdmissionInformation
                    {
                        StudentId = student.Id,
                        PreviousSchoolId = model.PreviousSchoolId,
                        EducationBackgroundId = model.EducationBackgroundId,
                        PreviousGraduatedYear = model.PreviousGraduatedYear,
                        PreviousSchoolGPA = model.PreviousSchoolGPA,
                        AdmissionTypeId = model.AdmissionTypeId,
                        AdmissionTermId = model.AdmissionTermId,
                        AdmissionDate = model.AdmissionDate,
                        AppliedAt = model.AppliedAt,
                        AdmissionDocumentGroupId = model.AdmissionDocumentGroupId,
                        AdmissionPlaceId = model.AdmissionPlaceId,
                        AdmissionRoundId = model.AdmissionRoundId,
                        AgencyId = model.AgencyId,
                        CurriculumVersionId = model.CurriculumVersionId,
                        DepartmentId = model.DepartmentId,
                        EntranceExamResult = model.EntranceExamResult,
                        FacultyId = model.FacultyId,
                        OfficerName = model.OfficerName,
                        OfficerPhone = model.OfficerPhone,
                        PreviousBachelorSchoolId = model.PreviousBachelorSchoolId,
                        PreviousMasterSchoolId = model.PreviousMasterSchoolId,
                        ChannelId = model.ChannelId
                    };

                    _db.AdmissionInformations.Add(admission);
                    _db.SaveChanges();

                    if (admission.Id != 0 && admission.AdmissionDocumentGroupId != null)
                    {
                        var success = _studentProvider.SaveDocumentStudentByDocumentGroup(admission.StudentId, admission.AdmissionDocumentGroupId ?? 0);
                        if (!success)
                        {
                            errorMessage.Add(Message.UnableToSaveDocument);
                        }
                    }
                }
                catch
                {
                    isError = true;
                    errorMessage.Add(Message.UnableToSaveAdmisison);
                }

                try
                {
                    var academic = new AcademicInformation
                    {
                        StudentId = student.Id,
                        Batch = model.Batch,
                        StudentGroupId = model.StudentGroupId,
                        GPA = 0,
                        CreditComp = 0,
                        CreditExempted = 0,
                        CreditEarned = 0,
                        CreditTransfer = 0,
                        CurriculumVersionId = model.CurriculumVersionId,
                        AcademicProgramId = model.AcademicProgramId,
                        AcademicLevelId = model.AcademicLevelId,
                        DegreeName = _db.CurriculumVersions.SingleOrDefault(x => x.Id == model.CurriculumVersionId)?.DegreeNameEn,
                        FacultyId = model.FacultyId ?? 0,
                        DepartmentId = model.DepartmentId,
                        // StudentFeeGroupId = model.StudentFeeGroupId ?? 1
                    };

                    _db.AcademicInformations.Add(academic);
                    _db.SaveChanges();
                }
                catch
                {
                    isError = true;
                    errorMessage.Add(Message.UnableToSaveAcademic);
                }

                if (isError)
                {
                    _flashMessage.Danger(string.Join(", ", errorMessage));
                }
                
                return RedirectToAction("Details", "AdmissionStudent", new { codeOrNumber = student.Code });
            }

            ViewBag.ReturnUrl = returnUrl;
            _flashMessage.Danger(Message.UnableToCreate);
            return View(model);
        }

        public void CreateModelSelectList(CitizenCardReaderViewModel model)
        {
            var academicLevelId = model.AcademicLevelId;
            var facultyId = model.FacultyId ?? 0;
            var curriculumId = model.CurriculumId ?? 0;
            var curriculumVersionId = model.CurriculumVersionId ?? 0;
            var departmentId = model.DepartmentId ?? 0;
            var admissionTermId = model.AdmissionTermId;
            var previousSchoolCountryId = model.SchoolCountryId ?? 0;
            var previousSchoolId = model.PreviousSchoolId ?? 0;
            var admissionRoundId = model.AdmissionRoundId ?? 0;
            var nationalityId = model?.NationalityId ?? 0;
            var batch = model.Batch;
            var studentGroupId = model.StudentGroupId;
            var studentFeeTypeId = model?.StudentFeeTypeId ?? 0;

            ViewBag.AcademicLevels = _selectListProvider.GetAcademicLevels();
            ViewBag.TitlesEn = _selectListProvider.GetTitlesEn();
            ViewBag.Races = _selectListProvider.GetRaces();
            ViewBag.Nationalities = _selectListProvider.GetNationalities();
            ViewBag.Religions = _selectListProvider.GetReligions();
            ViewBag.Countries = _selectListProvider.GetCountries();
            ViewBag.Provinces = _selectListProvider.GetProvinces(model.CountryId);
            ViewBag.Districts = _selectListProvider.GetDistricts(model.ProvinceId ?? 0);
            ViewBag.SubDistricts = _selectListProvider.GetSubdistricts(model.DistrictId ?? 0);
            ViewBag.MaritalStatuses = _selectListProvider.GetMaritalStatuses();
            ViewBag.LivingStatuses = _selectListProvider.GetLivingStatuses();
            ViewBag.Deformations = _selectListProvider.GetDeformations();
            ViewBag.AdmissionTypes = _selectListProvider.GetAdmissionTypes();
            ViewBag.PreviousSchools = _selectListProvider.GetPreviousSchools();
            ViewBag.EducationBackgrounds = _selectListProvider.GetEducationBackground();
            ViewBag.EntranceExamResults = _selectListProvider.GetEntranceExamResults();
            ViewBag.Agencies = _selectListProvider.GetAgencies();
            ViewBag.AdmissionChannels = _selectListProvider.GetAdmissionChannels();
            ViewBag.StudentGroups = _selectListProvider.GetStudentGroups();
            ViewBag.DocumentGroups = _selectListProvider.GetStudentDocumentGroups();
            ViewBag.AdmissionPlaces = _selectListProvider.GetAdmissionPlaces();

            if (academicLevelId != 0)
            {
                ViewBag.Terms = _selectListProvider.GetTermsByAcademicLevelId(academicLevelId);
                ViewBag.AdmissionRounds = admissionTermId == 0 ? _selectListProvider.GetAdmissionRoundByAcademicLevelId(academicLevelId)
                                                               : _selectListProvider.GetAdmissionRoundByTermId(admissionTermId);
                ViewBag.Faculties = _selectListProvider.GetFacultiesByAcademicLevelId(academicLevelId);
                ViewBag.StudentFeeGroups = _selectListProvider.GetStudentFeeGroups(academicLevelId, facultyId, departmentId, curriculumId,
                                                                                   curriculumVersionId, nationalityId, batch, studentGroupId,
                                                                                   studentFeeTypeId);
                ViewBag.AcademicPrograms = _selectListProvider.GetAcademicProgramsByAcademicLevelId(academicLevelId);

                if (facultyId != 0)
                {
                    ViewBag.Departments = _selectListProvider.GetDepartmentsByAcademicLevelIdAndFacultyId(academicLevelId, facultyId);
                }
            }

            if (academicLevelId != 0 && facultyId != 0 && departmentId != 0)
            {
                if (admissionTermId != 0)
                {
                    ViewBag.AdmissionCurriculumVersions = _selectListProvider.GetCurriculumVersionForAdmissionCurriculum(admissionTermId, admissionRoundId, facultyId, departmentId);
                }
            }
        }
    }
}