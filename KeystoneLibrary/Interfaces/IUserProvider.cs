using KeystoneLibrary.Models.DataModels;

namespace KeystoneLibrary.Interfaces
{
    public interface IUserProvider
    {
        ApplicationUser GetUser(string id);

        UserTimeStamp FillUserTimeStampFullName(UserTimeStamp model);
        UserTimeStamp FillUserTimeStampFullNameWithOptionalStudent(UserTimeStamp model);

        List<UserTimeStamp> FillUserTimeStampFullName(List<UserTimeStamp> models);
        List<UserTimeStamp> FillUserTimeStampFullNameWithOptionalStudent(List<UserTimeStamp> models);
        List<UserTimeStamp> GetCreatedFullNameByIds(List<string> Ids);
        string GetUserFullNameById(string id);
    }
}