using KeystoneLibrary.Models;
using KeystoneLibrary.Models.DataModels.Profile;

namespace KeystoneLibrary.Interfaces
{
    public interface ICardProvider
    {
        Task<StudentIdCardDetail> GetStudentSubstitudedCard(StudentIdCardViewModel viewModel);
        DateTime GetCardExpiration(Guid studentId, long academicLevelId, long? facultyId, long? departmentId, DateTime? cardCreatedDate);
        Task<StudentIdCardViewModel> GetStudentIdCardForm(List<Student> students);
    }
}