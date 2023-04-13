using System.IO;
using System.Threading.Tasks;

namespace HB.FullStack.Common.Files
{

    public interface ILocalFileManager
    {
        string GetFullPath(Directory2 directory, string fileName);

        string GetNewTempFullPath(string fileExtension);

        Task<string?> SaveFileAsync(byte[] data, Directory2 directory, string fileName);

        Task<string?> SaveFileAsync(byte[] data, string fullPath);

        Task<string> SaveFileAsync(Stream stream, Directory2 directory, string fileName);

        Task<string> SaveFileAsync(Stream stream, string fullPath);

        Task UnZipAssetsAsync(string? assetFileName);
    }
}