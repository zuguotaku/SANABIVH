using SevenZip;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.IO;

namespace AowVN.MVVM.Helper;
internal static class ArchiveReaderHelper
{
    private static byte percentVariable = 0;
    public static async Task ExtractToDirectoryAsync(this SevenZipExtractor archive, string path, CancellationToken token, IProgress<FileExtractedArgs>? progress = null)
    {
        long i = 0;
        
        archive.FileExtractionFinished += (_, s) =>
        {
            var info = s.FileInfo;
            Debug.WriteLine($"Địa chỉ file: {Path.Combine(path, info.FileName)}");
            i++;
            progress?.Report(new(info.FileName, i, archive.FilesCount, s.PercentDone));
            percentVariable = s.PercentDone;
        };

        await archive.ExtractArchiveAsync(path).ConfigureAwait(false);
    }
    public static byte GetPercentVariable()
    {
        return percentVariable;
    }
}


internal record FileExtractedArgs(string Path, long File, long Count, byte Percent)
{
    public override string ToString()
    {
        return $"Đang cài đặt ({File}/{Count}) File";
    }
}

