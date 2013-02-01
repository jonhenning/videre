using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using CodeEndeavors.Extensions;

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
                var zip = new ZipFile(fs);
                foreach (ZipEntry entry in zip)
                {
                    //if (entry.IsFile)
                    ret.Add(entry.Name);
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
                var zip = new ZipFile(fs);
                var entryId = zip.FindEntry(entryFileName, true);
                if (entryId > -1)
                {
                    var stream = zip.GetInputStream(entryId);
                    using (TextReader tr = new StreamReader(stream))
                        return tr.ReadToEnd();
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

    }


}
