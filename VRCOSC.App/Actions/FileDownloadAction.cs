// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Actions;

public class FileDownloadAction : ProgressAction
{
    private readonly Storage targetStorage;
    private readonly Uri url;
    private readonly string assetName;

    public override string Title => $"Downloading {assetName}";
    public override bool UseProgressBar => true;

    public FileDownloadAction(Uri url, Storage targetStorage, string assetName)
    {
        this.url = url;
        this.targetStorage = targetStorage;
        this.assetName = assetName;
    }

    protected override async Task Perform()
    {
        var fileDownload = new FileDownload();
        fileDownload.ProgressChanged += p => OnProgressChanged?.Invoke(p);
        await fileDownload.DownloadFileAsync(url, targetStorage.GetFullPath(assetName, true));
    }
}