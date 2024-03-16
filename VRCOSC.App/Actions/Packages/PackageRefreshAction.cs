// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.ObjectModel;
using System.Threading.Tasks;
using VRCOSC.App.Packages;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Actions.Packages;

public class PackagesRefreshAction : CompositeProgressAction
{
    public PackagesRefreshAction(ObservableCollection<PackageSource> sources, bool forceRemoteGrab, bool allowPreRelease)
    {
        sources.ForEach(source => AddAction(new PackageSourceRefreshAction(source, forceRemoteGrab, allowPreRelease)));
    }
}

public class PackageSourceRefreshAction : ProgressAction
{
    private readonly PackageSource source;
    private readonly bool forceRemoteGrab;
    private readonly bool allowPreRelease;

    public override string Title => $"Refreshing {source.RepoName}";

    public PackageSourceRefreshAction(PackageSource source, bool forceRemoteGrab, bool allowPreRelease)
    {
        this.source = source;
        this.forceRemoteGrab = forceRemoteGrab;
        this.allowPreRelease = allowPreRelease;
    }

    protected override Task Perform()
    {
        return source.Refresh(forceRemoteGrab, allowPreRelease);
    }

    public override float GetProgress() => 0f;
}
