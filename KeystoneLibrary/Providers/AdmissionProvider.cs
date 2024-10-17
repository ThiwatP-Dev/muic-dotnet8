using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Admission;
using KeystoneLibrary.Models.DataModels.Curriculums;
using KeystoneLibrary.Models.DataModels.MasterTables;
using KeystoneLibrary.Models.DataModels.Profile;
using Microsoft.EntityFrameworkCore;

namespace KeystoneLibrary.Providers
{
    public class AdmissionProvider : BaseProvider, IAdmissionProvider
    {
        // protected IEmailProvider _emailProvider;

        public AdmissionProvider(ApplicationDbContext db,
                                 IMapper mapper) : base(db, mapper) 
        {
            // _emailProvider = emailProvider;
        }
        
        public List<PreviousSchool> GetPreviousSchoolsByCountryId(long id)
        {
            var previousSchools = _db.PreviousSchools.Where(x => id == 0
                                                                 || x.CountryId == id)
                                                     .ToList();
            return previousSchools;
        }

        public RegistrationApplicationViewModel GetAdmissionInformation(Guid studentId) 
        {
            var model = new RegistrationApplicationViewModel();
            var student = _db.Students.SingleOrDefault(x => x.Id == studentId);
            model = _mapper.Map<Student, RegistrationApplicationViewModel>(student);
            model.ApplicationNumber = _db.AdmissionInformations.SingleOrDefault(x => x.StudentId == studentId).ApplicationNumber;
            model.StudentId = studentId;

            var admissionInformation = _db.AdmissionInformations.SingleOrDefault(x => x.StudentId == studentId);
            if (admissionInformation.PreviousSchoolId.HasValue) 
            {
                model.IsSchoolNotListed = false;
                model.SchoolInstituitionId = admissionInformation.PreviousSchoolId.Value;
            }
            else 
            {
                model.IsSchoolNotListed = true;
            }
            model.SchoolInstituitionCountryId = admissionInformation.GraduatedCountryId.Value;
            model.GraduatedYear = admissionInformation.PreviousGraduatedYear;
            model.GPA = admissionInformation.PreviousSchoolGPA;
            model.AcademicLevelId = admissionInformation.AcademicLevelId.Value;
            model.FacultyId = admissionInformation.FacultyId.Value;
            model.DepartmentId = admissionInformation.DepartmentId.Value;

            var studentAddresses = _db.StudentAddresses.Where(x => x.StudentId == studentId).ToList();
            if (studentAddresses.Any()) 
            {

                var currentAddress = studentAddresses.SingleOrDefault(x => x.Type == "C");
                if (currentAddress != null) 
                {
                    model.AddressEn = currentAddress.AddressEn1;
                    model.AddressTh = currentAddress.AddressTh1;
                    model.HouseNumber = currentAddress.HouseNumber;
                    model.Moo = currentAddress.Moo;
                    model.Soi = currentAddress.SoiEn;
                    model.Road = currentAddress.RoadEn;
                    model.CountryId = currentAddress.CountryId;
                    if (currentAddress.ProvinceId.HasValue) 
                    {
                        model.ProvinceId = currentAddress.ProvinceId.Value;
                    }
                    if (currentAddress.DistrictId.HasValue) 
                    {
                        model.DistrictId = currentAddress.DistrictId.Value;
                    }
                    if (currentAddress.SubdistrictId.HasValue) 
                    {
                        model.SubdistrictId = currentAddress.SubdistrictId.Value;
                    }
                    if (currentAddress.StateId.HasValue) 
                    {
                        model.StateId = currentAddress.StateId.Value;
                    }
                    if (currentAddress.CityId.HasValue) 
                    {
                        model.CityId = currentAddress.CityId.Value;
                    }
                    model.ZipCode = currentAddress.ZipCode;
                    model.TelephoneNumber = currentAddress.TelephoneNumber;
                }
                var permanentAddress = studentAddresses.SingleOrDefault(x => x.Type == "P");
                if (permanentAddress != null) 
                {
                    model.IsSameAddress = CheckIfSameAddress(currentAddress, permanentAddress);

                    if (!model.IsSameAddress)
                    {
                        model.PermanentAddressEn = permanentAddress.AddressEn1;
                        model.PermanentAddressTh = permanentAddress.AddressTh1;
                        model.PermanentHouseNumber = permanentAddress.HouseNumber;
                        model.PermanentMoo = permanentAddress.Moo;
                        model.PermanentSoi = permanentAddress.SoiEn;
                        model.PermanentRoad = permanentAddress.RoadEn;
                        model.PermanentCountryId = permanentAddress.CountryId;
                        if (permanentAddress.ProvinceId.HasValue) 
                        {
                            model.PermanentProvinceId = permanentAddress.ProvinceId.Value;
                        }
                        if (permanentAddress.DistrictId.HasValue) 
                        {
                            model.PermanentDistrictId = permanentAddress.DistrictId.Value;
                        }
                        if (permanentAddress.SubdistrictId.HasValue) 
                        {
                            model.PermanentSubdistrictId = permanentAddress.SubdistrictId.Value;
                        }
                        if (permanentAddress.StateId.HasValue) 
                        {
                            model.PermanentStateId = permanentAddress.StateId.Value;
                        }
                        if (permanentAddress.CityId.HasValue) 
                        {
                            model.PermanentCityId = permanentAddress.CityId.Value;
                        }
                        model.PermanentZipCode = permanentAddress.ZipCode;
                        model.PermanentTelephoneNumber = permanentAddress.TelephoneNumber;
                    }
                }
            }

            var parentInformations = _db.ParentInformations.Where(x => x.StudentId == studentId).ToList();
            if (parentInformations.Any()) 
            {
                var fatherRelationship = _db.Relationships.SingleOrDefault(x => x.NameEn.ToLower() == "father");
                var father = parentInformations.SingleOrDefault(x => x.RelationshipId == fatherRelationship.Id);
                model.FatherFullName = father.FirstNameEn;
                model.FatherCountryId = father.CountryId.Value;
                model.FatherEmail = father.Email;
                model.FatherPhoneNumber = father.TelephoneNumber1;
                model.FatherCitizenNumber = father.CitizenNumber;
                model.FatherPassportNumber = father.Passport;
                model.FatherOccupationId = father.OccupationId;
                if (father.IsMainParent) 
                {
                    model.GuardianRelationshipId = fatherRelationship.Id;
                }

                var motherRelationship = _db.Relationships.SingleOrDefault(x => x.NameEn.ToLower() == "mother");
                var mother = parentInformations.SingleOrDefault(x => x.RelationshipId == motherRelationship.Id);
                model.MotherFullName = mother.FirstNameEn;
                model.MotherCountryId = mother.CountryId.Value;
                model.MotherEmail = mother.Email;
                model.MotherPhoneNumber = mother.TelephoneNumber1;
                model.MotherCitizenNumber = mother.CitizenNumber;
                model.MotherPassportNumber = mother.Passport;
                model.MotherOccupationId = mother.OccupationId;
                if (mother.IsMainParent) 
                {
                    model.GuardianRelationshipId = motherRelationship.Id;
                }

                if (!father.IsMainParent && !mother.IsMainParent)
                {
                    var guardian = parentInformations.SingleOrDefault(x => x.IsMainParent && x.RelationshipId != fatherRelationship.Id && x.RelationshipId != motherRelationship.Id);
                    if (guardian != null) 
                    {
                        model.GuardianRelationshipId = guardian.RelationshipId;
                        model.GuardianFullName = guardian.FirstNameEn;
                        model.GuardianCountryId = guardian.CountryId.Value;
                        model.GuardianEmail = guardian.Email;
                        model.GuardianPhoneNumber = guardian.TelephoneNumber1;
                        model.GuardianCitizenNumber = guardian.CitizenNumber;
                        model.GuardianPassportNumber = guardian.Passport;
                        model.GuardianOccupationId = guardian.OccupationId;
                    }
                }
            }

            var exams = GetExemptedExaminations();
            var studentExemptedExaminations = _db.StudentExemptedExamScores.Where(x => x.StudentId == studentId).ToList();
            model.ExemptedExaminations = new List<ExemptedAdmissionExamination>();
            foreach (var exam in exams) 
            {
                var studentExemptedExamination = studentExemptedExaminations.SingleOrDefault(x => x.ExemptedExaminationId == exam.Id);
                if (studentExemptedExamination != null) 
                {
                    exam.Score = studentExemptedExamination.Score;
                }
                model.ExemptedExaminations.Add(exam);
            }
            return model;
        }

        public ResponseModel UpdateOnlineAdmissionApplication(RegistrationApplicationViewModel viewModel)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var student = _db.Students.SingleOrDefault(x => x.Id == viewModel.StudentId);
                    viewModel.ProfileImageURL = student.ProfileImageURL;
                    student = _mapper.Map<RegistrationApplicationViewModel, Student>(viewModel, student);
                    
                    var admissionInformation = _db.AdmissionInformations.SingleOrDefault(x => x.StudentId == viewModel.StudentId);
                    viewModel.ApplicationNumber = admissionInformation.ApplicationNumber;
                    admissionInformation = _mapper.Map<RegistrationApplicationViewModel, AdmissionInformation>(viewModel, admissionInformation);
                    admissionInformation.AcademicLevelId = viewModel.AcademicLevelId;
                    admissionInformation.ApplicationNumber = viewModel.ApplicationNumber;
                    if (!viewModel.IsSchoolNotListed)
                    {
                        admissionInformation.PreviousSchoolId = viewModel.SchoolInstituitionId;
                    }

                    admissionInformation.GraduatedCountryId = viewModel.SchoolInstituitionCountryId;
                    admissionInformation.PreviousSchoolGPA = viewModel.GPA;
                    admissionInformation.PreviousGraduatedYear = viewModel.GraduatedYear;
                    var thaiCountry = _db.Countries.Where(x => x.NameEn.ToLower().Contains("thai"))
                                                   .FirstOrDefault();
                    thaiCountry = thaiCountry == null ? new Country() : thaiCountry;

