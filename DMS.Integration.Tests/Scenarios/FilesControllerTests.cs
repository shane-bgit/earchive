using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using DMS.Api;
using DMS.Core.Communication.Files;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DMS.Integration.Tests.Scenarios
{
    [Collection("api")]
    public class FilesControllerTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;
        public FilesControllerTests(WebApplicationFactory<Startup> factory)
        {
            _client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAWSService<IAmazonS3>(new AWSOptions
                    {
                        DefaultClientConfig =
                        {
                            ServiceURL ="http://localhost:9003"
                        },
                        Credentials = new BasicAWSCredentials("FAKE", "FAKE")
                    });
                });
            }).CreateClient();

            Task.Run(CreateBucket).Wait();
        }

        private async Task CreateBucket()
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            await _client.PostAsJsonAsync("api/bucket/create/testS3Bucket", "testS3Bucket");
        }

        [Fact]
        public async Task When_AddFiles_endpoint_is_hit_we_are_returned_ok_status()
        {
            var response = await UploadFileToS3Bucket();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        public async Task<HttpResponseMessage> UploadFileToS3Bucket()
        {
            const string path = @"F:\DocumentArchive\S3\p1.jpg";

            var file = File.Create(path);
            HttpContent fileStreamContent = new StreamContent(file);

            var formData = new MultipartFormDataContent
            {
                { fileStreamContent, "formFiles", "p1.jpg" }
            };

            var response = await _client.PostAsync("api/files/testS3Bucket/add", formData);

            fileStreamContent.Dispose();
            formData.Dispose();

            return response;
        }

        [Fact]
        public async Task When_ListFiles_endpoint_is_hit_our_result_is_not_null()
        {
            await UploadFileToS3Bucket();

            var response = await _client.GetAsync("api/files/testS3Bucket/list");

            ListFilesResponse[] result;

            using (var content = response.Content.ReadAsStringAsync())
            {
                result = JsonConvert.DeserializeObject<ListFilesResponse[]>(await content);
            }

            Assert.NotNull(result);
        }

        [Fact]
        public async Task When_DownloadFiles_endpoint_is_hit_we_are_returned_ok_status()
        {
            const string filename = @"p1.jpg";

            await UploadFileToS3Bucket();

            var response = await _client.GetAsync($"api/files/testS3Bucket/download/{filename}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task When_DeleteFile_endpoint_is_hit_we_are_returned_ok_status()
        {
            const string filename = @"p1.jpg";

            await UploadFileToS3Bucket();

            var response = await _client.DeleteAsync($"api/files/testS3Bucket/delete/{filename}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }


        [Fact]
        public async Task When_AddJsonObject_endpoint_is_hit_we_are_returned_ok_status()
        {
            var jsonObjectRequest = new AddJsonObjectRequest
            {
                Id = Guid.NewGuid(),
                Data = "Test",
                TimeSent = DateTime.UtcNow
            };

            var response = await _client.PostAsJsonAsync("api/files/testS3Bucket/addjsonobject/", jsonObjectRequest);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}