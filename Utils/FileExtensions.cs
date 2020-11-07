using System;
using System.IO;
using System.Security.Cryptography;

namespace Utils {
    public static class FileExtensions {
        
        /// <summary>
        /// Calculates MD5 hash for <paramref name="filePath"/>. Will throw error if file does not exist.
        /// </summary>
        /// <param name="filePath">Full path of dll file (including file name).</param>
        /// <param name="fileName">Name of file shown in log message.</param>
        /// <returns>Returns the MD5 hash of the file.</returns>
        internal static string CalculateFileMd5Hash(string filePath) {
            using (MD5 md5 = MD5.Create()) {
                using (FileStream stream = File.OpenRead(filePath)) {
                    byte[] hash = md5.ComputeHash(stream);
                    string result = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    return result;
                }
            }
        }
    }
}