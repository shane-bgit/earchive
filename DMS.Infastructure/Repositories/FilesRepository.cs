using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using DMS.Core.Communication.Files;
using DMS.Core.Communication.Interfaces;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DMS.Infastructure.Repositories
{
    public class FilesRepository : IFilesRepository
    {
        private readonly IAmazonS3 _s3Client;

        public FilesRepository(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
        }

        public async Task<AddFileResponse> UploadFile(string bucketName, string key, IFormFile file)
        {
            if (string.IsNullOrEmpty(key))
            {
                key = file.FileName;
            }
            else if (!key.EndsWith(Path.GetExtension(file.FileName)))
            {
                key = string.Join("", key, Path.GetExtension(file.FileName));
            }

            var response = new List<string>();

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = file.OpenReadStream(),
                Key = key,
                BucketName = bucketName,
                CannedACL = S3CannedACL.Private
            };

            using (var fileTransferUtility = new TransferUtility(_s3Client))
            {
                await fileTransferUtility.UploadAsync(uploadRequest);
            }

            var expiryUriRequest = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = key,
                Expires = DateTime.Now.AddDays(1)
            };

            var url = _s3Client.GetPreSignedURL(expiryUriRequest);

            response.Add(url);

            return new AddFileResponse
            {
                PreSignedUrl = response
            };
        }

        public async Task<AddFileResponse> UploadFiles(string bucketName, IList<IFormFile> formFiles)
        {
            var response = new List<string>();

            foreach (var file in formFiles)
            {
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = file.OpenReadStream(),
                    Key = file.FileName,
                    BucketName = bucketName,
                    CannedACL = S3CannedACL.Private
                };

                using (var fileTransferUtility = new TransferUtility(_s3Client))
                {
                    await fileTransferUtility.UploadAsync(uploadRequest);
                }

                var expiryUriRequest = new GetPreSignedUrlRequest
                {
                    BucketName = bucketName,
                    Key = file.FileName,
                    Expires = DateTime.Now.AddDays(1)
                };

                var url = _s3Client.GetPreSignedURL(expiryUriRequest);

                response.Add(url);
            }

            return new AddFileResponse
            {
                PreSignedUrl = response
            };
        }

        public async Task<IEnumerable<ListFilesResponse>> ListFiles(string bucketName)
        {
            var responses = await _s3Client.ListObjectsAsync(bucketName);
            return responses.S3Objects.Select(b => new ListFilesResponse
            {
                BucketName = b.BucketName,
                Key = b.Key,
                Owner = b.Owner.DisplayName,
                Size = b.Size
            });
        }

        public async Task DownloadFile(string bucketName, string fileName)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");
            Directory.CreateDirectory(path);
            var pathAndFileName = Path.Combine(path, fileName);
            var downloadRequest = new TransferUtilityDownloadRequest
            {
                BucketName = bucketName,
                Key = fileName,
                FilePath = pathAndFileName,
                
            };

            using (var transferUtility = new TransferUtility(_s3Client))
            {
                await transferUtility.DownloadAsync(downloadRequest);
            }
        }

        public async Task<AddFileResponse> GetUrl(string bucketName, string key, int daysValid = 1)
        {
            var uriRequest = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = key,
                Expires = DateTime.Now.AddDays(daysValid)
            };

            var url = _s3Client.GetPreSignedURL(uriRequest);

            return new AddFileResponse() { PreSignedUrl = new List<string>() { url } };

        }

        public async Task<byte[]> GetFile(string bucketName, string fileName)
        {
            byte[] arr = { };
            await DownloadFile(bucketName, fileName);
            var pathAndFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp", fileName);

            using (FileStream fileDownloaded = new FileStream(pathAndFileName, FileMode.Open, FileAccess.Read))
            {
                arr = ToArray(fileDownloaded);                
            }

            try
            {
                File.Delete(pathAndFileName);
            }
            catch (Exception ex)
            {
                //exception
            }
            
            return arr;
        }

        public byte[] ToArray(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
        public async Task<DeleteFileResponse> DeleteFile(string bucketName, string fileName)
        {
            var multiObjectDeleteRequest = new DeleteObjectsRequest
            {
                BucketName = bucketName
            };

            multiObjectDeleteRequest.AddKey(fileName);

            var response = await _s3Client.DeleteObjectsAsync(multiObjectDeleteRequest);

            return new DeleteFileResponse
            {
                NumberOfDeletedObjects = response.DeletedObjects.Count
            };
        }

        public async Task AddJsonObject(string bucketName, AddJsonObjectRequest request)
        {
            var createdOnUtc = DateTime.UtcNow;
            var s3Key = $"{createdOnUtc:yyyy}/{createdOnUtc:MM}/{createdOnUtc:dd}/{request.Id}";
            var putObjectRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = s3Key,
                ContentBody = JsonConvert.SerializeObject(request)
            };

            await _s3Client.PutObjectAsync(putObjectRequest);
        }

        public async Task<GetJsonObjectResponse> GetJsonObject(string bucketName, string fileName)
        {
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = fileName
            };

            var response = await _s3Client.GetObjectAsync(request);

            using (var reader = new StreamReader(response.ResponseStream))
            {
                var contents = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<GetJsonObjectResponse>(contents);
            }
        }
    }
}
