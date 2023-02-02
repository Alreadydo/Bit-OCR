using Azure.Storage.Blobs;
using CognitiveLibrary.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CognitiveLibrary.Services
{
    public class BlobService : IBlobInterface
    {
        private readonly IConfiguration _configuration;

        public BlobService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public BlobClient CreateBlobClient(string fileName)
        {
            string containerName = "analyseresulttim";
            string blobName = $"{fileName}.json";
            BlobContainerClient blobContainerClient = new BlobContainerClient(_configuration["AzureBlob:ConnectionString"], containerName);
            BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);

            return blobClient;
        }

        public async Task DeleteBlob(string fileName)
        {
            var blobClient = CreateBlobClient(fileName);
            if (!blobClient.Exists())
            {
                throw new Exception("Blob does not exits");
            }
            await blobClient.DeleteIfExistsAsync();
        }

        public async Task UploadToBlobStorage(string fileName)
        {
            var blobClient = CreateBlobClient(fileName);
            string filePath = $"C:analyseresult/filterd/{fileName}.json";
            await blobClient.UploadAsync(filePath, overwrite: true);
        }
        
        public async Task UploadStreamToBlobStorage(Stream stream, string fileName)
        {
            var blobClient = CreateBlobClient(fileName);
            await blobClient.UploadAsync(stream, overwrite: true);

        }
    }
}
