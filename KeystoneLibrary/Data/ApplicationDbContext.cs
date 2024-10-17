using KeystoneLibrary.Models.DataModels.MasterTables;
using Microsoft.EntityFrameworkCore;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Curriculums;
using KeystoneLibrary.Models.DataModels.Withdrawals;
using KeystoneLibrary.Models.DataModels.Fee;
using KeystoneLibrary.Models.DataModels.Scholarship;
using KeystoneLibrary.Models.DataModels.Questionnaire;
using KeystoneLibrary.Models.DataModels.Admission;
using KeystoneLibrary.Models.DataModels.Profile;
using KeystoneLibrary.Models.DataModels.Prerequisites;
using KeystoneLibrary.Models.DataModels.Petitions;
using KeystoneLibrary.Models.DataModels.Advising;
using KeystoneLibrary.Models.DataModels.Authentication;
using KeystoneLibrary.Models.DataModels.Logs;
using KeystoneLibrary.Models.DataModels.Graduation;
using KeystoneLibrary.Models.DataModels.Configurations;
using KeystoneLibrary.Models.USpark;
using System.Security.Claims;

namespace KeystoneLibrary.Data
{
    public class ApplicationDbContext : DbContext
    {
        private HttpContext _httpContext { get; }

        public ApplicationDbContext() { }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
                                    IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _httpContext = httpContextAccessor.HttpContext;
            this.Database.SetCommandTimeout(3600);
        }

