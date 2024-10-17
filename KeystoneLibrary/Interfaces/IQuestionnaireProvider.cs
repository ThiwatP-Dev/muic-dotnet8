using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels;
using KeystoneLibrary.Models.DataModels.Questionnaire;

namespace KeystoneLibrary.Interfaces
{
    public interface IQuestionnaireProvider
    {
        bool IsDuplicatedTerm(long id, long termId);
        Questionnaire GetQuestionnaireById(long id);
        Term GetQuestionnaireTermByAcademicLevelId(long academicLevelId);
        QuestionnaireByInstructorAndSectionViewModel GetQuestionnaireByInstructorAndSectionReport(List<QuestionnaireByInstructorAndSectionStudent> students);
        void BuildScore(long termId);

        void SyncQuestionnairePeriodWithUSpark(QuestionnairePeriod model);
    }
}