                    var documentGroup = _db.AdmissionDocumentGroups.Where(x => x.AcademicLevelId == viewModel.AcademicLevelId 
                                                                        && (x.FacultyId == viewModel.FacultyId || x.FacultyId == null)
                                                                        && (x.DepartmentId == viewModel.DepartmentId || x.DepartmentId == null)
                                                                        && (x.GraduatedCountryId == viewModel.SchoolInstituitionCountryId 
                                                                            || (x.GraduatedCountryId == null
                                                                                && ((x.IsThai
                                                                                     && viewModel.SchoolInstituitionCountryId == thaiCountry.Id)
                                                                                   || (!x.IsThai && viewModel.SchoolInstituitionCountryId != thaiCountry.Id)))))
                                                                   .OrderByDescending(x => x.FacultyId)
                                                                   .ThenByDescending(x => x.DepartmentId)
                                                                   .ThenByDescending(x => x.GraduatedCountryId)
                                                                   .FirstOrDefault();
                    if (documentGroup != null) 
                    {
                        admissionInformation.AdmissionDocumentGroupId = documentGroup.Id;

                        //Get current documents and remove all unuploaded documents
                        var currentStudentDocuments = _db.StudentDocuments.Where(x => x.StudentId == viewModel.StudentId).ToList();
                        _db.StudentDocuments.RemoveRange(currentStudentDocuments.Where(x => x.ImageUrl == null));

                        //Add new group required documents
                        var requiredDocuments = _db.RequiredDocuments.Where(x => x.AdmissionDocumentGroupId == documentGroup.Id).ToList();
                        foreach (var document in requiredDocuments) 
                        {
                            var existingUploadedDocument = currentStudentDocuments.SingleOrDefault(x => x.ImageUrl != null && x.DocumentId == document.DocumentId);
                            if (existingUploadedDocument == null)
                            {
                                var studentDocument = new StudentDocument();
                                studentDocument.StudentId = student.Id;
                                studentDocument.RequiredDocumentId = document.Id;
                                studentDocument.Amount = document.Amount;
                                studentDocument.DocumentId = document.DocumentId;
                                studentDocument.IsRequired = document.IsRequired;

                                _db.StudentDocuments.Add(studentDocument);
                            }
                            else
                            {
                                existingUploadedDocument.RequiredDocumentId = document.Id;
                                existingUploadedDocument.Amount = document.Amount;
                                existingUploadedDocument.DocumentId = document.DocumentId;
                                existingUploadedDocument.IsRequired = document.IsRequired;
                            }
                        }
                    }

                    var parentInformations = _db.ParentInformations.Where(x => x.StudentId == viewModel.StudentId).ToList();
                    var fatherRelationship = _db.Relationships.SingleOrDefault(x => x.NameEn.ToLower() == "father");
                    if (!String.IsNullOrEmpty(viewModel.FatherFullName))
                    {
                        var father = parentInformations.SingleOrDefault(x => x.RelationshipId == fatherRelationship.Id);
                        if (father != null) 
                        {
                            father.FirstNameEn = viewModel.FatherFullName;
                            father.CountryId = viewModel.FatherCountryId;
                            father.Email = viewModel.FatherEmail;
                            father.TelephoneNumber1 = viewModel.FatherPhoneNumber;
                            father.OccupationId = viewModel.FatherOccupationId;
                            father.CitizenNumber = viewModel.FatherCitizenNumber;
                            father.Passport = viewModel.FatherPassportNumber;
                            father.IsMainParent = viewModel.GuardianRelationshipId == fatherRelationship.Id;
                        }
                        else 
                        {
                            _db.ParentInformations.Add(new ParentInformation
                                                       {
                                                           StudentId = student.Id,
                                                           RelationshipId = fatherRelationship.Id,
                                                           FirstNameEn = viewModel.FatherFullName,
                                                           CountryId = viewModel.FatherCountryId,
                                                           Email = viewModel.FatherEmail,
                                                           TelephoneNumber1 = viewModel.FatherPhoneNumber,
                                                           OccupationId = viewModel.FatherOccupationId,
                                                           CitizenNumber = viewModel.FatherCitizenNumber,
                                                           Passport = viewModel.FatherPassportNumber,
                                                           IsMainParent = viewModel.GuardianRelationshipId == fatherRelationship.Id
                                                       });
                        }
                    }

                    var motherRelationship = _db.Relationships.SingleOrDefault(x => x.NameEn.ToLower() == "mother");
                    if (!String.IsNullOrEmpty(viewModel.MotherFullName))
                    {
                        var mother = parentInformations.SingleOrDefault(x => x.RelationshipId == motherRelationship.Id);
                        if (mother != null) 
                        {
                            mother.FirstNameEn = viewModel.MotherFullName;
                            mother.CountryId = viewModel.MotherCountryId;
                            mother.Email = viewModel.MotherEmail;
                            mother.TelephoneNumber1 = viewModel.MotherPhoneNumber;
                            mother.OccupationId = viewModel.MotherOccupationId;
                            mother.CitizenNumber = viewModel.MotherCitizenNumber;
                            mother.Passport = viewModel.MotherPassportNumber;
                            mother.IsMainParent= viewModel.GuardianRelationshipId == motherRelationship.Id;
                        }
                        else 
                        {
                            _db.ParentInformations.Add(new ParentInformation
                                                       {
                                                           StudentId = student.Id,
                                                           RelationshipId = motherRelationship.Id,
                                                           FirstNameEn = viewModel.MotherFullName,
                                                           CountryId = viewModel.MotherCountryId,
                                                           Email = viewModel.MotherEmail,
                                                           TelephoneNumber1 = viewModel.MotherPhoneNumber,
                                                           OccupationId = viewModel.MotherOccupationId,
                                                           CitizenNumber = viewModel.MotherCitizenNumber,
                                                           Passport = viewModel.MotherPassportNumber,
                                                           IsMainParent = viewModel.GuardianRelationshipId == motherRelationship.Id
                                                       });
                        }
                    }

                    if (viewModel.GuardianRelationshipId != fatherRelationship.Id 
                        && viewModel.GuardianRelationshipId != motherRelationship.Id)
                    {
                        //Use guardian
                        var guardian = parentInformations.SingleOrDefault(x => x.RelationshipId != motherRelationship.Id && x.RelationshipId != fatherRelationship.Id);
                        if (guardian != null) 
                        {
                            guardian.FirstNameEn = viewModel.GuardianFullName;
                            guardian.CountryId = viewModel.GuardianCountryId;
                            guardian.Email = viewModel.GuardianEmail;
                            guardian.TelephoneNumber1 = viewModel.GuardianPhoneNumber;
                            guardian.OccupationId = viewModel.GuardianOccupationId;
                            guardian.CitizenNumber = viewModel.GuardianCitizenNumber;
                            guardian.Passport = viewModel.GuardianPassportNumber;
                            guardian.IsMainParent = true;
                        }
                        else 
                        {
                            _db.ParentInformations.Add(new ParentInformation
                                                       {
                                                           StudentId = student.Id,
                                                           RelationshipId = viewModel.GuardianRelationshipId,
                                                           FirstNameEn = viewModel.GuardianFullName,
                                                           CountryId = viewModel.GuardianCountryId,
                                                           Email = viewModel.GuardianEmail,
                                                           TelephoneNumber1 = viewModel.GuardianPhoneNumber,
                                                           OccupationId = viewModel.GuardianOccupationId,
                                                           CitizenNumber = viewModel.GuardianCitizenNumber,
                                                           Passport = viewModel.GuardianPassportNumber,
                                                           IsMainParent = true
                                                       });
                        }
                    }
                    else 
                    {
                        //Delete guardian info if exists
                        var guardian = parentInformations.SingleOrDefault(x => x.RelationshipId != motherRelationship.Id && x.RelationshipId != fatherRelationship.Id);
                        if (guardian != null)
                        {
                            _db.ParentInformations.Remove(guardian);
                        }
                    }

                    var addresses = _db.StudentAddresses.Where(x => x.StudentId == viewModel.StudentId).ToList();
                    if (!String.IsNullOrEmpty(viewModel.AddressEn) || !String.IsNullOrEmpty(viewModel.AddressTh) || !String.IsNullOrEmpty(viewModel.HouseNumber)) 
                    {
                        var currentAddress = addresses.SingleOrDefault(x => x.Type == "C");
                        if (currentAddress == null) 
                        {
                            currentAddress = new StudentAddress();
                            currentAddress.StudentId = student.Id;
                            currentAddress.Type = "C";
                            _db.StudentAddresses.Add(currentAddress);
                        }
                        
                        currentAddress.AddressEn1 = viewModel.AddressEn;
                        currentAddress.AddressTh1 = viewModel.AddressTh;
                        currentAddress.HouseNumber = viewModel.HouseNumber;
                        currentAddress.Moo = viewModel.Moo;
                        currentAddress.SoiEn = viewModel.Soi;
                        currentAddress.RoadEn = viewModel.Road;
                        currentAddress.CountryId = viewModel.CountryId;

                        if (viewModel.ProvinceId != 0)
                        {
                            currentAddress.ProvinceId = viewModel.ProvinceId;
                        }

                        if (viewModel.DistrictId != 0)
                        {
                        currentAddress.DistrictId = viewModel.DistrictId;
                        }

                        if (viewModel.SubdistrictId != 0)
                        {
                        currentAddress.SubdistrictId = viewModel.SubdistrictId;
                        }

                        if (viewModel.StateId != 0)
                        {
                        currentAddress.StateId = viewModel.StateId;
                        }

                        if (viewModel.CityId != 0)
                        {
                            currentAddress.CityId = viewModel.CityId;
                        }

                        currentAddress.ZipCode = viewModel.ZipCode.ToString();
                        currentAddress.TelephoneNumber = viewModel.TelephoneNumber;
                    }