        public DbSet<BankBranch> BankBranches { get; set; }
        public DbSet<Configuration> Configurations { get; set; }
        public DbSet<DistributionMethod> DistributionMethods { get; set; }
        public DbSet<MaintenanceFee> MaintenanceFees { get; set; }
        public DbSet<Incident> Incidents { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<Relationship> Relationships { get; set; }
        public DbSet<Term> Terms { get; set; }
        public DbSet<TermType> TermTypes { get; set; }
        public DbSet<Signatory> Signatories { get; set; }

        #region Admission
        // master table
        public DbSet<Agency> Agencies { get; set; }
        public DbSet<AgencyContract> AgencyContracts { get; set; }
        public DbSet<AdmissionChannel> AdmissionChannels { get; set; }
        public DbSet<AdmissionType> AdmissionTypes { get; set; }
        public DbSet<AdmissionPlace> AdmissionPlaces { get; set; }
        public DbSet<AdmissionExaminationType> AdmissionExaminationTypes { get; set; }
        public DbSet<CardExpirationOption> CardExpirationOptions { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<EducationBackground> EducationBackgrounds { get; set; }
        public DbSet<SchoolType> SchoolTypes { get; set; }
        public DbSet<SchoolTerritory> SchoolTerritories { get; set; }
        public DbSet<SchoolGroup> SchoolGroups { get; set; }
        public DbSet<PreviousSchool> PreviousSchools { get; set; }
        public DbSet<TransferUniversity> TransferUniversities { get; set; }

        // student
        public DbSet<Probation> Probations { get; set; }
        public DbSet<ExtendedYear> ExtendedYears { get; set; }

        // admission
        public DbSet<AdmissionCurriculum> AdmissionCurriculums { get; set; }
        public DbSet<AdmissionRound> AdmissionRounds { get; set; }
        public DbSet<AdmissionExamination> AdmissionExaminations { get; set; }
        public DbSet<AdmissionExaminationSchedule> AdmissionExaminationSchedules { get; set; }
        public DbSet<StudentCodeRange> StudentCodeRanges { get; set; }
        public DbSet<IntensiveCourse> IntensiveCourses { get; set; }
        public DbSet<StudentIntensiveCourse> StudentIntensiveCourses { get; set; }
        public DbSet<AdmissionDocumentGroup> AdmissionDocumentGroups { get; set; }
        public DbSet<StudentDocument> StudentDocuments { get; set; }
        public DbSet<RequiredDocument> RequiredDocuments { get; set; }
        public DbSet<ExemptedAdmissionExamination> ExemptedAdmissionExaminations { get; set; }
        public DbSet<ExemptedExaminationScore> ExemptedExaminationScores { get; set; }
        public DbSet<StudentExemptedExamScore> StudentExemptedExamScores { get; set; }
        public DbSet<VerificationLetter> VerificationLetters { get; set; }
        public DbSet<VerificationStudent> VerificationStudents { get; set; }

        #endregion

        #region Academic Calendar
        public DbSet<Event> Events { get; set; }
        public DbSet<EventCategory> EventCategories { get; set; }
        public DbSet<AcademicCalendar> AcademicCalendars { get; set; }
        public DbSet<Calendar> Calendars { get; set; }
        public DbSet<MuicHoliday> MuicHolidays { get; set; }
        #endregion

        #region Academic
        public DbSet<AcademicHonor> AcademicHonors { get; set; }
        public DbSet<AcademicLevel> AcademicLevels { get; set; }
        public DbSet<AcademicProgram> AcademicPrograms { get; set; }
        public DbSet<StudentGroup> StudentGroups { get; set; }
        #endregion

        #region Advisor
        public DbSet<AdvisingCourse> AdvisingCourses { get; set; }
        public DbSet<AdvisingLog> AdvisingLogs { get; set; }
        public DbSet<AdvisingStatus> AdvisingStatuses { get; set; }
        public DbSet<AdvisorStudent> AdvisorStudents { get; set; }
        #endregion

        #region Authentication & Menu
        public DbSet<Menu> Menus { get; set; }
        public DbSet<MenuGroup> MenuGroups { get; set; }
        public DbSet<MenuSubgroup> MenuSubgroups { get; set; }
        public DbSet<MenuType> MenuTypes { get; set; }
        public DbSet<MenuPermission> MenuPermissions { get; set; }
        public DbSet<Tab> Tabs { get; set; }
        public DbSet<TabPermission> TabPermissions { get; set; }
        #endregion

        #region Configuration
        public DbSet<RegistrationConfiguration> RegistrationConfigurations { get; set; }
        public DbSet<LateRegistrationConfiguration> LateRegistrationConfigurations { get; set; }
        public DbSet<LatePaymentConfiguration> LatePaymentConfigurations { get; set; }
        public DbSet<AddDropFeeConfiguration> AddDropFeeConfigurations { get; set; }
        #endregion

        #region Registration
        public DbSet<CreditLoad> CreditLoads { get; set; }
        public DbSet<LatePaymentTransaction> LatePaymentTransactions { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<PlanSchedule> PlanSchedules { get; set; }
        public DbSet<RegistrationCourse> RegistrationCourses { get; set; }
        public DbSet<RegistrationLog> RegistrationLogs { get; set; }
        public DbSet<RegistrationChangeCourseLog> RegistrationChangeCourseLogs { get; set; }
        public DbSet<RegistrationTerm> RegistrationTerms { get; set; }
        public DbSet<RegistrationStatus> RegistrationStatuses { get; set; }
        public DbSet<RegistrationCondition> RegistrationConditions { get; set; }
        public DbSet<RegistrationSlotCondition> RegistrationSlotConditions { get; set; }
        public DbSet<Slot> Slots { get; set; }
        public DbSet<Refund> Refunds { get; set; }
        public DbSet<Percentage> Percentages { get; set; }
        public DbSet<StudentTerm> StudentTerms { get; set; }

        public DbSet<BatchRegistrationConfirmJob> BatchRegistrationConfirmJobs { get; set; }
        public DbSet<BatchRegistrationConfirmJobDetail> BatchRegistrationConfirmJobDetails { get; set; }
        #endregion

        #region PersonInfo
        public DbSet<Title> Titles { get; set; }
        public DbSet<Nationality> Nationalities { get; set; }
        public DbSet<Race> Races { get; set; }
        public DbSet<Religion> Religions { get; set; }
        public DbSet<Revenue> Revenues { get; set; }
        public DbSet<Occupation> Occupations { get; set; }
        public DbSet<Deformation> Deformations { get; set; }
        public DbSet<StudentFeeType> StudentFeeTypes { get; set; }
        public DbSet<ResidentType> ResidentTypes { get; set; }
        #endregion

        #region Curriculum
        public DbSet<Curriculum> Curriculums { get; set; }
        public DbSet<CurriculumVersion> CurriculumVersions { get; set; }
        public DbSet<CurriculumBlacklistCourse> CurriculumBlacklistCourses { get; set; }
        public DbSet<CurriculumCourse> CurriculumCourses { get; set; }
        public DbSet<CurriculumDependency> CurriculumDependencies { get; set; }
        public DbSet<CurriculumInstructor> CurriculumInstructor { get; set; }
        public DbSet<CurriculumCourseGroup> CurriculumCourseGroups { get; set; }
        public DbSet<CurriculumSpecializationGroup> CurriculumSpecializationGroups { get; set; }
        public DbSet<CourseGroup> CourseGroups { get; set; }
        public DbSet<StudyCourse> StudyCourses { get; set; }
        public DbSet<StudyPlan> StudyPlans { get; set; }
        public DbSet<FilterCurriculumVersionGroup> FilterCurriculumVersionGroups { get; set; }
        public DbSet<FilterCurriculumVersionGroupDetail> FilterCurriculumVersionGroupDetails { get; set; }
        #endregion

        #region Courses
        public DbSet<CustomCourseGroup> CustomCourseGroups { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseEquivalent> CourseEquivalents { get; set; }
        public DbSet<CourseExclusion> CourseExclusions { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<SectionDetail> SectionDetails { get; set; }
        public DbSet<SharedSection> SharedSections { get; set; }
        public DbSet<InstructorSection> InstructorSections { get; set; }
        public DbSet<CourseRate> CourseRates { get; set; }
        public DbSet<SectionQuota> SectionQuotas { get; set; }
        public DbSet<SectionPeriod> SectionPeriods { get; set; }
        public DbSet<FilterCourseGroup> FilterCourseGroups { get; set; }
        public DbSet<FilterCourseGroupDetail> FilterCourseGroupDetails { get; set; }
        #endregion

        #region Room Reservation
        public DbSet<SectionSlot> SectionSlots { get; set; }
        public DbSet<RoomSlot> RoomSlots { get; set; }
        public DbSet<ExaminationReservation> ExaminationReservations { get; set; }
        public DbSet<RoomReservation> RoomReservations { get; set; }
        public DbSet<ReservationCalendar> ReservationCalendars { get; set; }
        #endregion

        #region Examination
        public DbSet<ExaminationType> ExaminationTypes { get; set; }
        public DbSet<ExaminationPeriod> ExaminationPeriods { get; set; }
        public DbSet<ExaminationCoursePeriod> ExaminationCoursePeriods { get; set; }
        public DbSet<ExaminationCourseCondition> ExaminationCourseConditions { get; set; }
        #endregion

        #region Instructor
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<InstructorWorkStatus> InstructorWorkStatuses { get; set; }
        #endregion

        #region Teaching Load
        public DbSet<InstructorType> InstructorTypes { get; set; }
        public DbSet<InstructorRanking> InstructorRankings { get; set; }
        public DbSet<TeachingType> TeachingTypes { get; set; }
        public DbSet<TeachingLoad> TeachingLoads { get; set; }
        #endregion

        #region Reason
        public DbSet<ReEnterReason> ReEnterReasons { get; set; }
        public DbSet<ResignReason> ResignReasons { get; set; }
        public DbSet<RetireReason> RetireReasons { get; set; }
        #endregion

        #region University
        public DbSet<Campus> Campuses { get; set; }
        public DbSet<Building> Buildings { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomType> RoomTypes { get; set; }
        public DbSet<Facility> Facilities { get; set; }
        public DbSet<RoomFacility> RoomFacilities { get; set; }
        public DbSet<Faculty> Faculties { get; set; }
        public DbSet<FacultyMember> FacultyMembers { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<SpecializationGroup> SpecializationGroups { get; set; }
        public DbSet<SpecializationGroupBlackList> SpecializationGroupBlackLists { get; set; }
        #endregion

        #region Announcement
        public DbSet<Topic> Topics { get; set; }
        public DbSet<Channel> Channels { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<AnnouncementTopic> AnnouncementTopics { get; set; }
        #endregion

        #region Address
        public DbSet<Country> Countries { get; set; }
        public DbSet<Province> Provinces { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<Subdistrict> Subdistricts { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<State> States { get; set; }
        #endregion

        #region  Student Model
        public DbSet<Student> Students { get; set; }
        public DbSet<AcademicInformation> AcademicInformations { get; set; }
        public DbSet<StudentGrade> StudentGrades { get; set; }
        public DbSet<AdmissionInformation> AdmissionInformations { get; set; }
        public DbSet<CheatingStatus> CheatingStatuses { get; set; }
        public DbSet<CurriculumInformation> CurriculumInformations { get; set; }
        public DbSet<ExtendedStudent> ExtendedStudents { get; set; }
        public DbSet<GraduationInformation> GraduationInformations { get; set; }
        public DbSet<MaintenanceStatus> MaintenanceStatuses { get; set; }
        public DbSet<ParentInformation> ParentInformations { get; set; }
        public DbSet<StudentAddress> StudentAddresses { get; set; }
        public DbSet<StudentCourseByPass> StudentCourseByPasses { get; set; }
        public DbSet<StudentIncident> StudentIncidents { get; set; }
        public DbSet<StudentIncidentLog> StudentIncidentLogs { get; set; }
        public DbSet<BlacklistedStudent> BlacklistedStudents { get; set; }
        public DbSet<ResignStudent> ResignStudents { get; set; }
        public DbSet<DismissStudent> DismissStudents { get; set; }
        public DbSet<StudentTransferLog> StudentTransferLogs { get; set; }
        public DbSet<StudentTransferLogDetail> StudentTransferLogDetails { get; set; }
        public DbSet<StudentLog> StudentLogs { get; set; }
        public DbSet<SpecializationGroupInformation> SpecializationGroupInformations { get; set; }
        public DbSet<StudentProbation> StudentProbations { get; set; }
        public DbSet<StudentStatusLog> StudentStatusLogs { get; set; }
        public DbSet<Retire> Retires { get; set; }
        public DbSet<StudentState> StudentStates { get; set; }
        public DbSet<StudentStateLog> StudentStateLogs { get; set; }
        #endregion

        #region Withdrawal
        public DbSet<Withdrawal> Withdrawals { get; set; }
        public DbSet<WithdrawalException> WithdrawalExceptions { get; set; }
        public DbSet<WithdrawalPeriod> WithdrawalPeriods { get; set; }
        public DbSet<WithdrawalLog> WithdrawalLogs { get; set; }
        #endregion

        #region Fee
        public DbSet<FeeItem> FeeItems { get; set; }
        public DbSet<FeeGroup> FeeGroups { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<TermFee> TermFees { get; set; }
        public DbSet<TuitionFee> TuitionFees { get; set; }
        public DbSet<TuitionFeeFormula> TuitionFeeFormulas { get; set; }
        public DbSet<TuitionFeeRate> TuitionFeeRates { get; set; }
        public DbSet<TuitionFeeType> TuitionFeeTypes { get; set; }
        public DbSet<StudentFeeGroup> StudentFeeGroups { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<InvoiceDeductTransaction> InvoiceDeductTransactions { get; set; }
        public DbSet<InvoicePrintLog> InvoicePrintLogs { get; set; }
        public DbSet<Receipt> Receipts { get; set; }
        public DbSet<ReceiptItem> ReceiptItems { get; set; }
        public DbSet<ReceiptPaymentMethod> ReceiptPaymentMethods { get; set; }
        public DbSet<ReceiptPrintLog> ReceiptPrintLogs { get; set; }
        public DbSet<BankPaymentResponse> BankPaymentResponses { get; set; }
        #endregion

        #region Finance
        public DbSet<Installment> Installments { get; set; }
        public DbSet<InstallmentTransaction> InstallmentTransactions { get; set; }
        public DbSet<OnCredit> OnCredits { get; set; }
        public DbSet<OnCreditTransaction> OnCreditTransactions { get; set; }
        #endregion

        #region Grade
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Barcode> Barcodes { get; set; }
        public DbSet<Coordinator> Coordinators { get; set; }
        public DbSet<GradingLog> GradingLogs { get; set; }
        public DbSet<GradingCurve> GradingCurves { get; set; }
        public DbSet<GradeTemplate> GradeTemplates { get; set; }
        public DbSet<StandardGradingGroup> StandardGradingGroups { get; set; }
        public DbSet<StandardGradingScore> StandardGradingScores { get; set; }
        public DbSet<MarkAllocation> MarkAllocations { get; set; }
        public DbSet<StudentRawScore> StudentRawScores { get; set; }
        public DbSet<StudentRawScoreDetail> StudentRawScoreDetails { get; set; }
        public DbSet<GradeMember> GradeMembers { get; set; }
        #endregion

        #region Scholarship
        public DbSet<BudgetDetail> BudgetDetails { get; set; }
        public DbSet<FinancialTransaction> FinancialTransactions { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<VoucherLog> VoucherLogs { get; set; }
        public DbSet<Scholarship> Scholarships { get; set; }
        public DbSet<ScholarshipActiveLog> ScholarshipActiveLogs { get; set; }
        public DbSet<ScholarshipBudget> ScholarshipBudgets { get; set; }
        public DbSet<ScholarshipFeeItem> ScholarshipFeeItems { get; set; }
        public DbSet<ScholarshipStudent> ScholarshipStudents { get; set; }
        public DbSet<ScholarshipType> ScholarshipTypes { get; set; }
        public DbSet<Sponsor> Sponsors { get; set; }
        #endregion

        #region Questionnaire
        public DbSet<QuestionnaireCourseGroup> QuestionnaireCourseGroups { get; set; }
        public DbSet<QuestionnaireCourseGroupDetail> QuestionnaireCourseGroupDetails { get; set; }
        public DbSet<Questionnaire> Questionnaires { get; set; }
        public DbSet<QuestionGroup> QuestionGroups { get; set; }
        public DbSet<QuestionnairePeriod> QuestionnairePeriods { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Response> Responses { get; set; }
        public DbSet<QuestionnaireApproval> QuestionnaireApprovals { get; set; }
        public DbSet<QuestionnaireApprovalLog> QuestionnaireApprovalLogs { get; set; }
        public DbSet<QuestionnaireMember> QuestionnaireMembers { get; set; }
        #endregion

        #region Petition
        public DbSet<Petition> Petitions { get; set; }
        public DbSet<StudentCertificate> StudentCertificates { get; set; }
        public DbSet<StudentPetition> StudentPetitions { get; set; }
        public DbSet<ChangingCurriculumPetition> ChangingCurriculumPetitions { get; set; }
        public DbSet<PetitionLog> PetitionLogs { get; set; }
        #endregion

        #region Log
        public DbSet<PrintingLog> PrintingLogs { get; set; }
        public DbSet<CardLog> CardLogs { get; set; }
        public DbSet<ChangedNameLog> ChangedNameLogs { get; set; }
        public DbSet<DataSyncLog> DataSyncLogs { get; set; }
        public DbSet<ApiCallLog> ApiCallLogs { get; set; }
        #endregion

        #region Pre-requisite
        public DbSet<AbilityCondition> AbilityConditions { get; set; }
        public DbSet<AndCondition> AndConditions { get; set; }
        public DbSet<BatchCondition> BatchConditions { get; set; }
        public DbSet<Corequisite> Corequisites { get; set; }
        public DbSet<CourseGroupCondition> CourseGroupConditions { get; set; }
        public DbSet<CreditCondition> CreditConditions { get; set; }
        public DbSet<GPACondition> GPAConditions { get; set; }
        public DbSet<GradeCondition> GradeConditions { get; set; }
        public DbSet<OrCondition> OrConditions { get; set; }
        public DbSet<Prerequisite> Prerequisites { get; set; }
        public DbSet<PredefinedCourse> PredefinedCourses { get; set; }
        public DbSet<TermCondition> TermConditions { get; set; }
        public DbSet<TotalCourseGroupCondition> TotalCourseGroupConditions { get; set; }
        public DbSet<StudentPredefinedCourse> StudentPredefinedCourses { get; set; }
        #endregion

        #region Graduation
        public DbSet<CourseGroupingDetail> CourseGroupingDetails { get; set; }
        public DbSet<CourseGroupingLog> CourseGroupingLogs { get; set; }
        public DbSet<CoursePrediction> CoursePredictions { get; set; }
        public DbSet<GraduatingRequest> GraduatingRequests { get; set; }
        public DbSet<GraduatingRequestLog> GraduatingRequestLogs { get; set; }
        public DbSet<CourseGroupModification> CourseGroupModifications { get; set; }
        #endregion

        #region USpark
        public DbSet<USparkSection> USparkSections { get; set; }
        public DbSet<USparkOrder> USparkOrders { get; set; }
        #endregion

        public DbSet<IdentityRole> Roles { get; set; }
        public DbSet<IdentityUserRole<string>> UserRoles { get; set; }
        public DbSet<IdentityUserClaim<string>> UserClaims { get; set; }
        public DbSet<IdentityUserLogin<string>> UserLogins { get; set; }
        public DbSet<IdentityRoleClaim<string>> UserRoleClaims { get; set; }
        public DbSet<IdentityUserToken<string>> UserTokens { get; set; }
        public DbSet<ApplicationUser> Users { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // IF NOT GIVEN CONFIGURATION SET TO LOCAL DB
                optionsBuilder.UseSqlServer("Server=THIWAT-ASUS\\SQLEXPRESS;Initial Catalog=muic_db;Persist Security Info=False;User ID=sa;Password=mrtoJGHToMLBaZdibrNf3pKc;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;");
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable(name: "Users");
            });

            builder.Entity<IdentityRole>(entity =>
            {
                entity.ToTable(name: "Roles");
            });

            builder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.ToTable(name: "UserRoles");
                entity.HasKey(x => new { x.UserId, x.RoleId });
            });

            builder.Entity<IdentityUserClaim<string>>(entity =>
            {
                entity.ToTable(name: "UserClaims");
            });

            builder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.ToTable(name: "UserLogins");
                entity.HasKey(x => new { x.LoginProvider, x.ProviderKey, x.UserId });
            });

            builder.Entity<IdentityRoleClaim<string>>(entity =>
            {
                entity.ToTable("RoleClaims");
                entity.HasKey(x => new { x.RoleId, x.Id, x.ClaimType, x.ClaimValue });
            });

            builder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.ToTable("UserTokens");
                entity.HasKey(x => new { x.LoginProvider, x.UserId, x.Name, x.Value });
            });

            // Primary Key
            builder.Entity<AnnouncementTopic>()
                   .HasKey(x => new { x.AnnouncementId, x.TopicId });

            builder.Entity<CurriculumInstructor>()
                   .HasKey(x => new { x.CurriculumVersionId, x.InstructorId });

            builder.Entity<VerificationStudent>()
                   .HasKey(x => new { x.VerificationLetterId, x.StudentId });
            // Schema master

            #region Admission
            builder.Entity<AdmissionType>()
                   .ToTable("AdmissionTypes", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<AdmissionPlace>()
                   .ToTable("AdmissionPlaces", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<AdmissionExaminationType>()
                   .ToTable("AdmissionExaminationTypes", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<AdmissionChannel>()
                   .ToTable("AdmissionChannels", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<CardExpirationOption>()
                   .ToTable("CardExpirationOptions", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<EducationBackground>()
                   .ToTable("EducationBackgrounds", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<Document>()
                   .ToTable("Documents", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<SchoolType>()
                   .ToTable("SchoolTypes", schema: "master")
                   .HasQueryFilter(x => x.IsActive)
                   .HasIndex(p => p.Code).IsUnique();

            builder.Entity<SchoolTerritory>()
                   .ToTable("SchoolTerritories", schema: "master")
                   .HasQueryFilter(x => x.IsActive)
                   .HasIndex(p => p.Code).IsUnique();

            builder.Entity<SchoolGroup>()
                   .ToTable("SchoolGroups", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<PreviousSchool>()
                   .ToTable("PreviousSchools", schema: "master")
                   .HasQueryFilter(x => x.IsActive)
                   .HasIndex(p => p.Code).IsUnique();

            builder.Entity<TransferUniversity>()
                   .ToTable("TransferUniversities", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<Probation>()
                   .ToTable("Probations", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<Retire>()
                   .ToTable("Retires", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<ExtendedYear>()
                   .ToTable("ExtendedYears", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<Agency>()
                   .ToTable("Agencies", schema: "admission")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<AgencyContract>()
                   .ToTable("AgencyContracts", schema: "admission");

            builder.Entity<AdmissionCurriculum>()
                   .ToTable("AdmissionCurriculums", schema: "admission")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<AdmissionRound>()
                   .ToTable("AdmissionRounds", schema: "admission")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<AdmissionExamination>()
                   .ToTable("AdmissionExaminations", schema: "admission")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<AdmissionExaminationSchedule>()
                   .ToTable("AdmissionExaminationSchedules", schema: "admission");

            builder.Entity<StudentCodeRange>()
                   .ToTable("StudentCodeRanges", schema: "admission")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<IntensiveCourse>()
                   .ToTable("IntensiveCourses", schema: "admission")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<StudentIntensiveCourse>()
                   .ToTable("StudentIntensiveCourses", schema: "admission")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<AdmissionDocumentGroup>()
                   .ToTable("AdmissionDocumentGroups", schema: "admission")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<StudentDocument>()
                   .ToTable("StudentDocuments", schema: "admission")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<RequiredDocument>()
                   .ToTable("RequiredDocuments", schema: "admission");

            builder.Entity<RequiredDocument>()
                   .HasOne(x => x.AdmissionDocumentGroup)
                   .WithMany(x => x.RequiredDocuments)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ExemptedAdmissionExamination>()
                   .ToTable("ExemptedAdmissionExaminations", schema: "admission")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<ExemptedExaminationScore>()
                   .ToTable("ExemptedExaminationScores", schema: "admission")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<StudentExemptedExamScore>()
                   .ToTable("StudentExemptedExamScores", schema: "admission")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<VerificationLetter>()
                   .ToTable("VerificationLetters", schema: "admission")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<VerificationStudent>()
                   .ToTable("VerificationStudents", schema: "admission");

            builder.Entity<VerificationStudent>()
                   .HasOne(x => x.VerificationLetter)
                   .WithMany(x => x.VerificationStudents)
                   .OnDelete(DeleteBehavior.Cascade);
            #endregion

            #region Academic
            builder.Entity<AcademicHonor>()
                   .ToTable("AcademicHonors", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<AcademicLevel>()
                   .ToTable("AcademicLevels", schema: "master")
                   .HasQueryFilter(x => x.IsActive)
                   .HasIndex(p => p.Code).IsUnique();

            builder.Entity<AcademicProgram>()
                   .ToTable("AcademicPrograms", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<StudentGroup>()
                   .ToTable("StudentGroups", schema: "master")
                   .HasQueryFilter(x => x.IsActive)
                   .HasIndex(p => p.Code).IsUnique();

            builder.Entity<RegistrationStatus>()
                   .ToTable("RegistrationStatuses", schema: "master")
                   .HasQueryFilter(x => x.IsActive);
            #endregion

            #region Advisor
            builder.Entity<AdvisingCourse>()
                   .ToTable("AdvisingCourses", schema: "advisor")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<AdvisingLog>()
                   .ToTable("AdvisingLogs", schema: "advisor");

            builder.Entity<AdvisingStatus>()
                   .ToTable("AdvisingStatuses", schema: "advisor")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<AdvisorStudent>()
                   .ToTable("AdvisorStudents", schema: "advisor")
                   .HasQueryFilter(x => x.IsActive);
            #endregion

            #region Academic Calendar
            builder.Entity<Event>()
                   .ToTable("Events", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<EventCategory>()
                   .ToTable("EventCategories", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<AcademicCalendar>()
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<Calendar>()
                   .ToTable("Calendars", schema: "calendar");

            builder.Entity<MuicHoliday>()
                  .HasQueryFilter(x => x.IsActive);
            #endregion

            #region Authentication
            builder.Entity<Menu>()
                   .ToTable("Menus", schema: "authentication");

            builder.Entity<MenuGroup>()
                   .ToTable("MenuGroups", schema: "authentication");

            builder.Entity<MenuSubgroup>()
                   .ToTable("MenuSubgroups", schema: "authentication");

            builder.Entity<MenuType>()
                   .ToTable("MenuTypes", schema: "authentication");

            builder.Entity<MenuPermission>()
                   .ToTable("MenuPermissions", schema: "authentication")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<Tab>()
                   .ToTable("Tabs", schema: "authentication");

            builder.Entity<TabPermission>()
                   .ToTable("TabPermissions", schema: "authentication")
                   .HasQueryFilter(x => x.IsActive);
            #endregion

            #region Configuration
            builder.Entity<RegistrationConfiguration>()
                   .ToTable("RegistrationConfigurations", schema: "configuration")
                   .HasQueryFilter(x => x.IsActive);
            builder.Entity<LateRegistrationConfiguration>()
                   .ToTable("LateRegistrationConfigurations", schema: "configuration")
                   .HasQueryFilter(x => x.IsActive);
            builder.Entity<LatePaymentConfiguration>()
                   .ToTable("LatePaymentConfigurations", schema: "configuration")
                   .HasQueryFilter(x => x.IsActive);
            builder.Entity<AddDropFeeConfiguration>()
                   .ToTable("AddDropFeeConfigurations", schema: "configuration")
                   .HasQueryFilter(x => x.IsActive);
            #endregion

            #region Course & Section
            builder.Entity<CustomCourseGroup>()
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<Course>()
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<CourseEquivalent>()
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<CourseExclusion>()
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<Section>()
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<CourseRate>()
                   .ToTable("CourseRates", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<SectionQuota>()
                   .ToTable("SectionQuotas", schema: "registration")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<SectionPeriod>()
                   .ToTable("SectionPeriods", schema: "registration")
                   .HasQueryFilter(x => x.IsActive);

            //Course has many section
            builder.Entity<Section>()
                   .HasOne(x => x.Course)
                   .WithMany(x => x.Sections)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SectionDetail>()
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<SharedSection>()
                   .ToTable("SharedSections", schema: "course");

            //Section has many section detail
            builder.Entity<SectionDetail>()
                   .HasOne(x => x.Section)
                   .WithMany(x => x.SectionDetails)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<InstructorSection>()
                   .HasQueryFilter(x => x.IsActive);

            //Section detail may has many instructor
            builder.Entity<InstructorSection>()
                   .HasOne(x => x.SectionDetail)
                   .WithMany(x => x.InstructorSections)
                   .OnDelete(DeleteBehavior.Cascade);
            #endregion

            #region Room Reservation
            builder.Entity<SectionSlot>()
                   .ToTable("SectionSlots", schema: "slot")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<RoomSlot>()
                   .ToTable("RoomSlots", schema: "slot")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<ExaminationReservation>()
                   .ToTable("ExaminationReservations", schema: "reservation")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<RoomReservation>()
                   .ToTable("RoomReservations", schema: "reservation")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<ReservationCalendar>()
                   .ToTable("ReservationCalendars", schema: "reservation")
                   .HasQueryFilter(x => x.IsActive);
            #endregion

            #region Curriculum
            builder.Entity<Curriculum>()
                   .ToTable("Curriculums", schema: "curriculum")
                   .HasQueryFilter(x => x.IsActive)
                   .HasIndex(p => p.ReferenceCode).IsUnique();

            builder.Entity<CurriculumVersion>()
                   .ToTable("CurriculumVersions", schema: "curriculum")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<CurriculumVersion>()
                   .HasOne(x => x.Curriculum)
                   .WithMany(x => x.CurriculumVersions)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CurriculumBlacklistCourse>()
                   .ToTable("CurriculumBlacklistCourses", schema: "curriculum")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<CurriculumBlacklistCourse>()
                   .HasOne(x => x.CurriculumVersion)
                   .WithMany(x => x.CurriculumBlacklistCourses)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CurriculumDependency>()
                   .ToTable("CurriculumDependencies", schema: "curriculum")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<CurriculumInstructor>()
                   .ToTable("CurriculumInstructors", schema: "curriculum")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<CurriculumInstructor>()
                   .HasOne(x => x.CurriculumVersion)
                   .WithMany(x => x.CurriculumInstructors)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CurriculumCourseGroup>()
                   .ToTable("CurriculumCourseGroups", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<CurriculumSpecializationGroup>()
                   .ToTable("CurriculumSpecializationGroups", schema: "curriculum")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<CourseGroup>()
                   .ToTable("CourseGroups", schema: "curriculum")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<CourseGroup>()
                   .HasOne(x => x.CurriculumVersion)
                   .WithMany(x => x.CourseGroups)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CurriculumCourse>()
                   .ToTable("CurriculumCourses", schema: "curriculum")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<CurriculumCourse>()
                   .HasOne(x => x.CourseGroup)
                   .WithMany(x => x.CurriculumCourses)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StudyPlan>()
                   .ToTable("StudyPlans", schema: "curriculum")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<StudyPlan>()
                   .HasOne(x => x.CurriculumVersion)
                   .WithMany(x => x.StudyPlans)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StudyCourse>()
                   .ToTable("StudyCourses", schema: "curriculum");

            builder.Entity<StudyCourse>()
                   .HasOne(x => x.StudyPlan)
                   .WithMany(x => x.StudyCourses)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<FilterCourseGroup>()
                   .ToTable("FilterCourseGroups", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<FilterCourseGroupDetail>()
                   .ToTable("FilterCourseGroupDetails", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<FilterCurriculumVersionGroup>()
                   .ToTable("FilterCurriculumVersionGroups", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<FilterCurriculumVersionGroupDetail>()
                   .ToTable("FilterCurriculumVersionGroupDetails", schema: "master")
                   .HasQueryFilter(x => x.IsActive);
            #endregion

            #region PersonInfo
            builder.Entity<Title>()
                   .ToTable("Titles", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<Nationality>()
                   .ToTable("Nationalities", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<Race>()
                   .ToTable("Races", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<Religion>()
                   .ToTable("Religions", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<Revenue>()
                   .ToTable("Revenues", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<Occupation>()
                   .ToTable("Occupations", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<Deformation>()
                   .ToTable("Deformations", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<StudentFeeType>()
                   .ToTable("StudentFeeTypes", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<ResidentType>()
                   .ToTable("ResidentTypes", schema: "master")
                   .HasQueryFilter(x => x.IsActive);
            #endregion

            #region Examination
            builder.Entity<ExaminationType>()
                   .ToTable("ExaminationTypes", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<ExaminationPeriod>()
                   .ToTable("ExaminationPeriods", schema: "examination")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<ExaminationCoursePeriod>()
                   .ToTable("ExaminationCoursePeriods", schema: "examination")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<ExaminationCourseCondition>()
                   .ToTable("ExaminationCourseConditions", schema: "examination")
                   .HasQueryFilter(x => x.IsActive);
            #endregion

            #region Instructor
            builder.Entity<Instructor>()
                   .HasQueryFilter(x => x.IsActive)
                   .HasIndex(p => p.Code).IsUnique();

            //one to one relationship with cascade delete
            builder.Entity<Instructor>()
                   .HasOne(x => x.InstructorWorkStatus)
                   .WithOne(x => x.Instructor)
                   .OnDelete(DeleteBehavior.Cascade);
            #endregion

            #region Teaching Load
            builder.Entity<InstructorType>()
                   .ToTable("InstructorTypes", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<InstructorRanking>()
                   .ToTable("InstructorRankings", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<TeachingType>()
                   .ToTable("TeachingTypes", schema: "master")
                   .HasQueryFilter(x => x.IsActive)
                   .HasIndex(p => p.Code).IsUnique();
            #endregion

            #region Reason
            builder.Entity<ReEnterReason>()
                   .ToTable("ReEnterReasons", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<ResignReason>()
                   .ToTable("ResignReasons", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<RetireReason>()
                   .ToTable("RetireReasons", schema: "master")
                   .HasQueryFilter(x => x.IsActive);
            #endregion

            #region Registration
            builder.Entity<CreditLoad>()
                   .ToTable("CreditLoads", schema: "registration")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<LatePaymentTransaction>()
                   .ToTable("LatePaymentTransactions", schema: "registration")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<Plan>()
                   .ToTable("Plans", schema: "registration")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<PlanSchedule>()
                   .ToTable("PlanSchedules", schema: "registration")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<RegistrationCourse>()
                   .ToTable("RegistrationCourses", schema: "registration")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<RegistrationCourse>()
                   .HasIndex(x => x.USPreregistrationId)
                      .HasFilter("USPreregistrationId IS NOT NULL")
                      .IsUnique();

            builder.Entity<RegistrationTerm>()
                   .ToTable("RegistrationTerms", schema: "registration")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<RegistrationCondition>()
                   .ToTable("RegistrationConditions", schema: "registration")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<RegistrationSlotCondition>()
                   .ToTable("RegistrationSlotConditions", schema: "registration");

            builder.Entity<Slot>()
                   .ToTable("Slots", schema: "registration")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<RegistrationLog>()
                   .ToTable("RegistrationLogs", schema: "registration");

            builder.Entity<RegistrationLog>()
                   .HasIndex(x => x.USparkId)
                      .HasFilter("USparkId IS NOT NULL")
                      .IsUnique();


            builder.Entity<RegistrationChangeCourseLog>()
                   .ToTable("RegistrationChangeCourseLogs", schema: "registration");

            builder.Entity<Refund>()
                   .ToTable("Refunds", schema: "registration")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<StudentTerm>()
                   .ToTable("StudentTerms", schema: "registration")
                   .HasQueryFilter(x => x.IsActive);


            builder.Entity<BatchRegistrationConfirmJob>()
                   .ToTable("BatchRegistrationConfirmJobs", schema: "registration");

            builder.Entity<BatchRegistrationConfirmJobDetail>()
                   .ToTable("BatchRegistrationConfirmJobDetails", schema: "registration");

            #endregion

            #region University
            builder.Entity<Campus>()
                   .ToTable("Campuses", schema: "master")
                   .HasQueryFilter(x => x.IsActive)
                   .HasIndex(p => p.Code).IsUnique();

            builder.Entity<Building>()
                   .ToTable("Buildings", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<Room>()
                   .ToTable("Rooms", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<RoomType>()
                   .ToTable("RoomTypes", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<Facility>()
                   .ToTable("Facilities", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<RoomFacility>()
                   .ToTable("RoomFacilities", schema: "master");

            builder.Entity<Faculty>()
                   .ToTable("Faculties", schema: "master")
                   .HasQueryFilter(x => x.IsActive)
                   .HasIndex(p => p.Code).IsUnique();

            builder.Entity<FacultyMember>()
                   .ToTable("FacultyMembers", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<Department>()
                   .ToTable("Departments", schema: "master")
                   .HasQueryFilter(x => x.IsActive)
                   .HasIndex(p => p.Code).IsUnique();

            builder.Entity<SpecializationGroup>()
                   .ToTable("SpecializationGroups", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<SpecializationGroupBlackList>()
                   .ToTable("SpecializationGroupBlackLists", schema: "master")
                   .HasQueryFilter(x => x.IsActive);
            #endregion

            #region Announcement
            builder.Entity<Channel>()
                   .ToTable("Channels", schema: "announcement")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<Topic>()
                   .ToTable("Topics", schema: "announcement")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<Announcement>()
                   .ToTable("Announcements", schema: "announcement")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<AnnouncementTopic>()
                   .ToTable("AnnouncementTopic", schema: "announcement");

            builder.Entity<AnnouncementTopic>()
                   .HasOne(x => x.Announcement)
                   .WithMany(x => x.AnnouncementTopics)
                   .OnDelete(DeleteBehavior.Cascade);
            #endregion

            #region Address
            builder.Entity<Country>()
                   .ToTable("Countries", schema: "master")
                   .HasQueryFilter(x => x.IsActive)
                   .HasIndex(p => p.Code).IsUnique();

            builder.Entity<Province>()
                   .ToTable("Provinces", schema: "master")
                   .HasQueryFilter(x => x.IsActive)
                   .HasIndex(p => p.Code).IsUnique();

            builder.Entity<District>()
                   .ToTable("Districts", schema: "master")
                   .HasQueryFilter(x => x.IsActive)
                   .HasIndex(p => p.Code).IsUnique();

            builder.Entity<Subdistrict>()
                   .ToTable("Subdistricts", schema: "master")
                   .HasQueryFilter(x => x.IsActive)
                   .HasIndex(p => p.Code).IsUnique();

            builder.Entity<City>()
                   .ToTable("Cities", schema: "master")
                   .HasQueryFilter(x => x.IsActive)
                   .HasIndex(p => p.Code).IsUnique();

            builder.Entity<State>()
                   .ToTable("States", schema: "master")
                   .HasQueryFilter(x => x.IsActive)
                   .HasIndex(p => p.Code).IsUnique();
            #endregion

            #region misc
            builder.Entity<BankBranch>()
                   .ToTable("BankBranches", schema: "master")
                   .HasQueryFilter(x => x.IsActive)
                   .HasIndex(p => p.Code).IsUnique();

            builder.Entity<DistributionMethod>()
                   .ToTable("DistributionMethods", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<Relationship>()
                   .ToTable("Relationships", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<Term>()
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<TermType>()
                   .ToTable("TermTypes", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<MaintenanceFee>()
                   .ToTable("MaintenanceFees", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<Incident>()
                   .ToTable("Incidents", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<Language>()
                   .ToTable("Languages", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<Signatory>()
                   .ToTable("Signatories", schema: "master")
                   .HasQueryFilter(x => x.IsActive);
            #endregion

            #region Schema Student
            builder.Entity<Student>()
                   .ToTable("Students", schema: "student")
                   //.HasQueryFilter(x => x.IsActive)
                   .HasIndex(p => p.Code).IsUnique();

            builder.Entity<AcademicInformation>()
                   .ToTable("AcademicInformations", schema: "student")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<StudentGrade>()
                   .ToTable("StudentGrades", schema: "student")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<AdmissionInformation>()
                   .ToTable("AdmissionInformations", schema: "student")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<CheatingStatus>()
                   .ToTable("CheatingStatuses", schema: "student")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<CurriculumInformation>()
                   .ToTable("CurriculumInformations", schema: "student")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<ExtendedStudent>()
                   .ToTable("ExtendedStudents", schema: "student")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<StudentAddress>()
                   .ToTable("StudentAddresses", schema: "student")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<StudentCourseByPass>()
                   .ToTable("StudentCourseByPasses", schema: "student")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<GraduationInformation>()
                   .ToTable("GraduationInformations", schema: "student")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<MaintenanceStatus>()
                   .ToTable("MaintenanceStatuses", schema: "student")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<ParentInformation>()
                   .ToTable("ParentInformations", schema: "student")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<StudentIncident>()
                   .ToTable("StudentIncidents", schema: "student")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<BlacklistedStudent>()
                   .ToTable("BlacklistedStudents", schema: "student")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<ResignStudent>()
                   .ToTable("ResignStudents", schema: "student")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<DismissStudent>()
                   .ToTable("DismissStudents", schema: "student")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<StudentTransferLog>()
                   .ToTable("StudentTransferLogs", schema: "student")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<StudentTransferLogDetail>()
                   .ToTable("StudentTransferLogDetails", schema: "student");

            builder.Entity<StudentLog>()
                   .ToTable("StudentLogs", schema: "student");

            builder.Entity<StudentProbation>()
                   .ToTable("StudentProbations", schema: "student")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<StudentStatusLog>()
                   .ToTable("StudentStatusLogs", schema: "student");

            builder.Entity<SpecializationGroupInformation>()
                   .ToTable("SpecializationGroupInformations", schema: "student")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<StudentState>()
                   .ToTable("StudentStates", schema: "student")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<StudentStateLog>()
                   .ToTable("StudentStateLogs", schema: "student")
                   .HasQueryFilter(x => x.IsActive);
            #endregion

            #region Withdrawal
            builder.Entity<Withdrawal>()
                   .ToTable("Withdrawals", schema: "withdrawal")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<WithdrawalException>()
                   .ToTable("WithdrawalExceptions", schema: "withdrawal")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<WithdrawalPeriod>()
                   .ToTable("WithdrawalPeriods", schema: "withdrawal")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<WithdrawalLog>()
                   .ToTable("WithdrawalLogs", schema: "withdrawal");
            #endregion

            #region Fee
            builder.Entity<FeeItem>()
                   .ToTable("FeeItems", schema: "fee")
                   .HasQueryFilter(x => x.IsActive)
                   .HasIndex(p => p.Code).IsUnique();

            builder.Entity<FeeGroup>()
                   .ToTable("FeeGroups", schema: "fee")
                   .HasQueryFilter(x => x.IsActive)
                   .HasIndex(p => p.Code).IsUnique();

            builder.Entity<PaymentMethod>()
                   .ToTable("PaymentMethods", schema: "fee")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<TermFee>()
                   .ToTable("TermFees", schema: "fee")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<TuitionFee>()
                   .ToTable("TuitionFees", schema: "fee")
                   .HasQueryFilter(x => x.IsActive)
                   .HasIndex(p => p.Code).IsUnique();

            builder.Entity<TuitionFeeFormula>()
                   .ToTable("TuitionFeeFormulas", schema: "fee")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<TuitionFeeRate>()
                   .ToTable("TuitionFeeRates", schema: "fee")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<TuitionFeeType>()
                   .ToTable("TuitionFeeTypes", schema: "fee")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<StudentFeeGroup>()
                   .ToTable("StudentFeeGroups", schema: "fee")
                   .HasQueryFilter(x => x.IsActive)
                   .HasIndex(p => p.Code).IsUnique();

            builder.Entity<Invoice>()
                   .ToTable("Invoices", schema: "fee")
                   .HasQueryFilter(x => x.IsActive)
                   .HasIndex(p => p.Number).IsUnique();

            builder.Entity<InvoiceItem>()
                   .ToTable("InvoiceItems", schema: "fee");

            builder.Entity<InvoiceDeductTransaction>()
                   .ToTable("InvoiceDeductTransactions", schema: "fee");

            builder.Entity<InvoicePrintLog>()
                   .ToTable("InvoicePrintLogs", schema: "fee");

            builder.Entity<BankPaymentResponse>()
                   .ToTable("BankPaymentResponses", schema: "fee");

            builder.Entity<Receipt>()
                   .ToTable("Receipts", schema: "fee")
                   .HasQueryFilter(x => x.IsActive)
                   .HasIndex(p => p.Number).IsUnique();

            builder.Entity<ReceiptItem>()
                   .ToTable("ReceiptItems", schema: "fee");

            builder.Entity<ReceiptPaymentMethod>()
                   .ToTable("ReceiptPaymentMethods", schema: "fee");

            builder.Entity<ReceiptPrintLog>()
                   .ToTable("ReceiptPrintLogs", schema: "fee");

            builder.Entity<Percentage>()
                   .ToTable("Percentages", schema: "master")
                   .HasQueryFilter(x => x.IsActive)
                   .HasIndex(p => p.Value).IsUnique();
            #endregion

            #region Student Finance
            builder.Entity<Installment>()
                   .ToTable("Installments", schema: "finance")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<InstallmentTransaction>()
                   .ToTable("InstallmentTransactions", schema: "finance");

            builder.Entity<OnCredit>()
                   .ToTable("OnCredits", schema: "finance")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<OnCreditTransaction>()
                   .ToTable("OnCreditTransactions", schema: "finance");
            #endregion

            #region Grade
            builder.Entity<Grade>()
                   .ToTable("Grades", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<Barcode>()
                   .ToTable("Barcodes", schema: "grade")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<Coordinator>()
                   .ToTable("Coordinators", schema: "grade")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<GradingLog>()
                   .ToTable("GradingLogs", schema: "grade");

            builder.Entity<GradingCurve>()
                   .ToTable("GradingCurves", schema: "grade")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<GradeTemplate>()
                   .ToTable("GradeTemplates", schema: "grade")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<StandardGradingGroup>()
                   .ToTable("StandardGradingGroups", schema: "grade")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<StandardGradingScore>()
                   .ToTable("StandardGradingScores", schema: "grade");

            builder.Entity<MarkAllocation>()
                   .ToTable("MarkAllocations", schema: "grade")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<StudentRawScore>()
                   .ToTable("StudentRawScores", schema: "grade")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<StudentRawScoreDetail>()
                   .ToTable("StudentRawScoreDetails", schema: "grade")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<GradeMember>()
                   .ToTable("GradeMembers", schema: "grade")
                   .HasQueryFilter(x => x.IsActive);
            #endregion

            #region Scholarship
            builder.Entity<BudgetDetail>()
                   .ToTable("BudgetDetails", schema: "scholarship");

            builder.Entity<FinancialTransaction>()
                   .ToTable("FinancialTransactions", schema: "scholarship")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<Voucher>()
                   .ToTable("Vouchers", schema: "scholarship")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<VoucherLog>()
                   .ToTable("VoucherLogs", schema: "scholarship");

            builder.Entity<Scholarship>()
                   .ToTable("Scholarships", schema: "scholarship")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<ScholarshipActiveLog>()
                   .ToTable("ScholarshipActiveLogs", schema: "scholarship");

            builder.Entity<ScholarshipBudget>()
                   .ToTable("ScholarshipBudgets", schema: "scholarship")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<ScholarshipFeeItem>()
                   .ToTable("ScholarshipFeeItems", schema: "scholarship")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<ScholarshipStudent>()
                   .ToTable("ScholarshipStudents", schema: "scholarship")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<ScholarshipType>()
                   .ToTable("ScholarshipTypes", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<Sponsor>()
                   .ToTable("Sponsors", schema: "master")
                   .HasQueryFilter(x => x.IsActive);
            #endregion

            #region Questionnaire
            builder.Entity<QuestionnaireCourseGroup>()
                   .ToTable("QuestionnaireCourseGroups", schema: "questionnaire")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<QuestionnaireCourseGroupDetail>()
                   .ToTable("QuestionnaireCourseGroupDetails", schema: "questionnaire")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<Questionnaire>()
                   .ToTable("Questionnaires", schema: "questionnaire")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<QuestionGroup>()
                   .ToTable("QuestionGroups", schema: "questionnaire")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<QuestionnairePeriod>()
                   .ToTable("QuestionnairePeriods", schema: "questionnaire")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<Question>()
                   .ToTable("Questions", schema: "questionnaire")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<Answer>()
                   .ToTable("Answers", schema: "questionnaire")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<Answer>()
                   .HasOne(x => x.Question)
                   .WithMany(x => x.Answers)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Response>()
                   .ToTable("Responses", schema: "questionnaire")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<QuestionnaireApproval>()
                   .ToTable("QuestionnaireApprovals", schema: "questionnaire")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<QuestionnaireApprovalLog>()
                   .ToTable("QuestionnaireApprovalLogs", schema: "questionnaire");

            builder.Entity<QuestionnaireMember>()
                   .ToTable("QuestionnaireMembers", schema: "questionnaire");
            #endregion

            #region Petition
            builder.Entity<Petition>()
                   .ToTable("Petitions", schema: "master")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<StudentCertificate>()
                   .ToTable("StudentCertificates", schema: "student")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<StudentPetition>()
                   .ToTable("StudentPetitions", schema: "student")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<ChangingCurriculumPetition>()
                   .ToTable("ChangingCurriculumPetitions", schema: "petition")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<PetitionLog>()
                   .ToTable("PetitionLogs", schema: "log");
            #endregion

            #region Log
            builder.Entity<PrintingLog>()
                   .ToTable("PrintingLogs", schema: "log");

            builder.Entity<CardLog>()
                   .ToTable("CardLogs", schema: "log");

            builder.Entity<ChangedNameLog>()
                   .ToTable("ChangedNameLogs", schema: "log");

            builder.Entity<DataSyncLog>()
                  .ToTable("DataSyncLogs", schema: "log");

            builder.Entity<ApiCallLog>()
                 .ToTable("ApiCallLogs", schema: "log");
            #endregion

            #region Pre-requisite
            builder.Entity<AbilityCondition>()
                   .ToTable("AbilityConditions", schema: "prerequisite");

            builder.Entity<AndCondition>()
                   .ToTable("AndConditions", schema: "prerequisite");

            builder.Entity<BatchCondition>()
                   .ToTable("BatchConditions", schema: "prerequisite");

            builder.Entity<Corequisite>()
                   .ToTable("Corequisites", schema: "prerequisite");

            builder.Entity<CourseGroupCondition>()
                   .ToTable("CourseGroupConditions", schema: "prerequisite");

            builder.Entity<CreditCondition>()
                   .ToTable("CreditConditions", schema: "prerequisite");

            builder.Entity<GPACondition>()
                   .ToTable("GPAConditions", schema: "prerequisite");

            builder.Entity<GradeCondition>()
                   .ToTable("GradeConditions", schema: "prerequisite");

            builder.Entity<OrCondition>()
                   .ToTable("OrConditions", schema: "prerequisite");

            builder.Entity<Prerequisite>()
                   .ToTable("Prerequisites", schema: "prerequisite");

            builder.Entity<PredefinedCourse>()
                   .ToTable("PredefinedCourses", schema: "prerequisite")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<TermCondition>()
                   .ToTable("TermConditions", schema: "prerequisite");

            builder.Entity<TotalCourseGroupCondition>()
                   .ToTable("TotalCourseGroupConditions", schema: "prerequisite");

            builder.Entity<StudentPredefinedCourse>()
                   .ToTable("StudentPredefinedCourses", schema: "prerequisite");
            #endregion

            #region Graduation
            builder.Entity<CourseGroupingDetail>()
                   .ToTable("CourseGroupingDetails", schema: "graduation");

            builder.Entity<CourseGroupingLog>()
                   .ToTable("CourseGroupingLogs", schema: "graduation")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<CoursePrediction>()
                   .ToTable("CoursePredictions", schema: "graduation")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<GraduatingRequest>()
                   .ToTable("GraduatingRequests", schema: "graduation")
                   .HasQueryFilter(x => x.IsActive);

            builder.Entity<GraduatingRequestLog>()
                   .ToTable("GraduatingRequestLogs", schema: "graduation");

            builder.Entity<CourseGroupModification>()
                   .ToTable("CourseGroupModifications", schema: "graduation");
            #endregion
            // Global Query Filters
            builder.Entity<USparkSection>()
                   .HasNoKey();
            builder.Entity<USparkOrder>()
                   .HasNoKey();
        }

        public override int SaveChanges()
        {
            AddUserIdAndTimestamps();
            return base.SaveChanges();
        }

        public int SaveChangesNoAutoUserIdAndTimestamps()
        {
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            AddUserIdAndTimestamps();
            return await base.SaveChangesAsync();
        }

        private void AddUserIdAndTimestamps()
        {
            var userId = _httpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            var changedEntities = ChangeTracker.Entries();
            foreach (var changedEntity in changedEntities)
            {
                if (changedEntity.Entity is Entity entity)
                {
                    switch (changedEntity.State)
                    {
                        case EntityState.Added:
                            entity.OnBeforeInsert(userId);
                            break;

                        case EntityState.Modified:
                            entity.OnBeforeUpdate(userId);
                            break;
                    }
                }
            }
        }
    }
}