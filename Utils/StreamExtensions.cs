using System.IO;

namespace Utils
{
    public static class StreamExtensions
    {
        private const int _defaultCopyBufferSize = 81920; //see https://referencesource.microsoft.com/#mscorlib/system/io/stream.cs,2a0f078c2e0c0aa8

        /// <summary>
        /// Copies an input stream to an output stream.
        /// </summary>
        /// <param name="input">Input stream.</param>
        /// <param name="output">Output stream.</param>
        public static void CopyStream(this Stream input, Stream output)
        {
            byte[] buffer = new byte[_defaultCopyBufferSize];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }
    }
}
