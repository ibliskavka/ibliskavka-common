using System.IO;
using System.IO.Compression;
using System.Text;

namespace Ibliskavka.Common
{
    public class Streams
    {
        /// <summary>
        /// Generates a stream from a string.
        /// </summary>
        private Stream StreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream, Encoding.Unicode);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Creates a single file archive stream containing the input string
        /// Based on: http://stackoverflow.com/questions/17232414/creating-a-zip-archive-in-memory-using-system-io-compression
        /// </summary>
        public static Stream CompressedStreamFromString(string fileName, string s)
        {
            var memoryStream = new MemoryStream();

            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                var demoFile = archive.CreateEntry(fileName);

                using (var entryStream = demoFile.Open())
                using (var streamWriter = new StreamWriter(entryStream, Encoding.Unicode))
                {
                    streamWriter.Write(s);
                }
            }

            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}