// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using VRCOSC.Packages;

namespace VRCOSC.Actions.Packages;

public class PackageUninstallAction : ProgressAction
{
    private readonly Storage storage;
    private readonly PackageSource packageSource;

    private float localProgress;

    public override string Title => $"Uninstalling {packageSource.GetDisplayName()}";

    public PackageUninstallAction(Storage storage, PackageSource packageSource)
    {
        this.storage = storage;
        this.packageSource = packageSource;
    }

    protected override Task Perform()
    {
        localProgress = 0f;
        storage.DeleteDirectory(packageSource.PackageID);
        localProgress = 1f;
        return Task.CompletedTask;
    }

    public override float GetProgress() => localProgress;
}
