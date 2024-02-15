// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.Utils;

using System;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;

public class FileDownload
{
    public event Action<float>? ProgressChanged;

    public async Task DownloadFileAsync(string url, string filePath)
    {
        using var client = new HttpClient();
        using var response = await client.GetAsync(new Uri(url), HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        await downloadFileFromStreamAsync(stream, filePath, response.Content.Headers.ContentLength);
    }

    private async Task downloadFileFromStreamAsync(Stream stream, string filePath, long? totalSize)
    {
        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

        var buffer = new byte[8192];
        var totalRead = 0L;
        var isMoreToRead = true;

        do
        {
            var read = await stream.ReadAsync(buffer).ConfigureAwait(false);

            if (read == 0)
            {
                isMoreToRead = false;
            }
            else
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, read)).ConfigureAwait(false);
                totalRead += read;
                var progress = totalSize.HasValue ? (float)totalRead / totalSize.Value * 100 : 0;
                ProgressChanged?.Invoke(progress);
            }
        } while (isMoreToRead);
    }
}
