using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using KeystoneLibrary.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO.Compression;

namespace KeystoneLibrary.Providers
{
    public class FileProvider : BaseProvider, IFileProvider
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        public FileProvider(ApplicationDbContext db,
                            IMapper mapper,
                            IHostingEnvironment hostingEnvironment,
                            IHttpContextAccessor httpContextAccessor) : base(db, mapper)
        {
            _hostingEnvironment = hostingEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public string UploadFile(UploadSubDirectory folderName, string base64File, string extension, string fileNameWithoutExtension)
        {
            return UploadFile(folderName, base64File, extension, fileNameWithoutExtension, out _);
        }

        public string UploadFile(UploadSubDirectory folderName, string base64File, string extension, string fileNameWithoutExtension, out string targetPath)
        {
            try
            {
                fileNameWithoutExtension = fileNameWithoutExtension.Replace(" ", "_");
                var destinationFileName = fileNameWithoutExtension + "." + extension.ToLower();
                var folder = "/uploaded/" + folderName.GetStringValue() + "/";
                string folderPath = _hostingEnvironment.WebRootPath + folder;
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                //var urlPath = _httpContextAccessor.HttpContext.Request.Scheme.ToString() + "://" + _httpContextAccessor.HttpContext.Request.Host.ToString() + folder + destinationFileName;
                var urlPath = "https://" + _httpContextAccessor.HttpContext.Request.Host.ToString() + folder + destinationFileName;
                targetPath = Path.Combine(folderPath, destinationFileName);
                System.IO.File.WriteAllBytes(targetPath, Convert.FromBase64String(base64File));
                return urlPath;
            }
            catch
            {
                targetPath = null;
                return null;
            }
        }

        public string UploadFile(UploadSubDirectory folderName, IFormFile formFile, string fileNameWithoutExtension)
        {
            return UploadFile(folderName, formFile, fileNameWithoutExtension, out _);
        }

        public string UploadFile(UploadSubDirectory folderName, IFormFile formFile, string fileNameWithoutExtension, out string targetPath)
        {
            try
            {
                fileNameWithoutExtension = fileNameWithoutExtension.Replace(" ", "_");
                var destinationFileName = fileNameWithoutExtension + Path.GetExtension(formFile.FileName).ToLower();
                var folder = "/uploaded/" + folderName.GetStringValue() + "/";
                string folderPath = _hostingEnvironment.WebRootPath + folder;
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                //var urlPath = _httpContextAccessor.HttpContext.Request.Scheme.ToString() + "://" + _httpContextAccessor.HttpContext.Request.Host.ToString() + folder + destinationFileName;
                var urlPath = "https://" + _httpContextAccessor.HttpContext.Request.Host.ToString() + folder + destinationFileName;
                targetPath = Path.Combine(folderPath, destinationFileName);
                using (var stream = System.IO.File.Create(targetPath))
                {
                    formFile.CopyTo(stream);
                }
                return urlPath;
            }
            catch
            {
                targetPath = null;
                return null;
            }
        }

        public bool DeleteFile(UploadSubDirectory folderName, string filepath)
        {
            try
            {
                var folder = "/uploaded/" + folderName.GetStringValue() + "/";
                string folderPath = _hostingEnvironment.WebRootPath + folder;
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                if (!filepath.StartsWith(folderPath)) {
                    return false;
                }

                File.Delete(filepath);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<byte[]> DownloadFiles(List<string> files)
        {
            // MIGRATE RECHECK
            var httpClient = new HttpClient();
            using var memoryStream = new MemoryStream();

            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var path in files)
                {
                    var response = await httpClient.GetAsync(path);
                    if (!response.IsSuccessStatusCode)
                    {
                        continue;
                    }

                    var fileBytes = await response.Content.ReadAsByteArrayAsync();
                    string name = path.Split('/')[path.Split('/').Length - 1];
                    var zipEntry = archive.CreateEntry(name, CompressionLevel.Fastest);

                    using var zipStream = zipEntry.Open();
                    await zipStream.WriteAsync(fileBytes, 0, fileBytes.Length);
                }
            }

            memoryStream.Position = 0;
            return memoryStream.ToArray();

            //using (ZipFile zip = new ZipFile())
            //{
            //    var destination = FolderHelper.GetFolderPath(FolderHelper.KnownFolder.Downloads);
            //    foreach (var path in files)
            //    {
            //        using (WebClient client = new WebClient())
            //        {
            //            byte[] bytes = client.DownloadData(path);
            //            string name = path.Split('/')[path.Split('/').Length - 1];
            //            zip.AddEntry(name, bytes);
            //        }
            //    }

            //    string fileName = $"Profiles_{DateTime.UtcNow:ddMMyyyy_HHmmss}.zip";
            //    zip.Save(destination + @"\" + fileName);
            //}
        }
    }
}