using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using KeystoneLibrary.Exceptions;
using Microsoft.EntityFrameworkCore;
using KeystoneLibrary.Models.DataModels.Profile;
using KeystoneLibrary.Models.DataModels.Fee;
using KeystoneLibrary.Helpers;
using KeystoneLibrary.Models.DataModels.MasterTables;
using System.Globalization;
using System.Text;
using KeystoneLibrary.Enumeration;
using KeystoneLibrary.Providers;

namespace Keystone.Controllers
{
    public class StudentAPIController : BaseController
    {
        protected readonly IStudentProvider _studentProvider;

        public StudentAPIController(ApplicationDbContext db,
                                    IStudentProvider studentProvider,
                                    KeystoneLibrary.Interfaces.IConfigurationProvider configurationProvider,
                                    IFileProvider fileProvider) : base(db, configurationProvider)
        {
            _studentProvider = studentProvider;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("StudentAPI/Adds")]
        public ActionResult AddStudents([FromBody] SaveStudentsViewModelV2 request)
        {
            if (ValidApiKey())
            {
                return Unauthorized(ApiException.InvalidKey());
            }

            try
            {
                if (request == null)
                {
                    return BadRequest(ApiException.InvalidParameter());
                }

                var studentGroup = _db.StudentGroups.AsNoTracking()
                                                    .FirstOrDefault(x => x.Code.ToUpper() == request.StudentGroupCode.ToUpper());

                if (studentGroup is null)
                {
                    return Error(StudentAPIException.StudentGroupNotFound(request.StudentGroupCode));
                }

                AdmissionType admissionType = null;

                if (!string.IsNullOrEmpty(request.AdmissionTypeNameEn))
                {
                    admissionType = _db.AdmissionTypes.AsNoTracking()
                                                      .FirstOrDefault(x => x.NameEn.ToUpper() == request.AdmissionTypeNameEn.ToUpper());

                    if (admissionType is null)
                    {
                        return Error(StudentAPIException.AdmissionTypeNotFound(request.AdmissionTypeNameEn));
                    }
                }

                if (string.IsNullOrEmpty(request.ResidentTypeNameEn))
                {
                    return BadRequest(ApiException.InvalidParameter("'Resident Type' is required"));
                }
                var residentType = _db.ResidentTypes.AsNoTracking()
                                                    .FirstOrDefault(x => x.NameEn.ToUpper() == request.ResidentTypeNameEn.ToUpper());
                if (residentType is null)
                {
                    return Error(StudentAPIException.ResidentTypeNotFound(request.ResidentTypeNameEn));
                }

                if (string.IsNullOrEmpty(request.StudentFeeTypeNameEn))
                {
                    return BadRequest(ApiException.InvalidParameter("'Student Fee Type' is required"));
                }
                var studentFeeType = _db.StudentFeeTypes.AsNoTracking()
                                                        .FirstOrDefault(x => x.NameEn.ToUpper() == request.StudentFeeTypeNameEn.ToUpper());
                if (studentFeeType is null)
                {
                    return Error(StudentAPIException.StudentFeeTypeNotFound(request.StudentFeeTypeNameEn));
                }

                StudentFeeGroup studentFeeGroup = null;

                if (!string.IsNullOrEmpty(request.StudentFeeGroupName))
                {
                    studentFeeGroup = _db.StudentFeeGroups.AsNoTracking()
                                                          .FirstOrDefault(x => x.Name.ToUpper() == request.StudentFeeGroupName.ToUpper());

                    if (studentFeeGroup is null)
                    {
                        return Error(StudentAPIException.StudentFeeGroupNotFound(request.StudentFeeGroupName));
                    }
                }

                if (string.IsNullOrEmpty(request.Code) || request.Code.Length < 2 || !int.TryParse(request.Code.Substring(0, 2), out int batch))
                {
                    return Error(StudentAPIException.InvalidStudentCode(request.Code));
                }

                var duplicateStudentCodes = _db.Students.AsNoTracking()
                                                        .Where(x => x.Code == request.Code);

                if (duplicateStudentCodes.Any())
                {
                    return Error(StudentAPIException.DuplicateStudentCode(request.Code));
                }

                if (request.AcademicYearTerm.IndexOf("Y", StringComparison.InvariantCultureIgnoreCase) < 0
                    || request.AcademicYearTerm.IndexOf("T", StringComparison.InvariantCultureIgnoreCase) < 0
                    || request.AcademicYearTerm.Length < 5)
                {
                    return Error(StudentAPIException.InvalidTermFormat(request.AcademicYearTerm));
                }

                var yearStartIndex = request.AcademicYearTerm.IndexOf("Y", StringComparison.InvariantCultureIgnoreCase);
                var termStartIndex = request.AcademicYearTerm.IndexOf("T", StringComparison.InvariantCultureIgnoreCase);

                if (!int.TryParse(request.AcademicYearTerm.Substring(yearStartIndex + 1, termStartIndex - 1), out int year))
                {
                    return Error(StudentAPIException.InvalidTermFormat(request.AcademicYearTerm));
                }

                if (!int.TryParse(request.AcademicYearTerm.Substring(termStartIndex + 1, request.AcademicYearTerm.Length - (termStartIndex + 1)), out int term))
                {
                    return Error(StudentAPIException.InvalidTermFormat(request.AcademicYearTerm));
                }

                var academicLevel = _db.AcademicLevels.AsNoTracking()
                                                      .FirstOrDefault(x => x.Code == "bachelors_degree");

                if (academicLevel is null)
                {
                    return Error(StudentAPIException.AcademicLevelNotFound("bachelors_degree"));
                }

                var academicTerm = _db.Terms.AsNoTracking()
                                            .FirstOrDefault(x => x.AcademicLevelId == academicLevel.Id
                                                                 && x.AcademicYear == year
                                                                 && x.AcademicTerm == term);

                if (academicTerm is null)
                {
                    return Error(StudentAPIException.TermNotFound(year, term));
                }

                if (string.IsNullOrEmpty(request.TitleNameEn))
                {
                    return BadRequest(ApiException.InvalidParameter("'title' is required"));
                }
                var title = _db.Titles.AsNoTracking()
                                      .FirstOrDefault(x => x.NameEn.ToUpper() == request.TitleNameEn.ToUpper());
                if (title is null)
                {
                    return Error(StudentAPIException.TitleNotFound(request.TitleNameEn));
                }

                if (string.IsNullOrEmpty(request.Gender))
                {
                    return BadRequest(ApiException.InvalidParameter("'gender' is required"));
                }
                var gender = 0;
                switch (request.Gender.ToUpper())
                {
                    case "MALE":
                        gender = 1;
                        break;
                    case "FEMALE":
                        gender = 2;
                        break;
                    default:
                        return Error(StudentAPIException.GenderNotFound(request.Gender));
                }

                Nationality nationality = null;

                if (!string.IsNullOrEmpty(request.NationalityNameEn))
                {
                    nationality = _db.Nationalities.AsNoTracking()
                                                   .FirstOrDefault(x => x.NameEn.ToUpper() == request.NationalityNameEn.ToUpper());

                    if (nationality is null)
                    {
                        return Error(StudentAPIException.NationalityNotFound(request.NationalityNameEn));
                    }
                }

                Race race = null;

                if (!string.IsNullOrEmpty(request.RaceNameEn))
                {
                    race = _db.Races.AsNoTracking()
                                    .FirstOrDefault(x => x.NameEn.ToUpper() == request.RaceNameEn.ToUpper());

                    if (race is null)
                    {
                        return Error(StudentAPIException.RaceNotFound(request.RaceNameEn));
                    }
                }

                if (string.IsNullOrEmpty(request.MaritalStatus))
                {
                    return BadRequest(ApiException.InvalidParameter("'marital_status' is required"));
                }
                var maritalStatus = string.Empty;
                switch (request.MaritalStatus.ToUpper())
                {
                    case "MARRIED":
                        maritalStatus = "m";
                        break;
                    case "SINGLE":
                        maritalStatus = "s";
                        break;
                    default:
                        return Error(StudentAPIException.MaritalStatusGroupNotFound(request.MaritalStatus));
                }

                Country birthCountry = null;

                if (!string.IsNullOrEmpty(request.BirthCountryNameEn))
                {
                    birthCountry = _db.Countries.AsNoTracking()
                                                .FirstOrDefault(x => x.NameEn.ToUpper() == request.BirthCountryNameEn.ToUpper());

                    if (birthCountry is null)
                    {
                        return Error(StudentAPIException.CountryNotFound(request.BirthCountryNameEn));
                    }
                }

                var registrationStatus = _db.RegistrationStatuses.AsNoTracking()
                                                                 .FirstOrDefault(x => x.NameEn.ToUpper() == "NOT SPECIFIED");

                if (registrationStatus is null)
                {
                    return Error(StudentAPIException.RegistrationStatusNotFound("NOT SPECIFIED"));
                }

                var student = new Student
                {
                    Code = request.Code,
                    StudentFeeTypeId = studentFeeType.Id,
                    StudentFeeGroupId = studentFeeGroup?.Id,
                    ResidentTypeId = residentType.Id,
                    StudentStatus = "s",
                    TitleId = title.Id,
                    FirstNameEn = request.FirstNameEn,
                    MidNameEn = request.MidNameEn,
                    LastNameEn = request.LastNameEn,
                    FirstNameTh = request.FirstNameTh,
                    MidNameTh = request.MidNameTh,
                    LastNameTh = request.LastNameTh,
                    Gender = gender,
                    RaceId = race?.Id,
                    NationalityId = nationality?.Id,
                    BirthDate = request.BirthDate,
                    BirthCountryId = birthCountry?.Id,
                    CitizenNumber = request.CitizenNumber,
                    Passport = request.Passport,
                    Email = request.Email,
                    PersonalEmail = request.PersonalEmail,
                    PersonalEmail2 = request.PersonalEmail2,
                    TelephoneNumber1 = request.TelephoneNumber1,
                    TelephoneNumber2 = request.TelephoneNumber2,
                    RegistrationStatusId = registrationStatus.Id,
                    MaritalStatus = maritalStatus,
                    LivingStatus = "a",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "StudentAPI>Create",
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "StudentAPI>Create"
                };

                var addresses = new List<StudentAddress>();

                if (string.IsNullOrEmpty(request.CurrentAddressCountryNameEn))
                {
                    return BadRequest(ApiException.InvalidParameter("'mailing_country' is required"));
                }
                var currentCountry = _db.Countries.AsNoTracking()
                                                  .FirstOrDefault(x => x.NameEn.ToUpper() == request.CurrentAddressCountryNameEn.ToUpper());
                if (currentCountry is null)
                {
                    return Error(StudentAPIException.CountryNotFound(request.CurrentAddressCountryNameEn));
                }

                Province currentProvince = null;

                if (!string.IsNullOrEmpty(request.CurrentAddressProvinceNameEn))
                {
                    currentProvince = _db.Provinces.AsNoTracking()
                                                   .FirstOrDefault(x => x.NameEn.ToUpper() == request.CurrentAddressProvinceNameEn.ToUpper());

                    if (currentProvince is null)
                    {
                        return Error(StudentAPIException.ProvinceNotFound(request.CurrentAddressProvinceNameEn));
                    }
                }

                var currentAddress = new StudentAddress
                {
                    Student = student,
                    Type = "c",
                    AddressEn1 = request.CurrentAddressEn1,
                    CountryId = currentCountry.Id,
                    RoadEn = request.CurrentAddressRoadEn,
                    ProvinceId = currentProvince?.Id,
                    ZipCode = request.CurrentAddressZipCode,
                    TelephoneNumber = request.CurrentAddressTelephoneNumber,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "StudentAPI>Create",
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "StudentAPI>Create"
                };

                addresses.Add(currentAddress);

                if (string.IsNullOrEmpty(request.PermanentAddressCountryNameEn))
                {
                    return BadRequest(ApiException.InvalidParameter("'current_country' is required"));
                }
                var permanentCountry = _db.Countries.AsNoTracking()
                                                    .FirstOrDefault(x => x.NameEn.ToUpper() == request.PermanentAddressCountryNameEn.ToUpper());
                if (permanentCountry is null)
                {
                    return Error(StudentAPIException.CountryNotFound(request.PermanentAddressCountryNameEn));
                }

                Province permanentProvince = null;

                if (!string.IsNullOrEmpty(request.PermanentAddressProvinceNameEn))
                {
                    permanentProvince = _db.Provinces.AsNoTracking()
                                                     .FirstOrDefault(x => x.NameEn.ToUpper() == request.PermanentAddressProvinceNameEn.ToUpper());

                    if (permanentProvince is null)
                    {
                        return Error(StudentAPIException.ProvinceNotFound(request.PermanentAddressProvinceNameEn));
                    }
                }

                District permanentDistrict = null;

                if (!string.IsNullOrEmpty(request.PermanentAddressDistrictNameEn))
                {
                    permanentDistrict = _db.Districts.AsNoTracking()
                                                     .FirstOrDefault(x => x.NameEn.ToUpper() == request.PermanentAddressDistrictNameEn.ToUpper());

                    if (permanentDistrict is null)
                    {
                        return Error(StudentAPIException.DistrictNotFound(request.PermanentAddressDistrictNameEn));
                    }
                }

                Subdistrict permanentSubdistrict = null;

                if (!string.IsNullOrEmpty(request.PermanentAddressSubdistrictNameEn))
                {
                    permanentSubdistrict = _db.Subdistricts.AsNoTracking()
                                                           .FirstOrDefault(x => x.NameEn.ToUpper() == request.PermanentAddressSubdistrictNameEn.ToUpper());

                    if (permanentSubdistrict is null)
                    {
                        return Error(StudentAPIException.SubdistrictNotFound(request.PermanentAddressSubdistrictNameEn));
                    }
                }

                if (string.IsNullOrEmpty(request.PermanentAddressZipCode))
                {
                    return BadRequest(ApiException.InvalidParameter("'current_zipcode' is required"));
                }

                var permanentAddress = new StudentAddress
                {
                    Student = student,
                    Type = "p",
                    AddressEn1 = request.PermanentAddressEn1,
                    AddressEn2 = request.PermanentAddressEn2,
                    RoadEn = request.PermanentAddressRoadEn,
                    CountryId = permanentCountry.Id,
                    ProvinceId = permanentProvince?.Id,
                    DistrictId = permanentDistrict?.Id,
                    SubdistrictId = permanentSubdistrict?.Id,
                    ZipCode = request.PermanentAddressZipCode,
                    TelephoneNumber = request.PermanentAddressTelephoneNumber,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "StudentAPI>Create",
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "StudentAPI>Create"
                };

                addresses.Add(permanentAddress);

                Country parentCountry = null;

                if (!string.IsNullOrEmpty(request.ParentAddressCountryNameEn))
                {
                    parentCountry = _db.Countries.AsNoTracking()
                                                 .FirstOrDefault(x => x.NameEn.ToUpper() == request.ParentAddressCountryNameEn.ToUpper());

                    if (parentCountry is null)
                    {
                        return Error(StudentAPIException.CountryNotFound(request.ParentAddressCountryNameEn));
                    }
                }

                if (string.IsNullOrEmpty(request.RelationshipNameEn))
                {
                    return BadRequest(ApiException.InvalidParameter("'emergency_relationship' is required"));
                }
                var relationship = _db.Relationships.AsNoTracking()
                                                    .FirstOrDefault(x => x.NameEn.ToUpper() == request.RelationshipNameEn.ToUpper());

                if (relationship is null)
                {
                    return Error(StudentAPIException.RelationshipNotFound(request.RelationshipNameEn));
                }

                Province parentProvince = null;

                if (!string.IsNullOrEmpty(request.ParentAddressProvinceNameEn))
                {
                    parentProvince = _db.Provinces.AsNoTracking()
                                                     .FirstOrDefault(x => x.NameEn.ToUpper() == request.ParentAddressProvinceNameEn.ToUpper());

                    if (parentProvince is null)
                    {
                        return Error(StudentAPIException.ProvinceNotFound(request.ParentAddressProvinceNameEn));
                    }
                }

                if (string.IsNullOrEmpty(request.ParentTelephoneNumber1))
                {
                    return BadRequest(ApiException.InvalidParameter("'emergency_contact' is required"));
                }

                var parent = new ParentInformation
                {
                    Student = student,
                    FirstNameEn = request.ParentFirstNameEn,
                    LastNameEn = request.ParentLastNameEn,
                    CountryId = parentCountry?.Id,
                    TelephoneNumber1 = request.ParentTelephoneNumber1,
                    RelationshipId = relationship.Id,
                    AddressEn = request.ParentAddressEn,
                    ProvinceId = parentProvince?.Id,
                    ZipCode = request.ParentAddressZipCode,
                    Email = request.ParentEmail,
                    LivingStatus = "a",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "StudentAPI>Create",
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "StudentAPI>Create"
                };

                var curriculumVersions = (from curriculum in _db.Curriculums
                                          join curriculumVersion in _db.CurriculumVersions on curriculum.Id equals curriculumVersion.CurriculumId
                                          join implementedTerm in _db.Terms on curriculumVersion.ImplementedTermId equals implementedTerm.Id
                                          where curriculumVersion.Code == request.CurriculumVersionCode
                                          orderby implementedTerm.AcademicYear descending, implementedTerm.AcademicTerm descending
                                          select new
                                          {
                                              curriculumVersion.Id,
                                              curriculum.FacultyId,
                                              curriculum.DepartmentId
                                          })
                                         .FirstOrDefault();

                if (curriculumVersions is null)
                {
                    return Error(StudentAPIException.CurriculumVersionNotFound(request.CurriculumVersionCode));
                }

                var academicProgram = _db.AcademicPrograms.AsNoTracking()
                                                          .FirstOrDefault(x => x.NameEn.ToUpper() == "UNDERGRADUATE");

                if (academicProgram is null)
                {
                    return Error(StudentAPIException.AcademicProgramNotFound("UNDERGRADUATE"));
                }

                var regisTerm = _db.Terms.AsNoTracking()
                                         .FirstOrDefault(x => x.AcademicLevelId == academicLevel.Id
                                             && x.IsRegistration);
                var minCredit = 0;
                var maxCredit = 0;
                if (regisTerm != null)
                {
                    minCredit = regisTerm.MinimumCredit;
                    maxCredit = regisTerm.MaximumCredit;
                }

                var academicInfo = new AcademicInformation
                {
                    Student = student,
                    Batch = batch,
                    CurriculumVersionId = curriculumVersions.Id,
                    AcademicLevelId = academicLevel.Id,
                    AcademicProgramId = academicProgram.Id,
                    StudentGroupId = studentGroup.Id,
                    FacultyId = curriculumVersions.FacultyId,
                    DepartmentId = curriculumVersions.DepartmentId,
                    MinimumCredit = minCredit,
                    MaximumCredit = maxCredit,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "StudentAPI>Create",
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "StudentAPI>Create"
                };

                var admissionInfo = new AdmissionInformation
                {
                    Student = student,
                    AdmissionTypeId = admissionType?.Id,
                    AdmissionTermId = academicTerm.Id,
                    AcademicLevelId = academicLevel.Id,
                    FacultyId = curriculumVersions.FacultyId,
                    DepartmentId = curriculumVersions.DepartmentId,
                    CurriculumVersionId = curriculumVersions.Id,
                    CheckReferenceNumber = request.CheckReferenceNumber,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "StudentAPI>Create",
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "StudentAPI>Create"
                };

                var curriculumInfo = new CurriculumInformation
                {
                    Student = student,
                    FacultyId = curriculumVersions.FacultyId,
                    DepartmentId = curriculumVersions.DepartmentId,
                    CurriculumVersionId = curriculumVersions.Id,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "StudentAPI>Create",
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "StudentAPI>Create"
                };

                var specializationInfos = new List<SpecializationGroupInformation>();
                if (!string.IsNullOrEmpty(request.SpecializationGroupCode1))
                {
                    var englishSpecialization = _db.SpecializationGroups.AsNoTracking()
                                                                        .FirstOrDefault(x => x.Code == request.SpecializationGroupCode1);

                    if (englishSpecialization != null)
                    {
                        var group = new SpecializationGroupInformation
                        {
                            SpecializationGroupId = englishSpecialization.Id,
                            CurriculumInformation = curriculumInfo,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = "StudentAPI>Create",
                            UpdatedAt = DateTime.UtcNow,
                            UpdatedBy = "StudentAPI>Create"
                        };

                        specializationInfos.Add(group);
                    }
                }

                if (!string.IsNullOrEmpty(request.SpecializationGroupCode2))
                {
                    var mathSpecialization = _db.SpecializationGroups.AsNoTracking()
                                                                 .FirstOrDefault(x => x.Code == request.SpecializationGroupCode2);

                    if (mathSpecialization != null)
                    {
                        var group = new SpecializationGroupInformation
                        {
                            SpecializationGroupId = mathSpecialization.Id,
                            CurriculumInformation = curriculumInfo,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = "StudentAPI>Create",
                            UpdatedAt = DateTime.UtcNow,
                            UpdatedBy = "StudentAPI>Create"
                        };

                        specializationInfos.Add(group);
                    }
                }

                try
                {
                    using (var transaction = _db.Database.BeginTransaction())
                    {
                        _db.Students.Add(student);

                        _db.StudentAddresses.AddRange(addresses);

                        _db.AcademicInformations.Add(academicInfo);

                        _db.AdmissionInformations.Add(admissionInfo);

                        _db.CurriculumInformations.Add(curriculumInfo);

                        _db.SpecializationGroupInformations.AddRange(specializationInfos);

                        _db.ParentInformations.Add(parent);

                        transaction.Commit();
                    }

                    _db.SaveChangesNoAutoUserIdAndTimestamps();
                }
                catch (Exception e)
                {
                    throw e;
                }

                return Success(1);
            }
            catch (Exception)
            {
                return Error(new ResultException
                {
                    Code = $"{ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, "API")}000",
                    Message = "Exception"
                });
            }
        }

