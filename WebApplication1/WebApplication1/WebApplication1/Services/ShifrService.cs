using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace WebApplication1.Services
{
    public class ShifrService
    {
        public static string HashPassword(string inputPassword)
        {
            var md5 = MD5.Create();
            byte[] result = MD5.HashData(Encoding.UTF8.GetBytes(inputPassword));
            return Convert.ToBase64String(result);
        }
 
        public static bool DeHashPassword(string serverPassword, string inputPassword)
        {
            string result=HashPassword(inputPassword);
            return serverPassword== result;
        }
    }
}
