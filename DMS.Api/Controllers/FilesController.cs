using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DMS.Core.Communication.Files;
using DMS.Core.Communication.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS.Api.Controllers
{
    [Route("api/files")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IFilesRepository _filesRepository; 

        public FilesController(IFilesRepository filesRepository)
        {
            _filesRepository = filesRepository;
        }

        [HttpPost]
        [Route("{bucketName}/add")]
        public async Task<ActionResult<AddFileResponse>> AddFile(string bucketName, string key, IFormFile formFile)
        {
            if (formFile == null)
            {
                return BadRequest("The request doesn't contain any files to be uploaded");
            }

            var response = await _filesRepository.UploadFile(bucketName, key, formFile);

            if (response == null)
            {
                return BadRequest();
            }

            return Ok(response);
        }

        [HttpPost]
        [Route("{bucketName}/add-multiple")]
        public async Task<ActionResult<AddFileResponse>> AddMultipleFiles(string bucketName, IList<IFormFile> formFiles)
        {
            if (formFiles == null)
            {
                return BadRequest("The request doesn't contain any files to be uploaded");
            }

            var response = await _filesRepository.UploadFiles(bucketName, formFiles);

            if (response == null)
            {
                return BadRequest();
            }

            return Ok(response);
        }

        [HttpGet]
        [Route("{bucketName}/list")]
        public async Task<ActionResult<IEnumerable<ListFilesResponse>>> ListFiles(string bucketName)
        {
            var response = await _filesRepository.ListFiles(bucketName);

            return Ok(response);
        }

        [HttpGet]
        [Route("{bucketName}/download/{key}")]
        public async Task<ActionResult> DownloadFile(string bucketName, string key)
        {
            byte[] stream = await _filesRepository.GetFile(bucketName, key);
            return File(stream, "application/vnd.openxmlformats", key);
        }

        [HttpGet]
        [Route("{bucketName}/url/{key}")]
        public async Task<ActionResult<AddFileResponse>> DownloadUrl(string bucketName, string key, int daysValid)
        {
            var response = await _filesRepository.GetUrl(bucketName, key, daysValid);
            return Ok(response);
        }

        [HttpDelete]
        [Route("{bucketName}/delete/{fileName}")]
        public async Task<ActionResult<DeleteFileResponse>> DeleteFile(string bucketName, string fileName)
        {
            //var response = await _filesRepository.DeleteFile(bucketName, fileName);
            //return Ok(response);
            throw new AccessViolationException("Access denied");
        }

        [HttpPost]
        [Route("{bucketName}/addjsonobject")]
        public async Task<IActionResult> AddJsonObject(string bucketName, AddJsonObjectRequest request)
        {
            await _filesRepository.AddJsonObject(bucketName, request);

            return Ok();
        }

        [HttpGet]
        [Route("{bucketName}/getjsonobject")]
        public async Task<ActionResult<GetJsonObjectResponse>> GetJsonObject(string bucketName, string fileName)
        {
            var response = await _filesRepository.GetJsonObject(bucketName, fileName);

            return Ok(response);
        }

    }
}