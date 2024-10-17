using AutoMapper;
using KeystoneLibrary.Data;
using KeystoneLibrary.Interfaces;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace KeystoneLibrary.Providers
{
    public class StudentPhotoProvider : BaseProvider, IStudentPhotoProvider
    {
        private readonly string _minIOEndPoint = "storage-api.muic.io";
        private readonly string _minIOAccessKey = "nmc9Z65x6FAqmcQg";
        private readonly string _minIOSecretKey = "kamtDS8vsmx9pt1MC20FJ5L43FUKOWHa";
        private readonly string _bucketName = "student-image";
        
        // Initialize the client with access credentials.
        private IMinioClient _minio;

        public StudentPhotoProvider(ApplicationDbContext db,
                           IMapper mapper,
                           IConfiguration config) : base(config, db, mapper)
        {
            _minIOEndPoint = _config["minIOUrl"] ?? _minIOEndPoint;
            _minIOAccessKey = _config["minIOAccessKey"] ?? _minIOAccessKey;
            _minIOSecretKey = _config["minIOSecretKey"] ?? _minIOSecretKey;
            _bucketName = _config["minIOBucketName"] ?? _bucketName;

            _minio = new MinioClient().WithEndpoint(_minIOEndPoint)
                                      .WithCredentials(_minIOAccessKey, _minIOSecretKey)
                                      .WithSSL(true)
                                      .Build();
        }

        public bool DeleteFile(string studentCode)
        {
            throw new NotImplementedException();
        }

        public void DownloadFiles(List<string> studentCodes)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetStudentImg(string studentCode)
        {
            //var getListBucketsTask = await _minio.ListBucketsAsync().ConfigureAwait(false);

            //// Iterate over the list of buckets.
            //foreach (var bucket in getListBucketsTask.Buckets)
            //{
            //    Console.WriteLine(bucket.Name + " " + bucket.CreationDateDateTime);
            //}

            
            PresignedGetObjectArgs args = new PresignedGetObjectArgs()
                                                  .WithBucket(_bucketName)
                                                  .WithObject(studentCode + ".jpg")
                                                  .WithExpiry(60 * 20 * 1); //sec * min * hour
            string url = await _minio.PresignedGetObjectAsync(args);
            return url;
        }

        public async Task<string> UploadFile(IFormFile file, string studentCode)
        {
            var extention = Path.GetExtension(file.FileName);
            if (extention != ".jpg")
            {
                throw new ArgumentException("Invalid File Type");
            }

            try
            {
                // Make a bucket on the server, if not already present.
                var beArgs = new BucketExistsArgs()
                                    .WithBucket(_bucketName);
                bool found = await _minio.BucketExistsAsync(beArgs);
                if (!found)
                {
                    var mbArgs = new MakeBucketArgs()
                        .WithBucket(_bucketName);
                    await _minio.MakeBucketAsync(mbArgs);
                }

                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);

                    // Upload a file to bucket.
                    var putObjectArgs = new PutObjectArgs()
                                                .WithBucket(_bucketName)
                                                .WithObject(studentCode + ".jpg")
                                                .WithStreamData(ms)
                                                .WithObjectSize(ms.Length)
                                                .WithContentType("image/jpg");
                    await _minio.PutObjectAsync(putObjectArgs);
                    Console.WriteLine("Successfully uploaded " + studentCode);
                }

                return await GetStudentImg(studentCode);
            }
            catch (MinioException e)
            {
                Console.WriteLine("File Upload Error: {0}", e.Message);
            }
            return "";
        }

        public async Task<string> UploadFile(string fileBase64, string studentCode)
        {
            if (string.IsNullOrEmpty(fileBase64))
            {
                throw new ArgumentException("Invalid File");
            }

            try
            {
                // Make a bucket on the server, if not already present.
                var beArgs = new BucketExistsArgs()
                                    .WithBucket(_bucketName);
                bool found = await _minio.BucketExistsAsync(beArgs).ConfigureAwait(false);
                if (!found)
                {
                    var mbArgs = new MakeBucketArgs()
                        .WithBucket(_bucketName);
                    await _minio.MakeBucketAsync(mbArgs).ConfigureAwait(false);
                }

                using (var ms = new MemoryStream(Convert.FromBase64String(fileBase64)))
                {
                    ms.Seek(0, SeekOrigin.Begin);

                    // Upload a file to bucket.
                    var putObjectArgs = new PutObjectArgs()
                                                .WithBucket(_bucketName)
                                                .WithObject(studentCode + ".jpg")
                                                .WithStreamData(ms)
                                                .WithObjectSize(ms.Length)
                                                .WithContentType("image/jpg");
                    await _minio.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
                    Console.WriteLine("Successfully uploaded " + studentCode);
                }

                return await GetStudentImg(studentCode);
            }
            catch (MinioException e)
            {
                Console.WriteLine("File Upload Error: {0}", e.Message);
            }
            return "";
        }
    }
}
