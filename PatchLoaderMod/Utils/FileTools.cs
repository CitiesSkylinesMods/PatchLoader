using System;
using System.IO;

namespace PatchLoaderMod.Utils {
    public static class FileTools {
        
        /// <summary>
        /// Copies an input stream to an output stream. Assumes call site will handle errors.
        /// </summary>
        /// 
        /// <param name="input">Input stream</param>
        /// <param name="output">Output stream</param>
        ///
        /// <returns>Returns <c>true</c> if successful, otherwise <c>false</c>.</returns>
        public static bool CopyStream(Stream input, Stream output) {
            Log._Debug("[FileTools.CopyStream] Writing stream.");
            try {
                byte[] buffer = new byte[8192];
                int len;
                while ((len = input.Read(buffer, 0, buffer.Length)) > 0) {
                    output.Write(buffer, 0, len);
                }
                return true;
            }
            catch (Exception e) {
                Log.Error($"[FileTools.CopyStream] Error: {e.Message}");
                return false;
            }
        }
    }
}