// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Platform;
using VRCOSC.Game.Config;
using VRCOSC.Game.Graphics.Screen;

namespace VRCOSC.Game.Graphics.About;

public partial class AboutHeader : BaseHeader
{
    [Resolved]
    private GameHost host { get; set; } = null!;

    [Resolved]
    private VRCOSCConfigManager configManager { get; set; } = null!;

    private Bindable<string> versionBindable = null!;

    protected override string Title => $"{host.Name} {versionBindable.Value}";

    protected override string SubTitle => "Copyright VolcanicArts 2023. See license file in repository root for more information";

    [BackgroundDependencyLoader]
    private void load()
    {
        versionBindable = configManager.GetBindable<string>(VRCOSCSetting.Version);
        versionBindable.BindValueChanged(_ => GenerateText(), true);
    }
}