                    if (viewModel.IsSameAddress || !String.IsNullOrEmpty(viewModel.PermanentAddressEn) || !String.IsNullOrEmpty(viewModel.PermanentAddressTh) || !String.IsNullOrEmpty(viewModel.PermanentHouseNumber)) 
                    {
                        var permanentAddress = addresses.SingleOrDefault(x => x.Type == "P");
                        if (permanentAddress == null) 
                        {
                            permanentAddress = new StudentAddress();
                            permanentAddress.StudentId = student.Id;
                            permanentAddress.Type = "P";
                            _db.StudentAddresses.Add(permanentAddress);
                        }

                        if (viewModel.IsSameAddress) 
                        {
                            permanentAddress.AddressEn1 = viewModel.AddressEn;
                            permanentAddress.AddressTh1 = viewModel.AddressTh;
                            permanentAddress.HouseNumber = viewModel.HouseNumber;
                            permanentAddress.Moo = viewModel.Moo;
                            permanentAddress.SoiEn = viewModel.Soi;
                            permanentAddress.RoadEn = viewModel.Road;
                            permanentAddress.CountryId = viewModel.CountryId;

                            if (viewModel.ProvinceId != 0)
                            {
                                permanentAddress.ProvinceId = viewModel.ProvinceId;
                            }

                            if (viewModel.DistrictId != 0)
                            {
                                permanentAddress.DistrictId = viewModel.DistrictId;
                            }

                            if (viewModel.SubdistrictId != 0)
                            {
                                permanentAddress.SubdistrictId = viewModel.SubdistrictId;
                            }

                            if (viewModel.StateId != 0)
                            {
                                permanentAddress.StateId = viewModel.StateId;
                            }

                            if (viewModel.CityId != 0)
                            {
                                permanentAddress.CityId = viewModel.CityId;
                            }

                            permanentAddress.ZipCode = viewModel.ZipCode.ToString();
                            permanentAddress.TelephoneNumber = viewModel.TelephoneNumber;
                        }
                        else 
                        {
                            permanentAddress.AddressEn1 = viewModel.PermanentAddressEn;
                            permanentAddress.AddressTh1 = viewModel.PermanentAddressTh;
                            permanentAddress.HouseNumber = viewModel.PermanentHouseNumber;
                            permanentAddress.Moo = viewModel.PermanentMoo;
                            permanentAddress.SoiEn = viewModel.PermanentSoi;
                            permanentAddress.RoadEn = viewModel.PermanentRoad;
                            permanentAddress.CountryId = viewModel.PermanentCountryId;

                            if (viewModel.PermanentProvinceId != 0)
                            {
                                permanentAddress.ProvinceId = viewModel.PermanentProvinceId;
                            }

                            if (viewModel.PermanentDistrictId != 0)
                            {
                                permanentAddress.DistrictId = viewModel.PermanentDistrictId;
                            }

                            if (viewModel.PermanentSubdistrictId != 0)
                            {
                                permanentAddress.SubdistrictId = viewModel.PermanentSubdistrictId;
                            }

                            if (viewModel.PermanentStateId != 0)
                            {
                                permanentAddress.StateId = viewModel.PermanentStateId;
                            }

                            if (viewModel.PermanentCityId != 0)
                            {
                                permanentAddress.CityId = viewModel.PermanentCityId;
                            }

                            permanentAddress.ZipCode = viewModel.PermanentZipCode.ToString();
                            permanentAddress.TelephoneNumber = viewModel.PermanentTelephoneNumber;
                        }
                    }

                    var dbExemptedExaminations = _db.StudentExemptedExamScores.Where(x => x.StudentId == viewModel.StudentId).ToList();
                    if (viewModel.ExemptedExaminations.Any()) 
                    {
                        foreach (var item in viewModel.ExemptedExaminations.Where(x => x.Score > 0)) 
                        {
                            if (item.Score == 0) 
                            {
                                //Delete
                                var dbScore = dbExemptedExaminations.SingleOrDefault(x => x.ExemptedExaminationId == item.Id);
                                if (dbScore != null) 
                                {
                                    _db.StudentExemptedExamScores.Remove(dbScore);
                                }
                            }
                            else 
                            {
                                //Create / Update
                                var score = dbExemptedExaminations.SingleOrDefault(x => x.ExemptedExaminationId == item.Id);
                                if (score == null) 
                                {
                                    score = new StudentExemptedExamScore();
                                    score.ExemptedExaminationId = item.Id;
                                    score.StudentId = student.Id;
                                    _db.StudentExemptedExamScores.Add(score);
                                }
                                score.Score = item.Score;
                            }
                        }
                    }
                    else 
                    {
                        //Delete all scores
                        _db.StudentExemptedExamScores.RemoveRange(dbExemptedExaminations);
                    }

