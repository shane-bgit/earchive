using DMS.Core.Communication.Files;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DMS.Core.Communication.Interfaces
{
    public interface IFilesRepository
    {
        Task<AddFileResponse> UploadFile(string bucketName, string key, IFormFile file);
        Task<AddFileResponse> UploadFiles(string bucketName, IList<IFormFile> files);
        Task<IEnumerable<ListFilesResponse>> ListFiles(string bucketName);
        Task DownloadFile(string bucketName, string fileName);
        Task<byte[]> GetFile(string bucketName, string fileName);
        Task<AddFileResponse> GetUrl(string bucketName, string key, int daysValid = 1);
        Task<DeleteFileResponse> DeleteFile(string bucketName, string fileName);
        Task AddJsonObject(string bucketName, AddJsonObjectRequest request);
        Task<GetJsonObjectResponse> GetJsonObject(string bucketName, string fileName);
    }
}
