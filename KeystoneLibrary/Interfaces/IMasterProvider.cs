using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Advising;
using KeystoneLibrary.Models.DataModels.Fee;
using KeystoneLibrary.Models.DataModels.MasterTables;
using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Interfaces
{
    public interface IMasterProvider
    {
        ResidentType FindResidentType(long id);
        StudentFeeType GetStudentFeeType(long id);
        Probation FindProbation(long id);
        ExtendedYear FindExtendedYear(long id);
        InstructorType FindInstructorType(long id);
        bool IsExistExtendedYear(ExtendedYear model);
        ExtendedStudent FindExtendedStudent(long id);
        SpecializationGroup FindSpecializationGroup(long id);
        Petition FindPetition(long id);
        List<ExemptedAdmissionExamination> GetExemptedAdmissionExaminations();
        Nationality GetNationality(long id);
        TransferUniversity FindTrasferUniversity(long id);
        Course GetExternalCourse(long id);
        CourseExclusion FindCourseExclusion(long id);
        List<Language> GetLanguages(List<long> ids);
        ExaminationType FindExaminationType(long id);
        AdvisingLog GetAdvisingLog(long id);
        AdvisingStatus GetAdvisingStatus(long id);
        ReEnterReason GetReEnterReason(long id);
        AdmissionPlace GetAdmissionPlace(long id);
        Invoice GetInvoice(long id);
        FeeItem GetFeeItem(long id);
        PaymentMethod GetPaymentMethod(long id);
        DistributionMethod GetDistributionMethod(long id);
        StandardGradingGroup GetStandardGradingGroup(long id);
        CustomCourseGroup GetCustomCourseGroup(long id);
        QuestionnaireCourseGroup GetQuestionnaireCourseGroup(long id);
    }
}