                    _db.SaveChanges();
                    transaction.Commit();
                    return new ResponseModel { IsSuccess = true, Message = Message.SaveSucceed, StudentId = student.Id};
                }
                catch
                {
                    transaction.Rollback();
                    return new ResponseModel { IsSuccess = false, Message = Message.DatabaseProblem };
                }
            }
        }


        public ResponseModel CreateOnlineAdmissionApplication(RegistrationApplicationViewModel viewModel)
        {
            if (IsStudentBlacklisted(viewModel.CitizenNumber, viewModel.Passport, viewModel.FirstNameEn, viewModel.LastNameEn,
                                    viewModel.FirstNameTh, viewModel.LastNameTh, viewModel.BirthDate.Value, viewModel.Gender))
            {
                return new ResponseModel { IsSuccess = false, Message = Message.BlackListedStudent };
            }

            var student = _mapper.Map<RegistrationApplicationViewModel, Student>(viewModel);
            var admissionInformation = _mapper.Map<RegistrationApplicationViewModel, AdmissionInformation>(viewModel);
            var studentAddresses = new List<StudentAddress>();
            var parentInformations = new List<ParentInformation>();
            var scores = new List<StudentExemptedExamScore>();

            student.RegistrationStatusId = 1;
            student.StudentStatus = "a";
            student.IsActive = StudentProvider.IsActiveFromStudentStatus(student.StudentStatus);
            
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    //var academicRound = _db.AdmissionRounds.FirstOrDefault(x => x.AcademicLevelId == viewModel.AcademicLevelId && x.StartedAt <= DateTime.Today && x.EndedAt >= DateTime.Today);
                    student.Code = "";//GenerateStudentCode(viewModel.AcademicLevelId, academicRound.Id);
                    _db.Students.Add(student);
                    _db.SaveChanges();

                    admissionInformation.StudentId = student.Id;
                    admissionInformation.Password = PasswordHelper.Generate(8, 0);
                    admissionInformation.AppliedAt = DateTime.Now;
                    // admissionInformation.AdmissionTermId = academicRound.AdmissionTermId;
                    // admissionInformation.AdmissionRoundId = academicRound.Id;
                    admissionInformation.AcademicLevelId = viewModel.AcademicLevelId;
                    admissionInformation.ApplicationNumber = GenerateApplicationNumber(viewModel.AcademicLevelId);
                    if (!viewModel.IsSchoolNotListed)
                    {
                        admissionInformation.PreviousSchoolId = viewModel.SchoolInstituitionId;
                    }

                    admissionInformation.GraduatedCountryId = viewModel.SchoolInstituitionCountryId;
                    admissionInformation.PreviousSchoolGPA = viewModel.GPA;
                    admissionInformation.PreviousGraduatedYear = viewModel.GraduatedYear;
                    var thaiCountry = _db.Countries.SingleOrDefault(x => x.Code == "10000");
                    var documentGroup = _db.AdmissionDocumentGroups.Where(x => x.AcademicLevelId == viewModel.AcademicLevelId 
                                                                        && (x.FacultyId == viewModel.FacultyId || x.FacultyId == null)
                                                                        && (x.DepartmentId == viewModel.DepartmentId || x.DepartmentId == null)
                                                                        && (x.GraduatedCountryId == viewModel.SchoolInstituitionCountryId 
                                                                            || (x.GraduatedCountryId == null && ((x.IsThai && viewModel.SchoolInstituitionCountryId == thaiCountry.Id) || (!x.IsThai && viewModel.SchoolInstituitionCountryId != thaiCountry.Id))))
                                                                    )
                                                                   .OrderByDescending(x => x.FacultyId)
                                                                   .ThenByDescending(x => x.DepartmentId)
                                                                   .ThenByDescending(x => x.GraduatedCountryId)
                                                                   .FirstOrDefault();
                    if (documentGroup != null) 
                    {
                        admissionInformation.AdmissionDocumentGroupId = documentGroup.Id;   

                        //Save student documents
                        var requiredDocuments = _db.RequiredDocuments.Where(x => x.AdmissionDocumentGroupId == documentGroup.Id).ToList();
                        foreach (var document in requiredDocuments) 
                        {
                            var studentDocument = new StudentDocument();
                            studentDocument.StudentId = student.Id;
                            studentDocument.RequiredDocumentId = document.Id;
                            studentDocument.Amount = document.Amount;
                            studentDocument.DocumentId = document.DocumentId;
                            studentDocument.IsRequired = document.IsRequired;
                            _db.StudentDocuments.Add(studentDocument);
                        }
                    }

                    if (!String.IsNullOrEmpty(viewModel.FatherFullName))
                    {
                        var father = _db.Relationships.SingleOrDefault(x => x.NameEn.ToLower() == "father");
                        parentInformations.Add(new ParentInformation
                                            {
                                                StudentId = student.Id,
                                                RelationshipId = father.Id,
                                                FirstNameEn = viewModel.FatherFullName,
                                                CountryId = viewModel.FatherCountryId,
                                                Email = viewModel.FatherEmail,
                                                TelephoneNumber1 = viewModel.FatherPhoneNumber,
                                                OccupationId = viewModel.FatherOccupationId,
                                                CitizenNumber = viewModel.FatherCitizenNumber,
                                                Passport = viewModel.FatherPassportNumber,
                                                IsMainParent = viewModel.GuardianRelationshipId == father.Id
                                            });
                    }

                    if (!String.IsNullOrEmpty(viewModel.MotherFullName))
                    {
                        var mother = _db.Relationships.SingleOrDefault(x => x.NameEn.ToLower() == "mother");
                        parentInformations.Add(new ParentInformation
                                            {
                                                StudentId = student.Id,
                                                RelationshipId = mother.Id,
                                                FirstNameEn = viewModel.MotherFullName,
                                                CountryId = viewModel.MotherCountryId,
                                                Email = viewModel.MotherEmail,
                                                TelephoneNumber1 = viewModel.MotherPhoneNumber,
                                                OccupationId = viewModel.MotherOccupationId,
                                                CitizenNumber = viewModel.MotherCitizenNumber,
                                                Passport = viewModel.MotherPassportNumber,
                                                IsMainParent = viewModel.GuardianRelationshipId == mother.Id
                                            });
                    }

                    if (!String.IsNullOrEmpty(viewModel.GuardianFullName))
                    {
                        parentInformations.Add(new ParentInformation
                                            {
                                                StudentId = student.Id,
                                                RelationshipId = viewModel.GuardianRelationshipId,
                                                FirstNameEn = viewModel.GuardianFullName,
                                                CountryId = viewModel.GuardianCountryId,
                                                Email = viewModel.GuardianEmail,
                                                TelephoneNumber1 = viewModel.GuardianPhoneNumber,
                                                OccupationId = viewModel.GuardianOccupationId,
                                                CitizenNumber = viewModel.GuardianCitizenNumber,
                                                Passport = viewModel.GuardianPassportNumber,
                                                IsMainParent = true
                                            });
                    }

                    if (!String.IsNullOrEmpty(viewModel.AddressEn) || !String.IsNullOrEmpty(viewModel.AddressTh) || !String.IsNullOrEmpty(viewModel.HouseNumber)) 
                    {
                        var currentAddress = new StudentAddress();
                        currentAddress.StudentId = student.Id;
                        currentAddress.Type = "C";
                        currentAddress.AddressEn1 = viewModel.AddressEn;
                        currentAddress.AddressTh1 = viewModel.AddressTh;
                        currentAddress.HouseNumber = viewModel.HouseNumber;
                        currentAddress.Moo = viewModel.Moo;
                        currentAddress.SoiEn = viewModel.Soi;
                        currentAddress.RoadEn = viewModel.Road;
                        currentAddress.CountryId = viewModel.CountryId;

                        if (viewModel.ProvinceId != 0)
                        {
                            currentAddress.ProvinceId = viewModel.ProvinceId;
                        }

                        if (viewModel.DistrictId != 0)
                        {
                        currentAddress.DistrictId = viewModel.DistrictId;
                        }

                        if (viewModel.SubdistrictId != 0)
                        {
                        currentAddress.SubdistrictId = viewModel.SubdistrictId;
                        }

                        if (viewModel.StateId != 0)
                        {
                        currentAddress.StateId = viewModel.StateId;
                        }

                        if (viewModel.CityId != 0)
                        {
                            currentAddress.CityId = viewModel.CityId;
                        }

                        currentAddress.ZipCode = viewModel.ZipCode.ToString();
                        currentAddress.TelephoneNumber = viewModel.TelephoneNumber;
                        studentAddresses.Add(currentAddress);
                    }

                    if (viewModel.IsSameAddress || !String.IsNullOrEmpty(viewModel.PermanentAddressEn) || !String.IsNullOrEmpty(viewModel.PermanentAddressTh) || !String.IsNullOrEmpty(viewModel.PermanentHouseNumber)) 
                    {
                        var permanentAddress = new StudentAddress();
                        permanentAddress.StudentId = student.Id;
                        permanentAddress.Type = "P";
                        if (viewModel.IsSameAddress) 
                        {
                            permanentAddress.AddressEn1 = viewModel.AddressEn;
                            permanentAddress.AddressTh1 = viewModel.AddressTh;
                            permanentAddress.HouseNumber = viewModel.HouseNumber;
                            permanentAddress.Moo = viewModel.Moo;
                            permanentAddress.SoiEn = viewModel.Soi;
                            permanentAddress.RoadEn = viewModel.Road;
                            permanentAddress.CountryId = viewModel.CountryId;

                            if (viewModel.ProvinceId != 0)
                            {
                                permanentAddress.ProvinceId = viewModel.ProvinceId;
                            }

                            if (viewModel.DistrictId != 0)
                            {
                                permanentAddress.DistrictId = viewModel.DistrictId;
                            }

                            if (viewModel.SubdistrictId != 0)
                            {
                                permanentAddress.SubdistrictId = viewModel.SubdistrictId;
                            }

                            if (viewModel.StateId != 0)
                            {
                                permanentAddress.StateId = viewModel.StateId;
                            }

                            if (viewModel.CityId != 0)
                            {
                                permanentAddress.CityId = viewModel.CityId;
                            }

                            permanentAddress.ZipCode = viewModel.ZipCode.ToString();
                            permanentAddress.TelephoneNumber = viewModel.TelephoneNumber;
                        }
                        else 
                        {
                            permanentAddress.AddressEn1 = viewModel.PermanentAddressEn;
                            permanentAddress.AddressTh1 = viewModel.PermanentAddressTh;
                            permanentAddress.HouseNumber = viewModel.PermanentHouseNumber;
                            permanentAddress.Moo = viewModel.PermanentMoo;
                            permanentAddress.SoiEn = viewModel.PermanentSoi;
                            permanentAddress.RoadEn = viewModel.PermanentRoad;
                            permanentAddress.CountryId = viewModel.PermanentCountryId;

                            if (viewModel.PermanentProvinceId != 0)
                            {
                                permanentAddress.ProvinceId = viewModel.PermanentProvinceId;
                            }

                            if (viewModel.PermanentDistrictId != 0)
                            {
                                permanentAddress.DistrictId = viewModel.PermanentDistrictId;
                            }

                            if (viewModel.PermanentSubdistrictId != 0)
                            {
                                permanentAddress.SubdistrictId = viewModel.PermanentSubdistrictId;
                            }

                            if (viewModel.PermanentStateId != 0)
                            {
                                permanentAddress.StateId = viewModel.PermanentStateId;
                            }

                            if (viewModel.PermanentCityId != 0)
                            {
                                permanentAddress.CityId = viewModel.PermanentCityId;
                            }

                            permanentAddress.ZipCode = viewModel.PermanentZipCode.ToString();
                            permanentAddress.TelephoneNumber = viewModel.PermanentTelephoneNumber;
                        }
                        studentAddresses.Add(permanentAddress);
                    }

                    if (viewModel.ExemptedExaminations.Any(x => x.Score > 0)) 
                    {
                        foreach (var item in viewModel.ExemptedExaminations.Where(x => x.Score > 0)) 
                        {
                            var score = new StudentExemptedExamScore();
                            score.ExemptedExaminationId = item.Id;
                            score.StudentId = student.Id;
                            score.Score = item.Score;
                            scores.Add(score);
                        }
                    }

                    _db.AdmissionInformations.Add(admissionInformation);
                    if (studentAddresses.Any()) 
                    {
                        _db.StudentAddresses.AddRange(studentAddresses);
                    }
                    _db.ParentInformations.AddRange(parentInformations);
                    if (viewModel.ExemptedExaminations.Any(x => x.Score > 0)) 
                    {
                        _db.StudentExemptedExamScores.AddRange(scores);
                    }
                    _db.SaveChanges();
                    transaction.Commit();

                    var academicLevel = _db.AcademicLevels.SingleOrDefault(x => x.Id == admissionInformation.AcademicLevelId);
                    // if (academicLevel != null && academicLevel.AdmissionEmail != null) 
                    // {
                    //     _emailProvider.SendEmailReceivedOnlineAdmission(student, admissionInformation, academicLevel.AdmissionEmail);
                    // }

                    return new ResponseModel { IsSuccess = true, Message = Message.SaveSucceed, StudentId = student.Id};
                }
                catch
                {
                    transaction.Rollback();
                    return new ResponseModel { IsSuccess = false, Message = Message.DatabaseProblem };
                }
            }
        }

        public string GenerateStudentCode(long academicLevelId, long admissionRoundId)
        {
            var range = _db.StudentCodeRanges.FirstOrDefault(x => x.AcademicLevelId == academicLevelId
                                                                  && x.AdmissionRoundId == admissionRoundId);
            if (range != null)
            {
                var code = RandomStudentCode(range.StartedCodeInt, range.EndedCodeInt);
                return code;
            }

            return "";
        }

        private string GenerateApplicationNumber(long academicLevelId)
        {
            var academicLevel = _db.AcademicLevels.SingleOrDefault(x => x.Id == academicLevelId);
            if (academicLevel != null) 
            {
                var prefix = academicLevel.NameEn.Substring(0, 1);
                var yearMonth = DateTime.Today.ToString("yyMM");
                var latestNumber = _db.AdmissionInformations.Where(x => x.ApplicationNumber.StartsWith(prefix + yearMonth)).OrderBy(x => x.ApplicationNumber).LastOrDefault();
                if (latestNumber != null) 
                {
                    var runningNumber = Convert.ToInt32(latestNumber.ApplicationNumber.Substring(5, 3));
                    var applicationNumber = prefix + yearMonth + (runningNumber + 1).ToString("000");
                    return applicationNumber;
                }
                return prefix + yearMonth + "001";
            }
            return "";
        }

        private string RandomStudentCode (int startedCode, int endedCode)
        {
            Random random = new Random();
            var code = random.Next(startedCode, endedCode).ToString();
            var isExist = _db.Students.Any(x => x.Code == code && x.StudentStatus != "d");
            return (isExist || code.EndsWith("81000")) ? RandomStudentCode(startedCode, endedCode) : code;
        }

        public bool IsStudentBlacklisted(string citizenNumber, string passportNumber, string firstNameEn, string lastNameEn, string firstNameTh, string lastNameTh, DateTime birthDate, int gender)
        {
            var isStudentBlacklisted = _db.BlacklistedStudents.Where(x => x.CitizenNumber == citizenNumber
                                                                          || x.Passport == passportNumber
                                                                          || ((x.FirstNameEn == firstNameEn
                                                                               && x.LastNameEn == lastNameEn)
                                                                              || (x.FirstNameTh == firstNameTh
                                                                                  && x.LastNameTh == lastNameTh)
                                                                              && x.BirthDate == birthDate
                                                                              && x.Gender == gender))
                                                              .Any();
            return isStudentBlacklisted;
        }

        public List<ExemptedAdmissionExamination> GetExemptedExaminations() 
        {
            var exemptedExaminations = _db.ExemptedAdmissionExaminations.Where(x => x.DisplayAdmissionForm).ToList();
            return exemptedExaminations;
        }

        public RegistrationApplicationViewModel GetStudentRequiredDocuments(Guid studentId) 
        {
            var model = new RegistrationApplicationViewModel();
            model.StudentId = studentId;
            var studentInfo = (from student in _db.Students
                               join admissionInfo in _db.AdmissionInformations on student.Id equals admissionInfo.StudentId
                               where student.Id == studentId
                               select new 
                               {
                                    Student = student,
                                    AdmissionInformation = admissionInfo
                               }).SingleOrDefault();
            if (studentInfo != null) 
            {
                model.ApplicationNumber = studentInfo.AdmissionInformation.ApplicationNumber;
                model.ProfileImageURL = studentInfo.Student.ProfileImageURL;
                var documentGroup = _db.AdmissionDocumentGroups.SingleOrDefault(x => x.Id == studentInfo.AdmissionInformation.AdmissionDocumentGroupId);
                if (documentGroup != null) 
                {
                    model.StudentDocuments = new List<StudentDocument>();
                    var requiredDocuments = _db.RequiredDocuments.Where(x => x.AdmissionDocumentGroupId == documentGroup.Id).ToList();
                    var uploadedDocuments = _db.StudentDocuments.Where(x => x.StudentId == studentId).ToList();
                    foreach (var item in requiredDocuments) 
                    {
                        var uploadedDocument = uploadedDocuments.SingleOrDefault(x => x.DocumentId == item.DocumentId);
                        if (uploadedDocument != null)
                        {
                            uploadedDocument.DocumentId = item.DocumentId;
                            uploadedDocument.RequiredDocumentId = item.Id;
                            uploadedDocument.Document = _db.Documents.SingleOrDefault(x => x.Id == item.DocumentId);
                            uploadedDocument.ImageUrl = uploadedDocument.ImageUrl;
                            model.StudentDocuments.Add(uploadedDocument);
                        }
                        else 
                        {
                            var document = new StudentDocument();
                            document.DocumentId = item.DocumentId;
                            document.RequiredDocumentId = item.Id;
                            document.Document = _db.Documents.SingleOrDefault(x => x.Id == item.DocumentId);
                            document.Amount = 1;
                            model.StudentDocuments.Add(document);
                        }
                    }
                }
            }
            return model;        
        }
        public RegistrationApplicationViewModel GetApplicationStatus(Guid studentId)
        {
            var studentInfo = (from student in _db.Students
                               join admissionInfo in _db.AdmissionInformations on student.Id equals admissionInfo.StudentId
                               where student.Id == studentId
                               select new 
                               {
                                    Student = student,
                                    AdmissionInformation = admissionInfo
                               }).SingleOrDefault();
            var model = new RegistrationApplicationViewModel();
            model.StudentId = studentId;
            model.ApplicationNumber = studentInfo.AdmissionInformation.ApplicationNumber;
            model.ApplicationStatus = 1;
            model.IsLogin = true;
            return model;
        }

        public Guid CheckLogin(string username, string password)
        {
            try 
            {
                var admissionInfomation = _db.AdmissionInformations.SingleOrDefault(x => x.ApplicationNumber == username && x.Password == password);
                if (admissionInfomation != null) 
                {
                    return admissionInfomation.StudentId;
                }
            }
            catch 
            {
                return Guid.Empty;   
            }
            return Guid.Empty;
        }

        public bool CheckIfSameAddress(StudentAddress firstAddress, StudentAddress secondAddress) 
        {
            return firstAddress.AddressEn1 == secondAddress.AddressEn1 
                && firstAddress.AddressTh1 == secondAddress.AddressTh1 
                && firstAddress.HouseNumber == secondAddress.HouseNumber
                && firstAddress.Moo == secondAddress.Moo 
                && firstAddress.SoiEn == secondAddress.SoiEn
                && firstAddress.RoadEn == secondAddress.RoadEn
                && firstAddress.CountryId == secondAddress.CountryId
                && firstAddress.ProvinceId == secondAddress.ProvinceId
                && firstAddress.DistrictId == secondAddress.DistrictId
                && firstAddress.SubdistrictId == secondAddress.SubdistrictId
                && firstAddress.StateId == secondAddress.StateId
                && firstAddress.CityId == secondAddress.CityId
                && firstAddress.ZipCode == secondAddress.ZipCode 
                && firstAddress.TelephoneNumber == secondAddress.TelephoneNumber;
        }

        public string  GetApplicationNumber(Guid studentId)
        {
            var number = _db.AdmissionInformations.AsNoTracking()
                                                  .SingleOrDefault(x => x.StudentId == studentId);
            return number == null ? "" : number.ApplicationNumber;  
        }

        public PreviousSchool GetPreviousSchool(long id)
        {
            var previousSchools = _db.PreviousSchools.Include(x => x.Country)
                                                     .Include(x => x.Province)
                                                     .Include(x => x.State)
                                                     .Include(x => x.SchoolGroup)
                                                     .Include(x => x.SchoolTerritory)
                                                     .Include(x => x.SchoolType)
                                                     .IgnoreQueryFilters()
                                                     .SingleOrDefault(x => x.Id == id);
            return previousSchools;
        }

        public List<EducationBackground> GetEducationBackgroundsByCountryId(long id)
        {
            var educationBackgrounds = _db.EducationBackgrounds.Where(x => x.CountryId == id)
                                                               .ToList();
            return educationBackgrounds;
        }

        public AdmissionType GetAdmissionType(long id)
        {
            var type = _db.AdmissionTypes.SingleOrDefault(x => x.Id == id);
            return type;
        }

        public List<AdmissionRound> GetAdmissionRoundByAcademicLevelId(long id)
        {
            var admissionRound = _db.AdmissionRounds.Include(x => x.AcademicLevel)
                                                    .Include(x => x.AdmissionTerm)
                                                    .Where(x => x.AcademicLevelId == id)
                                                    .OrderByDescending(x => x.AdmissionTerm.AcademicYear)
                                                        .ThenByDescending(x => x.AdmissionTerm.AcademicTerm)
                                                        .ThenBy(x => x.Round)
                                                    .ToList();
            return admissionRound;
        }

        public List<AdmissionRound> GetAdmissionRoundByTermId(long termId)
        {
            var admissionRound = _db.AdmissionRounds.Include(x => x.AdmissionTerm)
                                                    .Where(x => x.AdmissionTermId == termId)
                                                    .OrderByDescending(x => x.AdmissionTerm.AcademicYear)
                                                        .ThenByDescending(x => x.AdmissionTerm.AcademicTerm)
                                                        .ThenBy(x => x.Round)
                                                    .ToList();
            return admissionRound;
        }

        public List<AdmissionRound> GetAdmissionRoundByAcademicLevelIdAndTermId(long academicLevelId, long termId = 0)
        {
            var admissionRound = _db.AdmissionRounds.Include(x => x.AdmissionTerm)
                                                    .Where(x => x.AcademicLevelId == academicLevelId
                                                                && (termId == 0 || x.AdmissionTermId == termId))
                                                    .OrderByDescending(x => x.AdmissionTerm.AcademicYear)
                                                        .ThenByDescending(x => x.AdmissionTerm.AcademicTerm)
                                                        .ThenBy(x => x.Round)
                                                    .ToList();
            return admissionRound;
        }

        public Term GetTermByAdmissionRoundId(long admissionRoundId)
        {
            var term = _db.AdmissionRounds.Include(x => x.AdmissionTerm)
                                          .SingleOrDefault(x => x.Id == admissionRoundId)
                                          .AdmissionTerm;
            return term;
        }

        public List<CurriculumVersion> GetCurriculumVersionForAdmissionCurriculum(long termId, long admissionRoundId, long facultyId, long? departmentId = null)
        {
            var term = _db.Terms.SingleOrDefault(x => x.Id == termId);
            if (term == null)
            {
                return new List<CurriculumVersion>();
            }
            
            var admissionCurriculums = _db.AdmissionCurriculums.Where(x => x.AdmissionRoundId == admissionRoundId
                                                                           && x.FacultyId == facultyId
                                                                           && x.DepartmentId == departmentId)
                                                               .Select(x => x.CurriculumVersionId)
                                                               .ToList();

            var versions = _db.CurriculumVersions.Include(x => x.Curriculum)
                                                 .Include(x => x.OpenedTerm)
                                                 .Include(x => x.ClosedTerm)
                                                 .Where(x => !admissionCurriculums.Contains(x.Id)
                                                             && x.Curriculum.FacultyId == facultyId
                                                             && (x.OpenedTerm == null
                                                                 || (x.OpenedTerm.AcademicYear <= term.AcademicYear
                                                                     && x.OpenedTerm.AcademicTerm <= term.AcademicTerm))
                                                             && (x.ClosedTerm == null
                                                                 || (term.AcademicYear <= x.ClosedTerm.AcademicYear
                                                                     && term.AcademicTerm <= x.ClosedTerm.AcademicTerm)))
                                                 .OrderByDescending(x => x.OpenedTerm.AcademicYear)
                                                    .ThenBy(x => x.NameEn)
                                                 .ToList();
                                                 
            return versions;
        }

        public Student GetStudentInformationById(Guid id)
        {
            var student = _db.Students.Include(x => x.AdmissionInformation)
                                          .ThenInclude(x => x.Faculty)
                                      .Include(x => x.AdmissionInformation)
                                          .ThenInclude(x => x.Department)
                                      .Include(x => x.AdmissionInformation)
                                          .ThenInclude(x => x.CurriculumVersion)
                                          .ThenInclude(x => x.Curriculum)
                                      .Include(x => x.AdmissionInformation)
                                          .ThenInclude(x => x.PreviousSchool)
                                              .ThenInclude(x => x.Country)
                                      .Include(x => x.AdmissionInformation)
                                          .ThenInclude(x => x.AdmissionTerm)
                                      .Include(x => x.AdmissionInformation)
                                          .ThenInclude(x => x.AdmissionRound)
                                      .Include(x => x.AdmissionInformation)
                                          .ThenInclude(x => x.Agency)
                                      .Include(x => x.AcademicInformation)
                                          .ThenInclude(x => x.CurriculumVersion)
                                      .Include(x => x.StudentAddresses)
                                          .ThenInclude(x => x.Country)
                                      .Include(x => x.StudentAddresses)
                                          .ThenInclude(x => x.City)
                                      .Include(x => x.StudentAddresses)
                                          .ThenInclude(x => x.State)
                                      .Include(x => x.StudentAddresses)
                                          .ThenInclude(x => x.Province)
                                      .Include(x => x.StudentAddresses)
                                          .ThenInclude(x => x.District)
                                      .Include(x => x.StudentAddresses)
                                          .ThenInclude(x => x.Subdistrict)
                                      .Include(x => x.ParentInformations)
                                          .ThenInclude(x => x.Relationship)
                                      .Include(x => x.Title)
                                      .Include(x => x.Nationality)
                                      .Include(x => x.Religion)
                                      .SingleOrDefault(x => x.Id == id
                                                            && x.StudentStatus == "a");
            return student;
        }

        public Student GetStudentInformationByCode(string code)
        {
            var student = _db.Students.Include(x => x.AdmissionInformation)
                                          .ThenInclude(x => x.Faculty)
                                      .Include(x => x.AdmissionInformation)
                                          .ThenInclude(x => x.Department)
                                      .Include(x => x.AdmissionInformation)
                                          .ThenInclude(x => x.CurriculumVersion)
                                          .ThenInclude(x => x.Curriculum)
                                      .Include(x => x.AdmissionInformation)
                                          .ThenInclude(x => x.PreviousSchool)
                                              .ThenInclude(x => x.Country)
                                      .Include(x => x.AdmissionInformation)
                                          .ThenInclude(x => x.AdmissionTerm)
                                      .Include(x => x.AdmissionInformation)
                                          .ThenInclude(x => x.AdmissionRound)
                                      .Include(x => x.AdmissionInformation)
                                          .ThenInclude(x => x.Agency)
                                      .Include(x => x.AcademicInformation)
                                          .ThenInclude(x => x.CurriculumVersion)
                                      .Include(x => x.StudentAddresses)
                                          .ThenInclude(x => x.Country)
                                      .Include(x => x.StudentAddresses)
                                          .ThenInclude(x => x.City)
                                      .Include(x => x.StudentAddresses)
                                          .ThenInclude(x => x.State)
                                      .Include(x => x.StudentAddresses)
                                          .ThenInclude(x => x.Province)
                                      .Include(x => x.StudentAddresses)
                                          .ThenInclude(x => x.District)
                                      .Include(x => x.StudentAddresses)
                                          .ThenInclude(x => x.Subdistrict)
                                      .Include(x => x.ParentInformations)
                                          .ThenInclude(x => x.Relationship)
                                      .Include(x => x.Title)
                                      .Include(x => x.Nationality)
                                      .Include(x => x.Religion)
                                      .SingleOrDefault(x => x.StudentStatus == "a"
                                                            && (x.Code == code 
                                                                || x.CitizenNumber == code
                                                                || x.Passport == code
                                                                || x.AdmissionInformation.ApplicationNumber.ToLower() == code.ToLower()));
            return student;
        }

        public List<AdmissionExamination> GetAdmissionExaminationByFacultyIdAndTestedAt(long facultyId, DateTime? TestedAt)
        {
            var admissionExaminations = _db.AdmissionExaminations.Include(x => x.AcademicLevel)
                                                                 .Include(x => x.AdmissionRound)
                                                                 .Include(x => x.Faculty)
                                                                 .Include(x => x.Department)
                                                                 .Where(x => x.FacultyId == facultyId)
                                                                 // TestedAt
                                                                 .ToList();
            return admissionExaminations;
        }

        public StudentExemptedExamScore GetStudentExemptedExams(string type, Guid studentId)
        {
            var student = _db.StudentExemptedExamScores.FirstOrDefault(x => studentId == x.StudentId 
                                                                            && x.ExemptedAdmissionExamination.NameEn.Contains(type));
            
            return student;
        }

        public List<AdmissionDocumentGroup> GetStudentDocumentGroupsByCountryId(long academicLevelId, long? facultyId, long? departmentId, long? previousSchoolCountryId)
        {
            var country = _db.Countries.SingleOrDefault(x => x.Id == previousSchoolCountryId)?.NameEn ?? "";
            var isThai = country.ToLower().Contains("thai");
            var studentDocumentGroup = _db.AdmissionDocumentGroups.Where(x => x.AcademicLevelId == academicLevelId
                                                                              && (previousSchoolCountryId == null
                                                                                  || previousSchoolCountryId == 0
                                                                                  || x.IsThai == isThai)
                                                                              && (x.FacultyId == null
                                                                                  || facultyId == null
                                                                                  || facultyId == 0
                                                                                  || x.FacultyId == facultyId)
                                                                              && (x.DepartmentId == null
                                                                                  || departmentId == null
                                                                                  ||  departmentId == 0
                                                                                  || x.DepartmentId == departmentId)
                                                                              && (x.GraduatedCountryId == null
                                                                                  || previousSchoolCountryId == null
                                                                                  || previousSchoolCountryId == 0
                                                                                  || x.GraduatedCountryId == previousSchoolCountryId))
                                                                  .ToList();
            return studentDocumentGroup;
        }

        public bool IsExistIntensiveCourse(long courseId, long? facultyId, long? departmentId)
        {
            var isExist = _db.IntensiveCourses.Any(x => x.CourseId == courseId
                                                        && x.FacultyId == facultyId
                                                        && x.DepartmentId == departmentId);
            return isExist;
        }

        public bool IsDuplicateStudentCodeRange(long id, long academicLevelId, int startedCode, int endedCode)
        {
            var isDuplicate = _db.StudentCodeRanges.Any(x => x.Id != id
                                                             && x.AcademicLevelId == academicLevelId
                                                             && (x.StartedCodeInt <= endedCode
                                                                 && x.EndedCodeInt >= startedCode));
            return isDuplicate;
        }

        public bool IsExistRangeInAdmissionRound(long id, long academicLevelId, long admissionRoundId)
        {
            var isExist = _db.StudentCodeRanges.Any(x => x.Id != id
                                                         && x.AcademicLevelId == academicLevelId
                                                         && x.AdmissionRoundId == admissionRoundId);
            return isExist;
        }

        public bool IsExistCodeCitizenPassportApplicationNumber(string code)
        {
            var isExist = _db.Students.Include(x => x.AdmissionInformation)
                                      .Any(x => x.StudentStatus == "a"
                                               && (x.Code == code 
                                                   || x.CitizenNumber == code
                                                   || x.Passport == code
                                                   || (x.AdmissionInformation != null
                                                       && !string.IsNullOrEmpty(code)
                                                       && x.AdmissionInformation.ApplicationNumber == code.ToUpper())));
            return isExist;
        }

        public bool IsExistStudentCode(string code)
        {
            var isExist = _db.Students.Any(x => x.Code == code
                                                && (x.StudentStatus == "a" 
                                                    || x.StudentStatus == "s"));
            return isExist;
        }

        public bool IsExistStudentId(Guid studentId)
        {
            var isExist = _db.Students.Any(x => x.Id == studentId
                                                && (x.StudentStatus == "a" 
                                                    || x.StudentStatus == "s"));
            return isExist;
        }

        public string GetAdmissionFirstClassDate(long admissionRoundId)
        {
            var adimssionFirstClassDate = _db.AdmissionRounds.SingleOrDefault(x => x.Id == admissionRoundId);
            var adimssionFirstClassDateString = adimssionFirstClassDate?.FirstClassAtText ?? "";
            return adimssionFirstClassDateString;
        }

        public StudentCodeStatusRange GetStudentCodeRange(long admissionRoundId)
        {
            var code = _db.StudentCodeRanges.Where(x => x.AdmissionRoundId == admissionRoundId)
                                            .Select(x => new StudentCodeStatusRange
                                                         {
                                                            StartedCode = x.StartedCode,
                                                            EndedCode = x.EndedCode
                                                         })
                                            .FirstOrDefault();
            return code;
        }

        public void SetStudentVerificationLetter(List<Guid> studentIds, string referenceNumber, DateTime? sentDate)
        {
            var admissionInformations = _db.AdmissionInformations.Where(x => studentIds.Contains(x.StudentId))
                                                                 .ToList();
            admissionInformations.Select(x => {
                                                  x.CheckDated = sentDate;
                                                  x.CheckReferenceNumber = referenceNumber;
                                                  return x;
                                              })
                                 .ToList();
        }

        public void SetStudentReplyVerificationLetter(List<Guid> studentIds, string receivedNumber, DateTime? receivedDate)
        {
            var admissionInformations = _db.AdmissionInformations.Where(x => studentIds.Contains(x.StudentId))
                                                                 .ToList();
            admissionInformations.Select(x => {
                                                  x.ReplyDate = receivedDate;
                                                  x.ReplyReferenceNumber = receivedNumber;
                                                  return x;
                                              })
                                 .ToList();
        }

        public decimal GetIELTSScore(Guid studentId)
        {
            var ielts = _db.StudentExemptedExamScores.Include(x => x.ExemptedAdmissionExamination)
                                                     .Where(x => x.StudentId == studentId
                                                                 && x.ExemptedAdmissionExamination.NameEn.Contains("IELTS"))
                                                     .OrderByDescending(x => x.UpdatedAt)
                                                     .Select(x => x.Score)
                                                     .FirstOrDefault();
            return ielts;
        }

        public List<StudentStatisticByProvinceAndSchoolReportViewModel> GetStudentStatisticByProvinceAndSchoolReport(List<Student> students, Criteria criteria)
        {
            if (criteria.Type == "p")
            {
                var studentGroup = students.GroupBy(x => x.AdmissionInformation?.PreviousSchool?.Province ?? null)
                                           .Select(x => new StudentStatisticByProvinceAndSchoolReportViewModel
                                                        {
                                                            Criteria = criteria,
                                                            Territory = criteria.Language == "en" ? x.FirstOrDefault().AdmissionInformation?.PreviousSchool?.SchoolTerritory?.NameEn 
                                                                                                  : x.FirstOrDefault().AdmissionInformation?.PreviousSchool?.SchoolTerritory?.NameTh,
                                                            Province = criteria.Language == "en" ? x.FirstOrDefault().AdmissionInformation?.PreviousSchool?.Province?.NameEn 
                                                                                                 : x.FirstOrDefault().AdmissionInformation?.PreviousSchool?.Province?.NameTh,
                                                            RegistrationStudentCounts = x.GroupBy(y => y.AcademicInformation.Batch)
                                                                                         .Select(y => new StudentStatisticByProvinceAndSchoolDetail
                                                                                                      {
                                                                                                          Batch = y.Key,
                                                                                                          StudentCount = y.Count()
                                                                                                      })
                                                                                         .OrderBy(y => y.Batch)
                                                                                         .ToList(),
                                                            IntensiveStudentCounts = x.Where(y => y.RegistrationCourses.Any(z => z.Course.IsIntensiveCourse))
                                                                                      .GroupBy(y => y.AcademicInformation.Batch)
                                                                                      .Select(y => new StudentStatisticByProvinceAndSchoolDetail
                                                                                                   {
                                                                                                       Batch = y.Key,
                                                                                                       StudentCount = y.Count()
                                                                                                   })
                                                                                      .OrderBy(y => y.Batch)
                                                                                      .ToList(),
                                                            StudyStudentCounts = x.Where(y => y.RegistrationCourses.Any(z => z.Course.IsIntensiveCourse == false)
                                                                                              && y.RegistrationCourses.Any(z => z.Term?.AcademicYear >= y.AdmissionInformation?.AdmissionTerm?.AcademicYear)
                                                                                              && y.RegistrationCourses.Any(z => z.Term?.AcademicTerm > y.AdmissionInformation?.AdmissionTerm?.AcademicTerm))
                                                                                  .GroupBy(y => y.AcademicInformation.Batch)
                                                                                  .Select(y => new StudentStatisticByProvinceAndSchoolDetail
                                                                                               {
                                                                                                   Batch = y.Key,
                                                                                                   StudentCount = y.Count()
                                                                                               })
                                                                                  .OrderBy(y => y.Batch)
                                                                                  .ToList()
                                                        })
                                                        .ToList();

                return studentGroup;
            }
            else
            {
                var studentGroup = students.Where(x => criteria.Type == "s")
                                           .GroupBy(x => x.AdmissionInformation?.PreviousSchool ?? null)
                                           .Select(x => new StudentStatisticByProvinceAndSchoolReportViewModel
                                                        {
                                                            Criteria = criteria,
                                                            Province = criteria.Language == "en" ? x.FirstOrDefault().AdmissionInformation?.PreviousSchool?.Province?.NameEn 
                                                                                                 : x.FirstOrDefault().AdmissionInformation?.PreviousSchool?.Province?.NameTh,
                                                            SchoolName = criteria.Language == "en" ? x.FirstOrDefault().AdmissionInformation?.PreviousSchool?.NameEn 
                                                                                                   : x.FirstOrDefault().AdmissionInformation?.PreviousSchool?.NameTh,
                                                            RegistrationStudentCounts = x.GroupBy(y => y.AcademicInformation.Batch)
                                                                                         .Select(y => new StudentStatisticByProvinceAndSchoolDetail
                                                                                                      {
                                                                                                          Batch = y.Key,
                                                                                                          StudentCount = y.Count()
                                                                                                      })
                                                                                         .OrderBy(y => y.Batch)
                                                                                         .ToList(),
                                                            IntensiveStudentCounts = x.Where(y => y.RegistrationCourses.Any(z => z.Course.IsIntensiveCourse))
                                                                                      .GroupBy(y => y.AcademicInformation.Batch)
                                                                                      .Select(y => new StudentStatisticByProvinceAndSchoolDetail
                                                                                                   {
                                                                                                       Batch = y.Key,
                                                                                                       StudentCount = y.Count()
                                                                                                   })
                                                                                      .OrderBy(y => y.Batch)
                                                                                      .ToList(),
                                                            StudyStudentCounts = x.Where(y => y.RegistrationCourses.Any(z => z.Course.IsIntensiveCourse == false)
                                                                                              && y.RegistrationCourses.Any(z => z.Term?.AcademicYear >= y.AdmissionInformation?.AdmissionTerm?.AcademicYear)
                                                                                              && y.RegistrationCourses.Any(z => z.Term?.AcademicTerm > y.AdmissionInformation?.AdmissionTerm?.AcademicTerm))
                                                                                  .GroupBy(y => y.AcademicInformation.Batch)
                                                                                  .Select(y => new StudentStatisticByProvinceAndSchoolDetail
                                                                                               {
                                                                                                   Batch = y.Key,
                                                                                                   StudentCount = y.Count()
                                                                                               })
                                                                                  .OrderBy(y => y.Batch)
                                                                                  .ToList()
                                                        })
                                                        .ToList();

                return studentGroup;
            }
        }

        public bool IsExistAdmissionExamination(long admissionRoundId, long facultyId, List<long?> departmentIds = null)
        {
            var isexist = _db.AdmissionExaminations.Any(x => x.AdmissionRoundId == admissionRoundId
                                                             && x.FacultyId == facultyId
                                                             && ((departmentIds != null && departmentIds.Contains(x.DepartmentId ?? 0))
                                                                  || (departmentIds == null && x.DepartmentId == null)));
            return isexist;
        }

        public bool IsExistAdmissionRound(long id, long termId, int round)
        {
            var isExist = _db.AdmissionRounds.Any(x => x.Id != id
                                                       && x.AdmissionTermId == termId
                                                       && x.Round == round);
            return isExist;
        }

        public AdmissionRound GetAdmissionRound(long id)
        {
            var model = _db.AdmissionRounds.IgnoreQueryFilters()
                                           .SingleOrDefault(x => x.Id == id);
            return model;
        }

        public bool IsStudnetBlacklisted(string citizenNumber, string passportNumber, string firstNameEn, string lastNameEn, string firstNameTh, string lastNameTh, DateTime birthDate, int gender)
        {
            var isStudnetBlacklisted = _db.BlacklistedStudents.Where(x => (!string.IsNullOrEmpty(citizenNumber)
                                                                           && x.CitizenNumber == citizenNumber)
                                                                          || (!string.IsNullOrEmpty(passportNumber)
                                                                              && x.Passport == passportNumber)
                                                                          || ((x.FirstNameEn == firstNameEn
                                                                               && x.LastNameEn == lastNameEn)
                                                                              || (x.FirstNameTh == firstNameTh
                                                                                  && x.LastNameTh == lastNameTh)
                                                                              && x.BirthDate == birthDate
                                                                              && x.Gender == gender))
                                                              .Any();
            return isStudnetBlacklisted;
        }

        public List<AdmissionCurriculum> GetAdmissionCurriculumByRoundAndFaculty(long admissionRoundId, long facultyId)
        {
            var models = _db.AdmissionCurriculums.Include(x => x.CurriculumVersion)
                                                 .Include(x => x.AdmissionRound)
                                                 .Where(x => x.AdmissionRoundId == admissionRoundId
                                                             && x.FacultyId == facultyId)
                                                 .ToList();
            return models;
        }

        public bool IsExistAdmissionCurriculum(long admissionRoundId, long facultyId)
        {
            var isExisted = _db.AdmissionCurriculums.Any(x => x.AdmissionRoundId == admissionRoundId
                                                              && x.FacultyId == facultyId);
            return isExisted;
        }

        public ApplicationFormViewModel GetApplicationFormViewModel(Student student)
        {
            var model = new ApplicationFormViewModel();
            model = _mapper.Map<Student,ApplicationFormViewModel>(student);
            model.PersonalEmail = model.PersonalEmail == null ? "N/A" : model.PersonalEmail;
            model.SubdistrictEn = student.StudentAddresses.Any() ? student.StudentAddresses.FirstOrDefault(x => x.Type == "c")?.Subdistrict.NameEn : "N/A";
            model.SubdistrictTh = student.StudentAddresses.Any() ? student.StudentAddresses.FirstOrDefault(x => x.Type == "c")?.Subdistrict.NameTh : "N/A";
            model.DistrictEn = student.StudentAddresses.Any() ? student.StudentAddresses.FirstOrDefault(x => x.Type =="c")?.District.NameEn : "N/A";
            model.DistrictTh = student.StudentAddresses.Any() ? student.StudentAddresses.FirstOrDefault(x => x.Type =="c")?.District.NameTh : "N/A";
            model.ProvinceEn = student.StudentAddresses.Any() ? student.StudentAddresses.FirstOrDefault(x => x.Type == "c")?.Province.NameEn : "N/A";
            model.ProvinceTh = student.StudentAddresses.Any() ? student.StudentAddresses.FirstOrDefault(x => x.Type == "c")?.Province.NameTh : "N/A";
            model.HouseNumber = student.StudentAddresses.Any() ? student.StudentAddresses.FirstOrDefault(x => x.Type == "c")?.HouseNumber : "N/A";
            model.Moo = student.StudentAddresses.Any() ? student.StudentAddresses.FirstOrDefault(x => x.Type == "c")?.Moo : "-";
            model.AddressEn = student.StudentAddresses.Any() ? student.StudentAddresses.FirstOrDefault(x => x.Type == "c").AddressEn1 : "N/A";
            model.AddressTh = student.StudentAddresses.Any() ? (student.StudentAddresses.FirstOrDefault(x => x.Type == "c").AddressTh1 ?? "N/A") : "N/A";
            model.SoiEn = student.StudentAddresses.Any() ? student.StudentAddresses.FirstOrDefault(x => x.Type == "c").SoiEn : "N/A";
            model.SoiTh = student.StudentAddresses.Any() ? student.StudentAddresses.FirstOrDefault(x => x.Type == "c").SoiTh : "N/A";
            model.RoadEn = student.StudentAddresses.Any() ? student.StudentAddresses.FirstOrDefault(x => x.Type == "c").RoadEn : "N/A";
            model.RoadTh = student.StudentAddresses.Any() ? student.StudentAddresses.FirstOrDefault(x => x.Type == "c").RoadTh :"N/A";
            model.ZipCode = student.StudentAddresses.Any() ? student.StudentAddresses.FirstOrDefault(x => x.Type == "c").ZipCode : "N/A";
            model.AddressTelephoneNumber = student.StudentAddresses.Any() ? (student.StudentAddresses.FirstOrDefault(x => x.Type == "c").TelephoneNumber ?? "N/A") : "N/A";

            var TOEFL = GetStudentExemptedExams("TOFEL (IBT)", model.StudentId);
            model.TOEFLIBT = TOEFL == null ? "N/A" : TOEFL.Score.ToString();

            var IELTS = GetStudentExemptedExams("IELTS", model.StudentId);
            model.IELTS = IELTS == null ? "N/A" : IELTS.Score.ToString();

            var SAT = GetStudentExemptedExams("SAT 1 (READING AND WRITING)", model.StudentId);
            model.SAT = SAT == null ? "N/A" : SAT.Score.ToString();
            
            var SATMATH = GetStudentExemptedExams("SAT 1  (MATH)", model.StudentId);
            model.SATMATH = SATMATH == null ? "N/A" : SATMATH.Score.ToString();

            if (student.ParentInformations != null)
            {
                var father = student.ParentInformations.FirstOrDefault(x => x.Relationship.NameEn.ToLower() == "father");
                model.FatherName = father == null ? "N/A" : father.FirstNameEn;
                model.FatherTelephoneNumber = father == null ? "N/A" : father.TelephoneNumber1;
                model.FatherCitizenNumber = father == null ? "N/A" : father.CitizenNumber;

                var mother = student.ParentInformations.FirstOrDefault(x => x.Relationship.NameEn.ToLower() == "mother");
                model.MotherName = mother == null ? "N/A" : mother.FirstNameEn;
                model.MotherTelephoneNumber = mother == null ? "N/A" : mother.TelephoneNumber1;
                model.MotherCitizenNumber =  mother == null ? "N/A" : mother.CitizenNumber;

                var guardian = student.ParentInformations.FirstOrDefault(x => x.IsMainParent);
                model.GuardianName = guardian == null ? "N/A" : guardian.FirstNameEn;
                model.GuardianTelephoneNumber = guardian == null ? "N/A" : guardian.TelephoneNumber1;
                model.GuardianCitizenNumber = guardian == null ? "N/A" : guardian.CitizenNumber;
            }
            
            return model;
        }

        #region Verification
        public int GetVerificationLetterRunningNumber(int year)
        {
            var lastNumber = _db.VerificationLetters.Where(x => x.Year == year)
                                                    .LastOrDefault()?.RunningNumber ?? 0;
            return lastNumber + 1;
        }

        public List<VerificationStudent> GetVerificationStudents(VerificationLetter verificationLetter)
        {
            var students = (from student in _db.Students
                            join admissionInformation in _db.AdmissionInformations on student.Id equals admissionInformation.StudentId
                            join previousSchool in _db.PreviousSchools on admissionInformation.PreviousSchoolId equals previousSchool.Id
                            join schoolGroup in _db.SchoolGroups on previousSchool.SchoolGroupId equals schoolGroup.Id into SchoolGroups
                            from schoolGroup in SchoolGroups.DefaultIfEmpty()
                            join academicInformation in _db.AcademicInformations on student.Id equals academicInformation.StudentId
                            where academicInformation.AcademicLevelId == verificationLetter.AcademicLevelId
                                  && !verificationLetter.VerificationStudents.Select(x => x.StudentId).Contains(student.Id)
                                  && string.IsNullOrEmpty(student.AdmissionInformation.CheckReferenceNumber)
                                  && admissionInformation.CheckDated == null
                                  && student.StudentStatus == "s"
                                  && (verificationLetter.BatchFrom == null || academicInformation.Batch >= verificationLetter.BatchFrom)
                                  && (verificationLetter.BatchTo == null || academicInformation.Batch <= verificationLetter.BatchTo)
                                  && (verificationLetter.AdmissionTermId == null || admissionInformation.AdmissionTermId == verificationLetter.AdmissionTermId)
                                  && (verificationLetter.AdmissionRoundId == null || admissionInformation.AdmissionRoundId == verificationLetter.AdmissionRoundId)
                                  && (verificationLetter.StudentCodeFromInt == 0 || student.CodeInt >= verificationLetter.StudentCodeFromInt)
                                  && (verificationLetter.StudentCodeToInt == 0 || student.CodeInt <= verificationLetter.StudentCodeToInt)
                                  && (verificationLetter.SchoolGroupId == null || schoolGroup.Id == verificationLetter.SchoolGroupId)
                                  && (verificationLetter.PreviousSchoolId == null || previousSchool.Id == verificationLetter.PreviousSchoolId)
                                  select new VerificationStudent
                                         {
                                             StudentId = student.Id,
                                             VerificationLetter = verificationLetter,
                                             Student = student,
                                             PreviousSchoolNameEn = previousSchool.NameEn,
                                             PreviousSchoolNameTh = previousSchool.NameTh
                                         }).OrderBy(x => x.Student.Code)
                                           .ToList();
            return students;
        }

        public VerificationLetter GetVerificationLetter(long id)
        {
            var model = _db.VerificationLetters.Include(x => x.VerificationStudents)
                                                   .ThenInclude(x => x.Student)
                                                   .ThenInclude(x => x.AcademicInformation)
                                               .Include(x => x.SchoolGroup)
                                               .Include(x => x.PreviousSchool)
                                               .Include(x => x.VerificationStudents)
                                                   .ThenInclude(x => x.Student)
                                                   .ThenInclude(x => x.AdmissionInformation)
                                                   .ThenInclude(x => x.PreviousSchool)
                                               .SingleOrDefault(x => x.Id == id);

            if (model.VerificationStudents.Any())
            {
                model.AcademicLevelId = model.VerificationStudents.FirstOrDefault().Student?.AcademicInformation?.AcademicLevelId ?? 0;
            }

            model.VerificationStudents.Select(x => {
                                                       x.PreviousSchoolNameEn = x.Student.AdmissionInformation.PreviousSchool.NameEn;
                                                       x.PreviousSchoolNameTh = x.Student.AdmissionInformation.PreviousSchool.NameTh;
                                                       x.IsChecked = "on";
                                                       return x;
                                                   })
                                      .OrderBy(x => x.Student.Code)
                                      .ToList();
            return model;
        }

        public List<VerificationStudent> GetverificationLetterByVerificationLetterId(long letterId)
        {
            var students = (from verificationStudent in _db.VerificationStudents
                            join student in _db.Students on verificationStudent.StudentId equals student.Id
                            join admissionInfomation in _db.AdmissionInformations on student.Id equals admissionInfomation.StudentId
                            join previousSchool in _db.PreviousSchools on admissionInfomation.PreviousSchoolId equals previousSchool.Id
                            join academicInformation in _db.AcademicInformations on student.Id equals academicInformation.StudentId
                            where verificationStudent.VerificationLetterId == letterId
                            orderby student.Code
                            select new VerificationStudent
                                   {
                                       StudentId = student.Id,
                                       Student = student,
                                       VerificationLetterId = letterId,
                                       PreviousSchoolNameEn = previousSchool.NameEn,
                                       PreviousSchoolNameTh = previousSchool.NameTh,
                                       IsChecked = "on"
                                   }).ToList();
            return students;
        }
        #endregion
    }
}