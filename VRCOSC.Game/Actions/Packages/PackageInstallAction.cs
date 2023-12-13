// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.IO.Network;
using osu.Framework.Platform;
using VRCOSC.Packages;

namespace VRCOSC.Actions.Packages;

public class PackageInstallAction : CompositeProgressAction
{
    private readonly PackageSource packageSource;

    public override string Title => $"Installing {packageSource.GetDisplayName()}";

    public PackageInstallAction(Storage storage, PackageSource packageSource, bool shouldUninstall)
    {
        this.packageSource = packageSource;

        if (shouldUninstall) AddAction(new PackageUninstallAction(storage, packageSource));
        AddAction(new PackageDownloadAction(storage, packageSource));
    }

    private class PackageDownloadAction : CompositeProgressAction
    {
        private readonly PackageSource packageSource;

        public override string Title => $"Downloading all {packageSource.GetDisplayName()} files";

        public PackageDownloadAction(Storage storage, PackageSource packageSource)
        {
            this.packageSource = packageSource;

            var targetDirectory = storage.GetStorageForDirectory(packageSource.PackageID);
            var assetNames = packageSource.GetAssets().SkipWhile(assetName => assetName.Equals("vrcosc.json", StringComparison.InvariantCultureIgnoreCase));

            foreach (var assetName in assetNames)
            {
                AddAction(new PackageAssetDownloadAction(targetDirectory, packageSource, assetName));
            }
        }

        private class PackageAssetDownloadAction : ProgressAction
        {
            private readonly Storage targetDirectory;
            private readonly PackageSource packageSource;
            private readonly string assetName;

            private float localProgress;

            public override string Title => $"Downloading {assetName}";

            public PackageAssetDownloadAction(Storage targetDirectory, PackageSource packageSource, string assetName)
            {
                this.targetDirectory = targetDirectory;
                this.packageSource = packageSource;
                this.assetName = assetName;
            }

            protected override async Task Perform()
            {
                localProgress = 0f;

                var assetDownload = new FileWebRequest(targetDirectory.GetFullPath(assetName), $"{packageSource.DownloadURL}/{assetName}");

                assetDownload.DownloadProgress += (downloadedBytes, totalBytes) => localProgress = downloadedBytes / (float)totalBytes;

                await assetDownload.PerformAsync();
            }

            public override float GetProgress() => localProgress;
        }
    }
}
