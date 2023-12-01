// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics.UI.List;

namespace VRCOSC.Game.Screens.Main.Repo;

public partial class ModulePackageList : HeightLimitedScrollableList<ModulePackageInstance>
{
    [Resolved]
    private AppManager appManager { get; set; } = null!;

    [Resolved]
    private RepoTab repoTab { get; set; } = null!;

    protected override Drawable CreateHeader() => new ModulePackageListHeader();

    [BackgroundDependencyLoader]
    private void load()
    {
        populate();

        repoTab.Filter.BindValueChanged(e => applyFilter(e.NewValue), true);
    }

    private void applyFilter(PackageListingFilter filter)
    {
        this.ForEach(modulePackageInstance => modulePackageInstance.Satisfies(filter));
    }

    public void Refresh()
    {
        populate();
    }

    private void populate()
    {
        Clear();

        appManager.PackageManager.Sources
                  .OrderByDescending(packageSource => packageSource.IsInstalled())
                  .ThenBy(packageSource => packageSource.PackageType)
                  .ThenBy(packageSource => packageSource.GetDisplayName())
                  .ForEach(packageSource => Add(new ModulePackageInstance(packageSource)));

        applyFilter(repoTab.Filter.Value);
    }
}
