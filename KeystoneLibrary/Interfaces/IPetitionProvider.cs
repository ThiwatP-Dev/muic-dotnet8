using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Logs;
using KeystoneLibrary.Models.DataModels.Petitions;
using KeystoneLibrary.Models.Enums;
using KeystoneLibrary.Models.ViewModel;

namespace KeystoneLibrary.Interfaces
{
    public interface IPetitionProvider
    {
        void CreateChangingCurriculumPetition(CreateUsparkChangingCurriculumPetitionViewModel request);

        PageResultViewModel<UsparkChangingCurriculumPetitionViewModel> SearchChangingCurriculumPetition(Criteria criteria, int page = 1, int pageSize = 25);

        UsparkChangingCurriculumPetitionViewModel GetChangingCurriculumPetitionById(long petitionId);

        void UpdatePetitionStatus(long petitionId, PetitionStatusEnum status, string remark, string userId);

        ChangingCurriculumPetition GetChangingCurriculumPetition(long id);

        ChangedNameLog GetChangedNameLog(long id);
    }
}