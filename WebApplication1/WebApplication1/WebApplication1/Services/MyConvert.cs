using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace WebApplication1.Services
{
    public class MyConvert
    {
        public  static  byte[] ConvertFileToByteArray(IFormFile file)
        {
            using var fileStream = file.OpenReadStream();
            byte[] bytes = new byte[file.Length];
            fileStream.Read(bytes, 0, (int)file.Length);
            return bytes;
        }
    }
}
