using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Profile;
using KeystoneLibrary.Models.Api;
using KeystoneLibrary.Models.USpark;
using KeystoneLibrary.Models.Report;
using KeystoneLibrary.Models.ViewModel;

namespace KeystoneLibrary.Interfaces
{
    public interface IRegistrationProvider
    {
        // Term
        void UpdateStudentCreditLoadByRegistrationTerm(long academicLevelId);
        // 
        void ReturnSeat(Section section, int seat);
        void AddSeat(Section section, int seat);
        List<RegistrationCourse> GetActiveRegistrationCourses(Guid studentId, long termId);
        List<RegistrationCourse> GetRegistrationCourses(Guid studentId, long termId);
        RegistrationCourse GetRegistrationCourseById(long id);
        List<RegistrationCourse> GetRegistrationCoursesByStudentCode(string code);
        List<RegistrationCourse> GetRegistrationCoursesByCourseIdAndTermId(long courseId, long termId);
        List<Slot> GetRegistrationSlots(Student student, long registrationTermId);
        List<Course> GetOpenedCourses(long termId);
        Term GetRegistrationTerm(long academiclevelId);
        List<Section> GetSectionByCourseId(long termId, long courseId);
        List<Section> GetSectionByCourseIds(long termId, List<long> courseIds);
        List<string> GetSectionNumbers();
        List<string> GetSectionNumbersByCourses(long termId, List<long> courseIds);
        ExaminationPeriod GetExamDate(long courseId, long termId);
        Course GetCourse(long courseId);
        List<Course> GetCoursesByOpenSection(long termId, string sectionStatus);
        List<Course> GetCoursesByCloseSection(long termId, string sectionStatus);
        List<Course> GetCourseByIds(List<long> courseIds);
        string GetCreditMessage(RegistrationViewModel model);
        Section GetSection(long sectionId);
        List<Section> GetSections(List<long> sectionIds);
        List<Section> GetChildSection(long parentId);
        List<Section> GetSectionsByInstructorId(long instructorId, long termId, long courseId);
        bool IsExistSection(long termId, long courseId, string sectionNumber);
        bool SectionExists(Section sectionToBeUpdated, long termId, long courseId, string sectionNumber);
        List<SectionDetail> IsExistSectionDetail(List<SectionDetail> models, long termId, long sectionId);
        List<ScheduleViewModel> GetAvailableSections(string courseCode, string courseName, string sectionNumber, long termId);
        string Serialize(List<long> ids);
        List<long> Deserialize(string jsonString);
        List<Plan> GetPlans(long termId);
        List<PlanSchedule> GetPlanSchedules(long planId);
        Task<ApiResponse<string>> SubmitRegistration(string studentCode, string courseSectionRequest);
        List<RegistrationCourse> ModifyRegistrationCourse(Guid studentId, long termId, string round, List<AddingViewModel> addingResults, out List<RegistrationCourse> newCourses, out List<RegistrationCourse> deleteUnpaidCourses, out List<RegistrationCourse> deletePaidCourses, string channel);
        List<RegistrationCourse> GetRegistrationCourses(Guid studentId, long termId, List<RegistrationMatchCourse> courses);
        bool ModifyRegistrationCourse(List<SelectablePlannedSchedule> addingPlannedSchedules, long termId);
        List<Section> GetSectionsBySectionIds(List<long> sectionIds);
        int GetRegisteringCredit(long registrationTermId, Guid studentId);
        int GetAccumulativeCredit(Guid studentId);
        int GetAccumulativeRegistrationCredit(Guid studentId);
        decimal GetTotalRegistrationStudentByTermId(long termId);
        string GetSectionNumberByRegistrationId(long registrationId);
        RegistrationCourse GetRegistrationCourse(long id);
        List<RegistrationCourse> GetRegistrationCoursesByIds(List<long> ids);
        RegistrationCourse GetStudentRegistrationCourseBySection(Guid studentId, long sectionId);
        List<StudentTransferCourseViewModel> GetRegistrationCourses(Guid studentId);
        List<StudentCourseEquivalentViewModel> GetRegistrationEquivalentCourses(List<StudentTransferCourseViewModel> courseList, long curriculumVersionId);
        bool ChangeCurriculum(StudentTransferViewModel model, Student student, Term term);
        List<long> GetSectionIdsByCoursesRange(long termId, string courseCode, int courseCodeFrom, int courseCodeTo, int sectionFrom, int sectionTo);
        List<Term> GetStudentRegistrationTerms(string studentCode);
        bool IsRegistrationPeriod(DateTime datetime, long termId);

