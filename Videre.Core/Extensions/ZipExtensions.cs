using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using System.Collections.ObjectModel;
using System.IO;
using CodeEndeavors.Extensions;
using ICSharpCode.SharpZipLib.Core;

namespace Videre.Core.Extensions
{
    public static class ZipExtensions
    {
        public static void ExtractZip(this string fileName, string targetDirectory, string fileFilter = null, string directoryFilter = null, bool overwrite = true)
        {
            var zip = new FastZip();
            zip.ExtractZip(fileName, targetDirectory, overwrite ? FastZip.Overwrite.Always : FastZip.Overwrite.Never, null, fileFilter, directoryFilter, true);
        }

        public static List<string> GetZipFileList(this string fileName, Func<string, bool> where = null)
        {
            var ret = new List<string>();
            using (var fs = File.OpenRead(fileName))
            {
                using (var zip = new ZipFile(fs))
                {
                    foreach (ZipEntry entry in zip)
                    {
                        //if (entry.IsFile)
                        ret.Add(entry.Name);
                    }
                }
            }
            if (where != null)
                ret = ret.Where(where).ToList();
            return ret;
        }

        public static string GetZipEntryContents(this string zipFileName, string entryFileName)
        {
            using (var fs = File.OpenRead(zipFileName))
            {
                return GetZipEntryContents(fs, entryFileName);
            }
        }

        public static string GetZipEntryContents(this Stream fs, string entryFileName)
        {
            using (var zip = new ZipFile(fs))
            {
                zip.IsStreamOwner = false;
                var entryId = zip.FindEntry(entryFileName, true);
                if (entryId > -1)
                {
                    using (var stream = zip.GetInputStream(entryId))
                    {
                        using (TextReader tr = new StreamReader(stream))
                            return tr.ReadToEnd();
                    }
                }
                else
                    return null;
                //throw new Exception("Entry not found: " + entryFileName);
            }
        }

        public static void ExtractEntry(this string zipFileName, string zipEntryName, string extractFolder)
        {
            var zip = new FastZip();
            zip.ExtractZip(zipFileName, extractFolder, zipEntryName);
        }

        public static byte[] ZipToByteArray(this Dictionary<string, string> entries)
        {
            using (var outputMemStream = new MemoryStream())
            {
                using (var zipStream = new ZipOutputStream(outputMemStream))
                {
                    foreach (var zipEntryName in entries.Keys)
                    {
                        using (var memStreamIn = new MemoryStream(Encoding.ASCII.GetBytes(entries[zipEntryName])))
                        {
                            zipStream.SetLevel(3); //0-9, 9 being the highest level of compression

                            var newEntry = new ZipEntry(zipEntryName);
                            newEntry.DateTime = DateTime.Now;

                            zipStream.PutNextEntry(newEntry);

                            StreamUtils.Copy(memStreamIn, zipStream, new byte[4096]);
                            zipStream.CloseEntry();

                        }
                    }
                    zipStream.IsStreamOwner = false;    // False stops the Close also Closing the underlying stream.
                    zipStream.Close();          // Must finish the ZipOutputStream before using outputMemStream.
                }
                outputMemStream.Position = 0;
                return outputMemStream.ToArray(); //.GetBuffer();
            }

        }

    }


}
