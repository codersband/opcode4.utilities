using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Ionic.Zip;

namespace opcode4.utilities
{
    public static class ZipUtils
    {

        #region Zip
        public static MemoryStream ZipFilesToStream(List<string> fileList, bool reducePath)
        {
            using (var zip = new ZipFile())
            {
                var num = 0;
                foreach (var fn in fileList)
                {
                    var entry = reducePath ? zip.AddFile(fn, "") : zip.AddFile(fn);
                    num++;
                    entry.Comment = num.ToString();
                }

                var ms = new MemoryStream();
                zip.Save(ms);
                return ms;
            }
        }

        public static MemoryStream ZipFilesToStream(Dictionary<string, string> files, bool reducePath)
        {
            using (var zip = new ZipFile(Encoding.UTF8))
            {
                var num = 0;
                foreach (var fn in files)
                {
                    var entry = reducePath ? zip.AddFile(fn.Key, "") : zip.AddFile(fn.Key);
                    num++;
                    entry.Comment = num.ToString();
                    entry.FileName = string.Format("{0}_{1}",num,fn.Value);
                }
                var ms = new MemoryStream();
                zip.Save(ms);

                return ms;
            }
        }


        public static byte[] ZipFilesToBytes(Dictionary<string, string> files, bool reducePath)
        {
            using (var zip = new ZipFile(Encoding.UTF8))
            {
                var num = 0;
                foreach (var fn in files)
                {
                    var entry = reducePath ? zip.AddFile(fn.Key, "") : zip.AddFile(fn.Key);
                    num++;
                    entry.Comment = num.ToString();
                    entry.FileName = string.Format("{0}_{1}", num, fn.Value);
                }
                var ms = new MemoryStream();
                zip.Save(ms);

                ms.Position = 0;
                var bytes = new byte[ms.Length];
                ms.Read(bytes, 0, (int)ms.Length);

                return bytes;
            }
        }

        public static void ZipFiles(List<string> fileList, string targetFile)
        {
            using (var zip = new ZipFile())
            {
                var num = 0;
                foreach (string fn in fileList)
                {
                    var entry = zip.AddFile(fn);
                    num++;
                    entry.Comment = num.ToString();
                }

                zip.Save(targetFile);
            }
        }

        public static byte[] ZipStr(String str, Encoding encoding)
        {
            using (var output = new MemoryStream())
            {
                using (var gzip = new DeflateStream(output, CompressionMode.Compress))
                {
                    using (var writer = new StreamWriter(gzip, encoding))
                    {
                        
                        writer.Write(str);
                    }
                }

                return output.ToArray();
            }
        }

        #endregion Zip
     
        public static string UnZipStr(byte[] input, Encoding encoding)
        {
            using (var inputStream = new MemoryStream(input))
            {
                using (var gzip = new DeflateStream(inputStream, CompressionMode.Decompress))
                {
                    using (var reader = new StreamReader(gzip, encoding))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }

        #region unzip

        public static MemoryStream UnzipFileFromStream(Stream zipStream, int entryNum, out string fileName)
        {
            using (var zip = ZipFile.Read(zipStream))
            {
                if (zip.Entries.Count <= entryNum)
                    throw new ArgumentException("Invalid entry number");

                var cEntry = zip[entryNum]; 
                fileName = cEntry.FileName;
                var ms = new MemoryStream();
                cEntry.Extract(ms);
                return ms;
            }
        }

        #endregion unzip

        #region additional func
        public static string[] ReadContent(string filename)
        {
            using (var zip = new ZipFile(filename))
            {
                var res = new string[zip.Entries.Count];
                for (var i = 0; i < zip.Entries.Count; i++)
                {
                    res[i] = zip[i].FileName;
                }

                return res;
            }
        }
        #endregion additional func
    }
}
