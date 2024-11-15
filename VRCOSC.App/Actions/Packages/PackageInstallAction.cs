// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO.Compression;
using System.Threading.Tasks;
using VRCOSC.App.Packages;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Actions.Packages;

public class PackageInstallAction : CompositeProgressAction
{
    public PackageInstallAction(Storage storage, PackageSource packageSource, PackageRelease? packageRelease, bool shouldUninstall, bool refreshBeforeInstall = true)
    {
        if (shouldUninstall) AddAction(new PackageUninstallAction(storage, packageSource));
        if (refreshBeforeInstall) AddAction(new PackageSourceRefreshAction(packageSource, true));

        AddAction(new PackageDownloadAction(storage, packageSource, packageRelease));
    }

    private class PackageDownloadAction : CompositeProgressAction
    {
        private readonly Storage storage;
        private readonly PackageSource packageSource;
        private readonly PackageRelease? packageRelease;

        public override string Title => $"Downloading all {packageSource.DisplayName} files";

        public PackageDownloadAction(Storage storage, PackageSource packageSource, PackageRelease? packageRelease)
        {
            this.storage = storage;
            this.packageSource = packageSource;
            this.packageRelease = packageRelease;
        }

        protected override Task Perform()
        {
            Logger.Log($"Installing {packageSource.InternalReference}");

            var targetDirectory = storage.GetStorageForDirectory(packageSource.PackageID!);
            var release = packageRelease ?? packageSource.LatestRelease;

            foreach (var assetName in release.Assets)
            {
                AddAction(new PackageAssetDownloadAction(targetDirectory, packageSource, release, assetName));
            }

            return base.Perform();
        }

        private class PackageAssetDownloadAction : ProgressAction
        {
            private readonly Storage targetDirectory;
            private readonly PackageSource packageSource;
            private readonly PackageRelease packageRelease;
            private readonly string assetName;

            private float localProgress;

            public override string Title => $"Downloading {assetName}";

            public PackageAssetDownloadAction(Storage targetDirectory, PackageSource packageSource, PackageRelease packageRelease, string assetName)
            {
                this.targetDirectory = targetDirectory;
                this.packageSource = packageSource;
                this.packageRelease = packageRelease;
                this.assetName = assetName;
            }

            protected override async Task Perform()
            {
                localProgress = 0f;

                var fileDownload = new FileDownload();
                fileDownload.ProgressChanged += p => localProgress = p;
                await fileDownload.DownloadFileAsync(new Uri($"{packageSource.URL}/releases/download/{packageRelease.Version}/{assetName}"), targetDirectory.GetFullPath(assetName, true));

                if (assetName.EndsWith(".zip"))
                {
                    var zipPath = targetDirectory.GetFullPath(assetName);
                    var extractPath = targetDirectory.GetFullPath(string.Empty);

                    ZipFile.ExtractToDirectory(zipPath, extractPath);

                    targetDirectory.Delete(assetName);
                }

                localProgress = 1f;
            }

            public override float GetProgress() => localProgress;
        }
    }
}