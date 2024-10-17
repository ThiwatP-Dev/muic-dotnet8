namespace KeystoneLibrary.Models
{
    public class Criteria
    {
        private string _courseCode;
        
        public Criteria()
        {
            CourseIds = new List<long>();
            SectionIds = new List<long>();
        }
        public string PreviousCode { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CodeAndName { get; set; }
        public string CitizenAndPassport { get; set; }
        public string Passport { get; set; }
        public string CitizenNumber { get; set; }
        public string ReferenceNumber { get; set; }
        public string SeatAvailable { get; set; }
        public string GroupWithdrawBy { get; set; }
        public string CertificationType { get; set; }
        public List<long> SelectedInstructors { get; set; }
        public int? AcademicYear { get; set; }
        public int? AcademicTerm { get; set; }
        public string Language { get; set; }
        public int? Floor { get; set; }
        public int? Gender { get; set; }
        public string HasAccount { get; set; }
        public string Position { get; set; }
        public int GraduatedClass { get; set; }
        public string Purpose { get; set; }
        public string EntranceExamResult { get; set; }
        public int? RunningNumber { get; set; }
        public int DocumentYear { get; set; }
        public int? Credit { get; set; }
        public string RoomName { get; set; }
        public int Term { get; set; }
        public string OrderBy { get; set; }
        public string FeeType { get; set; }
        public DateTime? MidtermDate { get; set; }
        public DateTime? FinalDate { get; set; }
        public int? Period { get; set; }
        public string Condition { get; set; }
        public int NumberOfTerms { get; set; }
        public string TransferedGrade { get; set; }
        public int? BirthYear { get; set; }
        public string AccountCode { get; set; }
        public string CreatedBy { get; set; }
        public string InvoiceNumber { get; set; }
        public string InstructorEmails { get; set; }
        public int? ExpectedGraduationTerm { get; set; }
        public int? ExpectedGraduationYear { get; set; }

        // Id
        public long Id { get; set; }
        public long NationalityId { get; set; }
        public long ExemptedAdmissionExaminationId { get; set; }
        public long IncidentId { get; set; }
        public long ApprovedById { get; set; }
        public long QuestionnaireId { get; set; }
        public long AdmissionRoundId { get; set; }
        public long AdmissionTypeId { get; set; }
        public long SponsorId { get; set; }
        public long CurriculumId { get; set; }
        public long CurriculumVersionId { get; set; }
        public long ProbationId { get; set; }
        public long RetireId { get; set; }
        public long ResignReasonId { get; set; }
        public long ScholarshipTypeId { get; set; }
        public long ScholarshipId { get; set; }
        public Guid StudentId { get; set; }
        public long SchoolTypeId { get; set; }
        public long SchoolTerritoryId { get; set; }
        public long SchoolGroupId { get; set; }
        public long PreviousSchoolId { get; set; }
        public long TermId { get; set; }
        public long CampusId { get; set; }
        public long BuildingId { get; set; }
        public long RoomId { get; set; }
        public long RoomtypeId { get; set; }
        public long StudentTypeId { get; set; }
        public long ResidentTypeId { get; set; }
        public List<long> ResidentTypeIds { get; set; } = new List<long>();
        public int ExtendedYear { get; set; }
        public long OldCurriculumId { get; set; }
        public long NewCurriculumId { get; set; }
        public long InstructorId { get; set; }
        public long TeachingTypeId { get; set; }
        public long CountryId { get; set; }
        public long ProvinceId { get; set; }
        public long StateId { get; set; }
        public long PetitionId { get; set; }
        public long HonorId { get; set; }
        public long GradeId { get; set; }
        public List<long> GradeIds { get; set; } = new List<long>();
        public long ConditionId { get; set; }
        public long CourseGroupId { get; set; }
        public long CreditGradeId { get; set; }
        public long NonCreditGradeId { get; set; }
        public long TransferedGradeId { get; set; }
        public long TransferUniversityId { get; set; }
        public long InvoiceId { get; set; }
        public long FeeItemId { get; set; }
        public long TuitionFeeTypeId { get; set; }
        public List<long> FeeGroupIds { get; set; }
        public List<long> PaymentMethodIds { get; set; }
        public long FeeGroupId { get; set; }
        public List<long> FeeItemIds { get; set; } = new List<long>();
        public List<long> RoomIds { get; set; }
        public List<long> InstructorIds { get; set; }
        public List<long> CampusIds { get; set; }
        public long TermTypeId { get; set; }
        public long StudentFeeTypeId { get; set; }
        public long StudentFeeGroupId { get; set; }
        public List<long> StudentFeeGroupIds { get; set; }
        public long CustomCourseGroupId { get; set; }
        public List<long> QuestionGroupIds { get; set; }
        public List<long> StudentFeeTypeIds { get; set; }
        public List<long> TermIds { get; set; }
        public List<long> NationalityIds { get; set; }
        public List<long> AdmissionTypeIds { get; set; }
        public long DistributionMethodId { get; set; }
        public long AbilityId { get; set; }
        public long RegistrationTermId { get; set; }
        public long ConcentrationId { get; set; }
        public long MinorId { get; set; }

        public long InstructorTypeId { get; set; }
        public List<long> StudentTypeIds { get; set; }
        public long FilterCourseGroupId { get; set; }

        // Faculty & Department
        public long AcademicLevelId { get; set; }
        public long AcademicProgramId { get; set; }
        public List<long> AcademicLevelIds { get; set; }
        public long FacultyId { get; set; }
        public long CourseFacultyId { get; set; }
        public long DepartmentId { get; set; }
        public List<long> DepartmentIds { get; set; } = new List<long>();
        public long ExpectedGraduationTermId { get; set; }
        public List<long> FacultyIds { get; set; } = new List<long>();

        // Course & Section
        public string CourseCode 
        { 
            get
            {
                return _courseCode;
            }
            set
            {
                _courseCode = value?.Replace(" ", string.Empty);
            } 
        } // act, cs, etc.
        public string CourseName { get; set; } // act, cs, etc.
        public long CourseId { get; set; }
        public List<long> CourseIds { get; set; } = new List<long>();
        public long CourseRateId { get; set; }
        public string SectionStatus { get; set; } // all, yes, no
        public string SectionDetail { get; set; } // "", true, false
        public string SectionSlot { get; set; } // "", true, false
        public long SectionId { get; set; }
        public List<long> SectionIds { get; set; }
        public string SectionNumber { get; set; }
        public List<string> SectionNumbers { get; set; }
        public List<long> FacilityIds { get; set; }
        public int CourseGroupAmount { get; set; }
        public int? StudentLessThan { get; set; }
        public string SectionType { get; set; }

        // Range
        public string CreatedFrom { get; set; }
        public string CreatedTo { get; set; }
        public int? CourseNumberFrom { get; set; }
        public int? CourseNumberTo { get; set; }
        public int? SectionFrom { get; set; }
        public int? SectionTo { get; set; }
        public decimal? GPAFrom { get; set; }
        public decimal? GPATo { get; set; }
        public string StudentCode { get; set; }
        public int? StudentCodeFrom { get; set; }
        public int? StudentCodeTo { get; set; }
        public string StartedAt { get; set; }
        public string EndedAt { get; set; }
        public int? StartYear { get; set; }
        public int? EndYear { get; set; }
        public int? StartTerm { get; set; }
        public int? EndTerm { get; set; }
        public long StartTermId { get; set; }
        public long EndTermId { get; set; }
        public DateTime? RequestedFrom { get; set; }
        public DateTime? RequestedTo { get; set; }
        public DateTime? UpdatedFrom { get; set; }
        public DateTime? UpdatedTo { get; set; }
        public string UpdatedFromText { get; set; }
        public string UpdatedToText { get; set; }
        public int YearFrom { get; set; }
        public int? MonthFrom { get; set; }
        public int YearTo { get; set; }
        public int? MonthTo { get; set; }
        public int Batch { get; set; }
        public List<int> Batches { get; set; }
        public int? StartStudentBatch { get; set; }
        public int? EndStudentBatch { get; set; }
        public int? CapacityFrom { get; set; }
        public int? CapacityTo { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int TimeSlot { get; set; }
        public string Date { get; set; }
        public DateTime DateCheck { get; set; }
        public int Amount { get; set; }
        public int? CreditFrom { get; set; }
        public int? CreditTo { get; set; }
        public DateTime? ReceiptDateFrom { get; set; }
        public DateTime? ReceiptDateTo { get; set; }
        public DateTime? PaidDateFrom { get; set; }
        public DateTime? PaidDateTo { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public int? MinimumCreditFrom { get; set; }
        public int? MaximumCreditFrom { get; set; }
        public int? MinimumCreditTo { get; set; }
        public int? MaximumCreditTo { get; set; }

        // Boolean
        public bool? IsThai { get; set; } // true = Thai, false = non-Thai, null = all
        public bool NonRegistration { get; set; }
        public bool IsUrgent { get; set; }
        public bool IsShowByTime { get; set; }
        public bool IsFinishedRegistration { get; set; }
        public bool IsWithdrawal { get; set; }
        public bool IsWaitingToSubmit { get; set; }
        public string IsSpeacialCase { get; set; }
        public string IsSubmitted { get; set; }
        public string IsReceived { get; set; }
        public string IsAthlete { get; set; }
        public string IsEvening { get; set; }
        public string IsLatePayment { get; set; }
        public string IsApproved { get; set; }
        public string IsLocked { get; set; }
        public string IsAllowLecture { get; set; }
        public string IsClosed { get; set; }
        public string IsCancel { get; set; }
        public string IsMakeUp { get; set; }
        public string IsOnline { get; set; }
        public string HaveMidterm { get; set; }
        public string HaveFinal { get; set; }
        public string IsCheating { get; set; }
        public string IsInvestigation { get; set; }
        public string IsRegistrationApprove { get; set; }
        public string IsPaidAdmissionFee { get; set; }
        public string IsPaidAcceptanceLetterFee { get; set; }
        public string HaveCode { get; set; }
        public string IsSetTuitionFee { get; set; }
        public string IsRequested { get; set; }
        public string IsPublished { get; set; }
        public string IsNoStudent { get; set; }
        public string IsJointSection { get; set; }
        public string IsGradePublished { get; set; }
        public string IsGradeNotPass { get; set; }
        public string IsGhostSection { get; set; }
        public string IsOutboundSection { get; set; }
        public string IsTaken { get; set; }
        public bool IsOfficial { get; set; }
        public string IsPaymentSucceeded { get; set; }
        public string IsGraduated { get; set; }
        public string IsRecalcScore { get; set; }
        public bool IsShowPercentage { get; set; }
        public bool IsShowPaymentStatus { get; set; } = true;
        public bool IsShowUnpaidStudent { get; set; } = true;
        public bool IsShowWithdrawStudent { get; set; } = true;

        // Type & Status
        public string Active { get; set; }
        public string CalculateType { get; set; }
        public string TransferType { get; set; }
        public long ExaminationTypeId { get; set; }
        public string ExaminationType { get; set; } // Midterm, Final
        public string ResponseType { get; set; }
        public string Type { get; set; }
        public string PrintStatus { get; set; }
        public string UrgentStatus { get; set; }
        public string PaymentStatus { get; set; }
        public string Status { get; set; }
        public string StudentStatus { get; set; }
        public string CardStatus { get; set; }
        public string ImageStatus { get; set; }
        public string PaidStatus { get; set; } = "false";
        public string MaintenanceType { get; set; }
        public string EffectivedStatus { get; set; }
        public string SenderType { get; set; }
        public string CreditType { get; set; }
        public string SortBy { get; set; }
        public string SlotType { get; set; }
        public List<string> StudentStatuses { get; set; }
        public string FinancialType { get; set; }
        public string ReceiptInvoiceType { get; set; }
        public string RequestType { get; set; }
        public string Channel { get; set; }

        public string IsExcludeBalanceInvoice { get; set; } = "true";
        public string InvoiceType { get; set; } = "All";
        public string InvoiceRefundType { get; set; } = "All";
        public bool IsGroupStudent { get; set; }
        

        //For Fee Report
        public bool IsQueryPaymentMethod { get; set; } = false;
        public bool IsQueryReceipt { get; set; } = false;
        public bool IsConvertScholarAsAnotherFeeType { get; set; } = false;

        public bool IsIncludeUnConfirm { get; set; } = true;

    }
}