        [AllowAnonymous]
        [HttpPatch]
        [Route("StudentAPI/edit/{studentCode}")]
        public async Task<ActionResult> PatchStudentsAsync(string studentCode, [FromBody] UpdateStudentAPIViewModel request)
        {
            if (ValidApiKey())
            {
                return Unauthorized(ApiException.InvalidKey());
            }
            try
            {
                if (request == null || string.IsNullOrEmpty(studentCode))
                {
                    return BadRequest(ApiException.InvalidParameter());
                }

                var student = _db.Students.IgnoreQueryFilters()
                                          .FirstOrDefault(x => x.Code == studentCode);
                if (student == null)
                {
                    return NotFound();
                }
                if (!student.IsActive)
                {
                    return Error(StudentAPIException.StudentInActive());
                }

                student.UpdatedAt = DateTime.UtcNow;
                student.UpdatedBy = string.IsNullOrEmpty(request.UpdatedBy) ? "StudentAPI" : request.UpdatedBy;

                if (!string.IsNullOrEmpty(request.Code))
                {
                    var studentCodeExist = _db.Students.IgnoreQueryFilters()
                                                       .FirstOrDefault(x => x.Code == request.Code);
                    if (studentCodeExist == null)
                    {
                        student.Code = request.Code;
                    }
                    else
                    {
                        if (studentCodeExist.Id != student.Id)
                        {
                            return Error(StudentAPIException.DuplicateStudentCode(request.Code));
                        }
                    }
                }

                if (!string.IsNullOrEmpty(request.Title))
                {
                    var title = _db.Titles.AsNoTracking()
                                      .FirstOrDefault(x => x.NameEn.ToUpper() == request.Title.ToUpper());

                    if (title is null)
                    {
                        return Error(StudentAPIException.TitleNotFound(request.Title));
                    }
                    student.TitleId = title.Id;
                }

                if (request.FNameTH != null)
                {
                    if (request.FNameTH.Length > 100)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " fname_th max length = 100");
                    }
                    student.FirstNameTh = request.FNameTH;
                }
                if (request.MNameTH != null)
                {
                    if (request.MNameTH.Length > 100)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " mname_th max length = 100");
                    }
                    student.MidNameTh = request.MNameTH;
                }
                if (request.LNameTH != null)
                {
                    if (request.LNameTH.Length > 100)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " lname_th max length = 100");
                    }
                    student.LastNameTh = request.LNameTH;
                }

                if (!string.IsNullOrEmpty(request.FNameEN))
                {
                    if (request.FNameEN.Length > 100)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " fname_en max length = 100");
                    }
                    student.FirstNameEn = request.FNameEN;
                }
                if (request.MNameEN != null)
                {
                    if (request.MNameEN.Length > 100)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " mname_en max length = 100");
                    }
                    student.MidNameEn = request.MNameEN;
                }
                if (!string.IsNullOrEmpty(request.LNameEN))
                {
                    if (request.LNameEN.Length > 100)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " lname_en max length = 100");
                    }
                    student.LastNameEn = request.LNameEN;
                }

                if (request.Gender != null)
                {
                    var gender = 0;

                    if (!string.IsNullOrEmpty(request.Gender))
                    {
                        switch (request.Gender.ToUpper())
                        {
                            case "MALE":
                                gender = 1;
                                break;
                            case "FEMALE":
                                gender = 2;
                                break;
                            default:
                                return Error(StudentAPIException.GenderNotFound(request.Gender));
                        }
                    }

                    student.Gender = gender;
                }

                if (!string.IsNullOrEmpty(request.DOB))
                {
                    CultureInfo enUS = new CultureInfo("en-US");
                    if (!DateTime.TryParseExact(request.DOB, "dd/MM/yyyy", enUS, DateTimeStyles.None, out DateTime dob))
                    {
                        return Error(StudentAPIException.DateWrongFormat(request.DOB));
                    }
                    student.BirthDate = dob;
                }

                if (request.Nation != null)
                {
                    if (!string.IsNullOrEmpty(request.Nation))
                    {
                        Nationality nationality = _db.Nationalities.AsNoTracking()
                                                       .FirstOrDefault(x => x.NameEn.ToUpper() == request.Nation.ToUpper());
                        if (nationality is null)
                        {
                            return Error(StudentAPIException.NationalityNotFound(request.Nation));
                        }
                        student.NationalityId = nationality.Id;
                    }
                    else
                    {
                        student.NationalityId = null;
                    }
                }

                if (request.Race != null)
                {
                    if (!string.IsNullOrEmpty(request.Race))
                    {
                        var race = _db.Races.AsNoTracking()
                                    .FirstOrDefault(x => x.NameEn.ToUpper() == request.Race.ToUpper());

                        if (race is null)
                        {
                            return Error(StudentAPIException.RaceNotFound(request.Race));
                        }
                        student.RaceId = race.Id;
                    }
                    else
                    {
                        student.RaceId = null;
                    }
                }

                if (request.ThaiIDNo != null)
                {
                    if (request.ThaiIDNo.Length > 20)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " thai_id_no max length = 20");
                    }
                    student.CitizenNumber = request.ThaiIDNo;
                }

                if (request.PassportNo != null)
                {
                    if (request.PassportNo.Length > 20)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " passport_no max length = 20");
                    }
                    student.Passport = request.PassportNo;
                }
                if (request.PassportIssueDate != null)
                {
                    if (!string.IsNullOrEmpty(request.PassportIssueDate))
                    {
                        CultureInfo enUS = new CultureInfo("en-US");
                        if (!DateTime.TryParseExact(request.PassportIssueDate, "dd/MM/yyyy", enUS, DateTimeStyles.None, out DateTime date))
                        {
                            return Error(StudentAPIException.DateWrongFormat(request.PassportIssueDate));
                        }
                        student.PassportIssuedAt = date;
                    }
                    else
                    {
                        student.PassportIssuedAt = null;
                    }
                }
                if (request.PassportExpiryDate != null)
                {
                    if (!string.IsNullOrEmpty(request.PassportExpiryDate))
                    {
                        CultureInfo enUS = new CultureInfo("en-US");
                        if (!DateTime.TryParseExact(request.PassportExpiryDate, "dd/MM/yyyy", enUS, DateTimeStyles.None, out DateTime date))
                        {
                            return Error(StudentAPIException.DateWrongFormat(request.PassportExpiryDate));
                        }
                        student.PassportExpiredAt = date;
                    }
                    else
                    {
                        student.PassportExpiredAt = null;
                    }
                }

                if (request.BirthCountry != null)
                {
                    if (!string.IsNullOrEmpty(request.BirthCountry))
                    {
                        var currentCountry = _db.Countries.AsNoTracking()
                                                  .FirstOrDefault(x => x.NameEn.ToUpper() == request.BirthCountry.ToUpper());

                        if (currentCountry is null)
                        {
                            return Error(StudentAPIException.CountryNotFound(request.BirthCountry));
                        }

                        student.BirthCountryId = currentCountry.Id;
                    }
                    else
                    {
                        student.BirthCountryId = null;
                    }
                }
                if (request.BirthProvince != null)
                {
                    if (!string.IsNullOrEmpty(request.BirthProvince))
                    {
                        var provinces = _db.Provinces.AsNoTracking()
                                                     .FirstOrDefault(x => x.NameEn.ToUpper() == request.BirthProvince.ToUpper() && x.CountryId == student.BirthCountryId);

                        if (provinces is null)
                        {
                            return Error(StudentAPIException.ProvinceNotFound(request.BirthProvince));
                        }

                        student.BirthProvinceId = provinces.Id;
                    }
                    else
                    {
                        student.BirthProvinceId = null;
                    }
                }
                if (request.BirthState != null)
                {
                    if (!string.IsNullOrEmpty(request.BirthState))
                    {
                        var states = _db.States.AsNoTracking()
                                               .FirstOrDefault(x => x.NameEn.ToUpper() == request.BirthState.ToUpper() && x.CountryId == student.BirthCountryId);

                        if (states is null)
                        {
                            return Error(StudentAPIException.StateNotFound(request.BirthState));
                        }

                        student.BirthStateId = states.Id;
                    }
                    else
                    {
                        student.BirthStateId = null;
                    }
                }
                if (request.BirthCity != null)
                {
                    if (!string.IsNullOrEmpty(request.BirthCity))
                    {
                        var city = _db.Cities.AsNoTracking()
                                             .FirstOrDefault(x => x.NameEn.ToUpper() == request.BirthCity.ToUpper() && (x.CountryId == student.BirthCountryId || x.StateId == student.BirthStateId));

                        if (city is null)
                        {
                            return Error(StudentAPIException.CityNotFound(request.BirthCity));
                        }

                        student.BirthCityId = city.Id;
                    }
                    else
                    {
                        student.BirthCityId = null;
                    }
                }

                if (request.Religion != null)
                {
                    if (!string.IsNullOrEmpty(request.Religion))
                    {
                        var religion = _db.Religions.AsNoTracking()
                                                    .FirstOrDefault(x => x.NameEn.ToUpper() == request.Religion.ToUpper());

                        if (religion is null)
                        {
                            return Error(StudentAPIException.ReligionNotFound(request.Religion));
                        }

                        student.ReligionId = religion.Id;
                    }
                    else
                    {
                        student.ReligionId = null;
                    }
                }

                if (request.MaritalStatus != null)
                {
                    var maritalStatus = string.Empty;
                    if (!string.IsNullOrEmpty(request.MaritalStatus))
                    {
                        switch (request.MaritalStatus.ToUpper())
                        {
                            case "MARRIED":
                                maritalStatus = "m";
                                break;
                            case "SINGLE":
                                maritalStatus = "s";
                                break;
                            default:
                                return Error(StudentAPIException.MaritalStatusGroupNotFound(request.MaritalStatus));
                        }
                    }
                    student.MaritalStatus = maritalStatus;
                }

                if (request.NativeLang != null)
                {
                    if (!string.IsNullOrEmpty(request.NativeLang))
                    {
                        var languages = _db.Languages.AsNoTracking()
                                                     .FirstOrDefault(x => x.NameEn.ToUpper() == request.NativeLang.ToUpper());
                        if (languages is null)
                        {
                            return Error(StudentAPIException.NativeLanguageNotFound(request.NativeLang));
                        }
                        else
                        {
                            student.NativeLanguage = languages.Id + "";
                        }
                    }
                    else
                    {
                        student.NativeLanguage = null;
                    }
                }

                if (request.BankAccNo != null)
                {
                    if (request.BankAccNo.Length > 20)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " bank_acc_no max length = 20");
                    }
                    student.BankAccount = request.BankAccNo;
                }

                if (request.Email1 != null)
                {
                    if (request.Email1.Length > 320)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " email1 max length = 320");
                    }
                    student.Email = request.Email1;
                }
                if (request.Email2 != null)
                {
                    if (request.Email2.Length > 50)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " email2 max length = 50");
                    }
                    student.Email2 = request.Email2;
                }
                if (request.PersonelEmail != null)
                {
                    if (request.PersonelEmail.Length > 50)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " personel_email max length = 50");
                    }
                    student.PersonalEmail = request.PersonelEmail;
                }
                if (request.Facebook != null)
                {
                    if (request.Facebook.Length > 200)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " facebook max length = 200");
                    }
                    student.Facebook = request.Facebook;
                }
                if (request.Line != null)
                {
                    if (request.Line.Length > 200)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " line max length = 200");
                    }
                    student.Line = request.Line;
                }
                if (request.Tel1 != null)
                {
                    if (request.Tel1.Length > 20)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " tel1 max length = 20");
                    }
                    student.TelephoneNumber1 = request.Tel1;
                }
                if (request.Tel2 != null)
                {
                    if (request.Tel2.Length > 20)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " tel2 max length = 20");
                    }
                    student.TelephoneNumber2 = request.Tel2;
                }
                if (request.Other != null)
                {
                    if (request.Other.Length > 200)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " other max length = 200");
                    }
                    student.OtherContact = request.Other;
                }

                if (!string.IsNullOrEmpty(request.StudentGroup))
                {
                    var studentGroup = _db.StudentGroups.AsNoTracking()
                                                        .FirstOrDefault(x => x.Code.ToUpper() == request.StudentGroup.ToUpper());
                    if (studentGroup is null)
                    {
                        return Error(StudentAPIException.StudentGroupNotFound(request.StudentGroup));
                    }
                    var studentAcademicInformation = _db.AcademicInformations.FirstOrDefault(x => x.StudentId == student.Id);
                    if (studentAcademicInformation == null)
                    {
                        return NotFound("Academic Information Not found");
                    }
                    studentAcademicInformation.StudentGroupId = studentGroup.Id;
                    studentAcademicInformation.UpdatedAt = DateTime.UtcNow;
                    studentAcademicInformation.UpdatedBy = student.UpdatedBy;
                }

                if (!string.IsNullOrEmpty(request.AdmissionType) 
                        || (request.CheckReferenceNumber != null))
                {
                    var studentAdmissionInformation = _db.AdmissionInformations.FirstOrDefault(x => x.StudentId == student.Id);
                    if (studentAdmissionInformation == null)
                    {
                        return NotFound("Admission Information Not found");
                    }

                    if (!string.IsNullOrEmpty(request.AdmissionType))
                    {
                        var admissionType = _db.AdmissionTypes.AsNoTracking()
                                                              .FirstOrDefault(x => x.NameEn.ToUpper() == request.AdmissionType.ToUpper());
                        if (admissionType is null)
                        {
                            return Error(StudentAPIException.AdmissionTypeNotFound(request.AdmissionType));
                        }

                        studentAdmissionInformation.AdmissionTypeId = admissionType.Id;
                        studentAdmissionInformation.UpdatedAt = DateTime.UtcNow;
                        studentAdmissionInformation.UpdatedBy = student.UpdatedBy;
                    }
                    if (request.CheckReferenceNumber != null)
                    {
                        if (request.CheckReferenceNumber.Length > 200)
                        {
                            return BadRequest(ApiException.InvalidParameter() + " check reference number max length = 200");
                        }

                        studentAdmissionInformation.CheckReferenceNumber = request.CheckReferenceNumber;
                        studentAdmissionInformation.UpdatedAt = DateTime.UtcNow;
                        studentAdmissionInformation.UpdatedBy = student.UpdatedBy;
                    }
                }

                if (!string.IsNullOrEmpty(request.ResidentType))
                {
                    var residentType = _db.ResidentTypes.AsNoTracking()
                                                         .FirstOrDefault(x => x.NameEn.ToUpper() == request.ResidentType.ToUpper());
                    if (residentType is null)
                    {
                        return Error(StudentAPIException.ResidentTypeNotFound(request.ResidentType));
                    }
                    student.ResidentTypeId = residentType.Id;
                }

                if (!string.IsNullOrEmpty(request.StudentFeeType))
                {
                    var studentFeeType = _db.StudentFeeTypes.AsNoTracking()
                                                            .FirstOrDefault(x => x.NameEn.ToUpper() == request.StudentFeeType.ToUpper());
                    if (studentFeeType is null)
                    {
                        return Error(StudentAPIException.StudentFeeTypeNotFound(request.StudentFeeType));
                    }
                    student.StudentFeeTypeId = studentFeeType.Id;
                }

                if (!string.IsNullOrEmpty(request.StudentFeeGroup))
                {
                    var studentFeeGroup = _db.StudentFeeGroups.AsNoTracking()
                                                              .FirstOrDefault(x => x.Name.ToUpper() == request.StudentFeeGroup.ToUpper());
                    if (studentFeeGroup is null)
                    {
                        return Error(StudentAPIException.StudentFeeGroupNotFound(request.StudentFeeGroup));
                    }
                    student.StudentFeeGroupId = studentFeeGroup.Id;
                }

                _db.SaveChangesNoAutoUserIdAndTimestamps();

                await LogApiInStudentLog(request.UpdatedBy, student.Id, "StudentAPI > PatchStudent");

                return Success(1);
            }
            catch (Exception e)
            {
                return Error(new ResultException
                {
                    Code = $"{ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, "API")}000",
                    Message = "Exception " + e.Message
                });
            }
        }

        private async Task LogApiInStudentLog(string userName, Guid studentId, string source)
        {
            try
            {
                string rawContent = string.Empty;
                using (var reader = new StreamReader(Request.Body,
                              encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false))
                {
                    Request.Body.Position = 0;
                    rawContent = await reader.ReadToEndAsync();
                }

                var term = _db.Terms.FirstOrDefault(x => x.IsCurrent);
                StudentLog log = new StudentLog
                {
                    StudentId = studentId,
                    TermId = term?.Id ?? 0,
                    Source = source,
                    Log = rawContent,
                    CreatedBy = userName,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = null,  //somehow this kind set foreign key for this thing
                    UpdatedAt = DateTime.UtcNow
                };
                _db.StudentLogs.Add(log);
                _db.SaveChangesNoAutoUserIdAndTimestamps();
            }
            catch (Exception)
            {
            }
        }

        [AllowAnonymous]
        [HttpPatch]
        [Route("StudentAPI/edit_address/{studentCode}")]
        public async Task<ActionResult> PatchStudentAddressAsync(string studentCode, [FromBody] UpdateStudentAddressViewModel request)
        {
            if (ValidApiKey())
            {
                return Unauthorized(ApiException.InvalidKey());
            }
            try
            {
                if (request == null || string.IsNullOrEmpty(studentCode))
                {
                    return BadRequest(ApiException.InvalidParameter());
                }

                var student = _db.Students.IgnoreQueryFilters()
                                          .FirstOrDefault(x => x.Code == studentCode);
                if (student == null)
                {
                    return NotFound();
                }
                if (!student.IsActive)
                {
                    return Error(StudentAPIException.StudentInActive());
                }

                if (string.IsNullOrEmpty(request.Type) || (request.Type.ToLower() != "c" && request.Type.ToLower() != "p"))
                {
                    return Error(StudentAPIException.InvalidAddressType(request.Type));
                }

                var updateTime = DateTime.UtcNow;
                var userName = string.IsNullOrEmpty(request.UpdatedBy) ? "StudentAPI" : request.UpdatedBy;

                var studentAddress = _db.StudentAddresses.IgnoreQueryFilters()
                                                         .FirstOrDefault(x => x.StudentId == student.Id
                                                                                  && x.Type == request.Type.ToLower()
                                                                        );
                if (studentAddress == null)
                {
                    studentAddress = new StudentAddress();
                    studentAddress.StudentId = student.Id;
                    studentAddress.CreatedAt = updateTime;
                    studentAddress.CreatedBy = userName;

                    if (string.IsNullOrEmpty(request.Zip))
                    {
                        return Error(ApiException.InvalidParameter());
                    }

                    _db.StudentAddresses.Add(studentAddress);
                }
                studentAddress.UpdatedAt = updateTime;
                studentAddress.UpdatedBy = userName;

                if (request.Address1Th != null)
                {
                    if (request.Address1Th.Length > 500)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " address1_th max length = 500");
                    }
                    studentAddress.AddressTh1 = request.Address1Th;
                }
                if (request.Address2Th != null)
                {
                    if (request.Address2Th.Length > 500)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " address2_th max length = 500");
                    }
                    studentAddress.AddressTh2 = request.Address2Th;
                }
                if (request.Address1En != null)
                {
                    if (request.Address1En.Length > 500)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " address1_en max length = 500");
                    }
                    studentAddress.AddressEn1 = request.Address1En;
                }
                if (request.Address2En != null)
                {
                    if (request.Address2En.Length > 500)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " address2_en max length = 500");
                    }
                    studentAddress.AddressEn2 = request.Address2En;
                }
                if (request.HouseNo != null)
                {
                    if (request.HouseNo.Length > 50)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " house_no max length = 50");
                    }
                    studentAddress.HouseNumber = request.HouseNo;
                }
                if (request.Moo != null)
                {
                    if (request.Moo.Length > 50)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " moo max length = 50");
                    }
                    studentAddress.Moo = request.Moo;
                }
                if (request.SoiTh != null)
                {
                    if (request.SoiTh.Length > 200)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " soi_th max length = 200");
                    }
                    studentAddress.SoiTh = request.SoiTh;
                }
                if (request.SoiEn != null)
                {
                    if (request.SoiEn.Length > 200)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " soi_en max length = 200");
                    }
                    studentAddress.SoiEn = request.SoiEn;
                }
                if (request.RoadTh != null)
                {
                    if (request.RoadTh.Length > 200)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " road_th max length = 200");
                    }
                    studentAddress.RoadTh = request.RoadTh;
                }
                if (request.RoadEn != null)
                {
                    if (request.RoadEn.Length > 200)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " road_en max length = 200");
                    }
                    studentAddress.RoadEn = request.RoadEn;
                }

                if (request.Country != null)
                {
                    var currentCountry = _db.Countries.AsNoTracking()
                                                .FirstOrDefault(x => x.NameEn.ToUpper() == request.Country.ToUpper());

                    if (currentCountry is null)
                    {
                        return Error(StudentAPIException.CountryNotFound(request.Country));
                    }

                    studentAddress.CountryId = currentCountry.Id;
                }
                if (request.Province != null)
                {
                    if (!string.IsNullOrEmpty(request.Province))
                    {
                        var provinces = _db.Provinces.AsNoTracking()
                                                     .FirstOrDefault(x => x.NameEn.ToUpper() == request.Province.ToUpper() && x.CountryId == studentAddress.CountryId);

                        if (provinces is null)
                        {
                            return Error(StudentAPIException.ProvinceNotFound(request.Province));
                        }

                        studentAddress.ProvinceId = provinces.Id;
                    }
                    else
                    {
                        studentAddress.ProvinceId = null;
                    }
                }
                if (request.District != null)
                {
                    if (!string.IsNullOrEmpty(request.District))
                    {
                        var district = _db.Districts.AsNoTracking()
                                                    .FirstOrDefault(x => x.NameEn.ToUpper() == request.District.ToUpper() && x.CountryId == studentAddress.CountryId && x.ProvinceId == studentAddress.ProvinceId);
                        if (district is null)
                        {
                            return Error(StudentAPIException.DistrictNotFound(request.District));
                        }

                        studentAddress.DistrictId = district.Id;
                    }
                    else
                    {
                        studentAddress.DistrictId = null;
                    }
                }
                if (request.SubDistrict != null)
                {
                    if (!string.IsNullOrEmpty(request.SubDistrict))
                    {
                        var subDistrict = _db.Subdistricts.AsNoTracking()
                                                          .FirstOrDefault(x => x.NameEn.ToUpper() == request.SubDistrict.ToUpper()
                                                                                    && x.CountryId == studentAddress.CountryId
                                                                                    && x.ProvinceId == studentAddress.ProvinceId
                                                                                    && x.DistrictId == studentAddress.DistrictId);
                        if (subDistrict is null)
                        {
                            return Error(StudentAPIException.SubdistrictNotFound(request.SubDistrict));
                        }
                        studentAddress.SubdistrictId = subDistrict.Id;
                    }
                    else
                    {
                        studentAddress.DistrictId = null;
                    }
                }
                if (request.State != null)
                {
                    if (!string.IsNullOrEmpty(request.State))
                    {
                        var states = _db.States.AsNoTracking()
                                               .FirstOrDefault(x => x.NameEn.ToUpper() == request.State.ToUpper() && x.CountryId == studentAddress.CountryId);
                        if (states is null)
                        {
                            return Error(StudentAPIException.StateNotFound(request.State));
                        }

                        studentAddress.StateId = states.Id;
                    }
                    else
                    {
                        studentAddress.StateId = null;
                    }
                }
                if (request.City != null)
                {
                    if (!string.IsNullOrEmpty(request.City))
                    {
                        var city = _db.Cities.AsNoTracking()
                                             .FirstOrDefault(x => x.NameEn.ToUpper() == request.City.ToUpper() && (x.CountryId == studentAddress.CountryId || x.StateId == studentAddress.StateId));

                        if (city is null)
                        {
                            return Error(StudentAPIException.CityNotFound(request.City));
                        }

                        studentAddress.CityId = city.Id;
                    }
                    else
                    {
                        studentAddress.CityId = null;
                    }
                }

                if (request.Zip != null)
                {
                    if (string.IsNullOrEmpty(request.Zip) || request.Zip.Length > 20)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " zip max length = 20");
                    }
                    studentAddress.ZipCode = request.Zip;
                }

                if (request.Tel != null)
                {
                    if (request.Tel.Length > 20)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " tel max length = 20");
                    }
                    studentAddress.TelephoneNumber = request.Tel;
                }

                _db.SaveChanges();

                await LogApiInStudentLog(userName, student.Id, "StudentAPI > PatchStudentAddress");

                return Success(1);
            }
            catch (Exception e)
            {
                return Error(new ResultException
                {
                    Code = $"{ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, "API")}000",
                    Message = "Exception " + e.Message
                });
            }
        }

        [AllowAnonymous]
        [HttpPatch]
        [Route("StudentAPI/edit_parent/{studentCode}")]
        public async Task<ActionResult> PatchStudentParentAsync(string studentCode, [FromBody] UpdateStudentParentViewModel request)
        {
            if (ValidApiKey())
            {
                return Unauthorized(ApiException.InvalidKey());
            }
            try
            {
                if (request == null || string.IsNullOrEmpty(studentCode))
                {
                    return BadRequest(ApiException.InvalidParameter());
                }

                var student = _db.Students.IgnoreQueryFilters()
                                          .FirstOrDefault(x => x.Code == studentCode);
                if (student == null)
                {
                    return NotFound();
                }
                if (!student.IsActive)
                {
                    return Error(StudentAPIException.StudentInActive());
                }

                if (string.IsNullOrEmpty(request.Relationship))
                {
                    return Error(StudentAPIException.RelationshipNotFound(request.Relationship));
                }
                var relationship = _db.Relationships.AsNoTracking()
                                                    .FirstOrDefault(x => x.NameEn.ToUpper() == request.Relationship.ToUpper());

                if (relationship is null)
                {
                    return Error(StudentAPIException.RelationshipNotFound(request.Relationship));
                }

                var updateTime = DateTime.UtcNow;
                var userName = string.IsNullOrEmpty(request.UpdatedBy) ? "StudentAPI" : request.UpdatedBy;

                var parentInformation = _db.ParentInformations.IgnoreQueryFilters()
                                                           .FirstOrDefault(x => x.StudentId == student.Id
                                                                                    && x.RelationshipId == relationship.Id
                                                                          );
                if (parentInformation == null)
                {
                    parentInformation = new ParentInformation();
                    parentInformation.StudentId = student.Id;
                    parentInformation.RelationshipId = relationship.Id;
                    parentInformation.CreatedAt = updateTime;
                    parentInformation.CreatedBy = userName;

                    if (string.IsNullOrEmpty(request.Tel))
                    {
                        return Error(ApiException.InvalidParameter());
                    }

                    _db.ParentInformations.Add(parentInformation);
                }
                parentInformation.UpdatedAt = updateTime;
                parentInformation.UpdatedBy = userName;

                if (request.FNameTh != null)
                {
                    if (request.FNameTh.Length > 200)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " fname_th max length = 200");
                    }
                    parentInformation.FirstNameTh = request.FNameTh;
                }
                if (request.MNameTh != null)
                {
                    if (request.MNameTh.Length > 200)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " mname_th max length = 200");
                    }
                    parentInformation.MidNameTh = request.MNameTh;
                }
                if (request.SNameTh != null)
                {
                    if (request.SNameTh.Length > 200)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " sname_th max length = 200");
                    }
                    parentInformation.LastNameTh = request.SNameTh;
                }

                if (!string.IsNullOrEmpty(request.FNameEn))
                {
                    if (request.FNameEn.Length > 200)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " fname_en max length = 200");
                    }
                    parentInformation.FirstNameEn = request.FNameEn;
                }
                if (request.MNameEn != null)
                {
                    if (request.MNameEn.Length > 200)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " mname_en max length = 200");
                    }
                    parentInformation.MidNameEn = request.MNameEn;
                }
                if (!string.IsNullOrEmpty(request.SNameEn))
                {
                    if (request.SNameEn.Length > 200)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " sname_en max length = 200");
                    }
                    parentInformation.LastNameEn = request.SNameEn;
                }

                if (!string.IsNullOrEmpty(request.CitizenNo))
                {
                    if (request.CitizenNo.Length > 100)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " citizen_no max length = 100");
                    }
                    parentInformation.CitizenNumber = request.CitizenNo;
                }
                if (!string.IsNullOrEmpty(request.Passport))
                {
                    if (request.Passport.Length > 100)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " passport max length = 100");
                    }
                    parentInformation.Passport = request.Passport;
                }
                if (request.Tel != null)
                {
                    if (string.IsNullOrEmpty(request.Tel) || request.Tel.Length > 20)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " tel max length = 20");
                    }
                    parentInformation.TelephoneNumber1 = request.Tel;
                }
                if (!string.IsNullOrEmpty(request.AddressTh))
                {
                    if (request.AddressTh.Length > 500)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " address_th max length = 500");
                    }
                    parentInformation.AddressTh = request.AddressTh;
                }
                if (!string.IsNullOrEmpty(request.AddressEn))
                {
                    if (request.AddressEn.Length > 500)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " address_en max length = 500");
                    }
                    parentInformation.AddressEn = request.AddressEn;
                }

                if (request.Country != null)
                {
                    var currentCountry = _db.Countries.AsNoTracking()
                                                .FirstOrDefault(x => x.NameEn.ToUpper() == request.Country.ToUpper());

                    if (currentCountry is null)
                    {
                        return Error(StudentAPIException.CountryNotFound(request.Country));
                    }

                    parentInformation.CountryId = currentCountry.Id;
                }
                if (request.Province != null)
                {
                    if (!string.IsNullOrEmpty(request.Province))
                    {
                        var provinces = _db.Provinces.AsNoTracking()
                                                     .FirstOrDefault(x => x.NameEn.ToUpper() == request.Province.ToUpper() && x.CountryId == parentInformation.CountryId);

                        if (provinces is null)
                        {
                            return Error(StudentAPIException.ProvinceNotFound(request.Province));
                        }

                        parentInformation.ProvinceId = provinces.Id;
                    }
                    else
                    {
                        parentInformation.ProvinceId = null;
                    }
                }
                if (request.District != null)
                {
                    if (!string.IsNullOrEmpty(request.District))
                    {
                        var district = _db.Districts.AsNoTracking()
                                                    .FirstOrDefault(x => x.NameEn.ToUpper() == request.District.ToUpper() && x.CountryId == parentInformation.CountryId && x.ProvinceId == parentInformation.ProvinceId);
                        if (district is null)
                        {
                            return Error(StudentAPIException.DistrictNotFound(request.District));
                        }

                        parentInformation.DistrictId = district.Id;
                    }
                    else
                    {
                        parentInformation.DistrictId = null;
                    }
                }
                if (request.Subdistrict != null)
                {
                    if (!string.IsNullOrEmpty(request.Subdistrict))
                    {
                        var subDistrict = _db.Subdistricts.AsNoTracking()
                                                          .FirstOrDefault(x => x.NameEn.ToUpper() == request.Subdistrict.ToUpper()
                                                                                    && x.CountryId == parentInformation.CountryId
                                                                                    && x.ProvinceId == parentInformation.ProvinceId
                                                                                    && x.DistrictId == parentInformation.DistrictId);
                        if (subDistrict is null)
                        {
                            return Error(StudentAPIException.SubdistrictNotFound(request.Subdistrict));
                        }
                        parentInformation.SubdistrictId = subDistrict.Id;
                    }
                    else
                    {
                        parentInformation.DistrictId = null;
                    }
                }
                if (request.State != null)
                {
                    if (!string.IsNullOrEmpty(request.State))
                    {
                        var states = _db.States.AsNoTracking()
                                               .FirstOrDefault(x => x.NameEn.ToUpper() == request.State.ToUpper() && x.CountryId == parentInformation.CountryId);
                        if (states is null)
                        {
                            return Error(StudentAPIException.StateNotFound(request.State));
                        }

                        parentInformation.StateId = states.Id;
                    }
                    else
                    {
                        parentInformation.StateId = null;
                    }
                }
                if (request.City != null)
                {
                    if (!string.IsNullOrEmpty(request.City))
                    {
                        var city = _db.Cities.AsNoTracking()
                                             .FirstOrDefault(x => x.NameEn.ToUpper() == request.City.ToUpper() && (x.CountryId == parentInformation.CountryId || x.StateId == parentInformation.StateId));

                        if (city is null)
                        {
                            return Error(StudentAPIException.CityNotFound(request.City));
                        }

                        parentInformation.CityId = city.Id;
                    }
                    else
                    {
                        parentInformation.CityId = null;
                    }
                }

                if (request.Zip != null)
                {
                    if (string.IsNullOrEmpty(request.Zip) || request.Zip.Length > 20)
                    {
                        return BadRequest(ApiException.InvalidParameter() + " zip max length = 20");
                    }
                    parentInformation.ZipCode = request.Zip;
                }

                parentInformation.IsMainParent = "YES".Equals(request.MainParent?.ToUpper() ?? "");
                parentInformation.IsEmergencyContact = "YES".Equals(request.ForEmergency?.ToUpper() ?? "");

                _db.SaveChangesNoAutoUserIdAndTimestamps();

                await LogApiInStudentLog(userName, student.Id, "StudentAPI > PatchStudentParent");

                return Success(1);
            }
            catch (Exception e)
            {
                return Error(new ResultException
                {
                    Code = $"{ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, "API")}000",
                    Message = "Exception " + e.Message
                });
            }
        }

        [AllowAnonymous]
        [HttpPatch]
        [Route("StudentAPI/edit_status/{studentCode}")]
        public async Task<ActionResult> PatchStudentStatusAsync(string studentCode, [FromBody] UpdateStudentStatusViewModel request)
        {
            if (ValidApiKey())
            {
                return Unauthorized(ApiException.InvalidKey());
            }
            try
            {
                if (request == null || string.IsNullOrEmpty(studentCode))
                {
                    return BadRequest(ApiException.InvalidParameter());
                }

                var student = _db.Students.IgnoreQueryFilters()
                                          .Include(x => x.AcademicInformation)
                                          .FirstOrDefault(x => x.Code == studentCode);
                if (student == null)
                {
                    return NotFound();
                }
                //if (!student.IsActive)
                //{
                //    return Error(StudentAPIException.StudentInActive());
                //}
                if (student.AcademicInformation is null)
                {
                    return Error(StudentAPIException.InvalidStudentCode(studentCode + "'s academic information"));
                }
                var currentTerm = _db.Terms.AsNoTracking()
                                               .FirstOrDefault(x => x.AcademicLevelId == student.AcademicInformation.AcademicLevelId
                                                                        && x.IsCurrent);
                if (currentTerm == null)
                {
                    return Error(new ResultException
                    {
                        Code = $"{ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, "API")}000",
                        Message = "No current term."
                    });
                }

                if (string.IsNullOrEmpty(request.Status))
                {
                    return Error(ApiException.InvalidParameter());
                }
                if (!Enum.TryParse<StudentStatus>(request.Status.ToUpper(), out StudentStatus status))
                {
                    return Error(StudentAPIException.InvalidStudentStatus(request.Status));
                }

                DateTime effectiveDate = DateTime.Now;
                if (request.EffectDate != null)
                {
                    if (!string.IsNullOrEmpty(request.EffectDate))
                    {
                        CultureInfo enUS = new CultureInfo("en-US");
                        if (!DateTime.TryParseExact(request.EffectDate, "dd/MM/yyyy", enUS, DateTimeStyles.None, out DateTime date))
                        {
                            return Error(StudentAPIException.DateWrongFormat(request.EffectDate));
                        }
                        effectiveDate = date;
                    }
                }

                // In case change status to graduate need to work on more stuff
                KeystoneLibrary.Models.DataModels.Term term = null;
                if (IsGraduationStatus(status))
                {
                    if (string.IsNullOrEmpty(request.GraduateAcademicYear)
                        || request.GraduateAcademicYear.IndexOf("Y", StringComparison.InvariantCultureIgnoreCase) < 0
                        || request.GraduateAcademicYear.IndexOf("T", StringComparison.InvariantCultureIgnoreCase) < 0
                        || request.GraduateAcademicYear.Length < 5)
                    {
                        return Error(StudentAPIException.InvalidTermFormat(request.GraduateAcademicYear));
                    }

                    var yearStartIndex = request.GraduateAcademicYear.IndexOf("Y", StringComparison.InvariantCultureIgnoreCase);
                    var termStartIndex = request.GraduateAcademicYear.IndexOf("T", StringComparison.InvariantCultureIgnoreCase);

                    if (!int.TryParse(request.GraduateAcademicYear.Substring(yearStartIndex + 1, termStartIndex - 1), out int year))
                    {
                        return Error(StudentAPIException.InvalidTermFormat(request.GraduateAcademicYear));
                    }

                    if (!int.TryParse(request.GraduateAcademicYear.Substring(termStartIndex + 1, request.GraduateAcademicYear.Length - (termStartIndex + 1)), out int termNo))
                    {
                        return Error(StudentAPIException.InvalidTermFormat(request.GraduateAcademicYear));
                    }

                    var academicTerm = _db.Terms.AsNoTracking()
                                                .FirstOrDefault(x => x.AcademicLevelId == student.AcademicInformation.AcademicLevelId
                                                                     && x.AcademicYear == year
                                                                     && x.AcademicTerm == termNo);

                    if (academicTerm is null)
                    {
                        return Error(StudentAPIException.TermNotFound(year, termNo));
                    }

                    term = academicTerm;
                }

                var updateTime = DateTime.UtcNow;
                var userName = string.IsNullOrEmpty(request.UpdatedBy) ? "StudentAPI" : request.UpdatedBy;



                try
                {
                    using (var transaction = _db.Database.BeginTransaction())
                    {
                        student.StudentStatus = status.ToString().ToLower();
                        student.IsActive = StudentProvider.IsActiveFromStudentStatus(student.StudentStatus);
                        student.UpdatedAt = updateTime;
                        student.UpdatedBy = userName;

                        _db.SaveChangesNoAutoUserIdAndTimestamps();

                        var currentTermId = currentTerm.Id;
                        _studentProvider.SaveStudentStatusLog(student.Id
                                                                , currentTermId
                                                                , SaveStatusSouces.STUDENTAPI.GetDisplayName()
                                                                , request.Remark
                                                                , student.StudentStatus
                                                                , effectiveDate);

                        var graduationInfo = _db.GraduationInformations.FirstOrDefault(x => x.StudentId == student.Id);
                        if (IsGraduationStatus(status))
                        {
                            // Code from GraduationProvider.cs > SaveGraduation
                            if (graduationInfo == null)
                            {
                                var curriculumInfo = _studentProvider.GetCurrentCurriculum(student.Id);
                                GraduationInformation info = new GraduationInformation
                                {
                                    StudentId = student.Id,
                                    CurriculumInformationId = curriculumInfo?.Id,
                                    GraduatedAt = effectiveDate,
                                    TermId = term.Id,
                                    HonorId = null,
                                    Remark = request.Remark,
                                    OtherRemark1 = "",
                                    CreatedAt = updateTime,
                                    CreatedBy = userName,
                                    UpdatedAt = updateTime,
                                    UpdatedBy = userName,
                                };

                                _db.GraduationInformations.Add(info);
                            }
                            else
                            {
                                graduationInfo.GraduatedAt = effectiveDate;
                                graduationInfo.TermId = term.Id;
                                graduationInfo.HonorId = null;
                                graduationInfo.Remark = request.Remark;
                                graduationInfo.OtherRemark1 = "";
                                graduationInfo.UpdatedAt = updateTime;
                                graduationInfo.UpdatedBy = userName;
                            }
                        }
                        else
                        {
                            if (graduationInfo != null)
                            {
                                graduationInfo.IsActive = false;
                                graduationInfo.UpdatedAt = updateTime;
                                graduationInfo.UpdatedBy = userName;
                            }
                        }
                        _db.SaveChangesNoAutoUserIdAndTimestamps();

                        await LogApiInStudentLog(userName, student.Id, "StudentAPI > PatchStudentStatusAsync");

                        transaction.Commit();
                    }


                    _db.SaveChangesNoAutoUserIdAndTimestamps();
                }
                catch (Exception e)
                {
                    throw e;
                }

                return Success(1);
            }
            catch (Exception e)
            {
                return Error(new ResultException
                {
                    Code = $"{ResultExceptionHelper.Compose(ResultExceptionPrefix.BadRequest, "API")}000",
                    Message = "Exception " + e.Message
                });
            }
        }

        private static bool IsGraduationStatus(StudentStatus status)
        {
            return status == StudentStatus.G || status == StudentStatus.G1 || status == StudentStatus.G2;
        }
    }
    
}