using System.IO;
using System.IO.Compression;

namespace GRAT2_Client.Injectious
{
    class DecGzip
    {
        // https://github.com/rasta-mouse/TikiTorch/blob/master/TikiLoader/Generic.cs
        public static byte[] DecompressGzipped(byte[] gzip)
        {
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }

        }
    }
}