        #region For Delete All Course 
        void DeleteAllNotPaidCourse(Guid studentId, long termId);
        #endregion

        //USpark API
        void CallUSparkAPIAddSection(long sectionId);
        void CallUSparkAPICloseSection(long sectionId);
        void CallUSparkAPIOpenSection(long sectionId);
        int CallUSparkAPIGetCurrentSeat(long sectionId);
        void CallUSparkAPIUpdateSeat(long sectionId);
        void CancelRegistration(Guid studentId, long termId);
        Task<USparkCalculateTuitionRequestViewModel> CallUSparkAPIGetPreregistrations(string studentCode, int academicYear, int term);
        void CallUSparkAPIUpdatePreregistrations(Guid studentId, string userFullName, long termId, IEnumerable<long> sectionIds);
        void CallUSparkAPICheckoutOrder(long receiptId);
        void CallUSparkServiceAPIPaymentConfirm(long receiptId);
        void CallUSparkServiceAPIWaiveInvoice(long invoiceId);
        USparkOrder GenerateUSparkOrderFromInvoice(string studentCode, long termId, Invoice invoice, Invoice dropInvoice);

        Task<bool> UpdateCreditUspark(string studentCode, int maximumCredit, int minimumCredit);
        void AddRegistrationCourses(List<RegistrationCourse> registrationCourses);
        Task<bool> UpdateStudentCourseByPass(BodyUpdateStudentCourseByPass model);
        Task<bool> UpdateLockedStudentUspark(string studentCode, bool lockedRegistration, bool lockedPayment, bool lockedSignIn);

        string GetStudentState(Guid studentId, long termId);
        bool UpdateStudentState(Guid studentId, string studentCode, long termId, string state, string updateFrom, out string errorMsg);
        Task GetRegistrationCourseFromUspark(Guid studentId, long termId, string userId);
        bool UpdateWriteList(UpdateWhiteListViewModel model, long sectionId);
        void UpdateStudentStateToAdd(Guid studentId, long termId);
        void SimulateModifyRegistrationCourse(Guid studentId, long termId, List<AddingViewModel> addingResults, out List<RegistrationCourse> newCourses, out List<RegistrationCourse> deletePaidCourses);
        USparkOrder SimulateGenerateUSparkOrderFromInvoice(string studentCode, long termId, Invoice invoice, Invoice dropInvoice);
        List<StudentApiViewModel> GetRegistrationCourseByStudentCodes(List<StudentListViewModel> StudentCodes, long sectionId);
        List<CloseSectionStudentList> GetCloseSectionStudentListsByStudents(List<StudentListViewModel> studentList, long sectionId);
        List<AttendanceStudent> GetAttendanceSheetStudentListsByStudents(List<StudentListViewModel> studentList, long sectionId);
        List<SignatureSheetStudentDetail> GetSignatureSheetStudentListsByStudents(List<StudentListViewModel> studentList, long sectionId);
        List<StudentListViewModel> CallUSparkAPIGetStudentsBySectionId(long sectionId);

        void UpsertRegistrationCourses(IEnumerable<USparkPreregistrationViewModel> courses, string studentCode, long KSTermId, string userId);
        void UpsertRegistrationLog(IEnumerable<USparkRegistrationLogViewModel> logs, string studentCode, long KSTermId);

        void TransferCourseToUspark(USparkTransferCourse model);
    }
}