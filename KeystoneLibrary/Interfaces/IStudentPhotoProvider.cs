namespace KeystoneLibrary.Interfaces
{
    public interface IStudentPhotoProvider
    {
        Task<string> UploadFile(IFormFile file, string studentCode);
        Task<string> UploadFile(string fileBase64, string studentCode);
        Task<string> GetStudentImg(string studentCode);
        bool DeleteFile(string studentCode);
        void DownloadFiles(List<string> studentCodes);
    }
}