using Microsoft.AspNetCore.Cryptography.KeyDerivation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;

using AspTwitter.AppData;
using AspTwitter.Requests;


namespace AspTwitter
{
    static class Util
    {
        public static bool ExceedsLength(string val, MaxLength length)
        {
            return val.Length > (int)length;
        }

        public static string Truncate(string val, MaxLength length)
        {
            if (val.Length > (int)length)
            {
                return val.Substring(0, (int)length);
            }

            return val;
        }

        public static string Hash(string password)
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            string key = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 32));

            return $"{10000}.{Convert.ToBase64String(salt)}.{key}";
        }

        public static bool CompareHash(string password, string hash)
        {
            string[] hashParts = hash.Split('.');

            int iterations = Convert.ToInt32(hashParts[0]);
            byte[] salt = Convert.FromBase64String(hashParts[1]);
            byte[] key = Convert.FromBase64String(hashParts[2]);

            byte[] keyToCheck = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: iterations,
                numBytesRequested: 32);

            return key.SequenceEqual(keyToCheck);
        }

        public static EditUserRequest ProcessEditRequest(EditUserRequest request)
        {
            if (request.Name is null || request.Username is null)
            {
                return null;
            }

            request.Name = request.Name.Trim();
            request.Username = request.Username.Replace(" ", string.Empty);

            if (request.Name == string.Empty || request.Username == string.Empty)
            {
                return null;
            }

            if (request.About != null)
            {
                request.About = request.About.Trim();
            }

            request.Name = Truncate(request.Name, MaxLength.Name);
            request.Username = Truncate(request.Username, MaxLength.Username);
            request.About = Truncate(request.About, MaxLength.About);

            return request;
        }
    }
}
