using System;
using System.IO;
using System.Threading.Tasks;

namespace Changa.Services
{
    public interface IStorageService
    {
        Task<Uri> UploadImage(Stream image, string path);
    }
}
