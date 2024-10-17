using KeystoneLibrary.Models;

namespace KeystoneLibrary.Interfaces
{
    public interface IFileProvider
    {
        string UploadFile(UploadSubDirectory folderName, IFormFile file, string fileNameWithoutExtension);
        string UploadFile(UploadSubDirectory folderName, IFormFile formFile, string fileNameWithoutExtension, out string targetPath);
        string UploadFile(UploadSubDirectory folderName, String base64File, string extension, string fileNameWithoutExtension);
        string UploadFile(UploadSubDirectory folderName, string base64File, string extension, string fileNameWithoutExtension, out string targetPath);        
        bool DeleteFile(UploadSubDirectory folderName, string filepath);
        Task<byte[]> DownloadFiles(List<string> files);
    }
}