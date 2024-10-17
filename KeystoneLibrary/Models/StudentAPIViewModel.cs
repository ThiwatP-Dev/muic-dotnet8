namespace KeystoneLibrary.Models
{
    public class StudentResultApiViewModel
    {        
        public List<StudentSuccessViewModel> Success { get; set; }
        public List<StudentAPIViewModel> Duplicate { get; set; }
        public List<StudentFailViewModel> Fail { get; set; }
    }

    public class SaveStudentsViewModel
    {
        public long AcademicLevelId { get; set; }
        public int AcademicTerm { get; set; }
        public int AcademicYear { get; set; }
        public int Round { get; set; }
        public List<StudentAPIViewModel> Students { get; set; }
    }

    public class StudentInfoAPIViewModel
    {
        public string Title { get; set; }
        public string FirstNameEn { get; set; }
        public string MidNameEn { get; set; }
        public string LastNameEn { get; set; }
        public string FirstNameTh { get; set; }
        public string LastNameTh { get; set; }
        public int Gender { get; set; }
        public string Race { get; set; }
        public string Nationality { get; set; }
        public string Religion { get; set; }
        public string BirthDate { get; set; }
        public string BirthCountry { get; set; }
        public string CitizenNumber { get; set; }
        public string Passport { get; set; }
        public string PassportExpiredAt { get; set; }
        public string PassportIssueAt { get; set; }
        public string Email { get; set; }
        public string PersonalEmail { get; set; }
        public string TelephoneNumber1 { get; set; }
        public string TelephoneNumber2 { get; set; }
        public string ResidentType { get; set; }
        public string StudentFeeType { get; set; }
        public int Batch { get; set; }
        public string StudentGroup { get; set; }
        public string Faculty { get; set; }
        public string Department { get; set; }
        public string AcademicProgram { get; set; }
        public string AdvisorCode { get; set; }
        public int AdmissionTerm { get; set; }
        public int AdmissionYear { get; set; }
        public string AdmissionDate { get; set; }
        public string AlreadyExist { get; set; }
    }
    public class StudentAPIViewModel : StudentInfoAPIViewModel
    {
        public List<StudentAddressViewModel> StudentAddresses { get; set; }
        public List<ParentInformationViewModel> ParentInformations { get; set; }
    }

    public class StudentAddressViewModel
    {
        public string HouseNumber { get; set; }
        public string AddressTh1 { get; set; }
        public string AddressTh2 { get; set; }
        public string Moo { get; set; }
        public string SoiTh { get; set; }
        public string SoiEn { get; set; }
        public string Country { get; set; }
        public string Province { get; set; }
        public string District { get; set; }
        public string Subdistrict { get; set; }
        public string ZipCode { get; set; }
        public string TelephoneNumber { get; set; }
        public string Type { get; set; }
        public string AddressEn1 { get; set; }
        public string AddressEn2 { get; set; }
        public string RoadEn { get; set; }
        public string RoadTh { get; set; }
        public string Comment { get; set; }
    }
    public class ParentInformationViewModel
    {
        public string Relationship { get; set; }
        public string Email { get; set; }
        public string TelephoneNumber1 { get; set; }
        public string TelephoneNumber2 { get; set; }
        public string AddressEn { get; set; }
        public string AddressTh { get; set; }
        public string Country { get; set; }
        public string Province { get; set; }
        public string District { get; set; }
        public string Subdistrict { get; set; }
        public string ZipCode { get; set; }
        public string CitizenNumber { get; set; }
        public string FirstNameEn { get; set; }
        public string MidNameEn { get; set; }
        public string MidNameTh { get; set; }
        public string LastNameEn { get; set; }
        public string FirstNameTh { get; set; }
        public string LastNameTh { get; set; }
        public string IsEmergencyContact { get; set; }
        public string IsMainParent { get; set; }
        public string Passport { get; set; }
        public string Comment { get; set; }
    }

    public class StudentSuccessViewModel : StudentInfoAPIViewModel
    {
        public StudentSuccessViewModel() { }
        public StudentSuccessViewModel(string studentCode, StudentInfoAPIViewModel model)
        {
            StudentCode = studentCode;
            Title = model.Title;
            FirstNameEn = model.FirstNameEn;
            FirstNameTh = model.FirstNameTh;
            MidNameEn = model.MidNameEn;
            LastNameEn = model.LastNameEn;
            LastNameTh = model.LastNameTh;
            Gender = model.Gender;
            Race = model.Race;
            Nationality = model.Nationality;
            Religion = model.Religion;
            BirthDate = model.BirthDate;
            BirthCountry = model.BirthCountry;
            CitizenNumber = model.CitizenNumber;
            Passport = model.Passport;
            PassportExpiredAt = model.PassportExpiredAt;
            PassportIssueAt = model.PassportIssueAt;
            Email = model.Email;
            PersonalEmail = model.PersonalEmail;
            TelephoneNumber1 = model.TelephoneNumber1;
            TelephoneNumber2 = model.TelephoneNumber2;
            ResidentType = model.ResidentType;
            StudentFeeType = model.StudentFeeType;
            Batch = model.Batch;
            StudentGroup = model.StudentGroup;
            Faculty = model.Faculty;
            Department = model.Department;
            AcademicProgram = model.AcademicProgram;
            AdvisorCode = model.AdvisorCode;
            AdmissionTerm = model.AdmissionTerm;
            AdmissionYear = model.AdmissionYear;
            AdmissionDate = model.AdmissionDate;
        }

        public string StudentCode { get; set; }
        public StudentAddressResuleViewModel StudentAddress { get; set; }
        public StudentParentResultViewModel ParentInformation { get; set; }
    }

    public class StudentAddressResuleViewModel
    {
        public List<StudentAddressViewModel> Success { get; set;}
        public List<StudentAddressViewModel> Fail { get; set;}
    }    

    public class StudentParentResultViewModel
    {
        public List<ParentInformationViewModel> Success { get; set; }
        public List<ParentInformationViewModel> Fail { get; set; }
    }
    public class StudentFailViewModel
    {
        public StudentAPIViewModel Student { get; set; }
        public string Comment { get; set; }
    }

    public class SaveStudentsViewModelV2
    {
        [JsonProperty("Student Group")]
        public string StudentGroupCode { get; set; }

        [JsonProperty("admission_type")]
        public string AdmissionTypeNameEn { get; set; }

        [JsonProperty("Resident Type")]
        public string ResidentTypeNameEn { get; set; }

        [JsonProperty("Student Fee Type")]
        public string StudentFeeTypeNameEn { get; set; }

        [JsonProperty("Std Fee Group")]
        public string StudentFeeGroupName { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("academic_year")]
        public string AcademicYearTerm { get; set; }

        [JsonProperty("program_of_study")]
        public string CurriculumVersionCode { get; set; }

        [JsonProperty("title")]
        public string TitleNameEn { get; set; }

        [JsonProperty("name_eng")]
        public string FirstNameEn { get; set; }

        [JsonProperty("midname_eng")]
        public string MidNameEn { get; set; }

        [JsonProperty("lastname_eng")]
        public string LastNameEn { get; set; }

        [JsonProperty("native_first_name")]
        public string FirstNameTh { get; set; }

        [JsonProperty("native_mid_name")]
        public string MidNameTh { get; set; }

        [JsonProperty("native_last_name")]
        public string LastNameTh { get; set; }

        [JsonProperty("thai_citizen_identification_number")]
        public string CitizenNumber { get; set; }

        [JsonProperty("passport_number")]
        public string Passport { get; set; }

        [JsonProperty("mobile_phone_number")]
        public string TelephoneNumber1 { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }

        [JsonProperty("nationality")]
        public string NationalityNameEn { get; set; }

        [JsonProperty("race")]
        public string RaceNameEn { get; set; }

        [JsonProperty("Track English")]
        public string SpecializationGroupCode1 { get; set; }

        [JsonProperty("Track Math")]
        public string SpecializationGroupCode2 { get; set; }

        [JsonProperty("marital_status")]
        public string MaritalStatus { get; set; }

        [JsonProperty("date_of_birth")]
        public DateTime BirthDate { get; set; }

        [JsonProperty("country_of_birth")]
        public string BirthCountryNameEn { get; set; }

        [JsonProperty("mailing_address")]
        public string CurrentAddressEn1 { get; set; }

        [JsonProperty("mailing_country")]
        public string CurrentAddressCountryNameEn { get; set; }

        [JsonProperty("mailing_street")]
        public string CurrentAddressRoadEn { get; set ; }

        [JsonProperty("mailing_province")]
        public string CurrentAddressProvinceNameEn { get; set; }
        
        [JsonProperty("mailing_zipcode")]
        public string CurrentAddressZipCode { get; set; }

        [JsonProperty("mailing_e_mail")]
        public string PersonalEmail { get; set; }

        [JsonProperty("mailling_phone")]
        public string CurrentAddressTelephoneNumber { get; set; }

        [JsonProperty("current_address")]
        public string PermanentAddressEn1 { get; set; }

        [JsonProperty("current_building")]
        public string PermanentAddressEn2 { get; set; }

        [JsonProperty("current_country")]
        public string PermanentAddressCountryNameEn { get; set; }

        [JsonProperty("current_street")]
        public string PermanentAddressRoadEn { get; set; }

        [JsonProperty("current_sub_district")]
        public string PermanentAddressSubdistrictNameEn { get; set; }

        [JsonProperty("current_district")]
        public string PermanentAddressDistrictNameEn { get; set; }

        [JsonProperty("current_province")]
        public string PermanentAddressProvinceNameEn { get; set; }

        [JsonProperty("current_zipcode")]
        public string PermanentAddressZipCode { get; set; }

        [JsonProperty("current_home_phone")]
        public string PermanentAddressTelephoneNumber { get; set; }
        
        [JsonProperty("current_mobile_phone")]
        public string TelephoneNumber2 { get; set; }

        [JsonProperty("current_e_mail")]
        public string PersonalEmail2 { get; set; }

        [JsonProperty("emergency_name")]
        public string ParentFirstNameEn { get; set; }

        [JsonProperty("emergency_lastname")]
        public string ParentLastNameEn { get; set; }

        [JsonProperty("emergency_country_code")]
        public string ParentCountryCode { get; set; }

        [JsonProperty("emergency_contact")]
        public string ParentTelephoneNumber1 { get; set; }

        [JsonProperty("emergency_relationship")]
        public string RelationshipNameEn { get; set; }

        [JsonProperty("emergency_address")]
        public string ParentAddressEn { get; set; }

        [JsonProperty("emergency_country")]
        public string ParentAddressCountryNameEn { get; set; }

        [JsonProperty("emergency_street")]
        public string ParentAddressRoadEn { get; set; }

        [JsonProperty("emergency_province")]
        public string ParentAddressProvinceNameEn { get; set; }

        [JsonProperty("emergency_zipcode")]
        public string ParentAddressZipCode { get; set; }

        [JsonProperty("emergency_e_mail")]
        public string ParentEmail { get; set; }

        [JsonProperty("check_ref_number")]
        public string CheckReferenceNumber { get; set; }
    }

    public class UpdateStudentAPIViewModel
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("fname_th")]
        public string FNameTH { get; set; }

        [JsonProperty("mname_th")]
        public string MNameTH { get; set; }

        [JsonProperty("lname_th")]
        public string LNameTH { get; set; }

        [JsonProperty("fname_en")]
        public string FNameEN { get; set; }

        [JsonProperty("mname_en")]
        public string MNameEN { get; set; }

        [JsonProperty("lname_en")]
        public string LNameEN { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }

        [JsonProperty("dob")]
        public string DOB { get; set; }

        [JsonProperty("nation")]
        public string Nation { get; set; }

        [JsonProperty("race")]
        public string Race { get; set; }

        [JsonProperty("thai_id_no")]
        public string ThaiIDNo { get; set; }

        [JsonProperty("passport_no")]
        public string PassportNo { get; set; }

        [JsonProperty("passport_issue_date")]
        public string PassportIssueDate { get; set; }

        [JsonProperty("passport_expiry_date")]
        public string PassportExpiryDate { get; set; }

        [JsonProperty("birth_country")]
        public string BirthCountry { get; set; }

        [JsonProperty("birth_province")]
        public string BirthProvince { get; set; }

        [JsonProperty("birth_state")]
        public string BirthState { get; set; }

        [JsonProperty("birth_city")]
        public string BirthCity { get; set; }

        [JsonProperty("religion")]
        public string Religion { get; set; }

        [JsonProperty("marital_status")]
        public string MaritalStatus { get; set; }

        [JsonProperty("native_lang")]
        public string NativeLang { get; set; }

        [JsonProperty("bank_acc_no")]
        public string BankAccNo { get; set; }

        [JsonProperty("email1")]
        public string Email1 { get; set; }

        [JsonProperty("email2")]
        public string Email2 { get; set; }

        [JsonProperty("personel_email")]
        public string PersonelEmail { get; set; }

        [JsonProperty("facebook")]
        public string Facebook { get; set; }

        [JsonProperty("line")]
        public string Line { get; set; }

        [JsonProperty("tel1")]
        public string Tel1 { get; set; }

        [JsonProperty("tel2")]
        public string Tel2 { get; set; }

        [JsonProperty("other")]
        public string Other { get; set; }

        [JsonProperty("student_group")]
        public string StudentGroup { get; set; }

        [JsonProperty("admission_type")]
        public string AdmissionType { get; set; }

        [JsonProperty("resident_type")]
        public string ResidentType { get; set; }

        [JsonProperty("student_fee_type")]
        public string StudentFeeType { get; set; }

        [JsonProperty("student_fee_group")]
        public string StudentFeeGroup { get; set; }

        [JsonProperty("updatedBy")]
        public string UpdatedBy { get; set; }

        [JsonProperty("check_ref_number")]
        public string CheckReferenceNumber { get; set; }
    }

    public class UpdateStudentAddressViewModel
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("address1_th")]
        public string Address1Th { get; set; }

        [JsonProperty("address2_th")]
        public string Address2Th { get; set; }

        [JsonProperty("address1_en")]
        public string Address1En { get; set; }

        [JsonProperty("address2_en")]
        public string Address2En { get; set; }

        [JsonProperty("house_no")]
        public string HouseNo { get; set; }

        [JsonProperty("moo")]
        public string Moo { get; set; }

        [JsonProperty("soi_th")]
        public string SoiTh { get; set; }

        [JsonProperty("soi_en")]
        public string SoiEn { get; set; }

        [JsonProperty("road_th")]
        public string RoadTh { get; set; }

        [JsonProperty("road_en")]
        public string RoadEn { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("province")]
        public string Province { get; set; }

        [JsonProperty("district")]
        public string District { get; set; }

        [JsonProperty("sub_district")]
        public string SubDistrict { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("zip")]
        public string Zip { get; set; }

        [JsonProperty("tel")]
        public string Tel { get; set; }

        [JsonProperty("updatedBy")]
        public string UpdatedBy { get; set; }
    }

    public class UpdateStudentParentViewModel
    {
        [JsonProperty("relationship")]
        public string Relationship { get; set; }

        [JsonProperty("fname_th")]
        public string FNameTh { get; set; }

        [JsonProperty("mname_th")]
        public string MNameTh { get; set; }

        [JsonProperty("sname_th")]
        public string SNameTh { get; set; }

        [JsonProperty("fname_en")]
        public string FNameEn { get; set; }

        [JsonProperty("mname_en")]
        public string MNameEn { get; set; }

        [JsonProperty("sname_en")]
        public string SNameEn { get; set; }

        [JsonProperty("citizen_no")]
        public string CitizenNo { get; set; }

        [JsonProperty("passport")]
        public string Passport { get; set; }

        [JsonProperty("tel")]
        public string Tel { get; set; }

        [JsonProperty("address_th")]
        public string AddressTh { get; set; }

        [JsonProperty("address_en")]
        public string AddressEn { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("province")]
        public string Province { get; set; }

        [JsonProperty("district")]
        public string District { get; set; }

        [JsonProperty("subdistrict")]
        public string Subdistrict { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("zip")]
        public string Zip { get; set; }

        [JsonProperty("main_parent")]
        public string MainParent { get; set; }

        [JsonProperty("for_emergency")]
        public string ForEmergency { get; set; }

        [JsonProperty("updatedBy")]
        public string UpdatedBy { get; set; }
    }

    public class UpdateStudentStatusViewModel
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("effectdate")]
        public string EffectDate { get; set; }

        [JsonProperty("remark")]
        public string Remark { get; set; }

        [JsonProperty("updatedBy")]
        public string UpdatedBy { get; set; }

        // Additional fields for status "g", "g1", "g2"
        [JsonProperty("graduate_academic_year")]
        public string GraduateAcademicYear { get; set; }
    }
}