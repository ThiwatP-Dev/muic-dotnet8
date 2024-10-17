using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Models
{
    public class RegistrationViewModel
    {
        private string _firstName;
        private string _lastName;
        private string _studentFeeType;
        private string _studentFeeGroup;
        private string _nationality;
        private string _program;
        private string _admissionType;
        private string _faculty;
        private string _department;
        private string _curriculumVersion;
        private string _minor;
        private string _concentration;
        private string _registrationSlot;
        private string _registrationRemark;
        private string _scholarshipProfile;
        private string DefaultString = "N/A";
        
        public Guid StudentId { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public long AcademicLevelId { get; set; }
        public long TermId { get; set; }
        public int RegistrationRound { get; set; }
        public string TermText { get; set; }
        public string Advisor { get; set; }
        public string FirstName 
        {
            get
            {
                return _firstName ?? DefaultString;
            }
            set
            {
                _firstName = value;
            }
        }

        public string LastName
        {
            get
            {
                return _lastName ?? DefaultString;
            }
            set
            {
                _lastName = value;
            }
        }

        public string StudentFeeType
        {
            get
            {
                return _studentFeeType ?? DefaultString;
            }
            set
            {
                _studentFeeType = value;
            }
        }

        public string StudentFeeGroup
        {
            get
            {
                return _studentFeeGroup ?? DefaultString;
            }
            set
            {
                _studentFeeGroup = value;
            }
        }

        public string Nationality
        {
            get
            {
                return _nationality ?? DefaultString;
            }
            set
            {
                _nationality = value;
            }
        }
        
        public string Program
        {
            get
            {
                return _program ?? DefaultString;
            }
            set
            {
                _program = value;
            }
        }

        public string AdmissionType
        {
            get
            {
                return _admissionType ?? DefaultString;
            }
            set
            {
                _admissionType = value;
            }
        }

        public string Faculty
        {
            get
            {
                return _faculty ?? DefaultString;
            }
            set
            {
                _faculty = value;
            }
        }
        public string Department
        {
            get
            {
                return _department ?? DefaultString;
            }
            set
            {
                _department = value;
            }
        }

        public string CurriculumVersion
        {
            get
            {
                return _curriculumVersion ?? DefaultString;
            }
            set
            {
                _curriculumVersion = value;
            }
        }

        public string Minor
        {
            get
            {
                return _minor ?? DefaultString;
            }
            set
            {
                _minor = value;
            }
        }
        public string Concentration
        {
            get
            {
                return _concentration ?? DefaultString;
            }
            set
            {
                _concentration = value;
            }
        }

        public long RegistrationTermId { get; set; }
        public decimal GPA { get; set; } 
        public int MaximumCredit { get; set; }
        public int MinimumCredit { get; set; }
        public int AccumulativeCredit { get; set; }
        public int AccumulativeRegistrationCredit { get; set; }
        public string RegistrationSlot
        {
            get
            {
                return _registrationSlot ?? DefaultString;
            }
            set
            {
                _registrationSlot = value;
            }
        }

        public string RegistrationRemark
        {
            get
            {
                return _registrationRemark ?? DefaultString;
            }
            set
            {
                _registrationRemark = value;
            }
        }

        public string ScholarshipProfile 
        {
            get
            {
                return _scholarshipProfile ?? DefaultString;
            }
            set
            {
                _scholarshipProfile = value;
            }
        }

        public bool IsAllowRegistration { get; set; }
        public bool IsAllowPayment { get; set; }
        public bool IsAllowSignIn { get; set; }
        public bool IsFinishedRegistration { get; set; }
        public bool IsGraduating { get; set; }
        public bool IsRetired { get; set; }
        public bool IsReEntered { get; set; }
        public bool IsAdvised { get; set; }
        public bool IsMaintainedStatus { get; set; }
        public bool IsResigned { get; set; }
        public int TotalCredit { get; set; }
        public List<RegistrationCourse> Registrations { get; set; }
        public List<ScheduleViewModel> Schedules { get; set; }
        public string RegistrationScheduleJsonData { get; set; }
        public List<Receipt> Receipts { get; set; }
        public List<Invoice> Invoices { get; set; }
        public List<AddingViewModel> AddingResults { get; set; }
        public string StudentStatus { get; set; }
        public string RegistrationSubmissionRequest
        {
            get
            {
                var courseSections = AddingResults == null ? new List<string>() : AddingResults.Select(x => $"{x.CourseCode},{x.SectionNumber}");
                var courseSectionRequest = string.Join(";", courseSections);
                return courseSectionRequest;
            }
        }
        
        public CreditLoadInformationViewModel CreditLoadInformation { get; set; }
        public List<StudentRegistrationCourseViewModel> StudentRegistrationCourseViewModels { get; set; } = new List<StudentRegistrationCourseViewModel>();
        public StudentRegistrationCoursesViewModel StudentRegistrationCoursesViewModels { get; set; } = new StudentRegistrationCoursesViewModel();
    }

    public class AddingViewModel
    {
        public long RegistrationCourseId { get; set; }
        public long CourseId { get; set; }
        public long TermId { get; set; }
        public string CourseCode { get; set; }
        public string CourseCodeAndName { get; set; }
        public string MainInstructor { get; set; }
        public int Credit { get; set; }
        public string CreditText { get; set; }
        public int RegistrationCredit { get; set; }
        public int PaymentCredit { get; set; }
        public long SectionId { get; set; }
        public string SectionNumber { get; set; }
        public bool IsPaid { get; set; }
        public List<RefundDetail> RefundItems { get; set; }
        public string Status { get; set; }
        public List<Section> Sections { get; set; }
        public SelectList SectionSelectList
        {
            get
            {
                if (Sections != null && Sections.Any())
                {
                    var sections = Sections.Select(x => new SelectListItem
                                                    {
                                                        Text = x.Number,
                                                        Value = x.Id.ToString()
                                                    });
                    return new SelectList(sections, "Value", "Text");
                }
                return new SelectList(Enumerable.Empty<SelectListItem>());
            }
        }
    }

    public class RefundDetail
    {
        public long CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public long SectionId { get; set; }
        public string SectionNumber { get; set; }
        public long FeeItemId { get; set; }
        public string FeeItemName { get; set; }
        public decimal Amount { get; set; }
        public decimal RefundPercent { get; set; }
        public decimal RefundAmount
        {
            get
            {
                return RefundPercent == 0 ? 0 : Amount * (RefundPercent / 100);
            }
        }
        
        public long ReceiptId { get; set; }
        public long ReceiptItemId { get; set; }
    }

    public class UpdateWhiteListViewModel
    {
        public string Batches { get; set; }
        public string CurriculumCodes { get; set; }
        public string CurriculumVersionIds { get; set; }
        public string FacultyIds { get; set; }
        public string DepartmentIds { get; set; }
        public string StudentCodes { get; set; }
        public long? MainInstructorId { get; set; }
        public string MinorIds { get; set; }
        public string Remark { get; set; }
    }
}