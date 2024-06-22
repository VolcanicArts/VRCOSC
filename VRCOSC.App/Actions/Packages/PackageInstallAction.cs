// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using VRCOSC.App.Packages;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Actions.Packages;

public class PackageInstallAction : CompositeProgressAction
{
    public PackageInstallAction(Storage storage, PackageSource packageSource, PackageRelease packageRelease, bool shouldUninstall)
    {
        if (shouldUninstall) AddAction(new PackageUninstallAction(storage, packageSource));
        AddAction(new PackageSourceRefreshAction(packageSource, true));
        AddAction(new PackageDownloadAction(storage, packageSource, packageRelease));
    }

    private class PackageDownloadAction : CompositeProgressAction
    {
        private readonly PackageSource packageSource;

        public override string Title => $"Downloading all {packageSource.DisplayName} files";

        public PackageDownloadAction(Storage storage, PackageSource packageSource, PackageRelease packageRelease)
        {
            this.packageSource = packageSource;

            var targetDirectory = storage.GetStorageForDirectory(packageSource.PackageID!);
            var assetNames = packageRelease.DllFiles;

            foreach (var assetName in assetNames)
            {
                AddAction(new PackageAssetDownloadAction(targetDirectory, packageSource, packageRelease, assetName));
            }
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
                await fileDownload.DownloadFileAsync($"{packageSource.URL}/releases/download/{packageRelease.Version}/{assetName}", targetDirectory.GetFullPath(assetName, true));

                localProgress = 1f;
            }

            public override float GetProgress() => localProgress;
        }
    }
}
