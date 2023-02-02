using Azure.Storage.Blobs;
using System.Threading.Tasks;

namespace CognitiveLibrary.Interfaces
{
    public interface IBlobInterface
    {
        public BlobClient CreateBlobClient(string fileName);
        public Task UploadToBlobStorage(string fileName);
        public Task DeleteBlob(string fileName);
    }
}
