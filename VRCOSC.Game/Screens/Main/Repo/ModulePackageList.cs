// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using osu.Framework.Allocation;
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
        repoTab.Filter.BindValueChanged(_ => populate(), true);
    }

    public void Refresh()
    {
        populate();
    }

    private void populate()
    {
        Clear();

        AddRange(appManager.PackageManager.Sources
                           .OrderByDescending(packageSource => packageSource.IsInstalled())
                           .ThenBy(packageSource => packageSource.PackageType)
                           .ThenBy(packageSource => packageSource.GetDisplayName())
                           .Select(packageSource => new ModulePackageInstance(packageSource))
                           .Where(packageInstance => packageInstance.Satisfies(repoTab.Filter.Value)));
    }
}
